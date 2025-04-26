using AwqatSalaat.Helpers;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AwqatSalaat.Linux.Services
{
    /// <summary>
    /// Provides Linux-specific configuration storage and retrieval
    /// </summary>
    public class LinuxConfigurationAdapter
    {
        private const string ConfigFileName = "settings.json";
        private readonly string _configFilePath;

        public LinuxConfigurationAdapter()
        {
            // Use standard Linux configuration location
            string configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "awqat-salaat");

            Directory.CreateDirectory(configDir);
            _configFilePath = Path.Combine(configDir, ConfigFileName);

            Log.Information("Configuration file path: {ConfigPath}", _configFilePath);
        }

        /// <summary>
        /// Loads configuration from the Linux config file
        /// </summary>
        public T? LoadConfiguration<T>() where T : class, new()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Log.Information("Configuration file does not exist, creating default");
                    return new T();
                }

                string json = File.ReadAllText(_configFilePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load configuration: {Message}", ex.Message);
                return new T();
            }
        }

        /// <summary>
        /// Saves configuration to the Linux config file
        /// </summary>
        public async Task SaveConfigurationAsync<T>(T configuration) where T : class
        {
            try
            {
                string json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                await File.WriteAllTextAsync(_configFilePath, json);
                Log.Information("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save configuration: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Creates a desktop entry file for autostart
        /// </summary>
        public void ConfigureAutostart(bool enable)
        {
            try
            {
                string autostartDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config",
                    "autostart");

                string desktopEntryPath = Path.Combine(autostartDir, "awqat-salaat.desktop");

                if (enable)
                {
                    Directory.CreateDirectory(autostartDir);

                    // Get the executable path
                    string execPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    execPath = execPath.Replace(".dll", ""); // Fix for .NET Core apps

                    // Create desktop entry content
                    string desktopEntry = 
                        "[Desktop Entry]\n" +
                        "Type=Application\n" +
                        "Name=Awqat Salaat\n" +
                        "Comment=Islamic prayer times widget\n" +
                        $"Exec={execPath}\n" +
                        "Terminal=false\n" +
                        "Categories=Utility;\n" +
                        "StartupNotify=false\n";

                    File.WriteAllText(desktopEntryPath, desktopEntry);
                    Log.Information("Created autostart entry at {Path}", desktopEntryPath);
                }
                else if (File.Exists(desktopEntryPath))
                {
                    File.Delete(desktopEntryPath);
                    Log.Information("Removed autostart entry from {Path}", desktopEntryPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to configure autostart: {Message}", ex.Message);
            }
        }
    }
}