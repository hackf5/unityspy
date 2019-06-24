namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using HackF5.UnitySpy.Gui.Mvvm;

    public class MainViewModel : PropertyChangedBase
    {
        private readonly CommandCollection commands;

        private readonly AssemblyImageViewModel.Factory imageFactory;

        private readonly DialogService dialogService;

        private readonly ProcessViewModel.Factory processFactory;

        private AssemblyImageViewModel image;

        private ProcessViewModel selectedProcess;

        public MainViewModel(
            CommandCollection commands,
            ProcessViewModel.Factory processFactory,
            AssemblyImageViewModel.Factory imageFactory,
            DialogService dialogService)
        {
            this.commands = commands;
            this.processFactory = processFactory;
            this.imageFactory = imageFactory;
            this.dialogService = dialogService;
            this.RefreshProcessesCommand.Execute(default);
        }

        public AssemblyImageViewModel Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        public BindableCollection<ProcessViewModel> Processes { get; } = new BindableCollection<ProcessViewModel>();

        public AsyncCommand RefreshProcessesCommand => this.commands.CreateAsyncCommand(this.ExecuteRefreshProcesses);

        public ProcessViewModel SelectedProcess
        {
            get => this.selectedProcess;
            set
            {
                if (!this.SetProperty(ref this.selectedProcess, value))
                {
                    return;
                }

                Task.Run(() => this.BuildImageAsync(this.SelectedProcess));
            }
        }

        private async Task BuildImageAsync(ProcessViewModel process)
        {
            using (this.commands.Controller.Rent())
            {
                try
                {
                    var image = AssemblyImageFactory.Create(process.ProcessId);
                    this.Image = this.imageFactory(image);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    await this.dialogService.ShowAsync(
                        $"Failed to load process {process.Name} ({process.ProcessId}).",
                        ex.Message);
                }
            }
        }

        private async Task ExecuteRefreshProcesses()
        {
            this.Image = default;
            var processes = await Task.Run(Process.GetProcesses);

            this.SelectedProcess = default;
            this.Processes.IsNotifying = false;
            this.Processes.Clear();

            foreach (var process in processes.OrderBy(p => p.ProcessName).Select(p => this.processFactory(p)))
            {
                this.Processes.Add(process);
            }

            this.Processes.IsNotifying = true;
            this.Processes.Refresh();

            this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Name == "Hearthstone");
        }
    }
}