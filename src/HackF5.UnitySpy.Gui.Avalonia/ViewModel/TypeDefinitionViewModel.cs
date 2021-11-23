namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using HackF5.UnitySpy.Gui.ViewModel;
    using HackF5.UnitySpy.Gui.Avalonia.Mvvm;
    using JetBrains.Annotations;
    using ReactiveUI;

    public class TypeDefinitionViewModel : ReactiveObject
    {
        private const string TrailSeparator = "-->";

        private readonly CommandCollection commands;

        private readonly ITypeDefinition definition;

        private readonly ListContentViewModel.Factory listContentFactory;

        private readonly ManagedObjectInstanceContentViewModel.Factory managedObjectContentFactory;

        private readonly TypeDefinitionContentViewModel.Factory typeDefinitionContentFactory;

        private object content;

        private string path;

        public TypeDefinitionViewModel(
            [NotNull] CommandCollection commands,
            [NotNull] ITypeDefinition definition,
            [NotNull] TypeDefinitionContentViewModel.Factory typeDefinitionContentFactory,
            [NotNull] ManagedObjectInstanceContentViewModel.Factory managedObjectContentFactory,
            [NotNull] ListContentViewModel.Factory listContentFactory)
        {
            this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.typeDefinitionContentFactory = typeDefinitionContentFactory
                ?? throw new ArgumentNullException(nameof(typeDefinitionContentFactory));

            this.managedObjectContentFactory = managedObjectContentFactory
                ?? throw new ArgumentNullException(nameof(managedObjectContentFactory));

            this.listContentFactory = listContentFactory ?? throw new ArgumentNullException(nameof(listContentFactory));

            var model = this.typeDefinitionContentFactory(this.definition);
            model.AppendToTrail += this.ModelOnAppendToTrail;
            this.content = model;
            
            IObservable<bool> canExecute = this.WhenAnyValue(x => x.CanExecutePathBack);
            this.PathBackCommand = ReactiveCommand.CreateFromTask(this.ExecutePathBackAsync, canExecute); 
        }

        public delegate TypeDefinitionViewModel Factory(ITypeDefinition definition);

        public object Content
        {
            get => this.content;
            set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        public string FullName => this.definition.FullName;

        public bool HasStaticFields => this.definition.Fields.Any(f => f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant);

        public string Path
        {
            get => this.path;
            set
            {
                if(this.path != value)
                {
                    this.RaiseAndSetIfChanged(ref this.path, value);

                    this.ParsePath(this.Path ?? string.Empty);

                    this.RaisePropertyChanged(nameof(CanExecutePathBack));
                }
            }
        }

        public ReactiveCommand<Unit, Unit> PathBackCommand { get; }

        public ObservableCollection<string> Trail { get; } = new ObservableCollection<string>();

        private bool CanExecutePathBack => this.Trail.Count > 2;

        private async Task ExecutePathBackAsync()
        {
            await Task.CompletedTask;
            var items = this.Trail.Skip(1).Where(s => s != TypeDefinitionViewModel.TrailSeparator).ToList();
            items.RemoveAt(items.Count - 1);
            this.Path = string.Join(".", items).TrimStart('.');
        }

        private void ModelOnAppendToTrail(object sender, AppendToTrailEventArgs e)
        {
            var items = this.Trail.Skip(1).Where(s => s != TypeDefinitionViewModel.TrailSeparator);
            this.Path = (string.Join(".", items) + $".{e.Value}").TrimStart('.');
        }

        private void ParsePath(string value)
        {
            var parts = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            dynamic current = this.definition;
            var trail = new List<(string part, object item)>();
            foreach (var part in parts)
            {
                try
                {
                    var x = int.TryParse(part, out var index) ? (dynamic)index : part;
                    var next = current[x];
                    if (next is IMemoryObject || next is IList)
                    {
                        current = next;
                        trail.Add((part, current));
                        continue;
                    }

                    break;
                }
                catch
                {
                    break;
                }
            }

            this.Trail.Clear();
            this.Trail.Add(this.definition.FullName);
            this.Trail.Add(TypeDefinitionViewModel.TrailSeparator);
            if (!trail.Any())
            {
                var model = this.typeDefinitionContentFactory(this.definition);
                model.AppendToTrail += this.ModelOnAppendToTrail;
                this.Content = model;
                return;
            }

            foreach (var item in trail)
            {
                this.Trail.Add(item.part);
                this.Trail.Add(TypeDefinitionViewModel.TrailSeparator);
            }

            this.Trail.RemoveAt(this.Trail.Count - 1);

            var content = trail.Last().item;
            if (content is ITypeDefinition type)
            {
                var model = this.typeDefinitionContentFactory(type);
                model.AppendToTrail += this.ModelOnAppendToTrail;
                this.Content = model;
                return;
            }

            if (content is IManagedObjectInstance instance)
            {
                var model = this.managedObjectContentFactory(instance);
                model.AppendToTrail += this.ModelOnAppendToTrail;
                this.Content = model;
                return;
            }

            if (content is IList list)
            {
                var model = this.listContentFactory(list);
                model.AppendToTrail += this.ModelOnAppendToTrail;
                this.Content = model;
                return;
            }

            this.Content = default;
        }
    }
}