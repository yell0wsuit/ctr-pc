using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Game-specific <see cref="ResourceMgr"/> subclass that handles localized resource variants,
    /// language-dependent quad lookups, and texture atlas configuration.
    /// </summary>
    internal sealed class CTRResourceMgr : ResourceMgr
    {
        /// <summary>
        /// Adjusts a resource name for the active language when localized variants exist.
        /// </summary>
        /// <param name="resourceName">The string name of the resource to look up.</param>
        /// <returns>The localized resource name variant, or <paramref name="resourceName"/> unchanged if no variant exists.</returns>
        public static string HandleLocalizedResource(string resourceName)
        {
            return string.IsNullOrEmpty(resourceName)
                ? resourceName
                : resourceName switch
                {
                    _ when resourceName == Resources.Img.MenuExtraButtonsEn => LanguageHelper.Current switch
                    {
                        Language.LANGEN => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGRU => Resources.Img.MenuExtraButtonsRu,
                        Language.LANGDE => Resources.Img.MenuExtraButtonsGr,
                        Language.LANGFR => Resources.Img.MenuExtraButtonsFr,
                        Language.LANGES => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGIT => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGNL => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGPTBR => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGKO => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGJA => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGZH => Resources.Img.MenuExtraButtonsEn,
                        Language.LANGZHTW => Resources.Img.MenuExtraButtonsEn,
                        _ => Resources.Img.MenuExtraButtonsEn,
                    },
                    _ => resourceName,
                };
        }

        /// <summary>
        /// Resolves a localized XNA resource name for a string resource name.
        /// </summary>
        /// <param name="resourceName">The string name of the resource.</param>
        /// <returns>The localized resource name.</returns>
        public static string XNA_ResName(string resourceName)
        {
            return HandleLocalizedResource(resourceName);
        }

        /// <summary>
        /// Returns the texture quad index for the localized result stamp overlay.
        /// </summary>
        /// <returns>Quad index for the current language's result stamp.</returns>
        public static int GetResultStampQuad()
        {
            return LanguageHelper.Current switch
            {
                Language.LANGEN => 17,
                Language.LANGFR => 18,
                Language.LANGDE => 19,
                Language.LANGRU => 20,
                Language.LANGPTBR => 21,
                Language.LANGES => 22,
                Language.LANGIT => 23,
                Language.LANGJA => 24,
                Language.LANGKO => 25,
                Language.LANGNL => 26,
                Language.LANGZH => 27,
                Language.LANGZHTW => 27, // no zh_tw stamp, use English
                _ => 17,
            };
        }

        /// <summary>
        /// Returns the texture quad offset for the localized HUD button sprite.
        /// </summary>
        /// <returns>Quad offset for the current language's HUD button.</returns>
        public static int GetHudButtonQuadOffset()
        {
            return LanguageHelper.Current switch
            {
                Language.LANGEN => 12,
                Language.LANGDE => 13,
                Language.LANGRU => 14,
                Language.LANGJA => 15,
                Language.LANGKO => 16,
                Language.LANGZH => 17,
                Language.LANGZHTW => 17, // use zh
                Language.LANGES => 18,
                Language.LANGFR => 12,
                Language.LANGIT => 12,
                Language.LANGNL => 12,
                Language.LANGPTBR => 12,
                _ => 12, // en fallback
            };
        }

        /// <summary>
        /// Loads a resource by its string name. Auto-assigns an ID if needed.
        /// </summary>
        /// <param name="resourceName">The string name of the resource to load.</param>
        /// <param name="resType">The type of resource to load (image, sound, etc.).</param>
        /// <returns>The loaded resource object.</returns>
        public static object LoadResourceByName(string resourceName, ResourceType resType)
        {
            CTRResourceMgr mgr = new();
            return mgr.LoadResource(resourceName, resType);
        }

        /// <inheritdoc />
        protected override TextureAtlasConfig GetTextureAtlasConfig(string resourceName)
        {
            // Background images don't need JSON atlas - dimensions auto-detected from texture
            if (Resources.IsBackgroundImg(resourceName))
            {
                return null;
            }

            // Convention-based: all textures use JSON+PNG pairs in images folder
            return new TextureAtlasConfig
            {
                AtlasPath = ContentPaths.GetImagePath(resourceName, ".json"),
                ResourceName = resourceName,
                UseAntialias = true,
                CenterOffsets = false,
                ScaleRes = null
            };
        }

        /// <inheritdoc />
        protected override float GetAspectRatioScaleX()
        {
            int width = Global.ScreenSizeManager.CurrentSize.Width;
            int height = Global.ScreenSizeManager.CurrentSize.Height;
            if (width <= 0 || height <= 0)
            {
                return 1f;
            }

            // iOS ScreenSizeMgr derives ASPECT_RATIO from logical 640x960 fit scale.
            // This mirrors min(scaleX, scaleY) against that logical reference size.
            float scaleByWidth = width / 640f;
            float scaleByHeight = height / 960f;
            float scale = MathF.Min(scaleByWidth, scaleByHeight);
            return scale > 0f ? scale : 1f;
        }

    }
}
