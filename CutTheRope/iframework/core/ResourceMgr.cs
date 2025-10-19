using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CutTheRope.game;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.iframework.core
{
	// Token: 0x02000067 RID: 103
	internal class ResourceMgr : NSObject
	{
		// Token: 0x060003D5 RID: 981 RVA: 0x000151E4 File Offset: 0x000133E4
		public virtual bool hasResource(int resID)
		{
			NSObject value = null;
			this.s_Resources.TryGetValue(resID, out value);
			return value != null;
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x00015206 File Offset: 0x00013406
		public virtual void addResourceToLoadQueue(int resID)
		{
			this.loadQueue.Add(resID);
			this.loadCount++;
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x00015222 File Offset: 0x00013422
		public void clearCachedResources()
		{
			this.s_Resources = new Dictionary<int, NSObject>();
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x00015230 File Offset: 0x00013430
		public virtual NSObject loadResource(int resID, ResourceMgr.ResourceType resType)
		{
			NSObject value = null;
			if (this.s_Resources.TryGetValue(resID, out value))
			{
				return value;
			}
			string path = ((resType != ResourceMgr.ResourceType.STRINGS) ? CTRResourceMgr.XNA_ResName(resID) : "");
			bool flag = false;
			float scaleX = this.getNormalScaleX(resID);
			float scaleY = this.getNormalScaleY(resID);
			if (flag)
			{
				scaleX = this.getWvgaScaleX(resID);
				scaleY = this.getWvgaScaleY(resID);
			}
			switch (resType)
			{
			case ResourceMgr.ResourceType.IMAGE:
				value = this.loadTextureImageInfo(path, null, flag, scaleX, scaleY);
				break;
			case ResourceMgr.ResourceType.FONT:
				value = this.loadVariableFontInfo(path, resID, flag);
				this.s_Resources.Remove(resID);
				break;
			case ResourceMgr.ResourceType.SOUND:
				value = this.loadSoundInfo(path);
				break;
			case ResourceMgr.ResourceType.STRINGS:
				value = this.loadStringsInfo(resID);
				value = NSObject.NSS(value.ToString().Replace('\u00a0', ' '));
				break;
			}
			if (value != null)
			{
				this.s_Resources.Add(resID, value);
			}
			return value;
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00015309 File Offset: 0x00013509
		public virtual NSObject loadSoundInfo(string path)
		{
			return new NSObject().init();
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00015318 File Offset: 0x00013518
		public NSString loadStringsInfo(int key)
		{
			key &= 65535;
			if (this.xmlStrings == null)
			{
				this.xmlStrings = XMLNode.parseXML("menu_strings.xml");
			}
			XMLNode xMLNode = null;
			try
			{
				xMLNode = this.xmlStrings.childs()[key];
			}
			catch (Exception)
			{
			}
			if (xMLNode != null)
			{
				string tag = "en";
				if (ResDataPhoneFull.LANGUAGE == Language.LANG_RU)
				{
					tag = "ru";
				}
				if (ResDataPhoneFull.LANGUAGE == Language.LANG_FR)
				{
					tag = "fr";
				}
				if (ResDataPhoneFull.LANGUAGE == Language.LANG_DE)
				{
					tag = "de";
				}
				XMLNode xMLNode2 = xMLNode.findChildWithTagNameRecursively(tag, false);
				if (xMLNode2 == null)
				{
					xMLNode2 = xMLNode.findChildWithTagNameRecursively("en", false);
				}
				return xMLNode2.data;
			}
			return new NSString();
		}

		// Token: 0x060003DB RID: 987 RVA: 0x000153C8 File Offset: 0x000135C8
		public virtual FontGeneric loadVariableFontInfo(string path, int resID, bool isWvga)
		{
			XMLNode xmlnode = XMLNode.parseXML(path);
			int num = xmlnode["charoff"].intValue();
			int num2 = xmlnode["lineoff"].intValue();
			int num3 = xmlnode["space"].intValue();
			XMLNode xMLNode2 = xmlnode.findChildWithTagNameRecursively("chars", false);
			XMLNode xMLNode3 = xmlnode.findChildWithTagNameRecursively("kerning", false);
			NSString data = xMLNode2.data;
			if (xMLNode3 != null)
			{
				NSString data2 = xMLNode3.data;
			}
			Font font = new Font().initWithVariableSizeCharscharMapFileKerning(data, (Texture2D)this.loadResource(resID, ResourceMgr.ResourceType.IMAGE), null);
			font.setCharOffsetLineOffsetSpaceWidth((float)num, (float)num2, (float)num3);
			return font;
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00015464 File Offset: 0x00013664
		public virtual Texture2D loadTextureImageInfo(string path, XMLNode i, bool isWvga, float scaleX, float scaleY)
		{
			if (i == null)
			{
				i = XMLNode.parseXML(path);
			}
			bool flag = (i["filter"].intValue() & 1) == 1;
			int defaultAlphaPixelFormat = i["format"].intValue();
			string text = ResourceMgr.fullPathFromRelativePath(path);
			if (flag)
			{
				Texture2D.setAntiAliasTexParameters();
			}
			else
			{
				Texture2D.setAliasTexParameters();
			}
			Texture2D.setDefaultAlphaPixelFormat((Texture2D.Texture2DPixelFormat)defaultAlphaPixelFormat);
			Texture2D texture2D = new Texture2D().initWithPath(text, true);
			if (texture2D == null)
			{
				throw new Exception("texture not found: " + text);
			}
			Texture2D.setDefaultAlphaPixelFormat(Texture2D.kTexture2DPixelFormat_Default);
			if (isWvga)
			{
				texture2D.setWvga();
			}
			texture2D.setScale(scaleX, scaleY);
			this.setTextureInfo(texture2D, i, isWvga, scaleX, scaleY);
			return texture2D;
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0001550C File Offset: 0x0001370C
		public virtual void setTextureInfo(Texture2D t, XMLNode i, bool isWvga, float scaleX, float scaleY)
		{
			t.preCutSize = MathHelper.vectUndefined;
			XMLNode xMLNode = i.findChildWithTagNameRecursively("quads", false);
			if (xMLNode != null)
			{
				List<NSString> list = xMLNode.data.componentsSeparatedByString(',');
				if (list != null && list.Count > 0)
				{
					float[] array = new float[list.Count];
					for (int j = 0; j < list.Count; j++)
					{
						array[j] = list[j].floatValue();
					}
					this.setQuadsInfo(t, array, list.Count, scaleX, scaleY);
				}
			}
			XMLNode xMLNode2 = i.findChildWithTagNameRecursively("offsets", false);
			if (xMLNode2 == null)
			{
				return;
			}
			List<NSString> list2 = xMLNode2.data.componentsSeparatedByString(',');
			if (list2 == null || list2.Count <= 0)
			{
				return;
			}
			float[] array2 = new float[list2.Count];
			for (int k = 0; k < list2.Count; k++)
			{
				array2[k] = list2[k].floatValue();
			}
			this.setOffsetsInfo(t, array2, list2.Count, scaleX, scaleY);
			XMLNode xMLNode3 = i.findChildWithTagNameRecursively(NSObject.NSS("preCutWidth"), false);
			XMLNode xMLNode4 = i.findChildWithTagNameRecursively(NSObject.NSS("preCutHeight"), false);
			if (xMLNode3 != null && xMLNode4 != null)
			{
				t.preCutSize = MathHelper.vect((float)xMLNode3.data.intValue(), (float)xMLNode4.data.intValue());
				if (isWvga)
				{
					t.preCutSize.x = t.preCutSize.x / 1.5f;
					t.preCutSize.y = t.preCutSize.y / 1.5f;
				}
			}
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0001568A File Offset: 0x0001388A
		private static string fullPathFromRelativePath(string relPath)
		{
			return ResDataPhoneFull.ContentFolder + relPath;
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00015698 File Offset: 0x00013898
		private void setQuadsInfo(Texture2D t, float[] data, int size, float scaleX, float scaleY)
		{
			int num = data.Length / 4;
			t.setQuadsCapacity(num);
			int num2 = -1;
			for (int i = 0; i < num; i++)
			{
				int num3 = i * 4;
				Rectangle rect = FrameworkTypes.MakeRectangle(data[num3], data[num3 + 1], data[num3 + 2], data[num3 + 3]);
				if ((float)num2 < rect.h + rect.y)
				{
					num2 = (int)MathHelper.ceil((double)(rect.h + rect.y));
				}
				rect.x /= scaleX;
				rect.y /= scaleY;
				rect.w /= scaleX;
				rect.h /= scaleY;
				t.setQuadAt(rect, i);
			}
			if (num2 != -1)
			{
				t._lowypoint = num2;
			}
			t.optimizeMemory();
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0001575C File Offset: 0x0001395C
		private void setOffsetsInfo(Texture2D t, float[] data, int size, float scaleX, float scaleY)
		{
			int num = size / 2;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 2;
				t.quadOffsets[i].x = data[num2];
				t.quadOffsets[i].y = data[num2 + 1];
				Vector[] quadOffsets = t.quadOffsets;
				int num3 = i;
				quadOffsets[num3].x = quadOffsets[num3].x / scaleX;
				Vector[] quadOffsets2 = t.quadOffsets;
				int num4 = i;
				quadOffsets2[num4].y = quadOffsets2[num4].y / scaleY;
			}
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x000157D5 File Offset: 0x000139D5
		public virtual bool isWvgaResource(int r)
		{
			return r - 126 > 10;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x000157E2 File Offset: 0x000139E2
		public virtual float getNormalScaleX(int r)
		{
			return 1f;
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x000157E9 File Offset: 0x000139E9
		public virtual float getNormalScaleY(int r)
		{
			return 1f;
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x000157F0 File Offset: 0x000139F0
		public virtual float getWvgaScaleX(int r)
		{
			return 1.5f;
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x000157F7 File Offset: 0x000139F7
		public virtual float getWvgaScaleY(int r)
		{
			return 1.5f;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x000157FE File Offset: 0x000139FE
		public virtual void initLoading()
		{
			this.loadQueue.Clear();
			this.loaded = 0;
			this.loadCount = 0;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00015819 File Offset: 0x00013A19
		public virtual int getPercentLoaded()
		{
			if (this.loadCount == 0)
			{
				return 100;
			}
			return 100 * this.loaded / this.getLoadCount();
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x00015838 File Offset: 0x00013A38
		public virtual void loadPack(int[] pack)
		{
			int i = 0;
			while (pack[i] != -1)
			{
				this.addResourceToLoadQueue(pack[i]);
				i++;
			}
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x0001585C File Offset: 0x00013A5C
		public virtual void freePack(int[] pack)
		{
			int i = 0;
			while (pack[i] != -1)
			{
				this.freeResource(pack[i]);
				i++;
			}
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00015880 File Offset: 0x00013A80
		public virtual void loadImmediately()
		{
			while (this.loadQueue.Count != 0)
			{
				int resId = this.loadQueue[0];
				this.loadQueue.RemoveAt(0);
				this.loadResource(resId);
				this.loaded++;
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000158CC File Offset: 0x00013ACC
		public virtual void startLoading()
		{
			if (this.resourcesDelegate != null)
			{
				DelayedDispatcher.DispatchFunc dispatchFunc;
				if ((dispatchFunc = ResourceMgr.<>O.<0>__rmgr_internalUpdate) == null)
				{
					dispatchFunc = (ResourceMgr.<>O.<0>__rmgr_internalUpdate = new DelayedDispatcher.DispatchFunc(ResourceMgr.rmgr_internalUpdate));
				}
				this.Timer = NSTimer.schedule(dispatchFunc, this, 0.022222223f);
			}
			this.bUseFake = this.loadQueue.Count < 100;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00015922 File Offset: 0x00013B22
		private int getLoadCount()
		{
			if (!this.bUseFake)
			{
				return this.loadCount;
			}
			return 100;
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00015938 File Offset: 0x00013B38
		public void update()
		{
			if (this.loadQueue.Count > 0)
			{
				int resId = this.loadQueue[0];
				this.loadQueue.RemoveAt(0);
				this.loadResource(resId);
			}
			this.loaded++;
			if (this.loaded >= this.getLoadCount())
			{
				if (this.Timer >= 0)
				{
					NSTimer.stopTimer(this.Timer);
				}
				this.Timer = -1;
				this.resourcesDelegate.allResourcesLoaded();
			}
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x000159B5 File Offset: 0x00013BB5
		private static void rmgr_internalUpdate(NSObject obj)
		{
			((ResourceMgr)obj).update();
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x000159C4 File Offset: 0x00013BC4
		private void loadResource(int resId)
		{
			if (150 < resId)
			{
				return;
			}
			if (10 == resId)
			{
				if (this.xmlStrings == null)
				{
					this.xmlStrings = XMLNode.parseXML("menu_strings.xml");
				}
				return;
			}
			if (ResDataPhoneFull.isSound(resId))
			{
				Application.sharedSoundMgr().getSound(resId);
				return;
			}
			if (ResDataPhoneFull.isFont(resId))
			{
				Application.getFont(resId);
				return;
			}
			try
			{
				Application.getTexture(resId);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x00015A3C File Offset: 0x00013C3C
		public virtual void freeResource(int resId)
		{
			if (150 < resId)
			{
				return;
			}
			if (10 == resId)
			{
				this.xmlStrings = null;
				return;
			}
			if (ResDataPhoneFull.isSound(resId))
			{
				Application.sharedSoundMgr().freeSound(resId);
				return;
			}
			NSObject value = null;
			if (this.s_Resources.TryGetValue(resId, out value))
			{
				if (value != null)
				{
					value.dealloc();
				}
				this.s_Resources.Remove(resId);
			}
		}

		// Token: 0x040002A8 RID: 680
		public ResourceMgrDelegate resourcesDelegate;

		// Token: 0x040002A9 RID: 681
		private Dictionary<int, NSObject> s_Resources = new Dictionary<int, NSObject>();

		// Token: 0x040002AA RID: 682
		private XMLNode xmlStrings;

		// Token: 0x040002AB RID: 683
		private int loaded;

		// Token: 0x040002AC RID: 684
		private int loadCount;

		// Token: 0x040002AD RID: 685
		private List<int> loadQueue = new List<int>();

		// Token: 0x040002AE RID: 686
		private int Timer;

		// Token: 0x040002AF RID: 687
		private bool bUseFake;

		// Token: 0x020000BD RID: 189
		public enum ResourceType
		{
			// Token: 0x040008DB RID: 2267
			IMAGE,
			// Token: 0x040008DC RID: 2268
			FONT,
			// Token: 0x040008DD RID: 2269
			SOUND,
			// Token: 0x040008DE RID: 2270
			BINARY,
			// Token: 0x040008DF RID: 2271
			STRINGS,
			// Token: 0x040008E0 RID: 2272
			ELEMENT
		}

		// Token: 0x020000BE RID: 190
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040008E1 RID: 2273
			public static DelayedDispatcher.DispatchFunc <0>__rmgr_internalUpdate;
		}
	}
}
