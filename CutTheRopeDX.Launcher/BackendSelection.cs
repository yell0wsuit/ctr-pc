using System;

using CutTheRopeDX.Launcher.Graphics;

namespace CutTheRopeDX.Launcher
{
    /// <summary>
    /// Graphics backend a build was produced for.
    /// </summary>
    public enum GraphicsBackend
    {
        /// <summary>Vulkan, through MonoGame's native runtime.</summary>
        Vulkan,

        /// <summary>OpenGL, through MonoGame's DesktopGL framework.</summary>
        OpenGl,
    }

    /// <summary>
    /// Chooses which of the shipped game builds to run.
    /// </summary>
    /// <remarks>
    /// The two builds are compiled against different MonoGame assemblies exporting the same types, so the
    /// choice cannot be made inside the game; it has to be made before one of them is loaded. Keeping the
    /// decision separate from the process launching also lets it be tested without a Vulkan driver present.
    /// </remarks>
    public static class BackendSelection
    {
        /// <summary>Environment variable that forces a backend, overriding the probe.</summary>
        public const string OverrideVariable = "CTRDX_GRAPHICS_BACKEND";

        /// <summary>Subdirectory holding the Vulkan build.</summary>
        public const string VulkanDirectory = "vk";

        /// <summary>Subdirectory holding the OpenGL build.</summary>
        public const string OpenGlDirectory = "gl";

        /// <summary>
        /// Reads a forced backend from a command line or environment value.
        /// </summary>
        /// <param name="value">Value to interpret; may be <see langword="null" /> or empty.</param>
        /// <returns>The backend named, or <see langword="null" /> when nothing was named.</returns>
        /// <remarks>
        /// An unrecognised value returns <see langword="null" /> rather than failing, so a stale or
        /// mistyped setting falls back to detection instead of refusing to start the game.
        /// </remarks>
        public static GraphicsBackend? ParseOverride(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            string trimmed = value.Trim().TrimStart('-', '/');
            return trimmed switch
            {
                _ when trimmed.Equals("gl", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Equals("opengl", StringComparison.OrdinalIgnoreCase) => GraphicsBackend.OpenGl,
                _ when trimmed.Equals("vk", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Equals("vulkan", StringComparison.OrdinalIgnoreCase) => GraphicsBackend.Vulkan,
                _ => null,
            };
        }

        /// <summary>
        /// Finds a forced backend among the launcher's arguments.
        /// </summary>
        /// <param name="args">Arguments the launcher was started with.</param>
        /// <returns>The backend named by the last recognised argument, or <see langword="null" />.</returns>
        public static GraphicsBackend? ParseOverride(string[] args)
        {
            GraphicsBackend? found = null;
            foreach (string arg in args ?? [])
            {
                found = ParseOverride(arg) ?? found;
            }
            return found;
        }

        /// <summary>
        /// Decides which build to run.
        /// </summary>
        /// <param name="isWindows">Whether the launcher is running on Windows.</param>
        /// <param name="probe">Result of probing for a hardware Vulkan driver; ignored off Windows.</param>
        /// <param name="forced">Backend named explicitly, or <see langword="null" /> to detect.</param>
        /// <returns>The backend to run.</returns>
        /// <remarks>
        /// Only Windows ships both builds, because only there does hardware old enough to lack Vulkan
        /// still turn up in numbers: Vulkan on Intel starts at Skylake, while OpenGL reaches back to HD
        /// 4000 and beyond. Everywhere else the Vulkan build is the only one produced, so the probe is not
        /// consulted and a machine without Vulkan would fail loudly rather than be silently redirected to
        /// a build that was never shipped.
        /// </remarks>
        public static GraphicsBackend Decide(bool isWindows, VulkanProbeResult probe, GraphicsBackend? forced)
        {
            return forced ?? (!isWindows || probe == VulkanProbeResult.Hardware
                ? GraphicsBackend.Vulkan
                : GraphicsBackend.OpenGl);
        }

        /// <summary>
        /// Subdirectory, relative to the launcher, holding the chosen build.
        /// </summary>
        /// <param name="backend">Backend to run.</param>
        /// <returns>The directory name.</returns>
        public static string DirectoryFor(GraphicsBackend backend)
        {
            return backend == GraphicsBackend.OpenGl ? OpenGlDirectory : VulkanDirectory;
        }
    }
}
