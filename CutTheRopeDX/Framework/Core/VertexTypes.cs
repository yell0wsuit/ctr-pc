using System.Numerics;
using System.Runtime.InteropServices;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>Layout-identical to XNA VertexPositionColor (16 bytes).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor(Vector3 position, Color color)
    {
        public Vector3 Position = position;
        public Color Color = color;
    }

    /// <summary>Layout-identical to XNA VertexPositionColorTexture (24 bytes).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
    {
        public Vector3 Position = position;
        public Color Color = color;
        public Vector2 TextureCoordinate = textureCoordinate;
    }

    /// <summary>Layout-identical to XNA VertexPositionNormalTexture (32 bytes).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
    {
        public Vector3 Position = position;
        public Vector3 Normal = normal;
        public Vector2 TextureCoordinate = textureCoordinate;
    }
}
