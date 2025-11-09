using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CutTheRope.windows
{
    internal static class NativeMethods
    {
        public static Cursor LoadCustomCursor(string path)
        {
            IntPtr intPtr = LoadCursorFromFile(path);
            if (intPtr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            return new Cursor(intPtr);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursorFromFile(string path);
    }
}
