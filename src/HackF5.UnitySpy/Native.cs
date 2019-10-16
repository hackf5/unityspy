namespace HackF5.UnitySpy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;

    internal static class Native
    {
        private const string PsApiDll = "psapi.dll";

        [DllImport(Native.PsApiDll, SetLastError = true)]
        public static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport(Native.PsApiDll, SetLastError = true)]
        public static extern bool GetModuleInformation(
            IntPtr hProcess,
            IntPtr hModule,
            out ModuleInformation lpModInfo,
            uint cb);

        public static IntPtr[] GetProcessModulePointers(ProcessFacade process)
        {
            var modulePointers = new IntPtr[2048 * Constants.SizeOfPtr];

            // Determine number of modules
            if (!Native.EnumProcessModulesEx(
                process.Process.Handle,
                modulePointers,
                modulePointers.Length,
                out var bytesNeeded,
                0x03))
            {
                throw new COMException(
                    "Failed to read modules from the external process.",
                    Marshal.GetLastWin32Error());
            }

            var result = new IntPtr[bytesNeeded / IntPtr.Size];
            Buffer.BlockCopy(modulePointers, 0, result, 0, bytesNeeded);
            return result;
        }

        // https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        [DllImport(Native.PsApiDll, SetLastError = true)]
        private static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In] [Out]
            IntPtr[] lphModule,
            int cb,
            [MarshalAs(UnmanagedType.U4)] out int lpCbNeeded,
            uint dwFilterFlag);

        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public readonly IntPtr BaseOfDll;

            public readonly uint SizeInBytes;

            private readonly IntPtr entryPoint;
        }
    }
}