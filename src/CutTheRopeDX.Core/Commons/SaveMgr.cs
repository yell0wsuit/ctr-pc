namespace CutTheRopeDX.Commons
{
    /// <summary>
    /// Manages save-data backup and restore operations.
    /// </summary>
    /// <remarks>No-op code, leftover from mobile version.</remarks>
    internal sealed class SaveMgr
    {
        /// <summary>
        /// Creates a backup of the current save data.
        /// </summary>
        public static void Backup()
        {
        }

        /// <summary>
        /// Restores save data from a previous backup.
        /// </summary>
        public static void Restore()
        {
        }

        /// <summary>
        /// Returns whether a saved backup exists and can be restored.
        /// </summary>
        /// <returns><see langword="true"/> when a recoverable save exists; otherwise <see langword="false"/>.</returns>
        public static bool IsSaveAvailable()
        {
            return false;
        }
    }
}
