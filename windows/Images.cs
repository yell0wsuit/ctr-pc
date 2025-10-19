using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.windows
{
	// Token: 0x0200000D RID: 13
	internal class Images
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00003744 File Offset: 0x00001944
		private static ContentManager getContentManager(string imgName)
		{
			ContentManager value = null;
			Images._contentManagers.TryGetValue(imgName, out value);
			if (value == null)
			{
				value = new ContentManager(Global.XnaGame.Services, "content");
				Images._contentManagers.Add(imgName, value);
			}
			return value;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00003788 File Offset: 0x00001988
		public static Texture2D get(string imgName)
		{
			ContentManager contentManager = Images.getContentManager(imgName);
			Texture2D result = null;
			Texture2D texture2D;
			try
			{
				result = contentManager.Load<Texture2D>(imgName);
				texture2D = result;
			}
			catch (Exception)
			{
				texture2D = result;
			}
			return texture2D;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000037C0 File Offset: 0x000019C0
		public static void free(string imgName)
		{
			Images.getContentManager(imgName).Unload();
		}

		// Token: 0x04000055 RID: 85
		private static Dictionary<string, ContentManager> _contentManagers = new Dictionary<string, ContentManager>();
	}
}
