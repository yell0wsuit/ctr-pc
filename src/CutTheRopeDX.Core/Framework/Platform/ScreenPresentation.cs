namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Logical-resolution presentation state: fixed game resolution, current surface size, and
    /// coordinate transforms. <c>CtrRenderer</c> publishes its snapshot for every host through
    /// the engine's single surface-change transition.
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
            ViewportLayout.Compute(gameWidth, gameHeight);

        /// <summary>
        /// Gets the current drawable surface width.
        /// </summary>
        public int SurfaceWidth => Snapshot.SurfaceWidth;

        /// <summary>
        /// Gets the current drawable surface height.
        /// </summary>
        public int SurfaceHeight => Snapshot.SurfaceHeight;

        /// <summary>
        /// Converts a window-space X coordinate into the drawn region's space.
        /// </summary>
        /// <param name="x">Window-space X coordinate.</param>
        /// <returns>X coordinate relative to the drawn region.</returns>
        public int TransformWindowToViewX(int x)
        {
            return x - (int)Snapshot.RenderViewport.x;
        }

        /// <summary>
        /// Converts a window-space Y coordinate into the drawn region's space.
        /// </summary>
        /// <param name="y">Window-space Y coordinate.</param>
        /// <returns>Y coordinate relative to the drawn region.</returns>
        public int TransformWindowToViewY(int y)
        {
            return y - (int)Snapshot.RenderViewport.y;
        }

        /// <summary>
        /// Converts a coordinate in the drawn region into logical space.
        /// </summary>
        /// <param name="x">X coordinate relative to the drawn region.</param>
        /// <returns>Logical X coordinate.</returns>
        public float TransformViewToGameX(float x)
        {
            return x / Snapshot.Scale;
        }

        /// <summary>
        /// Converts a coordinate in the drawn region into logical space.
        /// </summary>
        /// <param name="y">Y coordinate relative to the drawn region.</param>
        /// <returns>Logical Y coordinate.</returns>
        public float TransformViewToGameY(float y)
        {
            return y / Snapshot.Scale;
        }

        /// <summary>
        /// Publishes the viewport state for a surface of the given size. Every input arrives in
        /// this one call, so a caller cannot set part of the state and publish the rest later.
        /// </summary>
        /// <param name="w">Drawable surface width.</param>
        /// <param name="h">Drawable surface height.</param>
        /// <param name="devicePixelRatio">Physical pixels per logical pixel on the host surface.</param>
        /// <returns>
        /// <see langword="true"/> when the published snapshot differs from the previous one.
        /// Callers react to it immediately; storing it would recreate the shadow state this
        /// design exists to remove.
        /// </returns>
        public bool SetSurfaceSize(int w, int h, float devicePixelRatio = 1f)
        {
            ViewportLayoutSnapshot next = ViewportLayout.Compute(w, h, devicePixelRatio);
            if (next == Snapshot)
            {
                return false;
            }
            Snapshot = next;
            return true;
        }
    }
}
