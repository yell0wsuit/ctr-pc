using CutTheRope.Framework.Visual;

namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// Supported formats for loading texture atlas metadata.
    /// </summary>
    internal enum TextureAtlasFormat
    {
        LegacyXml,
        TexturePackerJson
    }

    /// <summary>
    /// Configuration describing how to load a texture atlas resource.
    /// </summary>
    internal sealed class TextureAtlasConfig
    {
        /// <summary>Indicates which serialization format the atlas uses.</summary>
        public TextureAtlasFormat Format { get; init; } = TextureAtlasFormat.LegacyXml;

        /// <summary>Relative path to the atlas metadata file.</summary>
        public string AtlasPath { get; init; }

        /// <summary>String resource name associated with the atlas.</summary>
        public string ResourceName { get; init; }

        /// <summary>Overrides whether antialiasing should be applied when loading the atlas.</summary>
        public bool? UseAntialias { get; init; }

        /// <summary>Optional pixel format override for atlas textures.</summary>
        public CTRTexture2D.Texture2DPixelFormat? PixelFormat { get; init; }

        /// <summary>Explicit frame ordering when supplied by the metadata.</summary>
        public string[] FrameOrder { get; init; }

        /// <summary>Indicates whether sprite centers should be offset to their geometric centers.</summary>
        public bool CenterOffsets { get; init; }
    }
}
