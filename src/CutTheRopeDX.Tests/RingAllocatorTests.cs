using CutTheRopeDX.Desktop;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class RingAllocatorTests
    {
        [Fact]
        public void SequentialAllocationsAdvanceContiguously()
        {
            RingAllocator ring = new(100);
            Assert.True(ring.TryAllocate(30, out int a, out bool wrappedA));
            Assert.True(ring.TryAllocate(30, out int b, out bool wrappedB));
            Assert.Equal(0, a);
            Assert.Equal(30, b);
            Assert.False(wrappedA);
            Assert.False(wrappedB);
        }

        [Fact]
        public void ExactFitDoesNotWrap()
        {
            RingAllocator ring = new(100);
            Assert.True(ring.TryAllocate(60, out _, out _));
            Assert.True(ring.TryAllocate(40, out int start, out bool wrapped));
            Assert.Equal(60, start);
            Assert.False(wrapped);
        }

        [Fact]
        public void OverflowWrapsToZeroWithWrapFlag()
        {
            RingAllocator ring = new(100);
            Assert.True(ring.TryAllocate(80, out _, out _));
            Assert.True(ring.TryAllocate(30, out int start, out bool wrapped));
            Assert.Equal(0, start);
            Assert.True(wrapped);
            Assert.Equal(30, ring.Cursor);
        }

        [Fact]
        public void AllocationAfterWrapContinuesFromWrappedRegion()
        {
            RingAllocator ring = new(100);
            Assert.True(ring.TryAllocate(80, out _, out _));
            Assert.True(ring.TryAllocate(30, out _, out _));
            Assert.True(ring.TryAllocate(20, out int start, out bool wrapped));
            Assert.Equal(30, start);
            Assert.False(wrapped);
        }

        [Fact]
        public void RequestLargerThanCapacityFails()
        {
            RingAllocator ring = new(100);
            Assert.False(ring.TryAllocate(101, out _, out _));
            Assert.Equal(0, ring.Cursor);
        }

        [Fact]
        public void FullCapacityRequestSucceeds()
        {
            RingAllocator ring = new(100);
            Assert.True(ring.TryAllocate(100, out int start, out bool wrapped));
            Assert.Equal(0, start);
            Assert.False(wrapped);
        }
    }
}
