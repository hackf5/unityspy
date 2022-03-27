namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using System;
    using System.Threading.Tasks;
    using global::Avalonia;
    using global::Avalonia.Controls;
    using global::Avalonia.Platform;
    using global::Avalonia.Interactivity;
    using global::Avalonia.Markup.Xaml;
    using global::Avalonia.Media.Imaging;

    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    public enum MessageBoxImage
    {   
        Asterisk, 
        Error,
        Exclamation,
        Hand,
        Information,
        None,
        Question,
        Stop,
        Warning
    }

    public class MessageBox : Window
    {                
        private readonly Image image;

        private readonly TextBlock messageTxtBlck;
 
        private readonly Button okBtn;

        private readonly Button cancelBtn;

        private readonly Button yesBtn;

        private readonly Button noBtn;

        private Action<MessageBoxResult> onClose;

        public MessageBox() 
        {
            AvaloniaXamlLoader.Load(this);
            this.image = this.FindControl<Image>("Image");
            this.messageTxtBlck = this.FindControl<TextBlock>("Message");
            this.okBtn = this.FindControl<Button>("Ok");
            this.cancelBtn = this.FindControl<Button>("Cancel");
            this.yesBtn = this.FindControl<Button>("Yes");
            this.noBtn = this.FindControl<Button>("No");
        }

        public MessageBoxImage Image 
        {
            set 
            {
                if(value == MessageBoxImage.None)
                {
                    this.image.IsVisible = false;
                }
                else
                {
                    this.image.IsVisible = true;
                    string uri;
                    switch(value) 
                    {
                        case MessageBoxImage.Asterisk: uri = "/Assets/info.png"; break;
                        case MessageBoxImage.Error: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Exclamation: uri = "/Assets/warning.png"; break;
                        case MessageBoxImage.Hand: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Information: uri = "/Assets/info.png"; break;
                        case MessageBoxImage.Question: uri = "/Assets/question.png"; break;
                        case MessageBoxImage.Stop: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Warning: uri = "/Assets/warning.png"; break;
                        default: throw new NotSupportedException("Message Box Image type not supported");
                    }
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    this.image.Source = new Bitmap(assets.Open(new Uri(uri)));
                }                
            }
        }

        public MessageBoxButton Buttons 
        {
            set 
            {
                this.okBtn.IsVisible =     value == MessageBoxButton.OK       || value == MessageBoxButton.OKCancel;
                this.cancelBtn.IsVisible = value == MessageBoxButton.OKCancel || value == MessageBoxButton.YesNoCancel;
                this.yesBtn.IsVisible =    value == MessageBoxButton.YesNo    || value == MessageBoxButton.YesNoCancel;
                this.noBtn.IsVisible =     value == MessageBoxButton.YesNo    || value == MessageBoxButton.YesNoCancel;
            }
        }

        protected new void ShowDialog(Window parent) {
            if (parent != null) 
            {
                base.ShowDialog(parent);
            }
            else 
            {
                base.Show();
            } 
        }

        public void Ok_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            this.Close(MessageBoxResult.Ok);
        }

        public void Cancel_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            this.Close(MessageBoxResult.Cancel);
        }

        public void Yes_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            this.Close(MessageBoxResult.Yes);
        }

        public void No_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            this.Close(MessageBoxResult.No);
        }

        protected void Close(MessageBoxResult result) 
        {
            base.Close(result);
            this.onClose?.Invoke(result);
        }

        public static void Show(string message, string title, Action<MessageBoxResult> onClose)
        {
            Show(null, message, title, MessageBoxButton.OK, MessageBoxImage.None, onClose);
        }

        public static void Show(string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            Show(null, message, title, buttons, image, null);
        }

        public static void Show(Window parent, string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Action<MessageBoxResult> onClose = null) 
        {
            GetInstance(message, title, buttons, image, onClose).ShowDialog(parent);            
        }

        public static Task<MessageBoxResult> ShowAsync(string text, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None) 
        {
            return ShowAsync(null, text, title, buttons, image);
        }

        public static Task<MessageBoxResult> ShowAsync(Window parent, string text, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            if(parent == null)
            {
                var taskCompletionSource = new TaskCompletionSource<MessageBoxResult>();            
                MessageBox messageBox = GetInstance(text, title, buttons, image, result => taskCompletionSource.TrySetResult(result));
                messageBox.ShowDialog(parent);
                return taskCompletionSource.Task;
            }
            else 
            {
                MessageBox messageBox = GetInstance(text, title, buttons, image);
                return messageBox.ShowDialog<MessageBoxResult>(parent);
            }
        }

        private static MessageBox GetInstance(string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Action<MessageBoxResult> onClose = null) 
        {       
            Console.WriteLine("DEBUG: Showing MemoryView");
            MessageBox messageBox = new MessageBox();
            Console.WriteLine("DEBUG: Showing MemoryView");

            messageBox.Image = image;
            messageBox.Title = title;
            messageBox.messageTxtBlck.Text = message;
            messageBox.Buttons = buttons;
            messageBox.onClose = onClose;

            return messageBox;
        }

    }

}