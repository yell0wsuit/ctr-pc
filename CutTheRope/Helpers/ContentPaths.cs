#if !MACOS_AVFOUNDATION
using System;
#endif
using System.IO;

#if MACOS_AVFOUNDATION
using Foundation;
#endif

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
        /// The subdirectory for font files.
        /// </summary>
        public const string FontsDirectory = "fonts";

        /// <summary>
        /// The subdirectory for texture images (JSON+PNG pairs).
        /// </summary>
        public const string ImagesDirectory = "images";

        /// <summary>
        /// The subdirectory for background images without JSON atlas.
        /// </summary>
        public static readonly string BackgroundsDirectory = Path.Combine(ImagesDirectory, "backgrounds");

        /// <summary>
        /// The menu strings JSON filename.
        /// </summary>
        public const string MenuStringsFile = "menu_strings.json";

        /// <summary>
        /// The box packs configuration XML filename.
        /// </summary>
        public const string PacksConfigFile = "packs.xml";

        /// <summary>
        /// Gets the full path to a content file, including the root directory.
        /// </summary>
        /// <param name="relativePath">The relative path from the content root (e.g., "maps/1_1.xml")</param>
        /// <returns>The full content path (e.g., "content/maps/1_1.xml")</returns>
        public static string GetContentPath(string relativePath)
        {
            return string.IsNullOrWhiteSpace(relativePath) ? RootDirectory : Path.Combine(RootDirectory, relativePath);
        }

        /// <summary>
        /// Gets the path to a level file.
        /// </summary>
        /// <param name="mapFileName">The level filename (e.g., "1_1.xml")</param>
        /// <returns>The full path to the level file</returns>
        public static string GetMapPath(string mapFileName)
        {
            return GetContentPath(Path.Combine(MapsDirectory, mapFileName));
        }

        /// <summary>
        /// Gets the full path to a texture image resource (JSON or PNG).
        /// Use for TitleContainer.OpenStream and direct file access.
        /// </summary>
        /// <param name="resourceName">The resource name (e.g., "obj_ghost" or "candies/obj_candy_02")</param>
        /// <param name="extension">The file extension (e.g., ".json" or ".png")</param>
        /// <returns>The full path to the image file (e.g., "content/images/obj_ghost.json")</returns>
        public static string GetImagePath(string resourceName, string extension)
        {
            return Path.Combine(RootDirectory, ImagesDirectory, resourceName + extension);
        }

        /// <summary>
        /// Gets the ContentManager-relative path to a texture image resource.
        /// Use for ContentManager.Load which already has "content" as root.
        /// </summary>
        /// <param name="resourceName">The resource name (e.g., "obj_ghost" or "candies/obj_candy_02")</param>
        /// <returns>The relative path from content root (e.g., "images/obj_ghost")</returns>
        public static string GetImageContentPath(string resourceName)
        {
            return Path.Combine(ImagesDirectory, resourceName);
        }

        /// <summary>
        /// Gets the ContentManager-relative path to a background image resource.
        /// Use for ContentManager.Load which already has "content" as root.
        /// </summary>
        /// <param name="resourceName">The resource name (e.g., "bgr_01_p1")</param>
        /// <returns>The relative path from content root (e.g., "backgrounds/bgr_01_p1")</returns>
        public static string GetBackgroundImageContentPath(string resourceName)
        {
            return Path.Combine(BackgroundsDirectory, resourceName);
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
        /// Gets the absolute path to the content root directory for the current runtime context.
        /// </summary>
        public static string GetContentRootAbsolute()
        {
#if MACOS_AVFOUNDATION
            string basePath = NSBundle.MainBundle.ResourcePath;
            return Path.Combine(basePath, RootDirectory);
#else
            string basePath = AppContext.BaseDirectory;
            DirectoryInfo dir = new(basePath);

            while (dir != null)
            {
                if (dir.Name.Equals("MacOS", StringComparison.OrdinalIgnoreCase) &&
                    dir.Parent?.Name.Equals("Contents", StringComparison.OrdinalIgnoreCase) == true &&
                    dir.Parent.Parent?.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Path.Combine(dir.Parent.FullName, "Resources", RootDirectory);
                }

                dir = dir.Parent;
            }

            return Path.Combine(basePath, RootDirectory);
#endif
        }

        /// <summary>
        /// The video file extension.
        /// </summary>
        public const string VideoExtension = ".mp4";

        /// <summary>
        /// Gets the path to an HD video file.
        /// </summary>
        /// <param name="fileName">The video filename without extension</param>
        /// <returns>The relative path to the video file</returns>
        public static string GetVideoPath(string fileName)
        {
            return Path.Combine(VideoHdDirectory, fileName + VideoExtension);
        }

        /// <summary>
        /// Gets the path to a sound effect file.
        /// </summary>
        /// <param name="fileName">The sound effect filename</param>
        /// <returns>The relative path to the sound effect file (e.g., "sounds/sfx/tap")</returns>
        public static string GetSoundEffectPath(string fileName)
        {
            return Path.Combine(SoundsDirectory, SoundsSfxDirectory, fileName);
        }

        /// <summary>
        /// Gets the path to a music file.
        /// </summary>
        /// <param name="fileName">The music filename</param>
        /// <returns>The relative path to the music file (e.g., "sounds/menu_music")</returns>
        public static string GetMusicPath(string fileName)
        {
            return Path.Combine(SoundsDirectory, fileName);
        }

        /// <summary>
        /// Gets the full path to a font file.
        /// </summary>
        /// <param name="fileName">The font filename</param>
        /// <returns>The full path to the font file (e.g., "content/fonts/fontname.ttf")</returns>
        public static string GetFontPath(string fileName)
        {
            return Path.Combine(RootDirectory, FontsDirectory, fileName);
        }
    }
}
