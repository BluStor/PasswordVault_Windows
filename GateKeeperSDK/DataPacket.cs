using System.IO.Ports;

namespace GateKeeperSDK
{
    public class DataPacket
    {
        public const int HEADER_SIZE = 3;
        public const int CHECKSUM_SIZE = 2;

        public const byte MOST_SIGNIFICANT_BIT = 0x00;
        public const byte LEAST_SIGNIFICANT_BIT = 0x00;

        private byte[] mPayload;
        private int mChannel;

        /// <summary>
        /// Initialize new instance of DataPacket class
        /// </summary>
        /// <param name="payload">Data bytes</param>
        /// <param name="channel">Chanel value</param>
        public DataPacket(byte[] payload, int channel)
        {
            mPayload = payload;
            mChannel = channel;
        }

        public virtual byte[] Payload
        {
            get
            {
                return mPayload;
            }
        }

        public virtual int Channel
        {
            get
            {
                return mChannel;
            }
        }

        public class Builder
        {
            /// <summary>
            /// Builds response from the serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>DataPacket of the serial port response</returns>
            public static DataPacket build(SerialPort inputStream)
            {
                byte[] header = readHeader(inputStream);
                int packetSize = getPacketSize(header);
                int channel = getPacketChannel(header);
                byte[] payload = readPayload(inputStream, packetSize);
                byte[] checksum = readChecksum(inputStream);

                return new DataPacket(payload, channel);
            }

            /// <summary>
            /// Create byte package from data
            /// </summary>
            /// <param name="data">Data to wrap</param>
            /// <param name="channel">Channel value</param>
            /// <returns>Packed bytes</returns>
            public static byte[] toPacketBytes(byte[] data, int channel)
            {
                int packetSize = data.Length + 5;
                byte channelByte = getChannelByte(channel);
                byte msb = getMSB(packetSize);
                byte lsb = getLSB(packetSize);

                byte[] packet = new byte[data.Length + 5];
                packet[0] = channelByte;
                packet[1] = msb;
                packet[2] = lsb;
                for (int i = 0; i < data.Length; i++)
                {
                    packet[i + 3] = data[i];
                }

                packet[packet.Length - 2] = DataPacket.MOST_SIGNIFICANT_BIT;
                packet[packet.Length - 1] = DataPacket.LEAST_SIGNIFICANT_BIT;
                return packet;
            }

            /// <summary>
            /// Gets data size
            /// </summary>
            /// <param name="header">Response header</param>
            /// <returns>Data size</returns>
            internal static int getPacketSize(byte[] header)
            {
                byte packetSizeMSB = header[1];
                byte packetSizeLSB = header[2];
                int packetSize = (int)packetSizeMSB << 8;
                packetSize += (int)packetSizeLSB & 0xFF;
                return packetSize;
            }

            /// <summary>
            /// Gets packet channel from responce
            /// </summary>
            /// <param name="header">Response header</param>
            /// <returns>Packet channel</returns>
            internal static int getPacketChannel(byte[] header)
            {
                return (int)header[0];
            }

            /// <summary>
            /// Reads header from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>Header from serial port</returns>
            internal static byte[] readHeader(SerialPort inputStream)
            {
                return fillByteArrayFromStream(inputStream, DataPacket.HEADER_SIZE);
            }

            /// <summary>
            /// Reads data from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <param name="packetSize">Size of the packet</param>
            /// <returns>Byte array with data</returns>
            internal static byte[] readPayload(SerialPort inputStream, int packetSize)
            {
                int payloadsize = packetSize - (DataPacket.HEADER_SIZE + DataPacket.CHECKSUM_SIZE);
                return fillByteArrayFromStream(inputStream, payloadsize);
            }

            /// <summary>
            /// Reads checksum from serial channel
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <returns>Checksum array</returns>
            internal static byte[] readChecksum(SerialPort inputStream)
            {
                return fillByteArrayFromStream(inputStream, DataPacket.CHECKSUM_SIZE);
            }

            /// <summary>
            /// Reads bytes from serial port
            /// </summary>
            /// <param name="inputStream">Serial port to read</param>
            /// <param name="length">Byte array length</param>
            /// <returns>Byte array for response</returns>
            internal static byte[] fillByteArrayFromStream(SerialPort inputStream, int length)
            {
                byte[] data = new byte[length];
                int totalBytesRead = 0;
                int bytesRead = 0;
                while (totalBytesRead < length && bytesRead != -1)
                {
                    bytesRead = inputStream.Read(data, totalBytesRead, length - totalBytesRead);
                    if (bytesRead != -1)
                    {
                        totalBytesRead += bytesRead;
                    }
                }
                return data;
            }

            /// <summary>
            /// Gets channel byte
            /// </summary>
            /// <param name="channel">Number of channel</param>
            /// <returns></returns>
            internal static byte getChannelByte(int channel)
            {
                return (byte)(channel & 0xff);
            }

            /// <summary>
            /// Gets most significant bit
            /// </summary>
            /// <param name="size">Size</param>
            /// <returns>Most significant bit</returns>
            internal static byte getMSB(int size)
            {
                return (byte)(size >> 8);
            }

            /// <summary>
            /// Gets least significant bit
            /// </summary>
            /// <param name="size">Size</param>
            /// <returns>Least significant bit</returns>
            internal static byte getLSB(int size)
            {
                return (byte)(size & 0xff);
            }
        }
    }
}
