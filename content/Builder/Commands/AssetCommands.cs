using CutTheRopeDX.Content.Assets;

using Microsoft.Xna.Framework.Content.Pipeline;

using MonoGame.Framework.Content.Pipeline.Builder;

namespace CutTheRopeDX.Content.Commands
{
    /// <summary>
    /// Executes content acquisition, verification, and build commands.
    /// </summary>
    public static class AssetCommands
    {
        private const string AssetsUrl =
            "https://github.com/yell0wsuit/ctrdx-assets/releases/latest/download/ctrdx-assets-vk.zip";
        private const string ManifestName = "file_manifest.json";

        /// <summary>
        /// Executes a parsed content-builder command.
        /// </summary>
        /// <param name="commandLine">Parsed command-line arguments.</param>
        /// <returns>The process exit code.</returns>
        public static async Task<int> RunAsync(ContentCommandLine commandLine)
        {
            try
            {
                return commandLine.Command switch
                {
                    ContentCommand.Build => RunBuild(commandLine.BuilderArguments),
                    ContentCommand.Fetch => await RunFetchAsync(
                        ResolveSourceDirectory(commandLine.SourceDirectory)),
                    ContentCommand.Verify => RunVerify(
                        ResolveSourceDirectory(commandLine.SourceDirectory)),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(commandLine),
                        commandLine.Command,
                        "Unknown content command."),
                };
            }
            catch (Exception exception) when (
                exception is ArgumentException or
                    HttpRequestException or
                    TaskCanceledException or
                    IOException or
                    InvalidDataException)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static int RunBuild(IReadOnlyList<string> arguments)
        {
            GameContentBuilder builder = new();

            if (arguments.Count > 0)
            {
                builder.Run([.. arguments]);
            }
            else
            {
                _ = builder.Run(new ContentBuilderParams
                {
                    Mode = ContentBuilderMode.Builder,
                    WorkingDirectory = $"{AppContext.BaseDirectory}../../",
                    SourceDirectory = "content",
                    Platform = TargetPlatform.DesktopVK,
                    CompressContent = true,
                });
            }

            if (builder.FailedToBuild > 0)
            {
                return -1;
            }

            EmitImageDimensionsManifest(arguments);
            return 0;
        }

        /// <summary>
        /// Emits the headless image-dimensions manifest next to the built image assets.
        /// Only the argument-driven build (the one MSBuild runs) carries explicit source and
        /// output directories; a bare invocation resolves them itself and is skipped with a note.
        /// </summary>
        /// <param name="arguments">Arguments forwarded to the MonoGame builder.</param>
        private static void EmitImageDimensionsManifest(IReadOnlyList<string> arguments)
        {
            string? sourceDirectory = FindOptionValue(arguments, "-s", "--source");
            string? outputDirectory = FindOptionValue(arguments, "-o", "--output");

            if (sourceDirectory is null || outputDirectory is null)
            {
                Console.WriteLine(
                    "[I] Skipping image_dimensions.json: build was invoked without -s/-o.");
                return;
            }

            GameContentBuilder.EmitImageDimensionsManifest(
                Path.Combine(sourceDirectory, "images"),
                Path.Combine(outputDirectory, "images"));
        }

        private static string? FindOptionValue(
            IReadOnlyList<string> arguments,
            string shortName,
            string longName)
        {
            for (int index = 0; index < arguments.Count - 1; index++)
            {
                if (arguments[index] == shortName || arguments[index] == longName)
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }

        private static async Task<int> RunFetchAsync(string contentDirectory)
        {
            AssetManifest manifest = ReadRequiredManifest(contentDirectory);
            AssetStore store = new(contentDirectory, manifest);
            IReadOnlyList<string> missing = store.FindMissing();

            if (missing.Count == 0)
            {
                return 0;
            }

            Console.WriteLine(
                $"Content assets missing ({missing.Count}/{manifest.Files.Count} listed) — fetching.");
            string temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"ctrdx-assets-fetch-{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(temporaryDirectory);

            try
            {
                string archivePath = Path.Combine(temporaryDirectory, "ctrdx-assets-vk.zip");
                using AssetDownloader downloader = new();
                Console.WriteLine($"Downloading content assets from {AssetsUrl}...");
                await downloader.DownloadAsync(AssetsUrl, archivePath, retries: 3);

                AssetArchive archive = new(archivePath, manifest);
                AssetVerificationResult verification = archive.Verify();

                if (!verification.Success)
                {
                    WriteVerificationFailures(verification);
                    Console.Error.WriteLine(
                        $"Downloaded bundle doesn't match {ManifestName}; aborting.");
                    return 1;
                }

                archive.Install(contentDirectory, missing);
                Console.WriteLine(
                    $"Restored {missing.Count} missing binary asset(s) into {contentDirectory}.");
                return 0;
            }
            finally
            {
                try
                {
                    Directory.Delete(temporaryDirectory, recursive: true);
                }
                catch
                {
                    // Best-effort cleanup.
                }
            }
        }

        private static int RunVerify(string contentDirectory)
        {
            AssetManifest manifest = ReadRequiredManifest(contentDirectory);
            AssetVerificationResult verification =
                new AssetStore(contentDirectory, manifest).Verify();

            if (verification.Success)
            {
                Console.WriteLine(
                    $"All {manifest.Files.Count} content assets verified OK.");
                return 0;
            }

            WriteVerificationFailures(verification);
            Console.Error.WriteLine(
                $"Verify failed: {verification.Missing.Count} missing, " +
                $"{verification.Mismatched.Count} mismatched of {manifest.Files.Count}.");
            return 1;
        }

        private static AssetManifest ReadRequiredManifest(string contentDirectory)
        {
            string manifestPath = Path.Combine(contentDirectory, ManifestName);

            return File.Exists(manifestPath)
                ? AssetManifest.Read(manifestPath)
                : throw new FileNotFoundException(
                    $"No {ManifestName} found in {contentDirectory}.",
                    manifestPath);
        }

        private static string ResolveSourceDirectory(string? sourceDirectory)
        {
            return Path.GetFullPath(sourceDirectory ?? "content");
        }

        private static void WriteVerificationFailures(
            AssetVerificationResult verification)
        {
            foreach (string path in verification.Missing)
            {
                Console.Error.WriteLine($"missing:  {path}");
            }

            foreach (string path in verification.Mismatched)
            {
                Console.Error.WriteLine($"mismatch: {path}");
            }
        }
    }
}
