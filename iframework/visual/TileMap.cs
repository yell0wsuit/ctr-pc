﻿using System;
using System.Collections.Generic;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200004D RID: 77
	internal class TileMap : BaseElement
	{
		// Token: 0x06000295 RID: 661 RVA: 0x0000E728 File Offset: 0x0000C928
		public override void draw()
		{
			int count = this.drawers.Count;
			for (int i = 0; i < count; i++)
			{
				ImageMultiDrawer imageMultiDrawer = this.drawers[i];
				if (imageMultiDrawer != null)
				{
					imageMultiDrawer.draw();
				}
			}
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000E763 File Offset: 0x0000C963
		public override void dealloc()
		{
			this.matrix = null;
			this.drawers.Clear();
			this.drawers = null;
			this.tiles.Clear();
			this.tiles = null;
			base.dealloc();
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000E798 File Offset: 0x0000C998
		public virtual TileMap initWithRowsColumns(int r, int c)
		{
			if (this.init() != null)
			{
				this.rows = r;
				this.columns = c;
				this.cameraViewWidth = (int)FrameworkTypes.SCREEN_WIDTH;
				this.cameraViewHeight = (int)FrameworkTypes.SCREEN_HEIGHT;
				this.parallaxRatio = 1f;
				this.drawers = new List<ImageMultiDrawer>();
				this.tiles = new Dictionary<int, TileEntry>();
				this.matrix = new int[this.columns, this.rows];
				for (int i = 0; i < this.columns; i++)
				{
					for (int j = 0; j < this.rows; j++)
					{
						this.matrix[i, j] = -1;
					}
				}
				this.repeatedVertically = TileMap.Repeat.REPEAT_NONE;
				this.repeatedHorizontally = TileMap.Repeat.REPEAT_NONE;
				this.horizontalRandom = false;
				this.verticalRandom = false;
				this.restoreTileTransparency = true;
				this.randomSeed = MathHelper.RND_RANGE(1000, 2000);
			}
			return this;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000E878 File Offset: 0x0000CA78
		public virtual void addTileQuadwithID(Texture2D t, int q, int ti)
		{
			if (q == -1)
			{
				this.tileWidth = t._realWidth;
				this.tileHeight = t._realHeight;
			}
			else
			{
				this.tileWidth = (int)t.quadRects[q].w;
				this.tileHeight = (int)t.quadRects[q].h;
			}
			this.updateVars();
			int num = -1;
			for (int i = 0; i < this.drawers.Count; i++)
			{
				ImageMultiDrawer imageMultiDrawer = this.drawers[i];
				if (imageMultiDrawer.image.texture == t)
				{
					num = i;
				}
				if (imageMultiDrawer.image.texture._realWidth == this.tileWidth)
				{
					int realHeight = imageMultiDrawer.image.texture._realHeight;
					int num2 = this.tileHeight;
				}
			}
			if (num == -1)
			{
				Image image = Image.Image_create(t);
				if (this.restoreTileTransparency)
				{
					image.doRestoreCutTransparency();
				}
				ImageMultiDrawer item = new ImageMultiDrawer().initWithImageandCapacity(image, this.maxRowsOnScreen * this.maxColsOnScreen);
				num = this.drawers.Count;
				this.drawers.Add(item);
			}
			TileEntry tileEntry = new TileEntry();
			tileEntry.drawerIndex = num;
			tileEntry.quad = q;
			this.tiles[ti] = tileEntry;
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000E9B0 File Offset: 0x0000CBB0
		public virtual void fillStartAtRowColumnRowsColumnswithTile(int r, int c, int rs, int cs, int ti)
		{
			for (int i = c; i < c + cs; i++)
			{
				for (int j = r; j < r + rs; j++)
				{
					this.matrix[i, j] = ti;
				}
			}
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0000E9E9 File Offset: 0x0000CBE9
		public virtual void setParallaxRatio(float r)
		{
			this.parallaxRatio = r;
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0000E9F2 File Offset: 0x0000CBF2
		public virtual void setRepeatHorizontally(TileMap.Repeat r)
		{
			this.repeatedHorizontally = r;
			this.updateVars();
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000EA01 File Offset: 0x0000CC01
		public virtual void setRepeatVertically(TileMap.Repeat r)
		{
			this.repeatedVertically = r;
			this.updateVars();
		}

		// Token: 0x0600029D RID: 669 RVA: 0x0000EA10 File Offset: 0x0000CC10
		public virtual void updateWithCameraPos(Vector pos)
		{
			float num = (float)Math.Round((double)(pos.x / this.parallaxRatio));
			float num2 = (float)Math.Round((double)(pos.y / this.parallaxRatio));
			float num3 = this.x;
			float num4 = this.y;
			if (this.repeatedVertically != TileMap.Repeat.REPEAT_NONE)
			{
				float num13 = num4 - num2;
				int num5 = (int)num13 % this.tileMapHeight;
				num4 = ((num13 >= 0f) ? ((float)(num5 - this.tileMapHeight) + num2) : ((float)num5 + num2));
			}
			if (this.repeatedHorizontally != TileMap.Repeat.REPEAT_NONE)
			{
				float num14 = num3 - num;
				int num6 = (int)num14 % this.tileMapWidth;
				num3 = ((num14 >= 0f) ? ((float)(num6 - this.tileMapWidth) + num) : ((float)num6 + num));
			}
			if (!MathHelper.rectInRect(num, num2, num + (float)this.cameraViewWidth, num2 + (float)this.cameraViewHeight, num3, num4, num3 + (float)this.tileMapWidth, num4 + (float)this.tileMapHeight))
			{
				return;
			}
			Rectangle rectangle = MathHelper.rectInRectIntersection(new Rectangle(num3, num4, (float)this.tileMapWidth, (float)this.tileMapHeight), new Rectangle(num, num2, (float)this.cameraViewWidth, (float)this.cameraViewHeight));
			Vector vector = MathHelper.vect(Math.Max(0f, rectangle.x), Math.Max(0f, rectangle.y));
			Vector vector2 = MathHelper.vect((float)((int)vector.x / this.tileWidth), (float)((int)vector.y / this.tileHeight));
			float num7 = num4 + vector2.y * (float)this.tileHeight;
			Vector vector3 = MathHelper.vect(num3 + vector2.x * (float)this.tileWidth, num7);
			int count = this.drawers.Count;
			for (int i = 0; i < count; i++)
			{
				ImageMultiDrawer imageMultiDrawer = this.drawers[i];
				if (imageMultiDrawer != null)
				{
					imageMultiDrawer.numberOfQuadsToDraw = 0;
				}
			}
			int num8 = (int)(vector2.x + (float)this.maxColsOnScreen - 1f);
			int num9 = (int)(vector2.y + (float)this.maxRowsOnScreen - 1f);
			if (this.repeatedVertically == TileMap.Repeat.REPEAT_NONE)
			{
				num9 = Math.Min(this.rows - 1, num9);
			}
			if (this.repeatedHorizontally == TileMap.Repeat.REPEAT_NONE)
			{
				num8 = Math.Min(this.columns - 1, num8);
			}
			for (int j = (int)vector2.x; j <= num8; j++)
			{
				vector3.y = num7;
				int k = (int)vector2.y;
				while (k <= num9 && vector3.y < num2 + (float)this.cameraViewHeight)
				{
					Rectangle rectangle2 = MathHelper.rectInRectIntersection(new Rectangle(num, num2, (float)this.cameraViewWidth, (float)this.cameraViewHeight), new Rectangle(vector3.x, vector3.y, (float)this.tileWidth, (float)this.tileHeight));
					Rectangle r = new Rectangle(num - vector3.x + rectangle2.x, num2 - vector3.y + rectangle2.y, rectangle2.w, rectangle2.h);
					int num10 = j;
					int num11 = k;
					if (this.repeatedVertically == TileMap.Repeat.REPEAT_EDGES)
					{
						if (vector3.y < this.y)
						{
							num11 = 0;
						}
						else if (vector3.y >= this.y + (float)this.tileMapHeight)
						{
							num11 = this.rows - 1;
						}
					}
					if (this.repeatedHorizontally == TileMap.Repeat.REPEAT_EDGES)
					{
						if (vector3.x < this.x)
						{
							num10 = 0;
						}
						else if (vector3.x >= this.x + (float)this.tileMapWidth)
						{
							num10 = this.columns - 1;
						}
					}
					if (this.horizontalRandom)
					{
						num10 = Math.Abs((int)(MathHelper.fmSin(vector3.x) * (float)this.randomSeed) % this.columns);
					}
					if (this.verticalRandom)
					{
						num11 = Math.Abs((int)(MathHelper.fmSin(vector3.y) * (float)this.randomSeed) % this.rows);
					}
					if (num10 >= this.columns)
					{
						num10 %= this.columns;
					}
					if (num11 >= this.rows)
					{
						num11 %= this.rows;
					}
					int num12 = this.matrix[num10, num11];
					if (num12 >= 0)
					{
						TileEntry tileEntry = this.tiles[num12];
						ImageMultiDrawer imageMultiDrawer2 = this.drawers[tileEntry.drawerIndex];
						Texture2D texture = imageMultiDrawer2.image.texture;
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
					vector3.y += (float)this.tileHeight;
					k++;
				}
				vector3.x += (float)this.tileWidth;
				if (vector3.x >= num + (float)this.cameraViewWidth)
				{
					break;
				}
			}
		}

		// Token: 0x0600029E RID: 670 RVA: 0x0000EF48 File Offset: 0x0000D148
		public virtual void updateVars()
		{
			this.maxColsOnScreen = 2 + (int)Math.Floor((double)(this.cameraViewWidth / (this.tileWidth + 1)));
			this.maxRowsOnScreen = 2 + (int)Math.Floor((double)(this.cameraViewHeight / (this.tileHeight + 1)));
			if (this.repeatedVertically == TileMap.Repeat.REPEAT_NONE)
			{
				this.maxRowsOnScreen = Math.Min(this.maxRowsOnScreen, this.rows);
			}
			if (this.repeatedHorizontally == TileMap.Repeat.REPEAT_NONE)
			{
				this.maxColsOnScreen = Math.Min(this.maxColsOnScreen, this.columns);
			}
			this.width = (this.tileMapWidth = this.columns * this.tileWidth);
			this.height = (this.tileMapHeight = this.rows * this.tileHeight);
		}

		// Token: 0x040001FB RID: 507
		public int[,] matrix;

		// Token: 0x040001FC RID: 508
		private int rows;

		// Token: 0x040001FD RID: 509
		private int columns;

		// Token: 0x040001FE RID: 510
		private List<ImageMultiDrawer> drawers;

		// Token: 0x040001FF RID: 511
		private Dictionary<int, TileEntry> tiles;

		// Token: 0x04000200 RID: 512
		private int cameraViewWidth;

		// Token: 0x04000201 RID: 513
		private int cameraViewHeight;

		// Token: 0x04000202 RID: 514
		private int tileMapWidth;

		// Token: 0x04000203 RID: 515
		private int tileMapHeight;

		// Token: 0x04000204 RID: 516
		private int maxRowsOnScreen;

		// Token: 0x04000205 RID: 517
		private int maxColsOnScreen;

		// Token: 0x04000206 RID: 518
		private int randomSeed;

		// Token: 0x04000207 RID: 519
		private TileMap.Repeat repeatedVertically;

		// Token: 0x04000208 RID: 520
		private TileMap.Repeat repeatedHorizontally;

		// Token: 0x04000209 RID: 521
		private float parallaxRatio;

		// Token: 0x0400020A RID: 522
		private int tileWidth;

		// Token: 0x0400020B RID: 523
		private int tileHeight;

		// Token: 0x0400020C RID: 524
		private bool horizontalRandom;

		// Token: 0x0400020D RID: 525
		private bool verticalRandom;

		// Token: 0x0400020E RID: 526
		private bool restoreTileTransparency;

		// Token: 0x020000B3 RID: 179
		public enum Repeat
		{
			// Token: 0x040008AC RID: 2220
			REPEAT_NONE,
			// Token: 0x040008AD RID: 2221
			REPEAT_ALL,
			// Token: 0x040008AE RID: 2222
			REPEAT_EDGES
		}
	}
}
