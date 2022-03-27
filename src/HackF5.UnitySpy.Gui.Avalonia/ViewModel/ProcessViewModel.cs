namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System.Diagnostics;
    using ReactiveUI;

    public class ProcessViewModel : ReactiveObject
    {
        private readonly Process process;

        public int Id => process.Id;

        public string Name => process.ProcessName;

        public string NameAndId => Name + "(" + Id + ")";

        public ProcessViewModel(Process process)
        {
            this.process = process;
        }  

        public override int GetHashCode()
        {
            return process.GetHashCode() * 7;
        }
    }
}
