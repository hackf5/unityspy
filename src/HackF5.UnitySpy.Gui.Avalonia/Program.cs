namespace HackF5.UnitySpy.Gui.Avalonia
{
    using System;
    using global::Avalonia;
    using global::Avalonia.Controls.ApplicationLifetimes;
    using global::Avalonia.ReactiveUI;

    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new SkiaOptions{ MaxGpuResourceSizeBytes = 8096000})
                .With(new Win32PlatformOptions{ AllowEglInitialization = true })
                .LogToTrace()
                .UseReactiveUI();
    }
}
