# Awqat Salaat - Linux Version Plan

## Overview

This document outlines the plan for creating a Linux version of the Awqat Salaat prayer times widget. The Linux version will maintain the core functionality of the Windows version while adapting the UI and system integration components to work with Linux desktop environments.

## Architecture Analysis

### Current Architecture (Windows)

The current Awqat Salaat application consists of several key components:

1. **Core Logic (AwqatSalaat.Common)**

   - Prayer time calculation services (SalahHour, AlAdhan, Local)
   - Data models for prayer times
   - Configuration management
   - Localization support

2. **UI Components**

   - Windows-specific UI implementations (WinUI, Deskband)
   - Taskbar integration
   - Settings panels and dialogs

3. **System Integration**
   - Windows taskbar integration
   - Windows registry for settings
   - Windows-specific APIs for UI rendering

### Proposed Architecture (Linux)

1. **Core Logic (to be reused)**

   - Prayer time calculation services
   - Data models
   - Configuration management (adapted for Linux)
   - Localization support

2. **UI Components (to be replaced)**

   - Cross-platform UI framework (Avalonia UI)
   - System tray integration for Linux
   - Linux-compatible settings dialogs

3. **System Integration (to be replaced)**
   - Linux system tray integration using libappindicator
   - Configuration storage using standard Linux approaches (e.g., ~/.config/)
   - Linux-specific APIs for UI rendering

## Implementation Plan

### Phase 1: Project Setup and Core Logic Migration

1. **Create new project structure**

   - Set up Avalonia UI project
   - Configure .NET Core targeting Linux
   - Set up CI/CD for Linux builds

2. **Migrate core logic**
   - Port AwqatSalaat.Common to be platform-agnostic
   - Remove Windows-specific dependencies
   - Adapt configuration storage for Linux

### Phase 2: UI Implementation

1. **Implement main widget UI**

   - Create Avalonia UI version of the widget
   - Implement compact and regular display modes
   - Support RTL layouts for Arabic

2. **Implement settings UI**
   - Create settings dialog with all existing options
   - Implement prayer time configuration
   - Support calendar and adhan features

### Phase 3: System Integration

1. **Implement system tray integration**

   - Use libappindicator for system tray support
   - Implement context menu functionality
   - Handle notifications for prayer times

2. **Implement Linux-specific features**
   - Autostart configuration
   - Desktop environment integration
   - Notification system integration

### Phase 4: Testing and Packaging

1. **Testing**

   - Test on major Linux distributions (Ubuntu, Fedora, etc.)
   - Test with different desktop environments (GNOME, KDE, etc.)
   - Verify prayer time calculations

2. **Packaging**
   - Create .deb packages for Debian-based distributions
   - Create .rpm packages for Red Hat-based distributions
   - Create AppImage for universal distribution
   - Consider Flatpak and Snap packages

## Technical Decisions

### UI Framework: Avalonia UI

Avalonia UI is recommended for the Linux version because:

1. It's a cross-platform .NET UI framework that works well on Linux
2. It has a XAML-based approach similar to WPF/WinUI, making migration easier
3. It supports .NET Core and can be used with the existing C# codebase
4. It has good support for custom styling and theming

### System Tray Integration: libappindicator

libappindicator is recommended for system tray integration because:

1. It's widely supported across Linux distributions
2. It provides a consistent API for system tray icons
3. It supports context menus and notifications
4. It works with most desktop environments (GNOME, KDE, XFCE, etc.)

### Configuration Storage

For configuration storage on Linux:

1. Use standard Linux configuration locations (~/.config/awqat-salaat/)
2. Store settings in JSON or XML format
3. Support migration of settings from Windows version if detected

## Compatibility Considerations

1. **Desktop Environments**

   - Ensure compatibility with major desktop environments (GNOME, KDE, XFCE, etc.)
   - Handle different system tray implementations
   - Adapt to different theme engines

2. **Distributions**
   - Target Ubuntu LTS as primary platform
   - Ensure compatibility with other major distributions
   - Document specific requirements for different distributions

## Version Tracking

The Linux version will follow the same version numbering as the Windows version, with an additional identifier for the Linux platform (e.g., v4.0.1-linux).

## Timeline

- Phase 1: 4 weeks
- Phase 2: 6 weeks
- Phase 3: 4 weeks
- Phase 4: 2 weeks

Total estimated time: 16 weeks

## Resources Required

1. Development environment with Linux (Ubuntu recommended)
2. Knowledge of Avalonia UI and .NET on Linux
3. Understanding of Linux desktop integration
4. Testing environments for various Linux distributions

## Conclusion

Creating a Linux version of Awqat Salaat is feasible by leveraging the existing core logic while replacing the Windows-specific UI and system integration components. The use of Avalonia UI and libappindicator will provide a solid foundation for a Linux-compatible application that maintains the functionality and user experience of the Windows version.
