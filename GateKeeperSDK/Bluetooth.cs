using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace GateKeeperSDK
{
    public class Bluetooth : IDisposable
    {
        private const string DeviceClass = "1F00";
        private BluetoothEndPoint localEndpoint;
        private readonly BluetoothClient _localClient;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private readonly string _devicePassword;

        public BluetoothDeviceInfo Device { get; set; }
        public BluetoothClient Client => _localClient;

        /// <summary>
        /// Initialize new instance of Bluetooth client
        /// </summary>
        /// <param name="mac">Adress for local bluetooth device</param>
        /// <param name="devicePassword">Device password</param>
        public Bluetooth(string mac, string devicePassword)
        {
            _devicePassword = devicePassword;
            localEndpoint = new BluetoothEndPoint(BluetoothAddress.Parse(mac), BluetoothService.SerialPort);
            _localClient = new BluetoothClient(localEndpoint);
            var allDevices = _localClient.DiscoverDevices();
            _deviceList = allDevices.Where(x => x.ClassOfDevice.ToString() == DeviceClass).ToList();
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
        /// <param name="pass"></param>
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
                try
                {
                    _localClient.BeginConnect(device.DeviceAddress, BluetoothService.SerialPort,
                        new AsyncCallback(Connect), device);
                }
                catch (Exception) { }

                Device = device;
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
            Pair(_devicePassword);
            EnableSerialPort();
        }

        private void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // client is connected now
            }
        }

        public void Dispose()
        {
            _localClient.Close();
        }
    }
}
