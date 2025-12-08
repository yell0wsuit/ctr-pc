using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
            if (string.IsNullOrEmpty(resourceName))
            {
                return resourceName;
            }

            if (resourceName == Resources.Img.HudButtonsEn)
            {
                if (LANGUAGE == Language.LANGRU)
                {
                    return Resources.Img.HudButtonsRu;
                }
                if (LANGUAGE == Language.LANGDE)
                {
                    return Resources.Img.HudButtonsGr;
                }
                if (LANGUAGE == Language.LANGFR)
                {
                    return Resources.Img.HudButtonsEn;
                }
            }
            else if (resourceName == Resources.Img.MenuResultEn)
            {
                if (LANGUAGE == Language.LANGRU)
                {
                    return Resources.Img.MenuResultRu;
                }
                if (LANGUAGE == Language.LANGDE)
                {
                    return Resources.Img.MenuResultGr;
                }
                if (LANGUAGE == Language.LANGFR)
                {
                    return Resources.Img.MenuResultFr;
                }
            }
            else if (resourceName == Resources.Img.MenuExtraButtonsEn)
            {
                if (LANGUAGE == Language.LANGRU)
                {
                    return Resources.Img.MenuExtraButtonsRu;
                }
                if (LANGUAGE == Language.LANGDE)
                {
                    return Resources.Img.MenuExtraButtonsGr;
                }
                if (LANGUAGE == Language.LANGFR)
                {
                    return Resources.Img.MenuExtraButtonsFr;
                }
            }

            return resourceName;
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
                if (!File.Exists(registryPath))
                {
                    return result; // Return empty dict if no registry file
                }

                string json = File.ReadAllText(registryPath);
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

    }
}
