using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CutTheRope.windows
{
    internal class Images
    {
        private static ContentManager getContentManager(string imgName)
        {
            ContentManager value = null;
            Images._contentManagers.TryGetValue(imgName, out value);
            if (value == null)
            {
                value = new ContentManager(Global.XnaGame.Services, "content");
                Images._contentManagers.Add(imgName, value);
            }
            return value;
        }

        public static Texture2D get(string imgName)
        {
            ContentManager contentManager = Images.getContentManager(imgName);
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

        public static void free(string imgName)
        {
            Images.getContentManager(imgName).Unload();
        }

        private static Dictionary<string, ContentManager> _contentManagers = new();
    }
}
