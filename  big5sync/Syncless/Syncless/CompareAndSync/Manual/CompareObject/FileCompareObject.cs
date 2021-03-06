﻿/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using System.Collections.Generic;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    /// <summary>
    /// The <c>FileCompareObject</c> class contains all information related to a file node.
    /// </summary>
    public class FileCompareObject : BaseCompareObject
    {
        //Actual file information
        private string[] _hash; // Array to store the hash of each file.
        private long[] _length; // Array to store the length/size of each file.
        private long[] _lastWriteTimeUtc; // Array to store the last write/modified time of each file.
        private List<int> _conflictPos; //Stores a list of conflicted files.

        //Metadata file information
        private string[] _metaHash; // Array to store the hash of each file based on metadata.
        private long[] _metaLength; // Array to store the length/size of each file based on metadata.
        private long[] _metaLastWriteTimeUtc; // Array to store the last write time of each file based on metadata.

        /// <summary>
        /// Initializes a <c>FileCompareObject</c> given the name of the file, the number of paths to synchronize, and the parent of this file.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name of this <c>FileCompareObject</c>.</param>
        /// <param name="numOfPaths">An <see cref="int"/> containing the number of paths to synchronize.</param>
        /// <param name="parent">The <see cref="FolderCompareObject"/> which is the parent of this <c>FileCompareObject</c>.</param>
        public FileCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _hash = new string[numOfPaths];
            _length = new long[numOfPaths];
            _lastWriteTimeUtc = new long[numOfPaths];

            _metaHash = new string[numOfPaths];
            _metaLength = new long[numOfPaths];
            _metaLastWriteTimeUtc = new long[numOfPaths];
        }

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of file hash.
        /// </summary>
        public string[] Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of file length.
        /// </summary>
        public long[] Length
        {
            get { return _length; }
            set { _length = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of file last write time.
        /// </summary>
        public long[] LastWriteTimeUtc
        {
            get { return _lastWriteTimeUtc; }
            set { _lastWriteTimeUtc = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata hash.
        /// </summary>
        public string[] MetaHash
        {
            get { return _metaHash; }
            set { _metaHash = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata length.
        /// </summary>
        public long[] MetaLength
        {
            get { return _metaLength; }
            set { _metaLength = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata last write time.
        /// </summary>
        public long[] MetaLastWriteTimeUtc
        {
            get { return _metaLastWriteTimeUtc; }
            set { _metaLastWriteTimeUtc = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="List{T}"/> of conflicted positions.
        /// </summary>
        /// <remarks>A new list will be created if it is currently null.</remarks>
        public List<int> ConflictPositions
        {
            get
            {
                if (_conflictPos == null)
                    _conflictPos = new List<int>();
                return _conflictPos;
            }
            set { _conflictPos = value; }
        }

        #endregion

    }
}