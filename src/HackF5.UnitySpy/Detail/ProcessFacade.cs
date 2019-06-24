namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacade
    {
        public ProcessFacade(int processId)
        {
            this.Process = Process.GetProcessById(processId);
        }

        public Process Process { get; }

        public string ReadAsciiString(uint address, int maxSize = 1024)
        {
            return this.ReadBufferValue(address, maxSize, b => b.ToAsciiString());
        }

        public string ReadAsciiStringPtr(uint address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(uint address)
        {
            return this.ReadBufferValue(address, sizeof(int), b => b.ToInt32());
        }

        public object ReadManaged([NotNull] TypeInfo type, uint address)
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
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToInt64);

                case TypeCode.U8:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToUInt64);

                case TypeCode.R4:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToSingle);

                case TypeCode.R8:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToDouble);

                case TypeCode.STRING:
                    return this.ReadManagedString(address);

                case TypeCode.SZARRAY:
                    return this.ReadManagedArray(type, address);

                case TypeCode.VALUETYPE:
                    return this.ReadManagedStructInstance(type, address);

                case TypeCode.CLASS:
                    return this.ReadManagedClassInstance(type, address);

                case TypeCode.GENERICINST:
                    return this.ReadManagedGenericObject(type, address);

                // may need supporting
                case TypeCode.VAR:
                    //// this is the type code for generic structs class-internals.h_MonoGenericParam. Good luck with
                    //// that!
                case TypeCode.OBJECT:
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

        public byte[] ReadModule([NotNull] ProcessModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var buffer = new byte[module.ModuleMemorySize];
            this.ReadProcessMemory(buffer, module.BaseAddress);
            return buffer;
        }

        public uint ReadPtr(uint address) => this.ReadUInt32(address);

        public uint ReadUInt32(uint address)
        {
            return this.ReadBufferValue(address, sizeof(uint), b => b.ToUInt32());
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead);

        private TValue ReadBufferValue<TValue>(uint address, int size, Func<byte[], TValue> read)
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

        private object[] ReadManagedArray(TypeInfo type, uint address)
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

            var count = this.ReadInt32(ptr + 0xc);
            var start = ptr + 0x10;
            var result = new object[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = elementDefinition.TypeInfo.GetValue(start + (uint)(i * arrayDefinition.Size));
            }

            return result;
        }

        private ManagedClassInstance ReadManagedClassInstance(TypeInfo type, uint address)
        {
            var ptr = this.ReadPtr(address);
            return ptr == Constants.NullPtr
                ? default
                : new ManagedClassInstance(type.Image, ptr);
        }

        private object ReadManagedGenericObject(TypeInfo type, uint address)
        {
            var genericDefinition = type.Image.GetTypeDefinition(this.ReadPtr(type.Data));
            if (genericDefinition.IsValueType)
            {
                return new ManagedStructInstance(genericDefinition, address);
            }

            return this.ReadManagedClassInstance(type, address);
        }

        private string ReadManagedString(uint address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            var length = this.ReadInt32(ptr + 0x8);

            return this.ReadBufferValue(
                ptr + 0xc,
                2 * length,
                b => Encoding.Unicode.GetString(b, 0, 2 * length));
        }

        private object ReadManagedStructInstance(TypeInfo type, uint address)
        {
            var definition = type.Image.GetTypeDefinition(type.Data);
            var obj = new ManagedStructInstance(definition, address);
            return obj.TypeDefinition.IsEnum ? obj.GetValue<object>("value__") : obj;
        }

        private void ReadProcessMemory(
            byte[] buffer,
            uint processAddress,
            bool allowPartialRead = false,
            int? size = default)
            => this.ReadProcessMemory(buffer, new IntPtr(processAddress), allowPartialRead, size);

        private void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, Constants.NullPtr);
                if (!ProcessFacade.ReadProcessMemory(
                    this.Process.Handle,
                    processAddress,
                    bufferPointer,
                    size ?? buffer.Length,
                    out _))
                {
                    var error = Marshal.GetLastWin32Error();
                    if ((error == 299) && allowPartialRead)
                    {
                        return;
                    }

                    throw new Win32Exception(error);
                }
            }
            finally
            {
                bufferHandle.Free();
            }
        }
    }
}