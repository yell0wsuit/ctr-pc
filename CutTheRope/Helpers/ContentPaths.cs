using System.IO;

using CutTheRope.GameMain;

namespace CutTheRope.Helpers
{
    /// <summary>
    /// Centralized content path management.
    /// </summary>
    internal static class ContentPaths
    {
        /// <summary>
        /// The root content directory name.
        /// </summary>
        public const string RootDirectory = "content";

        /// <summary>
        /// The subdirectory for level files.
        /// </summary>
        public const string MapsDirectory = "maps";

        /// <summary>
        /// The subdirectory for SD video files.
        /// </summary>
        public const string VideoDirectory = "video";

        /// <summary>
        /// The subdirectory for HD video files.
        /// </summary>
        public const string VideoHdDirectory = "video_hd";

        /// <summary>
        /// The subdirectory for music files.
        /// </summary>
        public const string SoundsDirectory = "sounds";

        /// <summary>
        /// The subdirectory for sound effects.
        /// </summary>
        public const string SoundsSfxDirectory = "sfx";

        /// <summary>
        /// The classic XML resource data filename.
        /// </summary>
        public const string ResourceDataFile = "res_data_phone_full.xml";

        /// <summary>
        /// The menu strings XML filename.
        /// </summary>
        public const string MenuStringsFile = "menu_strings.xml";

        /// <summary>
        /// The Texture Packer registry JSON filename.
        /// </summary>
        public const string TexturePackerRegistryFile = "TexturePackerRegistry.json";

        /// <summary>
        /// The box packs configuration XML filename.
        /// </summary>
        public const string PacksConfigFile = "packs.xml";

        /// <summary>
        /// Gets the full path to a content file, including the root directory and optional content folder.
        /// </summary>
        /// <param name="relativePath">The relative path from the content root (e.g., "maps/1_1.xml")</param>
        /// <returns>The full content path (e.g., "content/maps/1_1.xml")</returns>
        public static string GetContentPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return RootDirectory;
            }

            // Normalize path separators to forward slashes for consistency
            relativePath = relativePath.Replace('\\', '/');

            // Combine with content folder if configured
            string pathWithFolder = string.IsNullOrEmpty(ResDataPhoneFull.ContentFolder)
                ? relativePath
                : Path.Combine(ResDataPhoneFull.ContentFolder, relativePath).Replace('\\', '/');

            return $"{RootDirectory}/{pathWithFolder}";
        }

        /// <summary>
        /// Gets the path to a level file.
        /// </summary>
        /// <param name="mapFileName">The level filename (e.g., "1_1.xml")</param>
        /// <returns>The full path to the level file</returns>
        public static string GetMapPath(string mapFileName)
        {
            return GetContentPath($"{MapsDirectory}/{mapFileName}");
        }

        /// <summary>
        /// Gets the full path to the Texture Packer registry file.
        /// </summary>
        public static string GetTexturePackerRegistryPath()
        {
            return GetContentPath(TexturePackerRegistryFile);
        }

        /// <summary>
        /// Gets the full path to the classic XML resource data file.
        /// </summary>
        public static string GetResourceDataPath()
        {
            return GetContentPath(ResourceDataFile);
        }

        /// <summary>
        /// Gets the full path to the menu strings file.
        /// </summary>
        public static string GetMenuStringsPath()
        {
            return GetContentPath(MenuStringsFile);
        }

        /// <summary>
        /// Gets the full path to the box packs configuration file.
        /// </summary>
        public static string GetPacksConfigPath()
        {
            return GetContentPath(PacksConfigFile);
        }

        /// <summary>
        /// Gets a relative path with the content folder prefix applied (for use with TitleContainer.OpenStream).
        /// </summary>
        /// <param name="relativePath">The relative path from the content root</param>
        /// <returns>The path with content folder prefix</returns>
        public static string GetRelativePathWithContentFolder(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            // Normalize path separators
            relativePath = relativePath.Replace('\\', '/');

            return string.IsNullOrEmpty(ResDataPhoneFull.ContentFolder)
                ? relativePath
                : $"{ResDataPhoneFull.ContentFolder}{relativePath}";
        }

        /// <summary>
        /// Gets the video directory path based on screen width (SD or HD).
        /// </summary>
        /// <param name="fileName">The video filename</param>
        /// <param name="screenWidth">The screen width to determine whether to pick SD or HD files</param>
        /// <returns>The relative path to the video file</returns>
        public static string GetVideoPath(string fileName, int screenWidth)
        {
            string videoDir = screenWidth <= 1024 ? VideoDirectory : VideoHdDirectory;
            return $"{videoDir}/{fileName}";
        }

        /// <summary>
        /// Gets the path to a sound effect file.
        /// </summary>
        /// <param name="fileName">The sound effect filename</param>
        /// <returns>The relative path to the sound effect file (e.g., "sounds/sfx/tap")</returns>
        public static string GetSoundEffectPath(string fileName)
        {
            return $"{SoundsDirectory}/{SoundsSfxDirectory}/{fileName}";
        }

        /// <summary>
        /// Gets the path to a music file.
        /// </summary>
        /// <param name="fileName">The music filename</param>
        /// <returns>The relative path to the music file (e.g., "sounds/menu_music")</returns>
        public static string GetMusicPath(string fileName)
        {
            return $"{SoundsDirectory}/{fileName}";
        }
    }
}
