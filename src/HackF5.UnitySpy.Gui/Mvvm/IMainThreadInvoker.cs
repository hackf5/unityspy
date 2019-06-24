namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Threading.Tasks;

    public interface IMainThreadInvoker
    {
        void InvokeOnMainThread(Action action);

        TValue InvokeFuncOnMainThread<TValue>(Func<TValue> func);

        Task InvokeOnMainThreadAsync(Action action);
    }
}