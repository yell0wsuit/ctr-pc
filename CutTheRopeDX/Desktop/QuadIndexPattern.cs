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

        /// <summary>
        /// Whether <paramref name="indices"/> describes <paramref name="quadCount"/> independent quads in
        /// the layout this batch draws, so the caller's vertices can be staged and drawn through the shared
        /// index buffer instead of getting a draw call of their own.
        /// </summary>
        /// <param name="indices">Index data submitted by the caller.</param>
        /// <param name="quadCount">Number of quads the caller is drawing.</param>
        /// <returns><see langword="true"/> when the vertices can be staged as batched quads.</returns>
        /// <remarks>
        /// Two windings of the second triangle are accepted, because both occur in the game: the one
        /// <see cref="Build"/> emits, and the mirrored one <c>ImageMultiDrawer</c> builds for the particle
        /// drawers. They cover the same three vertices, and cull mode is None, so they rasterize alike.
        /// Anything else is rejected rather than guessed at: a quad split along its other diagonal covers
        /// the same area but interpolates across it differently, which is not a substitution to make
        /// silently.
        /// </remarks>
        public static bool Matches(short[] indices, int quadCount)
        {
            if (indices == null || quadCount < 0 || indices.Length < quadCount * 6)
            {
                return false;
            }
            for (int quad = 0; quad < quadCount; quad++)
            {
                int index = quad * 6;
                int vertex = quad * 4;
                if (indices[index] != vertex
                    || indices[index + 1] != vertex + 1
                    || indices[index + 2] != vertex + 2)
                {
                    return false;
                }
                bool batchWinding = indices[index + 3] == vertex + 2
                    && indices[index + 4] == vertex + 1
                    && indices[index + 5] == vertex + 3;
                bool drawerWinding = indices[index + 3] == vertex + 3
                    && indices[index + 4] == vertex + 2
                    && indices[index + 5] == vertex + 1;
                if (!batchWinding && !drawerWinding)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
