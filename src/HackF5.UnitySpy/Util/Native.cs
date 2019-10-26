namespace HackF5.UnitySpy.Util
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Detail;

    internal static class Native
    {
        private const string PsApiDll = "psapi.dll";
        private const string KernelDll = "kernel32.dll";

        public static IntPtr GetProcessHandle(int processId)
        {
            return Native.OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.QueryInformation, true, processId);
        }

        public static IntPtr[] GetProcessModulePointers(ProcessFacade process)
        {
            var modulePointers = new IntPtr[2048 * Constants.SizeOfPtr];

            // Determine number of modules
            if (!Native.EnumProcessModulesEx(
                process.Process,
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

        [DllImport(Native.KernelDll, SetLastError = true)]
        public static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId
        );

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public readonly IntPtr BaseOfDll;

            public readonly uint SizeInBytes;

            private readonly IntPtr entryPoint;
        }
    }
}