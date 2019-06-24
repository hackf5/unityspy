namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System.Threading.Tasks;
    using System.Windows;

    [Register(RegistrationType.Singleton)]
    public class DialogService
    {
        public Task ShowAsync(string title, string description)
        {
            return MainThreadInvoker.Current.InvokeOnMainThreadAsync(() => MessageBox.Show(description, title));
        }
    }
}