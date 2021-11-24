namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;

    /// <summary>
    /// A MacOS specific facade over a process that provides access to its memory space
    /// through a server running with root privileges.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeMacOSClient : ProcessFacadeMacOS
    {
        private readonly ProcessFacadeClient client;

        public ProcessFacadeMacOSClient(Process process)
            : base(process)
        {
            this.client = new ProcessFacadeClient(process.Id);
        }

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

            this.client.ReadProcessMemory(buffer, processAddress, size ?? buffer.Length);
        }

        public override ModuleInfo GetModule(string moduleName)
            => this.client.GetModuleInfo(moduleName);
    }
}