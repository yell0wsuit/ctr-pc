namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Represents the unlock state of a level.
    /// </summary>
    public enum UNLOCKEDSTATE
    {
        /// <summary>The level is locked and cannot be played.</summary>
        LOCKED,

        /// <summary>The level is unlocked and available to play.</summary>
        UNLOCKED,

        /// <summary>The level was just unlocked and should show an unlock animation.</summary>
        JUSTUNLOCKED,

        /// <summary>The level was just unlocked via cheat/debug and should show an unlock animation.</summary>
        JUSTUNLOCKEDWITHCHEAT
    }
}
