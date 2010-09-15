//-----------------------------------------------------------------------
// <copyright file="TastyFileSystemEventType.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Defines the possible file system event types raised
    /// by a <see cref="TastyFileSystemWatcher"/> object.
    /// </summary>
    public enum TastyFileSystemEventType
    {
        /// <summary>
        /// Identifies a changed event.
        /// </summary>
        Changed,

        /// <summary>
        /// Identifies a created event.
        /// </summary>
        Created,

        /// <summary>
        /// Identifies a deleted event.
        /// </summary>
        Deleted,

        /// <summary>
        /// Identifies a disposed event.
        /// </summary>
        Disposed,

        /// <summary>
        /// Identifies an error event.
        /// </summary>
        Error,

        /// <summary>
        /// Identifies a renamed event.
        /// </summary>
        Renamed
    }
}
