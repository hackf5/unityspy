namespace HackF5.UnitySpy.Gui.Avalonia.Mvvm
{
    using System;
    using System.Threading.Tasks;
    using global::Avalonia;
    using global::Avalonia.Threading;
    using HackF5.UnitySpy.Gui.Mvvm;
    using JetBrains.Annotations;

    public class AvaloniaMainThreadInvoker : MainThreadInvoker
    {
        protected override bool CheckAccess() => Application.Current.CheckAccess();

        protected override void Invoke(Action action) => Dispatcher.UIThread.Post(action);

        protected override Task InvokeAsync(Action action) => Dispatcher.UIThread.InvokeAsync(action);
    }
}