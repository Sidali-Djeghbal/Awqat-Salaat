#!/bin/bash

# Awqat Salaat Linux Package Builder
# This script automates the process of building Linux packages for Awqat Salaat

set -e

echo "===== Awqat Salaat Linux Package Builder ====="
echo ""

# Check for required tools
check_command() {
    if ! command -v $1 &> /dev/null; then
        echo "Error: $1 is required but not installed."
        exit 1
    fi
}

check_command dotnet

# Create build directory
BUILD_DIR="./build"
mkdir -p "$BUILD_DIR"

echo "Building Awqat Salaat Linux application..."

# Build and publish the application
dotnet publish AwqatSalaat.Linux.csproj -c Release -r linux-x64 --self-contained true -o "$BUILD_DIR/publish/awqat-salaat"

echo "Application built successfully!"

# Create Debian package
if command -v dpkg-deb &> /dev/null; then
    echo "Creating Debian package..."
    
    # Create package structure
    DEB_DIR="$BUILD_DIR/deb-package"
    mkdir -p "$DEB_DIR/DEBIAN"
    mkdir -p "$DEB_DIR/usr/bin"
    mkdir -p "$DEB_DIR/usr/share/awqat-salaat"
    mkdir -p "$DEB_DIR/usr/share/applications"
    mkdir -p "$DEB_DIR/usr/share/doc/awqat-salaat"
    
    # Copy application files
    cp -r "$BUILD_DIR/publish/awqat-salaat"/* "$DEB_DIR/usr/share/awqat-salaat/"
    
    # Create launcher script
    echo '#!/bin/sh' > "$DEB_DIR/usr/bin/awqat-salaat"
    echo 'exec /usr/share/awqat-salaat/AwqatSalaat.Linux' >> "$DEB_DIR/usr/bin/awqat-salaat"
    chmod +x "$DEB_DIR/usr/bin/awqat-salaat"
    
    # Copy desktop entry and documentation
    cp "./debian/awqat-salaat.desktop" "$DEB_DIR/usr/share/applications/"
    cp "./README.md" "$DEB_DIR/usr/share/doc/awqat-salaat/"
    
    # Copy control files
    cp "./debian/control" "$DEB_DIR/DEBIAN/"
    cp "./debian/postinst" "$DEB_DIR/DEBIAN/"
    cp "./debian/prerm" "$DEB_DIR/DEBIAN/"
    chmod 755 "$DEB_DIR/DEBIAN/postinst"
    chmod 755 "$DEB_DIR/DEBIAN/prerm"
    
    # Build the package
    dpkg-deb --build "$DEB_DIR" "$BUILD_DIR/awqat-salaat_1.0.0_amd64.deb"
    
    echo "Debian package created: $BUILD_DIR/awqat-salaat_1.0.0_amd64.deb"
else
    echo "dpkg-deb not found, skipping Debian package creation"
fi

# Create AppImage (if appimagetool is available)
if command -v appimagetool &> /dev/null; then
    echo "Creating AppImage..."
    
    # Create AppDir structure
    APPDIR="$BUILD_DIR/AppDir"
    mkdir -p "$APPDIR/usr/bin"
    mkdir -p "$APPDIR/usr/share/awqat-salaat"
    mkdir -p "$APPDIR/usr/share/applications"
    mkdir -p "$APPDIR/usr/share/icons/hicolor/256x256/apps"
    
    # Copy application files
    cp -r "$BUILD_DIR/publish/awqat-salaat"/* "$APPDIR/usr/share/awqat-salaat/"
    
    # Create AppRun script
    cat > "$APPDIR/AppRun" << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
EXEC="${HERE}/usr/share/awqat-salaat/AwqatSalaat.Linux"
exec "${EXEC}" "$@"
EOF
    chmod +x "$APPDIR/AppRun"
    
    # Copy desktop file and icon
    cp "./debian/awqat-salaat.desktop" "$APPDIR/"
    cp "./Assets/as_ico_linux.ico" "$APPDIR/usr/share/icons/hicolor/256x256/apps/awqat-salaat.png"
    
    # Build the AppImage
    APPIMAGE_EXTRACT_AND_RUN=1 appimagetool "$APPDIR" "$BUILD_DIR/awqat-salaat-1.0.0-x86_64.AppImage"
    
    echo "AppImage created: $BUILD_DIR/awqat-salaat-1.0.0-x86_64.AppImage"
else
    echo "appimagetool not found, skipping AppImage creation"
fi

echo ""
echo "===== Build process completed ====="
echo "Packages can be found in the $BUILD_DIR directory"