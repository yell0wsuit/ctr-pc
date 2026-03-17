using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Resource manager wrapper that preserves legacy numeric identifiers while enabling string-based lookups.
    /// </summary>
    internal sealed class CTRResourceMgr : ResourceMgr
    {
        /// <summary>
        /// Adjusts a resource name for the active language when localized variants exist.
        /// </summary>
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
                        Language.LANGZH => throw new NotImplementedException(),
                        Language.LANGJA => throw new NotImplementedException(),
                        _ => Resources.Img.MenuExtraButtonsEn,
                    },
                    _ => resourceName,
                };
        }

        /// <summary>
        /// Resolves a localized XNA resource name for a string resource name.
        /// </summary>
        public static string XNA_ResName(string resourceName)
        {
            return HandleLocalizedResource(resourceName);
        }

        public static int GetResultStampQuad()
        {
            return LanguageHelper.Current switch
            {
                Language.LANGEN => 17,
                Language.LANGFR => 18,
                Language.LANGDE => 19,
                Language.LANGRU => 20,
                Language.LANGZH => throw new NotImplementedException(),
                Language.LANGJA => throw new NotImplementedException(),
                _ => 17,
            };
        }

        public static int GetHudButtonQuadOffset()
        {
            return LanguageHelper.Current switch
            {
                Language.LANGEN => 13,
                Language.LANGFR => 13,
                Language.LANGDE => 15,
                Language.LANGRU => 17,
                Language.LANGZH => throw new NotImplementedException(),
                Language.LANGJA => throw new NotImplementedException(),
                _ => 13,
            };
        }

        /// <summary>
        /// Loads a resource by its string name. Auto-assigns an ID if needed.
        /// </summary>
        public static object LoadResourceByName(string resourceName, ResourceType resType)
        {
            CTRResourceMgr mgr = new();
            return mgr.LoadResource(resourceName, resType);
        }

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
