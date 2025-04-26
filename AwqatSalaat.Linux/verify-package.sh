#!/bin/bash

# Awqat Salaat Package Verification Script
# This script helps users verify the contents of Awqat Salaat packages

set -e

echo "===== Awqat Salaat Package Verification Tool ====="
echo ""

if [ $# -lt 1 ]; then
    echo "Usage: $0 <package-file>"
    echo "Example: $0 awqat-salaat_1.0.0_amd64.deb"
    exit 1
fi

PACKAGE_FILE="$1"

if [ ! -f "$PACKAGE_FILE" ]; then
    echo "Error: Package file '$PACKAGE_FILE' not found."
    exit 1
fi

# Determine package type
if [[ "$PACKAGE_FILE" == *.deb ]]; then
    echo "Verifying Debian package: $PACKAGE_FILE"
    
    if ! command -v dpkg &> /dev/null; then
        echo "Error: dpkg is required to verify Debian packages."
        exit 1
    fi
    
    echo "\nPackage information:"
    dpkg-deb -I "$PACKAGE_FILE"
    
    echo "\nPackage contents:"
    dpkg-deb -c "$PACKAGE_FILE"
    
elif [[ "$PACKAGE_FILE" == *.rpm ]]; then
    echo "Verifying RPM package: $PACKAGE_FILE"
    
    if ! command -v rpm &> /dev/null; then
        echo "Error: rpm is required to verify RPM packages."
        exit 1
    fi
    
    echo "\nPackage information:"
    rpm -qip "$PACKAGE_FILE"
    
    echo "\nPackage contents:"
    rpm -qlp "$PACKAGE_FILE"
    
elif [[ "$PACKAGE_FILE" == *.AppImage ]]; then
    echo "Verifying AppImage: $PACKAGE_FILE"
    
    echo "\nAppImage information:"
    file "$PACKAGE_FILE"
    
    echo "\nMaking AppImage executable for verification..."
    chmod +x "$PACKAGE_FILE"
    
    echo "\nTo extract and view AppImage contents, run:"
    echo "$PACKAGE_FILE --appimage-extract"
    
else
    echo "Error: Unsupported package format. Supported formats: .deb, .rpm, .AppImage"
    exit 1
fi

echo "\n===== Verification complete ====="
echo "If the package contents look correct, you can proceed with installation."
echo "See INSTALL.md for installation instructions."