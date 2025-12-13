using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.GameMain;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    internal class ResourceMgr : FrameworkTypes
    {

        public virtual bool HasResource(int resID)
        {
            return TryResolveResource(resID, out int localizedResId, out _) && s_Resources.TryGetValue(localizedResId, out _);
        }

        /// <summary>
        /// Checks whether a cached resource exists using its string identifier.
        /// </summary>
        public bool HasResource(string resourceName)
        {
            return TryResolveResource(resourceName, out int resID, out _) && HasResource(resID);
        }

        public virtual void AddResourceToLoadQueue(int resID)
        {
            if (TryResolveResource(resID, out int localizedResId, out _))
            {
                loadQueue.Add(localizedResId);
                loadCount++;
            }
        }

        /// <summary>
        /// Adds a resource to the load queue by resolving its string identifier to the legacy numeric ID.
        /// </summary>
        public void AddResourceToLoadQueue(string resourceName)
        {
            if (TryResolveResource(resourceName, out int resID, out _))
            {
                AddResourceToLoadQueue(resID);
            }
        }

        public void ClearCachedResources()
        {
            s_Resources.Clear();
        }

        public virtual object LoadResource(int resID, ResourceType resType)
        {
            return !TryResolveResource(resID, out int localizedResId, out string resourceName)
                ? null
                : LoadResourceInternal(localizedResId, resourceName, resType);
        }

        /// <summary>
        /// Loads a resource using its string identifier while preserving caching semantics.
        /// </summary>
        public virtual object LoadResource(string resourceName, ResourceType resType)
        {
            return !TryResolveResource(resourceName, out int resId, out string localizedName)
                ? null
                : LoadResourceInternal(resId, localizedName, resType);
        }

        private object LoadResourceInternal(int resId, string resourceName, ResourceType resType)
        {
            if (s_Resources.TryGetValue(resId, out object value))
            {
                return value;
            }

            string path = resType != ResourceType.STRINGS ? CTRResourceMgr.XNA_ResName(resourceName) : "";
            bool flag = false;
            float scaleX = GetNormalScaleX(resId);
            float scaleY = GetNormalScaleY(resId);
            if (flag)
            {
                scaleX = GetWvgaScaleX(resId);
                scaleY = GetWvgaScaleY(resId);
            }
            switch (resType)
            {
                case ResourceType.IMAGE:
                    value = LoadTextureImageInfo(resId, resourceName, path, null, flag, scaleX, scaleY);
                    break;
                case ResourceType.FONT:
                    value = LoadVariableFontInfo(path, resId, flag);
                    _ = s_Resources.Remove(resId);
                    break;
                case ResourceType.SOUND:
                    value = LoadSoundInfo(path);
                    break;
                case ResourceType.STRINGS:
                    {
                        string strValue = LoadStringsInfo(resId);
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
                s_Resources.Add(resId, value);
            }
            return value;
        }

        private static bool TryResolveResource(int resId, out int localizedResId, out string localizedName)
        {
            localizedName = ResourceNameTranslator.TranslateLegacyId(resId);
            if (string.IsNullOrEmpty(localizedName))
            {
                localizedResId = -1;
                return false;
            }

            return TryResolveResource(localizedName, out localizedResId, out localizedName);
        }

        private static bool TryResolveResource(string resourceName, out int resId, out string localizedName)
        {
            localizedName = string.IsNullOrEmpty(resourceName)
                ? resourceName
                : CTRResourceMgr.HandleLocalizedResource(resourceName);

            resId = ResolveResourceId(localizedName);
            return resId >= 0;
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

        public virtual CTRTexture2D LoadTextureImageInfo(int resId, string resourceName, string path, XElement i, bool isWvga, float scaleX, float scaleY)
        {
            TextureAtlasConfig atlasConfig = GetTextureAtlasConfig(resourceName);
            bool preferTexturePacker = atlasConfig?.Format == TextureAtlasFormat.TexturePackerJson;

            XElement xmlInfo = i;
            if (!preferTexturePacker)
            {
                xmlInfo ??= XElementExtensions.LoadContentXml(path);
            }

            ParsedTexturePackerAtlas parsedAtlas = null;
            if (preferTexturePacker || xmlInfo == null)
            {
                parsedAtlas = TryLoadTexturePackerAtlas(atlasConfig, path);
                if (parsedAtlas == null && preferTexturePacker && xmlInfo == null)
                {
                    xmlInfo = XElementExtensions.LoadContentXml(path);
                }
            }

            if (xmlInfo == null && parsedAtlas == null)
            {
                return null;
            }

            bool useAntialias;
            CTRTexture2D.Texture2DPixelFormat pixelFormat;
            if (parsedAtlas == null && xmlInfo != null)
            {
                useAntialias = (xmlInfo.AttributeAsNSString("filter").IntValue() & 1) == 1;
                int formatValue = xmlInfo.AttributeAsNSString("format").IntValue();
                pixelFormat = (CTRTexture2D.Texture2DPixelFormat)formatValue;
            }
            else
            {
                useAntialias = atlasConfig?.UseAntialias ?? true;
                pixelFormat = atlasConfig?.PixelFormat ?? CTRTexture2D.kTexture2DPixelFormat_Default;
            }

            string text = FullPathFromRelativePath(path);
            if (useAntialias)
            {
                CTRTexture2D.SetAntiAliasTexParameters();
            }
            else
            {
                CTRTexture2D.SetAliasTexParameters();
            }

            CTRTexture2D.SetDefaultAlphaPixelFormat(pixelFormat);
            CTRTexture2D texture2D = new CTRTexture2D().InitWithPath(text, true)
                ?? throw new FileNotFoundException("texture not found: " + text, text);
            CTRTexture2D.SetDefaultAlphaPixelFormat(CTRTexture2D.kTexture2DPixelFormat_Default);
            if (isWvga)
            {
                texture2D.SetWvga();
            }
            texture2D.SetScale(scaleX, scaleY);

            if (parsedAtlas != null)
            {
                ApplyTexturePackerInfo(texture2D, parsedAtlas, isWvga, scaleX, scaleY);
            }
            else
            {
                SetTextureInfo(texture2D, xmlInfo, isWvga, scaleX, scaleY);
            }

            return texture2D;
        }

        protected virtual TextureAtlasConfig GetTextureAtlasConfig(string resourceName)
        {
            return null;
        }

        private static ParsedTexturePackerAtlas TryLoadTexturePackerAtlas(TextureAtlasConfig config, string textureName)
        {
            string atlasPath = ResolveAtlasPath(config, textureName);
            if (string.IsNullOrEmpty(atlasPath))
            {
                return null;
            }

            string json = LoadContentText(atlasPath);
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine($"TexturePacker atlas \"{atlasPath}\" could not be loaded (missing file or empty content).");
                return null;
            }

            TexturePackerParserOptions options = null;
            if ((config?.FrameOrder?.Length ?? 0) > 0 || (config?.CenterOffsets ?? false))
            {
                options = new TexturePackerParserOptions
                {
                    FrameOrder = config?.FrameOrder,
                    NormalizeOffsetsToCenter = config?.CenterOffsets ?? false
                };
            }

            try
            {
                return TexturePackerAtlasParser.Parse(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse TexturePacker atlas \"{atlasPath}\": {ex.Message}");
            }

            return null;
        }

        private static string ResolveAtlasPath(TextureAtlasConfig config, string textureName)
        {
            string atlasPath = config?.AtlasPath;
            if (string.IsNullOrWhiteSpace(atlasPath))
            {
                atlasPath = textureName;
            }

            if (!Path.HasExtension(atlasPath))
            {
                atlasPath += ".json";
            }

            return atlasPath;
        }

        private static string LoadContentText(string relativePath)
        {
            try
            {
                string contentPath = ContentPaths.GetContentPath(relativePath);
                using Stream stream = TitleContainer.OpenStream(contentPath);
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static void ApplyTexturePackerInfo(CTRTexture2D texture, ParsedTexturePackerAtlas atlas, bool isWvga, float scaleX, float scaleY)
        {
            texture.preCutSize = vectUndefined;
            if (atlas == null || atlas.Rects.Count == 0)
            {
                return;
            }

            float[] quadData = new float[atlas.Rects.Count * 4];
            for (int i = 0; i < atlas.Rects.Count; i++)
            {
                CTRRectangle rect = atlas.Rects[i];
                int index = i * 4;
                quadData[index] = rect.x;
                quadData[index + 1] = rect.y;
                quadData[index + 2] = rect.w;
                quadData[index + 3] = rect.h;
            }
            SetQuadsInfo(texture, quadData, quadData.Length, scaleX, scaleY);

            if (atlas.Offsets.Count == atlas.Rects.Count && atlas.HasNonZeroOffset)
            {
                float[] offsetData = new float[atlas.Offsets.Count * 2];
                for (int j = 0; j < atlas.Offsets.Count; j++)
                {
                    int offsetIndex = j * 2;
                    offsetData[offsetIndex] = atlas.Offsets[j].x;
                    offsetData[offsetIndex + 1] = atlas.Offsets[j].y;
                }
                SetOffsetsInfo(texture, offsetData, offsetData.Length, scaleX, scaleY);
            }

            if (atlas.PreCutWidth > 0f && atlas.PreCutHeight > 0f)
            {
                texture.preCutSize = Vect(atlas.PreCutWidth, atlas.PreCutHeight);
                if (isWvga)
                {
                    texture.preCutSize.x /= 1.5f;
                    texture.preCutSize.y /= 1.5f;
                }
            }
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
            return ContentPaths.GetRelativePathWithContentFolder(relPath);
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

        public virtual void LoadPack(string[] pack)
        {
            if (pack == null)
            {
                return;
            }

            int i = 0;
            while (i < pack.Length && !string.IsNullOrEmpty(pack[i]))
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

        public virtual void FreePack(string[] pack)
        {
            if (pack == null)
            {
                return;
            }

            int i = 0;
            while (i < pack.Length && !string.IsNullOrEmpty(pack[i]))
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
        }

        private int GetLoadCount()
        {
            return loadCount;
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
            if (!TryResolveResource(resId, out int localizedResId, out string localizedName))
            {
                return;
            }

            if (localizedName == Resources.Str.MenuStrings)
            {
                xmlStrings ??= XElementExtensions.LoadContentXml("menu_strings.xml");
                return;
            }
            if (Resources.IsSound(localizedName))
            {
                _ = Application.SharedSoundMgr().GetSound(localizedResId);
                return;
            }
            if (Resources.IsFont(localizedName))
            {
                _ = Application.GetFont(localizedName);
                return;
            }
            try
            {
                _ = Application.GetTexture(localizedName);
            }
            catch (Exception)
            {
            }
        }

        public virtual void FreeResource(int resId)
        {
            if (!TryResolveResource(resId, out int localizedResId, out string localizedName))
            {
                return;
            }

            if (localizedName == Resources.Str.MenuStrings)
            {
                xmlStrings = null;
                return;
            }
            if (Resources.IsSound(localizedName))
            {
                Application.SharedSoundMgr().FreeSound(localizedResId);
                return;
            }
            if (s_Resources.TryGetValue(localizedResId, out object value))
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _ = s_Resources.Remove(localizedResId);
            }
        }

        /// <summary>
        /// Frees a cached resource by its string identifier if it has been loaded.
        /// </summary>
        public void FreeResource(string resourceName)
        {
            if (TryResolveResource(resourceName, out int resId, out _))
            {
                FreeResource(resId);
            }
        }

        /// <summary>
        /// Resolves the legacy numeric identifier for a string-based resource name.
        /// </summary>
        protected static int ResolveResourceId(string resourceName)
        {
            return ResourceNameTranslator.ToResourceId(resourceName);
        }

        public IResourceMgrDelegate resourcesDelegate;

        /// <summary>Stores all cached resources (textures, fonts, sounds, strings)</summary>
        private readonly Dictionary<int, object> s_Resources = [];

        private XElement xmlStrings;

        private int loaded;

        private int loadCount;

        private readonly List<int> loadQueue = [];

        private int Timer;

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
