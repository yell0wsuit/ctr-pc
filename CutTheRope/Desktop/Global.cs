using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    /// <summary>
    /// Holds shared desktop runtime services used across the MonoGame host layer.
    /// </summary>
    internal sealed class Global
    {
        /// <summary>
        /// Gets or sets the shared sprite batch used for 2D rendering.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        /// Gets or sets the active graphics device.
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Gets or sets the graphics device manager that owns the graphics device.
        /// </summary>
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        /// <summary>
        /// Gets or sets the screen size manager responsible for logical-to-window transforms.
        /// </summary>
        public static ScreenSizeManager ScreenSizeManager { get; set; } = new(2560, 1440);

        /// <summary>
        /// Gets the shared desktop mouse cursor helper.
        /// </summary>
        public static MouseCursor MouseCursor { get; } = new();

        /// <summary>
        /// Gets or sets the active MonoGame host instance.
        /// </summary>
        public static Game1 XnaGame { get; set; }
    }
}
