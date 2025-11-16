using CutTheRope.Framework.Visual;

namespace CutTheRope.Framework.Core
{
    internal enum TextureAtlasFormat
    {
        LegacyXml,
        TexturePackerJson
    }

    internal sealed class TextureAtlasConfig
    {
        public TextureAtlasFormat Format { get; init; } = TextureAtlasFormat.LegacyXml;

        public string AtlasPath { get; init; }

        public bool? UseAntialias { get; init; }

        public CTRTexture2D.Texture2DPixelFormat? PixelFormat { get; init; }

        public string[] FrameOrder { get; init; }

        public bool CenterOffsets { get; init; }
    }
}
