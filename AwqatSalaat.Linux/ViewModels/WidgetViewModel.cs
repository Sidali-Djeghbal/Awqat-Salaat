using Avalonia.Threading;
using AwqatSalaat.Data;
using AwqatSalaat.Helpers;
using AwqatSalaat.Linux.Services;
using ReactiveUI;
using Serilog;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;

namespace AwqatSalaat.Linux.ViewModels
{
    /// <summary>
    /// ViewModel for the main widget window
    /// </summary>
    public class WidgetViewModel : ReactiveObject
    {
        private readonly LinuxConfigurationAdapter _configAdapter;
        private readonly NotificationService _notificationService;
        private readonly Timer _updateTimer;
        private readonly Timer _notificationTimer;
        private PrayerTimes? _prayerTimes;
        private PrayerConfig? _config;
        private PrayerTimeViewModel? _next;
        private bool _isNearNotification;

        // Events for notification state changes
        public event Action? NearNotificationStarted;
        public event Action? NearNotificationStopped;

        public WidgetViewModel()
        {
            _configAdapter = new LinuxConfigurationAdapter();
            _notificationService = new NotificationService();
            
            // Load configuration
            _config = _configAdapter.LoadConfiguration<PrayerConfig>();
            
            // Initialize timers
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            
            _notificationTimer = new Timer(500);
            _notificationTimer.Elapsed += NotificationTimer_Elapsed;
            
            // Initialize commands
            OpenSettingsCommand = ReactiveCommand.CreateFromTask(OpenSettingsAsync);
            
            // Start the application
            Initialize();
        }

        /// <summary>
        /// Gets the next prayer time information
        /// </summary>
        public PrayerTimeViewModel? Next
        {
            get => _next;
            private set => this.RaiseAndSetIfChanged(ref _next, value);
        }

        /// <summary>
        /// Gets whether a notification is currently active
        /// </summary>
        public bool IsNearNotification
        {
            get => _isNearNotification;
            private set
            {
                if (_isNearNotification != value)
                {
                    _isNearNotification = value;
                    this.RaisePropertyChanged();
                    
                    if (_isNearNotification)
                        NearNotificationStarted?.Invoke();
                    else
                        NearNotificationStopped?.Invoke();
                }
            }
        }

        /// <summary>
        /// Command to open the settings dialog
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }
        
        /// <summary>
        /// Refreshes the configuration and updates prayer times
        /// </summary>
        public void RefreshConfiguration()
        {
            try
            {
                Log.Information("Refreshing configuration");
                
                // Reload configuration
                _config = _configAdapter.LoadConfiguration<PrayerConfig>();
                
                // Update notification service configuration
                _notificationService.ReloadConfiguration();
                
                // Update prayer times
                UpdatePrayerTimes();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to refresh configuration: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Initializes the view model and starts the timers
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Load prayer times
                UpdatePrayerTimes();
                
                // Start timers
                _updateTimer.Start();
                _notificationTimer.Start();
                
                Log.Information("Widget view model initialized");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize widget view model: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Updates the prayer times data
        /// </summary>
        private void UpdatePrayerTimes()
        {
            try
            {
                if (_config == null)
                {
                    Log.Warning("Cannot update prayer times: Configuration is null");
                    return;
                }

                // Create prayer times service based on configuration
                var service = PrayerTimesService.Create(_config);
                _prayerTimes = service.GetPrayerTimes(DateTime.Now);
                
                // Update the next prayer time
                UpdateNextPrayer();
                
                Log.Information("Prayer times updated");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update prayer times: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Updates the next prayer information
        /// </summary>
        private async void UpdateNextPrayer()
        {
            if (_prayerTimes == null) return;
            
            var now = DateTime.Now;
            var nextPrayer = _prayerTimes.GetNextPrayer(now);
            
            if (nextPrayer != null)
            {
                var remainingTime = nextPrayer.Time - now;
                var formattedTime = FormatRemainingTime(remainingTime);
                
                Next = new PrayerTimeViewModel
                {
                    Name = nextPrayer.Name,
                    Time = nextPrayer.Time.ToString("HH:mm"),
                    RemainingTime = formattedTime
                };
                
                // Check if we're near notification time
                IsNearNotification = remainingTime.TotalMinutes <= 5;
                
                // Check for prayer time notifications
                await _notificationService.CheckAndNotifyAsync(nextPrayer, remainingTime);
            }
        }

        /// <summary>
        /// Formats the remaining time in a human-readable format
        /// </summary>
        private string FormatRemainingTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        /// <summary>
        /// Timer callback to update the UI
        /// </summary>
        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(UpdateNextPrayer);
        }

        /// <summary>
        /// Timer callback for notification blinking effect
        /// </summary>
        private void NotificationTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // This is handled by the view through the IsNearNotification property
        }

        /// <summary>
        /// Opens the settings dialog
        /// </summary>
        private async Task OpenSettingsAsync()
        {
            // TODO: Implement settings dialog
            Log.Information("Settings dialog requested");
            
            // After settings are changed, update configuration and prayer times
            _config = _configAdapter.LoadConfiguration<PrayerConfig>();
            UpdatePrayerTimes();
        }
    }

    /// <summary>
    /// View model for prayer time display
    /// </summary>
    public class PrayerTimeViewModel : ReactiveObject
    {
        private string _name = string.Empty;
        private string _time = string.Empty;
        private string _remainingTime = string.Empty;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Time
        {
            get => _time;
            set => this.RaiseAndSetIfChanged(ref _time, value);
        }

        public string RemainingTime
        {
            get => _remainingTime;
            set => this.RaiseAndSetIfChanged(ref _remainingTime, value);
        }
    }
}