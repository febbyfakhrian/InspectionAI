using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace InspectionAI.Classes.Hardware
{
    /// <summary>
    /// PLC Controller - Serial COM communication
    /// </summary>
    public class PLCController
    {
        private SerialPort serialPort;
        private bool isConnected = false;
        private string portName;
        private int baudRate;

        // Events
        public event EventHandler TriggerReceived;
        public event EventHandler<string> DataReceived;
        public event EventHandler<string> ErrorOccurred;

        public bool IsConnected => isConnected;
        public string PortName => portName;

        public PLCController(string port, int baud = 9600)
        {
            portName = port;
            baudRate = baud;
        }

        /// <summary>
        /// Connect to PLC via Serial COM
        /// </summary>
        public bool Connect(out string error)
        {
            error = "";

            try
            {
                // Close existing connection
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    serialPort.Dispose();
                }

                // Create new serial port
                serialPort = new SerialPort(portName, baudRate)
                {
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                // Subscribe to data received event
                serialPort.DataReceived += SerialPort_DataReceived;

                // Open port
                serialPort.Open();
                isConnected = true;

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from PLC
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                isConnected = false;
            }
            catch
            {
                // Ignore errors on disconnect
            }
        }

        /// <summary>
        /// Serial data received handler
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();

                // Raise data received event
                DataReceived?.Invoke(this, data);

                // Check for trigger command
                if (IsTriggerCommand(data))
                {
                    TriggerReceived?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// Check if received data is trigger command
        /// Common trigger formats: "START", "TRIGGER", "1", "0x01"
        /// </summary>
        private bool IsTriggerCommand(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;

            // Check common trigger patterns
            data = data.ToUpper();
            return data.Contains("START") ||
                   data.Contains("TRIGGER") ||
                   data == "1" ||
                   data == "0x01" ||
                   data == "RUN";
        }

        /// <summary>
        /// Send PASS signal to PLC
        /// </summary>
        public bool SendPass()
        {
            return SendCommand("PASS");
        }

        /// <summary>
        /// Send FAIL (NG) signal to PLC
        /// </summary>
        public bool SendFail()
        {
            return SendCommand("FAIL");
        }

        /// <summary>
        /// Send custom command to PLC
        /// </summary>
        public bool SendCommand(string command)
        {
            if (!isConnected || serialPort == null || !serialPort.IsOpen)
                return false;

            try
            {
                serialPort.WriteLine(command);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Send byte array to PLC
        /// </summary>
        public bool SendBytes(byte[] data)
        {
            if (!isConnected || serialPort == null || !serialPort.IsOpen)
                return false;

            try
            {
                serialPort.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get available COM ports
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Test connection to port
        /// </summary>
        public static bool TestPort(string port, out string error)
        {
            error = "";
            try
            {
                using (SerialPort testPort = new SerialPort(port, 9600))
                {
                    testPort.Open();
                    testPort.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}