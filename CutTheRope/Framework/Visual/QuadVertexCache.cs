using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Provides cached vertex arrays for quad rendering to eliminate per-draw allocations.
    /// Since rendering is single-threaded, we use static cached arrays.
    /// </summary>
    internal static class QuadVertexCache
    {
        private static readonly VertexPositionNormalTexture[] s_quadVertices = new VertexPositionNormalTexture[4];

        /// <summary>
        /// Fills the cached quad vertex array with position and texture coordinate data.
        /// Uses 2D positions (Z=0) - suitable for Image and UI rendering.
        /// </summary>
        /// <param name="x">Left X position</param>
        /// <param name="y">Top Y position</param>
        /// <param name="width">Quad width</param>
        /// <param name="height">Quad height</param>
        /// <param name="texLeft">Left texture coordinate (U min)</param>
        /// <param name="texTop">Top texture coordinate (V min)</param>
        /// <param name="texRight">Right texture coordinate (U max)</param>
        /// <param name="texBottom">Bottom texture coordinate (V max)</param>
        /// <returns>The cached vertex array (do not store reference - contents change on next call)</returns>
        public static VertexPositionNormalTexture[] GetTexturedQuad(
            float x, float y, float width, float height,
            float texLeft, float texTop, float texRight, float texBottom)
        {
            s_quadVertices[0] = new VertexPositionNormalTexture(
                new Vector3(x, y, 0f),
                Vector3.UnitZ,
                new Vector2(texLeft, texTop));

            s_quadVertices[1] = new VertexPositionNormalTexture(
                new Vector3(x + width, y, 0f),
                Vector3.UnitZ,
                new Vector2(texRight, texTop));

            s_quadVertices[2] = new VertexPositionNormalTexture(
                new Vector3(x, y + height, 0f),
                Vector3.UnitZ,
                new Vector2(texLeft, texBottom));

            s_quadVertices[3] = new VertexPositionNormalTexture(
                new Vector3(x + width, y + height, 0f),
                Vector3.UnitZ,
                new Vector2(texRight, texBottom));

            return s_quadVertices;
        }

        /// <summary>
        /// Fills the cached quad vertex array from raw float arrays.
        /// Supports both 2D (8 floats) and 3D (12 floats) position arrays.
        /// </summary>
        /// <param name="positions">Position array - 8 floats for 2D (x,y pairs) or 12 floats for 3D (x,y,z triplets)</param>
        /// <param name="texCoords">Texture coordinate array - 8 floats (u,v pairs for 4 vertices)</param>
        /// <returns>The cached vertex array (do not store reference - contents change on next call)</returns>
        public static VertexPositionNormalTexture[] GetTexturedQuadFromArrays(float[] positions, float[] texCoords)
        {
            if (positions.Length == 8)
            {
                // 2D positions (x, y pairs)
                s_quadVertices[0] = new VertexPositionNormalTexture(
                    new Vector3(positions[0], positions[1], 0f),
                    Vector3.UnitZ,
                    new Vector2(texCoords[0], texCoords[1]));

                s_quadVertices[1] = new VertexPositionNormalTexture(
                    new Vector3(positions[2], positions[3], 0f),
                    Vector3.UnitZ,
                    new Vector2(texCoords[2], texCoords[3]));

                s_quadVertices[2] = new VertexPositionNormalTexture(
                    new Vector3(positions[4], positions[5], 0f),
                    Vector3.UnitZ,
                    new Vector2(texCoords[4], texCoords[5]));

                s_quadVertices[3] = new VertexPositionNormalTexture(
                    new Vector3(positions[6], positions[7], 0f),
                    Vector3.UnitZ,
                    new Vector2(texCoords[6], texCoords[7]));
            }
            else
            {
                // 3D positions (x, y, z triplets)
                s_quadVertices[0] = new VertexPositionNormalTexture(
                    new Vector3(positions[0], positions[1], positions[2]),
                    Vector3.UnitZ,
                    new Vector2(texCoords[0], texCoords[1]));

                s_quadVertices[1] = new VertexPositionNormalTexture(
                    new Vector3(positions[3], positions[4], positions[5]),
                    Vector3.UnitZ,
                    new Vector2(texCoords[2], texCoords[3]));

                s_quadVertices[2] = new VertexPositionNormalTexture(
                    new Vector3(positions[6], positions[7], positions[8]),
                    Vector3.UnitZ,
                    new Vector2(texCoords[4], texCoords[5]));

                s_quadVertices[3] = new VertexPositionNormalTexture(
                    new Vector3(positions[9], positions[10], positions[11]),
                    Vector3.UnitZ,
                    new Vector2(texCoords[6], texCoords[7]));
            }

            return s_quadVertices;
        }
    }
}
