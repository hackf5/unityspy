namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using JetBrains.Annotations;

    /// <summary>
    /// A facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacade
    {
        public ProcessFacade(int processId)
        {
        }

        public IntPtr Process { get; }
         
        public string ReadAsciiString(uint address, int maxSize = 1024)
        { 
            return null;
        }

        public string ReadAsciiStringPtr(uint address, int maxSize = 1024) => null;

        public int ReadInt32(uint address)
        {
#pragma warning disable CS0037 // Cannot convert null to 'int' because it is a non-nullable value type
            return 0;
#pragma warning restore CS0037 // Cannot convert null to 'int' because it is a non-nullable value type
        }

        public byte[] ReadModule([NotNull] ModuleInfo monoModuleInfo)
        {
            return new byte[0];
        }

        public uint ReadPtr(uint address) => 0;

        public uint ReadUInt32(uint address)
        {
            return 0;
        }

        private TValue ReadBufferValue<TValue>(uint address, int size, Func<byte[], TValue> read)
        {
            return default(TValue);
        }

        private string ReadManagedString(uint address)
        {
            return null;
        }
    }
}