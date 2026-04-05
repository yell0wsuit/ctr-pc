using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class Global
    {
        public static SpriteBatch SpriteBatch { get; set; }

        public static GraphicsDevice GraphicsDevice { get; set; }

        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        public static ScreenSizeManager ScreenSizeManager { get; set; } = new(2560, 1440);

        public static MouseCursor MouseCursor { get; } = new();

        public static Game1 XnaGame { get; set; }
    }
}
