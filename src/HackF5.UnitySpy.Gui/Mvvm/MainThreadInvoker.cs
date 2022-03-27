namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using JetBrains.Annotations;

    public abstract class MainThreadInvoker : IMainThreadInvoker
    {
        public static IMainThreadInvoker Current { get; set; }

        public void InvokeOnMainThread([InstantHandle] [NotNull] Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (this.CheckAccess())
            {
                action();
                return;
            }

            this.Invoke(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (this.CheckAccess())
            {
                action();
                return;
            }

            await this.InvokeAsync(action);
        }

        protected abstract bool CheckAccess();

        protected abstract void Invoke(Action action);

        protected abstract Task InvokeAsync(Action action);
    }
}