using AppIndicator.Sharp;
using Avalonia.Controls;
using Avalonia.Threading;
using AwqatSalaat.Linux.Views;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace AwqatSalaat.Linux.Services
{
    /// <summary>
    /// Manages the system tray icon and menu for the Linux version of Awqat Salaat
    /// </summary>
    public class SystemTrayManager : IDisposable
    {
        private const string AppIndicatorId = "awqat-salaat";
        private const string IconName = "awqat-salaat";
        private const AppIndicatorCategory Category = AppIndicatorCategory.ApplicationStatus;

        private readonly WidgetWindow _widgetWindow;
        private AppIndicator? _appIndicator;
        private AppIndicatorMenu? _menu;
        private bool _disposedValue;

        public SystemTrayManager(WidgetWindow widgetWindow)
        {
            _widgetWindow = widgetWindow ?? throw new ArgumentNullException(nameof(widgetWindow));
        }

        public void Initialize()
        {
            try
            {
                Log.Information("Initializing system tray icon");

                // Create app indicator
                _appIndicator = new AppIndicator(AppIndicatorId, IconName, Category);
                
                // Create menu
                _menu = new AppIndicatorMenu();

                // Add menu items
                var showItem = new AppIndicatorMenuItem("Show");
                showItem.Activated += ShowItem_Activated;
                _menu.Add(showItem);

                var hideItem = new AppIndicatorMenuItem("Hide");
                hideItem.Activated += HideItem_Activated;
                _menu.Add(hideItem);

                _menu.Add(new AppIndicatorSeparatorMenuItem());

                var settingsItem = new AppIndicatorMenuItem("Settings");
                settingsItem.Activated += SettingsItem_Activated;
                _menu.Add(settingsItem);

                _menu.Add(new AppIndicatorSeparatorMenuItem());

                var quitItem = new AppIndicatorMenuItem("Quit");
                quitItem.Activated += QuitItem_Activated;
                _menu.Add(quitItem);

                // Set the menu
                _appIndicator.Menu = _menu;

                // Show the indicator
                _appIndicator.Status = AppIndicatorStatus.Active;

                Log.Information("System tray icon initialized");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize system tray icon: {Message}", ex.Message);
            }
        }

        private void ShowItem_Activated(object? sender, EventArgs e)
        {
            Log.Information("Show menu item activated");
            Dispatcher.UIThread.Post(() =>
            {
                _widgetWindow.Show();
                _widgetWindow.PositionNearSystemTray();
                _widgetWindow.Activate();
            });
        }

        private void HideItem_Activated(object? sender, EventArgs e)
        {
            Log.Information("Hide menu item activated");
            Dispatcher.UIThread.Post(() => _widgetWindow.Hide());
        }

        private void SettingsItem_Activated(object? sender, EventArgs e)
        {
            Log.Information("Settings menu item activated");
            Dispatcher.UIThread.Post(() =>
            {
                // Open settings dialog
                var settingsWindow = new Views.SettingsWindow
                {
                    DataContext = new ViewModels.SettingsViewModel()
                };
                
                settingsWindow.CenterOnScreen();
                settingsWindow.ShowDialog(_widgetWindow);
                
                // Refresh widget after settings are changed
                if (_widgetWindow.DataContext is ViewModels.WidgetViewModel viewModel)
                {
                    viewModel.RefreshConfiguration();
                }
            });
        }

        private void QuitItem_Activated(object? sender, EventArgs e)
        {
            Log.Information("Quit menu item activated");
            Dispatcher.UIThread.Post(() => App.Quit());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    _appIndicator?.Dispose();
                    _menu?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}