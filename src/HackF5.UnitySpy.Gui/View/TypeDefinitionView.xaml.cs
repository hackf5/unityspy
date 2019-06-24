namespace HackF5.UnitySpy.Gui.View
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