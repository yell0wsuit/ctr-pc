using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CutTheRope.windows
{
	// Token: 0x0200000F RID: 15
	internal static class NativeMethods
	{
		// Token: 0x06000065 RID: 101 RVA: 0x00003B4A File Offset: 0x00001D4A
		public static Cursor LoadCustomCursor(string path)
		{
			IntPtr intPtr = NativeMethods.LoadCursorFromFile(path);
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			return new Cursor(intPtr);
		}

		// Token: 0x06000066 RID: 102
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadCursorFromFile(string path);
	}
}
