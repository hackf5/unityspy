namespace HackF5.UnitySpy.Gui.Wpf.View
{
    public partial class TypeDefinitionView
    {
        public TypeDefinitionView()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) => this.PathTextBox.Focus();
        }
    }
}