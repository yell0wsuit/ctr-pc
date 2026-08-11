using System;
using System.IO;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Watches the externally supplied level file and reports debounced changes to the game loop.
    /// </summary>
    internal sealed class CustomLevelWatcher : IDisposable
    {
        /// <summary>
        /// Starts watching a level file for changes. Hosts without a watchable file system install
        /// no watcher factory, and the level then simply never reloads itself.
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

            watch = PlatformServices.FileWatchers?.Watch(directory, fileName, OnFileEvent);
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
            watch?.Dispose();
            watch = null;
        }

        /// <summary>
        /// Records a file system event. Runs off the game thread, so it touches no engine state.
        /// </summary>
        private void OnFileEvent()
        {
            gate.NotifyChanged(DateTime.UtcNow);
        }

        /// <summary>Debounces the raw file system events.</summary>
        private readonly PendingChangeGate gate;

        /// <summary>Handle stopping the watch, or <see langword="null"/> when the path is not watched.</summary>
        private IDisposable watch;
    }
}
