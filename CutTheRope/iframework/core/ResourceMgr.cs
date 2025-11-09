using CutTheRope.game;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CutTheRope.iframework.core
{
    internal class ResourceMgr : NSObject
    {
        public virtual bool hasResource(int resID)
        {
            NSObject value = null;
            s_Resources.TryGetValue(resID, out value);
            return value != null;
        }

        public virtual void addResourceToLoadQueue(int resID)
        {
            loadQueue.Add(resID);
            loadCount++;
        }

        public void clearCachedResources()
        {
            s_Resources = new Dictionary<int, NSObject>();
        }

        public virtual NSObject loadResource(int resID, ResourceType resType)
        {
            NSObject value = null;
            if (s_Resources.TryGetValue(resID, out value))
            {
                return value;
            }
            string path = (resType != ResourceType.STRINGS) ? CTRResourceMgr.XNA_ResName(resID) : "";
            bool flag = false;
            float scaleX = getNormalScaleX(resID);
            float scaleY = getNormalScaleY(resID);
            if (flag)
            {
                scaleX = getWvgaScaleX(resID);
                scaleY = getWvgaScaleY(resID);
            }
            switch (resType)
            {
                case ResourceType.IMAGE:
                    value = loadTextureImageInfo(path, null, flag, scaleX, scaleY);
                    break;
                case ResourceType.FONT:
                    value = loadVariableFontInfo(path, resID, flag);
                    s_Resources.Remove(resID);
                    break;
                case ResourceType.SOUND:
                    value = loadSoundInfo(path);
                    break;
                case ResourceType.STRINGS:
                    value = loadStringsInfo(resID);
                    value = NSS(value.ToString().Replace('\u00a0', ' '));
                    break;
            }
            if (value != null)
            {
                s_Resources.Add(resID, value);
            }
            return value;
        }

        public virtual NSObject loadSoundInfo(string path)
        {
            return new NSObject().init();
        }

        public NSString loadStringsInfo(int key)
        {
            key &= 65535;
            if (xmlStrings == null)
            {
                xmlStrings = XMLNode.parseXML("menu_strings.xml");
            }
            XMLNode xMLNode = null;
            try
            {
                xMLNode = xmlStrings.childs()[key];
            }
            catch (Exception)
            {
            }
            if (xMLNode != null)
            {
                string tag = "en";
                if (LANGUAGE == Language.LANG_RU)
                {
                    tag = "ru";
                }
                if (LANGUAGE == Language.LANG_FR)
                {
                    tag = "fr";
                }
                if (LANGUAGE == Language.LANG_DE)
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
            Font font = new Font().initWithVariableSizeCharscharMapFileKerning(data, (CTRTexture2D)loadResource(resID, ResourceType.IMAGE), null);
            font.setCharOffsetLineOffsetSpaceWidth((float)num, (float)num2, (float)num3);
            return font;
        }

        public virtual CTRTexture2D loadTextureImageInfo(string path, XMLNode i, bool isWvga, float scaleX, float scaleY)
        {
            if (i == null)
            {
                i = XMLNode.parseXML(path);
            }
            bool flag = (i["filter"].intValue() & 1) == 1;
            int defaultAlphaPixelFormat = i["format"].intValue();
            string text = fullPathFromRelativePath(path);
            if (flag)
            {
                CTRTexture2D.setAntiAliasTexParameters();
            }
            else
            {
                CTRTexture2D.setAliasTexParameters();
            }
            CTRTexture2D.setDefaultAlphaPixelFormat((CTRTexture2D.Texture2DPixelFormat)defaultAlphaPixelFormat);
            CTRTexture2D texture2D = new CTRTexture2D().initWithPath(text, true);
            if (texture2D == null)
            {
                throw new Exception("texture not found: " + text);
            }
            CTRTexture2D.setDefaultAlphaPixelFormat(CTRTexture2D.kTexture2DPixelFormat_Default);
            if (isWvga)
            {
                texture2D.setWvga();
            }
            texture2D.setScale(scaleX, scaleY);
            setTextureInfo(texture2D, i, isWvga, scaleX, scaleY);
            return texture2D;
        }

        public virtual void setTextureInfo(CTRTexture2D t, XMLNode i, bool isWvga, float scaleX, float scaleY)
        {
            t.preCutSize = vectUndefined;
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
                    setQuadsInfo(t, array, list.Count, scaleX, scaleY);
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
            setOffsetsInfo(t, array2, list2.Count, scaleX, scaleY);
            XMLNode xMLNode3 = i.findChildWithTagNameRecursively(NSS("preCutWidth"), false);
            XMLNode xMLNode4 = i.findChildWithTagNameRecursively(NSS("preCutHeight"), false);
            if (xMLNode3 != null && xMLNode4 != null)
            {
                t.preCutSize = vect((float)xMLNode3.data.intValue(), (float)xMLNode4.data.intValue());
                if (isWvga)
                {
                    t.preCutSize.x = t.preCutSize.x / 1.5f;
                    t.preCutSize.y = t.preCutSize.y / 1.5f;
                }
            }
        }

        private static string fullPathFromRelativePath(string relPath)
        {
            return ContentFolder + relPath;
        }

        private void setQuadsInfo(CTRTexture2D t, float[] data, int size, float scaleX, float scaleY)
        {
            int num = data.Length / 4;
            t.setQuadsCapacity(num);
            int num2 = -1;
            for (int i = 0; i < num; i++)
            {
                int num3 = i * 4;
                CTRRectangle rect = MakeRectangle(data[num3], data[num3 + 1], data[num3 + 2], data[num3 + 3]);
                if ((float)num2 < rect.h + rect.y)
                {
                    num2 = (int)ceil((double)(rect.h + rect.y));
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

        private void setOffsetsInfo(CTRTexture2D t, float[] data, int size, float scaleX, float scaleY)
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

        public virtual bool isWvgaResource(int r)
        {
            return r - 126 > 10;
        }

        public virtual float getNormalScaleX(int r)
        {
            return 1f;
        }

        public virtual float getNormalScaleY(int r)
        {
            return 1f;
        }

        public virtual float getWvgaScaleX(int r)
        {
            return 1.5f;
        }

        public virtual float getWvgaScaleY(int r)
        {
            return 1.5f;
        }

        public virtual void initLoading()
        {
            loadQueue.Clear();
            loaded = 0;
            loadCount = 0;
        }

        public virtual int getPercentLoaded()
        {
            if (loadCount == 0)
            {
                return 100;
            }
            return 100 * loaded / getLoadCount();
        }

        public virtual void loadPack(int[] pack)
        {
            int i = 0;
            while (pack[i] != -1)
            {
                addResourceToLoadQueue(pack[i]);
                i++;
            }
        }

        public virtual void freePack(int[] pack)
        {
            int i = 0;
            while (pack[i] != -1)
            {
                freeResource(pack[i]);
                i++;
            }
        }

        public virtual void loadImmediately()
        {
            while (loadQueue.Count != 0)
            {
                int resId = loadQueue[0];
                loadQueue.RemoveAt(0);
                loadResource(resId);
                loaded++;
            }
        }

        public virtual void startLoading()
        {
            if (resourcesDelegate != null)
            {
                DelayedDispatcher.DispatchFunc dispatchFunc = new(rmgr_internalUpdate);
                Timer = NSTimer.schedule(dispatchFunc, this, 0.022222223f);
            }
            bUseFake = loadQueue.Count < 100;
        }

        private int getLoadCount()
        {
            if (!bUseFake)
            {
                return loadCount;
            }
            return 100;
        }

        public void update()
        {
            if (loadQueue.Count > 0)
            {
                int resId = loadQueue[0];
                loadQueue.RemoveAt(0);
                loadResource(resId);
            }
            loaded++;
            if (loaded >= getLoadCount())
            {
                if (Timer >= 0)
                {
                    NSTimer.stopTimer(Timer);
                }
                Timer = -1;
                resourcesDelegate.allResourcesLoaded();
            }
        }

        private static void rmgr_internalUpdate(NSObject obj)
        {
            ((ResourceMgr)obj).update();
        }

        private void loadResource(int resId)
        {
            if (150 < resId)
            {
                return;
            }
            if (10 == resId)
            {
                if (xmlStrings == null)
                {
                    xmlStrings = XMLNode.parseXML("menu_strings.xml");
                }
                return;
            }
            if (isSound(resId))
            {
                Application.sharedSoundMgr().getSound(resId);
                return;
            }
            if (isFont(resId))
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

        public virtual void freeResource(int resId)
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
            if (isSound(resId))
            {
                Application.sharedSoundMgr().freeSound(resId);
                return;
            }
            NSObject value = null;
            if (s_Resources.TryGetValue(resId, out value))
            {
                value?.dealloc();
                s_Resources.Remove(resId);
            }
        }

        public ResourceMgrDelegate resourcesDelegate;

        private Dictionary<int, NSObject> s_Resources = new();

        private XMLNode xmlStrings;

        private int loaded;

        private int loadCount;

        private List<int> loadQueue = new();

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
