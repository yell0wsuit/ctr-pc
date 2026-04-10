namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Music pack name to use
    /// </summary>
    internal static class MusicPackNames
    {
        /// <summary>
        /// Identifier for the original <i>Cut the Rope</i> music pack.
        /// </summary>
        public const string CtROriginal = "ctr_original";
    }

    /// <summary>
    /// List of music pack
    /// </summary>
    internal static class MusicPacks
    {
        /// <summary>
        /// Music tracks used by the original <i>Cut the Rope</i> music pack.
        /// </summary>
        public static string[] CtROriginal = [
            Resources.Music.GameMusic,
            Resources.Music.GameMusic2,
            Resources.Music.GameMusic3,
            Resources.Music.GameMusic4
        ];
    }
}
