using InTheHand.Net.Bluetooth;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace GateKeeperSDK
{
    public class Card : IDisposable
    {
        private readonly string LIST = "LIST";
        private readonly string STOR = "STOR";
        private readonly string MKD = "MKD";
        private readonly string RETR = "RETR";
        private readonly string MLST = "MLST";
        private readonly string SRFT = "SRFT";
        private readonly string DELE = "DELE";
        private readonly string RMD = "RMD";
        private readonly string _defaultPath = "/apps/vault/data/";
        private readonly Bluetooth _bluetooth;
        private GkMultiplexer _mMultiplexer;
        private SerialPort _serialPort;
        private bool _isActive;


        /// <summary>
        /// Initialize new instance of card class
        /// </summary>
        /// <param name="pass">Card password to pair</param>
        /// <param name="cardName">Card name</param>
        public Card(string pass, string cardName = null)
        {
            if (BluetoothRadio.PrimaryRadio == null) throw new Exception("Local bluetooth device is disconnected");
            var mac = BluetoothRadio.PrimaryRadio.LocalAddress.ToString();
            _bluetooth = cardName == null ? new Bluetooth(mac, pass) : new Bluetooth(mac, pass, cardName);
            Connect();
        }

        /// <summary>
        /// Gets list of files stored on the card
        /// </summary>
        /// <param name="cardPath">Folder path on card. For example: "pictures/animals/cats"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response List(string cardPath)
        {
            cardPath = GlobularPath(cardPath);
            return Get(LIST, cardPath);
        }

        /// <summary>
        /// Gets value of total and free memory on the card
        /// </summary>
        /// <returns>Response from the card with status, message and data</returns>
        public Response FreeMemory()
        {
            return Get(MLST, "");
        }

        /// <summary>
        /// Deletes file from the card
        /// </summary>
        /// <param name="cardPath">Card path</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response Delete(string cardPath)
        {
            return Call(DELE, cardPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardPath"></param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response DeletePath(string cardPath)
        {
            return Call(RMD, cardPath);
        }

        /// <summary>
        /// Gets file from the card
        /// </summary>
        /// <param name="cardPath">File path. For example: "profiles/John/avatar.png"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response Get(string cardPath)
        {
            return Get(RETR, cardPath);
        }

        /// <summary>
        /// Creates folder on the card
        /// </summary>
        /// <param name="cardPath">Folder path to create. For example "pictures/animals/cats"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response CreatePath(string cardPath)
        {
            return Call(MKD, cardPath);
        }

        /// <summary>
        /// Stores file on the card
        /// </summary>
        /// <param name="cardPath">File name to store. For example: "profiles/John/avatar.png"</param>
        /// <param name="inputStream">File bytes to write</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response Put(string cardPath, byte[] inputStream)
        {
            _isActive = true;
            try
            {
                SendCommand(STOR, cardPath);
                Response commandResponse = GetCommandResponse();
                if (commandResponse.Status != 150)
                {
                    return commandResponse;
                }

                using (var stream = new MemoryStream(inputStream))
                {
                    _mMultiplexer.WriteToDataChannel(stream);
                }

                Response dataResponse = GetCommandResponse();

                if (dataResponse.Status != 226)
                {
                    return dataResponse;
                }

                Response result = Srft(cardPath);
                return result;
            }
            catch (IOException e)
            {
                Disconnect();
                throw e;
            }
            finally
            {
                _isActive = false;
            }
        }

        public Response Srft(string cardPath)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            return Call(SRFT, cardPath, new[] { timestamp });
        }

        /// <summary>
        /// Establishes connection between local bluetooth device and card
        /// </summary>
        public void Connect()
        {
            try
            {
                if (IsDisconnected())
                {
                    string sp;
                    if (!_bluetooth.Client.Connected)
                    {
                        _bluetooth.ClientConnect();
                    }
                    sp = _bluetooth.GetBluetoothSerialPort();
                    _bluetooth.Client.Close();
                    if (_bluetooth.Client.Connected)
                    {
                        return;
                    }
                    _serialPort = new SerialPort(sp);
                    SetSerialPortParameters(_serialPort);
                    _serialPort.Open();
                    _mMultiplexer = new GkMultiplexer(_serialPort);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                _bluetooth.RemoveDevice();
                Connect();
            }
            catch (IOException e)
            {
                _mMultiplexer = null;
                throw e;
            }
        }

        /// <summary>
        /// Disconects local client with card
        /// </summary>
        public void Disconnect()
        {
            if (_mMultiplexer != null)
            {
                try
                {
                    _mMultiplexer.Cleanup();
                }
                finally
                {
                    _mMultiplexer = null;
                }
            }
            if (_bluetooth.Client.Connected)
            {
                _bluetooth.Client.Close();
            }
        }


        public void Dispose()
        {
            _bluetooth.Client.Close();
            _bluetooth.Dispose();
            _serialPort.Close();
            _serialPort.Dispose();
        }


        /// <summary>
        /// Checks if card is disconnected
        /// </summary>
        /// <returns>True, if card is disconnected</returns>
        private bool IsDisconnected()
        {
            return _mMultiplexer == null || _bluetooth == null || _bluetooth.Client == null || !_bluetooth.Client.Connected;
        }

        /// <summary>
        /// Executes command for the card
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Path on the card</param>
        /// <param name="arguments">Arguments for card command</param>
        /// <returns>Response from the card with status, message and data</returns>
        private Response Call(string method, string cardPath, string[] arguments = default(string[]))
        {
            _isActive = true;
            try
            {
                SendCommand(method, cardPath, arguments);
                Response commandResponse = GetCommandResponse();
                return commandResponse;
            }
            catch (IOException e)
            {
                Disconnect();
                throw e;
            }
            finally
            {
                _isActive = false;
            }
        }

        /// <summary>
        /// Sets default serial port parameters
        /// </summary>
        /// <param name="serialPort">Serial port to set</param>
        private void SetSerialPortParameters(SerialPort serialPort)
        {
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;
        }

        /// <summary>
        /// Executes command for the card with data response
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Path on the card</param>
        /// <returns>Response from the card with status, message and data</returns>
        private Response Get(string method, string cardPath)
        {
            _isActive = true;
            try
            {
                FileStream dataFile = CreateDataFile();
                SendCommand(method, cardPath);
                Response commandResponse = GetCommandResponse();
                if (commandResponse.Status != 150)
                {
                    return commandResponse;
                }

                Response dataResponse = new Response(_mMultiplexer.ReadDataChannelToFile(dataFile), dataFile);
                return dataResponse;
            }
            catch (IOException e)
            {
                Disconnect();
                throw e;
            }
            finally
            {
                _isActive = false;
            }
        }

        /// <summary>
        /// Creates unique file in temp directory
        /// </summary>
        /// <returns>File stream</returns>
        private FileStream CreateDataFile()
        {
            var tempPath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
            return new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Sends command to the card
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Card path</param>
        /// <param name="arguments">String argument</param>
        private void SendCommand(string method, string cardPath, string[] arguments = default(string[]))
        {
            CheckMultiplexer();
            string cmd = BuildCommandString(method, cardPath, arguments);
            byte[] bytes = GetCommandBytes(cmd);
            _mMultiplexer.WriteToCommandChannel(bytes);
        }

        /// <summary>
        /// Gets response from command channel
        /// </summary>
        /// <returns>Response from the card with status, message and data</returns>
        private Response GetCommandResponse()
        {
            CheckMultiplexer();
            Response response = new Response(_mMultiplexer.ReadCommandChannelLine());
            return response;
        }

        /// <summary>
        /// Checks multiplexer for null value
        /// </summary>
        private void CheckMultiplexer()
        {
            if (_mMultiplexer == null)
            {
                throw new Exception("Not Connected");
            }
        }

        /// <summary>
        /// Builds command for card
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Card path</param>
        /// <param name="arguments">Command arguments</param>
        /// <returns>Concatenated command string</returns>
        private string BuildCommandString(string method, string cardPath, string[] arguments)
        {
            var argumentsString = arguments == null ? string.Empty : string.Join(" ", arguments);
            return argumentsString.Length == 0
                ? $"{method} {_defaultPath}{cardPath}"
                : $"{method} {argumentsString} {_defaultPath}{cardPath}";
        }

        /// <summary>
        /// Gets command bytes from string
        /// </summary>
        /// <param name="cmd">String to encode</param>
        /// <returns>Byte array representation of string</returns>
        private byte[] GetCommandBytes(string cmd)
        {
            return Encoding.ASCII.GetBytes(cmd + "\r\n");
        }

        /// <summary>
        /// Makes card path globular
        /// </summary>
        /// <param name="cardPath">Path on the card</param>
        /// <returns>Globulared path</returns>
        private string GlobularPath(string cardPath)
        {
            if (cardPath.Equals(""))
            {
                cardPath += "*";
            }
            else
            {
                cardPath += "/*";
            }
            return cardPath;
        }
    }
}
