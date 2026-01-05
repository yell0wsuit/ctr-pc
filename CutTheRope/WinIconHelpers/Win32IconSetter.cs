#if WINDOWS
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Provides Win32 helper methods for applying a window icon
/// (<c>.ico</c>) to an existing window handle (<c>HWND</c>).
/// </summary>
/// <remarks>
/// This is typically used when the application icon cannot be set
/// at build time and must be applied manually at runtime.
/// This file will be removed once the project is moved to
/// DesktopGL backend.
/// </remarks>
internal static partial class Win32IconSetter
{
    /// <summary>
    /// Window message used to set a window's icon.
    /// </summary>
    private const int WM_SETICON = 0x0080;

    /// <summary>
    /// Identifier for the small window icon.
    /// </summary>
    private const int ICON_SMALL = 0;

    /// <summary>
    /// Identifier for the large window icon.
    /// </summary>
    private const int ICON_BIG = 1;

    /// <summary>
    /// Image type constant indicating an icon resource.
    /// </summary>
    private const uint IMAGE_ICON = 1;

    /// <summary>
    /// Flag indicating the image should be loaded from a file on disk.
    /// </summary>
    private const uint LR_LOADFROMFILE = 0x0010;

    /// <summary>
    /// Sends a message to the specified window.
    /// </summary>
    /// <param name="hWnd">Handle to the target window.</param>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>
    /// The result of the message processing, which depends on the message sent.
    /// </returns>
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam
    );

    /// <summary>
    /// Loads an image resource from a file or module.
    /// </summary>
    /// <param name="hInst">
    /// Handle to the module containing the image resource, or
    /// <see cref="IntPtr.Zero"/> when loading from file.
    /// </param>
    /// <param name="name">Path to the image file.</param>
    /// <param name="type">The type of image to load.</param>
    /// <param name="cx">Desired width of the image.</param>
    /// <param name="cy">Desired height of the image.</param>
    /// <param name="fuLoad">Flags controlling image loading behavior.</param>
    /// <returns>
    /// A handle to the loaded image, or <see cref="IntPtr.Zero"/> on failure.
    /// </returns>
    [LibraryImport("user32.dll",
        EntryPoint = "LoadImageW",
        StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr LoadImage(
        IntPtr hInst,
        string name,
        uint type,
        int cx,
        int cy,
        uint fuLoad
    );

    /// <summary>
    /// Applies a small and large icon to the specified window.
    /// </summary>
    /// <param name="hwnd">Handle to the target window.</param>
    /// <param name="icoPath">
    /// File system path to the <c>.ico</c> file to apply.
    /// </param>
    /// <remarks>
    /// If the window handle is invalid or the icon cannot be loaded,
    /// the method exits silently without throwing exceptions.
    /// </remarks>
    public static void ApplyIcon(IntPtr hwnd, string icoPath)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        IntPtr small = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);
        IntPtr big = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 256, 256, LR_LOADFROMFILE);

        if (small != IntPtr.Zero)
        {
            _ = SendMessage(hwnd, WM_SETICON, ICON_SMALL, small);
        }

        if (big != IntPtr.Zero)
        {
            _ = SendMessage(hwnd, WM_SETICON, ICON_BIG, big);
        }
    }
}
#endif
