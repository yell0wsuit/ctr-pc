namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Receives notification when movie playback completes.
    /// </summary>
    internal interface IMovieMgrDelegate
    {
        /// <summary>Called when the movie at <paramref name="url"/> finishes playing.</summary>
        void MoviePlaybackFinished(string url);
    }
}
