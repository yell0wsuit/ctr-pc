using System;
using System.IO;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Resolves the root path where FFmpeg libraries are located.
    /// </summary>
    /// <remarks>
    /// On Windows and Linux, FFmpeg libraries are bundled in the "ffmpeg" subfolder.
    /// On macOS, they are bundled in the "Frameworks" folder inside the app bundle.
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
            if (OperatingSystem.IsMacOS())
            {
                return [Path.GetFullPath(Path.Combine(appBaseDirectory, "..", "Frameworks"))];
            }

            string ffmpegDir = Path.Combine(appBaseDirectory, "ffmpeg");

            if (OperatingSystem.IsWindows())
            {
                return
                [
                    ffmpegDir,
                    Path.Combine(appBaseDirectory, "runtimes", "win-x64", "native")
                ];
            }

            // Linux
            return [ffmpegDir];
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
