# Telescope Watcher - Windows Side

A Windows Forms application for managing serial COM port connections to telescope equipment.

## Features

- **Automatic COM Port Detection**: Scans and lists all available serial COM ports on your PC
- **Easy Connection Management**: Connect and disconnect from serial ports with a single click
- **Real-time Status Updates**: Monitor connection status and received data in real-time
- **Data Reception**: Displays data received from connected serial devices

## Requirements

- .NET 8.0 or later
- Windows operating system
- Visual Studio 2022 (recommended) or Visual Studio Code

## Project Structure

This is a proper Visual Studio solution with the following structure:

- **TelescopeWatcher.sln** - Visual Studio solution file
- **TelescopeWatcher.csproj** - WinForms project file
- All source code files (.cs)

## Opening the Project

### Using Visual Studio

1. Double-click `TelescopeWatcher.sln` or
2. Open Visual Studio ? File ? Open ? Project/Solution ? Select `TelescopeWatcher.sln`

### Using Visual Studio Code

1. Open the folder in VS Code
2. The C# extension will automatically detect the solution

## Building

### From Visual Studio

- Press `F6` or use Build ? Build Solution

### From Command Line

  ```bash
  dotnet restore TelescopeWatcher.sln
  dotnet build TelescopeWatcher.sln
  ```

## Running

### From Visual Studio

- Press `F5` to run with debugging or `Ctrl+F5` to run without debugging

### From Command Line

  ```bash
  dotnet run --project TelescopeWatcher.csproj
  ```

## Usage

1. Launch the application
2. The list will automatically populate with available COM ports
3. Select a COM port from the list
4. Click "Connect" to establish a connection
5. Click "Disconnect" to close the connection
6. Use "Refresh Ports" to rescan for available ports

## Default Serial Port Settings

- Baud Rate: 9600
- Data Bits: 8
- Parity: None
- Stop Bits: 1
- Handshake: None

## Dependencies

- System.IO.Ports (8.0.0) - For serial port communication
