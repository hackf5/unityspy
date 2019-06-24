namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System.Threading.Tasks;
    using System.Windows;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class DialogService
    {
        public static Task ShowAsync(string title, string description)
        {
            return MainThreadInvoker.Current.InvokeOnMainThreadAsync(() => MessageBox.Show(description, title));
        }
    }
}