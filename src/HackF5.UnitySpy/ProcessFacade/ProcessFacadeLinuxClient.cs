namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// A Linux specific facade over a process that provides access to its memory space
    /// through a server running with /proc/$pid/mem read privileges.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxClient : ProcessFacadeLinux
    {
        private readonly ProcessFacadeClient client;

        public ProcessFacadeLinuxClient(int processId)
            : base(processId)
        {
            this.client = new ProcessFacadeClient(processId);
        }

        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
            => this.client.ReadProcessMemory(buffer, processAddress, length);
    }
}