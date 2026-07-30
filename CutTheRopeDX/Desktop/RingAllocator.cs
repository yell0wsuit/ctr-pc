namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Pure-CPU cursor arithmetic for ring-buffer allocation. Hands out contiguous
    /// element regions, wrapping to offset zero when the remaining space is too small.
    /// A wrapped region signals the caller to orphan the GPU buffer with Discard.
    /// </summary>
    /// <param name="capacity">Total element capacity of the ring.</param>
    internal sealed class RingAllocator(int capacity)
    {
        /// <summary>
        /// Total element capacity of the ring.
        /// </summary>
        public int Capacity { get; } = capacity;

        /// <summary>
        /// Next free element offset.
        /// </summary>
        public int Cursor { get; private set; }

        /// <summary>
        /// Allocates a contiguous region of <paramref name="count"/> elements.
        /// </summary>
        /// <param name="count">Number of elements to allocate.</param>
        /// <param name="start">Start offset of the allocated region.</param>
        /// <param name="wrapped">Whether the ring wrapped to offset zero for this region;
        /// the caller must use Discard for the corresponding GPU write.</param>
        /// <returns><see langword="false"/> when <paramref name="count"/> exceeds the ring's
        /// total capacity; the caller must use a fallback upload path.</returns>
        public bool TryAllocate(int count, out int start, out bool wrapped)
        {
            if (count > Capacity)
            {
                start = 0;
                wrapped = false;
                return false;
            }
            if (Cursor + count > Capacity)
            {
                start = 0;
                Cursor = count;
                wrapped = true;
                return true;
            }
            start = Cursor;
            Cursor += count;
            wrapped = false;
            return true;
        }
    }
}
