namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Generates the immutable triangle-list index pattern for batched quads. Each quad's
    /// four strip-ordered vertices decompose to triangles (0,1,2) and (2,1,3), matching
    /// what DrawTriangleStrip renders for a four-vertex strip (cull mode is None, so
    /// winding is irrelevant).
    /// </summary>
    internal static class QuadIndexPattern
    {
        /// <summary>
        /// Maximum quads per batch. 2,048 quads use vertex indices up to 8,191,
        /// comfortably within 16-bit index range.
        /// </summary>
        public const int MaxQuads = 2048;

        /// <summary>
        /// Builds the index pattern for <paramref name="quadCount"/> quads.
        /// </summary>
        /// <param name="quadCount">Number of quads to index.</param>
        /// <returns>Six indices per quad, rebased by four vertices per quad.</returns>
        public static short[] Build(int quadCount)
        {
            short[] indices = new short[quadCount * 6];
            for (int i = 0; i < quadCount; i++)
            {
                int vertex = i * 4;
                int index = i * 6;
                indices[index] = (short)vertex;
                indices[index + 1] = (short)(vertex + 1);
                indices[index + 2] = (short)(vertex + 2);
                indices[index + 3] = (short)(vertex + 2);
                indices[index + 4] = (short)(vertex + 1);
                indices[index + 5] = (short)(vertex + 3);
            }
            return indices;
        }
    }
}
