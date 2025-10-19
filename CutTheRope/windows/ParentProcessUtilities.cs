using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CutTheRope.windows
{
	// Token: 0x02000011 RID: 17
	public struct ParentProcessUtilities
	{
		// Token: 0x060000AB RID: 171
		[DllImport("ntdll.dll")]
		private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

		// Token: 0x060000AC RID: 172 RVA: 0x00004AF1 File Offset: 0x00002CF1
		public static Process GetParentProcess()
		{
			return ParentProcessUtilities.GetParentProcess(Process.GetCurrentProcess().Handle);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004B02 File Offset: 0x00002D02
		public static Process GetParentProcess(int id)
		{
			return ParentProcessUtilities.GetParentProcess(Process.GetProcessById(id).Handle);
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004B14 File Offset: 0x00002D14
		public static Process GetParentProcess(IntPtr handle)
		{
			ParentProcessUtilities processInformation = default(ParentProcessUtilities);
			int returnLength;
			int num = ParentProcessUtilities.NtQueryInformationProcess(handle, 0, ref processInformation, Marshal.SizeOf(processInformation), out returnLength);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			Process process;
			try
			{
				process = Process.GetProcessById(processInformation.InheritedFromUniqueProcessId.ToInt32());
			}
			catch (ArgumentException)
			{
				process = null;
			}
			return process;
		}

		// Token: 0x0400007A RID: 122
		internal IntPtr Reserved1;

		// Token: 0x0400007B RID: 123
		internal IntPtr PebBaseAddress;

		// Token: 0x0400007C RID: 124
		internal IntPtr Reserved2_0;

		// Token: 0x0400007D RID: 125
		internal IntPtr Reserved2_1;

		// Token: 0x0400007E RID: 126
		internal IntPtr UniqueProcessId;

		// Token: 0x0400007F RID: 127
		internal IntPtr InheritedFromUniqueProcessId;
	}
}
