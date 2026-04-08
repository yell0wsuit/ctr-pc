using System;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Static utility methods for drawing textures, shapes, curves, and polygons.
    /// </summary>
    internal sealed class DrawHelper : FrameworkTypes
    {
        /// <summary>
        /// Draws the full texture at the specified position.
        /// </summary>
        /// <param name="image">Texture to draw.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public static void DrawImage(CTRTexture2D image, float x, float y)
        {
            CTRTexture2D.DrawAtPoint(image, Vect(x, y));
        }

        /// <summary>
        /// Draws a rectangular region of the texture at the specified position.
        /// </summary>
        /// <param name="image">Texture to draw from.</param>
        /// <param name="rect">Source rectangle within the texture.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public static void DrawImagePart(CTRTexture2D image, CTRRectangle rect, float x, float y)
        {
            CTRTexture2D.DrawRectAtPoint(image, rect, Vect(x, y));
        }

        /// <summary>
        /// Draws a specific quad from the texture, or the full <paramref name="image"/> if <paramref name="quadIndex"/> is -1.
        /// </summary>
        /// <param name="image">Texture to draw from.</param>
        /// <param name="quadIndex">Quad index, or -1 for full image.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public static void DrawImageQuad(CTRTexture2D image, int quadIndex, float x, float y)
        {
            if (quadIndex == -1)
            {
                DrawImage(image, x, y);
                return;
            }
            CTRTexture2D.DrawQuadAtPoint(image, quadIndex, Vect(x, y));
        }

        /// <summary>
        /// Draws a texture quad tiled to fill the specified area.
        /// </summary>
        /// <param name="image">Texture to tile.</param>
        /// <param name="quadIndex">Quad index, or -1 for full image.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width of the tiled area.</param>
        /// <param name="height">Height of the tiled area.</param>
        public static void DrawImageTiledCool(CTRTexture2D image, int quadIndex, float x, float y, float width, float height)
        {
            DrawImageTiledInternal(image, quadIndex, x, y, width, height, allowLegacyFallback: true);
        }

        /// <summary>
        /// Draws a texture quad tiled to fill the specified area.
        /// </summary>
        /// <param name="image">Texture to tile.</param>
        /// <param name="quadIndex">Quad index, or -1 for full image.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width of the tiled area.</param>
        /// <param name="height">Height of the tiled area.</param>
        public static void DrawImageTiled(CTRTexture2D image, int quadIndex, float x, float y, float width, float height)
        {
            DrawImageTiledInternal(image, quadIndex, x, y, width, height, allowLegacyFallback: true);
        }

        /// <summary>
        /// Internal tiled drawing implementation with optional legacy fallback.
        /// </summary>
        /// <param name="image">Texture to tile.</param>
        /// <param name="quadIndex">Quad index, or -1 for full image.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width of the tiled area.</param>
        /// <param name="height">Height of the tiled area.</param>
        /// <param name="allowLegacyFallback">Whether to use the per-tile fallback if batching is not possible.</param>
        private static void DrawImageTiledInternal(CTRTexture2D image, int quadIndex, float x, float y, float width, float height, bool allowLegacyFallback)
        {
            float texX = 0f;
            float texY = 0f;
            float tileWidth;
            float tileHeight;
            if (quadIndex == -1)
            {
                tileWidth = image._realWidth;
                tileHeight = image._realHeight;
            }
            else
            {
                texX = image.quadRects[quadIndex].x;
                texY = image.quadRects[quadIndex].y;
                tileWidth = image.quadRects[quadIndex].w;
                tileHeight = image.quadRects[quadIndex].h;
            }

            if (tileWidth <= 0f || tileHeight <= 0f || width <= 0f || height <= 0f)
            {
                return;
            }

            if (MathF.Abs(width - tileWidth) < 0.001f && MathF.Abs(height - tileHeight) < 0.001f)
            {
                DrawImageQuad(image, quadIndex, x, y);
                return;
            }

            if (!TryDrawImageTiledBatch(image, texX, texY, tileWidth, tileHeight, x, y, width, height) && allowLegacyFallback)
            {
                DrawImageTiledFallback(image, texX, texY, tileWidth, tileHeight, x, y, width, height);
            }
        }

        /// <summary>
        /// Attempts to draw tiled quads in a single batched draw call. Returns <see langword="false"/> if the batch is too large.
        /// </summary>
        /// <param name="image">Texture to tile.</param>
        /// <param name="texX">Source X offset in texture pixels.</param>
        /// <param name="texY">Source Y offset in texture pixels.</param>
        /// <param name="tileWidth">Tile width in pixels.</param>
        /// <param name="tileHeight">Tile height in pixels.</param>
        /// <param name="x">Destination X position.</param>
        /// <param name="y">Destination Y position.</param>
        /// <param name="width">Destination tiled width.</param>
        /// <param name="height">Destination tiled height.</param>
        /// <returns><see langword="true"/> when the tile batch was submitted; otherwise <see langword="false"/>.</returns>
        private static bool TryDrawImageTiledBatch(CTRTexture2D image, float texX, float texY, float tileWidth, float tileHeight, float x, float y, float width, float height)
        {
            int tileColumns = (int)MathF.Ceiling(width / tileWidth);
            int tileRows = (int)MathF.Ceiling(height / tileHeight);
            if (tileColumns <= 0 || tileRows <= 0)
            {
                return true;
            }

            int quadCount = tileColumns * tileRows;
            if (quadCount is <= 0 or > MaxBatchQuads)
            {
                return false;
            }

            int vertexCount = quadCount * 4;
            int indexCount = quadCount * 6;
            VertexPositionNormalTexture[] vertices = GetVertexCache(ref s_tiledVerticesCache, vertexCount);
            short[] indices = GetShortCache(ref s_tiledIndicesCache, indexCount);

            int v = 0;
            int i = 0;
            for (int row = 0; row < tileRows; row++)
            {
                float tileY = row * tileHeight;
                float drawY = y + tileY;
                float drawHeight = MathF.Min(tileHeight, height - tileY);
                float v1 = image._invHeight * texY;
                float v2 = image._invHeight * (texY + drawHeight);

                for (int col = 0; col < tileColumns; col++)
                {
                    float tileX = col * tileWidth;
                    float drawX = x + tileX;
                    float drawWidth = MathF.Min(tileWidth, width - tileX);
                    float u1 = image._invWidth * texX;
                    float u2 = image._invWidth * (texX + drawWidth);

                    vertices[v] = new VertexPositionNormalTexture(
                        new Vector3(drawX, drawY, 0f),
                        Vector3.UnitZ,
                        new Vector2(u1, v1));
                    vertices[v + 1] = new VertexPositionNormalTexture(
                        new Vector3(drawX + drawWidth, drawY, 0f),
                        Vector3.UnitZ,
                        new Vector2(u2, v1));
                    vertices[v + 2] = new VertexPositionNormalTexture(
                        new Vector3(drawX, drawY + drawHeight, 0f),
                        Vector3.UnitZ,
                        new Vector2(u1, v2));
                    vertices[v + 3] = new VertexPositionNormalTexture(
                        new Vector3(drawX + drawWidth, drawY + drawHeight, 0f),
                        Vector3.UnitZ,
                        new Vector2(u2, v2));

                    short baseIndex = (short)v;
                    indices[i] = baseIndex;
                    indices[i + 1] = (short)(baseIndex + 1);
                    indices[i + 2] = (short)(baseIndex + 2);
                    indices[i + 3] = (short)(baseIndex + 2);
                    indices[i + 4] = (short)(baseIndex + 1);
                    indices[i + 5] = (short)(baseIndex + 3);

                    v += 4;
                    i += 6;
                }
            }

            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(image.Name());
            Renderer.DrawTriangleList(vertices, indices, indexCount);
            return true;
        }

        /// <summary>
        /// Fallback tiled drawing using individual draw calls per tile.
        /// </summary>
        /// <param name="image">Texture to tile.</param>
        /// <param name="texX">Source X offset in texture pixels.</param>
        /// <param name="texY">Source Y offset in texture pixels.</param>
        /// <param name="tileWidth">Tile width in pixels.</param>
        /// <param name="tileHeight">Tile height in pixels.</param>
        /// <param name="x">Destination X position.</param>
        /// <param name="y">Destination Y position.</param>
        /// <param name="width">Destination tiled width.</param>
        /// <param name="height">Destination tiled height.</param>
        private static void DrawImageTiledFallback(CTRTexture2D image, float texX, float texY, float tileWidth, float tileHeight, float x, float y, float width, float height)
        {
            for (float currentY = 0f; currentY < height; currentY += tileHeight)
            {
                for (float currentX = 0f; currentX < width; currentX += tileWidth)
                {
                    float remainingWidth = width - currentX;
                    if (remainingWidth > tileWidth)
                    {
                        remainingWidth = tileWidth;
                    }
                    float remainingHeight = height - currentY;
                    if (remainingHeight > tileHeight)
                    {
                        remainingHeight = tileHeight;
                    }
                    CTRRectangle rect = MakeRectangle(texX, texY, remainingWidth, remainingHeight);
                    DrawImagePart(image, rect, x + currentX, y + currentY);
                }
            }
        }

        /// <summary>
        /// Computes UV <paramref name="texture"/> coordinates for the given source rectangle.
        /// </summary>
        /// <param name="texture">Texture to compute coordinates for.</param>
        /// <param name="rect">Source rectangle in pixel coordinates.</param>
        /// <returns>UV coordinates normalized to the texture size.</returns>
        public static Quad2D GetTextureCoordinates(CTRTexture2D texture, CTRRectangle rect)
        {
            return Quad2D.MakeQuad2D(
                texture._invWidth * rect.x,
                texture._invHeight * rect.y,
                texture._invWidth * rect.w,
                texture._invHeight * rect.h);
        }

        /// <summary>
        /// Recursively evaluates a multi-point Bezier curve at <paramref name="delta"/>.
        /// </summary>
        /// <param name="p">Control points.</param>
        /// <param name="count">Number of control points.</param>
        /// <param name="delta">Interpolation parameter (0–1).</param>
        /// <returns>The interpolated point on the curve.</returns>
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

        /// <summary>
        /// Linearly interpolates between two points at <paramref name="delta"/>.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <param name="delta">Interpolation parameter (0–1).</param>
        /// <returns>The interpolated point.</returns>
        public static Vector Calc2PointBezier(ref Vector a, ref Vector b, float delta)
        {
            float inverseDelta = 1f - delta;
            return new Vector
            {
                X = (a.X * inverseDelta) + (b.X * delta),
                Y = (a.Y * inverseDelta) + (b.Y * delta)
            };
        }

        /// <summary>
        /// Computes vertices for a circle approximation.
        /// </summary>
        /// <param name="x">Center X.</param>
        /// <param name="y">Center Y.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="vertexCount">Number of vertices.</param>
        /// <param name="glVertices">Output array of interleaved X/Y pairs.</param>
        public static void CalcCircle(float x, float y, float radius, int vertexCount, float[] glVertices)
        {
            float angleStep = MathF.Tau / vertexCount;
            float angle = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                glVertices[i * 2] = x + (radius * Cosf(angle));
                glVertices[(i * 2) + 1] = y + (radius * Sinf(angle));
                angle += angleStep;
            }
        }

        /// <summary>
        /// Draws the arc where two circles intersect.
        /// </summary>
        /// <param name="cx1">Center X of the first circle.</param>
        /// <param name="cy1">Center Y of the first circle.</param>
        /// <param name="radius1">Radius of the first circle.</param>
        /// <param name="cx2">Center X of the second circle.</param>
        /// <param name="cy2">Center Y of the second circle.</param>
        /// <param name="radius2">Radius of the second circle.</param>
        /// <param name="vertexCount">Number of vertices for the arc.</param>
        /// <param name="width">Line width.</param>
        /// <param name="fill">Fill color.</param>
        public static void DrawCircleIntersection(float cx1, float cy1, float radius1, float cx2, float cy2, float radius2, int vertexCount, float width, RGBAColor fill)
        {
            float centerDistance = VectDistance(Vect(cx1, cy1), Vect(cx2, cy2));
            if (centerDistance < radius1 + radius2 && radius1 < centerDistance + radius2)
            {
                float intersectionDistance = ((radius1 * radius1) - (radius2 * radius2) + (centerDistance * centerDistance))
                    / (2f * centerDistance);
                float angleOffset = Acosf((centerDistance - intersectionDistance) / radius2);
                float baseAngle = VectAngle(VectSub(Vect(cx1, cy1), Vect(cx2, cy2)));
                float startAngle = baseAngle - angleOffset;
                float endAngle = baseAngle + angleOffset;
                if (cx2 > cx1)
                {
                    startAngle += MathF.PI;
                    endAngle += MathF.PI;
                }
                DrawAntialiasedCurve2(cx2, cy2, radius2, startAngle, endAngle, vertexCount, width, 1f, fill);
            }
        }

        /// <summary>
        /// Draws an antialiased arc with inner/outer fade.
        /// </summary>
        /// <param name="cx">Center X.</param>
        /// <param name="cy">Center Y.</param>
        /// <param name="radius">Arc radius.</param>
        /// <param name="startAngle">Start angle in radians.</param>
        /// <param name="endAngle">End angle in radians.</param>
        /// <param name="vertexCount">Number of vertices for the arc.</param>
        /// <param name="width">Stroke width.</param>
        /// <param name="fadeWidth">Fade width for antialiasing.</param>
        /// <param name="fill">Stroke color.</param>
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

        /// <summary>
        /// Computes vertices for a circular arc using an incremental rotation approach.
        /// </summary>
        /// <param name="cx">Center X.</param>
        /// <param name="cy">Center Y.</param>
        /// <param name="radius">Arc radius.</param>
        /// <param name="startAngle">Arc start angle in radians.</param>
        /// <param name="endAngle">Arc end angle in radians.</param>
        /// <param name="vertexCount">Number of output vertices.</param>
        /// <param name="glVertices">Output array of interleaved X/Y pairs.</param>
        private static void CalcCurve(float cx, float cy, float radius, float startAngle, float endAngle, int vertexCount, float[] glVertices)
        {
            float angleStep = (endAngle - startAngle) / (vertexCount - 1);
            float tangentFactor = Tanf(angleStep);
            float cosineFactor = Cosf(angleStep);
            float currentX = radius * Cosf(startAngle);
            float currentY = radius * Sinf(startAngle);
            for (int i = 0; i < vertexCount; i++)
            {
                glVertices[i * 2] = currentX + cx;
                glVertices[(i * 2) + 1] = currentY + cy;
                float rotatedX = 0f - currentY;
                float rotatedY = currentX;
                currentX += rotatedX * tangentFactor;
                currentY += rotatedY * tangentFactor;
                currentX *= cosineFactor;
                currentY *= cosineFactor;
            }
        }

        /// <summary>
        /// Builds vertices for an antialiased line segment with fade-out edges.
        /// </summary>
        /// <param name="x1">Start X.</param>
        /// <param name="y1">Start Y.</param>
        /// <param name="x2">End X.</param>
        /// <param name="y2">End Y.</param>
        /// <param name="size">Half-width of the line.</param>
        /// <param name="color">Line color.</param>
        /// <returns>An 8-vertex strip suitable for antialiased line rendering.</returns>
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

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        /// <param name="color">Outline color.</param>
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

        /// <summary>
        /// Draws a filled rectangle with a <paramref name="border"/>.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        /// <param name="border">Border color.</param>
        /// <param name="fill">Fill color.</param>
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

        /// <summary>
        /// Cached vertex array for rectangle drawing.
        /// </summary>
        private static readonly VertexPositionColor[] s_rectVertices = new VertexPositionColor[4];

        /// <summary>
        /// Draws a polygon outline from interleaved X/Y vertex pairs.
        /// </summary>
        /// <param name="vertices">Interleaved X/Y vertex positions.</param>
        /// <param name="vertexCount">Number of vertices.</param>
        /// <param name="color">Outline color.</param>
        public static void DrawPolygon(float[] vertices, int vertexCount, RGBAColor color)
        {
            VertexPositionColor[] lineVertices = BuildClosedLineVertices(vertices, vertexCount, color.ToXNA());
            Renderer.DrawLineStrip(lineVertices, vertexCount + 1);
        }

        /// <summary>
        /// Draws a filled polygon with a <paramref name="border"/>.
        /// </summary>
        /// <param name="vertices">Interleaved X/Y vertex positions.</param>
        /// <param name="vertexCount">Number of vertices.</param>
        /// <param name="border">Border color.</param>
        /// <param name="fill">Fill color.</param>
        public static void DrawSolidPolygon(float[] vertices, int vertexCount, RGBAColor border, RGBAColor fill)
        {
            VertexPositionColor[] fillVertices = BuildColoredVertices(vertices, vertexCount, fill.ToXNA());
            Renderer.DrawTriangleStrip(fillVertices, vertexCount);
            VertexPositionColor[] lineVertices = BuildClosedLineVertices(vertices, vertexCount, border.ToXNA());
            Renderer.DrawLineStrip(lineVertices, vertexCount + 1);
        }

        /// <summary>
        /// Draws a filled polygon without a border.
        /// </summary>
        /// <param name="vertices">Interleaved X/Y vertex positions.</param>
        /// <param name="vertexCount">Number of vertices.</param>
        /// <param name="fill">Fill color.</param>
        public static void DrawSolidPolygonWOBorder(float[] vertices, int vertexCount, RGBAColor fill)
        {
            VertexPositionColor[] fillVertices = BuildColoredVertices(vertices, vertexCount, fill.ToXNA());
            Renderer.DrawTriangleStrip(fillVertices, vertexCount);
        }

        /// <summary>
        /// Builds colored vertices from <paramref name="positions"/> and per-vertex <paramref name="colors"/>.
        /// </summary>
        /// <param name="positions">Interleaved X/Y vertex positions.</param>
        /// <param name="colors">Per-vertex colors.</param>
        /// <param name="vertexCount">Number of vertices to build.</param>
        /// <returns>The cached vertex array containing the requested vertices.</returns>
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

        /// <summary>
        /// Builds colored vertices from <paramref name="positions"/> with a uniform <paramref name="color"/>.
        /// </summary>
        /// <param name="positions">Interleaved X/Y vertex positions.</param>
        /// <param name="vertexCount">Number of vertices to build.</param>
        /// <param name="color">Uniform color for all vertices.</param>
        /// <returns>The cached vertex array containing the requested vertices.</returns>
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

        /// <summary>
        /// Builds a closed line loop from <paramref name="positions"/> with a uniform <paramref name="color"/>.
        /// </summary>
        /// <param name="positions">Interleaved X/Y vertex positions.</param>
        /// <param name="vertexCount">Number of source vertices before closing the loop.</param>
        /// <param name="color">Uniform color for all vertices.</param>
        /// <returns>The cached vertex array with an extra closing vertex.</returns>
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

        /// <summary>
        /// Returns a cached <see cref="VertexPositionColor"/> array, resizing if needed.
        /// </summary>
        /// <param name="cache">Cache slot that stores the reusable array.</param>
        /// <param name="vertexCount">Minimum number of vertices required.</param>
        /// <returns>A cached array with at least <paramref name="vertexCount"/> elements.</returns>
        private static VertexPositionColor[] GetVertexCache(ref VertexPositionColor[] cache, int vertexCount)
        {
            if (cache == null || cache.Length < vertexCount)
            {
                cache = new VertexPositionColor[vertexCount];
            }
            return cache;
        }

        /// <summary>
        /// Returns a cached <see cref="VertexPositionNormalTexture"/> array, resizing if needed.
        /// </summary>
        /// <param name="cache">Cache slot that stores the reusable array.</param>
        /// <param name="vertexCount">Minimum number of vertices required.</param>
        /// <returns>A cached array with at least <paramref name="vertexCount"/> elements.</returns>
        private static VertexPositionNormalTexture[] GetVertexCache(ref VertexPositionNormalTexture[] cache, int vertexCount)
        {
            if (cache == null || cache.Length < vertexCount)
            {
                cache = new VertexPositionNormalTexture[vertexCount];
            }
            return cache;
        }

        /// <summary>
        /// Returns a cached float array, resizing if needed.
        /// </summary>
        /// <param name="cache">Cache slot that stores the reusable array.</param>
        /// <param name="length">Minimum required length.</param>
        /// <returns>A cached array with at least <paramref name="length"/> elements.</returns>
        private static float[] GetFloatCache(ref float[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new float[length];
            }
            return cache;
        }

        /// <summary>
        /// Returns a cached <see cref="RGBAColor"/> array, resizing if needed.
        /// </summary>
        /// <param name="cache">Cache slot that stores the reusable array.</param>
        /// <param name="length">Minimum required length.</param>
        /// <returns>A cached array with at least <paramref name="length"/> elements.</returns>
        private static RGBAColor[] GetColorCache(ref RGBAColor[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new RGBAColor[length];
            }
            return cache;
        }

        /// <summary>
        /// Returns a cached short array, resizing if needed.
        /// </summary>
        /// <param name="cache">Cache slot that stores the reusable array.</param>
        /// <param name="length">Minimum required length.</param>
        /// <returns>A cached array with at least <paramref name="length"/> elements.</returns>
        private static short[] GetShortCache(ref short[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new short[length];
            }
            return cache;
        }

        /// <summary>
        /// Draws a textured quad with radial (pie-chart) clipping, sweeping counterclockwise from 12 o'clock.
        /// Uses a triangle fan from the quad center with UV-mapped edge vertices.
        /// </summary>
        /// <param name="texture">The texture containing the quad.</param>
        /// <param name="quadIndex">Index of the quad in the texture atlas.</param>
        /// <param name="x">Draw X position (top-left).</param>
        /// <param name="y">Draw Y position (top-left).</param>
        /// <param name="fraction">Visible fraction (0 = invisible, 1 = fully visible).</param>
        public static void DrawRadialClippedQuad(CTRTexture2D texture, int quadIndex, float x, float y, float fraction)
        {
            if (fraction <= 0f)
            {
                return;
            }

            float w = texture.quadRects[quadIndex].w;
            float h = texture.quadRects[quadIndex].h;
            Quad2D quad = texture.quads[quadIndex];

            if (fraction >= 1f)
            {
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.BindTexture(texture.Name());
                VertexPositionNormalTexture[] fullQuad = QuadVertexCache.GetTexturedQuad(
                    x, y, w, h, quad.tlX, quad.tlY, quad.brX, quad.brY);
                Renderer.DrawTriangleStrip(fullQuad);
                return;
            }

            float hw = w / 2f;
            float hh = h / 2f;
            float cx = x + hw;
            float cy = y + hh;
            float sweepAngle = fraction * MathF.Tau;

            // Corner angles in sweep order from top (12 o'clock)
            float cornerAngle = MathF.Atan2(hw, hh);
            Span<float> corners =
            [
                cornerAngle,                   // top-left
                MathF.PI - cornerAngle,        // bottom-left
                MathF.PI + cornerAngle,        // bottom-right
                MathF.Tau - cornerAngle        // top-right
            ];

            // Build edge vertices: start at top center, add corners within sweep, end at sweep angle
            int edgeCount = 0;
            Span<float> edgeAngles = stackalloc float[6]; // start + 4 corners + end

            edgeAngles[edgeCount++] = 0f; // top center (start)

            for (int i = 0; i < 4; i++)
            {
                if (corners[i] < sweepAngle - 0.001f)
                {
                    edgeAngles[edgeCount++] = corners[i];
                }
            }

            edgeAngles[edgeCount++] = sweepAngle; // end

            // Build triangle fan: center + edge vertices
            int vertexCount = 1 + edgeCount;
            int triangleCount = edgeCount - 1;
            int indexCount = triangleCount * 3;

            VertexPositionNormalTexture[] vertices = GetVertexCache(ref s_radialVerticesCache, vertexCount);
            short[] indices = GetShortCache(ref s_radialIndicesCache, indexCount);

            // Center UV
            float texCenterU = (quad.tlX + quad.brX) / 2f;
            float texCenterV = (quad.tlY + quad.brY) / 2f;

            // Center vertex
            vertices[0] = new VertexPositionNormalTexture(
                new Vector3(cx, cy, 0f), Vector3.UnitZ, new Vector2(texCenterU, texCenterV));

            // Edge vertices
            for (int i = 0; i < edgeCount; i++)
            {
                float angle = edgeAngles[i];
                float dx = -MathF.Sin(angle);
                float dy = -MathF.Cos(angle);

                // Find intersection with quad boundary [-hw,hw] x [-hh,hh]
                float tx = dx != 0f ? ((dx > 0f ? hw : -hw) / dx) : float.MaxValue;
                float ty = dy != 0f ? ((dy < 0f ? -hh : hh) / dy) : float.MaxValue;
                float t = MathF.Min(MathF.Abs(tx), MathF.Abs(ty));

                float localX = t * dx;
                float localY = t * dy;

                // Map local position to UV
                float u = quad.tlX + ((localX + hw) / w * (quad.brX - quad.tlX));
                float v = quad.tlY + ((localY + hh) / h * (quad.brY - quad.tlY));

                vertices[1 + i] = new VertexPositionNormalTexture(
                    new Vector3(cx + localX, cy + localY, 0f), Vector3.UnitZ, new Vector2(u, v));
            }

            // Build triangle indices (fan from center)
            for (int i = 0; i < triangleCount; i++)
            {
                indices[i * 3] = 0;
                indices[(i * 3) + 1] = (short)(1 + i);
                indices[(i * 3) + 2] = (short)(2 + i);
            }

            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            Renderer.DrawTriangleList(vertices, indices, indexCount);
        }

        /// <summary>
        /// Cache for colored polygon/fill vertices.
        /// </summary>
        private static VertexPositionColor[] s_coloredVerticesCache;

        /// <summary>
        /// Cache for line strip vertices.
        /// </summary>
        private static VertexPositionColor[] s_lineVerticesCache;

        /// <summary>
        /// Cache for antialiased line vertices.
        /// </summary>
        private static VertexPositionColor[] s_antialiasedLineVerticesCache;

        /// <summary>
        /// Cache for tiled draw vertices.
        /// </summary>
        private static VertexPositionNormalTexture[] s_tiledVerticesCache;

        /// <summary>
        /// Cache for tiled draw indices.
        /// </summary>
        private static short[] s_tiledIndicesCache;

        /// <summary>
        /// Cache for radial clipped quad vertices.
        /// </summary>
        private static VertexPositionNormalTexture[] s_radialVerticesCache;

        /// <summary>
        /// Cache for radial clipped quad indices.
        /// </summary>
        private static short[] s_radialIndicesCache;

        /// <summary>
        /// Cache for curve strip vertex positions.
        /// </summary>
        private static float[] s_curveVerticesCache;

        /// <summary>
        /// Cache for outer curve positions.
        /// </summary>
        private static float[] s_curveOuterCache;

        /// <summary>
        /// Cache for inner curve positions.
        /// </summary>
        private static float[] s_curveInnerCache;

        /// <summary>
        /// Cache for inner edge curve positions.
        /// </summary>
        private static float[] s_curveInnerEdgeCache;

        /// <summary>
        /// Cache for inner fade curve positions.
        /// </summary>
        private static float[] s_curveInnerFadeCache;

        /// <summary>
        /// Cache for curve vertex colors.
        /// </summary>
        private static RGBAColor[] s_curveColorCache;

        /// <summary>
        /// Maximum number of quads in a single tiled batch.
        /// </summary>
        private const int MaxBatchQuads = short.MaxValue / 4;

    }
}
