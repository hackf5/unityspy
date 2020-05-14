namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClass instance in a Mono process. This object describes the type of a class or
    /// struct. The .NET equivalent is <see cref="System.Type"/>.
    /// See: _MonoClass in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("Class: {" + nameof(TypeDefinition.Name) + "}")]
    public class TypeDefinition : MemoryObject, ITypeDefinition
    {
        private readonly uint bitFields;

        private readonly ConcurrentDictionary<(string @class, string name), FieldDefinition> fieldCache =
            new ConcurrentDictionary<(string @class, string name), FieldDefinition>();

        private readonly int fieldCount;

        private readonly Lazy<IReadOnlyList<FieldDefinition>> lazyFields;

        private readonly Lazy<string> lazyFullName;

        private readonly Lazy<TypeDefinition> lazyNestedIn;

        private readonly Lazy<TypeDefinition> lazyParent;

        private readonly Lazy<TypeDefinition> lazyGeneric;

        public TypeDefinition([NotNull] AssemblyImage image, uint address)
            : base(image, address)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this.bitFields = this.ReadUInt32(MonoLibraryOffsets.TypeDefinitionBitFields);
            this.fieldCount = this.ReadInt32(MonoLibraryOffsets.TypeDefinitionFieldCount);
            this.lazyParent = new Lazy<TypeDefinition>(() => this.GetClassDefinition(MonoLibraryOffsets.TypeDefinitionParent));
            this.lazyNestedIn = new Lazy<TypeDefinition>(() => this.GetClassDefinition(MonoLibraryOffsets.TypeDefinitionNestedIn));
            this.lazyFullName = new Lazy<string>(this.GetFullName);
            this.lazyFields = new Lazy<IReadOnlyList<FieldDefinition>>(this.GetFields);
            this.lazyGeneric = new Lazy<TypeDefinition>(this.GetGeneric);

            this.Name = this.ReadString(MonoLibraryOffsets.TypeDefinitionName);
            this.NamespaceName = this.ReadString(MonoLibraryOffsets.TypeDefinitionNamespace);
            this.Size = this.ReadInt32(MonoLibraryOffsets.TypeDefinitionSize);
            var vtablePtr = this.ReadPtr(MonoLibraryOffsets.TypeDefinitionRuntimeInfo);
            this.VTable = vtablePtr == Constants.NullPtr ? Constants.NullPtr : image.Process.ReadPtr(vtablePtr + MonoLibraryOffsets.TypeDefinitionRuntimeInfoDomainVtables);
            this.TypeInfo = new TypeInfo(image, this.Address + MonoLibraryOffsets.TypeDefinitionByValArg);
            this.VTableSize = vtablePtr == Constants.NullPtr ? Constants.NullPtr : this.ReadInt32(MonoLibraryOffsets.TypeDefinitionVTableSize);
            this.ClassKind = (MonoClassKind)(this.ReadByte(MonoLibraryOffsets.TypeDefinitionClassKind) & 0x7);
        }

        IReadOnlyList<IFieldDefinition> ITypeDefinition.Fields => this.Fields;

        public string FullName => this.lazyFullName.Value;

        public bool IsEnum => (this.bitFields & 0x10) == 0x10;

        public bool IsValueType => (this.bitFields & 0x8) == 0x8;

        public string Name { get; }

        public string NamespaceName { get; }

        ITypeInfo ITypeDefinition.TypeInfo => this.TypeInfo;

        public IReadOnlyList<FieldDefinition> Fields => this.lazyFields.Value;

        public TypeDefinition NestedIn => this.lazyNestedIn.Value;

        public TypeDefinition Parent => this.lazyParent.Value;

        public int Size { get; }

        public TypeInfo TypeInfo { get; }

        public uint VTable { get; }

        public int VTableSize { get; }

        public MonoClassKind ClassKind { get; }

        public dynamic this[string fieldName] => this.GetStaticValue<dynamic>(fieldName);

        IFieldDefinition ITypeDefinition.GetField(string fieldName, string typeFullName) =>
            this.GetField(fieldName, typeFullName);

        public TValue GetStaticValue<TValue>(string fieldName)
        {
            var field = this.GetField(fieldName, this.FullName)
                ?? throw new ArgumentException(
                    $"Field '{fieldName}' does not exist in class '{this.FullName}'.",
                    nameof(fieldName));

            if (!field.TypeInfo.IsStatic)
            {
                throw new InvalidOperationException($"Field '{fieldName}' is not static in class '{this.FullName}'.");
            }

            if (field.TypeInfo.IsConstant)
            {
                throw new InvalidOperationException($"Field '{fieldName}' is constant in class '{this.FullName}'.");
            }

            return field.GetValue<TValue>(this.Process.ReadPtr(this.VTable + MonoLibraryOffsets.VTable + (uint)(4 * this.VTableSize)));
        }

        public FieldDefinition GetField(string fieldName, string typeFullName = default) =>
            this.fieldCache.GetOrAdd(
                (typeFullName, fieldName),
                k => this.Fields
                    .FirstOrDefault(
                        f => (f.Name == k.name) && ((k.@class == default) || (k.@class == f.DeclaringType.FullName))));

        public void Init()
        {
            this.NestedIn?.Init();
            this.Parent?.Init();
        }

        private TypeDefinition GetClassDefinition(uint address) =>
            this.Image.GetTypeDefinition(this.ReadPtr(address));

        private IReadOnlyList<FieldDefinition> GetFields()
        {
            // This looks like it has changed for generic types.

            var firstField = this.ReadPtr(MonoLibraryOffsets.TypeDefinitionFields);
            if (firstField == Constants.NullPtr)
            {
                return this.Parent?.Fields ?? Array.Empty<FieldDefinition>();
            }

            var fields = new List<FieldDefinition>();
            for (var fieldIndex = 0u; fieldIndex < this.fieldCount; fieldIndex++)
            {
                var field = firstField + (fieldIndex * 0x10);
                if (this.Process.ReadPtr(field) == Constants.NullPtr)
                {
                    break;
                }

                fields.Add(new FieldDefinition(this, field));
            }

            fields.AddRange(this.Parent?.Fields ?? Array.Empty<FieldDefinition>());

            return new ReadOnlyCollection<FieldDefinition>(fields.OrderBy(f => f.Name).ToArray());
        }

        private string GetFullName()
        {
            var builder = new StringBuilder();

            var hierarchy = this.NestedHierarchy().Reverse().ToArray();
            if (!string.IsNullOrWhiteSpace(this.NamespaceName))
            {
                builder.Append($"{hierarchy[0].NamespaceName}.");
            }

            foreach (var definition in hierarchy)
            {
                builder.Append($"{definition.Name}+");
            }

            return builder.ToString().TrimEnd('+');
        }

        private IEnumerable<TypeDefinition> NestedHierarchy()
        {
            yield return this;

            var nested = this.NestedIn;
            while (nested != default)
            {
                yield return nested;

                nested = nested.NestedIn;
            }
        }

        private TypeDefinition GetGeneric()
        {
            if (this.ClassKind != MonoClassKind.GInst)
            {
                return null;
            }

            return this.Image.GetTypeDefinition(this.Image.Process.ReadPtr(this.ReadPtr(MonoLibraryOffsets.TypeDefinitionSizeOf)));
        }
    }
}