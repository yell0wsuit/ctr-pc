using System;
using System.IO;
using System.Reflection;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Announces this build's identity and version to a launcher when a custom level is accepted.
    /// </summary>
    /// <remarks>
    /// The editor starts the game with <c>--level</c> but otherwise cannot tell Cut the Rope: DX from
    /// any other program, nor a build that understands the switch from an older one that silently
    /// ignores it and opens the normal menu - both exit 0 with no error. This handshake closes both
    /// gaps at once: the moment a custom level loads, the game writes a single identifying line to
    /// standard output. A launcher that sees the line within a short window knows it launched Cut the
    /// Rope: DX (the <see cref="Signature"/> token) and which version it is (for feature gating); the
    /// line's absence means the target is not Cut the Rope: DX, or is too old to playtest.
    /// </remarks>
    internal static class PlaytestHandshake
    {
        /// <summary>Fixed token identifying the emitter as playtest-capable Cut the Rope: DX. Stable contract with the editor.</summary>
        public const string Signature = "ctrdx-playtest";

        /// <summary>Handshake format version. Bumped when the line's shape or meaning changes.</summary>
        public const int Protocol = 1;

        /// <summary>Placeholder emitted when the build carries no readable version.</summary>
        public const string UnknownVersion = "unknown";

        /// <summary>Builds the handshake line for a given application version.</summary>
        /// <param name="version">The build's version string, echoed for the launcher's version check.</param>
        /// <returns>A single line of the form <c>ctrdx-playtest &lt;protocol&gt; &lt;version&gt;</c>.</returns>
        public static string FormatLine(string version)
        {
            string safe = string.IsNullOrWhiteSpace(version) ? UnknownVersion : version.Trim();
            return $"{Signature} {Protocol} {safe}";
        }

        /// <summary>Writes the handshake line for this build to <paramref name="output"/> and flushes it.</summary>
        /// <remarks>
        /// Flushed explicitly: the game blocks in its run loop straight after this call, so an unflushed
        /// line could sit in the buffer for the whole session and the launcher would time out waiting.
        /// </remarks>
        /// <param name="output">Destination writer, normally <see cref="Console.Out"/>.</param>
        public static void Announce(TextWriter output)
        {
            output.WriteLine(FormatLine(ResolveVersion()));
            output.Flush();
        }

        // The informational version carries the most detail - it includes the source revision when the
        // build is not a clean tag - which is exactly what a launcher wants for a precise version check.
        // Falls back to the plain assembly version, then to empty, which FormatLine renders as "unknown".
        private static string ResolveVersion()
        {
            Assembly assembly = typeof(PlaytestHandshake).Assembly;
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "";
        }
    }
}
