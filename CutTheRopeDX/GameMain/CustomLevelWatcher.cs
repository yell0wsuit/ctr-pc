using System;
using System.IO;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Watches the externally supplied level file and reports debounced changes to the game loop.
    /// </summary>
    internal sealed class CustomLevelWatcher : IDisposable
    {
        /// <summary>
        /// Starts watching a level file for changes.
        /// </summary>
        /// <param name="levelPath">Absolute path to the level file.</param>
        /// <param name="quietPeriod">How long the file must be idle before a change is reported.</param>
        public CustomLevelWatcher(string levelPath, TimeSpan quietPeriod)
        {
            gate = new PendingChangeGate(quietPeriod);

            string directory = Path.GetDirectoryName(levelPath);
            string fileName = Path.GetFileName(levelPath);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
            };
            watcher.Changed += OnFileEvent;
            watcher.Created += OnFileEvent;
            watcher.Renamed += OnFileEvent;
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Takes a pending file change if the file has been quiet long enough.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <returns><see langword="true"/> when the level should be reloaded; otherwise <see langword="false"/>.</returns>
        public bool TryConsumeChange(DateTime nowUtc)
        {
            return gate.TryConsume(nowUtc);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (watcher == null)
            {
                return;
            }

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFileEvent;
            watcher.Created -= OnFileEvent;
            watcher.Renamed -= OnFileEvent;
            watcher.Dispose();
            watcher = null;
        }

        /// <summary>
        /// Records a file system event. Runs on a threadpool thread, so it touches no engine state.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event data.</param>
        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            gate.NotifyChanged(DateTime.UtcNow);
        }

        /// <summary>Debounces the raw file system events.</summary>
        private readonly PendingChangeGate gate;

        /// <summary>Underlying file system watcher, or <see langword="null"/> when the path could not be watched.</summary>
        private FileSystemWatcher watcher;
    }
}
