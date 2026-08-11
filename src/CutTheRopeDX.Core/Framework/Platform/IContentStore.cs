namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Provides synchronous raw content bytes to Core.</summary>
    internal interface IContentStore
    {
        /// <summary>Reads content bytes, throwing when the path is unavailable.</summary>
        /// <param name="relativePath">Path relative to the content root.</param>
        byte[] Read(string relativePath);
    }
}
