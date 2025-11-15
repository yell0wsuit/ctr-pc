using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class Images
    {
        private static ContentManager GetContentManager(string imgName)
        {
            _ = _contentManagers.TryGetValue(imgName, out ContentManager value);
            if (value == null)
            {
                value = new ContentManager(Global.XnaGame.Services, "content");
                _contentManagers.Add(imgName, value);
            }
            return value;
        }

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

        public static void Free(string imgName)
        {
            GetContentManager(imgName).Unload();
        }

        private static readonly Dictionary<string, ContentManager> _contentManagers = [];
    }
}
