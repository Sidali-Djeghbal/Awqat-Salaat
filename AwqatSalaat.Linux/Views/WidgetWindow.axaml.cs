using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using AwqatSalaat.Linux.ViewModels;
using Serilog;
using System;
using System.Runtime.InteropServices;

namespace AwqatSalaat.Linux.Views
{
    public partial class WidgetWindow : Window
    {
        private bool _isDragging;
        private Point _dragStartPoint;

        public WidgetWindow()
        {
            InitializeComponent();

            // Configure window properties
            SystemDecorations = SystemDecorations.None;
            ShowInTaskbar = false;
            Topmost = true;
            CanResize = false;
            Background = Brushes.Transparent;

            // Set up event handlers
            PointerPressed += WidgetWindow_PointerPressed;
            PointerReleased += WidgetWindow_PointerReleased;
            PointerMoved += WidgetWindow_PointerMoved;

            // Handle data context changes
            DataContextChanged += WidgetWindow_DataContextChanged;
        }

        private void WidgetWindow_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is WidgetViewModel viewModel)
            {
                // Subscribe to view model events
                viewModel.NearNotificationStarted += ViewModel_NearNotificationStarted;
                viewModel.NearNotificationStopped += ViewModel_NearNotificationStopped;
            }
        }

        private void ViewModel_NearNotificationStarted()
        {
            // Handle notification started (e.g., change background color)
            Dispatcher.UIThread.Post(() =>
            {
                // Update UI to show notification state
                mainPanel.Classes.Add("notification-active");
            });
        }

        private void ViewModel_NearNotificationStopped()
        {
            // Handle notification stopped
            Dispatcher.UIThread.Post(() =>
            {
                // Update UI to remove notification state
                mainPanel.Classes.Remove("notification-active");
            });
        }

        private void WidgetWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(this);
            _isDragging = true;
            _dragStartPoint = point;
            e.Pointer.Capture(this);
        }

        private void WidgetWindow_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }

        private void WidgetWindow_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var point = e.GetPosition(this);
                var delta = point - _dragStartPoint;

                Position = new PixelPoint(
                    Position.X + (int)delta.X,
                    Position.Y + (int)delta.Y);
            }
        }

        public void PositionNearSystemTray()
        {
            try
            {
                // Get screen information
                var screen = Screens.Primary;
                if (screen == null) return;

                // Position near system tray (typically bottom-right)
                var screenBounds = screen.Bounds;
                var windowSize = ClientSize;

                // Calculate position (bottom right by default)
                var x = screenBounds.Width - windowSize.Width - 10;
                var y = screenBounds.Height - windowSize.Height - 40; // Leave space for taskbar

                Position = new PixelPoint(x, y);
                Log.Information("Positioned widget near system tray at {X},{Y}", x, y);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to position widget near system tray: {Message}", ex.Message);
            }
        }
    }
}