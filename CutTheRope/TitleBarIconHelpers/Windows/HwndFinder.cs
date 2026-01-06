#if WINDOWS
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Provides Win32 helper methods for locating the main window handle
/// (<c>HWND</c>) associated with the current process.
/// </summary>
/// <remarks>
/// This implementation enumerates all top-level windows and applies
/// standard heuristics to identify the main application window:
/// visible, owned by the current process, and not owned by another window.
/// This file will be removed once the project is moved to
/// DesktopGL backend.
/// </remarks>
internal static partial class HwndFinder
{
    /// <summary>
    /// Callback delegate used by the Win32 <c>EnumWindows</c> function.
    /// </summary>
    /// <param name="hWnd">Handle to the enumerated window.</param>
    /// <param name="lParam">Application-defined value passed to <c>EnumWindows</c>.</param>
    /// <returns>
    /// <c>true</c> to continue enumeration; <c>false</c> to stop.
    /// </returns>
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary>
    /// Enumerates all top-level windows on the desktop.
    /// </summary>
    /// <param name="lpEnumFunc">Callback invoked for each window.</param>
    /// <param name="lParam">Application-defined value passed to the callback.</param>
    /// <returns>
    /// <c>true</c> if enumeration succeeds; otherwise <c>false</c>.
    /// </returns>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    /// <summary>
    /// Retrieves the identifier of the process that created the specified window.
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <param name="lpdwProcessId">
    /// Receives the process identifier that owns the window.
    /// </param>
    /// <returns>
    /// The identifier of the thread that created the window.
    /// </returns>
    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint lpdwProcessId
    );

    /// <summary>
    /// Determines whether the specified window is visible.
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <returns>
    /// <c>true</c> if the window is visible; otherwise <c>false</c>.
    /// </returns>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    /// Retrieves a handle to a window that has a specified relationship
    /// to the given window.
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <param name="uCmd">
    /// Relationship flag indicating which window to retrieve.
    /// </param>
    /// <returns>
    /// A handle to the related window, or <see cref="IntPtr.Zero"/> if none exists.
    /// </returns>
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    /// <summary>
    /// Command value used with <c>GetWindow</c> to retrieve the owner window.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindow
    /// </remarks>
    private const uint GW_OWNER = 4;

    /// <summary>
    /// Finds the main top-level window (<c>HWND</c>) associated with
    /// the current process.
    /// </summary>
    /// <returns>
    /// The handle to the main window if found; otherwise <see cref="IntPtr.Zero"/>.
    /// </returns>
    /// <remarks>
    /// A window is considered the "main" window if it:
    /// <list type="bullet">
    ///   <item>Belongs to the current process</item>
    ///   <item>Is visible</item>
    ///   <item>Does not have an owner window</item>
    /// </list>
    /// Enumeration stops as soon as a matching window is found.
    /// </remarks>
    public static IntPtr FindMainWindowForCurrentProcess()
    {
        uint myPid = (uint)Environment.ProcessId;
        IntPtr found = IntPtr.Zero;

        _ = EnumWindows((hWnd, _) =>
        {
            _ = (nint)GetWindowThreadProcessId(hWnd, out uint pid);
            if (pid != myPid)
            {
                return true;
            }

            if (!IsWindowVisible(hWnd))
            {
                return true;
            }

            if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
            {
                return true;
            }

            found = hWnd;
            return false; // stop enumeration
        }, IntPtr.Zero);

        return found;
    }
}
#endif
