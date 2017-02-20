using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeperSDK
{
    public static class Helper
    {
        /// <summary>
        /// Gets byte array of hex string
        /// </summary>
        /// <param name="hex">Hex string to encode</param>
        /// <returns>Byte array representation of hex string</returns>
        public static byte[] hexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Gets string from byte array
        /// </summary>
        /// <param name="obj">Byte array to encode</param>
        /// <returns>String representation of byte array</returns>
        public static string asString(this byte[] obj)
        {
            return obj.Aggregate(string.Empty, (current, b) => current + (b + " "));
        }
    }

    public static class Arrays
    {
        /// <summary>
        /// Copies byte array with specific length
        /// </summary>
        /// <param name="originalBytes">Byte array to copy</param>
        /// <param name="newLength">New length</param>
        /// <returns>New byte array with new length</returns>
        public static byte[] copyOf(byte[] originalBytes, int newLength)
        {
            var result = new byte[newLength];
            for (int i = 0; i < newLength; i++)
            {
                result[i] = originalBytes[i];
            }

            return result;
        }
    }
}
