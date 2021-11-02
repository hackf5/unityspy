namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Linux specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public abstract class ProcessFacadeLinux : ProcessFacade
    {
        protected readonly int processId;

        private readonly List<MemoryMapping> mappings;

        public ProcessFacadeLinux(int processId)
        {
            this.processId = processId;

            string[] mappingsInFile = File.ReadAllLines($"/proc/{processId}/maps");
            this.mappings = new List<MemoryMapping>(mappingsInFile.Length);
            string[] lineColumnValues;
            string[] memoryRegion;
            string name;

            foreach (var line in mappingsInFile)
            {
                lineColumnValues = line.Split(' ');
                memoryRegion = lineColumnValues[0].Split('-');
                if (lineColumnValues.Length > 6)
                {
                    name = line.Substring(73);
                }
                else
                {
                    name = "";
                }

                this.mappings.Add(new MemoryMapping(memoryRegion[0], memoryRegion[1], name, lineColumnValues[4] != "0"));
            }
        }

        public override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            int length = size ?? buffer.Length;
            if (this.mappings.Exists(mapping => mapping.Contains(processAddress)))
            {
                this.ReadProcessMemory(buffer, processAddress, length);
            }
            else
            {
                Console.Error.WriteLine($"Attempting to read unmapped address {processAddress.ToString("X")} + {length}");
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
            }
        }

        public override ModuleInfo GetModule(string moduleName)
        {
            int mappingIndex = this.mappings.FindIndex(mapping => mapping.ModuleName.EndsWith(moduleName));

            if (mappingIndex < 0)
            {
                throw new Exception("Mono module not found");
            }

            IntPtr startingAddress = this.mappings[mappingIndex].StartAddress;
            string fullModuleName = this.mappings[mappingIndex].ModuleName;

            while (mappingIndex < this.mappings.Count && (!this.mappings[mappingIndex].IsStartingModule || this.mappings[mappingIndex].ModuleName == fullModuleName))
            {
                mappingIndex++;
            }  

            mappingIndex--;
            uint size = Convert.ToUInt32(MemoryMapping.GetSize(startingAddress, this.mappings[mappingIndex].EndAddress));

            Console.WriteLine($"Mono Module starting address = {startingAddress.ToString("X")}, end address = {this.mappings[mappingIndex].EndAddress.ToString("X")}");

            return new ModuleInfo(moduleName, startingAddress, size, fullModuleName);
        }

        public string GetModulePath(string moduleName)
        {
            int mappingIndex = this.mappings.FindIndex(mapping => mapping.ModuleName.EndsWith(moduleName));

            if (mappingIndex < 0)
            {
                throw new Exception("Mono module not found");
            }

            return this.mappings[mappingIndex].ModuleName;
        }

        protected abstract void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int size);

        protected struct MemoryMapping
        {
            public IntPtr StartAddress { get; set; }

            public IntPtr EndAddress { get; set; }

            public string ModuleName { get; set; }

            public bool IsStartingModule { get; set; }

            public MemoryMapping(string startAddress, string endAddress, string moduleName, bool isStartingModule)
                : this(new IntPtr(Convert.ToInt64(startAddress, 16)),
                       new IntPtr(Convert.ToInt64(endAddress, 16)),
                       moduleName,
                       isStartingModule
                )
            {
            }

            public long Size => MemoryMapping.GetSize(this.StartAddress, this.EndAddress);

            public MemoryMapping(IntPtr startAddress, IntPtr endAddress, string moduleName, bool isStartingModule)
            {
                this.StartAddress = startAddress;
                this.EndAddress = endAddress;
                this.ModuleName = moduleName;
                this.IsStartingModule = isStartingModule;
            }

            public static long GetSize(IntPtr startAddress, IntPtr endAddress)
                => endAddress.ToInt64() - startAddress.ToInt64();

            public bool Contains(IntPtr address, int length = 0)
            {
                long addressAsLong = address.ToInt64();
                return addressAsLong >= this.StartAddress.ToInt64() && addressAsLong + length < this.EndAddress.ToInt64();
            }
        }
    }
}