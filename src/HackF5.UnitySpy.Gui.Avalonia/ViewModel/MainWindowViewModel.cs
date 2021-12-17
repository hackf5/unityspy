namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using global::Avalonia.Controls;
    using global::Avalonia.Threading;
    using HackF5.UnitySpy.Offsets;
    using HackF5.UnitySpy.ProcessFacade;
    using HackF5.UnitySpy.Util;
    using HackF5.UnitySpy.Gui.Avalonia.Mvvm;
    using HackF5.UnitySpy.Gui.Avalonia.View;
    using HackF5.UnitySpy.Gui.ViewModel;
    using ReactiveUI;

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly CommandCollection commandCollection;

        private readonly ObservableCollection<ProcessViewModel> processesCollection;

        protected ProcessViewModel selectedProcess;
        
        private readonly MainWindow mainWindow;

        private RawMemoryView rawMemoryView;

        private int modeSelectedIndex = 0;

        private string memPseudoFilePath;

        private string dumpFilesPath;

        private string mapsFilePath;

        private string gameExecutableFilePath;       

        private ProcessFacade processFacade; 
        
        private AssemblyImageViewModel image;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.processesCollection = new ObservableCollection<ProcessViewModel>();
            this.RefreshProcesses = ReactiveCommand.Create(this.StartRefresh);
            this.OpenMemPseudoFile = ReactiveCommand.Create(this.StartOpenMemPseudoFile);
            this.OpenDumpFilesDirectory = ReactiveCommand.Create(this.StartOpenDumpFilesDirectory);  
            this.OpenMapsFile = ReactiveCommand.Create(this.StartOpenMapsFile); 
            this.OpenGameExecutableFile = ReactiveCommand.Create(this.StartOpenGameExecutableFile);   
            this.BuildImageAssembly = ReactiveCommand.Create(this.StartBuildImageAssembly); 
            this.ReadRawMemory = ReactiveCommand.Create(this.StartReadRawMemory);
            this.commandCollection = new CommandCollection();  
            this.rawMemoryView = new RawMemoryView();
            this.StartRefresh();
        }       

        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 

        public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public string[] Modes
        {
            get
            {
                if (IsWindows)
                {
                    return new string[]
                    {
                        "Direct",
                        "Linux Dump Files"
                    };
                }
                else if (IsMacOS)
                {
                    return new string[]
                    {
                        "Direct (root access)",
                        "Linux Dump Files",
                        "Client (user access) - server.c (root access)"
                    };
                }
                if (IsLinux)
                {
                    return new string[]
                    {
                        "Direct (root access)",
                        "Dump Files",
                        "Client (user access) - server.c (root access)",
                        "PTrace (process_vm_readv)"
                    };
                }

                return null;
            }
        }
        
        public ObservableCollection<ProcessViewModel> Processes => this.processesCollection;

        public ProcessViewModel SelectedProcess 
        {
            get => this.selectedProcess;
            set 
            {
                if (this.selectedProcess != value) 
                {
                    this.RaiseAndSetIfChanged(ref this.selectedProcess, value);
                    if(this.IsWindows || this.IsMacOS)
                    {
                        this.StartBuildImageAssembly();
                    }
                    else if(IsLinux)
                    {
                        this.MemPseudoFilePath = $"/proc/{value.Id}/mem";
                    }
                }
            }
        }

        public string MemPseudoFilePath
        {
            get => this.memPseudoFilePath;
            set => this.RaiseAndSetIfChanged(ref this.memPseudoFilePath, value);
        }

        public string DumpFilesPath
        {
            get => this.dumpFilesPath;
            set => this.RaiseAndSetIfChanged(ref this.dumpFilesPath, value);
        }

        public string MapsFilePath
        {
            get => this.mapsFilePath;
            set => this.RaiseAndSetIfChanged(ref this.mapsFilePath, value);
        }
                                
        public string GameExecutableFilePath
        {
            get => this.gameExecutableFilePath;
            set => this.RaiseAndSetIfChanged(ref this.gameExecutableFilePath, value);
        }
                                
        public int ModeSelectedIndex
        {
            get => this.modeSelectedIndex;
            set {
                this.RaiseAndSetIfChanged(ref this.modeSelectedIndex, value);
                this.RaisePropertyChanged(nameof(IsLinuxDirectMode));
                this.RaisePropertyChanged(nameof(IsDumpMode));
                this.RaisePropertyChanged(nameof(NeedsGameExecutableFile));
            }
        }

        public bool IsLinuxDirectMode => IsLinux && ModeSelectedIndex == 0;

        public bool IsDumpMode => ModeSelectedIndex == 1;

        public bool NeedsGameExecutableFile => IsLinux || IsDumpMode;

        public AssemblyImageViewModel Image
        {
            get => this.image;
            set => this.RaiseAndSetIfChanged(ref this.image, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshProcesses { get; }
        
        public ReactiveCommand<Unit, Unit> OpenMemPseudoFile { get; }
        
        public ReactiveCommand<Unit, Unit> OpenDumpFilesDirectory { get; }
        
        public ReactiveCommand<Unit, Unit> OpenMapsFile { get; }
                
        public ReactiveCommand<Unit, Unit> OpenGameExecutableFile { get; }
        
        public ReactiveCommand<Unit, Unit> BuildImageAssembly { get; }
        
        public ReactiveCommand<Unit, Unit> ReadRawMemory { get; }

        private void StartRefresh() 
        {
            Task.Run(this.ExecuteRefreshProcesses);
        }

        private void StartOpenMapsFile() 
        {
            Task.Run(this.ExecuteOpenMapsFileCommand);
        }

        private void StartOpenMemPseudoFile() 
        {
            Task.Run(this.ExecuteOpenMemPseudoFileCommand);
        }

        private void StartOpenDumpFilesDirectory() 
        {
            Task.Run(this.ExecuteOpenDumpFilesDirectoryCommand);
        }

        private void StartOpenGameExecutableFile() 
        {
            Task.Run(this.ExecuteOpenGameExecutableFileCommand);
        }

        private void StartBuildImageAssembly() 
        {            
            Task.Run(() => BuildImageAsync());
        }

        private void StartReadRawMemory() 
        {                        
            Dispatcher.UIThread.InvokeAsync(this.ShowRawMemoryView);
        }

        private async Task ExecuteRefreshProcesses()
        {
            Process[] processes = await Task.Run(Process.GetProcesses);

            this.processesCollection.Clear();
            IEnumerable<Process> orderedProcesses = processes.OrderBy(p => p.ProcessName);
            foreach(Process process in orderedProcesses) 
            {
                this.processesCollection.Add(new ProcessViewModel(process));
            }

            this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Name == "MTGA.exe"); 
        }   
        
        private async void ExecuteOpenMapsFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/maps file", this.mapsFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.MapsFilePath = filenames[0];
			}
		}
        
        private async void ExecuteOpenMemPseudoFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/mem pseudo-file", this.memPseudoFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.MemPseudoFilePath = filenames[0];
			}
		}
        
        private async void ExecuteOpenDumpFilesDirectoryCommand()
		{
			OpenFolderDialog dlg = new OpenFolderDialog();
			dlg.Title = "Open dump files directory";
            dlg.Directory = this.dumpFilesPath;

			this.DumpFilesPath = await dlg.ShowAsync(mainWindow);
		}
        
        private async void ExecuteOpenGameExecutableFileCommand()
		{
			var filenames = await this.ShowOpenFileDialog("Open game executable file", this.gameExecutableFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.GameExecutableFilePath = filenames[0];
			}
		}

        private async Task<string[]> ShowOpenFileDialog(string tile, string preselectedFile)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = tile;
			dlg.AllowMultiple = false;

            dlg.Directory = Path.GetDirectoryName(preselectedFile);

			return await dlg.ShowAsync(mainWindow);
		}

        public async Task ShowRawMemoryView()
        {            
            if (this.processFacade == null)
            {
                this.CreateProcessFacade();
            }

            await this.rawMemoryView.ShowDialog(this.mainWindow);
        }
        
        private async Task BuildImageAsync()
        {
            IAssemblyImage assemblyImage;            
            try
            {           
                MonoLibraryOffsets monoLibraryOffsets;
                if (this.processFacade == null)
                {
                    this.CreateProcessFacade();
                }

                if (this.IsWindows)      
                {
                    if (this.IsDumpMode)
                    {
                        monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(this.gameExecutableFilePath);
                    }
                    else
                    {
                        ProcessFacadeWindows windowsProcessFacade = this.processFacade as ProcessFacadeWindows;
                        monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(windowsProcessFacade.GetMainModuleFileName());
                    }
                }
                else if (this.IsMacOS)
                {
                    if (this.IsDumpMode)
                    {
                        monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(this.gameExecutableFilePath);
                    }
                    else
                    {
                        ProcessFacadeMacOS macOSProcessFacade = this.processFacade as ProcessFacadeMacOS;
                        monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(macOSProcessFacade.Process.MainModule.FileName);
                    }
                }
                else if (this.IsLinux)
                { 
                    if (!File.Exists(this.gameExecutableFilePath))  
                    {
                        ProcessFacadeLinux linuxProcessFacade = this.processFacade as ProcessFacadeLinux;  
                        string processName = Process.GetProcessById(this.selectedProcess.Id).ProcessName;
                        this.gameExecutableFilePath = linuxProcessFacade.GetModulePath(processName);
                    }
                    monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(this.gameExecutableFilePath);
                }
                else
                {   
                    throw new NotSupportedException("Platform not supported");
                }
                UnityProcessFacade unityProcess = new UnityProcessFacade(processFacade, monoLibraryOffsets);
                assemblyImage = AssemblyImageFactory.Create(unityProcess);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await MessageBox.ShowAsync(
                    $"Failed to load process {this.selectedProcess.Name} ({this.selectedProcess.Id}).",
                    ex.Message);
                return;
            }

            TypeDefinitionContentViewModel.Factory typeDefContFactory = 
                type => new TypeDefinitionContentViewModel(type, 
                    staticField => new StaticFieldViewModel(staticField));

            InstanceFieldViewModel.Factory instanceFieldFactory = 
                (field, managedObject) => new InstanceFieldViewModel(field, managedObject);

            ManagedObjectInstanceContentViewModel.Factory managedInstanceFactory = 
                instance => new ManagedObjectInstanceContentViewModel(instance, instanceFieldFactory);

            ListItemViewModel.Factory listItemFactory = 
                (item, index) => new ListItemViewModel(item, index);

            ListContentViewModel.Factory listContentFactory = 
                list => new ListContentViewModel(list, listItemFactory);

            TypeDefinitionViewModel.Factory typeDefFactory = 
                type => new TypeDefinitionViewModel(this.commandCollection, 
                                                    type, 
                                                    typeDefContFactory, 
                                                    managedInstanceFactory, 
                                                    listContentFactory);

            Dispatcher.UIThread.InvokeAsync(() => this.Image = new AssemblyImageViewModel(assemblyImage, typeDefFactory));

            bool hasPapa = false;
            foreach(var type in assemblyImage.TypeDefinitions)
            {
                Console.WriteLine($"========= Type Definition = {type.Name} on = {type.NamespaceName} fieldCount = {type.Fields.Count} =========");

                if(type.Name.StartsWith("PAPA")) 
                {
                    hasPapa = true;
                    break;
                }
            }

            Console.WriteLine($"========= Type Definitions Read = {assemblyImage.TypeDefinitions.Count()} Has PAPA = {hasPapa} =========");
        }

        private void CreateProcessFacade()
        {
            if (this.ModeSelectedIndex == 1)
            {
                this.processFacade = new ProcessFacadeLinuxDump(this.mapsFilePath, this.dumpFilesPath);
            }
            else if (this.IsLinux)
            {
                switch (this.ModeSelectedIndex)
                {
                    case 0:
                        this.processFacade = new ProcessFacadeLinuxDirect(this.selectedProcess.Id, this.memPseudoFilePath);
                        break;
                    case 2:
                        this.processFacade = new ProcessFacadeLinuxClient(this.selectedProcess.Id);
                        break;
                    case 3:
                        this.processFacade = new ProcessFacadeLinuxPTrace(this.selectedProcess.Id);
                        break;
                    default:
                        throw new NotSupportedException("Linux mode not supported");
                }
            }
            else
            {
                Process process = Process.GetProcessById(this.selectedProcess.Id);
                if (this.IsWindows)
                {
                    this.processFacade = new ProcessFacadeWindows(process);
                }
                else if (this.IsMacOS)
                {
                    switch (this.ModeSelectedIndex)
                    {
                        case 0:
                            this.processFacade = new ProcessFacadeMacOSDirect(process);
                            break;
                        case 2:
                            this.processFacade = new ProcessFacadeMacOSClient(process);
                            break;
                        default:
                            throw new NotSupportedException("MacOS mode not supported");
                    }
                }
                else
                {
                    throw new NotSupportedException("Platform not supported");
                }
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                RawMemoryViewModel rawMemoryViewModel = (RawMemoryViewModel)this.rawMemoryView.DataContext;
                if (rawMemoryViewModel != null)
                {
                    rawMemoryViewModel.Process = this.processFacade;
                }
            });
        }
    }
}
