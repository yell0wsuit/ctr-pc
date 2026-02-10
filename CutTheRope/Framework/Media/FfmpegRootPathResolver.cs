using System;
using System.IO;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Resolves the root path where FFmpeg libraries are located.
    /// </summary>
    /// <remarks>
    /// On Windows, FFmpeg DLLs are bundled via the FFmpeg.GPL NuGet package and placed
    /// next to the executable. On Linux, system-installed libraries are used. On macOS,
    /// Homebrew or bundled Frameworks are searched.
    /// </remarks>
    internal static class FfmpegRootPathResolver
    {
        /// <summary>
        /// Searches for FFmpeg libraries in known locations and returns the first valid path.
        /// </summary>
        /// <param name="appBaseDirectory">The application's base directory.</param>
        /// <param name="directoryExists">Function to check if a directory exists.</param>
        /// <returns>
        /// The path to the directory containing FFmpeg libraries, or <c>null</c> if not found.
        /// </returns>
        public static string Resolve(
            string appBaseDirectory,
            Func<string, bool> directoryExists)
        {
            string[] candidates = GetCandidatePaths(appBaseDirectory);
            string probeLibrary = GetProbeLibrary();

            foreach (string candidate in candidates)
            {
                if (!directoryExists(candidate))
                {
                    continue;
                }

                if (Directory.GetFiles(candidate, probeLibrary).Length > 0)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the platform-specific candidate directories to search for FFmpeg libraries.
        /// </summary>
        private static string[] GetCandidatePaths(string appBaseDirectory)
        {
            if (OperatingSystem.IsWindows())
            {
                return
                [
                    appBaseDirectory,
                    Path.Combine(appBaseDirectory, "ffmpeg"),
                    Path.Combine(appBaseDirectory, "runtimes", "win-x64", "native")
                ];
            }

            if (OperatingSystem.IsLinux())
            {
                return
                [
                    appBaseDirectory,
                    Path.Combine(appBaseDirectory, "ffmpeg"),
                    "/usr/lib/x86_64-linux-gnu",
                    "/usr/lib64",
                    "/usr/lib",
                    "/usr/local/lib"
                ];
            }

            // macOS
            string frameworksPath = Path.GetFullPath(Path.Combine(appBaseDirectory, "..", "Frameworks"));
            return
            [
                "/opt/homebrew/opt/ffmpeg/lib",
                "/usr/local/opt/ffmpeg/lib",
                frameworksPath
            ];
        }

        /// <summary>
        /// Gets a library name pattern to probe for FFmpeg presence.
        /// </summary>
        private static string GetProbeLibrary()
        {
            return OperatingSystem.IsWindows() ? "avcodec-*.dll" : OperatingSystem.IsLinux() ? "libavcodec.so*" : "libavcodec*.dylib";
        }
    }
}
