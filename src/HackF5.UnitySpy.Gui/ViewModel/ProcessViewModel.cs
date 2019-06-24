namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System.Diagnostics;
    using HackF5.UnitySpy.Gui.Mvvm;

    public class ProcessViewModel : PropertyChangedBase
    {
        private readonly Process process;

        public ProcessViewModel(Process process)
        {
            this.process = process;
        }

        public delegate ProcessViewModel Factory(Process process);

        public string Name => this.process.ProcessName;

        public int ProcessId => this.process.Id;
        }
}