namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Threading;
    using System.Windows.Input;
    using JetBrains.Annotations;

    [PublicAPI]
    public class SyncCommand : SyncCommand<object>
    {
        public SyncCommand(Action execute, Func<bool> canExecute = default, OperationController operationController = default)
            : base(o => execute(), o => canExecute?.Invoke() ?? true, operationController)
        {
        }
    }

    [PublicAPI]
    public class SyncCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecute;

        private readonly Action<T> execute;

        private readonly OperationController operationController;

        private volatile int isExecutingFlag;

        public SyncCommand(
            [NotNull] Action<T> execute,
            Func<T, bool> canExecute = default,
            OperationController operationController = default)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? (o => true);
            this.operationController = operationController ?? new OperationController();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool IsExecuting => this.isExecutingFlag == 1;

        public bool CanExecute(object parameter) =>
            !this.IsExecuting && !this.operationController.IsExecuting && this.canExecute((T)parameter);

        public void Execute(object parameter)
        {
            if (Interlocked.CompareExchange(ref this.isExecutingFlag, 1, 0) != 0)
            {
                // prevent reentrancy.
                return;
            }

            if (!this.operationController.TryRent(out var token))
            {
                Interlocked.Exchange(ref this.isExecutingFlag, 0);
                return;
            }

            try
            {
                this.execute((T)parameter);
            }
            finally
            {
                token.Dispose();
                Interlocked.Exchange(ref this.isExecutingFlag, 0);
                this.ChangeCanExecute();
            }
        }

        public void ChangeCanExecute()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}