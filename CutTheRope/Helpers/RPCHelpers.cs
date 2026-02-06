using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;
using CutTheRope.Helpers.Discord;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        private DiscordIpcClient _client;
        private DateTime? startTimestamp;
        private bool _isConnected;

        // Check if RPC is enabled in the save file
        // By default, RPC is enabled
        // Exposing in a save file is to make way for later setting UI integration
        private static bool IsRpcEnabled =>
            Preferences.GetBooleanForKey(CTRPreferences.PREFS_RPC_ENABLED);

        // Replace with your own Discord Application ID if needed
        private readonly string DISCORD_APP_ID = "1457063659724603457";

        /// <summary>
        /// 
        /// </summary>
        public void MenuPresence()
        {
            if (_client == null || !IsRpcEnabled || !_isConnected)
            {
                return;
            }

            try
            {
                _client.SetActivity(
                    details: "Browsing Menu",
                    state: $"⭐ Total: {CTRPreferences.GetTotalStars()}",
                    startTimestamp: GetOrCreateEpochSeconds());
            }
            catch
            {
                _isConnected = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Setup()
        {
            if (!IsRpcEnabled)
            {
                return;
            }

            try
            {
                _client = new DiscordIpcClient(DISCORD_APP_ID);
                _isConnected = _client.TryConnect();

                if (!_isConnected)
                {
                    return;
                }

                _client.SetActivity(startTimestamp: GetOrCreateEpochSeconds());
            }
            catch
            {
                _isConnected = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetOrCreateEpochSeconds()
        {
            startTimestamp ??= DateTime.UtcNow;
            return new DateTimeOffset(startTimestamp.Value, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        public void Dispose()
        {
            try
            {
                _client?.ClearActivity();
            }
            catch
            {
                // Best effort
            }

            _client?.Dispose();
            _client = null;
            _isConnected = false;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="level"></param>
        /// <param name="stars"></param>
        /// <param name="isWon"></param>
        /// <param name="score"></param>
        /// <param name="time"></param>
        public void SetLevelPresence(int pack, int level, int stars, bool isWon = false, int? score = null, int? time = null)
        {
            if (_client == null || !IsRpcEnabled || !_isConnected || (Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true) == null))
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

            try
            {
                _client.SetActivity(
                    details: $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}: {Application.GetString($"LEVEL", forceEnglish: true)} {pack + 1}-{level + 1}",
                    state: state,
                    startTimestamp: GetOrCreateEpochSeconds(),
                    smallImageKey: $"pack_{pack + 1}",
                    smallImageText: $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}");
            }
            catch
            {
                _isConnected = false;
            }
        }
    }
}
