using System;
using System.Collections.Generic;

namespace GateKeeperSDK
{
    /// <summary>
    /// A GKFile represents a file or path on a GateKeeper Card.
    /// </summary>
    public class GkFile
    {
        public static readonly string Tag = typeof(GkFile).FullName;
        /// <summary>
        /// The absolute path to the entry on the GateKeeper Card.
        /// </summary>
        protected internal string MCardPath;
        /// <summary>
        /// The name of the file.
        /// </summary>
        protected internal string MName;
        /// <summary>
        /// The {@code Type} of the file.
        /// </summary>
        protected internal GkFile.GkType MType;
        /// <summary>
        /// The size of the file.
        /// </summary>
        private readonly int _mFileSize;

        /// <summary>
        /// Create a {@code GKFile} with the given {@code name} and {@code type}.
        /// </summary>
        /// <param name="name"> the {@code String} name of the file </param>
        /// <param name="type"> the {@code Type} of the file
        /// @since 0.5.0 </param>
        public GkFile(string name, GkFile.GkType type)
        {
            MName = name;
            MType = type;
        }

        /// <summary>
        /// Create a {@code GKFile} with the given {@code name} and {@code type}.
        /// </summary>
        /// <param name="name"> the {@code String} name of the file </param>
        /// <param name="type"> the {@code Type} of the file </param>
        /// <param name="fileSize"> the {@code int} size of the file in bytes
        /// @since 0.15.0 </param>
        public GkFile(string name, GkFile.GkType type, int fileSize)
        {
            MName = name;
            MType = type;
            _mFileSize = fileSize;
        }

        /// <summary>
        /// Retrieve the name of the file.
        /// </summary>
        /// <returns> the name of the file
        /// @since 0.5.0 </returns>
        public virtual string Name
        {
            get
            {
                return MName;
            }
            set
            {
                MName = value;
            }
        }


        /// <summary>
        /// Retrieve the type of the file.
        /// </summary>
        /// <returns> the type of the file
        /// @since 0.5.0 </returns>
        public virtual GkFile.GkType Type
        {
            get
            {
                return MType;
            }
            set
            {
                MType = value;
            }
        }


        /// <summary>
        /// Retrieve whether the file refers to a directory or a file entry on the GateKeeper Card.
        /// </summary>
        /// <returns> {@code true} if the file refers to a directory on the GateKeeper Card or
        /// {@code false} if the file refers to a file entry.
        /// @since 0.5.0 </returns>
        public virtual bool Directory => MType == GkFile.GkType.Directory;

        /// <summary>
        /// Retrieve the file system extension (when present) of the file.
        /// </summary>
        /// <returns> the {@code String} extension of the file.
        /// {@code NULL} if the file is a directory or does not have an extension.
        /// @since 0.5.0 </returns>
        public virtual string Extension
        {
            get
            {
                if (MType == GkFile.GkType.Directory)
                {
                    return null;
                }
                string[] parts = MName.Split(new[] { '\\', '.' });
                string ext = (parts.Length > 1) ? parts[parts.Length - 1] : null;
                return ext;
            }
        }

        /// <summary>
        /// Retrieve the filename without the extension (when present) of the file.
        /// </summary>
        /// <returns> the {@code String} name of the file without extension.
        /// {@code NULL} if the file is a directory or does not have an extension.
        /// @since 0.11.0 </returns>
        public virtual string FilenameBase
        {
            get
            {
                if (MType == GkFile.GkType.Directory)
                {
                    return null;
                }
                int extensionIndex = MName.LastIndexOf(".", StringComparison.Ordinal);
                if (extensionIndex == -1)
                {
                    extensionIndex = MName.Length;
                }
                return MName.Substring(0, extensionIndex);
            }
        }

        /// <summary>
        /// Retrieve the absolute path to the file on the GateKeeper Card.
        /// </summary>
        /// <returns> the absolute {@code String} path to the file
        /// @since 0.5.0 </returns>
        public virtual string CardPath
        {
            get
            {
                return MCardPath;
            }
            set
            {
                MCardPath = value;
            }
        }


        /// <summary>
        /// Assign the absolute path to the file on the GateKeeper Card.
        /// </summary>
        /// <param name="parentPath"> the absolute {@code String} path to the parent of the file </param>
        /// <param name="fileName">   the {@code String} name of the file
        /// @since 0.5.0 </param>
        public virtual void SetCardPath(string parentPath, string fileName)
        {
            CardPath = GkFileUtils.JoinPath(parentPath, fileName);
        }

        /// <summary>
        /// Retrieve the parent directory
        /// </summary>
        /// <returns> the parent directory if present, null otherwise
        /// @since 0.19.0 </returns>
        public virtual string ParentCardPath
        {
            get
            {
                if (CardPath == null)
                {
                    return null;
                }

                List<string> parts = GkFileUtils.ParsePath(CardPath);
                if (parts.Count <= 1)
                {
                    return null;
                }

                int newSize = parts.Count - 1;
                string[] parentParts = parts.GetRange(0, newSize).ToArray();
                return "/" + GkFileUtils.JoinPath(parentParts);
            }
        }

        /// <summary>
        /// Retrieve the size of the file as returned from the card.
        /// </summary>
        /// <returns> the {@code int} value of the file size
        /// @since 0.15.0 </returns>
        public virtual int FileSize => _mFileSize;

        /// <summary>
        /// The type of the entry on the GateKeeper Card.
        /// </summary>
        public enum GkType
        {
            File,
            Directory
        }
    }
}
