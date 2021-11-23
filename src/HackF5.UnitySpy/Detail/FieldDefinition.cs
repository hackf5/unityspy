namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;
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
        private readonly List<TypeInfo> genericTypeArguments;

        public FieldDefinition([NotNull] TypeDefinition declaringType, IntPtr address)
            : base((declaringType ?? throw new ArgumentNullException(nameof(declaringType))).Image, address)
        {
            this.DeclaringType = declaringType;

            // MonoType        *type;
            this.TypeInfo = new TypeInfo(declaringType.Image, this.ReadPtr(0x0));

            // MonoType        *name;
            this.Name = this.ReadString(this.Process.SizeOfPtr);

            // wee need to skip MonoClass *parent field so we add
            // 3 pointer sizes (*type, *name, *parent) to the base address
            this.Offset = this.ReadInt32(this.Process.SizeOfPtr * 3);

            // Get the generic type arguments
            if (this.TypeInfo.TypeCode == TypeCode.GENERICINST)
            {
                var monoGenericClassAddress = this.TypeInfo.Data;
                var monoClassAddress = this.Process.ReadPtr(monoGenericClassAddress);
                TypeDefinition monoClass = this.Image.GetTypeDefinition(monoClassAddress);

                var monoGenericContainerPtr = monoClassAddress + this.Process.MonoLibraryOffsets.TypeDefinitionGenericContainer;
                var monoGenericContainerAddress = this.Process.ReadPtr(monoGenericContainerPtr);

                var monoGenericContextPtr = monoGenericClassAddress + this.Process.SizeOfPtr;
                var monoGenericInsPtr = this.Process.ReadPtr(monoGenericContextPtr);
                //var argumentCount = this.Process.ReadInt32(monoGenericInsPtr + 0x4);
                var argumentCount = this.Process.ReadInt32(monoGenericContainerAddress + (4 * this.Process.SizeOfPtr));
                var typeArgVPtr = monoGenericInsPtr + 0x8;
                this.genericTypeArguments = new List<TypeInfo>(argumentCount);
                for (int i = 0; i < argumentCount; i++)
                {
                    var genericTypeArgumentPtr = this.Process.ReadPtr(typeArgVPtr + (i * this.Process.SizeOfPtr));
                    this.genericTypeArguments.Add(new TypeInfo(this.Image, genericTypeArgumentPtr));
                }
            }
            else
            {
                this.genericTypeArguments = null;
            }
        }

        ITypeDefinition IFieldDefinition.DeclaringType => this.DeclaringType;

        public string Name { get; }

        ITypeInfo IFieldDefinition.TypeInfo => this.TypeInfo;

        public TypeDefinition DeclaringType { get; }

        public int Offset { get; set; }

        public TypeInfo TypeInfo { get; }

        public TValue GetValue<TValue>(IntPtr address)
        {
            return this.GetValue<TValue>(this.genericTypeArguments, address);
        }

        public TValue GetValue<TValue>(List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var offset = this.Offset - (this.DeclaringType.IsValueType ? this.Process.SizeOfPtr * 2 : 0);
            if (this.genericTypeArguments != null)
            {
                return (TValue)this.TypeInfo.GetValue(this.genericTypeArguments, address + offset);
            }
            else
            {
                return (TValue)this.TypeInfo.GetValue(genericTypeArguments, address + offset);
            }
        }
    }
}