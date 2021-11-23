namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Linux specific facade over a process that provides access to its memory space
    /// that requires /proc/$pid/mem read privileges.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxDirect : ProcessFacadeLinux
    {
        private readonly string memFilePath;

        public ProcessFacadeLinuxDirect(int processId, string memFilePath)
            : base(processId)
        {
            this.memFilePath = memFilePath;
        }

        protected unsafe override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {
            using (FileStream memFileStream = new FileStream(this.memFilePath, FileMode.Open))
            {
                memFileStream.Seek(processAddress.ToInt64(), 0);
                if (length != memFileStream.Read(buffer, 0, length))
                {
                    throw new Exception("Error reading data from " + this.memFilePath);
                }
            }
        }
    }
}