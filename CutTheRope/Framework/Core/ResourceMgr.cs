using System;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// Adds a resource to the load queue by resource name.
        /// </summary>
        public void AddResourceToLoadQueue(string resourceName)
        {
            if (TryResolveResource(resourceName, out string localizedName))
            {
                loadQueue.Add(localizedName);
                loadCount++;
            }
        }

        public void ClearCachedResources()
        {
            s_Resources.Clear();
        }

        /// <summary>
        /// Loads a resource using its string identifier while preserving caching semantics.
        /// </summary>
        public virtual object LoadResource(string resourceName, ResourceType resType)
        {
            return !TryResolveResource(resourceName, out string localizedName)
                ? null
                : LoadResourceInternal(localizedName, resType);
        }

        private object LoadResourceInternal(string resourceName, ResourceType resType)
        {
            if (s_Resources.TryGetValue(resourceName, out object value))
            {
                return value;
            }

            string path = CTRResourceMgr.XNA_ResName(resourceName);
            float scaleX = GetNormalScaleX(resourceName);
            float scaleY = GetNormalScaleY(resourceName);
            switch (resType)
            {
                case ResourceType.IMAGE:
                    value = LoadTextureImageInfo(resourceName, path, null, false, scaleX, scaleY);
                    break;
                case ResourceType.FONT:
                    value = LoadVariableFontInfo(path, resourceName, false);
                    _ = s_Resources.Remove(resourceName);
                    break;
                case ResourceType.SOUND:
                    value = LoadSoundInfo(path);
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
                s_Resources[resourceName] = value;
            }
            return value;
        }

        private static bool TryResolveResource(string resourceName, out string localizedName)
        {
            localizedName = string.IsNullOrEmpty(resourceName)
                ? resourceName
                : CTRResourceMgr.HandleLocalizedResource(resourceName);

            return !string.IsNullOrEmpty(localizedName) && Resources.IsValidResourceName(localizedName);
        }

        public virtual FrameworkTypes LoadSoundInfo(string path)
        {
            return new FrameworkTypes();
        }

        public virtual FontGeneric LoadVariableFontInfo(string path, string resourceName, bool isWvga)
        {
            // Check if user prefers old font system for supported languages (en, de, fr, ru)
            // Disabled because new quad system doesn't support old sprite fonts well
            bool preferOldFontSystem = false;
            bool isLanguageSupported = LanguageHelper.IsCurrentAny(
                Language.LANGEN,
                Language.LANGDE,
                Language.LANGFR,
                Language.LANGRU
            );

            if (preferOldFontSystem && isLanguageSupported)
            {
                // Use old sprite-based font system
                return LoadSpriteFontInfo(path, resourceName);
            }

            if (string.IsNullOrEmpty(resourceName))
            {
                // Fallback to old sprite font loading if no resource name found
                return LoadSpriteFontInfo(path, resourceName);
            }

            // Load FontStashSharp font using the new system
            FontConfiguration config = Resources.FontConfig.GetConfiguration(resourceName, LanguageHelper.CurrentAsInt);
            FontStashFont fontStashFont = FontManager.LoadFont(
                config.FontFile,
                config.Size,
                config.Color,
                config.Effects,
                config.LineSpacing,
                config.TopSpacing
            );

            return fontStashFont;
        }

        /// <summary>
        /// Legacy sprite font loading (kept for backward compatibility).
        /// </summary>
        private static Font LoadSpriteFontInfo(string path, string resourceName)
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
            Font font = new Font().InitWithVariableSizeCharscharMapFileKerning(data, Application.GetTexture(resourceName));
            font.SetCharOffsetLineOffsetSpaceWidth(num, num2, num3);
            return font;
        }

        public virtual CTRTexture2D LoadTextureImageInfo(string resourceName, string path, XElement i, bool isWvga, float scaleX, float scaleY)
        {
            TextureAtlasConfig atlasConfig = GetTextureAtlasConfig(resourceName);
            ParsedTexturePackerAtlas parsedAtlas = LoadTexturePackerAtlas(atlasConfig, resourceName);

            bool useAntialias = atlasConfig?.UseAntialias ?? true;
            string pngPath = Resources.IsBackgroundImg(resourceName)
                ? ContentPaths.GetBackgroundImageContentPath(resourceName)
                : ContentPaths.GetImageContentPath(resourceName);
            if (useAntialias)
            {
                CTRTexture2D.SetAntiAliasTexParameters();
            }
            else
            {
                CTRTexture2D.SetAliasTexParameters();
            }

            CTRTexture2D texture2D = new CTRTexture2D().InitWithPath(pngPath) ?? throw new FileNotFoundException(
                    $"Resource '{resourceName}' is missing the PNG. Did you forget to add {resourceName}.png?",
                    pngPath);

            if (isWvga)
            {
                texture2D.SetWvga();
            }

            texture2D.SetScale(scaleX, scaleY);

            ApplyTexturePackerInfo(texture2D, parsedAtlas, isWvga, scaleX, scaleY);

            return texture2D;
        }

        protected virtual TextureAtlasConfig GetTextureAtlasConfig(string resourceName)
        {
            return null;
        }

        private static ParsedTexturePackerAtlas LoadTexturePackerAtlas(TextureAtlasConfig config, string resourceName)
        {
            // No atlas config means use full texture (e.g., background images)
            if (config == null)
            {
                return null;
            }

            string atlasPath = config.AtlasPath;
            if (string.IsNullOrEmpty(atlasPath))
            {
                throw new FileNotFoundException(
                    $"Resource '{resourceName}' is missing the quad JSON. Did you forget to add {resourceName}.json?",
                    resourceName + ".json");
            }

            string json;
            try
            {
                using Stream stream = TitleContainer.OpenStream(atlasPath);
                using StreamReader reader = new(stream);
                json = reader.ReadToEnd();
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(
                    $"Resource '{resourceName}' is missing the quad JSON. Did you forget to add {resourceName}.json?",
                    atlasPath);
            }

            if (string.IsNullOrEmpty(json))
            {
                throw new InvalidDataException(
                    $"Resource '{resourceName}' has an empty JSON file at {atlasPath}.");
            }

            TexturePackerParserOptions options = null;
            if (config?.CenterOffsets ?? false)
            {
                options = new TexturePackerParserOptions
                {
                    NormalizeOffsetsToCenter = true
                };
            }

            return TexturePackerAtlasParser.Parse(json, options);
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
            SetQuadsInfo(texture, quadData, scaleX, scaleY);

            if (atlas.Offsets.Count == atlas.Rects.Count && atlas.HasNonZeroOffset)
            {
                float[] offsetData = new float[atlas.Offsets.Count * 2];
                for (int j = 0; j < atlas.Offsets.Count; j++)
                {
                    int offsetIndex = j * 2;
                    offsetData[offsetIndex] = atlas.Offsets[j].X;
                    offsetData[offsetIndex + 1] = atlas.Offsets[j].Y;
                }
                SetOffsetsInfo(texture, offsetData, offsetData.Length, scaleX, scaleY);
            }

            if (atlas.PreCutWidth > 0f && atlas.PreCutHeight > 0f)
            {
                texture.preCutSize = Vect(atlas.PreCutWidth, atlas.PreCutHeight);
                if (isWvga)
                {
                    texture.preCutSize.X /= 1.5f;
                    texture.preCutSize.Y /= 1.5f;
                }
            }
        }

        private static void SetQuadsInfo(CTRTexture2D t, float[] data, float scaleX, float scaleY)
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
                t.quadOffsets[i].X = data[num2];
                t.quadOffsets[i].Y = data[num2 + 1];
                Vector[] quadOffsets = t.quadOffsets;
                int num3 = i;
                quadOffsets[num3].X = quadOffsets[num3].X / scaleX;
                Vector[] quadOffsets2 = t.quadOffsets;
                int num4 = i;
                quadOffsets2[num4].Y = quadOffsets2[num4].Y / scaleY;
            }
        }

        public virtual float GetNormalScaleX(string resourceName)
        {
            return 1f;
        }

        public virtual float GetNormalScaleY(string resourceName)
        {
            return 1f;
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
                string resourceName = loadQueue[0];
                loadQueue.RemoveAt(0);
                LoadResource(resourceName);
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
                string resourceName = loadQueue[0];
                loadQueue.RemoveAt(0);
                LoadResource(resourceName);
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

        private static void LoadResource(string resourceName)
        {
            if (!TryResolveResource(resourceName, out string localizedName))
            {
                return;
            }

            if (localizedName == Resources.Str.MenuStrings)
            {
                LocalizationManager.EnsureLoaded();
                return;
            }
            if (Resources.IsSound(localizedName))
            {
                _ = Application.SharedSoundMgr().GetSound(localizedName);
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

        /// <summary>
        /// Frees a cached resource by its string identifier if it has been loaded.
        /// </summary>
        public void FreeResource(string resourceName)
        {
            if (!TryResolveResource(resourceName, out string localizedName))
            {
                return;
            }

            if (localizedName == Resources.Str.MenuStrings)
            {
                LocalizationManager.ClearCache();
                return;
            }
            if (Resources.IsSound(localizedName))
            {
                Application.SharedSoundMgr().FreeSound(localizedName);
                return;
            }
            if (s_Resources.TryGetValue(localizedName, out object value))
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _ = s_Resources.Remove(localizedName);
            }
        }

        public IResourceMgrDelegate resourcesDelegate;

        /// <summary>Stores all cached resources (textures, fonts, sounds, strings)</summary>
        private readonly Dictionary<string, object> s_Resources = [];

        private int loaded;

        private int loadCount;

        private readonly List<string> loadQueue = [];

        private int Timer;

        public enum ResourceType
        {
            IMAGE,
            FONT,
            SOUND,
            BINARY,
            ELEMENT
        }
    }
}
