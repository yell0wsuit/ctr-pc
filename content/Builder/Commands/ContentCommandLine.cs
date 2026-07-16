namespace CutTheRopeDX.Content.Commands
{
    /// <summary>
    /// Identifies an operation supported by the content-builder executable.
    /// </summary>
    public enum ContentCommand
    {
        /// <summary>
        /// Runs the MonoGame content build.
        /// </summary>
        Build,

        /// <summary>
        /// Restores missing source assets.
        /// </summary>
        Fetch,

        /// <summary>
        /// Verifies local source assets against the manifest.
        /// </summary>
        Verify,
    }

    /// <summary>
    /// Contains parsed content-builder command-line arguments.
    /// </summary>
    /// <param name="Command">The selected command.</param>
    /// <param name="SourceDirectory">The source directory for asset commands.</param>
    /// <param name="BuilderArguments">Arguments forwarded to the MonoGame builder.</param>
    public sealed record ContentCommandLine(
        ContentCommand Command,
        string? SourceDirectory,
        IReadOnlyList<string> BuilderArguments)
    {
        /// <summary>
        /// Parses content-builder command-line arguments.
        /// </summary>
        /// <param name="arguments">Arguments supplied to the executable.</param>
        /// <returns>The parsed command line.</returns>
        public static ContentCommandLine Parse(IReadOnlyList<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return new ContentCommandLine(ContentCommand.Build, null, []);
            }

            ContentCommand? command = arguments[0] switch
            {
                "build" => ContentCommand.Build,
                "fetch" => ContentCommand.Fetch,
                "verify" => ContentCommand.Verify,
                _ => null,
            };

            if (command is null)
            {
                return new ContentCommandLine(ContentCommand.Build, null, [.. arguments]);
            }

            if (command == ContentCommand.Build)
            {
                return new ContentCommandLine(
                    ContentCommand.Build,
                    null,
                    [.. arguments.Skip(1)]);
            }

            string? sourceDirectory = null;

            for (int index = 1; index < arguments.Count; index++)
            {
                if (arguments[index] is "--source" or "-s")
                {
                    if (++index >= arguments.Count)
                    {
                        throw new ArgumentException("The source option requires a directory.");
                    }

                    sourceDirectory = arguments[index];
                }
                else
                {
                    throw new ArgumentException(
                        $"Unknown {command.Value.ToString().ToLowerInvariant()} option '{arguments[index]}'.");
                }
            }

            return new ContentCommandLine(command.Value, sourceDirectory, []);
        }
    }
}
