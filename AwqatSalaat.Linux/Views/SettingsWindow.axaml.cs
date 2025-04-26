using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AwqatSalaat.Linux.ViewModels;
using Serilog;
using System;

namespace AwqatSalaat.Linux.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            
            // Set up event handlers
            DataContextChanged += SettingsWindow_DataContextChanged;
            
            Log.Information("Settings window initialized");
        }

        private void SettingsWindow_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                // Subscribe to view model events
                viewModel.SaveCommand.Subscribe(_ => Close());
                viewModel.CancelCommand.Subscribe(_ => Close());
            }
        }

        /// <summary>
        /// Centers the window on the screen
        /// </summary>
        public void CenterOnScreen()
        {
            var screen = Screens.Primary;
            if (screen != null)
            {
                double x = (screen.Bounds.Width - Width) / 2;
                double y = (screen.Bounds.Height - Height) / 2;
                Position = new PixelPoint((int)x, (int)y);
            }
        }
    }
}