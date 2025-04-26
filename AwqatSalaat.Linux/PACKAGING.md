# Packaging Awqat Salaat for Linux

This guide explains how to package the Awqat Salaat Linux application for distribution to users.

## Prerequisites

- .NET 6.0 SDK or later
- `dpkg-deb` for Debian/Ubuntu packages
- `rpmbuild` for RPM packages (Fedora/RHEL/CentOS)

## Building the Application

1. Build the application in Release mode:

```bash
dotnet build AwqatSalaat.Linux/AwqatSalaat.Linux.csproj -c Release
```

2. Publish the application as a self-contained application for Linux:

```bash
dotnet publish AwqatSalaat.Linux/AwqatSalaat.Linux.csproj -c Release -r linux-x64 --self-contained true -o ./publish/awqat-salaat
```

## Creating a Debian (.deb) Package

1. Create the package directory structure:

```bash
mkdir -p ./deb-package/DEBIAN
mkdir -p ./deb-package/usr/bin
mkdir -p ./deb-package/usr/share/awqat-salaat
mkdir -p ./deb-package/usr/share/applications
mkdir -p ./deb-package/usr/share/doc/awqat-salaat
```

2. Copy the application files:

```bash
cp -r ./publish/awqat-salaat/* ./deb-package/usr/share/awqat-salaat/
```

3. Create a launcher script in `./deb-package/usr/bin/awqat-salaat`:

```bash
#!/bin/sh
exec /usr/share/awqat-salaat/AwqatSalaat.Linux
```

4. Make the launcher script executable:

```bash
chmod +x ./deb-package/usr/bin/awqat-salaat
```

5. Copy the desktop entry file:

```bash
cp ./AwqatSalaat.Linux/debian/awqat-salaat.desktop ./deb-package/usr/share/applications/
```

6. Copy documentation:

```bash
cp ./AwqatSalaat.Linux/README.md ./deb-package/usr/share/doc/awqat-salaat/
```

7. Copy the control file and installation scripts:

```bash
cp ./AwqatSalaat.Linux/debian/control ./deb-package/DEBIAN/
cp ./AwqatSalaat.Linux/debian/postinst ./deb-package/DEBIAN/
chmod 755 ./deb-package/DEBIAN/postinst
```

8. Build the Debian package:

```bash
dpkg-deb --build ./deb-package awqat-salaat_1.0.0_amd64.deb
```

## Creating an RPM Package

1. Create an RPM spec file named `awqat-salaat.spec`:

```spec
Name:           awqat-salaat
Version:        1.0.0
Release:        1%{?dist}
Summary:        Islamic prayer times widget for Linux

License:        Proprietary
URL:            https://github.com/Sidali-Djeghbal/Awqat-Salaat
Source0:        %{name}-%{version}.tar.gz

Requires:       libappindicator-gtk3, libnotify, pulseaudio

BuildArch:      x86_64

%description
An Islamic prayer times widget that displays the next prayer time and countdown.
This Linux version is built using Avalonia UI framework and provides the same
core functionality as the Windows version, adapted for Linux desktop environments.

%prep
%setup -q

%install
mkdir -p %{buildroot}/usr/bin
mkdir -p %{buildroot}/usr/share/%{name}
mkdir -p %{buildroot}/usr/share/applications
mkdir -p %{buildroot}/usr/share/doc/%{name}

cp -r ./* %{buildroot}/usr/share/%{name}/

# Create launcher script
echo '#!/bin/sh' > %{buildroot}/usr/bin/%{name}
echo 'exec /usr/share/%{name}/AwqatSalaat.Linux' >> %{buildroot}/usr/bin/%{name}
chmod +x %{buildroot}/usr/bin/%{name}

# Copy desktop file
cp ./awqat-salaat.desktop %{buildroot}/usr/share/applications/

# Copy documentation
cp ./README.md %{buildroot}/usr/share/doc/%{name}/

%files
%attr(755, root, root) /usr/bin/%{name}
/usr/share/%{name}/*
/usr/share/applications/awqat-salaat.desktop
/usr/share/doc/%{name}/*

%post
# Create config directory if it doesn't exist
mkdir -p /home/$SUDO_USER/.config/awqat-salaat

# Update desktop database
if [ -x "$(command -v update-desktop-database)" ]; then
    update-desktop-database -q
fi

%changelog
* Wed May 15 2023 Awqat Salaat Team <email@example.com> - 1.0.0-1
- Initial package
```

2. Create the tarball for the RPM:

```bash
mkdir -p ./rpm-build/awqat-salaat-1.0.0
cp -r ./publish/awqat-salaat/* ./rpm-build/awqat-salaat-1.0.0/
cp ./AwqatSalaat.Linux/debian/awqat-salaat.desktop ./rpm-build/awqat-salaat-1.0.0/
cp ./AwqatSalaat.Linux/README.md ./rpm-build/awqat-salaat-1.0.0/
tar -czf ./rpm-build/awqat-salaat-1.0.0.tar.gz -C ./rpm-build awqat-salaat-1.0.0
```

3. Build the RPM package:

```bash
rpmbuild -ta ./rpm-build/awqat-salaat-1.0.0.tar.gz
```

## AppImage Creation (Alternative)

AppImage is a portable package format that doesn't require installation:

1. Install the AppImage tools:

```bash
sudo apt-get install -y libfuse2 appimagetool
```

2. Create the AppDir structure:

```bash
mkdir -p ./AppDir/usr/bin
mkdir -p ./AppDir/usr/share/awqat-salaat
mkdir -p ./AppDir/usr/share/applications
mkdir -p ./AppDir/usr/share/icons/hicolor/256x256/apps
```

3. Copy the application files:

```bash
cp -r ./publish/awqat-salaat/* ./AppDir/usr/share/awqat-salaat/
```

4. Create the AppRun script:

```bash
cat > ./AppDir/AppRun << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
EXEC="${HERE}/usr/share/awqat-salaat/AwqatSalaat.Linux"
exec "${EXEC}" "$@"
EOF
chmod +x ./AppDir/AppRun
```

5. Copy the desktop file and icon:

```bash
cp ./AwqatSalaat.Linux/debian/awqat-salaat.desktop ./AppDir/
cp ./AwqatSalaat.Linux/Assets/as_ico_linux.ico ./AppDir/usr/share/icons/hicolor/256x256/apps/awqat-salaat.png
```

6. Build the AppImage:

```bash
APPIMAGE_EXTRACT_AND_RUN=1 appimagetool ./AppDir awqat-salaat-1.0.0-x86_64.AppImage
```

## Distribution

After creating the packages, you can distribute them through:

1. GitHub Releases
2. Personal Package Archives (PPA) for Ubuntu
3. Copr for Fedora
4. Your own website or file hosting service

## Installation Instructions for Users

### Debian/Ubuntu (.deb)

```bash
sudo apt install ./awqat-salaat_1.0.0_amd64.deb
```

### Fedora/RHEL/CentOS (.rpm)

```bash
sudo dnf install ./awqat-salaat-1.0.0-1.fc35.x86_64.rpm
```

### AppImage

```bash
chmod +x ./awqat-salaat-1.0.0-x86_64.AppImage
./awqat-salaat-1.0.0-x86_64.AppImage
```

## Troubleshooting

If users encounter issues with the packages, they should check:

1. All dependencies are installed
2. The application has proper permissions
3. Configuration directory exists at ~/.config/awqat-salaat/

For more detailed troubleshooting, users can run the application from the terminal to see any error messages.
