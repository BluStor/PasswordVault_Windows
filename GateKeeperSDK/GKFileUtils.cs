using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GateKeeperSDK
{
    public class GkFileUtils
    {

        /// <summary>
        /// Base path of the card
        /// </summary>
        public const string Root = "/data";
        /// <summary>
        /// Base path for license storage
        /// </summary>
        public const string LicenseRoot = "/license";

        private const string DirectoryGroup = "([-d])";
        private const string PermissionsGroup = "\\S+";
        private const string LinksGroup = "\\s+\\S+";
        private const string UserGroup = "\\s+\\S+";
        private const string GroupGroup = "\\s+\\S+";
        private const string SizeGroup = "\\s+(\\d+)";
        private const string MonthGroup = "\\s+\\S+";
        private const string DayGroup = "\\s+\\S+";
        private const string YearGroup = "\\s+\\S+";
        private const string NameGroup = "\\s+(.*)";
        /// <summary>
        /// Regex pattern for the return values of files
        /// </summary>
        public static readonly Regex FilePattern = new Regex(DirectoryGroup + PermissionsGroup + LinksGroup + UserGroup + GroupGroup + SizeGroup + MonthGroup + DayGroup + YearGroup + NameGroup + "$");

        /// <summary>
        /// Regex pattern for parsing data files by line
        /// </summary>
        public const string DataLinePattern = "(.*)(\r\n|\n)";

        /// <param name="fileData"> the data returned from a LIST command </param>
        /// <returns> a {@code GKFile} built from the fileData
        /// @since 0.15.0 </returns>
        public static GkFile ParseFile(string fileData)
        {
            Match fileMatcher = GkFileUtils.FilePattern.Match(fileData);
            if (fileMatcher.Success)
            {
                string typeString = fileMatcher.Groups[1].Value;
                int size = Convert.ToInt32(fileMatcher.Groups[2].Value);
                string name = fileMatcher.Groups[3].Value;
                GkFile.GkType type = typeString.Equals("d") ? GkFile.GkType.Directory : GkFile.GkType.File;
                return new GkFile(name, type, size);
            }

            return null;
        }

        /// <summary>
        /// Join an array of <seealso cref="String"/> objects using the '/' path separator.
        /// </summary>
        /// <param name="paths"> the array of <seealso cref="String"/> objects to be joined </param>
        /// <returns> the array of {@code paths} delimited by '/' </returns>
        public static string JoinPath(params string[] paths)
        {
            List<string> list = NonblankPathSegments(paths);
            return GkStringUtils.Join(list.ToArray(), "/").Replace("/\\/+/", "/");
        }

        /// <summary>
        /// Parse path into an ArrayList using the '/' path separator.
        /// </summary>
        /// <param name="path"> the string to be split into the ArrayList </param>
        /// <returns> the array of path segments </returns>
        public static List<string> ParsePath(string path)
        {
            return NonblankPathSegments(path.Split(new[] { '/' }));
        }

        /// <summary>
        /// Read a file and return as a String
        /// </summary>
        /// <param name="path">      the filepath to append the extension to </param>
        /// <param name="extension"> the extension to be appended </param>
        /// <returns> the string representing the filename with extension
        /// @since 0.11.0 </returns>
        public static string AddExtension(string path, string extension)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (string.IsNullOrEmpty(extension))
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
        public static string ReadFile(Stream file)
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

        private static List<string> NonblankPathSegments(string[] paths)
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
