using System;
using System.IO;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Selects the rendering backend at startup, falling back to bundled software Vulkan when the machine
    /// has no usable GPU driver.
    /// </summary>
    /// <remarks>
    /// Must run before the graphics device is created, because the backend is chosen through an environment
    /// variable that SDL reads when it loads Vulkan.
    /// </remarks>
    internal static class GraphicsFallback
    {
        /// <summary>Environment variable SDL reads to load an alternative Vulkan library.</summary>
        private const string SdlVulkanLibraryVariable = "SDL_VULKAN_LIBRARY";

        /// <summary>Path of the bundled SwiftShader library, relative to the executable.</summary>
        private const string SwiftShaderRelativePath = "swiftshader/vk_swiftshader.dll";

        /// <summary>
        /// Sequences the backend decision against injected effects.
        /// </summary>
        /// <param name="readMode">Reads the stored mode; returns an empty string when unset.</param>
        /// <param name="writeMode">Persists a mode, synchronously.</param>
        /// <param name="probe">Runs the hardware Vulkan probe.</param>
        /// <param name="showNotice">Warns the user that rendering will be done in software.</param>
        /// <param name="applySoftware">Switches rendering to the bundled software library.</param>
        public static void Run(
            Func<string> readMode,
            Action<string> writeMode,
            Func<VulkanProbeResult> probe,
            Action showNotice,
            Action applySoftware)
        {
            GraphicsDecision decision = GraphicsBackendSelector.DecideFromStored(readMode());

            if (decision.NeedsProbe)
            {
                // The marker must reach disk before the probe runs; a driver that faults inside
                // vkCreateInstance takes the whole process with it, and the next launch reads this.
                writeMode(GraphicsMode.Probing);
                decision = GraphicsBackendSelector.DecideFromProbe(probe());
            }

            if (decision.ModeToPersist is not null)
            {
                writeMode(decision.ModeToPersist);
            }

            if (decision.ShowNotice)
            {
                showNotice();
            }

            if (decision.UseSoftware)
            {
                applySoftware();
            }
        }

        /// <summary>
        /// Runs the backend decision with the real preference store, probe, dialog and environment.
        /// Does nothing off Windows, and never throws.
        /// </summary>
        public static void Configure()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            try
            {
                Preferences.LoadPreferences();

                Run(
                    () => Preferences.GetStringForKey(GraphicsMode.PreferenceKey),
                    WriteMode,
                    VulkanProbe.Run,
                    SoftwareRenderingNotice.Show,
                    ApplySoftwareRendering);
            }
            catch (Exception ex)
            {
                // Never let backend selection be the reason the game won't start.
                Console.Error.WriteLine($"[graphics] Backend selection failed, using the default: {ex.Message}");
            }
        }

        /// <summary>
        /// Persists a mode and flushes it to disk immediately.
        /// </summary>
        /// <param name="mode">Mode to store.</param>
        /// <remarks>
        /// <c>commit: true</c> only raises a deferred save flag that the game loop drains once per frame.
        /// None of this runs inside the game loop, so <see cref="Preferences.Update"/> must be called
        /// explicitly or nothing is written.
        /// </remarks>
        private static void WriteMode(string mode)
        {
            Preferences.SetStringForKey(mode, GraphicsMode.PreferenceKey, commit: true);
            Preferences.Update();
        }

        /// <summary>
        /// Points SDL at the bundled SwiftShader library.
        /// </summary>
        private static void ApplySoftwareRendering()
        {
            string path = Path.Combine(AppContext.BaseDirectory, SwiftShaderRelativePath);

            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"[graphics] Software rendering was selected but {path} is missing.");
                return;
            }

            Environment.SetEnvironmentVariable(SdlVulkanLibraryVariable, path);
        }
    }
}
