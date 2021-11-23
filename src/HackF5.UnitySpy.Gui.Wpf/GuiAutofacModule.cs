namespace HackF5.UnitySpy.Gui.Wpf
{
    using Autofac;
    using HackF5.UnitySpy.Gui.Wpf.Mvvm;

    public class GuiAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.AutoRegisterAssemblyTypes(this.ThisAssembly);
            builder.RegisterAssemblyTypes(this.ThisAssembly).Where(t => t.Name.EndsWith("ViewModel"));
            ViewLocator.RegisterAssembly(this.ThisAssembly);
        }
    }
}