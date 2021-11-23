namespace HackF5.UnitySpy.Gui.Avalonia.Mvvm
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using HackF5.UnitySpy.Gui.Mvvm;
    using JetBrains.Annotations;

    [UsedImplicitly]
    [Register(RegistrationType.Transient)]
    public class CommandCollection
    {
        private readonly ConcurrentDictionary<string, ICommand> commands = new ConcurrentDictionary<string, ICommand>();

        public AsyncCommand CreateAsyncCommand(
            [NotNull] Func<Task> execute,
            Func<bool> canExecute = default,
            [CallerMemberName] string propertyName = default)
        {
            return (AsyncCommand)this.commands.GetOrAdd(
                propertyName ?? throw new ArgumentException("Property name cannot be null.", nameof(propertyName)),
                _ => new AsyncCommand(execute, canExecute));
        }
    }
}