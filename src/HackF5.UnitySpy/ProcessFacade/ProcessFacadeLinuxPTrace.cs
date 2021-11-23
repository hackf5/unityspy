namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;

    /// <summary>
    /// A Linux specific facade over a process that provides access to its memory space through ptrace.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxPTrace : ProcessFacadeLinux
    {
        public ProcessFacadeLinuxPTrace(int processId)
            : base(processId)
        {
        }

        protected unsafe override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {
            fixed (byte* bytePtr = buffer)
            {
                var ptr = (IntPtr)bytePtr;
                var localIo = new Iovec
                {
                    IovBase = ptr.ToPointer(),
                    IovLen = length,
                };
                var remoteIo = new Iovec
                {
                    IovBase = processAddress.ToPointer(),
                    IovLen = length,
                };

                var res = ProcessVmReadV(this.ProcessId, &localIo, 1, &remoteIo, 1, 0);
                if (res != -1)
                {
                    // Array.Copy(*(byte[]*)ptr, 0, buffer, 0, length);
                    Marshal.Copy(ptr, buffer, 0, length);
                }
                else
                {
                    throw new Exception("Error while trying to read memory through from process_vm_readv. Check errno.");
                }
            }
        }

        [DllImport("libc", EntryPoint = "process_vm_readv")]
        private static extern unsafe int ProcessVmReadV(
            int pid,
            Iovec* local_iov,
            ulong liovcnt,
            Iovec* remote_iov,
            ulong riovcnt,
            ulong flags);

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct Iovec
        {
            public void* IovBase;
            public int IovLen;
        }
    }
}