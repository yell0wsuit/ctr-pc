using System;

using CutTheRopeDX.Browser;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class HostEventRingTests
    {
        private static byte[] NewBuffer()
        {
            byte[] buffer = new byte[HostEventRing.BufferBytes];
            HostEventRing.Initialize(buffer);
            return buffer;
        }

        [Fact]
        public void DrainReturnsNothingWhenNothingWasWritten()
        {
            byte[] buffer = NewBuffer();
            HostEvent[] drained = new HostEvent[8];

            Assert.Equal(0, HostEventRing.Drain(buffer, drained));
        }

        [Fact]
        public void DrainReturnsWritesInOrder()
        {
            byte[] buffer = NewBuffer();
            Assert.True(HostEventRing.TryWrite(
                buffer, new HostEvent(HostEventKind.Wheel, 120, 0, 0, 0, 0)));
            Assert.True(HostEventRing.TryWrite(
                buffer, new HostEvent(HostEventKind.Wheel, -120, 0, 0, 0, 0)));

            HostEvent[] drained = new HostEvent[8];
            Assert.Equal(2, HostEventRing.Drain(buffer, drained));
            Assert.Equal(120, drained[0].Word0);
            Assert.Equal(-120, drained[1].Word0);
            Assert.Equal(HostEventKind.Wheel, drained[0].Kind);
        }

        [Fact]
        public void DrainedRecordsAreNotReturnedTwice()
        {
            byte[] buffer = NewBuffer();
            _ = HostEventRing.TryWrite(
                buffer, new HostEvent(HostEventKind.Active, 1, 0, 0, 0, 0));

            HostEvent[] drained = new HostEvent[8];
            Assert.Equal(1, HostEventRing.Drain(buffer, drained));
            Assert.Equal(0, HostEventRing.Drain(buffer, drained));
        }

        [Fact]
        public void FloatPayloadSurvivesTheRoundTrip()
        {
            byte[] buffer = NewBuffer();
            HostEvent written = new(
                HostEventKind.Pointer,
                1,
                BitConverter.SingleToInt32Bits(12.5f),
                BitConverter.SingleToInt32Bits(-3.25f),
                BitConverter.SingleToInt32Bits(748f),
                BitConverter.SingleToInt32Bits(472f));
            _ = HostEventRing.TryWrite(buffer, written);

            HostEvent[] drained = new HostEvent[1];
            _ = HostEventRing.Drain(buffer, drained);

            Assert.Equal(12.5f, BitConverter.Int32BitsToSingle(drained[0].Word1));
            Assert.Equal(-3.25f, BitConverter.Int32BitsToSingle(drained[0].Word2));
            Assert.Equal(748f, BitConverter.Int32BitsToSingle(drained[0].Word3));
            Assert.Equal(472f, BitConverter.Int32BitsToSingle(drained[0].Word4));
        }

        [Fact]
        public void WritesBeyondCapacityAreDroppedAndCounted()
        {
            byte[] buffer = NewBuffer();
            for (int index = 0; index < HostEventRing.Capacity; index++)
            {
                Assert.True(HostEventRing.TryWrite(
                    buffer, new HostEvent(HostEventKind.Wheel, index, 0, 0, 0, 0)));
            }

            Assert.False(HostEventRing.TryWrite(
                buffer, new HostEvent(HostEventKind.Wheel, 9999, 0, 0, 0, 0)));
            Assert.Equal(1, HostEventRing.DroppedCount(buffer));
        }

        [Fact]
        public void DrainStopsAtTheDestinationLengthAndResumesNextCall()
        {
            byte[] buffer = NewBuffer();
            for (int index = 0; index < 5; index++)
            {
                _ = HostEventRing.TryWrite(
                    buffer, new HostEvent(HostEventKind.Wheel, index, 0, 0, 0, 0));
            }

            HostEvent[] drained = new HostEvent[2];
            Assert.Equal(2, HostEventRing.Drain(buffer, drained));
            Assert.Equal(0, drained[0].Word0);
            Assert.Equal(2, HostEventRing.Drain(buffer, drained));
            Assert.Equal(2, drained[0].Word0);
            Assert.Equal(1, HostEventRing.Drain(buffer, drained));
            Assert.Equal(4, drained[0].Word0);
        }

        [Fact]
        public void IndicesSurviveWrappingPastCapacity()
        {
            byte[] buffer = NewBuffer();
            HostEvent[] drained = new HostEvent[4];

            for (int round = 0; round < HostEventRing.Capacity + 7; round++)
            {
                Assert.True(HostEventRing.TryWrite(
                    buffer, new HostEvent(HostEventKind.Wheel, round, 0, 0, 0, 0)));
                Assert.Equal(1, HostEventRing.Drain(buffer, drained));
                Assert.Equal(round, drained[0].Word0);
            }

            Assert.Equal(0, HostEventRing.DroppedCount(buffer));
        }
    }
}
