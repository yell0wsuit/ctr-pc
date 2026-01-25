using System;

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
                    _ when resourceName == Resources.Img.HudButtonsEn => LanguageHelper.Current switch
                    {
                        Language.LANGEN => Resources.Img.HudButtonsEn,
                        Language.LANGRU => Resources.Img.HudButtonsRu,
                        Language.LANGDE => Resources.Img.HudButtonsGr,
                        Language.LANGFR => Resources.Img.HudButtonsEn,
                        Language.LANGZH => throw new NotImplementedException(),
                        Language.LANGJA => throw new NotImplementedException(),
                        _ => Resources.Img.HudButtonsEn,
                    },
                    _ when resourceName == Resources.Img.MenuResultEn => LanguageHelper.Current switch
                    {
                        Language.LANGEN => Resources.Img.MenuResultEn,
                        Language.LANGRU => Resources.Img.MenuResultRu,
                        Language.LANGDE => Resources.Img.MenuResultGr,
                        Language.LANGFR => Resources.Img.MenuResultFr,
                        Language.LANGZH => throw new NotImplementedException(),
                        Language.LANGJA => throw new NotImplementedException(),
                        _ => Resources.Img.MenuResultEn,
                    },
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
                CenterOffsets = false
            };
        }
    }
}
