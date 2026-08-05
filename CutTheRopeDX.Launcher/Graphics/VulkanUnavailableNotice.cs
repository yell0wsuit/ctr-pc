using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CutTheRopeDX.Launcher.Graphics
{
    /// <summary>
    /// Tells the user, via an OS dialog, that Vulkan could not be opened and the game will run on OpenGL.
    /// </summary>
    /// <remarks>
    /// Worth saying out loud rather than falling back silently, because the two builds are not identical
    /// in the long run: a driver problem the player can actually fix is otherwise indistinguishable from
    /// the game simply behaving that way on their machine.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    internal static partial class VulkanUnavailableNotice
    {
        private const uint MbOk = 0x0;
        private const uint MbIconInformation = 0x40;
        private const uint MbSetForeground = 0x10000;

        private const string Caption = "Vulkan unavailable";

        /// <summary>
        /// Names the cause, says what happens instead, and points at the one thing that might fix it.
        /// Deliberately does not call the result degraded: OpenGL is a supported way to run the game, not
        /// a broken one, and on the hardware this fallback exists for it is the only way.
        /// </summary>
        private const string Text =
            "Cut the Rope: DX couldn't start Vulkan on this device, so it will run using OpenGL instead.\n\n"
            + "The game should play normally. If you expected Vulkan to work, updating your graphics "
            + "drivers usually resolves it.\n\n"
            + "This message won't appear again unless your graphics setup changes.";

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int MessageBox(IntPtr window, string text, string caption, uint type);

        /// <summary>
        /// Shows the dialog and blocks until the user dismisses it.
        /// </summary>
        /// <remarks>
        /// Failures are swallowed. A missing warning is a far better outcome than a blocked launch, which
        /// is the whole reason the fallback exists.
        /// </remarks>
        public static void Show()
        {
            try
            {
                // MB_SETFOREGROUND matters because there is no game window yet for the dialog to own.
                _ = MessageBox(IntPtr.Zero, Text, Caption, MbOk | MbIconInformation | MbSetForeground);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"[launcher] Could not show the Vulkan notice: {exception.Message}");
            }
        }
    }
}
