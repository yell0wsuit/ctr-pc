using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Thin managed wrapper over the playtest.js BroadcastChannel module.</summary>
    internal static partial class PlaytestInterop
    {
        /// <summary>Imports playtest.js. Must be awaited once before any other call.</summary>
        /// <returns>A task that completes when the module is available.</returns>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("playtest", "../playtest.js");
        }

        /// <summary>Reads the session nonce from the launch URL.</summary>
        /// <returns>The nonce, or an empty string for a normal launch.</returns>
        [JSImport("nonceFromQuery", "playtest")]
        public static partial string NonceFromQuery();

        /// <summary>Opens the channel and begins queueing incoming messages.</summary>
        [JSImport("open", "playtest")]
        public static partial void Open();

        /// <summary>Posts one JSON message on the channel.</summary>
        /// <param name="json">Message text from <see cref="GameMain.PlaytestChannelMessage"/>.</param>
        [JSImport("post", "playtest")]
        public static partial void Post(string json);

        /// <summary>Takes every message queued since the previous call.</summary>
        /// <returns>The queued messages, oldest first; empty when none arrived.</returns>
        [JSImport("drain", "playtest")]
        public static partial string[] Drain();

        /// <summary>Closes the playtest window.</summary>
        [JSImport("closeWindow", "playtest")]
        public static partial void CloseWindow();
    }
}
