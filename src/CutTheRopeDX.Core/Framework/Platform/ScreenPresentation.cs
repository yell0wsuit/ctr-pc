namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Logical-resolution presentation state: fixed game resolution, current surface size, scaled
    /// view rect, and coordinate transforms. Its only input is <see cref="SetSurfaceSize"/>; both
    /// the desktop host (via <c>ScreenSizeManager</c>) and the headless host feed it.
    /// </summary>
    /// <param name="gameWidth">Logical game width.</param>
    /// <param name="gameHeight">Logical game height.</param>
    internal sealed class ScreenPresentation(int gameWidth, int gameHeight)
    {
        /// <summary>
        /// Gets or sets the active presentation instance. Desktop and headless hosts both write
        /// through this single instance.
        /// </summary>
        public static ScreenPresentation Instance { get; set; } = new(2560, 1440);

        /// <summary>
        /// Gets the logical game width.
        /// </summary>
        public int GameWidth { get; } = gameWidth;

        /// <summary>
        /// Gets the logical game height.
        /// </summary>
        public int GameHeight { get; } = gameHeight;

        /// <summary>
        /// Gets the current drawable surface width, as last reported via <see cref="SetSurfaceSize"/>.
        /// </summary>
        public int SurfaceWidth { get; private set; }

        /// <summary>
        /// Gets the current drawable surface height, as last reported via <see cref="SetSurfaceSize"/>.
        /// </summary>
        public int SurfaceHeight { get; private set; }

        /// <summary>
        /// Whether fullscreen-style view scaling should crop width instead of fitting the full game
        /// width. Consulted the next time <see cref="SetSurfaceSize"/> recomputes the scaled view rect.
        /// </summary>
        public bool FullScreenCropWidth { get; set; } = true;

        /// <summary>
        /// Gets the X coordinate of the letterboxed or pillarboxed view rectangle used for rendering
        /// the game.
        /// </summary>
        public int ScaledViewX { get; private set; }

        /// <summary>
        /// Gets the Y coordinate of the letterboxed or pillarboxed view rectangle used for rendering
        /// the game.
        /// </summary>
        public int ScaledViewY { get; private set; }

        /// <summary>
        /// Gets the width of the letterboxed or pillarboxed view rectangle used for rendering the game.
        /// </summary>
        public int ScaledViewWidth { get; private set; }

        /// <summary>
        /// Gets the height of the letterboxed or pillarboxed view rectangle used for rendering the game.
        /// </summary>
        public int ScaledViewHeight { get; private set; }

        /// <summary>
        /// Gets the horizontal scale factor from logical game width to the current scaled view width.
        /// </summary>
        public double WidthAspectRatio => ScaledViewWidth / (double)GameWidth;

        /// <summary>
        /// Converts a window-space X coordinate into scaled-view space.
        /// </summary>
        /// <param name="x">Window-space X coordinate.</param>
        /// <returns>Scaled-view X coordinate.</returns>
        public int TransformWindowToViewX(int x)
        {
            return x - ScaledViewX;
        }

        /// <summary>
        /// Converts a window-space Y coordinate into scaled-view space.
        /// </summary>
        /// <param name="y">Window-space Y coordinate.</param>
        /// <returns>Scaled-view Y coordinate.</returns>
        public int TransformWindowToViewY(int y)
        {
            return y - ScaledViewY;
        }

        /// <summary>
        /// Converts a scaled-view X coordinate into logical game space.
        /// </summary>
        /// <param name="x">Scaled-view X coordinate.</param>
        /// <returns>Logical game-space X coordinate.</returns>
        public float TransformViewToGameX(float x)
        {
            return x * GameWidth / ScaledViewWidth;
        }

        /// <summary>
        /// Converts a scaled-view Y coordinate into logical game space.
        /// </summary>
        /// <param name="y">Scaled-view Y coordinate.</param>
        /// <returns>Logical game-space Y coordinate.</returns>
        public float TransformViewToGameY(float y)
        {
            return y * GameHeight / ScaledViewHeight;
        }

        /// <summary>
        /// Returns the logical game width that preserves aspect ratio for the supplied scaled height.
        /// </summary>
        /// <param name="scaledHeight">Scaled view height.</param>
        /// <returns>Aspect-ratio-correct game width.</returns>
        public int ScaledGameWidth(int scaledHeight)
        {
            return (int)((scaledHeight / _gameAspectRatio) + 0.5);
        }

        /// <summary>
        /// Returns the logical game height that preserves aspect ratio for the supplied scaled width.
        /// </summary>
        /// <param name="scaledWidth">Scaled view width.</param>
        /// <returns>Aspect-ratio-correct game height.</returns>
        public int ScaledGameHeight(int scaledWidth)
        {
            return (int)((scaledWidth * _gameAspectRatio) + 0.5);
        }

        /// <summary>The single input: the drawable surface is now <paramref name="w"/>×<paramref name="h"/>.</summary>
        /// <param name="w">Drawable surface width.</param>
        /// <param name="h">Drawable surface height.</param>
        public void SetSurfaceSize(int w, int h)
        {
            SurfaceWidth = w;
            SurfaceHeight = h;
            UpdateScaledView();
        }

        /// <summary>
        /// Recomputes the scaled render rectangle for the current surface size.
        /// </summary>
        private void UpdateScaledView()
        {
            // Always use fullscreen-style letterboxing/pillarboxing for both modes
            int sourceWidth = SurfaceWidth;
            int sourceHeight = SurfaceHeight;
            if (sourceWidth >= sourceHeight)
            {
                int scaledHeight = FullScreenCropWidth ? sourceHeight : ScaledGameHeight(sourceWidth);
                int scaledWidth = FullScreenCropWidth ? ScaledGameWidth(scaledHeight) : sourceWidth;
                ScaledViewX = (sourceWidth - scaledWidth) / 2;
                ScaledViewY = (sourceHeight - scaledHeight) / 2;
                ScaledViewWidth = scaledWidth;
                ScaledViewHeight = scaledHeight;
                return;
            }
            int portraitScaledHeight = FullScreenCropWidth ? (int)(sourceWidth / 5f * 4f) : ScaledGameHeight(sourceWidth);
            int portraitScaledWidth = FullScreenCropWidth ? ScaledGameWidth(portraitScaledHeight) : sourceWidth;
            ScaledViewX = (sourceWidth - portraitScaledWidth) / 2;
            ScaledViewY = (sourceHeight - portraitScaledHeight) / 2;
            ScaledViewWidth = portraitScaledWidth;
            ScaledViewHeight = portraitScaledHeight;
        }

        /// <summary>
        /// Cached logical game aspect ratio.
        /// </summary>
        private readonly double _gameAspectRatio = gameHeight / (double)gameWidth;
    }
}
