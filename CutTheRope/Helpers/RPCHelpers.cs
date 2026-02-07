using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;

using DiscordRPC;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        public DiscordRpcClient Client { get; private set; }
        private DateTime? startTimestamp;

        // Check if RPC is enabled in the save file
        // By default, RPC is enabled
        // Exposing in a save file is to make way for later setting UI integration
        private static bool IsRpcEnabled =>
            Preferences.GetBooleanForKey(CTRPreferences.PREFS_RPC_ENABLED);

        //replace with your own Discord Application ID if needed
        private readonly string DISCORD_APP_ID = "1457063659724603457";

        public void MenuPresence()
        {
            if (Client == null || !IsRpcEnabled || !Client.IsInitialized)
            {
                return;
            }
            Client.SetPresence(new RichPresence()
            {
                Details = "Browsing Menu",
                State = $"⭐ Total: {CTRPreferences.GetTotalStars()}",
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });

        }

        public void Setup()
        {
            if (!IsRpcEnabled)
            {
                return;
            }

            Client = new DiscordRpcClient(DISCORD_APP_ID);
            _ = Client.Initialize();

            if (!Client.IsInitialized)
            {
                return;
            }
            Client.SetPresence(new RichPresence()
            {
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });
        }

        private DateTime GetOrCreateStartTime()
        {
            startTimestamp ??= DateTime.UtcNow;
            return startTimestamp.Value;
        }

        public void Dispose()
        {
            Client?.ClearPresence();
            Client?.Dispose();
            Client = null;
            GC.SuppressFinalize(this);
        }

        public void SetLevelPresence(int pack, int level, int stars, bool isWon = false, int? score = null, int? time = null)
        {
            if (Client == null || !IsRpcEnabled || !Client.IsInitialized || (Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true) == null))
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

            Client.SetPresence(new RichPresence()
            {
                Details = $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}: {Application.GetString($"LEVEL", forceEnglish: true)} {pack + 1}-{level + 1}",
                State = state,
                Assets = new Assets()
                {
                    SmallImageKey = $"pack_{pack + 1}",
                    SmallImageText = $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}"
                },
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });
        }
    }
}
