using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;
using CutTheRope.Helpers.Discord;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        private DiscordIpcClient _client;
        private DateTime? startTimestamp;

        // Check if RPC is enabled in the save file
        // By default, RPC is enabled
        // Exposing in a save file is to make way for later setting UI integration
        private static bool IsRpcEnabled =>
            Preferences.GetBooleanForKey(CTRPreferences.PREFS_RPC_ENABLED);

        // Replace with your own Discord Application ID if needed
        private readonly string DISCORD_APP_ID = "1457063659724603457";

        /// <summary>
        /// Updates Discord Rich Presence to show the user is browsing the menu.
        /// </summary>
        public void MenuPresence()
        {
            DiscordIpcClient client = Volatile.Read(ref _client);
            if (client == null || !IsRpcEnabled || !client.IsConnected)
            {
                return;
            }

            client.SetActivity(
                details: "Browsing Menu",
                state: $"⭐ Total: {CTRPreferences.GetTotalStars()}",
                startTimestamp: GetOrCreateEpochSeconds());
        }

        /// <summary>
        /// Starts the Discord IPC connection on a background thread.
        /// </summary>
        public void Setup()
        {
            if (!IsRpcEnabled)
            {
                return;
            }

            _ = Task.Run(() =>
            {
                try
                {
                    DiscordIpcClient client = new(DISCORD_APP_ID);
                    if (!client.TryConnect())
                    {
                        client.Dispose();
                        return;
                    }

                    client.SetActivity(startTimestamp: GetOrCreateEpochSeconds());
                    Volatile.Write(ref _client, client);
                }
                catch
                {
                    // Ignore connection failures
                }
            });
        }

        /// <summary>
        /// Returns the session start time as Unix epoch seconds, creating it on first call.
        /// </summary>
        /// <returns>Unix epoch seconds of the session start.</returns>
        private long GetOrCreateEpochSeconds()
        {
            startTimestamp ??= DateTime.UtcNow;
            return new DateTimeOffset(startTimestamp.Value, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        public void Dispose()
        {
            DiscordIpcClient client = Interlocked.Exchange(ref _client, null);
            if (client != null)
            {
                try
                {
                    client.ClearActivity();
                }
                catch
                {
                    // Best effort
                }

                client.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates Discord Rich Presence with the current level information.
        /// </summary>
        /// <param name="pack">Zero-based pack index.</param>
        /// <param name="level">Zero-based level index within the pack.</param>
        /// <param name="stars">Number of stars collected (0-3).</param>
        /// <param name="isWon">Whether the level has been completed.</param>
        /// <param name="score">Final score if the level was won.</param>
        /// <param name="time">Elapsed time in seconds if the level was won.</param>
        public void SetLevelPresence(int pack, int level, int stars, bool isWon = false, int? score = null, int? time = null)
        {
            DiscordIpcClient client = Volatile.Read(ref _client);
            if (client == null || !IsRpcEnabled || !client.IsConnected || Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true) == null)
            {
                return;
            }

            string currentStars = $"⭐ {stars}/3";
            string state = currentStars;

            if (isWon)
            {
                List<string> parts = [];
                if (time.HasValue)
                {
                    // Format time as MM:SS
                    int minutes = time.Value / 60;
                    int seconds = time.Value % 60;
                    parts.Add($"⏱️ {minutes:D2}:{seconds:D2}");
                }
                if (score.HasValue)
                {
                    parts.Add($"🔢 {score.Value}");
                }
                if (parts.Count > 0)
                {
                    state += " | " + string.Join(" | ", parts);
                }
            }

            client.SetActivity(
                details: $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}: {Application.GetString($"LEVEL", forceEnglish: true)} {pack + 1}-{level + 1}",
                state: state,
                startTimestamp: GetOrCreateEpochSeconds(),
                smallImageKey: $"pack_{pack + 1}",
                smallImageText: $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}");
        }
    }
}
