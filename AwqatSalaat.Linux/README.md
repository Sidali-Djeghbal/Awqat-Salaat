# Awqat Salaat - Linux Version

## Overview

Awqat Salaat is an Islamic prayer times widget that displays the next prayer time and countdown. This Linux version is built using Avalonia UI framework and provides the same core functionality as the Windows version, adapted for Linux desktop environments.

## Features

- Display next prayer time with countdown
- System tray integration using AppIndicator
- Configurable prayer time calculation methods
- Notifications for prayer times
- Adhan (call to prayer) support
- Autostart capability
- Multi-language support (English and Arabic)

## Technical Implementation

### Architecture

The Linux version of Awqat Salaat follows a similar architecture to the Windows version, with platform-specific adaptations:

1. **Core Logic**: Reuses the prayer time calculation services from AwqatSalaat.Common
2. **UI Components**: Implemented using Avalonia UI for cross-platform compatibility
3. **System Integration**: Uses Linux-specific APIs for system tray, notifications, and configuration

### Key Components

- **LinuxConfigurationAdapter**: Handles configuration storage in ~/.config/awqat-salaat/
- **SystemTrayManager**: Manages the system tray icon and menu using AppIndicator
- **NotificationService**: Handles system notifications for prayer times
- **WidgetViewModel**: Manages the prayer time data and UI updates
- **SettingsViewModel**: Handles configuration settings

## Installation

### Prerequisites

- .NET 6.0 or later
- libappindicator3 (for system tray support)
- notify-send (for notifications)
- pulseaudio (for adhan playback)

### Installation Steps

1. Install the required dependencies:

```bash
# For Ubuntu/Debian
sudo apt install libappindicator3-dev libnotify-bin pulseaudio

# For Fedora
sudo dnf install libappindicator-gtk3-devel libnotify pulseaudio
```

2. Download the latest release package
3. Extract the package to a location of your choice
4. Run the application:

```bash
./AwqatSalaat.Linux
```

5. To enable autostart, use the settings dialog or manually create a desktop entry in ~/.config/autostart/

## Building from Source

1. Clone the repository
2. Install .NET 6.0 SDK
3. Build the project:

```bash
dotnet build AwqatSalaat.Linux/AwqatSalaat.Linux.csproj -c Release
```

4. Run the application:

```bash
dotnet run --project AwqatSalaat.Linux/AwqatSalaat.Linux.csproj
```

## Distribution Packages

Awqat Salaat can be packaged in various Linux distribution formats for easy installation:

### Available Package Formats

- **Debian (.deb)** - For Ubuntu, Debian, and derivatives
- **RPM (.rpm)** - For Fedora, RHEL, CentOS, and derivatives
- **AppImage** - Portable format that works across distributions

### Creating Distribution Packages

A build script is provided to automate the packaging process:

```bash
chmod +x ./build-package.sh
./build-package.sh
```

This will create packages in the `build` directory.

For detailed instructions on creating packages manually, see the [PACKAGING.md](PACKAGING.md) file.

### Installation

For installation instructions, see the [INSTALL.md](INSTALL.md) file.

## License

This project is licensed under the same terms as the main Awqat Salaat project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
