namespace HackF5.UnitySpy.Gui.View
{
    using System.Windows.Input;
    using HackF5.UnitySpy.Gui.ViewModel;

    public partial class ManagedObjectInstanceContentView
    {
        public ManagedObjectInstanceContentView()
        {
            this.InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(this.ItemsList.SelectedItem is InstanceFieldViewModel item))
            {
                return;
            }

            if (!(this.DataContext is ManagedObjectInstanceContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Name);
        }
    }
}