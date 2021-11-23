namespace HackF5.UnitySpy.Gui.Avalonia
{
    using Autofac;
    using global::Avalonia;
    using global::Avalonia.Controls.ApplicationLifetimes;
    using global::Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.Gui.Mvvm;
    using HackF5.UnitySpy.Gui.Avalonia.Mvvm;
    using HackF5.UnitySpy.Gui.Avalonia.ViewModel;
    using HackF5.UnitySpy.Gui.Avalonia.View;

    public class App : Application
    {
        private IContainer container;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            MainThreadInvoker.Current = new AvaloniaMainThreadInvoker();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.DataContext = new MainWindowViewModel(mainWindow);
                desktop.MainWindow = mainWindow;
            }

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(this.GetType().Assembly);
            this.container = builder.Build();
            
            // Not sure why this is here. Is it needed?
            var theme = new global::Avalonia.Themes.Default.DefaultTheme();
            theme.TryGetResource("Button", out _);

            base.OnFrameworkInitializationCompleted();
        }
    }
}
