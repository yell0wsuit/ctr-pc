using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000033 RID: 51
	internal class GLDrawer : NSObject
	{
		// Token: 0x060001BE RID: 446 RVA: 0x000088F0 File Offset: 0x00006AF0
		public static void drawImage(Texture2D image, float x, float y)
		{
			Texture2D.drawAtPoint(image, MathHelper.vect(x, y));
		}

		// Token: 0x060001BF RID: 447 RVA: 0x000088FF File Offset: 0x00006AFF
		public static void drawImagePart(Texture2D image, Rectangle r, double x, double y)
		{
			GLDrawer.drawImagePart(image, r, (float)x, (float)y);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000890C File Offset: 0x00006B0C
		public static void drawImagePart(Texture2D image, Rectangle r, float x, float y)
		{
			Texture2D.drawRectAtPoint(image, r, MathHelper.vect(x, y));
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000891C File Offset: 0x00006B1C
		public static void drawImageQuad(Texture2D image, int q, double x, double y)
		{
			GLDrawer.drawImageQuad(image, q, (float)x, (float)y);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00008929 File Offset: 0x00006B29
		public static void drawImageQuad(Texture2D image, int q, float x, float y)
		{
			if (q == -1)
			{
				GLDrawer.drawImage(image, x, y);
				return;
			}
			Texture2D.drawQuadAtPoint(image, q, MathHelper.vect(x, y));
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00008948 File Offset: 0x00006B48
		public static void drawImageTiledCool(Texture2D image, int q, float x, float y, float width, float height)
		{
			float xParam = 0f;
			float yParam = 0f;
			float num;
			float num2;
			if (q == -1)
			{
				num = (float)image._realWidth;
				num2 = (float)image._realHeight;
			}
			else
			{
				xParam = image.quadRects[q].x;
				yParam = image.quadRects[q].y;
				num = image.quadRects[q].w;
				num2 = image.quadRects[q].h;
			}
			for (float num3 = 0f; num3 < height; num3 += num2)
			{
				for (float num4 = 0f; num4 < width; num4 += num)
				{
					float num5 = width - num4;
					if (num5 > num)
					{
						num5 = num;
					}
					float num6 = height - num3;
					if (num6 > num2)
					{
						num6 = num2;
					}
					Rectangle r = FrameworkTypes.MakeRectangle(xParam, yParam, num5, num6);
					GLDrawer.drawImagePart(image, r, x + num4, y + num3);
				}
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00008A24 File Offset: 0x00006C24
		public static void drawImageTiled(Texture2D image, int q, float x, float y, float width, float height)
		{
			if (FrameworkTypes.IS_WVGA)
			{
				GLDrawer.drawImageTiledCool(image, q, x, y, width, height);
				return;
			}
			float xParam = 0f;
			float yParam = 0f;
			float num;
			float num2;
			if (q == -1)
			{
				num = (float)image._realWidth;
				num2 = (float)image._realHeight;
			}
			else
			{
				xParam = image.quadRects[q].x;
				yParam = image.quadRects[q].y;
				num = image.quadRects[q].w;
				num2 = image.quadRects[q].h;
			}
			if (width == num && height == num2)
			{
				GLDrawer.drawImageQuad(image, q, x, y);
				return;
			}
			int num3 = (int)MathHelper.ceil((double)(width / num));
			int num12 = (int)MathHelper.ceil((double)(height / num2));
			int num4 = (int)width % (int)num;
			int num5 = (int)height % (int)num2;
			int num6 = (int)((num4 == 0) ? num : ((float)num4));
			int num7 = (int)((num5 == 0) ? num2 : ((float)num5));
			int num8 = (int)x;
			int num9 = (int)y;
			for (int num10 = num12 - 1; num10 >= 0; num10--)
			{
				num8 = (int)x;
				for (int num11 = num3 - 1; num11 >= 0; num11--)
				{
					if (num11 == 0 || num10 == 0)
					{
						Rectangle r = FrameworkTypes.MakeRectangle(xParam, yParam, (num11 == 0) ? ((float)num6) : num, (num10 == 0) ? ((float)num7) : num2);
						GLDrawer.drawImagePart(image, r, (float)num8, (float)num9);
					}
					else
					{
						GLDrawer.drawImageQuad(image, q, (float)num8, (float)num9);
					}
					num8 += (int)num;
				}
				num9 += (int)num2;
			}
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00008B86 File Offset: 0x00006D86
		public static Quad2D getTextureCoordinates(Texture2D t, Rectangle r)
		{
			return Quad2D.MakeQuad2D(t._invWidth * r.x, t._invHeight * r.y, t._invWidth * r.w, t._invHeight * r.h);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x00008BC4 File Offset: 0x00006DC4
		public static Vector calcPathBezier(Vector[] p, int count, float delta)
		{
			Vector[] array = new Vector[count - 1];
			if (count > 2)
			{
				for (int i = 0; i < count - 1; i++)
				{
					array[i] = GLDrawer.calc2PointBezier(ref p[i], ref p[i + 1], delta);
				}
				return GLDrawer.calcPathBezier(array, count - 1, delta);
			}
			if (count == 2)
			{
				return GLDrawer.calc2PointBezier(ref p[0], ref p[1], delta);
			}
			return default(Vector);
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00008C38 File Offset: 0x00006E38
		public static Vector calc2PointBezier(ref Vector a, ref Vector b, float delta)
		{
			float num = 1f - delta;
			return new Vector
			{
				x = a.x * num + b.x * delta,
				y = a.y * num + b.y * delta
			};
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00008C88 File Offset: 0x00006E88
		public static void calcCircle(float x, float y, float radius, int vertexCount, float[] glVertices)
		{
			float num = (float)(6.283185307179586 / (double)vertexCount);
			float num2 = 0f;
			for (int i = 0; i < vertexCount; i++)
			{
				glVertices[i * 2] = x + radius * MathHelper.cosf(num2);
				glVertices[i * 2 + 1] = y + radius * MathHelper.sinf(num2);
				num2 += num;
			}
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00008CDC File Offset: 0x00006EDC
		public static void drawCircleIntersection(float cx1, float cy1, float radius1, float cx2, float cy2, float radius2, int vertexCount, float width, RGBAColor fill)
		{
			float num = MathHelper.vectDistance(MathHelper.vect(cx1, cy1), MathHelper.vect(cx2, cy2));
			if (num < radius1 + radius2 && radius1 < num + radius2)
			{
				float num2 = (radius1 * radius1 - radius2 * radius2 + num * num) / (2f * num);
				float num3 = MathHelper.acosf((num - num2) / radius2);
				float num6 = MathHelper.vectAngle(MathHelper.vectSub(MathHelper.vect(cx1, cy1), MathHelper.vect(cx2, cy2)));
				float num4 = num6 - num3;
				float num5 = num6 + num3;
				if (cx2 > cx1)
				{
					num4 += 3.1415927f;
					num5 += 3.1415927f;
				}
				GLDrawer.drawAntialiasedCurve2(cx2, cy2, radius2, num4, num5, vertexCount, width, 1f, fill);
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00008D80 File Offset: 0x00006F80
		public static void drawAntialiasedCurve2(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float width, float fadeWidth, RGBAColor fill)
		{
			float[] array = new float[(vertexCount - 1) * 12 + 4];
			float[] array2 = new float[vertexCount * 2];
			float[] array3 = new float[vertexCount * 2];
			float[] array4 = new float[vertexCount * 2];
			float[] array5 = new float[vertexCount * 2];
			RGBAColor[] array6 = new RGBAColor[(vertexCount - 1) * 6 + 2];
			GLDrawer.calcCurve(cx, cy, radius + fadeWidth, startAngle, endAngle, vertexCount, array2);
			GLDrawer.calcCurve(cx, cy, radius, startAngle, endAngle, vertexCount, array3);
			GLDrawer.calcCurve(cx, cy, radius - width, startAngle, endAngle, vertexCount, array4);
			GLDrawer.calcCurve(cx, cy, radius - width - fadeWidth, startAngle, endAngle, vertexCount, array5);
			array[0] = array2[0];
			array[1] = array2[1];
			array6[0] = RGBAColor.transparentRGBA;
			for (int i = 1; i < vertexCount; i += 2)
			{
				array[12 * i - 10] = array2[i * 2];
				array[12 * i - 9] = array2[i * 2 + 1];
				array[12 * i - 8] = array3[i * 2 - 2];
				array[12 * i - 7] = array3[i * 2 - 1];
				array[12 * i - 6] = array3[i * 2];
				array[12 * i - 5] = array3[i * 2 + 1];
				array[12 * i - 4] = array4[i * 2 - 2];
				array[12 * i - 3] = array4[i * 2 - 1];
				array[12 * i - 2] = array4[i * 2];
				array[12 * i - 1] = array4[i * 2 + 1];
				array[12 * i] = array5[i * 2 - 2];
				array[12 * i + 1] = array5[i * 2 - 1];
				array[12 * i + 2] = array5[i * 2 + 2];
				array[12 * i + 3] = array5[i * 2 + 3];
				array[12 * i + 4] = array4[i * 2];
				array[12 * i + 5] = array4[i * 2 + 1];
				array[12 * i + 6] = array4[i * 2 + 2];
				array[12 * i + 7] = array4[i * 2 + 3];
				array[12 * i + 8] = array3[i * 2];
				array[12 * i + 9] = array3[i * 2 + 1];
				array[12 * i + 10] = array3[i * 2 + 2];
				array[12 * i + 11] = array3[i * 2 + 3];
				array[12 * i + 12] = array2[i * 2];
				array[12 * i + 13] = array2[i * 2 + 1];
				array6[6 * i - 5] = RGBAColor.transparentRGBA;
				array6[6 * i - 4] = fill;
				array6[6 * i - 3] = fill;
				array6[6 * i - 2] = fill;
				array6[6 * i - 1] = fill;
				array6[6 * i] = RGBAColor.transparentRGBA;
				array6[6 * i + 1] = RGBAColor.transparentRGBA;
				array6[6 * i + 2] = fill;
				array6[6 * i + 3] = fill;
				array6[6 * i + 4] = fill;
				array6[6 * i + 5] = fill;
				array6[6 * i + 6] = RGBAColor.transparentRGBA;
			}
			array[(vertexCount - 1) * 12 + 2] = array2[vertexCount * 2 - 2];
			array[(vertexCount - 1) * 12 + 3] = array2[vertexCount * 2 - 1];
			array6[(vertexCount - 1) * 6 + 1] = RGBAColor.transparentRGBA;
			OpenGL.glColorPointer(4, 5, 0, array6);
			OpenGL.glDisableClientState(0);
			OpenGL.glEnableClientState(13);
			OpenGL.glVertexPointer(2, 5, 0, array);
			OpenGL.glDrawArrays(8, 0, (vertexCount - 1) * 6 + 2);
			OpenGL.glEnableClientState(0);
			OpenGL.glDisableClientState(13);
		}

		// Token: 0x060001CB RID: 459 RVA: 0x00009114 File Offset: 0x00007314
		private static void calcCurve(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float[] glVertices)
		{
			float num7 = (endAngle - startAngle) / (float)(vertexCount - 1);
			float num = MathHelper.tanf(num7);
			float num2 = MathHelper.cosf(num7);
			float num3 = radius * MathHelper.cosf(startAngle);
			float num4 = radius * MathHelper.sinf(startAngle);
			for (int i = 0; i < vertexCount; i++)
			{
				glVertices[i * 2] = num3 + cx;
				glVertices[i * 2 + 1] = num4 + cy;
				float num5 = 0f - num4;
				float num6 = num3;
				num3 += num5 * num;
				num4 += num6 * num;
				num3 *= num2;
				num4 *= num2;
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00009194 File Offset: 0x00007394
		public static void drawAntialiasedLine(float x1, float y1, float x2, float y2, float size, RGBAColor color)
		{
			Vector v = MathHelper.vect(x1, y1);
			Vector vector = MathHelper.vectSub(MathHelper.vect(x2, y2), v);
			Vector v2 = MathHelper.vectPerp(vector);
			Vector vector2 = MathHelper.vectNormalize(v2);
			v2 = MathHelper.vectMult(vector2, size);
			Vector v3 = MathHelper.vectNeg(v2);
			Vector v4 = MathHelper.vectAdd(v2, vector);
			Vector v5 = MathHelper.vectAdd(MathHelper.vectNeg(v2), vector);
			v2 = MathHelper.vectAdd(v2, v);
			v3 = MathHelper.vectAdd(v3, v);
			v4 = MathHelper.vectAdd(v4, v);
			v5 = MathHelper.vectAdd(v5, v);
			Vector vector3 = MathHelper.vectSub(v2, vector2);
			Vector vector4 = MathHelper.vectSub(v4, vector2);
			Vector vector5 = MathHelper.vectAdd(v3, vector2);
			Vector vector6 = MathHelper.vectAdd(v5, vector2);
			float[] pointer = new float[]
			{
				v2.x, v2.y, v4.x, v4.y, vector3.x, vector3.y, vector4.x, vector4.y, vector5.x, vector5.y,
				vector6.x, vector6.y, v3.x, v3.y, v5.x, v5.y
			};
			GLDrawer.colors[2] = color;
			GLDrawer.colors[3] = color;
			GLDrawer.colors[4] = color;
			GLDrawer.colors[5] = color;
			OpenGL.glColorPointer_add(4, 5, 0, GLDrawer.colors);
			OpenGL.glVertexPointer_add(2, 5, 0, pointer);
		}

		// Token: 0x060001CD RID: 461 RVA: 0x00009333 File Offset: 0x00007533
		public static void drawRect(float x, float y, float w, float h, RGBAColor color)
		{
			GLDrawer.drawPolygon(new float[]
			{
				x,
				y,
				x + w,
				y,
				x,
				y + h,
				x + w,
				y + h
			}, 4, color);
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000936B File Offset: 0x0000756B
		public static void drawSolidRect(float x, float y, float w, float h, RGBAColor border, RGBAColor fill)
		{
			GLDrawer.drawSolidPolygon(new float[]
			{
				x,
				y,
				x + w,
				y,
				x,
				y + h,
				x + w,
				y + h
			}, 4, border, fill);
		}

		// Token: 0x060001CF RID: 463 RVA: 0x000093A8 File Offset: 0x000075A8
		public static void drawSolidRectWOBorder(float x, float y, float w, float h, RGBAColor fill)
		{
			float[] pointer = new float[]
			{
				x,
				y,
				x + w,
				y,
				x,
				y + h,
				x + w,
				y + h
			};
			OpenGL.glColor4f(fill.toXNA());
			OpenGL.glVertexPointer(2, 5, 0, pointer);
			OpenGL.glDrawArrays(8, 0, 4);
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00009401 File Offset: 0x00007601
		public static void drawPolygon(float[] vertices, int vertexCount, RGBAColor color)
		{
			OpenGL.glColor4f(color.toXNA());
			OpenGL.glVertexPointer(2, 5, 0, vertices);
			OpenGL.glDrawArrays(9, 0, vertexCount);
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00009421 File Offset: 0x00007621
		public static void drawSolidPolygon(float[] vertices, int vertexCount, RGBAColor border, RGBAColor fill)
		{
			OpenGL.glVertexPointer(2, 5, 0, vertices);
			OpenGL.glColor4f(fill.toXNA());
			OpenGL.glDrawArrays(8, 0, vertexCount);
			OpenGL.glColor4f(border.toXNA());
			OpenGL.glDrawArrays(9, 0, vertexCount);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00009455 File Offset: 0x00007655
		public static void drawSolidPolygonWOBorder(float[] vertices, int vertexCount, RGBAColor fill)
		{
			OpenGL.glVertexPointer(2, 5, 0, vertices);
			OpenGL.glColor4f(fill.toXNA());
			OpenGL.glDrawArrays(8, 0, vertexCount);
		}

		// Token: 0x04000139 RID: 313
		private static RGBAColor[] colors = new RGBAColor[]
		{
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA
		};
	}
}
