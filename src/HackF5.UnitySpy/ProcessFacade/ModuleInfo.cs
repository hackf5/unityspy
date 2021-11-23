namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(ModuleInfo.ModuleName) + "}")]
    public class ModuleInfo
    {
        public ModuleInfo(string moduleName, IntPtr baseAddress, uint size, string path)
        {
            this.ModuleName = moduleName;
            this.BaseAddress = baseAddress;
            this.Size = size;
            this.Path = path;
        }

        public IntPtr BaseAddress { get; }

        public string ModuleName { get; }

        public string Path { get; }

        public uint Size { get; }
    }
}