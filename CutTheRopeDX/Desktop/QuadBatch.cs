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
            if (QuadCount == 0)
            {
                Key = key;
            }
            int baseIndex = QuadCount * 4;
            for (int i = 0; i < 4; i++)
            {
                StagingArray[baseIndex + i] = QuadBaking.Bake(vertices[i], modelView, premultipliedTint);
            }
            QuadCount++;
        }

        /// <summary>Empties the batch without releasing the staging array.</summary>
        public void Clear()
        {
            QuadCount = 0;
        }
    }
}
