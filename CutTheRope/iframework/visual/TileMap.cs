using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal class TileMap : BaseElement
    {
        public override void draw()
        {
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                imageMultiDrawer?.draw();
            }
        }

        public override void dealloc()
        {
            matrix = null;
            drawers.Clear();
            drawers = null;
            tiles.Clear();
            tiles = null;
            base.dealloc();
        }

        public virtual TileMap initWithRowsColumns(int r, int c)
        {
            if (init() != null)
            {
                rows = r;
                columns = c;
                cameraViewWidth = (int)SCREEN_WIDTH;
                cameraViewHeight = (int)SCREEN_HEIGHT;
                parallaxRatio = 1f;
                drawers = new List<ImageMultiDrawer>();
                tiles = new Dictionary<int, TileEntry>();
                matrix = new int[columns, rows];
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        matrix[i, j] = -1;
                    }
                }
                repeatedVertically = Repeat.REPEAT_NONE;
                repeatedHorizontally = Repeat.REPEAT_NONE;
                horizontalRandom = false;
                verticalRandom = false;
                restoreTileTransparency = true;
                randomSeed = RND_RANGE(1000, 2000);
            }
            return this;
        }

        public virtual void addTileQuadwithID(CTRTexture2D t, int q, int ti)
        {
            if (q == -1)
            {
                tileWidth = t._realWidth;
                tileHeight = t._realHeight;
            }
            else
            {
                tileWidth = (int)t.quadRects[q].w;
                tileHeight = (int)t.quadRects[q].h;
            }
            updateVars();
            int num = -1;
            for (int i = 0; i < drawers.Count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                if (imageMultiDrawer.image.texture == t)
                {
                    num = i;
                }
                if (imageMultiDrawer.image.texture._realWidth == tileWidth)
                {
                    int realHeight = imageMultiDrawer.image.texture._realHeight;
                    int num2 = tileHeight;
                }
            }
            if (num == -1)
            {
                Image image = Image.Image_create(t);
                if (restoreTileTransparency)
                {
                    image.doRestoreCutTransparency();
                }
                ImageMultiDrawer item = new ImageMultiDrawer().initWithImageandCapacity(image, maxRowsOnScreen * maxColsOnScreen);
                num = drawers.Count;
                drawers.Add(item);
            }
            TileEntry tileEntry = new();
            tileEntry.drawerIndex = num;
            tileEntry.quad = q;
            tiles[ti] = tileEntry;
        }

        public virtual void fillStartAtRowColumnRowsColumnswithTile(int r, int c, int rs, int cs, int ti)
        {
            for (int i = c; i < c + cs; i++)
            {
                for (int j = r; j < r + rs; j++)
                {
                    matrix[i, j] = ti;
                }
            }
        }

        public virtual void setParallaxRatio(float r)
        {
            parallaxRatio = r;
        }

        public virtual void setRepeatHorizontally(Repeat r)
        {
            repeatedHorizontally = r;
            updateVars();
        }

        public virtual void setRepeatVertically(Repeat r)
        {
            repeatedVertically = r;
            updateVars();
        }

        public virtual void updateWithCameraPos(Vector pos)
        {
            float num = (float)Math.Round((double)(pos.x / parallaxRatio));
            float num2 = (float)Math.Round((double)(pos.y / parallaxRatio));
            float num3 = x;
            float num4 = y;
            if (repeatedVertically != Repeat.REPEAT_NONE)
            {
                float num13 = num4 - num2;
                int num5 = (int)num13 % tileMapHeight;
                num4 = (num13 >= 0f) ? ((float)(num5 - tileMapHeight) + num2) : ((float)num5 + num2);
            }
            if (repeatedHorizontally != Repeat.REPEAT_NONE)
            {
                float num14 = num3 - num;
                int num6 = (int)num14 % tileMapWidth;
                num3 = (num14 >= 0f) ? ((float)(num6 - tileMapWidth) + num) : ((float)num6 + num);
            }
            if (!rectInRect(num, num2, num + (float)cameraViewWidth, num2 + (float)cameraViewHeight, num3, num4, num3 + (float)tileMapWidth, num4 + (float)tileMapHeight))
            {
                return;
            }
            CTRRectangle rectangle = rectInRectIntersection(new CTRRectangle(num3, num4, (float)tileMapWidth, (float)tileMapHeight), new CTRRectangle(num, num2, (float)cameraViewWidth, (float)cameraViewHeight));
            Vector vector = vect(Math.Max(0f, rectangle.x), Math.Max(0f, rectangle.y));
            Vector vector2 = vect((float)((int)vector.x / tileWidth), (float)((int)vector.y / tileHeight));
            float num7 = num4 + vector2.y * (float)tileHeight;
            Vector vector3 = vect(num3 + vector2.x * (float)tileWidth, num7);
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
            {
                ImageMultiDrawer imageMultiDrawer = drawers[i];
                if (imageMultiDrawer != null)
                {
                    imageMultiDrawer.numberOfQuadsToDraw = 0;
                }
            }
            int num8 = (int)(vector2.x + (float)maxColsOnScreen - 1f);
            int num9 = (int)(vector2.y + (float)maxRowsOnScreen - 1f);
            if (repeatedVertically == Repeat.REPEAT_NONE)
            {
                num9 = Math.Min(rows - 1, num9);
            }
            if (repeatedHorizontally == Repeat.REPEAT_NONE)
            {
                num8 = Math.Min(columns - 1, num8);
            }
            for (int j = (int)vector2.x; j <= num8; j++)
            {
                vector3.y = num7;
                int k = (int)vector2.y;
                while (k <= num9 && vector3.y < num2 + (float)cameraViewHeight)
                {
                    CTRRectangle rectangle2 = rectInRectIntersection(new CTRRectangle(num, num2, (float)cameraViewWidth, (float)cameraViewHeight), new CTRRectangle(vector3.x, vector3.y, (float)tileWidth, (float)tileHeight));
                    CTRRectangle r = new(num - vector3.x + rectangle2.x, num2 - vector3.y + rectangle2.y, rectangle2.w, rectangle2.h);
                    int num10 = j;
                    int num11 = k;
                    if (repeatedVertically == Repeat.REPEAT_EDGES)
                    {
                        if (vector3.y < y)
                        {
                            num11 = 0;
                        }
                        else if (vector3.y >= y + (float)tileMapHeight)
                        {
                            num11 = rows - 1;
                        }
                    }
                    if (repeatedHorizontally == Repeat.REPEAT_EDGES)
                    {
                        if (vector3.x < x)
                        {
                            num10 = 0;
                        }
                        else if (vector3.x >= x + (float)tileMapWidth)
                        {
                            num10 = columns - 1;
                        }
                    }
                    if (horizontalRandom)
                    {
                        num10 = Math.Abs((int)(fmSin(vector3.x) * (float)randomSeed) % columns);
                    }
                    if (verticalRandom)
                    {
                        num11 = Math.Abs((int)(fmSin(vector3.y) * (float)randomSeed) % rows);
                    }
                    if (num10 >= columns)
                    {
                        num10 %= columns;
                    }
                    if (num11 >= rows)
                    {
                        num11 %= rows;
                    }
                    int num12 = matrix[num10, num11];
                    if (num12 >= 0)
                    {
                        TileEntry tileEntry = tiles[num12];
                        ImageMultiDrawer imageMultiDrawer2 = drawers[tileEntry.drawerIndex];
                        CTRTexture2D texture = imageMultiDrawer2.image.texture;
                        if (tileEntry.quad != -1)
                        {
                            r.x += texture.quadRects[tileEntry.quad].x;
                            r.y += texture.quadRects[tileEntry.quad].y;
                        }
                        Quad2D textureCoordinates = GLDrawer.getTextureCoordinates(imageMultiDrawer2.image.texture, r);
                        Quad3D qv = Quad3D.MakeQuad3D((double)(pos.x + rectangle2.x), (double)(pos.y + rectangle2.y), 0.0, (double)rectangle2.w, (double)rectangle2.h);
                        ImageMultiDrawer imageMultiDrawer3 = imageMultiDrawer2;
                        Quad2D quad2D = textureCoordinates;
                        Quad3D quad3D = qv;
                        ImageMultiDrawer imageMultiDrawer4 = imageMultiDrawer2;
                        int numberOfQuadsToDraw = imageMultiDrawer4.numberOfQuadsToDraw;
                        imageMultiDrawer4.numberOfQuadsToDraw = numberOfQuadsToDraw + 1;
                        imageMultiDrawer3.setTextureQuadatVertexQuadatIndex(quad2D, quad3D, numberOfQuadsToDraw);
                    }
                    vector3.y += (float)tileHeight;
                    k++;
                }
                vector3.x += (float)tileWidth;
                if (vector3.x >= num + (float)cameraViewWidth)
                {
                    break;
                }
            }
        }

        public virtual void updateVars()
        {
            maxColsOnScreen = 2 + (int)Math.Floor((double)(cameraViewWidth / (tileWidth + 1)));
            maxRowsOnScreen = 2 + (int)Math.Floor((double)(cameraViewHeight / (tileHeight + 1)));
            if (repeatedVertically == Repeat.REPEAT_NONE)
            {
                maxRowsOnScreen = Math.Min(maxRowsOnScreen, rows);
            }
            if (repeatedHorizontally == Repeat.REPEAT_NONE)
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

        private bool restoreTileTransparency;

        public enum Repeat
        {
            REPEAT_NONE,
            REPEAT_ALL,
            REPEAT_EDGES
        }
    }
}
