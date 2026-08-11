using System;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Watches single files the game wants to notice changing, for hosts that have a file system
    /// to watch. Core owns when a change matters and how long to wait for the writer to finish;
    /// the host owns the machinery that reports one.
    /// </summary>
    internal interface IFileWatcherFactory
    {
        /// <summary>Starts watching one file for writes, creations and renames.</summary>
        /// <param name="directory">Directory holding the file.</param>
        /// <param name="fileName">File name to watch within <paramref name="directory"/>.</param>
        /// <param name="onChanged">
        /// Called on every raw file system event, possibly off the game thread and possibly several
        /// times per edit, so it must touch no engine state.
        /// </param>
        /// <returns>A handle that stops the watch when disposed, or <see langword="null"/> if the path cannot be watched.</returns>
        IDisposable Watch(string directory, string fileName, Action onChanged);
    }
}
