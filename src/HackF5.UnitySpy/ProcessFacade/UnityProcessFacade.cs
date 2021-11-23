namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Offsets;
    using JetBrains.Annotations;

    /// <summary>
    /// A facade over an unity process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class UnityProcessFacade
    {
        private readonly ProcessFacade process;

        private readonly MonoLibraryOffsets monoLibraryOffsets;

        public UnityProcessFacade(ProcessFacade process, MonoLibraryOffsets monoLibraryOffsets)
        {
            this.process = process;
            this.monoLibraryOffsets = monoLibraryOffsets;

            if (monoLibraryOffsets != null)
            {
                this.process.Is64Bits = monoLibraryOffsets.Is64Bits;
            }
        }

        public MonoLibraryOffsets MonoLibraryOffsets => this.monoLibraryOffsets;

        public bool Is64Bits => this.process.Is64Bits;

        public int SizeOfPtr => this.process.SizeOfPtr;

        public ProcessFacade Process => this.process;

        public string ReadAsciiString(IntPtr address, int maxSize = 1024) =>
            this.process.ReadAsciiString(address, maxSize);

        public string ReadAsciiStringPtr(IntPtr address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(IntPtr address) => this.process.ReadInt32(address);

        public long ReadInt64(IntPtr address) => this.process.ReadInt64(address);

        public object ReadManaged([NotNull] TypeInfo type, IntPtr address)
            => this.process.ReadManaged(type, address);

        public IntPtr ReadPtr(IntPtr address) => this.process.ReadPtr(address);

        public uint ReadUInt32(IntPtr address) => this.process.ReadUInt32(address);

        public ulong ReadUInt64(IntPtr address) => this.process.ReadUInt64(address);

        public byte ReadByte(IntPtr address) => this.process.ReadByte(address);

        public byte[] ReadModule([NotNull] ModuleInfo moduleInfo) => this.process.ReadModule(moduleInfo);

        public ModuleInfo GetMonoModule()
        {
            string monoLibrary;

            if (this.process is ProcessFacadeMacOS)
            {
                monoLibrary = this.monoLibraryOffsets.MonoLibraryName.MachOFormatName;
            }
            else
            {
                monoLibrary = this.monoLibraryOffsets.MonoLibraryName.PeFormatName;
            }

            return this.process.GetModule(monoLibrary);
        }
    }
}