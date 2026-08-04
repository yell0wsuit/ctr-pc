using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Pure CPU accumulator for compatible sprite quads.
    /// </summary>
    internal sealed class QuadBatch
    {
        /// <summary>Maximum quads per batch.</summary>
        public const int Capacity = QuadIndexPattern.MaxQuads;

        /// <summary>Number of quads currently staged.</summary>
        public int QuadCount { get; private set; }

        /// <summary>Whether no quads are staged.</summary>
        public bool IsEmpty => QuadCount == 0;

        /// <summary>Whether the batch is at capacity.</summary>
        public bool IsFull => QuadCount == Capacity;

        /// <summary>How many more quads the batch can stage before it must be flushed.</summary>
        public int RemainingCapacity => Capacity - QuadCount;

        /// <summary>The compatibility key of the accumulated batch.</summary>
        public QuadBatchKey Key { get; private set; }

        /// <summary>The reusable vertex staging array.</summary>
        public VertexPositionColorTexture[] StagingArray { get; } = new VertexPositionColorTexture[Capacity * 4];

        /// <summary>Whether a quad with the given key may join the current batch.</summary>
        public bool CanAccept(in QuadBatchKey key)
        {
            return QuadCount == 0 || Key.Equals(key);
        }

        /// <summary>Transforms and stages one four-vertex sprite quad.</summary>
        public void Append(VertexPositionNormalTexture[] vertices, in QuadBatchKey key, in Matrix modelView, Color premultipliedTint)
        {
            AppendQuads(vertices, 1, key, modelView, premultipliedTint);
        }

        /// <summary>
        /// Transforms and stages <paramref name="quadCount"/> independent quads laid out four vertices at a
        /// time, the layout an indexed quad list already uses.
        /// </summary>
        /// <param name="vertices">Source vertices, four per quad.</param>
        /// <param name="quadCount">Number of quads to stage.</param>
        /// <param name="key">Compatibility key the batch adopts when it is empty.</param>
        /// <param name="modelView">Model-view matrix baked into each vertex position.</param>
        /// <param name="premultipliedTint">Tint baked into each vertex colour.</param>
        public void AppendQuads(VertexPositionNormalTexture[] vertices, int quadCount, in QuadBatchKey key, in Matrix modelView, Color premultipliedTint)
        {
            if (QuadCount == 0)
            {
                Key = key;
            }
            int destination = QuadCount * 4;
            int sourceCount = quadCount * 4;
            for (int i = 0; i < sourceCount; i++)
            {
                StagingArray[destination + i] = QuadBaking.Bake(vertices[i], modelView, premultipliedTint);
            }
            QuadCount += quadCount;
        }

        /// <summary>
        /// Transforms and stages a triangle strip of <paramref name="vertexCount"/> vertices as quads.
        /// </summary>
        /// <param name="vertices">Source vertices in strip order.</param>
        /// <param name="vertexCount">Number of strip vertices to stage. Must be even and at least four.</param>
        /// <param name="key">Compatibility key the batch adopts when it is empty.</param>
        /// <param name="modelView">Model-view matrix baked into each vertex position.</param>
        /// <param name="premultipliedTint">Tint baked into each vertex colour.</param>
        /// <remarks>
        /// A strip shares each pair of vertices with the quad that follows, so the pairs are copied twice
        /// and the staged vertex count grows from <paramref name="vertexCount"/> to twice
        /// <c>vertexCount - 2</c>. That is the price of the strip joining a batch at all, and transforming a
        /// handful of extra vertices is far cheaper than the draw call it saves.
        /// </remarks>
        public void AppendStrip(VertexPositionNormalTexture[] vertices, int vertexCount, in QuadBatchKey key, in Matrix modelView, Color premultipliedTint)
        {
            if (QuadCount == 0)
            {
                Key = key;
            }
            int quadCount = StripQuadCount(vertexCount);
            for (int quad = 0; quad < quadCount; quad++)
            {
                int source = quad * 2;
                int destination = (QuadCount + quad) * 4;
                for (int i = 0; i < 4; i++)
                {
                    StagingArray[destination + i] = QuadBaking.Bake(vertices[source + i], modelView, premultipliedTint);
                }
            }
            QuadCount += quadCount;
        }

        /// <summary>
        /// Number of quads a strip of <paramref name="vertexCount"/> vertices decomposes into, or zero when
        /// it cannot be expressed as whole quads.
        /// </summary>
        /// <param name="vertexCount">Number of strip vertices.</param>
        /// <returns>The quad count, or zero when the strip is not stageable.</returns>
        /// <remarks>
        /// An odd-length strip ends on a triangle with no partner, which no arrangement of quads covers, so
        /// it is left to take a draw call of its own.
        /// </remarks>
        public static int StripQuadCount(int vertexCount)
        {
            return vertexCount < 4 || vertexCount % 2 != 0 ? 0 : (vertexCount - 2) / 2;
        }

        /// <summary>Empties the batch without releasing the staging array.</summary>
        public void Clear()
        {
            QuadCount = 0;
        }
    }
}
