namespace HackF5.UnitySpy.Gui.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;
    using HackF5.UnitySpy.Gui.ViewModel;

    public partial class AssemblyImageView
    {
        public AssemblyImageView()
        {
            this.InitializeComponent();
            this.DefinitionsList.DataContextChanged += (sender, args) =>
                this.Dispatcher.BeginInvoke(new Action(this.RefreshView), DispatcherPriority.Input);
        }

        private bool Filter(object item)
        {
            if (!(item is TypeDefinitionViewModel definition))
            {
                return false;
            }

            if ((this.HasStaticOnly.IsChecked ?? false) && !definition.HasStaticFields)
            {
                return false;
            }

            var text = this.ListViewFilter.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (!(definition.FullName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            return true;
        }

        private void HasStaticOnly_OnClick(object sender, RoutedEventArgs e) => this.RefreshView();

        private void ListViewFilter_OnTextChanged(object sender, TextChangedEventArgs e) => this.RefreshView();

        private void RefreshView()
        {
            var source = this.DefinitionsList?.ItemsSource;
            if (source == null)
            {
                return;
            }

            var view = CollectionViewSource.GetDefaultView(source);
            view.Filter = this.Filter;
            view.Refresh();
        }

        private void DefinitionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DefinitionsList.ScrollIntoView(this.DefinitionsList.SelectedItem);
        }
    }
}