namespace HackF5.UnitySpy.Detail
{
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoType instance in a Mono process. This object describes type information.
    /// There is no direct .NET equivalent, but probably comes under the umbrella of <see cref="System.Type"/>.
    /// See: _MonoType in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/metadata-internals.h.
    /// </summary>
    [PublicAPI]
    public class TypeInfo : MemoryObject, ITypeInfo
    {
        public TypeInfo(AssemblyImage image, uint address)
            : base(image, address)
        {
            this.Data = this.ReadUInt32(0x0);
            this.Attrs = this.ReadUInt32(0x4);
        }

        public uint Attrs { get; }

        public uint Data { get; }

        public bool IsStatic => (this.Attrs & 0x10) == 0x10;

        public bool IsConstant => (this.Attrs & 0x40) == 0x40;

        public TypeCode TypeCode => (TypeCode)(0xff & (this.Attrs >> 16));

        public bool TryGetTypeDefinition(out ITypeDefinition typeDefinition)
        {
            switch (this.TypeCode)
            {
                case TypeCode.CLASS:
                case TypeCode.SZARRAY:
                case TypeCode.GENERICINST:
                case TypeCode.VALUETYPE:
                    typeDefinition = this.Image.GetTypeDefinition(this.Process.ReadPtr(this.Data));
                    return true;
                default:
                    typeDefinition = null;
                    return false;
            }
        }

        public object GetValue(uint address) => this.Process.ReadManaged(this, address);
    }
}