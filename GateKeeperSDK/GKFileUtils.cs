using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GateKeeperSDK
{
    public class GKFileUtils
    {

        /// <summary>
        /// Base path of the card
        /// </summary>
        public const string ROOT = "/data";
        /// <summary>
        /// Base path for license storage
        /// </summary>
        public const string LICENSE_ROOT = "/license";

        private const string DIRECTORY_GROUP = "([-d])";
        private const string PERMISSIONS_GROUP = "\\S+";
        private const string LINKS_GROUP = "\\s+\\S+";
        private const string USER_GROUP = "\\s+\\S+";
        private const string GROUP_GROUP = "\\s+\\S+";
        private const string SIZE_GROUP = "\\s+(\\d+)";
        private const string MONTH_GROUP = "\\s+\\S+";
        private const string DAY_GROUP = "\\s+\\S+";
        private const string YEAR_GROUP = "\\s+\\S+";
        private const string NAME_GROUP = "\\s+(.*)";
        /// <summary>
        /// Regex pattern for the return values of files
        /// </summary>
        public static readonly Regex FILE_PATTERN = new Regex(DIRECTORY_GROUP + PERMISSIONS_GROUP + LINKS_GROUP + USER_GROUP + GROUP_GROUP + SIZE_GROUP + MONTH_GROUP + DAY_GROUP + YEAR_GROUP + NAME_GROUP + "$");

        /// <summary>
        /// Regex pattern for parsing data files by line
        /// </summary>
        public const string DATA_LINE_PATTERN = "(.*)(\r\n|\n)";

        /// <param name="fileData"> the data returned from a LIST command </param>
        /// <returns> a {@code GKFile} built from the fileData
        /// @since 0.15.0 </returns>
        public static GKFile parseFile(string fileData)
        {
            Match fileMatcher = GKFileUtils.FILE_PATTERN.Match(fileData);
            if (fileMatcher.Success)
            {
                string typeString = fileMatcher.Groups[1].Value;
                int size = Convert.ToInt32(fileMatcher.Groups[2].Value);
                string name = fileMatcher.Groups[3].Value;
                GKFile.GKType type = typeString.Equals("d") ? GKFile.GKType.DIRECTORY : GKFile.GKType.FILE;
                return new GKFile(name, type, size);
            }

            return null;
        }

        /// <summary>
        /// Join an array of <seealso cref="String"/> objects using the '/' path separator.
        /// </summary>
        /// <param name="paths"> the array of <seealso cref="String"/> objects to be joined </param>
        /// <returns> the array of {@code paths} delimited by '/' </returns>
        public static string joinPath(params string[] paths)
        {
            List<string> list = nonblankPathSegments(paths);
            return GKStringUtils.Join(list.ToArray(), "/").Replace("/\\/+/", "/");
        }

        /// <summary>
        /// Parse path into an ArrayList using the '/' path separator.
        /// </summary>
        /// <param name="path"> the string to be split into the ArrayList </param>
        /// <returns> the array of path segments </returns>
        public static List<string> parsePath(string path)
        {
            return nonblankPathSegments(path.Split(new[] { '/' }));
        }

        /// <summary>
        /// Read a file and return as a String
        /// </summary>
        /// <param name="path">      the filepath to append the extension to </param>
        /// <param name="extension"> the extension to be appended </param>
        /// <returns> the string representing the filename with extension
        /// @since 0.11.0 </returns>
        public static string addExtension(string path, string extension)
        {
            if (path == null || path.Length == 0)
            {
                return "";
            }

            if (extension == null || extension.Length == 0)
            {
                return path;
            }

            return path + "." + extension;
        }

        /// <summary>
        /// Read a file and return as a String
        /// </summary>
        /// <param name="file"> the file to read </param>
        /// <returns> the string representing the contents of the file </returns>
        /// <exception cref="IOException"> when reading the file fails
        /// @since 0.11.0 </exception>
        public static string readFile(Stream file)
        {
            StreamReader br = new StreamReader(file);
            try
            {
                StringBuilder sb = new StringBuilder();
                for (;;)
                {
                    string line = br.ReadLine();
                    if (line == null)
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(line);
                    }
                    sb.Append(Environment.NewLine);
                }
            }
            finally
            {
                br.Close();
            }
        }

        private static List<string> nonblankPathSegments(string[] paths)
        {
            List<string> list = new List<string>();
            foreach (object path in paths)
            {
                if (path != null && !path.Equals(""))
                {
                    list.Add((string)path);
                }
            }
            return list;
        }
    }
}
