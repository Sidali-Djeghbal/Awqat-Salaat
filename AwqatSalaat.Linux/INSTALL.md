# Awqat Salaat Linux - Installation Guide

This guide explains how to install the Awqat Salaat Linux application from the provided packages.

## Installation from Debian Package (.deb)

For Ubuntu, Debian, and other Debian-based distributions:

1. Ensure you have the required dependencies:

```bash
sudo apt install libappindicator3-1 libnotify-bin pulseaudio
```

2. Install the package:

```bash
sudo apt install ./awqat-salaat_1.0.0_amd64.deb
```

3. Launch the application:
   - From the application menu: Search for "Awqat Salaat"
   - From the terminal: Run `awqat-salaat`

## Installation from RPM Package (.rpm)

For Fedora, RHEL, CentOS, and other RPM-based distributions:

1. Ensure you have the required dependencies:

```bash
sudo dnf install libappindicator-gtk3 libnotify pulseaudio
```

2. Install the package:

```bash
sudo dnf install ./awqat-salaat-1.0.0-1.fc35.x86_64.rpm
```

3. Launch the application:
   - From the application menu: Search for "Awqat Salaat"
   - From the terminal: Run `awqat-salaat`

## Using the AppImage

AppImage is a portable format that doesn't require installation:

1. Make the AppImage executable:

```bash
chmod +x ./awqat-salaat-1.0.0-x86_64.AppImage
```

2. Run the application:

```bash
./awqat-salaat-1.0.0-x86_64.AppImage
```

3. (Optional) Integrate with your desktop:
   - Right-click on the AppImage and select "Properties"
   - Go to "Permissions" tab and check "Allow executing file as program"
   - You can also use tools like AppImageLauncher for better desktop integration

## Configuration

After installation, the application will create a configuration directory at `~/.config/awqat-salaat/`.

### Enabling Autostart

To make Awqat Salaat start automatically when you log in:

1. Open the application
2. Go to Settings
3. Enable the "Start with system" option

Alternatively, you can manually create a desktop entry in `~/.config/autostart/`:

```bash
cp /usr/share/applications/awqat-salaat.desktop ~/.config/autostart/
```

## Troubleshooting

If you encounter issues with the application:

1. **Missing dependencies**: Ensure all required dependencies are installed
2. **Permission issues**: Check if the application has proper permissions
3. **Configuration problems**: Try removing the configuration directory and restarting the application:

```bash
rm -rf ~/.config/awqat-salaat
```

4. **Debug mode**: Run the application from the terminal to see error messages:

```bash
awqat-salaat
```

## Uninstallation

### Debian-based systems:

```bash
sudo apt remove awqat-salaat
```

### RPM-based systems:

```bash
sudo dnf remove awqat-salaat
```

### AppImage:

Simply delete the AppImage file. If you want to remove configuration files as well:

```bash
rm -rf ~/.config/awqat-salaat
```
