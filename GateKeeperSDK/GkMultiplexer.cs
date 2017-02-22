using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace GateKeeperSDK
{
    /// <summary>
    /// Intended for internal use only.
    /// </summary>
    public class GkMultiplexer
    {
        public static readonly string Tag = typeof(GkMultiplexer).FullName;
        public const int MaximumPayloadSize = 512;
        public const int CommandChannel = 1;
        public const int DataChannel = 2;

        private const byte CarriageReturn = 13;
        private const byte LineFeed = 10;
        private const int UploadDelayMillis = 1;

        private long _currentDataTransferFileSize = 0;

        private readonly SerialPort _serialPort;

        /// <summary>
        /// Initialize new instance of multiplexer class
        /// </summary>
        /// <param name="serialPort">Serial port to use</param>
        public GkMultiplexer(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public virtual long CurrentDataTransferSize => _currentDataTransferFileSize;

        /// <summary>
        /// Writes bytes to command channel
        /// </summary>
        /// <param name="data">Data to write</param>
        public virtual void WriteToCommandChannel(byte[] data)
        {
            Write(data, CommandChannel);
        }

        /// <summary>
        /// Writes bytes to data channel
        /// </summary>
        /// <param name="data">Data to write</param>
        public virtual void WriteToDataChannel(byte[] data)
        {
            Write(data, DataChannel);
        }

        /// <summary>
        /// Writes data to data channel from stream
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        public virtual void WriteToDataChannel(Stream inputStream)
        {
            byte[] buffer = new byte[MaximumPayloadSize];
            _currentDataTransferFileSize = 0;
            int counter = 0;
            try
            {
                int bytesRead;
                do
                {
                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        continue;
                    }
                    _currentDataTransferFileSize += buffer.Length;
                    counter += 1;
                    if (bytesRead < buffer.Length)
                    {
                        WriteToDataChannel(Arrays.CopyOf(buffer, bytesRead));
                    }
                    else
                    {
                        WriteToDataChannel(buffer);
                    }
                    Thread.Sleep(UploadDelayMillis);
                } while (bytesRead != 0);
            }
            finally
            {
                _currentDataTransferFileSize = 0;
            }
        }

        /// <summary>
        /// Read data from command channel
        /// </summary>
        /// <returns>Byte array from command channel</returns>
        public virtual byte[] ReadCommandChannelLine()
        {
            DataPacket packet = DataPacket.Builder.Build(_serialPort);
            return ReadCommandLine(packet);
        }

        /// <summary>
        /// Read from the data channel and write it to the specified file. Once a response is read from
        /// the command channel, we are done reading data and can return the command response
        /// </summary>
        /// <param name="dataFile"> the {@code File} to write the data channel to </param>
        /// <returns> the data read from the command channel following data transfer </returns>
        /// <exception cref="IOException"> when connection to the card is disrupted or writing data to a file fails </exception>
        public virtual byte[] ReadDataChannelToFile(Stream dataFile)
        {
            try
            {
                DataPacket packet = DataPacket.Builder.Build(_serialPort);

                _currentDataTransferFileSize = 0;
                int COUNTER = 0;
                while (DataChannel == packet.Channel)
                {
                    byte[] payload = packet.Payload;
                    COUNTER += 1;
                    _currentDataTransferFileSize += payload.Length;
                    dataFile.Write(payload, 0, payload.Length);
                    packet = DataPacket.Builder.Build(_serialPort);
                }

                return ReadCommandLine(packet);
            }
            catch (IOException e)
            {
                Cleanup();
                throw e;
            }
            finally
            {
                _currentDataTransferFileSize = 0;
            }
        }

        /// <summary>
        /// Closes serial port
        /// </summary>
        public virtual void Cleanup()
        {
            try
            {
                _serialPort.Close();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// Reads command line from data packet
        /// </summary>
        /// <param name="packet">Packet to read</param>
        /// <returns>Byte array of data without headers</returns>
        private byte[] ReadCommandLine(DataPacket packet)
        {
            List<byte> bytes = new List<byte>();
            while (!ContainsCrlf(packet.Payload))
            {
                CopyUntilCrlf(packet.Payload, bytes);
                packet = DataPacket.Builder.Build(_serialPort);
            }
            CopyUntilCrlf(packet.Payload, bytes);
            return bytes.ToArray();
        }

        /// <summary>
        /// Checks if byte array contains \n \r
        /// </summary>
        /// <param name="payload">Byte array to check</param>
        /// <returns>True, if byte array contains \n \r</returns>
        private bool ContainsCrlf(byte[] payload)
        {
            for (int i = 0; i < payload.Length - 1; i++)
            {
                if (payload[i] == CarriageReturn && payload[i + 1] == LineFeed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copies data until \n \r
        /// </summary>
        /// <param name="data">Input byte array</param>
        /// <param name="bytes">Result bytes</param>
        /// <returns>Byte array</returns>
        private byte[] CopyUntilCrlf(byte[] data, List<byte> bytes)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte a = data[i];
                if (a == CarriageReturn && i < data.Length - 1 && data[i + 1] == LineFeed)
                {
                    break;
                }
                bytes.Add(a);
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// Writes data to serial port
        /// </summary>
        /// <param name="data">Data to write</param>
        /// <param name="channel">Channel to which data will be written</param>
        private void Write(byte[] data, int channel)
        {
            byte[] packetBytes = DataPacket.Builder.ToPacketBytes(data, channel);
            _serialPort.Write(packetBytes, 0, packetBytes.Length);
        }
    }

}
