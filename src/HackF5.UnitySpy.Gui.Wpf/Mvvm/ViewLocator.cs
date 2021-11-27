namespace HackF5.UnitySpy.Gui.Wpf.Mvvm
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using Autofac.Util;
    using JetBrains.Annotations;

    public static class ViewLocator
    {
        private static readonly HashSet<Assembly> RegisteredAssemblies = new HashSet<Assembly>();

        private static readonly ConcurrentDictionary<Type, Func<FrameworkElement>> ViewFactories =
            new ConcurrentDictionary<Type, Func<FrameworkElement>>();

        private static readonly Dictionary<string, Type> ViewTypes = new Dictionary<string, Type>();

        public static FrameworkElement GetViewFor(object viewModel)
        {
            if (viewModel == default)
            {
                return default;
            }

            var view = ViewLocator.ViewFactories.GetOrAdd(viewModel.GetType(), ViewLocator.GetViewFactory)();
            view.DataContext = viewModel;
            return view;
        }

        public static void RegisterAssembly(Assembly assembly)
        {
            if (!ViewLocator.RegisteredAssemblies.Add(assembly))
            {
                return;
            }

            var assemblyTypes = assembly.GetLoadableTypes()
                .Where(
                    t =>
                        t.IsSubclassOf(typeof(FrameworkElement))
                        && t.Name.EndsWith("View")
                        && (t.GetConstructor(Array.Empty<Type>()) != default));

            foreach (var assemblyType in assemblyTypes)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                ViewLocator.ViewTypes[assemblyType.Name] = assemblyType;
            }
        }

        private static Func<FrameworkElement> GetViewFactory([NotNull] Type viewModelType)
        {
            // ReSharper disable once PossibleNullReferenceException
            var key = viewModelType.Name.Replace("ViewModel", "View");

            if (!ViewLocator.ViewTypes.TryGetValue(key, out var viewType))
            {
                return () => new TextBlock { Text = $"No view exists for view model of type {viewModelType}." };
            }

            return () => (FrameworkElement)Activator.CreateInstance(viewType);
        }
    }
}