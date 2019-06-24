namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using JetBrains.Annotations;

    public class MainThreadInvoker : IMainThreadInvoker
    {
        public static IMainThreadInvoker Current { get; set; } = new MainThreadInvoker();

        public void InvokeOnMainThread([InstantHandle] [NotNull] Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Application.Current.CheckAccess())
            {
                action();
                return;
            }

            Application.Current.Dispatcher.Invoke(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Application.Current.CheckAccess())
            {
                action();
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
}