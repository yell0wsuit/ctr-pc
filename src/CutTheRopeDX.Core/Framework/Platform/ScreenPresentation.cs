namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Logical-resolution presentation state: fixed game resolution, current surface size, scaled
    /// view rect, and coordinate transforms. <c>CtrRenderer</c> publishes its snapshot for every
    /// host through the engine's single surface-change transition.
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
        /// The published viewport state. Every other member of this type derives from it, and
        /// nothing else in the engine holds a copy, so there is no second value to keep in step.
        /// </summary>
        public ViewportLayoutSnapshot Snapshot { get; private set; } =
            ViewportLayout.Compute(gameWidth, gameHeight, true);

        /// <summary>
        /// Gets the current drawable surface width.
        /// </summary>
        public int SurfaceWidth => Snapshot.SurfaceWidth;

        /// <summary>
        /// Gets the current drawable surface height.
        /// </summary>
        public int SurfaceHeight => Snapshot.SurfaceHeight;

        /// <summary>
        /// Gets the X coordinate of the rectangle fixed-layout content renders into.
        /// </summary>
        public int ScaledViewX => (int)Snapshot.LegacyContentBounds.x;

        /// <summary>
        /// Gets the Y coordinate of the rectangle fixed-layout content renders into.
        /// </summary>
        public int ScaledViewY => (int)Snapshot.LegacyContentBounds.y;

        /// <summary>
        /// Gets the width of the rectangle fixed-layout content renders into.
        /// </summary>
        public int ScaledViewWidth => (int)Snapshot.LegacyContentBounds.w;

        /// <summary>
        /// Gets the height of the rectangle fixed-layout content renders into.
        /// </summary>
        public int ScaledViewHeight => (int)Snapshot.LegacyContentBounds.h;

        /// <summary>
        /// Gets the horizontal scale factor from logical game width to the current scaled view width.
        /// </summary>
        public double WidthAspectRatio => Snapshot.LegacyScale;

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

        /// <summary>
        /// Publishes the viewport state for a surface of the given size. Every input arrives in
        /// this one call, so a caller cannot set part of the state and publish the rest later.
        /// </summary>
        /// <param name="w">Drawable surface width.</param>
        /// <param name="h">Drawable surface height.</param>
        /// <param name="cropWidth">
        /// Whether portrait surfaces crop width to a 5:4 band instead of fitting the design
        /// aspect. An input to this transition rather than stored state.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the published snapshot differs from the previous one.
        /// Callers react to it immediately; storing it would recreate the shadow state this
        /// design exists to remove.
        /// </returns>
        public bool SetSurfaceSize(int w, int h, bool cropWidth)
        {
            ViewportLayoutSnapshot next = ViewportLayout.Compute(w, h, cropWidth);
            if (next == Snapshot)
            {
                return false;
            }
            Snapshot = next;
            return true;
        }

        /// <summary>
        /// Cached logical game aspect ratio.
        /// </summary>
        private readonly double _gameAspectRatio = gameHeight / (double)gameWidth;
    }
}
