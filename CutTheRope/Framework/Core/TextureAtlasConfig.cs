namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// Configuration describing how to load a texture atlas resource.
    /// </summary>
    internal sealed class TextureAtlasConfig
    {
        /// <summary>Relative path to the atlas JSON file.</summary>
        public string AtlasPath { get; init; }

        /// <summary>String resource name associated with the atlas.</summary>
        public string ResourceName { get; init; }

        /// <summary>Whether antialiasing should be applied when loading the atlas.</summary>
        public bool UseAntialias { get; init; } = true;

        /// <summary>Whether sprite centers should be offset to their geometric centers.</summary>
        public bool CenterOffsets { get; init; }
    }
}
