using System;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CutTheRope.Helpers.Discord
{
    /// <summary>
    /// Minimal Discord IPC client supporting only Rich Presence (SET_ACTIVITY).
    /// </summary>
    internal sealed class DiscordIpcClient(string clientId) : IDisposable
    {
        private const int OP_HANDSHAKE = 0;
        private const int OP_FRAME = 1;
        private const int OP_CLOSE = 2;

        private readonly string _clientId = clientId;
        private DiscordIpcConnection _connection;
        private int _nonce;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Connects to the local Discord client and performs the IPC handshake.
        /// </summary>
        /// <returns><see langword="true"/> if the handshake succeeded; otherwise <see langword="false"/>.</returns>
        public bool TryConnect()
        {
            try
            {
                _connection?.Dispose();
                _connection = new DiscordIpcConnection();
                if (!_connection.TryConnect())
                {
                    _connection.Dispose();
                    _connection = null;
                    return false;
                }

                // Send handshake
                byte[] handshake = BuildHandshakePayload();
                WriteFrame(OP_HANDSHAKE, handshake);

                // Read READY response
                if (!TryReadFrame(out int opcode, out string payload))
                {
                    _connection.Dispose();
                    _connection = null;
                    return false;
                }

                // Verify we got a FRAME with READY event
                if (opcode != OP_FRAME || !payload.Contains("\"READY\"", StringComparison.Ordinal))
                {
                    _connection.Dispose();
                    _connection = null;
                    return false;
                }

                IsConnected = true;
                return true;
            }
            catch
            {
                _connection?.Dispose();
                _connection = null;
                IsConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Sends a SET_ACTIVITY command to update the Rich Presence shown on Discord.
        /// </summary>
        /// <param name="details">Top line of the presence (e.g. level name).</param>
        /// <param name="state">Second line of the presence (e.g. star count).</param>
        /// <param name="startTimestamp">Unix epoch seconds for the elapsed-time counter.</param>
        /// <param name="smallImageKey">Asset key for the small image.</param>
        /// <param name="smallImageText">Tooltip text for the small image.</param>
        public void SetActivity(
            string details = null,
            string state = null,
            long? startTimestamp = null,
            string smallImageKey = null,
            string smallImageText = null)
        {
            if (!IsConnected || _connection?.Stream == null)
            {
                return;
            }

            try
            {
                byte[] payload = BuildSetActivityPayload(details, state, startTimestamp, smallImageKey, smallImageText);
                WriteFrame(OP_FRAME, payload);
            }
            catch
            {
                IsConnected = false;
            }
        }

        /// <summary>
        /// Clears the current Rich Presence from Discord.
        /// </summary>
        public void ClearActivity()
        {
            if (!IsConnected || _connection?.Stream == null)
            {
                return;
            }

            try
            {
                byte[] payload = BuildClearActivityPayload();
                WriteFrame(OP_FRAME, payload);
            }
            catch
            {
                IsConnected = false;
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                try
                {
                    if (IsConnected)
                    {
                        WriteFrame(OP_CLOSE, "{}"u8.ToArray());
                    }
                }
                catch
                {
                    // Best effort
                }

                _connection.Dispose();
                _connection = null;
            }

            IsConnected = false;
        }

        /// <summary>
        /// Writes a single IPC frame: 8-byte header (opcode + length) followed by the payload.
        /// </summary>
        /// <param name="opcode">The IPC opcode (handshake, frame, or close).</param>
        /// <param name="payload">The UTF-8 JSON payload bytes.</param>
        private void WriteFrame(int opcode, byte[] payload)
        {
            Span<byte> header = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(header[..4], opcode);
            BinaryPrimitives.WriteInt32LittleEndian(header[4..], payload.Length);

            Stream stream = _connection.Stream;
            stream.Write(header);
            stream.Write(payload);
            stream.Flush();
        }

        /// <summary>
        /// Reads a single IPC response frame from Discord.
        /// </summary>
        /// <param name="opcode">The opcode of the received frame.</param>
        /// <param name="payload">The JSON payload of the received frame.</param>
        /// <returns><see langword="true"/> if a frame was successfully read; otherwise <see langword="false"/>.</returns>
        private bool TryReadFrame(out int opcode, out string payload)
        {
            opcode = 0;
            payload = null;

            try
            {
                Stream stream = _connection.Stream;

                Span<byte> header = stackalloc byte[8];
                ReadExactly(stream, header);

                opcode = BinaryPrimitives.ReadInt32LittleEndian(header[..4]);
                int length = BinaryPrimitives.ReadInt32LittleEndian(header[4..]);

                if (length is <= 0 or > 65536)
                {
                    return false;
                }

                byte[] buf = new byte[length];
                ReadExactly(stream, buf);
                payload = Encoding.UTF8.GetString(buf);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads exactly <paramref name="buffer"/>.Length bytes from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="buffer">The buffer to fill.</param>
        /// <exception cref="EndOfStreamException">Thrown if the stream ends before the buffer is filled.</exception>
        private static void ReadExactly(Stream stream, Span<byte> buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = stream.Read(buffer[totalRead..]);
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }

                totalRead += read;
            }
        }

        /// <summary>
        /// Builds the JSON payload for the initial handshake: <c>{"v":1,"client_id":"..."}</c>.
        /// </summary>
        /// <returns>UTF-8 encoded JSON bytes.</returns>
        private byte[] BuildHandshakePayload()
        {
            using MemoryStream ms = new();
            using (Utf8JsonWriter w = new(ms))
            {
                w.WriteStartObject();
                w.WriteNumber("v", 1);
                w.WriteString("client_id", _clientId);
                w.WriteEndObject();
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Builds a SET_ACTIVITY payload with <c>"activity": null</c> to clear the presence.
        /// </summary>
        /// <returns>UTF-8 encoded JSON bytes.</returns>
        private byte[] BuildClearActivityPayload()
        {
            using MemoryStream ms = new();
            using (Utf8JsonWriter w = new(ms))
            {
                w.WriteStartObject();
                w.WriteString("cmd", "SET_ACTIVITY");

                w.WritePropertyName("args");
                w.WriteStartObject();
                w.WriteNumber("pid", Environment.ProcessId);
                w.WriteNull("activity");
                w.WriteEndObject(); // args

                w.WriteString("nonce", Interlocked.Increment(ref _nonce).ToString(CultureInfo.InvariantCulture));
                w.WriteEndObject();
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Builds the JSON-RPC payload for the SET_ACTIVITY command.
        /// </summary>
        /// <param name="details">Top line of the presence.</param>
        /// <param name="state">Second line of the presence.</param>
        /// <param name="startTimestamp">Unix epoch seconds for elapsed time.</param>
        /// <param name="smallImageKey">Asset key for the small image.</param>
        /// <param name="smallImageText">Tooltip text for the small image.</param>
        /// <returns>UTF-8 encoded JSON bytes.</returns>
        private byte[] BuildSetActivityPayload(
            string details, string state, long? startTimestamp,
            string smallImageKey, string smallImageText)
        {
            using MemoryStream ms = new();
            using (Utf8JsonWriter w = new(ms))
            {
                w.WriteStartObject();
                w.WriteString("cmd", "SET_ACTIVITY");

                w.WritePropertyName("args");
                w.WriteStartObject();
                w.WriteNumber("pid", Environment.ProcessId);

                bool hasActivity = details != null || state != null
                    || startTimestamp.HasValue || smallImageKey != null;

                if (hasActivity)
                {
                    w.WritePropertyName("activity");
                    w.WriteStartObject();

                    if (details != null)
                    {
                        w.WriteString("details", details);
                    }

                    if (state != null)
                    {
                        w.WriteString("state", state);
                    }

                    if (startTimestamp.HasValue)
                    {
                        w.WritePropertyName("timestamps");
                        w.WriteStartObject();
                        w.WriteNumber("start", startTimestamp.Value);
                        w.WriteEndObject();
                    }

                    if (smallImageKey != null)
                    {
                        w.WritePropertyName("assets");
                        w.WriteStartObject();
                        w.WriteString("small_image", smallImageKey);
                        if (smallImageText != null)
                        {
                            w.WriteString("small_text", smallImageText);
                        }

                        w.WriteEndObject();
                    }

                    w.WriteEndObject(); // activity
                }

                w.WriteEndObject(); // args

                w.WriteString("nonce", Interlocked.Increment(ref _nonce).ToString(CultureInfo.InvariantCulture));
                w.WriteEndObject();
            }
            return ms.ToArray();
        }
    }
}
