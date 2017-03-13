using System.Net.Sockets;

namespace GateKeeperSDK
{
    public class DataPacket
    {
        public const int HeaderSize = 3;
        public const int ChecksumSize = 2;

        public const byte MostSignificantBit = 0x00;
        public const byte LeastSignificantBit = 0x00;

        private readonly byte[] _mPayload;
        private readonly int _mChannel;

        /// <summary>
        /// Initialize new instance of DataPacket class
        /// </summary>
        /// <param name="payload">Data bytes</param>
        /// <param name="channel">Chanel value</param>
        public DataPacket(byte[] payload, int channel)
        {
            _mPayload = payload;
            _mChannel = channel;
        }

        public virtual byte[] Payload => _mPayload;

        public virtual int Channel => _mChannel;

        public class Builder
        {
            /// <summary>
            /// Builds response from the serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>DataPacket of the serial port response</returns>
            public static DataPacket Build(NetworkStream inputStream)
            {
                byte[] header = ReadHeader(inputStream);
                int packetSize = GetPacketSize(header);
                int channel = GetPacketChannel(header);
                byte[] payload = ReadPayload(inputStream, packetSize);
                byte[] checksum = ReadChecksum(inputStream);

                return new DataPacket(payload, channel);
            }

            /// <summary>
            /// Create byte package from data
            /// </summary>
            /// <param name="data">Data to wrap</param>
            /// <param name="channel">Channel value</param>
            /// <returns>Packed bytes</returns>
            public static byte[] ToPacketBytes(byte[] data, int channel)
            {
                int packetSize = data.Length + 5;
                byte channelByte = GetChannelByte(channel);
                byte msb = GetMsb(packetSize);
                byte lsb = GetLsb(packetSize);

                byte[] packet = new byte[data.Length + 5];
                packet[0] = channelByte;
                packet[1] = msb;
                packet[2] = lsb;
                for (int i = 0; i < data.Length; i++)
                {
                    packet[i + 3] = data[i];
                }

                packet[packet.Length - 2] = DataPacket.MostSignificantBit;
                packet[packet.Length - 1] = DataPacket.LeastSignificantBit;
                return packet;
            }

            /// <summary>
            /// Gets data size
            /// </summary>
            /// <param name="header">Response header</param>
            /// <returns>Data size</returns>
            internal static int GetPacketSize(byte[] header)
            {
                byte packetSizeMsb = header[1];
                byte packetSizeLsb = header[2];
                int packetSize = (int)packetSizeMsb << 8;
                packetSize += (int)packetSizeLsb & 0xFF;
                return packetSize;
            }

            /// <summary>
            /// Gets packet channel from responce
            /// </summary>
            /// <param name="header">Response header</param>
            /// <returns>Packet channel</returns>
            internal static int GetPacketChannel(byte[] header)
            {
                return (int)header[0];
            }

            /// <summary>
            /// Reads header from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>Header from serial port</returns>
            internal static byte[] ReadHeader(NetworkStream inputStream)
            {
                return FillByteArrayFromStream(inputStream, DataPacket.HeaderSize);
            }

            /// <summary>
            /// Reads data from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <param name="packetSize">Size of the packet</param>
            /// <returns>Byte array with data</returns>
            internal static byte[] ReadPayload(NetworkStream inputStream, int packetSize)
            {
                int payloadsize = packetSize - (DataPacket.HeaderSize + DataPacket.ChecksumSize);
                return FillByteArrayFromStream(inputStream, payloadsize);
            }

            /// <summary>
            /// Reads checksum from serial channel
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>Checksum array</returns>
            internal static byte[] ReadChecksum(NetworkStream inputStream)
            {
                return FillByteArrayFromStream(inputStream, DataPacket.ChecksumSize);
            }

            /// <summary>
            /// Reads bytes from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <param name="length">Byte array length</param>
            /// <returns>Byte array for response</returns>
            internal static byte[] FillByteArrayFromStream(NetworkStream inputStream, int length)
            {
                byte[] data = new byte[length];
                int totalBytesRead = 0;
                int bytesRead = 0;
                while (totalBytesRead < length && bytesRead != -1)
                {
                    bytesRead = inputStream.Read(data, totalBytesRead, length - totalBytesRead);
                    if (bytesRead != 0)
                    {
                        totalBytesRead += bytesRead;
                    }
                    else
                    {
                        break;
                    }
                }
                return data;
            }

            /// <summary>
            /// Gets channel byte
            /// </summary>
            /// <param name="channel">Number of channel</param>
            /// <returns></returns>
            internal static byte GetChannelByte(int channel)
            {
                return (byte)(channel & 0xff);
            }

            /// <summary>
            /// Gets most significant bit
            /// </summary>
            /// <param name="size">Size</param>
            /// <returns>Most significant bit</returns>
            internal static byte GetMsb(int size)
            {
                return (byte)(size >> 8);
            }

            /// <summary>
            /// Gets least significant bit
            /// </summary>
            /// <param name="size">Size</param>
            /// <returns>Least significant bit</returns>
            internal static byte GetLsb(int size)
            {
                return (byte)(size & 0xff);
            }
        }
    }
}
