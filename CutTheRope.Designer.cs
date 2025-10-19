using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CutTheRope
{
	// Token: 0x02000004 RID: 4
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class CutTheRope
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001D RID: 29 RVA: 0x000024BE File Offset: 0x000006BE
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (CutTheRope.resourceMan == null)
				{
					CutTheRope.resourceMan = new ResourceManager("CutTheRope.CutTheRope", typeof(CutTheRope).Assembly);
				}
				return CutTheRope.resourceMan;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001E RID: 30 RVA: 0x000024EA File Offset: 0x000006EA
		// (set) Token: 0x0600001F RID: 31 RVA: 0x000024F1 File Offset: 0x000006F1
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return CutTheRope.resourceCulture;
			}
			set
			{
				CutTheRope.resourceCulture = value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000020 RID: 32 RVA: 0x000024F9 File Offset: 0x000006F9
		internal static string FakeString
		{
			get
			{
				return CutTheRope.ResourceManager.GetString("FakeString", CutTheRope.resourceCulture);
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000250F File Offset: 0x0000070F
		internal CutTheRope()
		{
		}

		// Token: 0x0400000A RID: 10
		private static ResourceManager resourceMan;

		// Token: 0x0400000B RID: 11
		private static CultureInfo resourceCulture;
	}
}
