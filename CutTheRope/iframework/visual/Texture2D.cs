using System;
using CutTheRope.ctr_commons;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200004A RID: 74
	internal class Texture2D : NSObject
	{
		// Token: 0x06000273 RID: 627 RVA: 0x0000DF10 File Offset: 0x0000C110
		public static void drawRectAtPoint(Texture2D t, Rectangle rect, Vector point)
		{
			float num = t._invWidth * rect.x;
			float num2 = t._invHeight * rect.y;
			float num3 = num + t._invWidth * rect.w;
			float num4 = num2 + t._invHeight * rect.h;
			float[] pointer = new float[] { num, num2, num3, num2, num, num4, num3, num4 };
			float[] array = new float[12];
			array[0] = point.x;
			array[1] = point.y;
			array[3] = rect.w + point.x;
			array[4] = point.y;
			array[6] = point.x;
			array[7] = rect.h + point.y;
			array[9] = rect.w + point.x;
			array[10] = rect.h + point.y;
			float[] pointer2 = array;
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(t.name());
			OpenGL.glVertexPointer(3, 5, 0, pointer2);
			OpenGL.glTexCoordPointer(2, 5, 0, pointer);
			OpenGL.glDrawArrays(8, 0, 4);
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0000E01D File Offset: 0x0000C21D
		public Texture2D name()
		{
			return this;
		}

		// Token: 0x06000275 RID: 629 RVA: 0x0000E020 File Offset: 0x0000C220
		public bool isWvga()
		{
			return this._isWvga;
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0000E028 File Offset: 0x0000C228
		public virtual void setQuadsCapacity(int n)
		{
			this.quadsCount = n;
			this.quads = new Quad2D[this.quadsCount];
			this.quadRects = new Rectangle[this.quadsCount];
			this.quadOffsets = new Vector[this.quadsCount];
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0000E064 File Offset: 0x0000C264
		public virtual void setQuadAt(Rectangle rect, int n)
		{
			this.quads[n] = GLDrawer.getTextureCoordinates(this, rect);
			this.quadRects[n] = rect;
			this.quadOffsets[n] = MathHelper.vectZero;
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0000E097 File Offset: 0x0000C297
		public virtual void setWvga()
		{
			this._isWvga = true;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000E0A0 File Offset: 0x0000C2A0
		public virtual void setScale(float scaleX, float scaleY)
		{
			this._scaleX = scaleX;
			this._scaleY = scaleY;
			this.calculateForQuickDrawing();
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000E0B8 File Offset: 0x0000C2B8
		public static void drawQuadAtPoint(Texture2D t, int q, Vector point)
		{
			Quad2D quad2D = t.quads[q];
			float[] array = new float[12];
			array[0] = point.x;
			array[1] = point.y;
			array[3] = t.quadRects[q].w + point.x;
			array[4] = point.y;
			array[6] = point.x;
			array[7] = t.quadRects[q].h + point.y;
			array[9] = t.quadRects[q].w + point.x;
			array[10] = t.quadRects[q].h + point.y;
			float[] pointer = array;
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(t.name());
			OpenGL.glVertexPointer(3, 5, 0, pointer);
			OpenGL.glTexCoordPointer(2, 5, 0, quad2D.toFloatArray());
			OpenGL.glDrawArrays(8, 0, 4);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0000E1A0 File Offset: 0x0000C3A0
		public static void drawAtPoint(Texture2D t, Vector point)
		{
			float[] pointer = new float[] { 0f, 0f, t._maxS, 0f, 0f, t._maxT, t._maxS, t._maxT };
			float[] array = new float[12];
			array[0] = point.x;
			array[1] = point.y;
			array[3] = (float)t._realWidth + point.x;
			array[4] = point.y;
			array[6] = point.x;
			array[7] = (float)t._realHeight + point.y;
			array[9] = (float)t._realWidth + point.x;
			array[10] = (float)t._realHeight + point.y;
			float[] pointer2 = array;
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(t.name());
			OpenGL.glVertexPointer(3, 5, 0, pointer2);
			OpenGL.glTexCoordPointer(2, 5, 0, pointer);
			OpenGL.glDrawArrays(8, 0, 4);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0000E278 File Offset: 0x0000C478
		public virtual void calculateForQuickDrawing()
		{
			if (this._isWvga)
			{
				this._realWidth = (int)(this._width * this._maxS / this._scaleX);
				this._realHeight = (int)(this._height * this._maxT / this._scaleY);
				this._invWidth = 1f / (this._width / this._scaleX);
				this._invHeight = 1f / (this._height / this._scaleY);
				return;
			}
			this._realWidth = (int)(this._width * this._maxS);
			this._realHeight = (int)(this._height * this._maxT);
			this._invWidth = 1f / this._width;
			this._invHeight = 1f / this._height;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0000E352 File Offset: 0x0000C552
		public static void setAntiAliasTexParameters()
		{
		}

		// Token: 0x0600027E RID: 638 RVA: 0x0000E354 File Offset: 0x0000C554
		public static void setAliasTexParameters()
		{
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000E356 File Offset: 0x0000C556
		public virtual void reg()
		{
			this.prev = Texture2D.tail;
			if (this.prev != null)
			{
				this.prev.next = this;
			}
			else
			{
				Texture2D.root = this;
			}
			Texture2D.tail = this;
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000E388 File Offset: 0x0000C588
		public virtual void unreg()
		{
			if (this.prev != null)
			{
				this.prev.next = this.next;
			}
			else
			{
				Texture2D.root = this.next;
			}
			if (this.next != null)
			{
				this.next.prev = this.prev;
			}
			else
			{
				Texture2D.tail = this.prev;
			}
			this.next = (this.prev = null);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000E3F4 File Offset: 0x0000C5F4
		public virtual Texture2D initWithPath(string path, bool assets)
		{
			if (base.init() == null)
			{
				return null;
			}
			this._resName = path;
			this._name = 65536U;
			this._localTexParams = Texture2D._texParams;
			this.reg();
			this.xnaTexture_ = Images.get(path);
			if (this.xnaTexture_ == null)
			{
				return null;
			}
			this.imageLoaded(this.xnaTexture_.Width, this.xnaTexture_.Height);
			this.quadsCount = 0;
			this.calculateForQuickDrawing();
			this.resume();
			return this;
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0000E474 File Offset: 0x0000C674
		private static int calcRealSize(int size)
		{
			return size;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000E478 File Offset: 0x0000C678
		private void imageLoaded(int w, int h)
		{
			this._lowypoint = h;
			int num = Texture2D.calcRealSize(w);
			int num2 = Texture2D.calcRealSize(h);
			this._size = new Vector((float)num, (float)num2);
			this._width = (uint)num;
			this._height = (uint)num2;
			this._format = Texture2D._defaultAlphaPixelFormat;
			this._maxS = (float)w / (float)num;
			this._maxT = (float)h / (float)num2;
			this._hasPremultipliedAlpha = true;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x0000E4DF File Offset: 0x0000C6DF
		private void resume()
		{
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0000E4E1 File Offset: 0x0000C6E1
		public static void setDefaultAlphaPixelFormat(Texture2D.Texture2DPixelFormat format)
		{
			Texture2D._defaultAlphaPixelFormat = format;
		}

		// Token: 0x06000286 RID: 646 RVA: 0x0000E4E9 File Offset: 0x0000C6E9
		public void optimizeMemory()
		{
			int lowypoint = this._lowypoint;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x0000E4F2 File Offset: 0x0000C6F2
		public virtual void suspend()
		{
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0000E4F4 File Offset: 0x0000C6F4
		public static void suspendAll()
		{
			for (Texture2D texture2D = Texture2D.root; texture2D != null; texture2D = texture2D.next)
			{
				texture2D.suspend();
			}
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0000E51C File Offset: 0x0000C71C
		public static void resumeAll()
		{
			for (Texture2D texture2D = Texture2D.root; texture2D != null; texture2D = texture2D.next)
			{
				texture2D.resume();
			}
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0000E544 File Offset: 0x0000C744
		public virtual NSObject initFromPixels(int x, int y, int w, int h)
		{
			if (base.init() == null)
			{
				return null;
			}
			this._name = 65536U;
			this._lowypoint = -1;
			this._localTexParams = Texture2D._defaultTexParams;
			this.reg();
			int num = Texture2D.calcRealSize(w);
			int num2 = Texture2D.calcRealSize(h);
			float transitionTime = Application.sharedRootController().transitionTime;
			Application.sharedRootController().transitionTime = -1f;
			RenderTarget2D renderTarget;
			if (Global.ScreenSizeManager.IsFullScreen)
			{
				CtrRenderer.onDrawFrame();
				renderTarget = OpenGL.DetachRenderTarget();
			}
			else
			{
				renderTarget = new RenderTarget2D(Global.GraphicsDevice, Global.GraphicsDevice.PresentationParameters.BackBufferWidth, Global.GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
				Global.GraphicsDevice.SetRenderTarget(renderTarget);
				CtrRenderer.onDrawFrame();
			}
			Global.GraphicsDevice.SetRenderTarget(null);
			Application.sharedRootController().transitionTime = transitionTime;
			this.xnaTexture_ = renderTarget;
			this._format = Texture2D.Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;
			this._size = new Vector((float)num, (float)num2);
			this._width = (uint)num;
			this._height = (uint)num2;
			this._maxS = (float)w / (float)num;
			this._maxT = (float)h / (float)num2;
			this._hasPremultipliedAlpha = true;
			this.quadsCount = 0;
			this.calculateForQuickDrawing();
			this.resume();
			return this;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000E66F File Offset: 0x0000C86F
		public override void dealloc()
		{
			if (this.xnaTexture_ != null)
			{
				Images.free(this._resName);
				this.xnaTexture_ = null;
			}
		}

		// Token: 0x040001D5 RID: 469
		private const int UNDEFINED_TEXTURE = 65536;

		// Token: 0x040001D6 RID: 470
		public Texture2D xnaTexture_;

		// Token: 0x040001D7 RID: 471
		public string _resName;

		// Token: 0x040001D8 RID: 472
		private uint _name;

		// Token: 0x040001D9 RID: 473
		public Quad2D[] quads;

		// Token: 0x040001DA RID: 474
		private uint _width;

		// Token: 0x040001DB RID: 475
		private uint _height;

		// Token: 0x040001DC RID: 476
		public int _lowypoint;

		// Token: 0x040001DD RID: 477
		public float _maxS;

		// Token: 0x040001DE RID: 478
		public float _maxT;

		// Token: 0x040001DF RID: 479
		private float _scaleX;

		// Token: 0x040001E0 RID: 480
		private float _scaleY;

		// Token: 0x040001E1 RID: 481
		private Texture2D.Texture2DPixelFormat _format;

		// Token: 0x040001E2 RID: 482
		private Vector _size;

		// Token: 0x040001E3 RID: 483
		private bool _hasPremultipliedAlpha;

		// Token: 0x040001E4 RID: 484
		public Vector[] quadOffsets;

		// Token: 0x040001E5 RID: 485
		public Rectangle[] quadRects;

		// Token: 0x040001E6 RID: 486
		public int quadsCount;

		// Token: 0x040001E7 RID: 487
		public int _realWidth;

		// Token: 0x040001E8 RID: 488
		public int _realHeight;

		// Token: 0x040001E9 RID: 489
		public float _invWidth;

		// Token: 0x040001EA RID: 490
		public float _invHeight;

		// Token: 0x040001EB RID: 491
		public Vector preCutSize;

		// Token: 0x040001EC RID: 492
		private bool _isWvga;

		// Token: 0x040001ED RID: 493
		private Texture2D.TexParams _localTexParams;

		// Token: 0x040001EE RID: 494
		private static Texture2D.TexParams _defaultTexParams;

		// Token: 0x040001EF RID: 495
		private static Texture2D.TexParams _texParams;

		// Token: 0x040001F0 RID: 496
		private static Texture2D.TexParams _texParamsCopy;

		// Token: 0x040001F1 RID: 497
		private bool PixelCorrectionDone;

		// Token: 0x040001F2 RID: 498
		private static Texture2D root;

		// Token: 0x040001F3 RID: 499
		private static Texture2D tail;

		// Token: 0x040001F4 RID: 500
		private Texture2D next;

		// Token: 0x040001F5 RID: 501
		private Texture2D prev;

		// Token: 0x040001F6 RID: 502
		public static Texture2D.Texture2DPixelFormat kTexture2DPixelFormat_Default = Texture2D.Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;

		// Token: 0x040001F7 RID: 503
		private static Texture2D.Texture2DPixelFormat _defaultAlphaPixelFormat = Texture2D.kTexture2DPixelFormat_Default;

		// Token: 0x020000B1 RID: 177
		public enum Texture2DPixelFormat
		{
			// Token: 0x040008A0 RID: 2208
			kTexture2DPixelFormat_RGBA8888,
			// Token: 0x040008A1 RID: 2209
			kTexture2DPixelFormat_RGB565,
			// Token: 0x040008A2 RID: 2210
			kTexture2DPixelFormat_RGBA4444,
			// Token: 0x040008A3 RID: 2211
			kTexture2DPixelFormat_RGB5A1,
			// Token: 0x040008A4 RID: 2212
			kTexture2DPixelFormat_A8,
			// Token: 0x040008A5 RID: 2213
			kTexture2DPixelFormat_PVRTC2,
			// Token: 0x040008A6 RID: 2214
			kTexture2DPixelFormat_PVRTC4
		}

		// Token: 0x020000B2 RID: 178
		private struct TexParams
		{
			// Token: 0x040008A7 RID: 2215
			private uint minFilter;

			// Token: 0x040008A8 RID: 2216
			private uint magFilter;

			// Token: 0x040008A9 RID: 2217
			private uint wrapS;

			// Token: 0x040008AA RID: 2218
			private uint wrapT;
		}
	}
}
