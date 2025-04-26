using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AwqatSalaat.Linux.Services;
using AwqatSalaat.Linux.ViewModels;
using AwqatSalaat.Linux.Views;
using Serilog;
using System;
using System.IO;

namespace AwqatSalaat.Linux
{
    public partial class App : Application
    {
        private SystemTrayManager? _systemTrayManager;
        private WidgetWindow? _widgetWindow;

        public static event Action? Quitting;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Initialize the widget window (will be hidden initially)
                _widgetWindow = new WidgetWindow
                {
                    DataContext = new WidgetViewModel()
                };

                desktop.MainWindow = _widgetWindow;

                // Initialize system tray
                _systemTrayManager = new SystemTrayManager(_widgetWindow);
                _systemTrayManager.Initialize();

                // Handle shutdown
                desktop.ShutdownRequested += Desktop_ShutdownRequested;
                desktop.Exit += Desktop_Exit;

                // Show the widget (positioned near system tray)
                _widgetWindow.Show();
                _widgetWindow.PositionNearSystemTray();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            Log.Information("Shutdown requested");
            Quitting?.Invoke();
        }

        private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Information("Application exiting");
            _systemTrayManager?.Dispose();
        }

        public static void Quit()
        {
            Log.Information("Quitting application");
            Quitting?.Invoke();

            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}