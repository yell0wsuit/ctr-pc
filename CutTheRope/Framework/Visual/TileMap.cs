using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    internal sealed class TileMap : BaseElement
    {
        public override void Draw()
        {
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                imageMultiDrawer?.Draw();
            }
        }

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

        public void SetParallaxRatio(float r)
        {
            parallaxRatio = r;
        }

        public void SetRepeatHorizontally(Repeat r)
        {
            repeatedHorizontally = r;
            UpdateVars();
        }

        public void SetRepeatVertically(Repeat r)
        {
            repeatedVertically = r;
            UpdateVars();
        }

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

        public int[,] matrix;

        private int rows;

        private int columns;

        private List<ImageMultiDrawer> drawers;

        private Dictionary<int, TileEntry> tiles;

        private int cameraViewWidth;

        private int cameraViewHeight;

        private int tileMapWidth;

        private int tileMapHeight;

        private int maxRowsOnScreen;

        private int maxColsOnScreen;

        private int randomSeed;

        private Repeat repeatedVertically;

        private Repeat repeatedHorizontally;

        private float parallaxRatio;

        private int tileWidth;

        private int tileHeight;

        private bool horizontalRandom;

        private bool verticalRandom;

        public enum Repeat
        {
            NONE,
            ALL,
            EDGES
        }
    }
}
