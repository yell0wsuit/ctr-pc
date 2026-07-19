namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Tracks whether this process is running a single externally supplied level rather than the normal game.
    /// </summary>
    /// <remarks>
    /// Custom level runs skip the splash and menu, never write progress, and reload their level from disk on change.
    /// </remarks>
    internal static class CustomLevelSession
    {
        /// <summary>Gets whether this process is running an externally supplied level.</summary>
        public static bool IsActive { get; private set; }

        /// <summary>Gets the absolute path to the level file being run, or <see langword="null"/> when inactive.</summary>
        public static string LevelPath { get; private set; }

        /// <summary>
        /// Marks this process as a custom level run for the given file.
        /// </summary>
        /// <param name="levelPath">Absolute path to the level XML file.</param>
        public static void Activate(string levelPath)
        {
            LevelPath = levelPath;
            IsActive = !string.IsNullOrWhiteSpace(levelPath);
        }

        /// <summary>Clears custom level state.</summary>
        public static void Clear()
        {
            LevelPath = null;
            IsActive = false;
        }
    }
}
