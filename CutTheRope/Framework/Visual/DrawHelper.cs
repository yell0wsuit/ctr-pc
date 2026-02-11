using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal sealed class DrawHelper : FrameworkTypes
    {
        public static void DrawImage(CTRTexture2D image, float x, float y)
        {
            CTRTexture2D.DrawAtPoint(image, Vect(x, y));
        }

        public static void DrawImagePart(CTRTexture2D image, CTRRectangle r, float x, float y)
        {
            CTRTexture2D.DrawRectAtPoint(image, r, Vect(x, y));
        }

        public static void DrawImageQuad(CTRTexture2D image, int q, float x, float y)
        {
            if (q == -1)
            {
                DrawImage(image, x, y);
                return;
            }
            CTRTexture2D.DrawQuadAtPoint(image, q, Vect(x, y));
        }

        public static void DrawImageTiledCool(CTRTexture2D image, int q, float x, float y, float width, float height)
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
                    DrawImagePart(image, r, x + num4, y + num3);
                }
            }
        }

        public static void DrawImageTiled(CTRTexture2D image, int q, float x, float y, float width, float height)
        {
            if (IS_WVGA)
            {
                DrawImageTiledCool(image, q, x, y, width, height);
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
                DrawImageQuad(image, q, x, y);
                return;
            }
            int num3 = (int)Ceil(width / num);
            int num12 = (int)Ceil(height / num2);
            int num4 = (int)width % (int)num;
            int num5 = (int)height % (int)num2;
            int num6 = (int)(num4 == 0 ? num : num4);
            int num7 = (int)(num5 == 0 ? num2 : num5);
            int num9 = (int)y;
            for (int num10 = num12 - 1; num10 >= 0; num10--)
            {
                int num8 = (int)x;
                for (int num11 = num3 - 1; num11 >= 0; num11--)
                {
                    if (num11 == 0 || num10 == 0)
                    {
                        CTRRectangle r = MakeRectangle(xParam, yParam, num11 == 0 ? num6 : num, num10 == 0 ? num7 : num2);
                        DrawImagePart(image, r, num8, num9);
                    }
                    else
                    {
                        DrawImageQuad(image, q, num8, num9);
                    }
                    num8 += (int)num;
                }
                num9 += (int)num2;
            }
        }

        public static Quad2D GetTextureCoordinates(CTRTexture2D t, CTRRectangle r)
        {
            return Quad2D.MakeQuad2D(t._invWidth * r.x, t._invHeight * r.y, t._invWidth * r.w, t._invHeight * r.h);
        }

        public static Vector CalcPathBezier(Vector[] p, int count, float delta)
        {
            Vector[] array = new Vector[count - 1];
            if (count > 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    array[i] = Calc2PointBezier(ref p[i], ref p[i + 1], delta);
                }
                return CalcPathBezier(array, count - 1, delta);
            }
            return count == 2 ? Calc2PointBezier(ref p[0], ref p[1], delta) : default;
        }

        public static Vector Calc2PointBezier(ref Vector a, ref Vector b, float delta)
        {
            float num = 1f - delta;
            return new Vector
            {
                X = (a.X * num) + (b.X * delta),
                Y = (a.Y * num) + (b.Y * delta)
            };
        }

        public static void CalcCircle(float x, float y, float radius, int vertexCount, float[] glVertices)
        {
            float num = (float)(6.283185307179586 / vertexCount);
            float num2 = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                glVertices[i * 2] = x + (radius * Cosf(num2));
                glVertices[(i * 2) + 1] = y + (radius * Sinf(num2));
                num2 += num;
            }
        }

        public static void DrawCircleIntersection(float cx1, float cy1, float radius1, float cx2, float cy2, float radius2, int vertexCount, float width, RGBAColor fill)
        {
            float num = VectDistance(Vect(cx1, cy1), Vect(cx2, cy2));
            if (num < radius1 + radius2 && radius1 < num + radius2)
            {
                float num2 = ((radius1 * radius1) - (radius2 * radius2) + (num * num)) / (2f * num);
                float num3 = Acosf((num - num2) / radius2);
                float num6 = VectAngle(VectSub(Vect(cx1, cy1), Vect(cx2, cy2)));
                float num4 = num6 - num3;
                float num5 = num6 + num3;
                if (cx2 > cx1)
                {
                    num4 += 3.1415927f;
                    num5 += 3.1415927f;
                }
                DrawAntialiasedCurve2(cx2, cy2, radius2, num4, num5, vertexCount, width, 1f, fill);
            }
        }

        public static void DrawAntialiasedCurve2(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float width, float fadeWidth, RGBAColor fill)
        {
            float[] array = GetFloatCache(ref s_curveVerticesCache, ((vertexCount - 1) * 12) + 4);
            float[] array2 = GetFloatCache(ref s_curveOuterCache, vertexCount * 2);
            float[] array3 = GetFloatCache(ref s_curveInnerCache, vertexCount * 2);
            float[] array4 = GetFloatCache(ref s_curveInnerEdgeCache, vertexCount * 2);
            float[] array5 = GetFloatCache(ref s_curveInnerFadeCache, vertexCount * 2);
            RGBAColor[] array6 = GetColorCache(ref s_curveColorCache, ((vertexCount - 1) * 6) + 2);
            CalcCurve(cx, cy, radius + fadeWidth, startAngle, endAngle, vertexCount, array2);
            CalcCurve(cx, cy, radius, startAngle, endAngle, vertexCount, array3);
            CalcCurve(cx, cy, radius - width, startAngle, endAngle, vertexCount, array4);
            CalcCurve(cx, cy, radius - width - fadeWidth, startAngle, endAngle, vertexCount, array5);
            array[0] = array2[0];
            array[1] = array2[1];
            array6[0] = RGBAColor.transparentRGBA;
            for (int i = 1; i < vertexCount; i += 2)
            {
                array[(12 * i) - 10] = array2[i * 2];
                array[(12 * i) - 9] = array2[(i * 2) + 1];
                array[(12 * i) - 8] = array3[(i * 2) - 2];
                array[(12 * i) - 7] = array3[(i * 2) - 1];
                array[(12 * i) - 6] = array3[i * 2];
                array[(12 * i) - 5] = array3[(i * 2) + 1];
                array[(12 * i) - 4] = array4[(i * 2) - 2];
                array[(12 * i) - 3] = array4[(i * 2) - 1];
                array[(12 * i) - 2] = array4[i * 2];
                array[(12 * i) - 1] = array4[(i * 2) + 1];
                array[12 * i] = array5[(i * 2) - 2];
                array[(12 * i) + 1] = array5[(i * 2) - 1];
                array[(12 * i) + 2] = array5[(i * 2) + 2];
                array[(12 * i) + 3] = array5[(i * 2) + 3];
                array[(12 * i) + 4] = array4[i * 2];
                array[(12 * i) + 5] = array4[(i * 2) + 1];
                array[(12 * i) + 6] = array4[(i * 2) + 2];
                array[(12 * i) + 7] = array4[(i * 2) + 3];
                array[(12 * i) + 8] = array3[i * 2];
                array[(12 * i) + 9] = array3[(i * 2) + 1];
                array[(12 * i) + 10] = array3[(i * 2) + 2];
                array[(12 * i) + 11] = array3[(i * 2) + 3];
                array[(12 * i) + 12] = array2[i * 2];
                array[(12 * i) + 13] = array2[(i * 2) + 1];
                array6[(6 * i) - 5] = RGBAColor.transparentRGBA;
                array6[(6 * i) - 4] = fill;
                array6[(6 * i) - 3] = fill;
                array6[(6 * i) - 2] = fill;
                array6[(6 * i) - 1] = fill;
                array6[6 * i] = RGBAColor.transparentRGBA;
                array6[(6 * i) + 1] = RGBAColor.transparentRGBA;
                array6[(6 * i) + 2] = fill;
                array6[(6 * i) + 3] = fill;
                array6[(6 * i) + 4] = fill;
                array6[(6 * i) + 5] = fill;
                array6[(6 * i) + 6] = RGBAColor.transparentRGBA;
            }
            array[((vertexCount - 1) * 12) + 2] = array2[(vertexCount * 2) - 2];
            array[((vertexCount - 1) * 12) + 3] = array2[(vertexCount * 2) - 1];
            array6[((vertexCount - 1) * 6) + 1] = RGBAColor.transparentRGBA;
            int stripVertexCount = ((vertexCount - 1) * 6) + 2;
            VertexPositionColor[] vertices = BuildColoredVertices(array, array6, stripVertexCount);
            Renderer.DrawTriangleStrip(vertices, stripVertexCount);
        }

        private static void CalcCurve(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float[] glVertices)
        {
            float num7 = (endAngle - startAngle) / (vertexCount - 1);
            float num = Tanf(num7);
            float num2 = Cosf(num7);
            float num3 = radius * Cosf(startAngle);
            float num4 = radius * Sinf(startAngle);
            for (int i = 0; i < vertexCount; i++)
            {
                glVertices[i * 2] = num3 + cx;
                glVertices[(i * 2) + 1] = num4 + cy;
                float num5 = 0f - num4;
                float num6 = num3;
                num3 += num5 * num;
                num4 += num6 * num;
                num3 *= num2;
                num4 *= num2;
            }
        }

        public static VertexPositionColor[] BuildAntialiasedLineVertices(float x1, float y1, float x2, float y2, float size, RGBAColor color)
        {
            Vector v = Vect(x1, y1);
            Vector vector = VectSub(Vect(x2, y2), v);
            Vector v2 = VectPerp(vector);
            Vector vector2 = VectNormalize(v2);
            v2 = VectMult(vector2, size);
            Vector v3 = VectNeg(v2);
            Vector v4 = VectAdd(v2, vector);
            Vector v5 = VectAdd(VectNeg(v2), vector);
            v2 = VectAdd(v2, v);
            v3 = VectAdd(v3, v);
            v4 = VectAdd(v4, v);
            v5 = VectAdd(v5, v);
            Vector vector3 = VectSub(v2, vector2);
            Vector vector4 = VectSub(v4, vector2);
            Vector vector5 = VectAdd(v3, vector2);
            Vector vector6 = VectAdd(v5, vector2);
            VertexPositionColor[] vertices = GetVertexCache(ref s_antialiasedLineVerticesCache, 8);
            Color transparent = RGBAColor.transparentRGBA.ToXNA();
            Color lineColor = color.ToXNA();
            vertices[0] = new VertexPositionColor(new Vector3(v2.X, v2.Y, 0f), transparent);
            vertices[1] = new VertexPositionColor(new Vector3(v4.X, v4.Y, 0f), transparent);
            vertices[2] = new VertexPositionColor(new Vector3(vector3.X, vector3.Y, 0f), lineColor);
            vertices[3] = new VertexPositionColor(new Vector3(vector4.X, vector4.Y, 0f), lineColor);
            vertices[4] = new VertexPositionColor(new Vector3(vector5.X, vector5.Y, 0f), lineColor);
            vertices[5] = new VertexPositionColor(new Vector3(vector6.X, vector6.Y, 0f), lineColor);
            vertices[6] = new VertexPositionColor(new Vector3(v3.X, v3.Y, 0f), transparent);
            vertices[7] = new VertexPositionColor(new Vector3(v5.X, v5.Y, 0f), transparent);
            return vertices;
        }

        public static void DrawRect(float x, float y, float w, float h, RGBAColor color)
        {
            DrawPolygon(
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

        public static void DrawSolidRect(float x, float y, float w, float h, RGBAColor border, RGBAColor fill)
        {
            DrawSolidPolygon(
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

        /// <summary>
        /// Draws a solid filled rectangle without a border.
        /// </summary>
        /// <param name="x">X coordinate of the top-left corner.</param>
        /// <param name="y">Y coordinate of the top-left corner.</param>
        /// <param name="w">Width of the rectangle.</param>
        /// <param name="h">Height of the rectangle.</param>
        /// <param name="fill">Fill color of the rectangle.</param>
        public static void DrawSolidRectWOBorder(float x, float y, float w, float h, RGBAColor fill)
        {
            Color color = fill.ToXNA();
            s_rectVertices[0] = new VertexPositionColor(new Vector3(x, y, 0f), color);
            s_rectVertices[1] = new VertexPositionColor(new Vector3(x + w, y, 0f), color);
            s_rectVertices[2] = new VertexPositionColor(new Vector3(x, y + h, 0f), color);
            s_rectVertices[3] = new VertexPositionColor(new Vector3(x + w, y + h, 0f), color);
            Renderer.DrawTriangleStrip(s_rectVertices, 4);
        }

        // Cached vertex array for rectangle drawing
        private static readonly VertexPositionColor[] s_rectVertices = new VertexPositionColor[4];

        public static void DrawPolygon(float[] vertices, int vertexCount, RGBAColor color)
        {
            VertexPositionColor[] lineVertices = BuildClosedLineVertices(vertices, vertexCount, color.ToXNA());
            Renderer.DrawLineStrip(lineVertices, vertexCount + 1);
        }

        public static void DrawSolidPolygon(float[] vertices, int vertexCount, RGBAColor border, RGBAColor fill)
        {
            VertexPositionColor[] fillVertices = BuildColoredVertices(vertices, vertexCount, fill.ToXNA());
            Renderer.DrawTriangleStrip(fillVertices, vertexCount);
            VertexPositionColor[] lineVertices = BuildClosedLineVertices(vertices, vertexCount, border.ToXNA());
            Renderer.DrawLineStrip(lineVertices, vertexCount + 1);
        }

        public static void DrawSolidPolygonWOBorder(float[] vertices, int vertexCount, RGBAColor fill)
        {
            VertexPositionColor[] fillVertices = BuildColoredVertices(vertices, vertexCount, fill.ToXNA());
            Renderer.DrawTriangleStrip(fillVertices, vertexCount);
        }

        private static VertexPositionColor[] BuildColoredVertices(float[] positions, RGBAColor[] colors, int vertexCount)
        {
            VertexPositionColor[] vertices = GetVertexCache(ref s_coloredVerticesCache, vertexCount);
            int positionIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = new(positions[positionIndex++], positions[positionIndex++], 0f);
                vertices[i] = new VertexPositionColor(position, colors[i].ToXNA());
            }
            return vertices;
        }

        private static VertexPositionColor[] BuildColoredVertices(float[] positions, int vertexCount, Color color)
        {
            VertexPositionColor[] vertices = GetVertexCache(ref s_coloredVerticesCache, vertexCount);
            int positionIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = new(positions[positionIndex++], positions[positionIndex++], 0f);
                vertices[i] = new VertexPositionColor(position, color);
            }
            return vertices;
        }

        private static VertexPositionColor[] BuildClosedLineVertices(float[] positions, int vertexCount, Color color)
        {
            VertexPositionColor[] vertices = GetVertexCache(ref s_lineVerticesCache, vertexCount + 1);
            int positionIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = new(positions[positionIndex++], positions[positionIndex++], 0f);
                vertices[i] = new VertexPositionColor(position, color);
            }
            vertices[^1] = vertices[0];
            return vertices;
        }

        private static VertexPositionColor[] GetVertexCache(ref VertexPositionColor[] cache, int vertexCount)
        {
            if (cache == null || cache.Length < vertexCount)
            {
                cache = new VertexPositionColor[vertexCount];
            }
            return cache;
        }

        private static float[] GetFloatCache(ref float[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new float[length];
            }
            return cache;
        }

        private static RGBAColor[] GetColorCache(ref RGBAColor[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new RGBAColor[length];
            }
            return cache;
        }

        private static VertexPositionColor[] s_coloredVerticesCache;
        private static VertexPositionColor[] s_lineVerticesCache;
        private static VertexPositionColor[] s_antialiasedLineVerticesCache;
        private static float[] s_curveVerticesCache;
        private static float[] s_curveOuterCache;
        private static float[] s_curveInnerCache;
        private static float[] s_curveInnerEdgeCache;
        private static float[] s_curveInnerFadeCache;
        private static RGBAColor[] s_curveColorCache;

    }
}
