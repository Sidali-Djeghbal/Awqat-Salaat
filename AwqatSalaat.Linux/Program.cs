using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using AwqatSalaat.Helpers;
using AwqatSalaat.Linux.Services;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace AwqatSalaat.Linux
{
    internal class Program
    {
        private static Mutex? appMutex;

        // Initialization code for the application.
        [STAThread]
        public static void Main(string[] args)
        {
            InitLogger();
            ExitIfOtherInstanceIsRunning();

            try
            {
                // Build Avalonia app
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed: {Message}", ex.Message);
                ShowError($"Unhandled Exception:\n{ex}");
                Environment.Exit(1);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

        private static void InitLogger()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AwqatSalaat",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logDirectory, "log_.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application starting");
        }

        private static void ExitIfOtherInstanceIsRunning()
        {
            const string mutexId = @"C790179C-7492-4CCE-B377-5F48F394B2CB";

            appMutex = new Mutex(true, mutexId, out bool created);

            if (!created)
            {
                Log.Information("Widget is already running");
                ShowError("Awqat Salaat is already running.");
                Environment.Exit(1);
            }
        }

        private static void ShowError(string message)
        {
            // In a real implementation, this would show a dialog
            // For now, just log the error
            Log.Error(message);
            Console.Error.WriteLine(message);
        }
    }
}