using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CutTheRopeDX.GameMain
{
    /// <summary>The kinds of message the browser playtest channel carries.</summary>
    internal enum PlaytestMessageKind
    {
        /// <summary>Not a message this protocol version understands.</summary>
        Unknown = 0,

        /// <summary>Game to editor: a playtest session booted and is waiting for its level.</summary>
        Ready,

        /// <summary>Editor to game: the level XML to run.</summary>
        Level,

        /// <summary>Game to editor: the level could not be loaded.</summary>
        Error,

        /// <summary>Game to editor: the playtest window is going away.</summary>
        Bye,
    }

    /// <summary>
    /// Encodes and decodes the JSON the editor and the browser build exchange over the
    /// <c>ctrdx-playtest</c> BroadcastChannel.
    /// </summary>
    /// <remarks>
    /// A browser tab has no command line, so this channel replaces <c>--level</c>. The launch URL's
    /// <c>?playtest=</c> parameter only marks the session; the level itself arrives as a
    /// <see cref="PlaytestMessageKind.Level"/> message. The editor implements the same format, and the
    /// literal wire strings asserted by both repositories' tests are the contract between them.
    /// </remarks>
    internal static class PlaytestChannelMessage
    {
        /// <summary>Protocol version stamped on, and required of, every message.</summary>
        public const int Version = 1;

        // Relaxed escaping keeps a level from ballooning: the default encoder escapes '<' and '>' as
        // \u003C and \u003E, and level XML is mostly those two characters. (The double quote is not a
        // reason to relax - JSON escapes it as \" under either encoder.) Relaxing is safe here because
        // these strings are never interpolated into HTML: they go to postMessage and back through
        // JSON.parse.
        private static readonly JsonWriterOptions WriterOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>Builds the handshake a booting playtest session announces itself with.</summary>
        /// <param name="nonce">Session nonce from the launch URL, echoed so only its own editor answers.</param>
        /// <param name="handshakeLine">A line from <see cref="PlaytestHandshake.FormatLine"/>.</param>
        /// <returns>JSON to post on the channel.</returns>
        public static string FormatReady(string nonce, string handshakeLine)
        {
            return Write("ready", nonce, "line", handshakeLine);
        }

        /// <summary>Builds the message carrying a level to a playtest session.</summary>
        /// <param name="nonce">Session nonce the target window announced.</param>
        /// <param name="xml">Serialized level XML.</param>
        /// <returns>JSON to post on the channel.</returns>
        public static string FormatLevel(string nonce, string xml)
        {
            return Write("level", nonce, "xml", xml);
        }

        /// <summary>Builds the message reporting a level the game could not load.</summary>
        /// <param name="nonce">Session nonce, so only the editor that opened this window reacts.</param>
        /// <param name="message">Human-readable failure reason.</param>
        /// <returns>JSON to post on the channel.</returns>
        public static string FormatError(string nonce, string message)
        {
            return Write("error", nonce, "message", message);
        }

        /// <summary>Builds the message announcing that a playtest window is going away.</summary>
        /// <param name="nonce">Session nonce, so only the editor that opened this window reacts.</param>
        /// <returns>JSON to post on the channel.</returns>
        public static string FormatBye(string nonce)
        {
            return Write("bye", nonce, null, null);
        }

        /// <summary>Decodes one channel message.</summary>
        /// <param name="json">The raw string received on the channel.</param>
        /// <param name="kind">Receives the message kind, or <see cref="PlaytestMessageKind.Unknown"/> on failure.</param>
        /// <param name="nonce">Receives the session nonce, or an empty string when absent.</param>
        /// <param name="payload">Receives the kind's single string field, or an empty string when absent.</param>
        /// <returns><see langword="true"/> for a message this build understands.</returns>
        /// <remarks>
        /// A wrong version, an unknown type and malformed JSON all return <see langword="false"/> rather
        /// than throwing, so a future protocol talking to this build degrades to silence.
        /// </remarks>
        public static bool TryParse(string json, out PlaytestMessageKind kind, out string nonce, out string payload)
        {
            kind = PlaytestMessageKind.Unknown;
            nonce = "";
            payload = "";

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (!root.TryGetProperty("v", out JsonElement version)
                    || version.ValueKind != JsonValueKind.Number
                    || !version.TryGetInt32(out int parsedVersion)
                    || parsedVersion != Version)
                {
                    return false;
                }

                if (!root.TryGetProperty("type", out JsonElement type) || type.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                switch (type.GetString())
                {
                    case "ready":
                        kind = PlaytestMessageKind.Ready;
                        payload = ReadString(root, "line");
                        break;
                    case "level":
                        kind = PlaytestMessageKind.Level;
                        payload = ReadString(root, "xml");
                        break;
                    case "error":
                        kind = PlaytestMessageKind.Error;
                        payload = ReadString(root, "message");
                        break;
                    case "bye":
                        kind = PlaytestMessageKind.Bye;
                        break;
                    default:
                        return false;
                }

                nonce = ReadString(root, "nonce");
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>Reads an optional string property, treating absence and wrong type alike.</summary>
        /// <param name="root">The message object.</param>
        /// <param name="name">Property name.</param>
        /// <returns>The value, or an empty string.</returns>
        private static string ReadString(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : "";
        }

        /// <summary>Serializes one message. Property order is the wire contract, so it is fixed here.</summary>
        /// <param name="type">The <c>type</c> discriminator.</param>
        /// <param name="nonce">Session nonce, or <see langword="null"/> to omit the property.</param>
        /// <param name="fieldName">Name of the kind's single payload property, or <see langword="null"/> to omit it.</param>
        /// <param name="fieldValue">Value for <paramref name="fieldName"/>.</param>
        /// <returns>The JSON text.</returns>
        private static string Write(string type, string nonce, string fieldName, string fieldValue)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream, WriterOptions))
            {
                writer.WriteStartObject();
                writer.WriteNumber("v", Version);
                writer.WriteString("type", type);
                if (nonce != null)
                {
                    writer.WriteString("nonce", nonce);
                }
                if (fieldName != null)
                {
                    writer.WriteString(fieldName, fieldValue);
                }
                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
