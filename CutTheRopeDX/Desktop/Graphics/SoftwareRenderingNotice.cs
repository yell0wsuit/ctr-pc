using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CutTheRopeDX.Desktop.Graphics
{
    /// <summary>
    /// Tells the user, via an OS dialog, that the game is about to render in software.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static partial class SoftwareRenderingNotice
    {
        private const uint MbOk = 0x0;
        private const uint MbIconWarning = 0x30;
        private const uint MbSetForeground = 0x10000;

        private const string Caption = "Software rendering";

        private const string Text =
            "Your graphics card doesn't support Vulkan, so Cut the Rope: DX will render using your CPU instead.\n\n" +
            "The game will still run, but it may feel slower than normal.";

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int MessageBox(IntPtr window, string text, string caption, uint type);

        /// <summary>
        /// Shows the warning dialog and blocks until the user dismisses it.
        /// </summary>
        /// <remarks>Failures are swallowed: a missing warning is a far better outcome than a blocked launch.</remarks>
        public static void Show()
        {
            try
            {
                // MB_SETFOREGROUND matters here because the game has no window yet to own the dialog.
                _ = MessageBox(IntPtr.Zero, Text, Caption, MbOk | MbIconWarning | MbSetForeground);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[graphics] Could not show the software rendering notice: {ex.Message}");
            }
        }
    }
}
