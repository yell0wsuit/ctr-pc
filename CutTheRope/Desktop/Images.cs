using System;
using System.Collections.Generic;

using CutTheRope.Helpers;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    /// <summary>
    /// Loads and caches image content through per-asset content managers.
    /// </summary>
    internal sealed class Images
    {
        /// <summary>
        /// Returns the content manager used to load and unload a specific image asset.
        /// </summary>
        /// <param name="imgName">The image asset name.</param>
        /// <returns>The content manager associated with <paramref name="imgName"/>.</returns>
        private static ContentManager GetContentManager(string imgName)
        {
            _ = _contentManagers.TryGetValue(imgName, out ContentManager value);
            if (value == null)
            {
                value = new ContentManager(Global.XnaGame.Services, ContentPaths.RootDirectory);
                _contentManagers.Add(imgName, value);
            }
            return value;
        }

        /// <summary>
        /// Loads an image texture by asset name.
        /// </summary>
        /// <param name="imgName">The image asset name.</param>
        /// <returns>The loaded texture, or <see langword="null"/> if loading fails.</returns>
        public static Texture2D Get(string imgName)
        {
            ContentManager contentManager = GetContentManager(imgName);
            Texture2D result = null;
            Texture2D texture2D;
            try
            {
                result = contentManager.Load<Texture2D>(imgName);
                texture2D = result;
            }
            catch (Exception)
            {
                texture2D = result;
            }
            return texture2D;
        }

        /// <summary>
        /// Unloads the cached content manager for the specified image asset.
        /// </summary>
        /// <param name="imgName">The image asset name.</param>
        public static void Free(string imgName)
        {
            GetContentManager(imgName).Unload();
        }

        /// <summary>
        /// Stores per-image content managers so individual assets can be unloaded independently.
        /// </summary>
        private static readonly Dictionary<string, ContentManager> _contentManagers = [];
    }
}
