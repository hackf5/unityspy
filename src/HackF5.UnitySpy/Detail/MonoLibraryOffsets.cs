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

            VTable = 0x28
        };

        private static readonly List<MonoLibraryOffsets> supportedVersions = new List<MonoLibraryOffsets>()
        {
            Unity2018_4_10_x86_Offests
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

        public static MonoLibraryOffsets GetOffsets(string unityVersion, bool is64Bits)
        {
            MonoLibraryOffsets monoLibraryOffsets = supportedVersions.Find(
                   offsets => offsets.Is64Bits == is64Bits
                && unityVersion.StartsWith(offsets.UnityVersion)
            );

            if(monoLibraryOffsets == null)
            {
                string mode = is64Bits ? "in 64 bits mode" : "in 32 Bits mode";
                throw new NotSupportedException($"The unity version the process is running " +
                    $"({unityVersion} {mode}) is not supported");
            }

            return monoLibraryOffsets;
        }
    }
}