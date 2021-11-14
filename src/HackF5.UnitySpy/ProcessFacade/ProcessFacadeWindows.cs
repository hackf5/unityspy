namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeWindows : ProcessFacade
    {
        private const string PsApiDll = "psapi.dll";

        private readonly Process process;

        public ProcessFacadeWindows(Process process)
            : base()
        {
            this.process = process;
        }

        public Process Process => this.process;

        public override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("the buffer parameter cannot be null");
            }

            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                if (!ProcessFacadeWindows.ReadProcessMemory(
                    this.process.Handle,
                    processAddress,
                    bufferPointer,
                    size ?? buffer.Length,
                    out _))
                {
                    var error = Marshal.GetLastWin32Error();
                    if ((error == 299) && allowPartialRead)
                    {
                        return;
                    }

                    throw new Win32Exception(error);
                }
            }
            finally
            {
                bufferHandle.Free();
            }
        }

        public string GetMainModuleFileName(int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(this.process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }

        // https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        // TODO add check for matching platforms and implement the following code while keeping the existing one otherwise:
        // This can be done with this if the process is running in 64 bits mode (and UnitySpy too of course)
        // foreach(ProcessModule module in process.Process.Modules)
        // {
        //    if(module.ModuleName == moduleName)
        //    {
        //        return new ModuleInfo(module.ModuleName, module.BaseAddress, module.ModuleMemorySize);
        //    }
        // }
        public override ModuleInfo GetModule(string moduleName)
        {
            var modulePointers = this.GetModulePointers();

            // Collect modules from the process
            var modules = new List<ModuleInfo>();
            foreach (var modulePointer in modulePointers)
            {
                var moduleFilePath = new StringBuilder(1024);
                var errorCode = GetModuleFileNameEx(
                    this.process.Handle,
                    modulePointer,
                    moduleFilePath,
                    (uint)moduleFilePath.Capacity);

                if (errorCode == 0)
                {
                    throw new COMException("Failed to get module file name.", Marshal.GetLastWin32Error());
                }

                var currentModuleName = Path.GetFileName(moduleFilePath.ToString());
                GetModuleInformation(
                    this.process.Handle,
                    modulePointer,
                    out var moduleInformation,
                    (uint)(IntPtr.Size * modulePointers.Length));

                // Convert to a normalized module and add it to our list
                var module = new ModuleInfo(currentModuleName, moduleInformation.BaseOfDll, moduleInformation.SizeInBytes, moduleFilePath.ToString());
                modules.Add(module);
            }

            return modules.FirstOrDefault(module => module.ModuleName == moduleName);
        }

        [DllImport(ProcessFacadeWindows.PsApiDll, SetLastError = true)]
        private static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport(ProcessFacadeWindows.PsApiDll, SetLastError = true)]
        private static extern bool GetModuleInformation(
            IntPtr hProcess,
            IntPtr hModule,
            out ModuleInformation lpModInfo,
            uint cb);

        // https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        [DllImport(ProcessFacadeWindows.PsApiDll, SetLastError = true)]
        private static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In] [Out]
            IntPtr[] lphModule,
            int cb,
            [MarshalAs(UnmanagedType.U4)] out int lpCbNeeded,
            uint dwFilterFlag);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead);

        // https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        private IntPtr[] GetModulePointers()
        {
            var modulePointers = new IntPtr[2048 * this.SizeOfPtr];

            // Determine number of modules
            if (!EnumProcessModulesEx(
                this.process.Handle,
                modulePointers,
                modulePointers.Length,
                out var bytesNeeded,
                0x03))
            {
                throw new COMException(
                    "Failed to read modules from the external process.",
                    Marshal.GetLastWin32Error());
            }

            var result = new IntPtr[bytesNeeded / IntPtr.Size];
            Buffer.BlockCopy(modulePointers, 0, result, 0, bytesNeeded);
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModuleInformation
        {
            public readonly IntPtr BaseOfDll;

            public readonly uint SizeInBytes;

            private readonly IntPtr entryPoint;
        }
    }
}