﻿namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(ModuleInfo.ModuleName) + "}")]
    public class ModuleInfo
    {
        public ModuleInfo(string moduleName, IntPtr baseAddress, uint size)
        {
        }

        public IntPtr BaseAddress { get; }

        public string ModuleName { get; }

        public uint Size { get; }
    }
}