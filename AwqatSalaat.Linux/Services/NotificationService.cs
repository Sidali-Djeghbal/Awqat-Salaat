using AwqatSalaat.Data;
using AwqatSalaat.Helpers;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AwqatSalaat.Linux.Services
{
    /// <summary>
    /// Handles system notifications for prayer times on Linux
    /// </summary>
    public class NotificationService
    {
        private readonly LinuxConfigurationAdapter _configAdapter;
        private PrayerConfig? _config;
        private DateTime _lastNotificationTime = DateTime.MinValue;

        public NotificationService()
        {
            _configAdapter = new LinuxConfigurationAdapter();
            _config = _configAdapter.LoadConfiguration<PrayerConfig>();
            Log.Information("Notification service initialized");
        }

        /// <summary>
        /// Checks if a notification should be shown for the given prayer time
        /// </summary>
        public async Task CheckAndNotifyAsync(Prayer prayer, TimeSpan timeUntilPrayer)
        {
            if (_config == null) return;

            try
            {
                // Check if notifications are enabled
                if (!_config.EnableNotifications) return;

                // Check if we should notify for this prayer
                if (!ShouldNotifyForPrayer(prayer)) return;

                // Check if we're within the notification time window
                if (!IsWithinNotificationWindow(timeUntilPrayer)) return;

                // Check if we've already notified recently
                if (DateTime.Now - _lastNotificationTime < TimeSpan.FromMinutes(1)) return;

                // Show the notification
                await ShowNotificationAsync(prayer);
                _lastNotificationTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to check or show notification: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Determines if notifications should be shown for the given prayer
        /// </summary>
        private bool ShouldNotifyForPrayer(Prayer prayer)
        {
            if (_config == null) return false;

            return prayer.Type switch
            {
                PrayerType.Fajr => _config.NotifyFajr,
                PrayerType.Sunrise => _config.NotifySunrise,
                PrayerType.Dhuhr => _config.NotifyDhuhr,
                PrayerType.Asr => _config.NotifyAsr,
                PrayerType.Maghrib => _config.NotifyMaghrib,
                PrayerType.Isha => _config.NotifyIsha,
                _ => false
            };
        }

        /// <summary>
        /// Determines if the current time is within the notification window
        /// </summary>
        private bool IsWithinNotificationWindow(TimeSpan timeUntilPrayer)
        {
            if (_config == null) return false;

            // Get notification time in minutes
            int notificationMinutes = _config.NotificationTime;

            // Check if we're within the notification window
            return timeUntilPrayer.TotalMinutes <= notificationMinutes && 
                   timeUntilPrayer.TotalMinutes >= 0;
        }

        /// <summary>
        /// Shows a system notification for the prayer time
        /// </summary>
        private async Task ShowNotificationAsync(Prayer prayer)
        {
            try
            {
                // Format the notification message
                string title = "Awqat Salaat";
                string message = $"{prayer.Name} prayer time at {prayer.Time:HH:mm}";

                // Use notify-send command for Linux notifications
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "notify-send",
                        Arguments = $"--app-name=\"Awqat Salaat\" --icon=dialog-information \"{title}\" \"{message}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                // Play adhan sound if enabled
                if (_config?.EnableAdhan == true)
                {
                    await PlayAdhanAsync(prayer);
                }

                Log.Information("Notification shown for {Prayer} at {Time}", prayer.Name, prayer.Time);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to show notification: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Plays the adhan sound for the prayer
        /// </summary>
        private async Task PlayAdhanAsync(Prayer prayer)
        {
            try
            {
                if (_config == null) return;

                // Get the adhan sound file path
                string soundsDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Sounds");

                string soundFile = prayer.Type == PrayerType.Fajr
                    ? Path.Combine(soundsDir, _config.FajrAdhanSound.FileName)
                    : Path.Combine(soundsDir, _config.RegularAdhanSound.FileName);

                // Check if the file exists
                if (!File.Exists(soundFile))
                {
                    Log.Warning("Adhan sound file not found: {SoundFile}", soundFile);
                    return;
                }

                // Play the sound using a command-line player
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "paplay", // PulseAudio player
                        Arguments = $"\"{soundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                Log.Information("Adhan played for {Prayer}", prayer.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to play adhan: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Reloads the configuration
        /// </summary>
        public void ReloadConfiguration()
        {
            _config = _configAdapter.LoadConfiguration<PrayerConfig>();
            Log.Information("Notification service configuration reloaded");
        }
    }
}