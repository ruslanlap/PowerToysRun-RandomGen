#!/bin/bash

set -e

rm -rf ./RandomGen/Publish
rm -rf ./RandomGen/Community.PowerToys.Run.Plugin.RandomGen/obj
rm -rf ./RandomGen-x64.zip
rm -rf ./RandomGen-ARM64.zip
rm -rf ./RandomGen/Community.PowerToys.Run.Plugin.RandomGen/bin

PROJECT_PATH="RandomGen/Community.PowerToys.Run.Plugin.RandomGen/Community.PowerToys.Run.Plugin.RandomGen.csproj"
OUT_ROOT="./RandomGen/Community.PowerToys.Run.Plugin.RandomGen/bin"
DEST_DIR="./RandomGen/Publish"

echo "🛠️  Building for x64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-x64

echo "🛠️  Building for ARM64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-arm64

echo "📂 Copying published files to $DEST_DIR..."
rm -rf "$DEST_DIR"
mkdir -p "$DEST_DIR"

# ⚠️ Зверни увагу на цю зміну
PUBLISH_X64=$(find "$OUT_ROOT" -type d -path '*win-x64/publish' | head -n 1)

echo "ℹ️  Using publish folder: $PUBLISH_X64"
cp -r "$PUBLISH_X64"/* "$DEST_DIR"

echo "📦 Zipping results..."
ZIP_X64="./RandomGen-x64.zip"
zip -r "$ZIP_X64" "$DEST_DIR"/*

echo "✅ Done! Created:"
echo " - $ZIP_X64"
