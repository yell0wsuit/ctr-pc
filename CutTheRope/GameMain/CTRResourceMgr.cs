using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

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
            Dictionary<string, TextureAtlasConfig> configs = LoadTexturePackerRegistry();
            return configs.TryGetValue(resourceName, out TextureAtlasConfig config) ? config : null;
        }

        private static Dictionary<string, TextureAtlasConfig> LoadTexturePackerRegistry()
        {
            Dictionary<string, TextureAtlasConfig> result = [];

            try
            {
                string registryPath = ContentPaths.GetTexturePackerRegistryPath();
                string json = TryReadContentText(registryPath);
                if (string.IsNullOrEmpty(json))
                {
                    Console.WriteLine($"TexturePackerRegistry not found at \"{registryPath}\". TexturePacker atlases will fall back to legacy XML data.");
                    return result; // Return empty dict if no registry file
                }

                using JsonDocument doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("textures", out JsonElement texturesElement) ||
                    texturesElement.ValueKind != JsonValueKind.Array)
                {
                    return result;
                }

                foreach (JsonElement textureElement in texturesElement.EnumerateArray())
                {
                    if (!textureElement.TryGetProperty("resourceName", out JsonElement resourceNameElement) ||
                        !textureElement.TryGetProperty("atlasPath", out JsonElement atlasPathElement))
                    {
                        continue;
                    }

                    string resourceName = resourceNameElement.GetString();
                    string atlasPath = atlasPathElement.GetString();

                    if (string.IsNullOrEmpty(resourceName))
                    {
                        throw new InvalidDataException($"TexturePackerRegistry entry is missing a resourceName.");
                    }

                    TextureAtlasConfig config = new()
                    {
                        Format = TextureAtlasFormat.TexturePackerJson,
                        AtlasPath = atlasPath,
                        ResourceName = resourceName,
                        UseAntialias = GetBoolProperty(textureElement, "useAntialias", true),
                        FrameOrder = GetStringArrayProperty(textureElement, "frameOrder"),
                        CenterOffsets = GetBoolProperty(textureElement, "centerOffsets", false)
                    };

                    result[resourceName] = config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load TexturePackerRegistry.json: {ex.Message}");
            }

            return result;
        }

        private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
        {
            return (element.TryGetProperty(propertyName, out JsonElement prop) &&
                prop.ValueKind == JsonValueKind.True) || ((!element.TryGetProperty(propertyName, out prop) ||
                prop.ValueKind != JsonValueKind.False) && defaultValue);
        }

        private static string[] GetStringArrayProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement prop) ||
                prop.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            List<string> result = [];
            foreach (JsonElement item in prop.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    result.Add(item.GetString());
                }
            }

            return result.Count > 0 ? [.. result] : null;
        }

        private static string TryReadContentText(string relativePath)
        {
            try
            {
                using Stream stream = TitleContainer.OpenStream(relativePath);
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open \"{relativePath}\": {ex.Message}");
                return null;
            }
        }

    }
}
