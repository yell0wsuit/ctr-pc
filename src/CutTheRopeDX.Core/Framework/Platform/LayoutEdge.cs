namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Which point of a viewport an element is positioned against.
    /// </summary>
    internal enum LayoutEdge
    {
        /// <summary>Top-left corner.</summary>
        TopLeft,

        /// <summary>Center of the top edge.</summary>
        TopCenter,

        /// <summary>Top-right corner.</summary>
        TopRight,

        /// <summary>Center of the left edge.</summary>
        MiddleLeft,

        /// <summary>Center of the viewport.</summary>
        MiddleCenter,

        /// <summary>Center of the right edge.</summary>
        MiddleRight,

        /// <summary>Bottom-left corner.</summary>
        BottomLeft,

        /// <summary>Center of the bottom edge.</summary>
        BottomCenter,

        /// <summary>Bottom-right corner.</summary>
        BottomRight,
    }
}
