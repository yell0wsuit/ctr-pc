using System;
using System.IO;

namespace CutTheRope.Framework.Media
{
    internal static class FfmpegRootPathResolver
    {
        private static readonly string[] RequiredLibraries =
        [
            "libavcodec.dylib",
            "libavformat.dylib",
            "libavutil.dylib",
            "libswscale.dylib",
            "libswresample.dylib"
        ];

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
