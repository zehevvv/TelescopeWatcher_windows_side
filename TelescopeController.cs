using System.IO.Ports;

namespace TelescopeWatcher
{
    public class TelescopeController
    {
        private readonly SerialPort? serialPort;
        private readonly SerialServerClient? serverClient;
        private readonly bool isServerMode;
        private readonly Action<string>? logCallback;

        public int TimeBetweenSteps { get; set; } = 10;
        public int FocusSpeed { get; set; } = 9;

        public TelescopeController(SerialPort? port, SerialServerClient? client, Action<string>? logCallback)
        {
            this.serialPort = port;
            this.serverClient = client;
            this.isServerMode = (client != null);
            this.logCallback = logCallback;
        }

        public void SendMoveCommand(string direction)
        {
            if (!IsConnected())
            {
                LogMessage("Error: Connection not available!");
                return;
            }

            try
            {
                string motorCommand;
                string directionCommand;

                if (direction == "UP")
                {
                    motorCommand = "v=1";
                    directionCommand = "d=1";
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                }
                else if (direction == "DOWN")
                {
                    motorCommand = "v=1";
                    directionCommand = "d=0";
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                }
                else if (direction == "LEFT")
                {
                    motorCommand = "v=0";
                    directionCommand = "d=0";
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                }
                else // RIGHT
                {
                    motorCommand = "v=0";
                    directionCommand = "d=1";
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                }

                WriteCommand(directionCommand);
                Thread.Sleep(50);

                string timeCommand = TimeBetweenSteps == -1 ? "t=0.01" : (TimeBetweenSteps == 0 ? "t=0.1" : $"t={TimeBetweenSteps}");
                WriteCommand(timeCommand);
                Thread.Sleep(50);

                WriteCommand("s=10000");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending command: {ex.Message}");
            }
        }

        public void SendStepsCommand()
        {
            if (!IsConnected()) return;

            try
            {
                WriteCommand("s=10000");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending steps command: {ex.Message}");
            }
        }

        public void SendStopCommand()
        {
            if (!IsConnected()) return;

            try
            {
                WriteCommand("s=0");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending stop command: {ex.Message}");
            }
        }

        public void SendFocusCommand(string direction)
        {
            if (!IsConnected())
            {
                LogMessage("Error: Connection not available!");
                return;
            }

            try
            {
                WriteCommand($"b={FocusSpeed}");
                Thread.Sleep(50);

                string directionCommand = direction == "INCREASE" ? "a=1" : "a=0";
                WriteCommand(directionCommand);
                Thread.Sleep(50);

                WriteCommand("c=100");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending focus command: {ex.Message}");
            }
        }

        public void SendFocusStepsCommand()
        {
            if (!IsConnected()) return;

            try
            {
                WriteCommand("c=100");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending focus steps command: {ex.Message}");
            }
        }

        public void SendFocusStopCommand()
        {
            if (!IsConnected()) return;

            try
            {
                WriteCommand("c=0");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending focus stop command: {ex.Message}");
            }
        }

        /// <summary>Sends a raw command string directly to the connected port or server client.</summary>
        public void SendRawCommand(string command)
        {
            if (!IsConnected()) return;
            try
            {
                WriteCommand(command);
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending raw command: {ex.Message}");
            }
        }

        private bool IsConnected()
        {
            if (isServerMode)
            {
                return serverClient != null && serverClient.IsConnected();
            }
            return serialPort != null && serialPort.IsOpen;
        }

        private void WriteCommand(string command)
        {
            if (isServerMode)
            {
                serverClient?.WriteLine(command);
            }
            else
            {
                serialPort?.WriteLine(command);
            }
        }

        private void LogMessage(string message)
        {
            logCallback?.Invoke(message);
        }
    }
}
