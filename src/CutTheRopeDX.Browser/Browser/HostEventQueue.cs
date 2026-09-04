using System;

namespace CutTheRopeDX.Browser
{
    /// <summary>The game thread's read side of the event ring.</summary>
    internal static unsafe class HostEventQueue
    {
        // One frame's worth of input is well under twenty records; this is sized for
        // a stalled frame rather than a typical one.
        private const int DrainLimit = 256;

        private static readonly HostEvent[] Drained = new HostEvent[DrainLimit];

        private static byte* _buffer;

        /// <summary>Allocates the ring and hands its address to the browser thread.</summary>
        public static void Initialize()
        {
            _buffer = (byte*)HostShim.EventBuffer(HostEventRing.BufferBytes);
            HostEventRing.Initialize(Span());
            HostEventInterop.Attach((int)_buffer);
        }

        /// <summary>Returns every event written since the last drain.</summary>
        public static ReadOnlySpan<HostEvent> Drain()
        {
            if (_buffer is null)
            {
                return default;
            }

            int count = HostEventRing.Drain(Span(), Drained);
            return Drained.AsSpan(0, count);
        }

        private static Span<byte> Span()
        {
            return new Span<byte>(_buffer, HostEventRing.BufferBytes);
        }
    }
}
