using System;
using System.IO;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Resolves the root path where FFmpeg libraries are located on macOS.
    /// </summary>
    internal static class FfmpegRootPathResolver
    {
        /// <summary>
        /// FFmpeg library files required for video playback.
        /// </summary>
        private static readonly string[] RequiredLibraries =
        [
            "libavcodec.dylib",
            "libavformat.dylib",
            "libavutil.dylib",
            "libswscale.dylib",
            "libswresample.dylib"
        ];

        /// <summary>
        /// Searches for FFmpeg libraries in known locations and returns the first valid path.
        /// </summary>
        /// <param name="appBaseDirectory">The application's base directory.</param>
        /// <param name="directoryExists">Function to check if a directory exists.</param>
        /// <param name="fileExists">Function to check if a file exists.</param>
        /// <returns>
        /// The path to the directory containing FFmpeg libraries, or <c>null</c> if not found.
        /// </returns>
        /// <remarks>
        /// Searches the following locations in order:
        /// <list type="number">
        ///   <item><description>Homebrew ARM path: /opt/homebrew/opt/ffmpeg/lib</description></item>
        ///   <item><description>Homebrew Intel path: /usr/local/opt/ffmpeg/lib</description></item>
        ///   <item><description>App bundle Frameworks folder: ../Frameworks relative to app base</description></item>
        /// </list>
        /// </remarks>
        public static string Resolve(
            string appBaseDirectory,
            Func<string, bool> directoryExists,
            Func<string, bool> fileExists)
        {
            string frameworksPath = Path.GetFullPath(Path.Combine(appBaseDirectory, "..", "Frameworks"));

            string[] candidates =
            [
                "/opt/homebrew/opt/ffmpeg/lib",
                "/usr/local/opt/ffmpeg/lib",
                frameworksPath
            ];

            foreach (string candidate in candidates)
            {
                if (!directoryExists(candidate))
                {
                    continue;
                }

                bool hasAllLibraries = true;
                foreach (string lib in RequiredLibraries)
                {
                    if (!fileExists(Path.Combine(candidate, lib)))
                    {
                        hasAllLibraries = false;
                        break;
                    }
                }

                if (hasAllLibraries)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
