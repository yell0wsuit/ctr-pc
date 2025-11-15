using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRope.game;
using CutTheRope.Helpers;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.iframework.core
{
    internal class ResourceMgr : FrameworkTypes
    {

        public virtual bool HasResource(int resID)
        {
            return s_Resources.TryGetValue(resID, out _);
        }

        public virtual void AddResourceToLoadQueue(int resID)
        {
            loadQueue.Add(resID);
            loadCount++;
        }

        public void ClearCachedResources()
        {
            s_Resources.Clear();
        }

        public virtual object LoadResource(int resID, ResourceType resType)
        {
            if (s_Resources.TryGetValue(resID, out object value))
            {
                return value;
            }
            string path = resType != ResourceType.STRINGS ? CTRResourceMgr.XNA_ResName(resID) : "";
            bool flag = false;
            float scaleX = GetNormalScaleX(resID);
            float scaleY = GetNormalScaleY(resID);
            if (flag)
            {
                scaleX = GetWvgaScaleX(resID);
                scaleY = GetWvgaScaleY(resID);
            }
            switch (resType)
            {
                case ResourceType.IMAGE:
                    value = LoadTextureImageInfo(path, null, flag, scaleX, scaleY);
                    break;
                case ResourceType.FONT:
                    value = LoadVariableFontInfo(path, resID, flag);
                    _ = s_Resources.Remove(resID);
                    break;
                case ResourceType.SOUND:
                    value = LoadSoundInfo(path);
                    break;
                case ResourceType.STRINGS:
                    {
                        string strValue = LoadStringsInfo(resID);
                        value = strValue.Replace('\u00a0', ' ');
                    }
                    break;
                case ResourceType.BINARY:
                    break;
                case ResourceType.ELEMENT:
                    break;
                default:
                    break;
            }
            if (value != null)
            {
                s_Resources.Add(resID, value);
            }
            return value;
        }

        public virtual FrameworkTypes LoadSoundInfo(string path)
        {
            return new FrameworkTypes();
        }

        public string LoadStringsInfo(int key)
        {
            key &= 65535;
            xmlStrings ??= XElementExtensions.LoadContentXml("menu_strings.xml");
            XElement xMLNode = null;
            try
            {
                xMLNode = xmlStrings?.Elements().ElementAtOrDefault(key);
            }
            catch (Exception)
            {
            }
            if (xMLNode != null)
            {
                string tag = "en";
                if (LANGUAGE == Language.LANGRU)
                {
                    tag = "ru";
                }
                if (LANGUAGE == Language.LANGFR)
                {
                    tag = "fr";
                }
                if (LANGUAGE == Language.LANGDE)
                {
                    tag = "de";
                }
                XElement xMLNode2 = xMLNode.FindChildWithTagNameRecursively(tag, false);
                xMLNode2 ??= xMLNode.FindChildWithTagNameRecursively("en", false);
                return xMLNode2?.ValueAsNSString() ?? string.Empty;
            }
            return string.Empty;
        }

        public virtual FontGeneric LoadVariableFontInfo(string path, int resID, bool isWvga)
        {
            XElement xmlnode = XElementExtensions.LoadContentXml(path);
            int num = xmlnode.AttributeAsNSString("charoff").IntValue();
            int num2 = xmlnode.AttributeAsNSString("lineoff").IntValue();
            int num3 = xmlnode.AttributeAsNSString("space").IntValue();
            XElement xMLNode2 = xmlnode.FindChildWithTagNameRecursively("chars", false);
            XElement xMLNode3 = xmlnode.FindChildWithTagNameRecursively("kerning", false);
            string data = xMLNode2.ValueAsNSString();
            if (xMLNode3 != null)
            {
                _ = xMLNode3.ValueAsNSString();
            }
            Font font = new Font().InitWithVariableSizeCharscharMapFileKerning(data, (CTRTexture2D)LoadResource(resID, ResourceType.IMAGE), null);
            font.SetCharOffsetLineOffsetSpaceWidth(num, num2, num3);
            return font;
        }

        public virtual CTRTexture2D LoadTextureImageInfo(string path, XElement i, bool isWvga, float scaleX, float scaleY)
        {
            i ??= XElementExtensions.LoadContentXml(path);
            if (i == null)
            {
                return null;
            }
            bool flag = (i.AttributeAsNSString("filter").IntValue() & 1) == 1;
            int defaultAlphaPixelFormat = i.AttributeAsNSString("format").IntValue();
            string text = FullPathFromRelativePath(path);
            if (flag)
            {
                CTRTexture2D.SetAntiAliasTexParameters();
            }
            else
            {
                CTRTexture2D.SetAliasTexParameters();
            }
            CTRTexture2D.SetDefaultAlphaPixelFormat((CTRTexture2D.Texture2DPixelFormat)defaultAlphaPixelFormat);
            CTRTexture2D texture2D = new CTRTexture2D().InitWithPath(text, true)
                ?? throw new FileNotFoundException("texture not found: " + text, text);
            CTRTexture2D.SetDefaultAlphaPixelFormat(CTRTexture2D.kTexture2DPixelFormat_Default);
            if (isWvga)
            {
                texture2D.SetWvga();
            }
            texture2D.SetScale(scaleX, scaleY);
            SetTextureInfo(texture2D, i, isWvga, scaleX, scaleY);
            return texture2D;
        }

        public virtual void SetTextureInfo(CTRTexture2D t, XElement i, bool isWvga, float scaleX, float scaleY)
        {
            t.preCutSize = vectUndefined;
            XElement xMLNode = i.FindChildWithTagNameRecursively("quads", false);
            if (xMLNode != null)
            {
                List<string> list = xMLNode.ValueAsNSString().ComponentsSeparatedByString(',');
                if (list != null && list.Count > 0)
                {
                    float[] array = new float[list.Count];
                    for (int j = 0; j < list.Count; j++)
                    {
                        array[j] = list[j].FloatValue();
                    }
                    SetQuadsInfo(t, array, list.Count, scaleX, scaleY);
                }
            }
            XElement xMLNode2 = i.FindChildWithTagNameRecursively("offsets", false);
            if (xMLNode2 == null)
            {
                return;
            }
            List<string> list2 = xMLNode2.ValueAsNSString().ComponentsSeparatedByString(',');
            if (list2 == null || list2.Count <= 0)
            {
                return;
            }
            float[] array2 = new float[list2.Count];
            for (int k = 0; k < list2.Count; k++)
            {
                array2[k] = list2[k].FloatValue();
            }
            SetOffsetsInfo(t, array2, list2.Count, scaleX, scaleY);
            XElement xMLNode3 = i.FindChildWithTagNameRecursively("preCutWidth", false);
            XElement xMLNode4 = i.FindChildWithTagNameRecursively("preCutHeight", false);
            if (xMLNode3 != null && xMLNode4 != null)
            {
                t.preCutSize = Vect(xMLNode3.ValueAsNSString().IntValue(), xMLNode4.ValueAsNSString().IntValue());
                if (isWvga)
                {
                    t.preCutSize.x /= 1.5f;
                    t.preCutSize.y /= 1.5f;
                }
            }
        }

        private static string FullPathFromRelativePath(string relPath)
        {
            return ContentFolder + relPath;
        }

        private static void SetQuadsInfo(CTRTexture2D t, float[] data, int size, float scaleX, float scaleY)
        {
            int num = data.Length / 4;
            t.SetQuadsCapacity(num);
            int num2 = -1;
            for (int i = 0; i < num; i++)
            {
                int num3 = i * 4;
                CTRRectangle rect = MakeRectangle(data[num3], data[num3 + 1], data[num3 + 2], data[num3 + 3]);
                if (num2 < rect.h + rect.y)
                {
                    num2 = (int)Ceil((double)(rect.h + rect.y));
                }
                rect.x /= scaleX;
                rect.y /= scaleY;
                rect.w /= scaleX;
                rect.h /= scaleY;
                t.SetQuadAt(rect, i);
            }
            if (num2 != -1)
            {
                t._lowypoint = num2;
            }
            CTRTexture2D.OptimizeMemory();
        }

        private static void SetOffsetsInfo(CTRTexture2D t, float[] data, int size, float scaleX, float scaleY)
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

        public virtual bool IsWvgaResource(int r)
        {
            return r - 126 > 10;
        }

        public virtual float GetNormalScaleX(int r)
        {
            return 1f;
        }

        public virtual float GetNormalScaleY(int r)
        {
            return 1f;
        }

        public virtual float GetWvgaScaleX(int r)
        {
            return 1.5f;
        }

        public virtual float GetWvgaScaleY(int r)
        {
            return 1.5f;
        }

        public virtual void InitLoading()
        {
            loadQueue.Clear();
            loaded = 0;
            loadCount = 0;
        }

        public virtual int GetPercentLoaded()
        {
            return loadCount == 0 ? 100 : 100 * loaded / GetLoadCount();
        }

        public virtual void LoadPack(int[] pack)
        {
            int i = 0;
            while (pack[i] != -1)
            {
                AddResourceToLoadQueue(pack[i]);
                i++;
            }
        }

        public virtual void FreePack(int[] pack)
        {
            int i = 0;
            while (pack[i] != -1)
            {
                FreeResource(pack[i]);
                i++;
            }
        }

        public virtual void LoadImmediately()
        {
            while (loadQueue.Count != 0)
            {
                int resId = loadQueue[0];
                loadQueue.RemoveAt(0);
                LoadResource(resId);
                loaded++;
            }
        }

        public virtual void StartLoading()
        {
            if (resourcesDelegate != null)
            {
                DelayedDispatcher.DispatchFunc dispatchFunc = new(Rmgr_internalUpdate);
                Timer = TimerManager.Schedule(dispatchFunc, this, 0.022222223f);
            }
            bUseFake = loadQueue.Count < 100;
        }

        private int GetLoadCount()
        {
            return !bUseFake ? loadCount : 100;
        }

        public void Update()
        {
            if (loadQueue.Count > 0)
            {
                int resId = loadQueue[0];
                loadQueue.RemoveAt(0);
                LoadResource(resId);
            }
            loaded++;
            if (loaded >= GetLoadCount())
            {
                if (Timer >= 0)
                {
                    TimerManager.StopTimer(Timer);
                }
                Timer = -1;
                resourcesDelegate.AllResourcesLoaded();
            }
        }

        private static void Rmgr_internalUpdate(FrameworkTypes obj)
        {
            ((ResourceMgr)obj).Update();
        }

        private void LoadResource(int resId)
        {
            if (150 < resId)
            {
                return;
            }
            if (10 == resId)
            {
                xmlStrings ??= XElementExtensions.LoadContentXml("menu_strings.xml");
                return;
            }
            if (IsSound(resId))
            {
                _ = Application.SharedSoundMgr().GetSound(resId);
                return;
            }
            if (IsFont(resId))
            {
                _ = Application.GetFont(resId);
                return;
            }
            try
            {
                _ = Application.GetTexture(resId);
            }
            catch (Exception)
            {
            }
        }

        public virtual void FreeResource(int resId)
        {
            if (150 < resId)
            {
                return;
            }
            if (10 == resId)
            {
                xmlStrings = null;
                return;
            }
            if (IsSound(resId))
            {
                Application.SharedSoundMgr().FreeSound(resId);
                return;
            }
            if (s_Resources.TryGetValue(resId, out object value))
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _ = s_Resources.Remove(resId);
            }
        }

        public IResourceMgrDelegate resourcesDelegate;

        /// <summary>Stores all cached resources (textures, fonts, sounds, strings)</summary>
        private readonly Dictionary<int, object> s_Resources = [];

        private XElement xmlStrings;

        private int loaded;

        private int loadCount;

        private readonly List<int> loadQueue = [];

        private int Timer;

        private bool bUseFake;

        public enum ResourceType
        {
            IMAGE,
            FONT,
            SOUND,
            BINARY,
            STRINGS,
            ELEMENT
        }
    }
}
