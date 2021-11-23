namespace HackF5.UnitySpy
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Offsets;
    using HackF5.UnitySpy.ProcessFacade;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A factory that creates <see cref="IAssemblyImage"/> instances that provides access into a Unity application's
    /// managed memory.
    /// SEE: https://github.com/Unity-Technologies/mono.
    /// </summary>
    [PublicAPI]
    public static class AssemblyImageFactory
    {
        public static IAssemblyImage Create([NotNull] UnityProcessFacade process, string assemblyName = "Assembly-CSharp")
        {
            if (process == null)
            {
                throw new ArgumentNullException("process parameter cannot be null");
            }

            var monoModule = process.GetMonoModule();
            IntPtr rootDomainFunctionAddress;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                rootDomainFunctionAddress = GetRootDomainFunctionAddressMachOFormat(monoModule);
            }
            else
            {
                var moduleDump = process.ReadModule(monoModule);
                rootDomainFunctionAddress = AssemblyImageFactory.GetRootDomainFunctionAddressPEFormat(moduleDump, monoModule, process.Is64Bits);
            }

            return AssemblyImageFactory.GetAssemblyImage(process, assemblyName, rootDomainFunctionAddress);
        }

        private static AssemblyImage GetAssemblyImage(UnityProcessFacade process, string name, IntPtr rootDomainFunctionAddress)
        {
            IntPtr domain;
            if (process.Is64Bits)
            {
                int ripPlusOffsetOffset;
                int ripValueOffset;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Offsets taken by decompiling the 64 bits version of libmonobdwgc-2.0.dylib
                    //
                    // push rbp
                    // mov rbp,rsp
                    // mov rax, [rip + 0x4250ba]
                    // pop rbp
                    // ret
                    //
                    // These five lines in Hex translate to
                    // 55
                    // 4889E5
                    // 488B05 BA5042 00
                    // 5D
                    // C3
                    //
                    // So wee need to offset the first seven bytes to get to the relative offset we need to add to rip
                    // rootDomainFunctionAddress + 7
                    //
                    // rip has the current value of the rootDoaminAddress plus the 4 bytes of the first two instructions
                    // plus the 7 bytes of the rip + offset instruction (mov rax, [rip + 0x4250ba]).
                    // then we need to add this offsets to get the domain starting address
                    ripPlusOffsetOffset = 7;
                    ripValueOffset = 11;
                }
                else
                {
                    // Offsets taken by decompiling the 64 bits version of mono-2.0-bdwgc.dll
                    //
                    // mov rax, [rip + 0x46ad39]
                    // ret
                    //
                    // These two lines in Hex translate to
                    // 488B05 39AD46 00
                    // C3
                    //
                    // So wee need to offset the first three bytes to get to the relative offset we need to add to rip
                    // rootDomainFunctionAddress + 3
                    //
                    // rip has the current value of the rootDoaminAddress plus the 7 bytes of the first instruction (mov rax, [rip + 0x46ad39])
                    // then we need to add this offsets to get the domain starting address
                    ripPlusOffsetOffset = 3;
                    ripValueOffset = 7;
                }

                var offset = process.ReadInt32(rootDomainFunctionAddress + ripPlusOffsetOffset) + ripValueOffset;
                //// pointer to struct of type _MonoDomain
                domain = process.ReadPtr(rootDomainFunctionAddress + offset);
            }
            else
            {
                var domainAddress = process.ReadPtr(rootDomainFunctionAddress + 1);
                //// pointer to struct of type _MonoDomain
                domain = process.ReadPtr(domainAddress);
            }

            Console.WriteLine($"[DEBUG] Root Domain Address = {domain.ToString("X")}");

            //// pointer to array of structs of type _MonoAssembly
            var assemblyArrayAddress = process.ReadPtr(domain + process.MonoLibraryOffsets.ReferencedAssemblies);
            for (var assemblyAddress = assemblyArrayAddress;
                assemblyAddress != Constants.NullPtr;
                assemblyAddress = process.ReadPtr(assemblyAddress + process.SizeOfPtr))
            {
                var assembly = process.ReadPtr(assemblyAddress);
                var assemblyNameAddress = process.ReadPtr(assembly + (process.SizeOfPtr * 2));
                var assemblyName = process.ReadAsciiString(assemblyNameAddress);
                if (assemblyName == name)
                {
                    Console.WriteLine($"[DEBUG] Assembly name = {assemblyName}");
                    return new AssemblyImage(process, process.ReadPtr(assembly + process.MonoLibraryOffsets.AssemblyImage));
                }
            }

            throw new InvalidOperationException($"Unable to find assembly '{name}'");
        }

        private static IntPtr GetRootDomainFunctionAddressPEFormat(byte[] moduleDump, ModuleInfo monoModuleInfo, bool is64Bits)
        {
            // offsets taken from https://docs.microsoft.com/en-us/windows/desktop/Debug/pe-format
            // ReSharper disable once CommentTypo
            var startIndex = moduleDump.ToInt32(PEFormatOffsets.Signature); // lfanew

            var exportDirectoryIndex = startIndex + PEFormatOffsets.GetExportDirectoryIndex(is64Bits);
            var exportDirectory = moduleDump.ToInt32(exportDirectoryIndex);

            var numberOfFunctions = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.NumberOfFunctions);
            var functionAddressArrayIndex = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.FunctionAddressArrayIndex);
            var functionNameArrayIndex = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.FunctionNameArrayIndex);

            var rootDomainFunctionAddress = Constants.NullPtr;
            for (var functionIndex = 0;
                functionIndex < (numberOfFunctions * PEFormatOffsets.FunctionEntrySize);
                functionIndex += PEFormatOffsets.FunctionEntrySize)
            {
                var functionNameIndex = moduleDump.ToInt32(functionNameArrayIndex + functionIndex);
                var functionName = moduleDump.ToAsciiString(functionNameIndex);
                if (functionName == "mono_get_root_domain")
                {
                    rootDomainFunctionAddress = monoModuleInfo.BaseAddress
                        + moduleDump.ToInt32(functionAddressArrayIndex + functionIndex);

                    break;
                }
            }

            if (rootDomainFunctionAddress == Constants.NullPtr)
            {
                throw new InvalidOperationException("Failed to find mono_get_root_domain function.");
            }

            return rootDomainFunctionAddress;
        }

        private static IntPtr GetRootDomainFunctionAddressMachOFormat(ModuleInfo monoModuleInfo)
        {
            var rootDomainFunctionAddress = Constants.NullPtr;

            byte[] moduleFromPath = File.ReadAllBytes(monoModuleInfo.Path);

            int numberOfCommands = moduleFromPath.ToInt32(MachOFormatOffsets.NumberOfCommands);
            int offsetToNextCommand = MachOFormatOffsets.LoadCommands;
            for (int i = 0; i < numberOfCommands; i++)
            {
                // Check if load command is LC_SYMTAB
                if (moduleFromPath.ToInt32(offsetToNextCommand) == 2)
                {
                    int symbolTableOffset = moduleFromPath.ToInt32(offsetToNextCommand + MachOFormatOffsets.SymbolTableOffset);
                    int numberOfSymbols = moduleFromPath.ToInt32(offsetToNextCommand + MachOFormatOffsets.NumberOfSymbols);
                    int stringTableOffset = moduleFromPath.ToInt32(offsetToNextCommand + MachOFormatOffsets.StringTableOffset);

                    for (int j = 0; j < numberOfSymbols; j++)
                    {
                        int symbolNameOffset = moduleFromPath.ToInt32(symbolTableOffset + (j * MachOFormatOffsets.SizeOfNListItem));
                        var symbolName = moduleFromPath.ToAsciiString(stringTableOffset + symbolNameOffset);

                        if (symbolName == "_mono_get_root_domain")
                        {
                            rootDomainFunctionAddress = monoModuleInfo.BaseAddress
                                + moduleFromPath.ToInt32(symbolTableOffset + (j * MachOFormatOffsets.SizeOfNListItem)
                                                            + MachOFormatOffsets.NListValue);

                            Console.WriteLine($"[DEBUG] Found _mono_get_root_domain address = {rootDomainFunctionAddress.ToString("X")}");
                            break;
                        }
                    }

                    break;
                }
                else
                {
                    offsetToNextCommand += moduleFromPath.ToInt32(offsetToNextCommand + MachOFormatOffsets.CommandSize);
                }
            }

            if (rootDomainFunctionAddress == Constants.NullPtr)
            {
                throw new InvalidOperationException("Failed to find mono_get_root_domain function.");
            }

            return rootDomainFunctionAddress;
        }
    }
}