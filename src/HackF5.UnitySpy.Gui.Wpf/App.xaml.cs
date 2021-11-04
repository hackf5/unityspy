namespace HackF5.UnitySpy.Gui.Wpf
{
    using System.Windows;
    using Autofac;
    using HackF5.UnitySpy.Gui.Wpf.Mvvm;
    using HackF5.UnitySpy.Gui.Wpf.ViewModel;

    public partial class App
    {
        private IContainer container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(this.GetType().Assembly);
            this.container = builder.Build();

            this.MainWindow = (Window)ViewLocator.GetViewFor(this.container.Resolve<MainViewModel>());
            this.MainWindow.Show();
        }
    }
}
