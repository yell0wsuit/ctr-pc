using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Pins the bundled SwiftShader library into the process under the name the renderer looks for.
    /// </summary>
    /// <remarks>
    /// MonoGame's native runtime resolves Vulkan entry points through volk, which calls
    /// <c>LoadLibrary("vulkan-1.dll")</c> itself; it does not go through SDL. Redirecting only SDL (via
    /// <c>SDL_VULKAN_LIBRARY</c>) therefore leaves the renderer on the system driver while SDL creates
    /// surfaces against a different library, which faults when the swapchain is built.
    /// <para>
    /// Loading our copy first by full path avoids that: Windows matches a later <c>LoadLibrary</c> against
    /// already-loaded modules by base name, so both volk and SDL receive this module instead of the one in
    /// System32. The bundled library is shipped renamed to <c>vulkan-1.dll</c> for that reason, and it
    /// exports the loader-level entry points volk needs.
    /// </para>
    /// </remarks>
    [SupportedOSPlatform("windows")]
    internal static partial class SoftwareVulkanLoader
    {
        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16,
            SetLastError = true)]
        private static partial IntPtr LoadLibrary(string fileName);

        /// <summary>
        /// Loads the library at <paramref name="path"/> so it claims its base name within this process.
        /// </summary>
        /// <param name="path">Absolute path of the library to load.</param>
        /// <returns><see langword="true"/> when the library was loaded; otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// The handle is deliberately never freed. The module has to stay resident for the lifetime of the
        /// process, because the renderer resolves it long after this call returns.
        /// </remarks>
        public static bool TryLoad(string path)
        {
            if (LoadLibrary(path) != IntPtr.Zero)
            {
                return true;
            }

            Console.Error.WriteLine(
                $"[graphics] Could not load {path}: Win32 error {Marshal.GetLastPInvokeError()}.");
            return false;
        }
    }
}
