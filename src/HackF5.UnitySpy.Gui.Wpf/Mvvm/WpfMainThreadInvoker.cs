namespace HackF5.UnitySpy.Gui.Wpf.Mvvm
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using HackF5.UnitySpy.Gui.Mvvm;
    using JetBrains.Annotations;

    public class WpfMainThreadInvoker : MainThreadInvoker
    {
        protected override bool CheckAccess() => Application.Current.CheckAccess();

        protected override void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);

        protected override Task InvokeAsync(Action action) => Application.Current.Dispatcher.InvokeAsync(action);
    }
}