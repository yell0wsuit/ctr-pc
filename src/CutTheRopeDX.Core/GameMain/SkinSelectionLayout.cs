using System;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How the skin selection screen divides a viewport: the size one slot is drawn at, how many
    /// fit across, where the tabs sit above them and what is left over for the grid to scroll in.
    /// </summary>
    /// <remarks>
    /// Pure, and derived from the viewport alone, so the screen can be laid out again at any shape
    /// without rebuilding the slots - which is what the selection screen needs, because a slot owns
    /// an animated preview that is expensive to make and cheap to move.
    /// </remarks>
    /// <param name="Scale">Uniform scale everything on the screen is drawn at.</param>
    /// <param name="TabStride">Horizontal distance between adjacent tabs.</param>
    /// <param name="TabsPerRow">How many tabs fit across.</param>
    /// <param name="TabRows">How many rows the tabs take.</param>
    /// <param name="TabHeight">Height one tab is drawn at.</param>
    /// <param name="Columns">How many slots fit across.</param>
    /// <param name="CellWidth">Width one slot is drawn at.</param>
    /// <param name="CellHeight">Height one slot is drawn at.</param>
    /// <param name="ColumnSpacing">Gap between two slots in a row.</param>
    /// <param name="RowHeight">Height of a row of slots, which is taller than a slot.</param>
    /// <param name="RowSpacing">Gap between two rows.</param>
    /// <param name="GridWidth">Width of the grid, and of the window it scrolls inside.</param>
    /// <param name="WindowTop">Top of that window, below the tabs.</param>
    /// <param name="WindowHeight">Height of that window.</param>
    internal readonly record struct SkinSelectionLayout(
        float Scale,
        float TabStride,
        int TabsPerRow,
        int TabRows,
        float TabHeight,
        int Columns,
        float CellWidth,
        float CellHeight,
        float ColumnSpacing,
        float RowHeight,
        float RowSpacing,
        float GridWidth,
        float WindowTop,
        float WindowHeight)
    {
        /// <summary>
        /// Divides a viewport between the tabs and the grid.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="scale">Uniform scale to draw the screen at.</param>
        /// <param name="tabWidth">Authored width of the widest tab.</param>
        /// <param name="tabHeight">Authored height of a tab.</param>
        /// <param name="tabCount">How many tabs the screen has.</param>
        /// <param name="chromeWidth">Room the chrome in the bottom corner takes across.</param>
        /// <param name="chromeHeight">Room the chrome in the bottom corner takes up the screen.</param>
        /// <returns>The layout for that viewport.</returns>
        public static SkinSelectionLayout For(
            CTRRectangle visible,
            float scale,
            float tabWidth,
            float tabHeight,
            int tabCount,
            float chromeWidth = 0f,
            float chromeHeight = 0f)
        {
            float room = visible.w - (EdgeMargin * 2f);
            float tabStride = (tabWidth + TabGap) * scale;

            // How many rows the tabs need, then an even share of them across those rows: three
            // tabs with a lone fourth underneath fit exactly as well as two and two, and read far
            // worse.
            int tabsThatFit = Fit(room, tabStride, tabCount);
            int tabRows = (tabCount + tabsThatFit - 1) / tabsThatFit;
            int tabsPerRow = (tabCount + tabRows - 1) / tabRows;

            float scaledTabHeight = tabHeight * scale;
            float tabsBottom = (TabTop * scale)
                + (tabRows * scaledTabHeight)
                + ((tabRows - 1) * TabRowGap * scale);

            float cellWidth = SlotWidth * scale;
            float columnSpacing = SlotSpacing * scale;
            int columns = Fit(room, cellWidth + columnSpacing, MaxColumns);
            float gridWidth = (columns * cellWidth) + ((columns - 1) * columnSpacing);
            float windowTop = tabsBottom + (GridTopGap * scale);

            // The button in the bottom corner is drawn over the window, so where the grid is wide
            // enough to reach that corner the window stops above it instead. On a viewport where
            // the grid does not come near it - a wide one, where the grid is a column in the
            // middle - the authored margin stands and no height is given up for nothing.
            bool gridReachesTheCorner = (visible.w - gridWidth) / 2f < chromeWidth;
            float bottomMargin = MathF.Max(
                GridBottomMargin * scale,
                gridReachesTheCorner ? chromeHeight : 0f);

            return new SkinSelectionLayout(
                scale,
                tabStride,
                tabsPerRow,
                tabRows,
                scaledTabHeight,
                columns,
                cellWidth,
                SlotHeight * scale,
                columnSpacing,
                SlotHeight * RowHeightFactor * scale,
                AuthoredRowSpacing * scale,
                gridWidth,
                windowTop,
                MathF.Max(SlotHeight * scale, visible.h - windowTop - bottomMargin));
        }

        /// <summary>
        /// Gets where a tab sits relative to the middle of the screen.
        /// </summary>
        /// <param name="tab">Zero-based tab index.</param>
        /// <param name="tabCount">How many tabs the screen has.</param>
        /// <returns>The tab's horizontal offset from the middle.</returns>
        public float TabX(int tab, int tabCount)
        {
            int row = tab / TabsPerRow;
            int inRow = Math.Min(TabsPerRow, tabCount - (row * TabsPerRow));
            return SkinSelectionTabLayout.GetCenteredX(tab - (row * TabsPerRow), inRow, TabStride);
        }

        /// <summary>
        /// Gets where the top of a tab belongs, in logical space.
        /// </summary>
        /// <param name="tab">Zero-based tab index.</param>
        /// <returns>The top of the row the tab is in.</returns>
        public float TabTopFor(int tab)
        {
            int row = tab / TabsPerRow;
            return (TabTop * Scale) + (row * (TabHeight + (TabRowGap * Scale)));
        }

        /// <summary>
        /// How many items of a given pitch fit in a width, at least one and never more than
        /// <paramref name="most"/>.
        /// </summary>
        /// <param name="room">Width available.</param>
        /// <param name="pitch">Distance from one item to the next.</param>
        /// <param name="most">Largest number to return.</param>
        /// <returns>The number that fits.</returns>
        private static int Fit(float room, float pitch, int most)
        {
            return pitch > 0f ? Math.Clamp((int)(room / pitch), 1, most) : most;
        }

        /// <summary>
        /// Most slots the grid puts in one row. The slots are a menu of skins rather than a sheet
        /// of thumbnails, so a wide screen spends its room on margins past this point.
        /// </summary>
        public const int MaxColumns = 4;

        /// <summary>Authored width of one slot.</summary>
        private const float SlotWidth = 271f;

        /// <summary>Authored height of one slot.</summary>
        private const float SlotHeight = 336f;

        /// <summary>Authored gap between two slots in a row.</summary>
        private const float SlotSpacing = 20f;

        /// <summary>Authored gap between two rows of slots.</summary>
        private const float AuthoredRowSpacing = 10f;

        /// <summary>How much taller than a slot the row holding it is.</summary>
        private const float RowHeightFactor = 1.2f;

        /// <summary>Authored gap between two tabs in a row.</summary>
        private const float TabGap = 24f;

        /// <summary>Authored gap between two rows of tabs.</summary>
        private const float TabRowGap = 10f;

        /// <summary>Authored distance from the top of the screen to the first row of tabs.</summary>
        private const float TabTop = 50f;

        /// <summary>Authored gap between the tabs and the grid below them.</summary>
        private const float GridTopGap = 30f;

        /// <summary>Authored distance the grid keeps from the bottom of the screen.</summary>
        private const float GridBottomMargin = 120f;


        /// <summary>Authored distance the tabs and the grid keep from the sides of the screen.</summary>
        private const float EdgeMargin = 10f;
    }
}
