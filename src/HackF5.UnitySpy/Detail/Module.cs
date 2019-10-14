namespace HackF5.UnitySpy.Detail
{
    using System;

    public class Module
    {
        public Module(string moduleName, IntPtr baseAddress, uint size)
        {
            this.ModuleName = moduleName;
            this.BaseAddress = baseAddress;
            this.Size = size;
        }

        public string ModuleName { get; set; }

        public IntPtr BaseAddress { get; set; }

        public uint Size { get; set; }
    }
}
