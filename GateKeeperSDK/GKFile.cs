using System;
using System.Collections.Generic;

namespace GateKeeperSDK
{
    /// <summary>
    /// A GKFile represents a file or path on a GateKeeper Card.
    /// </summary>
    public class GKFile
    {
        public static readonly string TAG = typeof(GKFile).FullName;
        /// <summary>
        /// The absolute path to the entry on the GateKeeper Card.
        /// </summary>
        protected internal string mCardPath;
        /// <summary>
        /// The name of the file.
        /// </summary>
        protected internal string mName;
        /// <summary>
        /// The {@code Type} of the file.
        /// </summary>
        protected internal GKFile.GKType mType;
        /// <summary>
        /// The size of the file.
        /// </summary>
        private int mFileSize;

        /// <summary>
        /// Create a {@code GKFile} with the given {@code name} and {@code type}.
        /// </summary>
        /// <param name="name"> the {@code String} name of the file </param>
        /// <param name="type"> the {@code Type} of the file
        /// @since 0.5.0 </param>
        public GKFile(string name, GKFile.GKType type)
        {
            mName = name;
            mType = type;
        }

        /// <summary>
        /// Create a {@code GKFile} with the given {@code name} and {@code type}.
        /// </summary>
        /// <param name="name"> the {@code String} name of the file </param>
        /// <param name="type"> the {@code Type} of the file </param>
        /// <param name="fileSize"> the {@code int} size of the file in bytes
        /// @since 0.15.0 </param>
        public GKFile(string name, GKFile.GKType type, int fileSize)
        {
            mName = name;
            mType = type;
            mFileSize = fileSize;
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
                return mName;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Retrieve the type of the file.
        /// </summary>
        /// <returns> the type of the file
        /// @since 0.5.0 </returns>
        public virtual GKFile.GKType Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }


        /// <summary>
        /// Retrieve whether the file refers to a directory or a file entry on the GateKeeper Card.
        /// </summary>
        /// <returns> {@code true} if the file refers to a directory on the GateKeeper Card or
        /// {@code false} if the file refers to a file entry.
        /// @since 0.5.0 </returns>
        public virtual bool Directory
        {
            get
            {
                return mType == GKFile.GKType.DIRECTORY;
            }
        }

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
                if (mType == GKFile.GKType.DIRECTORY)
                {
                    return null;
                }
                string[] parts = mName.Split(new[] { '\\', '.' });
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
                if (mType == GKFile.GKType.DIRECTORY)
                {
                    return null;
                }
                int extensionIndex = mName.LastIndexOf(".", StringComparison.Ordinal);
                if (extensionIndex == -1)
                {
                    extensionIndex = mName.Length;
                }
                return mName.Substring(0, extensionIndex);
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
                return mCardPath;
            }
            set
            {
                mCardPath = value;
            }
        }


        /// <summary>
        /// Assign the absolute path to the file on the GateKeeper Card.
        /// </summary>
        /// <param name="parentPath"> the absolute {@code String} path to the parent of the file </param>
        /// <param name="fileName">   the {@code String} name of the file
        /// @since 0.5.0 </param>
        public virtual void setCardPath(string parentPath, string fileName)
        {
            CardPath = GKFileUtils.joinPath(parentPath, fileName);
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

                List<string> parts = GKFileUtils.parsePath(CardPath);
                if (parts.Count <= 1)
                {
                    return null;
                }

                int newSize = parts.Count - 1;
                string[] parentParts = parts.GetRange(0, newSize).ToArray();
                return "/" + GKFileUtils.joinPath(parentParts);
            }
        }

        /// <summary>
        /// Retrieve the size of the file as returned from the card.
        /// </summary>
        /// <returns> the {@code int} value of the file size
        /// @since 0.15.0 </returns>
        public virtual int FileSize
        {
            get
            {
                return mFileSize;
            }
        }

        /// <summary>
        /// The type of the entry on the GateKeeper Card.
        /// </summary>
        public enum GKType
        {
            FILE,
            DIRECTORY
        }
    }
}
