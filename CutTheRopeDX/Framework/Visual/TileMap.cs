using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A grid-based tile map that renders tiles using <see cref="ImageMultiDrawer"/> instances, supporting parallax, repeating, and random tile selection.
    /// </summary>
    internal sealed class TileMap : BaseElement
    {
        /// <inheritdoc />
        public override void Draw()
        {
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                imageMultiDrawer?.Draw();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                matrix = null;
                if (drawers != null)
                {
                    foreach (ImageMultiDrawer drawer in drawers)
                    {
                        drawer?.Dispose();
                    }
                    drawers.Clear();
                    drawers = null;
                }
                tiles?.Clear();
                tiles = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes the tile map with the specified grid dimensions.
        /// </summary>
        /// <param name="r">Number of rows.</param>
        /// <param name="c">Number of columns.</param>
        /// <returns>The initialized tile map instance.</returns>
        public TileMap InitWithRowsColumns(int r, int c)
        {
            rows = r;
            columns = c;
            cameraViewWidth = (int)SCREEN_WIDTH;
            cameraViewHeight = (int)SCREEN_HEIGHT;
            parallaxRatio = 1f;
            drawers = [];
            tiles = [];
            matrix = new int[columns, rows];
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    matrix[i, j] = -1;
                }
            }
            repeatedVertically = Repeat.NONE;
            repeatedHorizontally = Repeat.NONE;
            horizontalRandom = false;
            verticalRandom = false;
            randomSeed = RND_RANGE(1000, 2000);
            return this;
        }

        /// <summary>
        /// Registers a tile from a texture quad and assigns it a tile ID.
        /// </summary>
        /// <param name="t">Texture containing the tile.</param>
        /// <param name="q">Quad index within the texture, or -1 for full image.</param>
        /// <param name="ti">Tile ID used in the matrix.</param>
        public void AddTileQuadwithID(CTRTexture2D t, int q, int ti)
        {
            // If texture has no quads (e.g., background images), use full image dimensions
            if (t.quadsCount == 0 || q == -1)
            {
                tileWidth = t._realWidth;
                tileHeight = t._realHeight;
            }
            else
            {
                tileWidth = (int)t.quadRects[q].w;
                tileHeight = (int)t.quadRects[q].h;
            }
            UpdateVars();
            int drawerIndex = -1;
            for (int i = 0; i < drawers.Count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                if (imageMultiDrawer.image.texture == t)
                {
                    drawerIndex = i;
                }
                if (imageMultiDrawer.image.texture._realWidth == tileWidth)
                {
                    _ = imageMultiDrawer.image.texture._realHeight;
                }
            }
            if (drawerIndex == -1)
            {
                Image image = Image.Image_create(t);
                ImageMultiDrawer item = new ImageMultiDrawer().InitWithImageandCapacity(image, maxRowsOnScreen * maxColsOnScreen);
                drawerIndex = drawers.Count;
                drawers.Add(item);
            }
            TileEntry tileEntry = new()
            {
                drawerIndex = drawerIndex,
                quad = q
            };
            tiles[ti] = tileEntry;
        }

        /// <summary>
        /// Fills a rectangular region of the matrix with the specified tile ID.
        /// </summary>
        /// <param name="r">Starting row.</param>
        /// <param name="c">Starting column.</param>
        /// <param name="rs">Number of rows to fill.</param>
        /// <param name="cs">Number of columns to fill.</param>
        /// <param name="ti">Tile ID to fill with.</param>
        public void FillStartAtRowColumnRowsColumnswithTile(int r, int c, int rs, int cs, int ti)
        {
            for (int i = c; i < c + cs; i++)
            {
                for (int j = r; j < r + rs; j++)
                {
                    matrix[i, j] = ti;
                }
            }
        }

        /// <summary>
        /// Sets the parallax scroll ratio (1 = no parallax).
        /// </summary>
        /// <param name="r">Parallax ratio.</param>
        public void SetParallaxRatio(float r)
        {
            parallaxRatio = r;
        }

        /// <summary>
        /// Sets the horizontal repeat mode.
        /// </summary>
        /// <param name="r">Repeat mode.</param>
        public void SetRepeatHorizontally(Repeat r)
        {
            repeatedHorizontally = r;
            UpdateVars();
        }

        /// <summary>
        /// Sets the vertical repeat mode.
        /// </summary>
        /// <param name="r">Repeat mode.</param>
        public void SetRepeatVertically(Repeat r)
        {
            repeatedVertically = r;
            UpdateVars();
        }

        /// <summary>
        /// Updates visible tiles and populates drawers based on the camera position.
        /// </summary>
        /// <param name="pos">Camera position in world coordinates.</param>
        public void UpdateWithCameraPos(Vector pos)
        {
            float cameraX = MathF.Round(pos.X / parallaxRatio);
            float cameraY = MathF.Round(pos.Y / parallaxRatio);
            float mapX = x;
            float mapY = y;
            if (repeatedVertically != Repeat.NONE)
            {
                float verticalDelta = mapY - cameraY;
                int verticalWrapOffset = (int)verticalDelta % tileMapHeight;
                mapY = verticalDelta >= 0f ? verticalWrapOffset - tileMapHeight + cameraY : verticalWrapOffset + cameraY;
            }
            if (repeatedHorizontally != Repeat.NONE)
            {
                float horizontalDelta = mapX - cameraX;
                int horizontalWrapOffset = (int)horizontalDelta % tileMapWidth;
                mapX = horizontalDelta >= 0f ? horizontalWrapOffset - tileMapWidth + cameraX : horizontalWrapOffset + cameraX;
            }
            if (!RectInRect(cameraX, cameraY, cameraX + cameraViewWidth, cameraY + cameraViewHeight, mapX, mapY, mapX + tileMapWidth, mapY + tileMapHeight))
            {
                return;
            }
            CTRRectangle rectangle = RectInRectIntersection(new CTRRectangle(mapX, mapY, tileMapWidth, tileMapHeight), new CTRRectangle(cameraX, cameraY, cameraViewWidth, cameraViewHeight));
            Vector vector = Vect(MathF.Max(0f, rectangle.x), MathF.Max(0f, rectangle.y));
            Vector vector2 = Vect((int)vector.X / tileWidth, (int)vector.Y / tileHeight);
            float rowStartY = mapY + (vector2.Y * tileHeight);
            Vector vector3 = Vect(mapX + (vector2.X * tileWidth), rowStartY);
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                _ = (imageMultiDrawer?.numberOfQuadsToDraw = 0);
            }
            int maxVisibleColumn = (int)(vector2.X + maxColsOnScreen - 1f);
            int maxVisibleRow = (int)(vector2.Y + maxRowsOnScreen - 1f);
            if (repeatedVertically == Repeat.NONE)
            {
                maxVisibleRow = Math.Min(rows - 1, maxVisibleRow);
            }
            if (repeatedHorizontally == Repeat.NONE)
            {
                maxVisibleColumn = Math.Min(columns - 1, maxVisibleColumn);
            }
            for (int j = (int)vector2.X; j <= maxVisibleColumn; j++)
            {
                vector3.Y = rowStartY;
                int k = (int)vector2.Y;
                while (k <= maxVisibleRow && vector3.Y < cameraY + cameraViewHeight)
                {
                    CTRRectangle rectangle2 = RectInRectIntersection(new CTRRectangle(cameraX, cameraY, cameraViewWidth, cameraViewHeight), new CTRRectangle(vector3.X, vector3.Y, tileWidth, tileHeight));
                    CTRRectangle r = new(cameraX - vector3.X + rectangle2.x, cameraY - vector3.Y + rectangle2.y, rectangle2.w, rectangle2.h);
                    int tileColumn = j;
                    int tileRow = k;
                    if (repeatedVertically == Repeat.EDGES)
                    {
                        if (vector3.Y < y)
                        {
                            tileRow = 0;
                        }
                        else if (vector3.Y >= y + tileMapHeight)
                        {
                            tileRow = rows - 1;
                        }
                    }
                    if (repeatedHorizontally == Repeat.EDGES)
                    {
                        if (vector3.X < x)
                        {
                            tileColumn = 0;
                        }
                        else if (vector3.X >= x + tileMapWidth)
                        {
                            tileColumn = columns - 1;
                        }
                    }
                    if (horizontalRandom)
                    {
                        tileColumn = Math.Abs((int)(FmSin(vector3.X) * randomSeed) % columns);
                    }
                    if (verticalRandom)
                    {
                        tileRow = Math.Abs((int)(FmSin(vector3.Y) * randomSeed) % rows);
                    }
                    if (tileColumn >= columns)
                    {
                        tileColumn %= columns;
                    }
                    if (tileRow >= rows)
                    {
                        tileRow %= rows;
                    }
                    int tileIndex = matrix[tileColumn, tileRow];
                    if (tileIndex >= 0)
                    {
                        TileEntry tileEntry = tiles[tileIndex];
                        ImageMultiDrawer imageMultiDrawer2 = drawers[tileEntry.drawerIndex];
                        CTRTexture2D texture = imageMultiDrawer2.image.texture;
                        if (tileEntry.quad != -1 && texture.quadRects != null)
                        {
                            r.x += texture.quadRects[tileEntry.quad].x;
                            r.y += texture.quadRects[tileEntry.quad].y;
                        }
                        Quad2D textureCoordinates = DrawHelper.GetTextureCoordinates(imageMultiDrawer2.image.texture, r);
                        Quad3D qv = Quad3D.MakeQuad3D(pos.X + rectangle2.x, pos.Y + rectangle2.y, 0f, rectangle2.w, rectangle2.h);
                        ImageMultiDrawer imageMultiDrawer3 = imageMultiDrawer2;
                        Quad2D quad2D = textureCoordinates;
                        Quad3D quad3D = qv;
                        ImageMultiDrawer imageMultiDrawer4 = imageMultiDrawer2;
                        int numberOfQuadsToDraw = imageMultiDrawer4.numberOfQuadsToDraw;
                        imageMultiDrawer4.numberOfQuadsToDraw = numberOfQuadsToDraw + 1;
                        imageMultiDrawer3.SetTextureQuadatVertexQuadatIndex(quad2D, quad3D, numberOfQuadsToDraw);
                    }
                    vector3.Y += tileHeight;
                    k++;
                }
                vector3.X += tileWidth;
                if (vector3.X >= cameraX + cameraViewWidth)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Recalculates visible tile counts and total map dimensions.
        /// </summary>
        public void UpdateVars()
        {
            maxColsOnScreen = 2 + (int)MathF.Floor(cameraViewWidth / (tileWidth + 1));
            maxRowsOnScreen = 2 + (int)MathF.Floor(cameraViewHeight / (tileHeight + 1));
            if (repeatedVertically == Repeat.NONE)
            {
                maxRowsOnScreen = Math.Min(maxRowsOnScreen, rows);
            }
            if (repeatedHorizontally == Repeat.NONE)
            {
                maxColsOnScreen = Math.Min(maxColsOnScreen, columns);
            }
            width = tileMapWidth = columns * tileWidth;
            height = tileMapHeight = rows * tileHeight;
        }

        /// <summary>
        /// 2D grid mapping (column, row) to tile IDs.
        /// </summary>
        public int[,] matrix;

        /// <summary>
        /// Number of rows in the grid.
        /// </summary>
        private int rows;

        /// <summary>
        /// Number of columns in the grid.
        /// </summary>
        private int columns;

        /// <summary>
        /// List of drawers, one per unique texture.
        /// </summary>
        private List<ImageMultiDrawer> drawers;

        /// <summary>
        /// Tile definitions keyed by tile ID.
        /// </summary>
        private Dictionary<int, TileEntry> tiles;

        /// <summary>
        /// Camera viewport width in pixels.
        /// </summary>
        private int cameraViewWidth;

        /// <summary>
        /// Camera viewport height in pixels.
        /// </summary>
        private int cameraViewHeight;

        /// <summary>
        /// Total tile map width in pixels.
        /// </summary>
        private int tileMapWidth;

        /// <summary>
        /// Total tile map height in pixels.
        /// </summary>
        private int tileMapHeight;

        /// <summary>
        /// Maximum number of tile rows visible on screen.
        /// </summary>
        private int maxRowsOnScreen;

        /// <summary>
        /// Maximum number of tile columns visible on screen.
        /// </summary>
        private int maxColsOnScreen;

        /// <summary>
        /// Seed for random tile selection.
        /// </summary>
        private int randomSeed;

        /// <summary>
        /// Vertical repeat mode.
        /// </summary>
        private Repeat repeatedVertically;

        /// <summary>
        /// Horizontal repeat mode.
        /// </summary>
        private Repeat repeatedHorizontally;

        /// <summary>
        /// Parallax scroll ratio (1 = no parallax).
        /// </summary>
        private float parallaxRatio;

        /// <summary>
        /// Width of a single tile in pixels.
        /// </summary>
        private int tileWidth;

        /// <summary>
        /// Height of a single tile in pixels.
        /// </summary>
        private int tileHeight;

        /// <summary>
        /// Whether columns are selected randomly instead of sequentially.
        /// </summary>
        private bool horizontalRandom;

        /// <summary>
        /// Whether rows are selected randomly instead of sequentially.
        /// </summary>
        private bool verticalRandom;

        /// <summary>
        /// Tile repeat modes for map edges.
        /// </summary>
        public enum Repeat
        {
            /// <summary>
            /// No repeating.
            /// </summary>
            NONE,

            /// <summary>
            /// Repeat all tiles seamlessly.
            /// </summary>
            ALL,

            /// <summary>
            /// Repeat only edge tiles.
            /// </summary>
            EDGES
        }
    }
}
