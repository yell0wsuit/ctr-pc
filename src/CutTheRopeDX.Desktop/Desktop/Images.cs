using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Platform;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
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
                value = new DesktopContentManager(Global.XnaGame.Services);
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
            try
            {
                return contentManager.Load<Texture2D>(imgName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Images] Failed to load '{imgName}': {ex}");
                return null;
            }
        }

        /// <summary>
        /// Loads an image texture by asset name and returns it wrapped as an opaque platform
        /// texture handle, reusing the same wrapper instance across repeated calls for the
        /// same asset until <see cref="Free"/> is called.
        /// </summary>
        /// <param name="imgName">The image asset name.</param>
        /// <returns>The texture handle, or <see langword="null"/> if loading fails.</returns>
        public static ITextureHandle GetHandle(string imgName)
        {
            Texture2D texture = Get(imgName);
            if (texture == null)
            {
                return null;
            }

            if (!_textureHandles.TryGetValue(imgName, out MonoGameTexture handle))
            {
                handle = new MonoGameTexture(texture);
                _textureHandles.Add(imgName, handle);
            }

            return handle;
        }

        /// <summary>
        /// Unloads the cached content manager for the specified image asset.
        /// </summary>
        /// <param name="imgName">The image asset name.</param>
        public static void Free(string imgName)
        {
            GetContentManager(imgName).Unload();
            _ = _textureHandles.Remove(imgName);
        }

        /// <summary>
        /// Stores per-image content managers so individual assets can be unloaded independently.
        /// </summary>
        private static readonly Dictionary<string, ContentManager> _contentManagers = [];

        /// <summary>
        /// Stores cached texture handle wrappers alongside each loaded image asset.
        /// </summary>
        private static readonly Dictionary<string, MonoGameTexture> _textureHandles = [];
    }
}
