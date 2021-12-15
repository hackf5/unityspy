namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Linux specific facade over a process that provides access to its memory space
    /// that requires /proc/$pid/mem read privileges.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxDump : ProcessFacadeLinux
    {
        private readonly List<MemoryMapping> dumpFiles;

        public ProcessFacadeLinuxDump(string mapsFilePath, string dumpsPath)
            : base(mapsFilePath)
        {
            string[] dumpFilePaths = Directory.GetFiles(dumpsPath);
            this.dumpFiles = new List<MemoryMapping>(dumpFilePaths.Length);
            string[] splitFileName;

            foreach (var filePath in dumpFilePaths)
            {
                splitFileName = new FileInfo(filePath).Name.Split('-');
                if (splitFileName.Length == 2)
                {
                    this.dumpFiles.Add(new MemoryMapping(splitFileName[0], splitFileName[1], filePath, false));
                }
            }
        }

        protected unsafe override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {
            this.ReadProcessMemory(buffer, processAddress, 0, length);
        }

        private unsafe void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int bufferIndex,
            int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("the buffer parameter cannot be null");
            }

            foreach (MemoryMapping dumpFile in this.dumpFiles)
            {
                if (dumpFile.Contains(processAddress))
                {
                    using (FileStream memFileStream = new FileStream(dumpFile.ModuleName, FileMode.Open))
                    {
                        long fileOffset = processAddress.ToInt64() - dumpFile.StartAddress.ToInt64();
                        memFileStream.Seek(fileOffset, 0);

                        if (fileOffset + length < dumpFile.Size)
                        {
                            if (length != memFileStream.Read(buffer, bufferIndex, length))
                            {
                                throw new Exception("Error reading data from " + dumpFile.ModuleName);
                            }
                        }
                        else
                        {
                            int bytesToReadInCurrentFile = Convert.ToInt32(dumpFile.Size - fileOffset);
                            if (bytesToReadInCurrentFile != memFileStream.Read(buffer, bufferIndex, bytesToReadInCurrentFile))
                            {
                                throw new Exception("Error reading data from " + dumpFile.ModuleName);
                            }

                            int newBufferIndex = bufferIndex + bytesToReadInCurrentFile;
                            int newLength = length - bytesToReadInCurrentFile;
                            this.ReadProcessMemory(buffer, dumpFile.EndAddress, newBufferIndex, newLength);
                        }
                    }

                    return;
                }
            }
        }
    }
}