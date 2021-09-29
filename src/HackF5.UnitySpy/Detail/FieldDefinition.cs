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
        public FieldDefinition([NotNull] TypeDefinition declaringType, IntPtr address)
            : base((declaringType ?? throw new ArgumentNullException(nameof(declaringType))).Image, address)
        {
            this.DeclaringType = declaringType;
            this.TypeInfo = new TypeInfo(declaringType.Image, this.ReadPtr(0x0));
            this.Name = this.ReadString(this.Process.SizeOfPtr);
            this.Offset = this.ReadInt32(this.Process.SizeOfPtr * 3);
        }

        ITypeDefinition IFieldDefinition.DeclaringType => this.DeclaringType;

        public string Name { get; }

        ITypeInfo IFieldDefinition.TypeInfo => this.TypeInfo;

        public TypeDefinition DeclaringType { get; }

        public int Offset { get; set; }

        public TypeInfo TypeInfo { get; }

        public TValue GetValue<TValue>(IntPtr address)
        {
            var offset = this.Offset - (this.DeclaringType.IsValueType ? this.Process.SizeOfPtr * 2 : 0);
            return (TValue)this.TypeInfo.GetValue(address + offset);
        }
    }
}