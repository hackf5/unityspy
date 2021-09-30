// ReSharper disable IdentifierTypo
namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;

    public class MonoLibraryOffsets
    {

        public static readonly MonoLibraryOffsets Unity2018_4_10_x86_Offests = new MonoLibraryOffsets
        {
            UnityVersion = "2018.4.10",
            Is64Bits = false,

            AssemblyImage = 0x44,
            ReferencedAssemblies = 0x6c,
            ImageClassCache = 0x354,
            HashTableSize = 0xc,
            HashTableTable = 0x14,
            
            TypeDefinitionFieldSize = 0x10,
            TypeDefinitionBitFields = 0x14,
            TypeDefinitionClassKind = 0x1e,
            TypeDefinitionParent = 0x20,
            TypeDefinitionNestedIn = 0x24,
            TypeDefinitionName = 0x2c,
            TypeDefinitionNamespace = 0x30,
            TypeDefinitionVTableSize = 0x38,
            TypeDefinitionSize = 0x5c,
            TypeDefinitionFields = 0x60,
            TypeDefinitionByValArg = 0x74,
            TypeDefinitionRuntimeInfo = 0x84,

            TypeDefinitionFieldCount = 0xa4,
            TypeDefinitionNextClassCache = 0xa8,

            TypeDefinitionGenericContainer = 0x94,
            
            TypeDefinitionRuntimeInfoDomainVtables = 0x4,

            VTable = 0x28,

            UnicodeString = 0xc
        };

        public static readonly MonoLibraryOffsets Unity2019_4_5_x64_Offests = new MonoLibraryOffsets
        {
            UnityVersion = "2019.4.5",
            Is64Bits = true,

            AssemblyImage = 0x44 + 0x1c,
            ReferencedAssemblies = 0x6c + 0x5c,
            ImageClassCache = 0x354 + 0x16c,
            HashTableSize = 0xc + 0xc,
            HashTableTable = 0x14 + 0xc,

            TypeDefinitionFieldSize = 0x10 + 0x10,
            TypeDefinitionBitFields = 0x14 + 0xc,
            TypeDefinitionClassKind = 0x1e + 0xc,
            TypeDefinitionParent = 0x20 + 0x10,                         // 0x30
            TypeDefinitionNestedIn = 0x24 + 0x14,                       // 0x38
            TypeDefinitionName = 0x2c + 0x1c,                           // 0x48
            TypeDefinitionNamespace = 0x30 + 0x20,                      // 0x50
            TypeDefinitionVTableSize = 0x38 + 0x24,
            TypeDefinitionSize = 0x5c + 0x20 + 0x18 - 0x4,              // 0x90 Array Element Count
            TypeDefinitionFields = 0x60 + 0x20 + 0x18,                  // 0x98
            TypeDefinitionByValArg = 0x74 + 0x44,
            TypeDefinitionRuntimeInfo = 0x84 + 0x34 + 0x18,             // 0xB8

            TypeDefinitionFieldCount = 0xa4 + 0x34 + 0x10 + 0x18,
            TypeDefinitionNextClassCache = 0xa8 + 0x34 + 0x10 + 0x18 + 0x4,

            TypeDefinitionGenericContainer = 0x94 + 0x34 + 0x18 + 0x10,

            TypeDefinitionRuntimeInfoDomainVtables = 0x4 + 0x4,

            VTable = 0x28 + 0x18,

            UnicodeString = 0x14
        };

        private static readonly List<MonoLibraryOffsets> SupportedVersions = new List<MonoLibraryOffsets>()
        {
            Unity2018_4_10_x86_Offests,
            Unity2019_4_5_x64_Offests
        };

        public string UnityVersion { get; private set; }

        public bool Is64Bits { get; private set; }

        public int AssemblyImage { get; private set; }

        public int ReferencedAssemblies { get; private set; }

        public int ImageClassCache { get; private set; }

        public int HashTableSize { get; private set; }

        public int HashTableTable { get; private set; }


        // MonoClass Offsets

        public int TypeDefinitionFieldSize { get; private set; }

        public int TypeDefinitionBitFields { get; private set; }

        public int TypeDefinitionClassKind { get; private set; }

        public int TypeDefinitionParent { get; private set; }

        public int TypeDefinitionNestedIn { get; private set; }

        public int TypeDefinitionName { get; private set; }

        public int TypeDefinitionNamespace { get; private set; }

        public int TypeDefinitionVTableSize { get; private set; }

        public int TypeDefinitionSize { get; private set; }

        public int TypeDefinitionFields { get; private set; }

        public int TypeDefinitionByValArg { get; private set; }

        public int TypeDefinitionRuntimeInfo { get; private set; }


        // MonoClassDef Offsets

        public int TypeDefinitionFieldCount { get; private set; }

        public int TypeDefinitionNextClassCache { get; private set; }


        // MonoClassGtd Offsets

        public int TypeDefinitionGenericContainer { get; private set; }


        // MonoClassRuntimeInfo Offsets

        public int TypeDefinitionRuntimeInfoDomainVtables { get; private set; }


        // MonoVTable Offsets

        public int VTable { get; private set; }


        // Managed String Offsets

        public int UnicodeString { get; private set; }
        

        public static MonoLibraryOffsets GetOffsets(string unityVersion, bool is64Bits)
        {
            MonoLibraryOffsets monoLibraryOffsets = SupportedVersions.Find(
                   offsets => offsets.Is64Bits == is64Bits
                && unityVersion.StartsWith(offsets.UnityVersion)
            );

            // TODO add code to find the best candidate instead of throwing exception.
            if (monoLibraryOffsets == null)
            {
                string mode = is64Bits ? "in 64 bits mode" : "in 32 Bits mode";
                throw new NotSupportedException($"The unity version the process is running " +
                    $"({unityVersion} {mode}) is not supported");
            }

            return monoLibraryOffsets;
        }
    }
}