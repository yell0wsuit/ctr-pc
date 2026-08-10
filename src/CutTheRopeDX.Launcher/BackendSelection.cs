using System.IO;

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

        /// <summary>Name of the Vulkan build, which sits beside the launcher.</summary>
        public const string VulkanExecutable = "ctrdx-vk";

        /// <summary>Name of the OpenGL build, which sits beside the launcher.</summary>
        public const string OpenGlExecutable = "ctrdx-gl";

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
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().TrimStart('-', '/').ToLowerInvariant() switch
                {
                    "gl" or "opengl" => GraphicsBackend.OpenGl,
                    "vk" or "vulkan" => GraphicsBackend.Vulkan,
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
        /// Whether telling the user about a fallback is worth interrupting the launch for.
        /// </summary>
        /// <param name="chosen">Backend this launch settled on.</param>
        /// <param name="lastSeen">Backend recorded on the previous launch, or <see langword="null" /> when none is.</param>
        /// <param name="wasForced">Whether the backend came from a switch or environment variable.</param>
        /// <returns><see langword="true" /> when the dialog should be shown.</returns>
        /// <remarks>
        /// Only the change is worth reporting. Warning on every launch would train the player to dismiss
        /// the dialog without reading it, and the message it carries matters exactly once: when the machine
        /// stops being able to do something it previously could, or was expected to.
        /// <para>
        /// A launch that died inside the probe leaves a marker that is not a backend name, so the recovery
        /// launch after it sees no last-seen value and does warn. That is intended: the player has not been
        /// told yet, and a driver crashing on probe is precisely the case they can act on.
        /// </para>
        /// <para>
        /// Never shown when the backend was asked for. Someone passing <c>--gl</c> already knows.
        /// </para>
        /// </remarks>
        public static bool ShouldWarn(GraphicsBackend chosen, GraphicsBackend? lastSeen, bool wasForced)
        {
            return !wasForced && chosen == GraphicsBackend.OpenGl && lastSeen != GraphicsBackend.OpenGl;
        }

        /// <summary>
        /// Name the chosen build carries, without a file extension.
        /// </summary>
        /// <param name="backend">Backend to run.</param>
        /// <returns>The executable name.</returns>
        public static string ExecutableFor(GraphicsBackend backend)
        {
            return backend == GraphicsBackend.OpenGl ? OpenGlExecutable : VulkanExecutable;
        }

        /// <summary>
        /// Paths, in order of preference, at which the chosen build may be found.
        /// </summary>
        /// <param name="baseDirectory">Directory the launcher is running from.</param>
        /// <param name="backend">Backend to run.</param>
        /// <returns>Candidate paths under <paramref name="baseDirectory"/>.</returns>
        /// <remarks>
        /// Both builds sit in one directory and are told apart by name. That works whether or not they were
        /// compiled ahead of time, because every publish that ships is single-file: the managed assemblies
        /// go inside the executable either way, and what stays loose beside them is native and named
        /// differently per backend.
        /// <para>
        /// The extensionless form is what the same builds are called off Windows, which is where the
        /// launcher is developed and its dispatch is exercised even though only Windows ships it.
        /// </para>
        /// </remarks>
        public static string[] CandidatePaths(string baseDirectory, GraphicsBackend backend)
        {
            string name = ExecutableFor(backend);
            return
            [
                Path.Combine(baseDirectory, name + ".exe"),
                Path.Combine(baseDirectory, name),
            ];
        }
    }
}
