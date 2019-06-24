namespace HackF5.UnitySpy.Gui
{
    using Autofac;
    using HackF5.UnitySpy.Gui.Mvvm;

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