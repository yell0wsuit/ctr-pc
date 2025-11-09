using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.visual
{
    internal class GLDrawer : NSObject
    {
        public static void drawImage(CTRTexture2D image, float x, float y)
        {
            CTRTexture2D.drawAtPoint(image, vect(x, y));
        }

        public static void drawImagePart(CTRTexture2D image, CTRRectangle r, double x, double y)
        {
            drawImagePart(image, r, (float)x, (float)y);
        }

        public static void drawImagePart(CTRTexture2D image, CTRRectangle r, float x, float y)
        {
            CTRTexture2D.drawRectAtPoint(image, r, vect(x, y));
        }

        public static void drawImageQuad(CTRTexture2D image, int q, double x, double y)
        {
            drawImageQuad(image, q, (float)x, (float)y);
        }

        public static void drawImageQuad(CTRTexture2D image, int q, float x, float y)
        {
            if (q == -1)
            {
                drawImage(image, x, y);
                return;
            }
            CTRTexture2D.drawQuadAtPoint(image, q, vect(x, y));
        }

        public static void drawImageTiledCool(CTRTexture2D image, int q, float x, float y, float width, float height)
        {
            float xParam = 0f;
            float yParam = 0f;
            float num;
            float num2;
            if (q == -1)
            {
                num = image._realWidth;
                num2 = image._realHeight;
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
                    CTRRectangle r = MakeRectangle(xParam, yParam, num5, num6);
                    drawImagePart(image, r, x + num4, y + num3);
                }
            }
        }

        public static void drawImageTiled(CTRTexture2D image, int q, float x, float y, float width, float height)
        {
            if (IS_WVGA)
            {
                drawImageTiledCool(image, q, x, y, width, height);
                return;
            }
            float xParam = 0f;
            float yParam = 0f;
            float num;
            float num2;
            if (q == -1)
            {
                num = image._realWidth;
                num2 = image._realHeight;
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
                drawImageQuad(image, q, x, y);
                return;
            }
            int num3 = (int)ceil((double)(width / num));
            int num12 = (int)ceil((double)(height / num2));
            int num4 = (int)width % (int)num;
            int num5 = (int)height % (int)num2;
            int num6 = (int)((num4 == 0) ? num : num4);
            int num7 = (int)((num5 == 0) ? num2 : num5);
            int num8 = (int)x;
            int num9 = (int)y;
            for (int num10 = num12 - 1; num10 >= 0; num10--)
            {
                num8 = (int)x;
                for (int num11 = num3 - 1; num11 >= 0; num11--)
                {
                    if (num11 == 0 || num10 == 0)
                    {
                        CTRRectangle r = MakeRectangle(xParam, yParam, (num11 == 0) ? num6 : num, (num10 == 0) ? num7 : num2);
                        drawImagePart(image, r, num8, num9);
                    }
                    else
                    {
                        drawImageQuad(image, q, num8, num9);
                    }
                    num8 += (int)num;
                }
                num9 += (int)num2;
            }
        }

        public static Quad2D getTextureCoordinates(CTRTexture2D t, CTRRectangle r)
        {
            return Quad2D.MakeQuad2D(t._invWidth * r.x, t._invHeight * r.y, t._invWidth * r.w, t._invHeight * r.h);
        }

        public static Vector calcPathBezier(Vector[] p, int count, float delta)
        {
            Vector[] array = new Vector[count - 1];
            if (count > 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    array[i] = calc2PointBezier(ref p[i], ref p[i + 1], delta);
                }
                return calcPathBezier(array, count - 1, delta);
            }
            if (count == 2)
            {
                return calc2PointBezier(ref p[0], ref p[1], delta);
            }
            return default(Vector);
        }

        public static Vector calc2PointBezier(ref Vector a, ref Vector b, float delta)
        {
            float num = 1f - delta;
            return new Vector
            {
                x = a.x * num + b.x * delta,
                y = a.y * num + b.y * delta
            };
        }

        public static void calcCircle(float x, float y, float radius, int vertexCount, float[] glVertices)
        {
            float num = (float)(6.283185307179586 / vertexCount);
            float num2 = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                glVertices[i * 2] = x + radius * cosf(num2);
                glVertices[i * 2 + 1] = y + radius * sinf(num2);
                num2 += num;
            }
        }

        public static void drawCircleIntersection(float cx1, float cy1, float radius1, float cx2, float cy2, float radius2, int vertexCount, float width, RGBAColor fill)
        {
            float num = vectDistance(vect(cx1, cy1), vect(cx2, cy2));
            if (num < radius1 + radius2 && radius1 < num + radius2)
            {
                float num2 = (radius1 * radius1 - radius2 * radius2 + num * num) / (2f * num);
                float num3 = acosf((num - num2) / radius2);
                float num6 = vectAngle(vectSub(vect(cx1, cy1), vect(cx2, cy2)));
                float num4 = num6 - num3;
                float num5 = num6 + num3;
                if (cx2 > cx1)
                {
                    num4 += 3.1415927f;
                    num5 += 3.1415927f;
                }
                drawAntialiasedCurve2(cx2, cy2, radius2, num4, num5, vertexCount, width, 1f, fill);
            }
        }

        public static void drawAntialiasedCurve2(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float width, float fadeWidth, RGBAColor fill)
        {
            float[] array = new float[(vertexCount - 1) * 12 + 4];
            float[] array2 = new float[vertexCount * 2];
            float[] array3 = new float[vertexCount * 2];
            float[] array4 = new float[vertexCount * 2];
            float[] array5 = new float[vertexCount * 2];
            RGBAColor[] array6 = new RGBAColor[(vertexCount - 1) * 6 + 2];
            calcCurve(cx, cy, radius + fadeWidth, startAngle, endAngle, vertexCount, array2);
            calcCurve(cx, cy, radius, startAngle, endAngle, vertexCount, array3);
            calcCurve(cx, cy, radius - width, startAngle, endAngle, vertexCount, array4);
            calcCurve(cx, cy, radius - width - fadeWidth, startAngle, endAngle, vertexCount, array5);
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

        private static void calcCurve(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float[] glVertices)
        {
            float num7 = (endAngle - startAngle) / (vertexCount - 1);
            float num = tanf(num7);
            float num2 = cosf(num7);
            float num3 = radius * cosf(startAngle);
            float num4 = radius * sinf(startAngle);
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

        public static void drawAntialiasedLine(float x1, float y1, float x2, float y2, float size, RGBAColor color)
        {
            Vector v = vect(x1, y1);
            Vector vector = vectSub(vect(x2, y2), v);
            Vector v2 = vectPerp(vector);
            Vector vector2 = vectNormalize(v2);
            v2 = vectMult(vector2, size);
            Vector v3 = vectNeg(v2);
            Vector v4 = vectAdd(v2, vector);
            Vector v5 = vectAdd(vectNeg(v2), vector);
            v2 = vectAdd(v2, v);
            v3 = vectAdd(v3, v);
            v4 = vectAdd(v4, v);
            v5 = vectAdd(v5, v);
            Vector vector3 = vectSub(v2, vector2);
            Vector vector4 = vectSub(v4, vector2);
            Vector vector5 = vectAdd(v3, vector2);
            Vector vector6 = vectAdd(v5, vector2);
            float[] pointer =
            [
                v2.x, v2.y, v4.x, v4.y, vector3.x, vector3.y, vector4.x, vector4.y, vector5.x, vector5.y,
                vector6.x, vector6.y, v3.x, v3.y, v5.x, v5.y
            ];
            colors[2] = color;
            colors[3] = color;
            colors[4] = color;
            colors[5] = color;
            OpenGL.glColorPointer_add(4, 5, 0, colors);
            OpenGL.glVertexPointer_add(2, 5, 0, pointer);
        }

        public static void drawRect(float x, float y, float w, float h, RGBAColor color)
        {
            drawPolygon(
            [
                x,
                y,
                x + w,
                y,
                x,
                y + h,
                x + w,
                y + h
            ], 4, color);
        }

        public static void drawSolidRect(float x, float y, float w, float h, RGBAColor border, RGBAColor fill)
        {
            drawSolidPolygon(
            [
                x,
                y,
                x + w,
                y,
                x,
                y + h,
                x + w,
                y + h
            ], 4, border, fill);
        }

        public static void drawSolidRectWOBorder(float x, float y, float w, float h, RGBAColor fill)
        {
            float[] pointer =
            [
                x,
                y,
                x + w,
                y,
                x,
                y + h,
                x + w,
                y + h
            ];
            OpenGL.glColor4f(fill.toXNA());
            OpenGL.glVertexPointer(2, 5, 0, pointer);
            OpenGL.glDrawArrays(8, 0, 4);
        }

        public static void drawPolygon(float[] vertices, int vertexCount, RGBAColor color)
        {
            OpenGL.glColor4f(color.toXNA());
            OpenGL.glVertexPointer(2, 5, 0, vertices);
            OpenGL.glDrawArrays(9, 0, vertexCount);
        }

        public static void drawSolidPolygon(float[] vertices, int vertexCount, RGBAColor border, RGBAColor fill)
        {
            OpenGL.glVertexPointer(2, 5, 0, vertices);
            OpenGL.glColor4f(fill.toXNA());
            OpenGL.glDrawArrays(8, 0, vertexCount);
            OpenGL.glColor4f(border.toXNA());
            OpenGL.glDrawArrays(9, 0, vertexCount);
        }

        public static void drawSolidPolygonWOBorder(float[] vertices, int vertexCount, RGBAColor fill)
        {
            OpenGL.glVertexPointer(2, 5, 0, vertices);
            OpenGL.glColor4f(fill.toXNA());
            OpenGL.glDrawArrays(8, 0, vertexCount);
        }

        private static RGBAColor[] colors =
[
    RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA
];
    }
}
