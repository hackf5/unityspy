namespace HackF5.UnitySpy.Offsets
{
    public struct MonoLibraryName
    {
        private readonly string peFormatName;

        private readonly string machOFormatName;

        public MonoLibraryName(string peFormatName, string machOFormatName)
        {
            this.peFormatName = peFormatName;
            this.machOFormatName = machOFormatName;
        }

        public string PeFormatName => this.peFormatName;

        public string MachOFormatName => this.machOFormatName;
    }
}