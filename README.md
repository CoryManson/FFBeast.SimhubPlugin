# FFBeast Wheelbase Control for SimHub

A SimHub plugin that allows you to control your FFBeast wheelbase directly from SimHub, enabling you to bind controller buttons or keyboard keys to wheelbase functions like recenter.

> **Note**: This plugin is designed specifically for the FFBeast direct drive wheelbase and requires the device to be connected via USB.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](https://dotnet.microsoft.com/)
[![SimHub](https://img.shields.io/badge/SimHub-Compatible-green.svg)](https://www.simhubdash.com/)

## Features

- **Reset Center**: Recalibrate the wheelbase center position with a button press
- **Reboot Controller**: Restart the wheelbase controller
- **Auto-reconnect**: Automatically reconnects to the wheelbase if connection is lost
- **SimHub Integration**: Full integration with SimHub's control mapping system
- **Settings UI**: Test actions and view connection status directly in SimHub settings

## Prerequisites

- Windows PC with SimHub installed
- FFBeast wheelbase connected via USB
- .NET 9 SDK or later (for building from source)
- Visual Studio 2022 or VS Code (optional, can build with dotnet CLI)

## Installation

### Option 1: Using Pre-built DLL (Recommended)

1. Go to the [Releases](https://github.com/CoryManson/FFBeast.SimhubPlugin/releases) page
2. Download the latest `FFBeast.SimHubPlugin-vX.X.X.zip` file
3. Extract all files from the ZIP archive
4. Copy all DLL files to your SimHub installation directory:
   ```
   C:\Program Files (x86)\SimHub\
   ```
5. Restart SimHub
6. The plugin should appear in the SimHub settings under **Additional Plugins**

### Option 2: Build from Source

1. **Prerequisites**:
   - .NET 9 SDK or later
   - SimHub installed (default: `C:\Program Files (x86)\SimHub\`)

2. **Clone the Repository**:
   ```powershell
   git clone https://github.com/CoryManson/FFBeast.SimhubPlugin.git
   cd FFBeast.SimhubPlugin
   ```

3. **Set Environment Variable** (Optional):
   - If SimHub is installed in a non-default location, set `SIMHUB_INSTALL_PATH`:
     ```powershell
     [System.Environment]::SetEnvironmentVariable('SIMHUB_INSTALL_PATH', 'C:\Your\SimHub\Path\', 'User')
     ```

4. **Build the Project**:
   ```powershell
   dotnet build FFBeast.SimHubPlugin.csproj -c Release
   ```

5. **Copy to SimHub**:
   ```powershell
   Copy-Item "bin\Release\net48\FFBeast.SimHubPlugin*" "C:\Program Files (x86)\SimHub\" -Force
   ```

6. **Restart SimHub**:
   - Close and restart SimHub to load the plugin

## Usage

### Setting up Button Bindings in SimHub

1. **Open SimHub** and navigate to **Controls and Events**

2. **Go to Custom Serial Controls** (or Controls tab)

3. **Add a New Action**:
   - Click "Add" or "+" button
   - Look for actions starting with "FFBeast."

4. **Available Actions**:
   - `FFBeast.ResetCenter` - Reset wheelbase center position
   - `FFBeast.Reboot` - Reboot the wheelbase controller

5. **Map to Your Controller**:
   - Select the action (e.g., `FFBeast.ResetCenter`)
   - Click "Map" or "Bind"
   - Press the button on your controller or keyboard key you want to use
   - Save the mapping

### Example: Binding Recenter to a Button

1. In SimHub, go to **Controls and Events** → **Custom Serial controls**
2. Click **Add action**
3. Find and select **FFBeast.ResetCenter**
4. Click the **Map** button
5. Press your desired button (e.g., a button on your wheel or controller)
6. The button is now bound - press it anytime to recenter your wheelbase!

### Plugin Settings

Access the plugin settings in SimHub:
1. Go to **Additional plugins** or **Plugins** tab
2. Find **FFBeast Wheelbase Control**
3. View connection status and device name
4. Use **Quick Test** buttons to test actions:
   - **Test Recenter** - Immediately recenter the wheelbase
   - **Test Reboot** - Reboot the wheelbase (with confirmation dialog)

## Troubleshooting

### Plugin Not Showing Up

- Verify all DLLs are copied to the SimHub directory
- Check SimHub logs for any errors
- Ensure .NET Framework 4.8 is installed
- Try restarting SimHub

### Cannot Connect to Wheelbase

- Make sure the FFBeast wheelbase is connected via USB
- Check Windows Device Manager for the device (VID: 1115, PID: 22999)
- Try unplugging and reconnecting the wheelbase
- The plugin auto-reconnects every 5 seconds if disconnected

### Button Mapping Not Working

- Verify the action is properly mapped in SimHub Controls
- Check that the plugin shows "Connected" status in green
- Test the action using the Quick Test buttons first
- Try remapping the button
- Check SimHub logs for any error messages

## Development

### Project Structure

```
simhub-wheel-api/
├── FFBeast.SimHubPlugin.csproj   # Modern SDK-style project file
├── FFBeastPlugin.cs              # Main plugin class (IPlugin, IDataPlugin, IWPFSettings)
├── FFBeastWheelApiClient.cs      # HID communication layer using HidSharp
├── SettingsControl.xaml          # WPF settings UI
├── SettingsControl.xaml.cs       # Settings UI code-behind
├── build.ps1                     # PowerShell build script
├── Properties/                   # Assembly metadata and resources
│   ├── AssemblyInfo.cs
│   ├── Resources.resx
│   └── Resources.Designer.cs
├── README.md                     # This file
├── QUICKSTART.md                 # Quick start guide
└── BUILD.md                      # Detailed build instructions
```

### HID Communication Protocol

The plugin communicates with the FFBeast wheelbase using 65-byte HID reports:

```csharp
// Report structure (65 bytes: Report ID + 64 bytes data)
byte[] buffer = new byte[65];
buffer[0] = REPORT_GENERIC_INPUT_OUTPUT;  // 0xA3 (Report ID)
buffer[1] = COMMAND;                      // Command byte
// Remaining bytes are padding (zeros)
```

**Supported Commands**:
- `0x04` - Reset center position (recenter the wheelbase)
- `0x01` - Reboot controller (restart the wheelbase)

**Device Selection**:
- VID: 1115 (0x045B), PID: 22999 (0x59D7)
- Filters by `MaxOutputReportLength >= 65` to select the vendor interface (MI_00)
- Avoids the joystick interface (MI_01) which has 18-byte reports

### Building for Distribution

1. Build in Release mode:
   ```powershell
   dotnet build FFBeast.SimHubPlugin.csproj -c Release
   ```

2. Files to include in distribution (from `bin/Release/net48/`):
   - `FFBeast.SimHubPlugin.dll` - Main plugin assembly
   - `FFBeast.SimHubPlugin.pdb` - Debug symbols (optional)
   - `HidSharp.dll` - HID communication library

### Creating a Release

To create a release for distribution:

1. **Build Release configuration**:
   ```powershell
   dotnet build FFBeast.SimHubPlugin.csproj -c Release
   ```

2. **Package the files**:
   ```powershell
   # Create a release folder
   New-Item -ItemType Directory -Force -Path release
   
   # Copy required files
   Copy-Item "bin\Release\net48\FFBeast.SimHubPlugin.dll" release\
   Copy-Item "bin\Release\net48\HidSharp.dll" release\
   Copy-Item "README.md" release\
   
   # Create ZIP archive
   Compress-Archive -Path release\* -DestinationPath FFBeast.SimHubPlugin-v1.0.0.zip
   ```

3. **Create GitHub Release**:
   - Go to your repository on GitHub
   - Click "Releases" → "Create a new release"
   - Tag version (e.g., `v1.0.0`)
   - Upload the ZIP file
   - Add release notes

## Technical Details

### Dependencies

- **HidSharp 2.1.0** - Cross-platform HID library for USB communication
- **SimHub SDK DLLs** - GameReaderCommon, SimHub.Plugins, SimHub.Logging, log4net
- **Target Framework**: .NET Framework 4.8 (SimHub compatibility)
- **Build SDK**: .NET 9 SDK (modern SDK-style project format)

### FFBeast Wheelbase Specifications

- **Vendor ID**: 1115 (0x045B)
- **Product ID**: 22999 (0x59D7)
- **HID Interface**: Vendor-specific interface (MI_00)
- **Report Length**: 65 bytes (Report ID + 64 bytes data)
- **Connection**: USB HID device

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

### Development Setup

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test with your FFBeast wheelbase and SimHub
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### References

- [FFBeast Wheel API Library](https://github.com/pelukkk/pelukkk.github.io/tree/main/ffbeast-wheel-api-lib) - Original C/C++ API implementation
- [SimHub](https://www.simhubdash.com/) - Racing dashboard and control platform

## License

This project is provided as-is for the FFBeast community.

## Support

For issues or questions:
- Check SimHub logs for error messages
- Refer to FFBeast documentation
- Open an issue on the project repository

## Version History

### 1.0.0 (2025-11-03)
- Initial release
- SimHub plugin with action mapping support
- Reset Center and Reboot controller actions
- Auto-reconnect functionality (5-second interval)
- WPF settings UI with connection status and test buttons
- Modern SDK-style project compatible with .NET 9 SDK
