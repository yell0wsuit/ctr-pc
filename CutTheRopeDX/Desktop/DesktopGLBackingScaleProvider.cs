using System;
using System.Runtime.InteropServices;

namespace CutTheRopeDX.Desktop
{
    internal sealed partial class DesktopGLBackingScaleProvider : IBackingScaleProvider
    {
        private const uint SDL_WINDOW_ALLOW_HIGHDPI = 0x00002000;

        public bool TryGetCurrentScale(out double scale)
        {
            if (!OperatingSystem.IsMacOS())
            {
                scale = 1d;
                return true;
            }

            try
            {
                IntPtr windowHandle = TryGetWindowHandle();
                if (windowHandle == IntPtr.Zero)
                {
                    scale = 0d;
                    return false;
                }

                SDL_GetWindowSize(windowHandle, out int logicalWidth, out int logicalHeight);
                SDL_GL_GetDrawableSize(windowHandle, out int drawableWidth, out int drawableHeight);
                uint windowFlags = SDL_GetWindowFlags(windowHandle);
                if (!IsHighDpiAllowed(windowFlags))
                {
                    scale = 1d;
                    return true;
                }

                scale = CalculateScaleFromSizes(logicalWidth, logicalHeight, drawableWidth, drawableHeight);
                return scale > 0d;
            }
            catch
            {
                scale = 0d;
                return false;
            }
        }

        internal static double CalculateScaleFromSizes(int logicalWidth, int logicalHeight, int drawableWidth, int drawableHeight)
        {
            if (logicalWidth <= 0 || logicalHeight <= 0 || drawableWidth <= 0 || drawableHeight <= 0)
            {
                return 0d;
            }

            double scaleX = drawableWidth / (double)logicalWidth;
            double scaleY = drawableHeight / (double)logicalHeight;
            double scale = Math.Max(scaleX, scaleY);
            return scale < 1d ? 1d : Math.Round(scale, 2, MidpointRounding.AwayFromZero);
        }

        internal static bool IsHighDpiAllowed(uint windowFlags)
        {
            return (windowFlags & SDL_WINDOW_ALLOW_HIGHDPI) != 0;
        }

        private static IntPtr TryGetWindowHandle()
        {
            return Global.XnaGame?.Window == null ? nint.Zero : Global.XnaGame.Window.Handle;
        }

        [LibraryImport("libSDL2-2.0.0.dylib")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void SDL_GetWindowSize(IntPtr window, out int width, out int height);

        [LibraryImport("libSDL2-2.0.0.dylib")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void SDL_GL_GetDrawableSize(IntPtr window, out int width, out int height);

        [LibraryImport("libSDL2-2.0.0.dylib")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial uint SDL_GetWindowFlags(IntPtr window);
    }
}
