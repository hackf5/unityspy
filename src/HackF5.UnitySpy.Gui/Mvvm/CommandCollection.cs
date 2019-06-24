namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using JetBrains.Annotations;

    [Register(RegistrationType.Transient)]
    public class CommandCollection
    {
        private readonly ConcurrentDictionary<string, ICommand> commands = new ConcurrentDictionary<string, ICommand>();

        public CommandCollection(OperationController controller)
        {
            this.Controller = controller;
        }

        public OperationController Controller { get; }

        public AsyncCommand CreateAsyncCommand(
            [NotNull] Func<Task> execute,
            Func<bool> canExecute = default,
            [CallerMemberName] string propertyName = default)
        {
            return (AsyncCommand)this.commands.GetOrAdd(
                propertyName ?? throw new ArgumentException("Property name cannot be null.", nameof(propertyName)),
                _ => new AsyncCommand(execute, canExecute, this.Controller));
        }

        public AsyncCommand<T> CreateAsyncCommand<T>(
            [NotNull] Func<T, Task> execute,
            Func<T, bool> canExecute = default,
            [CallerMemberName] string propertyName = default)
        {
            return (AsyncCommand<T>)this.commands.GetOrAdd(
                propertyName ?? throw new ArgumentException("Property name cannot be null.", nameof(propertyName)),
                _ => new AsyncCommand<T>(execute, canExecute, this.Controller));
        }

        public SyncCommand CreateSyncCommand(
            [NotNull] Action execute,
            Func<bool> canExecute = default,
            [CallerMemberName] string propertyName = default)
        {
            return (SyncCommand)this.commands.GetOrAdd(
                propertyName ?? throw new ArgumentException("Property name cannot be null.", nameof(propertyName)),
                _ => new SyncCommand(execute, canExecute, this.Controller));
        }

        public SyncCommand<T> CreateSyncCommand<T>(
            [NotNull] Action<T> execute,
            Func<T, bool> canExecute = default,
            [CallerMemberName] string propertyName = default)
        {
            return (SyncCommand<T>)this.commands.GetOrAdd(
                propertyName ?? throw new ArgumentException("Property name cannot be null.", nameof(propertyName)),
                _ => new SyncCommand<T>(execute, canExecute, this.Controller));
        }
    }
}