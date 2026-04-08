namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Maps a tile ID to its drawer and quad index within the <see cref="TileMap"/>.
    /// </summary>
    internal sealed class TileEntry : FrameworkTypes
    {
        /// <summary>
        /// Index into the <see cref="TileMap"/>'s drawer list.
        /// </summary>
        public int drawerIndex;

        /// <summary>
        /// Quad index within the drawer's texture, or -1 for full image.
        /// </summary>
        public int quad;
    }
}
