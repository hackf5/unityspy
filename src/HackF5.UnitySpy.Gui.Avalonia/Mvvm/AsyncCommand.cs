#pragma warning disable SA1402 // File may only contain a single type

namespace HackF5.UnitySpy.Gui.Avalonia.Mvvm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using JetBrains.Annotations;

    [PublicAPI]
    public class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand([NotNull] Func<Task> execute, Func<bool> canExecute = default)
            : base(o => execute(), o => canExecute?.Invoke() ?? true)
        {
        }
    }

    [PublicAPI]
    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecute;

        private readonly Func<T, Task> execute;

        private volatile int isExecutingFlag;

        public AsyncCommand(
            [NotNull] Func<T, Task> execute,
            Func<T, bool> canExecute = default)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? (o => true);
        }

        public event EventHandler CanExecuteChanged;
        // {
        //     add => CommandManager.RequerySuggested += value;
        //     remove => CommandManager.RequerySuggested -= value;
        // }

        public bool IsExecuting => this.isExecutingFlag == 1;

        public Task Task { get; private set; } = Task.CompletedTask;

        public bool CanExecute(object parameter) =>
            !this.IsExecuting && this.canExecute((T)parameter);

        public async void Execute(object parameter)
        {
            if (Interlocked.CompareExchange(ref this.isExecutingFlag, 1, 0) != 0)
            {
                // prevent reentrancy.
                return;
            }

            var tcs = new TaskCompletionSource<object>();
            this.Task = tcs.Task;

            try
            {
                this.ChangeCanExecute();

                await this.execute((T)parameter);
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                throw;
            }
            finally
            {
                Interlocked.Exchange(ref this.isExecutingFlag, 0);
                this.ChangeCanExecute();
            }
        }

        public void ChangeCanExecute()
        {
            //CommandManager.InvalidateRequerySuggested();
        }
    }
}