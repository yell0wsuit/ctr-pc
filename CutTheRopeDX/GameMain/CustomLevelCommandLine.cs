using System;
using System.IO;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Outcome of parsing custom-level command line arguments.
    /// </summary>
    /// <param name="IsCustomLevel">Whether the <c>--level</c> switch was present.</param>
    /// <param name="LevelPath">Absolute path to the requested level file, or <see langword="null"/> when unavailable.</param>
    /// <param name="ErrorMessage">Reason the arguments are unusable, or <see langword="null"/> when they are valid.</param>
    internal readonly record struct CustomLevelCommandLineResult(
        bool IsCustomLevel,
        string LevelPath,
        string ErrorMessage);

    /// <summary>
    /// Parses the custom-level command line switches. Performs no file access.
    /// </summary>
    internal static class CustomLevelCommandLine
    {
        /// <summary>Command line switch that selects a custom level file.</summary>
        public const string LevelSwitch = "--level";

        /// <summary>
        /// Parses command line arguments for the custom-level switch.
        /// </summary>
        /// <param name="args">Raw process arguments, excluding the executable name.</param>
        /// <returns>The parse outcome.</returns>
        public static CustomLevelCommandLineResult Parse(string[] args)
        {
            if (args == null)
            {
                return new CustomLevelCommandLineResult(false, null, null);
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], LevelSwitch, StringComparison.Ordinal))
                {
                    continue;
                }

                return i + 1 >= args.Length || string.IsNullOrWhiteSpace(args[i + 1])
                    ? new CustomLevelCommandLineResult(
                        true,
                        null,
                        LevelSwitch + " requires a path to a level XML file.")
                    : new CustomLevelCommandLineResult(true, Path.GetFullPath(args[i + 1]), null);
            }

            return new CustomLevelCommandLineResult(false, null, null);
        }
    }
}
