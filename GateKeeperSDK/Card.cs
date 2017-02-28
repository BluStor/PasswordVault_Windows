using InTheHand.Net.Bluetooth;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace GateKeeperSDK
{
    public class Card : IDisposable
    {
        private readonly string _pass;
        private readonly string _mac;
        private readonly string _cardName;
        private readonly string _defaultPath = "/apps/vault/data";
        private Bluetooth _bluetooth;
        private GkMultiplexer _mMultiplexer;
        private SerialPort _serialPort;
        private bool _isActive;

        public string BluetoothName => _bluetooth?.Device?.DeviceName;
        public string Mac => _mac;
        public bool IsAvailableSerial => _serialPort != null && _serialPort.IsOpen;


        /// <summary>
        /// Initialize new instance of card class
        /// </summary>
        /// <param name="pass">Card password to pair</param>
        /// <param name="cardName">Card name</param>
        /// <param name="mac">Local bluetooth device mac adress</param>
        /// <param name="establishConnection">If true, bluetooth connection will be established</param>
        public Card(string pass, string cardName = null, string mac = null, bool establishConnection = true)
        {
            _pass = pass;
            _mac = mac;
            _cardName = cardName;
            if(establishConnection) BuildConnection();
        }

        /// <summary>
        /// Builds connection for bluetooth
        /// </summary>
        public void BuildConnection()
        {
            if (!IsAvailableSerial)
            {
                _bluetooth = _cardName == null ? new Bluetooth(_mac, _pass) : new Bluetooth(_mac, _pass, _cardName);
                Connect();
            }
        }

        /// <summary>
        /// Gets list of files stored on the card
        /// </summary>
        /// <param name="cardPath">Folder path on card. For example: "pictures/animals/cats"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response List(string cardPath)
        {
            cardPath = GlobularPath(cardPath);
            return Get(Commands.LIST, cardPath, null, false);
        }

        /// <summary>
        /// Gets current working directory
        /// </summary>
        /// <returns>Response from the card with status, message and data</returns>
        public Response CurrentWorkingDirectory()
        {
            return Call(Commands.PWD, string.Empty, null, false);
        }

        /// <summary>
        /// Gets current working directory
        /// </summary>
        /// <returns>Response from the card with status, message and data</returns>
        public Response ChangeWorkingDirectory(string cardPath)
        {
            return Call(Commands.CWD, cardPath, null, false);
        }

        /// <summary>
        /// Gets value of total and free memory on the card
        /// </summary>
        /// <returns>Response from the card with status, message and data</returns>
        public Response FreeMemory()
        {
            return Get(Commands.MLST, "", null, true);
        }

        public Response Rename(string source, string target)
        {
            _isActive = true;

            var result = Call(Commands.RNFR, source, null, false);
            if (result.Status == 350)
            {
                result = Call(Commands.RNTO, target, null, false);
            }

            _isActive = false;

            return result;
        }

        /// <summary>
        /// Deletes file from the card
        /// </summary>
        /// <param name="cardPath">Card path</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response Delete(string cardPath)
        {
            return Call(Commands.DELE, cardPath, null, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardPath"></param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response DeletePath(string cardPath)
        {
            return Call(Commands.RMD, cardPath, null, false);
        }

        /// <summary>
        /// Gets file from the card
        /// </summary>
        /// <param name="cardPath">File path. For example: "profiles/John/avatar.png"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response Get(string cardPath)
        {
            return Get(Commands.RETR, cardPath, null, false);
        }

        /// <summary>
        /// Creates folder on the card
        /// </summary>
        /// <param name="cardPath">Folder path to create. For example "pictures/animals/cats"</param>
        /// <returns>Response from the card with status, message and data</returns>
        public Response CreatePath(string cardPath)
        {
            return Call(Commands.MKD, cardPath, null, false);
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
                SendCommand(Commands.STOR, cardPath, null, false);
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
            return Call(Commands.SRFT, cardPath, new[] { timestamp }, false);
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
                try
                {
                    _bluetooth.CheckDeviceList();
                    Connect();
                }
                catch (NullReferenceException ex)
                {
                    throw new Exception(ex.Message, e);
                }
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
            if (_bluetooth != null && _bluetooth.Client != null && _bluetooth.Client.Connected)
            {
                _bluetooth.Client.Close();
            }
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
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
        /// Executes command for the card
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Path on the card</param>
        /// <param name="arguments">Arguments for card command</param>
        /// <param name="needsTransform">If true, card path will be complemented with default path</param>
        /// <returns>Response from the card with status, message and data</returns>
        private Response Call(string method, string cardPath, string[] arguments = default(string[]), bool needsTransform = true)
        {
            _isActive = true;
            try
            {
                SendCommand(method, cardPath, arguments, needsTransform);
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
        /// <param name="arguments">Arguments for card command</param>
        /// <param name="needsTransform">If true, card path will be complemented with default path</param>
        /// <returns>Response from the card with status, message and data</returns>
        private Response Get(string method, string cardPath, string[] arguments = default(string[]), bool needsTransform = true)
        {
            _isActive = true;
            try
            {
                FileStream dataFile = CreateDataFile();
                SendCommand(method, cardPath, arguments, needsTransform);
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
        /// <param name="needsTransform">If true, card path will be complemented with default path</param>
        private void SendCommand(string method, string cardPath, string[] arguments = default(string[]), bool needsTransform = true)
        {
            CheckMultiplexer();
            string cmd = BuildCommandString(method, cardPath, arguments, needsTransform);
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
        /// Checks if card is disconnected
        /// </summary>
        /// <returns>True, if card is disconnected</returns>
        private bool IsDisconnected()
        {
            return _mMultiplexer == null || _bluetooth?.Client == null || !_bluetooth.Client.Connected;
        }

        /// <summary>
        /// Builds command for card
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="cardPath">Card path</param>
        /// <param name="arguments">Command arguments</param>
        /// <param name="needsTransform">If true, card path will be complemented with default path</param>
        /// <returns>Concatenated command string</returns>
        private string BuildCommandString(string method, string cardPath, string[] arguments, bool needsTransform = true)
        {
            var argumentsString = arguments == null ? string.Empty : string.Join(" ", arguments);
            if (argumentsString.Length == 0)
            {
                if (needsTransform)
                {
                    return $"{method} {_defaultPath}{cardPath}";
                }

                return $"{method} {cardPath}";
            }
            else
            {
                if (needsTransform)
                {
                    return $"{method} {argumentsString} {_defaultPath}{cardPath}";
                }
                return $"{method} {argumentsString} {cardPath}";
            }
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
