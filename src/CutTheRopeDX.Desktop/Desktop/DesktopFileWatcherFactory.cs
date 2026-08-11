using System;
using System.IO;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Watches files with <see cref="FileSystemWatcher"/>, the desktop's own mechanism.</summary>
    internal sealed class DesktopFileWatcherFactory : IFileWatcherFactory
    {
        /// <inheritdoc />
        public IDisposable Watch(string directory, string fileName, Action onChanged)
        {
            return new FileWatch(directory, fileName, onChanged);
        }

        /// <summary>One live watch, kept alive by the caller holding the handle.</summary>
        private sealed class FileWatch : IDisposable
        {
            /// <summary>Starts raising <paramref name="onChanged"/> for the named file.</summary>
            /// <param name="directory">Directory holding the file.</param>
            /// <param name="fileName">File name to watch.</param>
            /// <param name="onChanged">Called on each raw file system event.</param>
            public FileWatch(string directory, string fileName, Action onChanged)
            {
                this.onChanged = onChanged;
                watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                };
                watcher.Changed += OnFileEvent;
                watcher.Created += OnFileEvent;
                watcher.Renamed += OnFileEvent;
                watcher.EnableRaisingEvents = true;
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

            private void OnFileEvent(object sender, FileSystemEventArgs e)
            {
                onChanged();
            }

            private readonly Action onChanged;
            private FileSystemWatcher watcher;
        }
    }
}
