using System;
using System.IO;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Outcome of parsing the game's command line arguments.
    /// </summary>
    /// <param name="IsCustomLevel">Whether the <c>--level</c> switch was present.</param>
    /// <param name="LevelPath">Absolute path to the requested level file, or <see langword="null"/> when unavailable.</param>
    /// <param name="ErrorMessage">Reason the arguments are unusable, or <see langword="null"/> when they are valid.</param>
    /// <param name="IsHeadless">Whether the <c>--headless</c> switch was present.</param>
    internal readonly record struct CommandLineResult(
        bool IsCustomLevel,
        string LevelPath,
        string ErrorMessage,
        bool IsHeadless);

    /// <summary>
    /// Parses the game's command line switches. Performs no file access.
    /// </summary>
    /// <remarks>
    /// A bare <c>.xml</c> path is also accepted so a level file can be dropped onto the executable,
    /// which is how Windows Explorer hands the path over.
    /// </remarks>
    internal static class CommandLine
    {
        /// <summary>Command line switch that selects a custom level file.</summary>
        public const string LevelSwitch = "--level";

        /// <summary>Command line switch that runs the game without a window or graphics device.</summary>
        public const string HeadlessSwitch = "--headless";

        /// <summary>
        /// Parses command line arguments for the supported switches.
        /// </summary>
        /// <param name="args">Raw process arguments, excluding the executable name.</param>
        /// <returns>The parse outcome.</returns>
        public static CommandLineResult Parse(string[] args)
        {
            if (args == null)
            {
                return new CommandLineResult(false, null, null, false);
            }

            bool isHeadless = Array.Exists(
                args,
                arg => string.Equals(arg, HeadlessSwitch, StringComparison.Ordinal));

            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], LevelSwitch, StringComparison.Ordinal))
                {
                    continue;
                }

                return i + 1 >= args.Length || string.IsNullOrWhiteSpace(args[i + 1])
                    ? new CommandLineResult(
                        true,
                        null,
                        LevelSwitch + " requires a path to a level XML file.",
                        isHeadless)
                    : new CommandLineResult(true, Path.GetFullPath(args[i + 1]), null, isHeadless);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (string.IsNullOrWhiteSpace(arg)
                    || arg.StartsWith('-')
                    || !arg.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return new CommandLineResult(true, Path.GetFullPath(arg), null, isHeadless);
            }

            return new CommandLineResult(false, null, null, isHeadless);
        }
    }
}
