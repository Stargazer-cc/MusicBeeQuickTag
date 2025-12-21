# MusicBeeQuickTag

[![Version](https://img.shields.io/badge/version-v2.0-blue.svg)](https://github.com/Stargazer-cc/MusicBeeQuickTag/releases)
[![MusicBee](https://img.shields.io/badge/MusicBee-3.0%2B-orange.svg)](https://getmusicbee.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**[中文文档](README_CN.md) | English**

A MusicBee plugin for quickly browsing and applying existing tag values from your music library.

---

## ✨ Features

- **Dynamic Field Scanning**: Automatically scans all available tag fields without manual configuration
- **Custom Display Order**: Adjust field display order through a dual-list interface
- **Column-based Layout**: All fields displayed side-by-side in one interface for easy viewing
- **Real-time Track Monitoring**: Automatically detects track selection changes in MusicBee
- **One-click Application**: Double-click any tag value to apply it to selected tracks

---

## 📦 Installation

1. Download the `mb_MusicBeeQuickTag.dll` file
2. Open MusicBee Preferences → Plugins tab → Click "Add Plugin" → Select the downloaded `mb_MusicBeeQuickTag.dll` file

---

## 🚀 Usage

### 1. Configure Fields
Before using the tool, select which tag fields you want to manage:
1. Go to `Edit` → `Preferences` → `Plugins`.
2. Find **MusicBeeQuickTag** and click `Configure`.
3. In the settings window:
   - **Left Panel**: Shows all available fields found in your library.
   - **Right Panel**: Shows the fields currently selected for display.
   - Use the `>>` and `<<` buttons to add or remove fields.
   - Use `Move Up` and `Move Down` to arrange the display order (top-to-bottom in settings = left-to-right in the tool).
4. Click `Save` to apply changes.

### 2. Browse and Apply Tags
1. **Open the Tool**: Go to `Tools` → `MusicBeeQuickTag`. The window will open and scan your library for existing tag values.
2. **Select Tracks**: In the main MusicBee window, select the music files you want to tag.
   - The **Status Bar** at the top of the plugin window will update to show the number of selected files (e.g., "3 Files Selected") or the track details if only one is selected.
3. **Apply a Value**:
   - Browse the columns to find the tag value you want to apply.
   - **Double-click** the value to **replace** the existing tag.
   - **Shift + Double-click** the value to **append** it to the existing tag (useful for multi-value fields like Genre or Artist).
   - The plugin will instantly update all selected tracks.
   - A confirmation message will appear in the status bar.

---

## 📝 Version History

**v2.0** (2025-11-28)
- Dynamic scanning of all available fields
- Support for custom field display order
- Optimized scanning performance (batch API)
- Refactored settings interface

**v1.x**
- Initial implementation

---

## 📄 License

[MIT License](LICENSE)
