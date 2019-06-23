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
        private readonly Dictionary<string, TypeDefinition> classDefinitionByFullName =
            new Dictionary<string, TypeDefinition>();

        private readonly ConcurrentDictionary<uint, TypeDefinition> classDefinitionsByAddress;

        public AssemblyImage(ProcessFacade process, uint address)
            : base(null, address)
        {
            this.Process = process;

            this.classDefinitionsByAddress = this.CreateClassDefinitions();

            foreach (var definition in this.ClassDefinitions)
            {
                definition.Init();
            }

            foreach (var definition in this.ClassDefinitions)
            {
                if (definition.FullName.Contains("`"))
                {
                    // ignore generic classes as they have name clashes. in order to make them unique these it would be
                    // necessary to examine the information held in TypeInfo.Data. see
                    // ProcessFacade.ReadManagedGenericObject for moral support.
                    continue;
                }

                this.classDefinitionByFullName.Add(definition.FullName, definition);
            }
        }

        IEnumerable<ITypeDefinition> IAssemblyImage.ClassDefinitions => this.ClassDefinitions;

        public IEnumerable<TypeDefinition> ClassDefinitions =>
            this.classDefinitionsByAddress.ToArray().Select(k => k.Value);

        public override AssemblyImage Image => this;

        public override ProcessFacade Process { get; }

        ITypeDefinition IAssemblyImage.GetClassDefinition(string fullName) => this.GetClassDefinition(fullName);

        public TypeDefinition GetClassDefinition(string fullName) =>
            this.classDefinitionByFullName.TryGetValue(fullName, out var d) ? d : default;

        public TypeDefinition GetClassDefinition(uint address)
        {
            if (address == Constants.NullPtr)
            {
                return default;
            }

            return this.classDefinitionsByAddress.GetOrAdd(
                address,
                key => new TypeDefinition(this, key));
        }

        private ConcurrentDictionary<uint, TypeDefinition> CreateClassDefinitions()
        {
            var definitions = new ConcurrentDictionary<uint, TypeDefinition>();

            var classCache = 0x2a0u;
            var classCacheSize = this.ReadUInt32(classCache + 0xc);
            var classCacheTableArray = this.ReadPtr(classCache + 0x14);

            for (var tableItem = 0u;
                tableItem < (classCacheSize * Constants.SizeOfPtr);
                tableItem += Constants.SizeOfPtr)
            {
                for (var definition = this.Process.ReadPtr(classCacheTableArray + tableItem);
                    definition != Constants.NullPtr;
                    definition = this.Process.ReadPtr(definition + 0xa8))
                {
                    definitions.GetOrAdd(definition, new TypeDefinition(this, definition));
                }
            }

            return definitions;
        }
    }
}