namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Background update checking. Optional; null when absent.</summary>
    internal interface IUpdateService
    {
        void StartIfNeeded();
        void Cancel();
        bool TryConsumeUpdate(out UpdateInfo info);
    }

    /// <summary>
    /// Holds resolved version and release metadata for a newer update.
    /// </summary>
    public sealed class UpdateInfo
    {
        /// <summary>
        /// The currently running version string.
        /// </summary>
        public string CurrentVersion { get; init; }

        /// <summary>
        /// The latest available version string.
        /// </summary>
        public string LatestVersion { get; init; }

        /// <summary>
        /// URL to the release page for the latest version.
        /// </summary>
        public string ReleaseUrl { get; init; }
    }
}
