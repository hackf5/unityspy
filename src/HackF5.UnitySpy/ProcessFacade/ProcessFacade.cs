namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;
    using TypeCode = HackF5.UnitySpy.Detail.TypeCode;

    /// <summary>
    /// A facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public abstract class ProcessFacade
    {
        public bool Is64Bits { get; set; }

        public int SizeOfPtr => this.Is64Bits ? 8 : 4;

        public string ReadAsciiString(IntPtr address, int maxSize = 1024) =>
            this.ReadBufferValue(address, maxSize, b => b.ToAsciiString());

        public string ReadAsciiStringPtr(IntPtr address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(IntPtr address) =>
            this.ReadBufferValue(address, sizeof(int), b => b.ToInt32());

        public long ReadInt64(IntPtr address) =>
            this.ReadBufferValue(address, sizeof(long), b => b.ToInt64());

        public object ReadManaged([NotNull] TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            switch (type.TypeCode)
            {
                case TypeCode.BOOLEAN:
                    return this.ReadBufferValue(address, 1, b => b[0] != 0);

                case TypeCode.CHAR:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToChar);

                case TypeCode.I1:
                    return this.ReadBufferValue(address, sizeof(byte), b => b[0]);

                case TypeCode.U1:
                    return this.ReadBufferValue(address, sizeof(sbyte), b => unchecked((sbyte)b[0]));

                case TypeCode.I2:
                    return this.ReadBufferValue(address, sizeof(short), ConversionUtils.ToInt16);

                case TypeCode.U2:
                    return this.ReadBufferValue(address, sizeof(ushort), ConversionUtils.ToUInt16);

                case TypeCode.I:
                case TypeCode.I4:
                    return this.ReadInt32(address);

                case TypeCode.U:
                case TypeCode.U4:
                    return this.ReadUInt32(address);

                case TypeCode.I8:
                    return this.ReadInt64(address);
                    // return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToInt64);

                case TypeCode.U8:
                    return this.ReadUInt64(address);
                    // return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToUInt64);

                case TypeCode.R4:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToSingle);

                case TypeCode.R8:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToDouble);

                case TypeCode.STRING:
                    return this.ReadManagedString(address);

                case TypeCode.SZARRAY:
                    return this.ReadManagedArray(type, genericTypeArguments, address);

                case TypeCode.VALUETYPE:
                    try
                    {
                        return this.ReadManagedStructInstance(type, genericTypeArguments, address);
                    }
                    catch (Exception)
                    {
                        return this.ReadInt32(address);
                    }

                case TypeCode.CLASS:
                    return this.ReadManagedClassInstance(type, genericTypeArguments, address);

                case TypeCode.GENERICINST:
                    return this.ReadManagedGenericObject(type, genericTypeArguments, address);

                //// this is the type code for generic structs class-internals.h_MonoGenericParam. Good luck with
                //// that!
                //// Using the Generic Object works in at least some cases, like
                //// when retrieving the NetCache service.
                //// It's probably better to have something incomplete here
                //// that will raise an exception later on than throwing the exception right away?
                case TypeCode.OBJECT:
                    return this.ReadManagedGenericObject(type, genericTypeArguments, address);

                case TypeCode.VAR:
                    // Really not sure this is the way to do it
                    return this.ReadManagedVar(type, genericTypeArguments, address);

                // may need supporting
                case TypeCode.ARRAY:
                case TypeCode.ENUM:
                case TypeCode.MVAR:

                //// junk
                case TypeCode.END:
                case TypeCode.VOID:
                case TypeCode.PTR:
                case TypeCode.BYREF:
                case TypeCode.TYPEDBYREF:
                case TypeCode.FNPTR:
                case TypeCode.CMOD_REQD:
                case TypeCode.CMOD_OPT:
                case TypeCode.INTERNAL:
                case TypeCode.MODIFIER:
                case TypeCode.SENTINEL:
                case TypeCode.PINNED:
                    throw new ArgumentException($"Cannot read values of type '{type.TypeCode}'.");

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        $"Cannot read unknown data type '{type.TypeCode}'.");
            }
        }

        public IntPtr ReadPtr(IntPtr address) =>
            (IntPtr)(this.Is64Bits ? this.ReadUInt64(address) : this.ReadUInt32(address));

        public uint ReadUInt32(IntPtr address) =>
            this.ReadBufferValue(address, sizeof(uint), b => b.ToUInt32());

        public ulong ReadUInt64(IntPtr address) =>
            this.ReadBufferValue(address, sizeof(ulong), b => b.ToUInt64());

        public byte ReadByte(IntPtr address) =>
            this.ReadBufferValue(address, sizeof(byte), b => b.ToByte());

        public byte[] ReadModule([NotNull] ModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException(nameof(moduleInfo));
            }

            var buffer = new byte[moduleInfo.Size];
            this.ReadProcessMemory(buffer, moduleInfo.BaseAddress);
            return buffer;
        }

        public abstract void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default);

        public abstract ModuleInfo GetModule(string moduleName);

        private TValue ReadBufferValue<TValue>(IntPtr address, int size, Func<byte[], TValue> read)
        {
            var buffer = ByteArrayPool.Instance.Rent(size);

            try
            {
                this.ReadProcessMemory(buffer, address, size: size);
                return read(buffer);
            }
            finally
            {
                ByteArrayPool.Instance.Return(buffer);
            }
        }

        private object[] ReadManagedArray(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            var vtable = this.ReadPtr(ptr);
            var arrayDefinitionPtr = this.ReadPtr(vtable);
            var arrayDefinition = type.Image.GetTypeDefinition(arrayDefinitionPtr);
            var elementDefinition = type.Image.GetTypeDefinition(this.ReadPtr(arrayDefinitionPtr));

            var count = this.ReadInt32(ptr + (this.SizeOfPtr * 3));
            var start = ptr + (this.SizeOfPtr * 4);
            var result = new object[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = elementDefinition.TypeInfo.GetValue(genericTypeArguments, start + (i * arrayDefinition.Size));
            }

            return result;
        }

        private ManagedClassInstance ReadManagedClassInstance(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            return ptr == Constants.NullPtr
                ? default
                : new ManagedClassInstance(type.Image, genericTypeArguments, ptr);
        }

        private object ReadManagedGenericObject(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            // TODO check if this is correct because we are getting the wrong class instance for GENERICINST
            var genericDefinition = type.Image.GetTypeDefinition(this.ReadPtr(type.Data));

            if (genericDefinition.IsValueType)
            {
                return new ManagedStructInstance(genericDefinition, genericTypeArguments, address);
            }

            return this.ReadManagedClassInstance(type, genericTypeArguments, address);
        }

        private object ReadManagedVar(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var monoGenericParamPtr = type.Data;
            int numberOfGenericArgument = this.ReadInt32(monoGenericParamPtr + this.SizeOfPtr);

            int offset = 0;
            for (int i = 0; i < numberOfGenericArgument; i++)
            {
                offset += this.GetSize(genericTypeArguments[i].TypeCode) - this.SizeOfPtr;
            }

            var genericArgumentType = genericTypeArguments[numberOfGenericArgument];
            return this.ReadManaged(genericArgumentType, null, address + offset);
        }

        private string ReadManagedString(IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            // Offsets taken from:
            // struct _MonoString {
            //     MonoObject object; // Has two pointers (SizeOfPtr * 2)
            //     int32_t length;
            //     mono_unichar2 chars [MONO_ZERO_LEN_ARRAY];
            // };
            var length = this.ReadInt32(ptr + (this.SizeOfPtr * 2));

            return this.ReadBufferValue(
                ptr + (this.SizeOfPtr * 2) + 4,
                2 * length,
                b => Encoding.Unicode.GetString(b, 0, 2 * length));
        }

        private object ReadManagedStructInstance(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var definition = type.Image.GetTypeDefinition(type.Data);
            var obj = new ManagedStructInstance(definition, genericTypeArguments, address);

            // var t = obj.GetValue<object>("enumSeperator");
            return obj.TypeDefinition.IsEnum ? obj.GetValue<object>("value__") : obj;
        }

        private int GetSize(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.BOOLEAN:
                    return sizeof(bool);

                case TypeCode.CHAR:
                    return sizeof(char);

                case TypeCode.I1:
                    return sizeof(byte);

                case TypeCode.U1:
                    return sizeof(sbyte);

                case TypeCode.I2:
                    return sizeof(short);

                case TypeCode.U2:
                    return sizeof(ushort);

                case TypeCode.I:
                case TypeCode.I4:
                    return sizeof(int);

                case TypeCode.U:
                case TypeCode.U4:
                    return sizeof(uint);

                case TypeCode.I8:
                    return sizeof(long);

                case TypeCode.U8:
                    return sizeof(ulong);

                case TypeCode.R4:
                case TypeCode.R8:
                    return sizeof(char);

                case TypeCode.STRING:
                case TypeCode.SZARRAY:
                case TypeCode.VALUETYPE:
                case TypeCode.CLASS:
                case TypeCode.GENERICINST:
                case TypeCode.OBJECT:
                case TypeCode.VAR:
                    return this.SizeOfPtr;

                // may need supporting
                case TypeCode.ARRAY:
                case TypeCode.ENUM:
                case TypeCode.MVAR:

                //// junk
                case TypeCode.END:
                case TypeCode.VOID:
                case TypeCode.PTR:
                case TypeCode.BYREF:
                case TypeCode.TYPEDBYREF:
                case TypeCode.FNPTR:
                case TypeCode.CMOD_REQD:
                case TypeCode.CMOD_OPT:
                case TypeCode.INTERNAL:
                case TypeCode.MODIFIER:
                case TypeCode.SENTINEL:
                case TypeCode.PINNED:
                    throw new ArgumentException($"Cannot get size of types '{typeCode}'.");

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(typeCode),
                        typeCode,
                        $"Cannot get size of unknown data type '{typeCode}'.");
            }
        }
    }
}