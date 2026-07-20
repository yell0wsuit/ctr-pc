using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// A ring-buffered DynamicVertexBuffer for one vertex type. Writes advance through
    /// the buffer with NoOverwrite so no upload rewrites a region the GPU may still be
    /// reading; writes at offset zero (first use or wrap) orphan the buffer with Discard.
    /// </summary>
    /// <typeparam name="T">The vertex type stored in the buffer.</typeparam>
    /// <param name="capacityVertices">Total vertex capacity of the ring.</param>
    internal sealed class VertexBufferRing<T>(int capacityVertices) where T : struct, IVertexType
    {
        /// <summary>
        /// The underlying GPU buffer to bind for draws.
        /// </summary>
        public DynamicVertexBuffer Buffer { get; } = new(Global.GraphicsDevice, default(T).VertexDeclaration, capacityVertices, BufferUsage.WriteOnly);

        /// <summary>
        /// Uploads <paramref name="count"/> vertices and returns the start vertex to draw from.
        /// </summary>
        /// <param name="data">Source vertex data.</param>
        /// <param name="count">Number of vertices to upload.</param>
        /// <returns>The start vertex offset, or -1 when the write exceeds the ring's
        /// total capacity and the caller must use a fallback upload.</returns>
        public int Write(T[] data, int count)
        {
            if (!_allocator.TryAllocate(count, out int start, out _))
            {
                return -1;
            }
            SetDataOptions options = start == 0 ? SetDataOptions.Discard : SetDataOptions.NoOverwrite;
            Buffer.SetData(start * _stride, data, 0, count, _stride, options);
            return start;
        }

        /// <summary>
        /// Cursor arithmetic for the ring.
        /// </summary>
        private readonly RingAllocator _allocator = new(capacityVertices);

        /// <summary>
        /// Vertex stride in bytes.
        /// </summary>
        private readonly int _stride = default(T).VertexDeclaration.VertexStride;
    }
}
