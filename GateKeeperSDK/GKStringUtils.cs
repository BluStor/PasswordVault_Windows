using System.Text;

namespace GateKeeperSDK
{
    /// <summary>
    /// GKStringUtils is a functional, static class intended for common operations
    /// involving {@code String} objects.
    /// </summary>
    public class GkStringUtils
    {
        /// <summary>
        /// Join an array of <seealso cref="string"/> objects with a <seealso cref="string"/> separator.
        /// </summary>
        /// <param name="strings">   the array of <seealso cref="string"/> objects to be joined </param>
        /// <param name="separator"> the <seealso cref="string"/> object used to delimit the {@code strings} </param>
        /// <returns> the array of {@code strings} delimited by the {@code separator}
        /// @since 0.5.0 </returns>
        public static string Join(string[] strings, string separator)
        {
            if (strings.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(strings[0]);
            for (int i = 1; i < strings.Length; i++)
            {
                sb.Append(separator);
                sb.Append(strings[i]);
            }
            return sb.ToString();
        }
    }
}
