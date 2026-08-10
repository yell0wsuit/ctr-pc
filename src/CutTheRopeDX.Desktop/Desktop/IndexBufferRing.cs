using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// A ring-buffered DynamicIndexBuffer for 16-bit indices. Same write discipline as
    /// <see cref="VertexBufferRing{T}"/>: NoOverwrite at advancing offsets, Discard at zero.
    /// </summary>
    /// <param name="capacityIndices">Total index capacity of the ring.</param>
    internal sealed class IndexBufferRing(int capacityIndices)
    {
        /// <summary>
        /// The underlying GPU buffer to bind for draws.
        /// </summary>
        public DynamicIndexBuffer Buffer { get; } = new(Global.GraphicsDevice, IndexElementSize.SixteenBits, capacityIndices, BufferUsage.WriteOnly);

        /// <summary>
        /// Uploads <paramref name="count"/> indices and returns the start index to draw from.
        /// </summary>
        /// <param name="data">Source index data.</param>
        /// <param name="count">Number of indices to upload.</param>
        /// <returns>The start index offset, or -1 when the write exceeds the ring's
        /// total capacity and the caller must use a fallback upload.</returns>
        public int Write(short[] data, int count)
        {
            if (!_allocator.TryAllocate(count, out int start, out _))
            {
                return -1;
            }
            SetDataOptions options = start == 0 ? SetDataOptions.Discard : SetDataOptions.NoOverwrite;
            Buffer.SetData(start * sizeof(short), data, 0, count, options);
            return start;
        }

        /// <summary>
        /// Cursor arithmetic for the ring.
        /// </summary>
        private readonly RingAllocator _allocator = new(capacityIndices);
    }
}
