namespace HackF5.UnitySpy.Gui.Avalonia
{
    using System.Reflection;
    using Autofac;
    using HackF5.UnitySpy.Gui.Avalonia.Mvvm;
    using HackF5.UnitySpy.Gui.Mvvm;
    using Module = Autofac.Module;

    public class GuiAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var guiBaseAssembly = Assembly.Load("HackF5.UnitySpy.Gui");
            builder.AutoRegisterAssemblyTypes(this.ThisAssembly);
            builder.AutoRegisterAssemblyTypes(guiBaseAssembly);
            builder.RegisterAssemblyTypes(this.ThisAssembly).Where(t => t.Name.EndsWith("ViewModel"));
            builder.RegisterAssemblyTypes(guiBaseAssembly).Where(t => t.Name.EndsWith("ViewModel"));
            ViewLocator.RegisterAssembly(this.ThisAssembly);
            ViewLocator.RegisterAssembly(guiBaseAssembly);
        }
    }
}