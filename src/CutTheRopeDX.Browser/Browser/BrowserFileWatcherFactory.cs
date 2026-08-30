using System;
using System.Collections.Generic;
using System.IO;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Reports level file changes in the browser, where <see cref="FileSystemWatcher"/> throws
    /// <see cref="PlatformNotSupportedException"/>.
    /// </summary>
    /// <remarks>
    /// The WASM filesystem is real enough for Core to read and write, but nothing observes it. So
    /// the writer reports its own change: <c>PlaytestSession</c> (Task 3) writes the level and then
    /// calls <see cref="NotifyChanged"/>. Core cannot tell this apart from a desktop file event,
    /// which is the point - it keeps the same debounce and the same reload decision on both heads.
    /// </remarks>
    internal sealed class BrowserFileWatcherFactory : IFileWatcherFactory
    {
        /// <summary>Registered callbacks, keyed by the full path being watched.</summary>
        private readonly Dictionary<string, List<Action>> _watches = new(StringComparer.Ordinal);

        /// <inheritdoc />
        public IDisposable Watch(string directory, string fileName, Action onChanged)
        {
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || onChanged == null)
            {
                return null;
            }

            string key = Path.Combine(directory, fileName);
            if (!_watches.TryGetValue(key, out List<Action> callbacks))
            {
                callbacks = [];
                _watches[key] = callbacks;
            }
            callbacks.Add(onChanged);
            return new Registration(this, key, onChanged);
        }

        /// <summary>Reports that a watched file has just been rewritten.</summary>
        /// <param name="directory">Directory holding the file.</param>
        /// <param name="fileName">File name within <paramref name="directory"/>.</param>
        /// <remarks>Silently does nothing when nothing is watching that path, which is the normal
        /// case before the game has reached gameplay and installed its watcher.</remarks>
        public void NotifyChanged(string directory, string fileName)
        {
            if (_watches.TryGetValue(Path.Combine(directory, fileName), out List<Action> callbacks))
            {
                // Snapshot: a callback is free to dispose its own registration.
                foreach (Action callback in callbacks.ToArray())
                {
                    callback();
                }
            }
        }

        /// <summary>Stops one watch when disposed.</summary>
        /// <param name="owner">The factory holding the registry.</param>
        /// <param name="key">Full path being watched.</param>
        /// <param name="callback">The callback to remove.</param>
        private sealed class Registration(BrowserFileWatcherFactory owner, string key, Action callback) : IDisposable
        {
            /// <inheritdoc />
            public void Dispose()
            {
                if (owner._watches.TryGetValue(key, out List<Action> callbacks))
                {
                    _ = callbacks.Remove(callback);
                    if (callbacks.Count == 0)
                    {
                        _ = owner._watches.Remove(key);
                    }
                }
            }
        }
    }
}
