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

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```
