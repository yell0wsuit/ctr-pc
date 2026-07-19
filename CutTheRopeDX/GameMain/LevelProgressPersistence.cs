namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides whether a completed level's result should be written to saved progress.
    /// </summary>
    internal static class LevelProgressPersistence
    {
        /// <summary>
        /// Determines whether a new score or star count should replace the stored one.
        /// </summary>
        /// <param name="customLevelActive">Whether this process is running an externally supplied level.</param>
        /// <param name="newValue">Score or star count achieved in the completed level.</param>
        /// <param name="storedValue">Score or star count currently saved for the level.</param>
        /// <returns><see langword="true"/> when the value should be persisted; otherwise <see langword="false"/>.</returns>
        public static bool ShouldPersist(bool customLevelActive, int newValue, int storedValue)
        {
            return !customLevelActive && newValue > storedValue;
        }
    }
}
