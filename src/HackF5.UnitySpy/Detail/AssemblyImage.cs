namespace HackF5.UnitySpy.Detail
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoImage instance in a Mono process. This object describes a managed assembly.
    /// The .NET equivalent is <see cref="System.Reflection.Assembly"/>.
    /// See: _MonoImage in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/metadata-internals.h.
    /// </summary>
    [PublicAPI]
    public class AssemblyImage : MemoryObject, IAssemblyImage
    {
        private readonly Dictionary<string, TypeDefinition> typeDefinitionsByFullName =
            new Dictionary<string, TypeDefinition>();

        private readonly ConcurrentDictionary<uint, TypeDefinition> typeDefinitionsByAddress;

        public AssemblyImage(ProcessFacade process, uint address)
            : base(null, address)
        {
            this.Process = process;

            this.typeDefinitionsByAddress = this.CreateTypeDefinitions();

            foreach (var definition in this.TypeDefinitions)
            {
                definition.Init();
            }

            foreach (var definition in this.TypeDefinitions)
            {
                if (definition.FullName.Contains("`"))
                {
                    // ignore generic classes as they have name clashes. in order to make them unique these it would be
                    // necessary to examine the information held in TypeInfo.Data. see
                    // ProcessFacade.ReadManagedGenericObject for moral support.
                    continue;
                }

                this.typeDefinitionsByFullName.Add(definition.FullName, definition);
            }
        }

        IEnumerable<ITypeDefinition> IAssemblyImage.TypeDefinitions => this.TypeDefinitions;

        public IEnumerable<TypeDefinition> TypeDefinitions =>
            this.typeDefinitionsByAddress.ToArray().Select(k => k.Value);

        public override AssemblyImage Image => this;

        public override ProcessFacade Process { get; }

        public dynamic this[string fullTypeName] => this.GetTypeDefinition(fullTypeName);

        ITypeDefinition IAssemblyImage.GetTypeDefinition(string fullTypeName) => this.GetTypeDefinition(fullTypeName);

        public TypeDefinition GetTypeDefinition(string fullTypeName) =>
            this.typeDefinitionsByFullName.TryGetValue(fullTypeName, out var d) ? d : default;

        public TypeDefinition GetTypeDefinition(uint address)
        {
            if (address == Constants.NullPtr)
            {
                return default;
            }

            return this.typeDefinitionsByAddress.GetOrAdd(
                address,
                key => new TypeDefinition(this, key));
        }

        private ConcurrentDictionary<uint, TypeDefinition> CreateTypeDefinitions()
        {
            var definitions = new ConcurrentDictionary<uint, TypeDefinition>();

            const uint classCache = MonoLibraryOffsets.ImageClassCache;
            var classCacheSize = this.ReadUInt32(classCache + MonoLibraryOffsets.HashTableSize);
            var classCacheTableArray = this.ReadPtr(classCache + MonoLibraryOffsets.HashTableTable);

            for (var tableItem = 0u;
                tableItem < (classCacheSize * Constants.SizeOfPtr);
                tableItem += Constants.SizeOfPtr)
            {
                for (var definition = this.Process.ReadPtr(classCacheTableArray + tableItem);
                    definition != Constants.NullPtr;
                    definition = this.Process.ReadPtr(definition + MonoLibraryOffsets.TypeDefinitionNextClassCache))
                {
                    definitions.GetOrAdd(definition, new TypeDefinition(this, definition));
                }
            }

            return definitions;
        }
    }
}