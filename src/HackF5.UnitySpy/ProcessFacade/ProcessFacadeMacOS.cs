namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;

    /// <summary>
    /// A MacOS specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeMacOS : ProcessFacade
    {
        private readonly Process process;

        public ProcessFacadeMacOS(Process process)
        {
            this.process = process;
        }

        public Process Process => this.process;

        public override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("the buffer parameter cannot be null");
            }

            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                if (ProcessFacadeMacOS.ReadProcessMemory(
                    this.process.Id,
                    processAddress,
                    bufferPointer,
                    size ?? buffer.Length) != 0)
                {
                    var error = Marshal.GetLastWin32Error();
                    if ((error == 299) && allowPartialRead)
                    {
                        return;
                    }

                    throw new InvalidOperationException($"Could not read memory. Error code: {error}");
                }
            }
            finally
            {
                bufferHandle.Free();
            }
        }

        public override ModuleInfo GetModule(string moduleName)
        {
            if (!this.Is64Bits)
            {
                throw new NotSupportedException("MacOS for 32 binaries is not supported currently");
            }

            IntPtr baseAddress;
            uint size = 0;
            IntPtr buffer = Marshal.AllocCoTaskMem(2048 + 1);
            string path;
            try
            {
                Marshal.WriteByte(buffer, 2048, 0); // Ensure there will be a null byte after call
                baseAddress = GetModuleInfo(this.process.Id, moduleName, ref size, buffer);
                path = Marshal.PtrToStringAnsi(buffer);
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }

            return new ModuleInfo(moduleName, baseAddress, size, path);
        }

        [DllImport("macos.dylib", EntryPoint = "read_process_memory_to_buffer", SetLastError = true)]
        private static extern int ReadProcessMemory(
            int processId,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize);

        [DllImport("macos.dylib", EntryPoint = "get_module_info", SetLastError = true)]
        private static extern IntPtr GetModuleInfo(
            int processId,
            string moduleName,
            ref uint nSize,
            IntPtr path);
    }
}