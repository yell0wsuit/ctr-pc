using System;
using System.Buffers.Binary;
using System.Threading;

namespace CutTheRopeDX.Browser
{
    /// <summary>Keys the browser thread forwards, numbered so no strings cross the ring.</summary>
    internal enum HostKey
    {
        /// <summary>No key.</summary>
        None = 0,

        /// <summary>Back, sent as Q because a browser keeps Escape for itself.</summary>
        Escape = 1,

        /// <summary>Reload, sent as R because a browser keeps F5 for itself.</summary>
        F5 = 2,

        /// <summary>Space.</summary>
        Space = 3,

        /// <summary>Enter.</summary>
        Enter = 4,

        /// <summary>Left arrow.</summary>
        Left = 5,

        /// <summary>Right arrow.</summary>
        Right = 6,
    }

    /// <summary>What a host event carries.</summary>
    internal enum HostEventKind
    {
        /// <summary>Unused slot.</summary>
        None = 0,

        /// <summary>Phase, then CSS offset x and y, then the rect width and height.</summary>
        Pointer = 1,

        /// <summary>Down flag, then the mapped key id.</summary>
        Key = 2,

        /// <summary>Wheel movement in the desktop's units.</summary>
        Wheel = 3,

        /// <summary>Combined visibility and focus state.</summary>
        Active = 4,

        /// <summary>CSS width, CSS height, then the device pixel ratio.</summary>
        Resize = 5,

        /// <summary>The player pressed Play. Carries nothing.</summary>
        Start = 6,
    }

    /// <summary>One record as it travels through the ring.</summary>
    /// <remarks>
    /// The payload is five raw words because the kinds disagree about their shape and
    /// the record has to stay a fixed size. Float payloads are carried as their bit
    /// patterns; <see cref="BitConverter.Int32BitsToSingle"/> reads them back.
    /// </remarks>
    internal readonly record struct HostEvent(
        HostEventKind Kind,
        int Word0,
        int Word1,
        int Word2,
        int Word3,
        int Word4);

    /// <summary>
    /// A single-writer, single-reader ring in shared wasm memory. The browser thread
    /// writes it and the owner thread drains it once per frame.
    /// </summary>
    /// <remarks>
    /// Both indices count records rather than addressing slots, so the reader can tell
    /// a full ring from an empty one without a spare slot. The slot is the index masked
    /// by the capacity, which is why the capacity is a power of two.
    /// </remarks>
    internal static class HostEventRing
    {
        /// <summary>Records the ring holds before it starts dropping.</summary>
        internal const int Capacity = 1024;

        /// <summary>Bytes of header ahead of the first record.</summary>
        internal const int HeaderBytes = 16;

        /// <summary>Bytes per record: the kind and five payload words.</summary>
        internal const int RecordBytes = 24;

        private const int WriteIndexOffset = 0;
        private const int ReadIndexOffset = 4;
        private const int DroppedOffset = 8;
        private const int CapacityOffset = 12;

        /// <summary>Total bytes the ring occupies.</summary>
        internal static int BufferBytes => HeaderBytes + (RecordBytes * Capacity);

        /// <summary>Zeroes the header and records the capacity the writer must honor.</summary>
        internal static void Initialize(Span<byte> buffer)
        {
            buffer[..HeaderBytes].Clear();
            BinaryPrimitives.WriteInt32LittleEndian(
                buffer[CapacityOffset..], Capacity);
        }

        /// <summary>Appends one record, or reports that the ring was full.</summary>
        internal static bool TryWrite(Span<byte> buffer, in HostEvent value)
        {
            int write = BinaryPrimitives.ReadInt32LittleEndian(
                buffer[WriteIndexOffset..]);
            int read = Volatile.Read(
                ref AsInt(buffer, ReadIndexOffset));

            if (write - read >= Capacity)
            {
                int dropped = BinaryPrimitives.ReadInt32LittleEndian(
                    buffer[DroppedOffset..]);
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer[DroppedOffset..], dropped + 1);
                return false;
            }

            Span<byte> record = buffer.Slice(SlotOffset(write), RecordBytes);
            BinaryPrimitives.WriteInt32LittleEndian(record, (int)value.Kind);
            BinaryPrimitives.WriteInt32LittleEndian(record[4..], value.Word0);
            BinaryPrimitives.WriteInt32LittleEndian(record[8..], value.Word1);
            BinaryPrimitives.WriteInt32LittleEndian(record[12..], value.Word2);
            BinaryPrimitives.WriteInt32LittleEndian(record[16..], value.Word3);
            BinaryPrimitives.WriteInt32LittleEndian(record[20..], value.Word4);

            // Published last, so a reader never sees a slot before it is filled.
            Volatile.Write(ref AsInt(buffer, WriteIndexOffset), write + 1);
            return true;
        }

        /// <summary>Copies out every record written since the last drain.</summary>
        /// <returns>How many records were written into <paramref name="into"/>.</returns>
        internal static int Drain(Span<byte> buffer, Span<HostEvent> into)
        {
            int write = Volatile.Read(ref AsInt(buffer, WriteIndexOffset));
            int read = BinaryPrimitives.ReadInt32LittleEndian(
                buffer[ReadIndexOffset..]);

            int available = write - read;
            int count = available < into.Length ? available : into.Length;
            for (int index = 0; index < count; index++)
            {
                ReadOnlySpan<byte> record =
                    buffer.Slice(SlotOffset(read + index), RecordBytes);
                into[index] = new HostEvent(
                    (HostEventKind)BinaryPrimitives.ReadInt32LittleEndian(record),
                    BinaryPrimitives.ReadInt32LittleEndian(record[4..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[8..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[12..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[16..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[20..]));
            }

            Volatile.Write(ref AsInt(buffer, ReadIndexOffset), read + count);
            return count;
        }

        /// <summary>How many records the writer has had to throw away.</summary>
        internal static int DroppedCount(ReadOnlySpan<byte> buffer)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(buffer[DroppedOffset..]);
        }

        private static int SlotOffset(int index)
        {
            return HeaderBytes + (((int)((uint)index & (Capacity - 1))) * RecordBytes);
        }

        private static ref int AsInt(Span<byte> buffer, int offset)
        {
            return ref System.Runtime.InteropServices.MemoryMarshal
                .Cast<byte, int>(buffer.Slice(offset, 4))[0];
        }
    }
}
