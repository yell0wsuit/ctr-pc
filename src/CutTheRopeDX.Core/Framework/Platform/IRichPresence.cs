using System;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Rich-presence integration (Discord on desktop). Optional; null when absent.</summary>
    internal interface IRichPresence : IDisposable
    {
        void Setup();
        void MenuPresence();
        void SetLevelPresence(int pack, int level, int stars, bool isWon = false,
            string levelName = null, int? score = null, int? time = null);
    }
}
