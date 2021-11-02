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
                var localIo = new iovec
                {
                    iov_base = ptr.ToPointer(),
                    iov_len = length
                };
                var remoteIo = new iovec
                {
                    iov_base = processAddress.ToPointer(),
                    iov_len = length
                };

                var res = process_vm_readv(this.processId, &localIo, 1, &remoteIo, 1, 0);
                if(res != -1)
                {
                    //Array.Copy(*(byte[]*)ptr, 0, buffer, 0, length);
                    Marshal.Copy(ptr, buffer, 0, length);
                }
                else
                {
                    throw new Exception("Error while trying to read memory through from process_vm_readv. Check errno.");
                }
            }
        }

        [DllImport("libc")]
        private static extern unsafe int process_vm_readv(
            int pid,
            iovec* local_iov,
            ulong liovcnt,
            iovec* remote_iov,
            ulong riovcnt,
            ulong flags);

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct iovec
        {
            public void* iov_base;
            public int iov_len;
        }

    }
}