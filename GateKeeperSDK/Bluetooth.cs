using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Authentication;

namespace GateKeeperSDK
{
    public class Bluetooth : IDisposable
    {
        private const string DeviceClass = "1F00";
        private const int DeviceSearchTimeMilliseconds = 7000;
        private readonly BluetoothEndPoint _localEndpoint;
        private readonly BluetoothClient _localClient;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private readonly List<BluetoothDeviceInfo> _allDeviceList;

        private bool _ready = false;
        private readonly object _syncObject = new object();

        public BluetoothDeviceInfo Device { get; set; }
        public BluetoothClient Client => _localClient;

        /// <summary>
        /// Initialize new instance of Bluetooth client
        /// </summary>
        /// <param name="mac">Adress for local bluetooth device</param>
        /// <param name="searchOnDeviceClass">If true, searches on device class</param>
        private Bluetooth(string mac, bool searchOnDeviceClass = true)
        {
            _localEndpoint = new BluetoothEndPoint(BluetoothAddress.Parse(mac), BluetoothService.SerialPort);
            _localClient = new BluetoothClient(_localEndpoint);
            _localClient.InquiryLength = TimeSpan.FromMilliseconds(DeviceSearchTimeMilliseconds);
            _allDeviceList = _localClient.DiscoverDevices(255, false, true, false, false).ToList();
            if (searchOnDeviceClass)
            {
                _deviceList = _allDeviceList.Where(x => x.ClassOfDevice.ToString() == DeviceClass).ToList();
                CheckAndSetDeviceList();
            }
        }

        /// <summary>
        /// Initialize new instance of Bluetooth client
        /// </summary>
        /// <param name="mac">Adress for local bluetooth device</param>
        /// <param name="deviceName">Device name</param>
        public Bluetooth(string mac, string deviceName) : this(mac, false)
        {
            _deviceList = _allDeviceList.Where(x => x.DeviceName.ToLower() == deviceName.ToLower()).ToList();
            CheckAndSetDeviceList();
        }

        /// <summary>
        /// Checks if device search failed
        /// </summary>
        private void CheckAndSetDeviceList()
        {
            if (_deviceList == null || _deviceList.Count == 0) throw new NullReferenceException(CyberGateErrorMessages.NotPaired);
            Device = _deviceList.First();
            if(!Device.Authenticated) throw new AuthenticationException(CyberGateErrorMessages.NotPaired);
        }

        /// <summary>
        /// Enables device serial port service
        /// </summary>
        public void EnableSerialPort()
        {
            Device.SetServiceState(BluetoothService.SerialPort, true);
        }

        /// <summary>
        /// Pair local bluetooth
        /// </summary>
        /// <param name="pass">Device password</param>
        public void Pair(string pass)
        {
            // get a list of all paired devices
            BluetoothDeviceInfo[] paired = _localClient.DiscoverDevices(255, false, true, false, false);
            // check every discovered device if it is already paired 
            foreach (BluetoothDeviceInfo device in _deviceList)
            {
                bool isPaired = false;
                for (int i = 0; i < paired.Length; i++)
                {
                    if (device.Equals(paired[i]))
                    {
                        isPaired = true;
                        break;
                    }
                }

                // if the device is not paired, pair it!
                if (!isPaired)
                {
                    // replace DEVICE_PIN here, synchronous method, but fast
                    isPaired = BluetoothSecurity.PairRequest(device.DeviceAddress, pass);
                    if (!isPaired)
                    {
                        Pair(pass);
                    }
                }

                // set pin of device to connect with
                _localClient.SetPin(pass);
                // async connection method

                Device = device;
                Connect();
            }
        }

        /// <summary>
        /// Gets serial port name of conected device
        /// </summary>
        /// <returns>String representation of serial port</returns>
        public string GetBluetoothSerialPort()
        {
            const string win32SerialPort = "Win32_SerialPort";
            SelectQuery q = new SelectQuery(win32SerialPort);
            ManagementObjectSearcher s = new ManagementObjectSearcher(q);
            foreach (ManagementBaseObject cur in s.Get())
            {
                ManagementObject mo = (ManagementObject)cur;
                string pnpId = mo.GetPropertyValue("PNPDeviceID").ToString();

                if (pnpId.Contains(Device.DeviceAddress.ToString()))
                {
                    object captionObject = mo.GetPropertyValue("Caption");
                    string caption = captionObject.ToString();
                    int index = caption.LastIndexOf("(COM", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        string portString = caption.Substring(index);
                        string comPort = portString.
                                      Replace("(", string.Empty).Replace(")", string.Empty);
                        return comPort;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates pair and enables serial port for client
        /// </summary>
        public void ClientConnect()
        {
            Connect();
            EnableSerialPort();
        }

        public void Dispose()
        {
            _localClient.Close();
            _localClient.Dispose();
        }

        private void Connected(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // client is connected now
                lock (_syncObject) _ready = true;
            }
        }

        private void Connect()
        {
            try
            {
                _localClient.BeginConnect(Device.DeviceAddress, BluetoothService.SerialPort,
                    new AsyncCallback(Connected), Device);

                bool ready = false;
                while (!ready)
                {
                    lock (_syncObject) ready = _ready;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConnectSync()
        {
            _localClient.Connect(Device.DeviceAddress, BluetoothService.SerialPort);
        }
    }
}
