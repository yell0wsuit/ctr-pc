using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Runs the browser end of a playtest: announces the session, receives levels from the editor,
    /// and puts them where Core already knows how to find them.
    /// </summary>
    /// <remarks>
    /// The desktop head is handed a level by path on the command line and watches that path. A tab
    /// has neither, so this stands in for both: the level arrives over the BroadcastChannel and is
    /// written to <see cref="LevelPath"/> in the WASM in-memory filesystem, which
    /// <see cref="CustomLevelSession"/> is then activated against. From that point Core cannot tell
    /// the two heads apart, and the reload path - debounce, resource scan, instant-versus-full
    /// decision - is literally the same code.
    /// </remarks>
    internal static class PlaytestSession
    {
        /// <summary>Directory holding the session's level file.</summary>
        private const string LevelDirectory = "/playtest";

        /// <summary>File name of the session's level file.</summary>
        private const string LevelFileName = "level.xml";

        /// <summary>How long to wait for the editor to answer the announcement before giving up.</summary>
        private static readonly TimeSpan LevelTimeout = TimeSpan.FromSeconds(10);

        /// <summary>How often to check for the first level while waiting.</summary>
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

        private static PushedFileWatcherFactory _watchers;
        private static string _nonce = "";

        /// <summary>Whether this page is running a playtest.</summary>
        public static bool IsActive { get; private set; }

        /// <summary>Full path of the level file inside the in-memory filesystem.</summary>
        public static string LevelPath => Path.Combine(LevelDirectory, LevelFileName);

        /// <summary>
        /// Announces the session and waits for the editor's first level.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when a level arrived and <see cref="CustomLevelSession"/> was
        /// activated; <see langword="false"/> for a normal launch, or when no editor answered.
        /// </returns>
        /// <remarks>
        /// The announcement goes out before the caller starts loading the content bundle, so the
        /// transfer overlaps a ~56 MB download instead of following it. A launch with no editor
        /// listening - a bookmarked playtest URL, or an editor that has since been closed - times
        /// out and falls through to the normal game rather than hanging on a loading screen.
        /// </remarks>
        public static async Task<bool> BeginAsync()
        {
            await PlaytestInterop.ImportAsync();

            _nonce = PlaytestInterop.NonceFromQuery();
            if (string.IsNullOrEmpty(_nonce))
            {
                return false;
            }

            PlaytestInterop.Open();
            PlaytestInterop.Post(PlaytestChannelMessage.FormatReady(_nonce, PlaytestHandshake.FormatLine(ResolveVersion())));

            DateTime deadline = DateTime.UtcNow + LevelTimeout;
            while (DateTime.UtcNow < deadline)
            {
                if (TryTakeLevel())
                {
                    _watchers = new PushedFileWatcherFactory();
                    PlatformServices.FileWatchers = _watchers;
                    CustomLevelSession.Activate(LevelPath);
                    IsActive = true;

                    // Stands in for the stderr pipe the desktop editor reads. Installed only for a
                    // playtest, so a normal game's console output is untouched.
                    Console.SetError(new PlaytestErrorWriter(Console.Error));
                    return true;
                }

                await Task.Delay(PollInterval);
            }

            Console.Error.WriteLine("playtest: no editor answered; starting the normal game.");
            return false;
        }

        /// <summary>
        /// Delivers any levels that arrived since the last frame. Called once per animation frame.
        /// </summary>
        /// <remarks>
        /// Writing the file and reporting the change are separate on purpose: the report goes
        /// through the same watcher seam a desktop file event does, so Core debounces it and picks
        /// the same reload strategy on both heads.
        /// </remarks>
        public static void Pump()
        {
            if (!IsActive)
            {
                return;
            }

            if (TryTakeLevel())
            {
                _watchers.NotifyChanged(LevelDirectory, LevelFileName);
            }
        }

        /// <summary>Tells the editor about a level this build could not load.</summary>
        /// <param name="message">Human-readable failure reason.</param>
        public static void ReportError(string message)
        {
            if (IsActive)
            {
                PlaytestInterop.Post(PlaytestChannelMessage.FormatError(message));
            }
        }

        /// <summary>Says goodbye and closes the playtest window.</summary>
        public static void Close()
        {
            if (!IsActive)
            {
                return;
            }

            PlaytestInterop.Post(PlaytestChannelMessage.FormatBye());
            PlaytestInterop.CloseWindow();
        }

        /// <summary>
        /// Drains the channel and writes the newest level for this session, if one arrived.
        /// </summary>
        /// <returns><see langword="true"/> when the level file was rewritten.</returns>
        /// <remarks>
        /// Only the last level in a batch is written. Two levels in one frame means the user pressed
        /// Play twice faster than a frame, and the older one is already stale.
        /// </remarks>
        private static bool TryTakeLevel()
        {
            string newest = null;
            foreach (string json in PlaytestInterop.Drain())
            {
                if (PlaytestChannelMessage.TryParse(json, out PlaytestMessageKind kind, out string nonce, out string payload)
                    && kind == PlaytestMessageKind.Level
                    && string.Equals(nonce, _nonce, StringComparison.Ordinal))
                {
                    newest = payload;
                }
            }

            if (newest == null)
            {
                return false;
            }

            // Written to a scratch file and moved into place, so a reader never sees a half-written
            // document. Same-directory moves are atomic, and this holds on the WASM filesystem too.
            _ = Directory.CreateDirectory(LevelDirectory);
            string scratch = LevelPath + ".tmp";
            File.WriteAllText(scratch, newest);
            File.Move(scratch, LevelPath, true);
            return true;
        }

        /// <summary>Resolves this build's version for the handshake.</summary>
        /// <returns>The informational version, the assembly version, or an empty string.</returns>
        private static string ResolveVersion()
        {
            Assembly assembly = typeof(PlaytestSession).Assembly;
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "";
        }
    }
}
