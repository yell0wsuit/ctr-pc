using System.Numerics;
using System.Runtime.InteropServices;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>Layout-identical to XNA VertexPositionColor (16 bytes).</summary>
    /// <param name="position">Vertex position.</param>
    /// <param name="color">Vertex color.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor(Vector3 position, Color color)
    {
        /// <summary>Vertex position.</summary>
        public Vector3 Position = position;

        /// <summary>Vertex color.</summary>
        public Color Color = color;
    }

    /// <summary>Layout-identical to XNA VertexPositionColorTexture (24 bytes).</summary>
    /// <param name="position">Vertex position.</param>
    /// <param name="color">Vertex color.</param>
    /// <param name="textureCoordinate">Vertex texture coordinate.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
    {
        /// <summary>Vertex position.</summary>
        public Vector3 Position = position;

        /// <summary>Vertex color.</summary>
        public Color Color = color;

        /// <summary>Vertex texture coordinate.</summary>
        public Vector2 TextureCoordinate = textureCoordinate;
    }

    /// <summary>Layout-identical to XNA VertexPositionNormalTexture (32 bytes).</summary>
    /// <param name="position">Vertex position.</param>
    /// <param name="normal">Vertex normal.</param>
    /// <param name="textureCoordinate">Vertex texture coordinate.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
    {
        /// <summary>Vertex position.</summary>
        public Vector3 Position = position;

        /// <summary>Vertex normal.</summary>
        public Vector3 Normal = normal;

        /// <summary>Vertex texture coordinate.</summary>
        public Vector2 TextureCoordinate = textureCoordinate;
    }
}
