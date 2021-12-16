namespace Avalonia.Controls
{
    using System;
    using System.Runtime.InteropServices;
    using Avalonia.Styling;
    using Avalonia.Platform;
    using Avalonia.Controls.Primitives;

    public class FluentWindow : Window, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Window);

        public FluentWindow()
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;

            WindowTransparencyLevel transparencyLevel;

            // Blur background looks weird in windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                transparencyLevel = WindowTransparencyLevel.None;
            }
            else
            {
                transparencyLevel = WindowTransparencyLevel.AcrylicBlur;
            }

            TransparencyLevelHint = transparencyLevel;            

            this.GetObservable(WindowStateProperty)
                .Subscribe(x =>
                {
                    PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                    PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
                });

            this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
                .Subscribe(x =>
                {
                    if (!x)
                    {
                        SystemDecorations = SystemDecorations.Full;
                        TransparencyLevelHint = transparencyLevel;
                    }
                });
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);            
            ExtendClientAreaChromeHints = 
                ExtendClientAreaChromeHints.PreferSystemChrome |                 
                ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
    }
}
