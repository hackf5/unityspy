namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClassField instance in a Mono process. This object describes a field in a
    /// managed class or struct. The .NET equivalent is <see cref="System.Reflection.FieldInfo"/>.
    /// See: _MonoImage in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay(
        "Field: {" + nameof(FieldDefinition.Offset) + "} - {" + nameof(FieldDefinition.Name) + "}")]
    public class FieldDefinition : MemoryObject, IFieldDefinition
    {
        public FieldDefinition([NotNull] TypeDefinition declaringType, uint address)
            : base((declaringType ?? throw new ArgumentNullException(nameof(declaringType))).Image, address)
        {
        }

        ITypeDefinition IFieldDefinition.DeclaringType => null;

        public string Name { get; }

        ITypeInfo IFieldDefinition.TypeInfo => null;

        public ITypeDefinition DeclaringType { get; }

        public int Offset { get; set; }

        public TValue GetValue<TValue>(uint address)
        {
            return default(TValue);
        }
    }
}