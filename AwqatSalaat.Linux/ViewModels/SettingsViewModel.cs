using Avalonia.Threading;
using AwqatSalaat.Data;
using AwqatSalaat.Helpers;
using AwqatSalaat.Linux.Services;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AwqatSalaat.Linux.ViewModels
{
    /// <summary>
    /// ViewModel for the settings window
    /// </summary>
    public class SettingsViewModel : ReactiveObject
    {
        private readonly LinuxConfigurationAdapter _configAdapter;
        private PrayerConfig _config;
        private string _locationText = string.Empty;
        private string _selectedNotificationTime = "5 minutes";
        private string _selectedLanguage = "English";
        private string _selectedCalculationMethod = "Muslim World League";
        private string _selectedJuristicMethod = "Standard (Shafi, Hanbali, Maliki)";
        private string _selectedTimeFormat = "24-hour";
        private string _selectedTheme = "Dark";
        private string _selectedFajrAdhan = "Default";
        private string _selectedRegularAdhan = "Default";

        public SettingsViewModel()
        {
            _configAdapter = new LinuxConfigurationAdapter();
            _config = _configAdapter.LoadConfiguration<PrayerConfig>() ?? new PrayerConfig();
            
            // Initialize location text
            _locationText = $"{_config.Latitude}, {_config.Longitude} ({_config.City}, {_config.Country})";
            
            // Initialize selected values from config
            InitializeSelectedValues();
            
            // Initialize commands
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(Cancel);
            DetectLocationCommand = ReactiveCommand.CreateFromTask(DetectLocationAsync);
            OpenWebsiteCommand = ReactiveCommand.Create(OpenWebsite);
            
            Log.Information("Settings view model initialized");
        }

        #region Properties

        /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        public PrayerConfig Config
        {
            get => _config;
            set => this.RaiseAndSetIfChanged(ref _config, value);
        }

        /// <summary>
        /// Gets or sets the location text
        /// </summary>
        public string LocationText
        {
            get => _locationText;
            set => this.RaiseAndSetIfChanged(ref _locationText, value);
        }

        /// <summary>
        /// Gets or sets the selected notification time
        /// </summary>
        public string SelectedNotificationTime
        {
            get => _selectedNotificationTime;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedNotificationTime, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected language
        /// </summary>
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected calculation method
        /// </summary>
        public string SelectedCalculationMethod
        {
            get => _selectedCalculationMethod;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCalculationMethod, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected juristic method
        /// </summary>
        public string SelectedJuristicMethod
        {
            get => _selectedJuristicMethod;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedJuristicMethod, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected time format
        /// </summary>
        public string SelectedTimeFormat
        {
            get => _selectedTimeFormat;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTimeFormat, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected theme
        /// </summary>
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTheme, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected Fajr adhan
        /// </summary>
        public string SelectedFajrAdhan
        {
            get => _selectedFajrAdhan;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFajrAdhan, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets or sets the selected regular adhan
        /// </summary>
        public string SelectedRegularAdhan
        {
            get => _selectedRegularAdhan;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedRegularAdhan, value);
                UpdateConfigFromSelection();
            }
        }

        /// <summary>
        /// Gets the version information
        /// </summary>
        public string VersionInfo => $"Version {Assembly.GetExecutingAssembly().GetName().Version}";

        #endregion

        #region Collections

        /// <summary>
        /// Gets the available notification times
        /// </summary>
        public List<string> NotificationTimes => new()
        {
            "1 minute",
            "3 minutes",
            "5 minutes",
            "10 minutes",
            "15 minutes",
            "30 minutes"
        };

        /// <summary>
        /// Gets the available languages
        /// </summary>
        public List<string> Languages => new()
        {
            "English",
            "Arabic"
        };

        /// <summary>
        /// Gets the available calculation methods
        /// </summary>
        public List<string> CalculationMethods => new()
        {
            "Muslim World League",
            "Islamic Society of North America",
            "Egyptian General Authority of Survey",
            "Umm Al-Qura University, Makkah",
            "University of Islamic Sciences, Karachi",
            "Institute of Geophysics, University of Tehran",
            "Shia Ithna-Ashari, Leva Institute, Qum"
        };

        /// <summary>
        /// Gets the available juristic methods
        /// </summary>
        public List<string> JuristicMethods => new()
        {
            "Standard (Shafi, Hanbali, Maliki)",
            "Hanafi"
        };

        /// <summary>
        /// Gets the available time formats
        /// </summary>
        public List<string> TimeFormats => new()
        {
            "12-hour",
            "24-hour"
        };

        /// <summary>
        /// Gets the available themes
        /// </summary>
        public List<string> Themes => new()
        {
            "Light",
            "Dark",
            "System"
        };

        /// <summary>
        /// Gets the available Fajr adhan sounds
        /// </summary>
        public List<string> FajrAdhanSounds => new()
        {
            "Default",
            "Makkah",
            "Madinah"
        };

        /// <summary>
        /// Gets the available regular adhan sounds
        /// </summary>
        public List<string> RegularAdhanSounds => new()
        {
            "Default",
            "Makkah",
            "Madinah"
        };

        #endregion

        #region Commands

        /// <summary>
        /// Command to save settings
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        /// <summary>
        /// Command to cancel settings
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Command to detect location
        /// </summary>
        public ReactiveCommand<Unit, Unit> DetectLocationCommand { get; }

        /// <summary>
        /// Command to open website
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenWebsiteCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes selected values from configuration
        /// </summary>
        private void InitializeSelectedValues()
        {
            try
            {
                // Set notification time
                SelectedNotificationTime = _config.NotificationTime switch
                {
                    1 => "1 minute",
                    3 => "3 minutes",
                    5 => "5 minutes",
                    10 => "10 minutes",
                    15 => "15 minutes",
                    30 => "30 minutes",
                    _ => "5 minutes"
                };

                // Set language
                SelectedLanguage = _config.Language == "ar" ? "Arabic" : "English";

                // Set calculation method
                SelectedCalculationMethod = _config.CalculationMethod switch
                {
                    0 => "Muslim World League",
                    1 => "Islamic Society of North America",
                    2 => "Egyptian General Authority of Survey",
                    3 => "Umm Al-Qura University, Makkah",
                    4 => "University of Islamic Sciences, Karachi",
                    5 => "Institute of Geophysics, University of Tehran",
                    7 => "Shia Ithna-Ashari, Leva Institute, Qum",
                    _ => "Muslim World League"
                };

                // Set juristic method
                SelectedJuristicMethod = _config.JuristicMethod == 1 ? "Hanafi" : "Standard (Shafi, Hanbali, Maliki)";

                // Set time format
                SelectedTimeFormat = _config.TimeFormat == 0 ? "24-hour" : "12-hour";

                // Set theme
                SelectedTheme = _config.Theme switch
                {
                    0 => "Light",
                    1 => "Dark",
                    2 => "System",
                    _ => "Dark"
                };

                // Set adhan sounds
                SelectedFajrAdhan = _config.FajrAdhanSound.Name;
                SelectedRegularAdhan = _config.RegularAdhanSound.Name;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize selected values: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Updates configuration from selected values
        /// </summary>
        private void UpdateConfigFromSelection()
        {
            try
            {
                // Update notification time
                _config.NotificationTime = SelectedNotificationTime switch
                {
                    "1 minute" => 1,
                    "3 minutes" => 3,
                    "5 minutes" => 5,
                    "10 minutes" => 10,
                    "15 minutes" => 15,
                    "30 minutes" => 30,
                    _ => 5
                };

                // Update language
                _config.Language = SelectedLanguage == "Arabic" ? "ar" : "en";

                // Update calculation method
                _config.CalculationMethod = SelectedCalculationMethod switch
                {
                    "Muslim World League" => 0,
                    "Islamic Society of North America" => 1,
                    "Egyptian General Authority of Survey" => 2,
                    "Umm Al-Qura University, Makkah" => 3,
                    "University of Islamic Sciences, Karachi" => 4,
                    "Institute of Geophysics, University of Tehran" => 5,
                    "Shia Ithna-Ashari, Leva Institute, Qum" => 7,
                    _ => 0
                };

                // Update juristic method
                _config.JuristicMethod = SelectedJuristicMethod == "Hanafi" ? 1 : 0;

                // Update time format
                _config.TimeFormat = SelectedTimeFormat == "24-hour" ? 0 : 1;

                // Update theme
                _config.Theme = SelectedTheme switch
                {
                    "Light" => 0,
                    "Dark" => 1,
                    "System" => 2,
                    _ => 1
                };

                // Update adhan sounds (simplified for now)
                _config.FajrAdhanSound = new AdhanSound { Name = SelectedFajrAdhan, FileName = "kholafa_08041446.mp3" };
                _config.RegularAdhanSound = new AdhanSound { Name = SelectedRegularAdhan, FileName = "kholafa_13041446.mp3" };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update config from selection: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Saves the configuration
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Update autostart setting
                var configAdapter = new LinuxConfigurationAdapter();
                configAdapter.ConfigureAutostart(_config.StartWithSystem);

                // Save configuration
                await _configAdapter.SaveConfigurationAsync(_config);
                Log.Information("Settings saved");

                // Close the window
                CloseWindow();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save settings: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Cancels the settings
        /// </summary>
        private void Cancel()
        {
            Log.Information("Settings canceled");
            CloseWindow();
        }

        /// <summary>
        /// Closes the settings window
        /// </summary>
        private void CloseWindow()
        {
            // This will be handled by the view
        }

        /// <summary>
        /// Detects the user's location
        /// </summary>
        private async Task DetectLocationAsync()
        {
            try
            {
                Log.Information("Detecting location");
                // TODO: Implement location detection
                // For now, just show a placeholder
                LocationText = "Detecting location...";
                await Task.Delay(1000); // Simulate network delay
                
                // Update with dummy data for now
                _config.Latitude = 21.3891;
                _config.Longitude = 39.8579;
                _config.City = "Makkah";
                _config.Country = "Saudi Arabia";
                
                LocationText = $"{_config.Latitude}, {_config.Longitude} ({_config.City}, {_config.Country})";
                Log.Information("Location detected: {Location}", LocationText);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to detect location: {Message}", ex.Message);
                LocationText = "Failed to detect location";
            }
        }

        /// <summary>
        /// Opens the project website
        /// </summary>
        private void OpenWebsite()
        {
            try
            {
                string url = "https://github.com/yourusername/awqat-salaat";
                
                // Open URL in default browser
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else
                {
                    // Fallback for other platforms
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                
                Log.Information("Opened website: {Url}", url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open website: {Message}", ex.Message);
            }
        }

        #endregion
    }
}