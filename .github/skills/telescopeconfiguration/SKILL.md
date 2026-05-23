---
name: telescopeconfiguration
description: >
  Use this skill for runtime configuration, connection settings, network
  addresses, ports, endpoints, serial parameters, camera stream URLs, or any
  concrete value used to connect the Windows app to the Raspberry Pi.
  Keywords: IP, port, URL, Wi-Fi, SSID, baud rate, serial, HTTP, endpoint,
  stream, camera list, configuration, default.
---

# Telescope Configuration - Runtime Reference

All concrete configuration values used by the Windows TelescopeWatcher app.

---

## 1 - Network / Wi-Fi

| Setting | Value |
|---|---|
| Pi Wi-Fi SSID | RaspberryPiCam |
| Default Pi IP (Wi-Fi hotspot) | 192.168.4.1 |
| Default server URL | http://192.168.4.1:5000 |
| Connection check endpoint | GET /motor/read |

Direct Ethernet cable is also supported. Enter the Pi Ethernet IP manually.
Port 5000 is appended automatically if omitted.

---

## 2 - HTTP API Endpoints (Pi HTTP Server, port 5000)

| Endpoint | Purpose |
|---|---|
| GET /motor/write?cmd=encoded | Send motor or focus command |
| GET /motor/read | Read from Arduino; used as health check |
| GET /cam/list | List available cameras on the Pi |
| GET /cam/start?camera=name | Start a camera stream |
| GET /cam/stop?camera=name | Stop a camera stream |
| GET /restart | Restart the Pi HTTP server |

Camera start JSON response: { scheme, streamPort, streamPath }
Full MJPEG URL: {scheme}://{host}:{streamPort}{streamPath}

---

## 3 - Serial Port (direct USB mode)

| Parameter | Value |
|---|---|
| Baud rate | 115200 |
| Data bits | 8 |
| Parity | None |
| Stop bits | 1 |
| Handshake | None |
| Read timeout | 500 ms |
| Write timeout | 500 ms |

Serial mode is legacy. Default mode on startup is HTTP server mode.

---

## 4 - HTTP Client Timeouts

| Client | Timeout |
|---|---|
| SerialServerClient motor commands | 500 ms (fire-and-forget) |
| SerialServerClient overall HttpClient | 5 s |
| Video HTTP client | 2 s |
| Connection test | 10 s |

---

## 5 - Speed and Steps Configuration

| Steps/sec | TimeBetweenSteps | Wire value |
|---|---|---|
| 3 | 333 ms | t=333 |
| 10 | 100 ms | t=100 |
| 100 | 10 ms | t=10 |
| 1000 | 1 ms | t=1 |
| 10000 | 0 (special) | t=0.1 |
| 100000 | -1 (special) | t=0.01 |

Default speed on startup: 100 steps/sec. Default focus speed: 9.

---

## 6 - Camera Settings JSON Presets

Location: Camera_settings/ folder.
Filename pattern: camera-model condition.json

| File | Camera | Condition |
|---|---|---|
| Arducam_imx291_night.json | IMX291 | Night |
| HD camera dawn.json | HD cam | Dawn |
| HD camera day.json | HD cam | Day |
| HD camera night can control gain.json | HD cam | Night (gain ctrl) |
| HD camera without ir cut.json | HD cam | No IR-cut filter |
| uc60 dawn.json | UC60 | Dawn |
| uc60 day.json | UC60 | Day |
| uc60 night.json | UC60 | Night |

---

## 7 - Two Connection Modes

Server mode (default):
- Enter Pi IP in the UI text field (default 192.168.4.1)
- Port 5000 appended automatically if not specified
- SerialServerClient handles motor commands over HTTP
- Camera streaming available via /cam/start endpoint

Serial mode (legacy):
- Select COM port from the list
- 115200 baud, connects directly to Arduino Nano over USB
- No camera streaming in this mode

