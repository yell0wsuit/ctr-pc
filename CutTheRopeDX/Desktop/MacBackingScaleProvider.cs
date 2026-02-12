#if MACOS_AVFOUNDATION
using AppKit;
#endif

namespace CutTheRope.Desktop
{
    internal sealed class MacBackingScaleProvider : IBackingScaleProvider
    {
        public bool TryGetCurrentScale(out double scale)
        {
#if MACOS_AVFOUNDATION
            try
            {
                NSWindow keyWindow = NSApplication.SharedApplication?.KeyWindow;
                NSScreen windowScreen = keyWindow?.Screen;
                if (windowScreen != null && windowScreen.BackingScaleFactor > 0d)
                {
                    scale = windowScreen.BackingScaleFactor;
                    return true;
                }

                NSScreen mainScreen = NSScreen.MainScreen;
                if (mainScreen != null && mainScreen.BackingScaleFactor > 0d)
                {
                    scale = mainScreen.BackingScaleFactor;
                    return true;
                }
            }
            catch
            {
            }
#endif
            scale = 0d;
            return false;
        }
    }
}
