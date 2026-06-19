using System;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Physics-backed rope used for bungee connections, rope cutting, and seasonal rope rendering.
    /// </summary>
    internal sealed class Bungee : ConstraintSystem
    {
        /// <summary>
        /// Draws a single antialiased line segment, continuing from the previous segment's edge vertices.
        /// </summary>
        /// <param name="x1">Start X coordinate.</param>
        /// <param name="y1">Start Y coordinate.</param>
        /// <param name="x2">End X coordinate.</param>
        /// <param name="y2">End Y coordinate.</param>
        /// <param name="size">Half-width of the line.</param>
        /// <param name="color">Line color.</param>
        /// <param name="lx">Left edge X from the previous segment; set to <c>-1</c> for the first segment.</param>
        /// <param name="ly">Left edge Y from the previous segment.</param>
        /// <param name="rx">Right edge X from the previous segment.</param>
        /// <param name="ry">Right edge Y from the previous segment.</param>
        /// <param name="highlighted">Whether to render with additive highlight blending.</param>
        private static void DrawAntialiasedLineContinued(float x1, float y1, float x2, float y2, float size, RGBAColor color, ref float lx, ref float ly, ref float rx, ref float ry, bool highlighted)
        {
            Vector v = Vect(x1, y1);
            Vector v2 = Vect(x2, y2);
            Vector vector = VectSub(v2, v);
            if (!VectEqual(vector, vectZero))
            {
                Vector v3 = highlighted ? vector : VectMult(vector, color.AlphaChannel == 1f ? 1.02f : 1f);
                Vector v4 = VectPerp(vector);
                Vector vector2 = VectNormalize(v4);
                v4 = VectMult(vector2, size);
                Vector v5 = VectNeg(v4);
                Vector v6 = VectAdd(v4, vector);
                Vector v7 = VectAdd(v5, vector);
                v6 = VectAdd(v6, v);
                v7 = VectAdd(v7, v);
                Vector v8 = VectAdd(v4, v3);
                Vector v9 = VectAdd(v5, v3);
                Vector vector3 = VectMult(vector2, size + 6f);
                Vector v10 = VectNeg(vector3);
                Vector v11 = VectAdd(vector3, vector);
                Vector v12 = VectAdd(v10, vector);
                vector3 = VectAdd(vector3, v);
                v10 = VectAdd(v10, v);
                v11 = VectAdd(v11, v);
                v12 = VectAdd(v12, v);
                if (lx == -1f)
                {
                    v4 = VectAdd(v4, v);
                    v5 = VectAdd(v5, v);
                }
                else
                {
                    v4 = Vect(lx, ly);
                    v5 = Vect(rx, ry);
                }
                v8 = VectAdd(v8, v);
                v9 = VectAdd(v9, v);
                lx = v6.X;
                ly = v6.Y;
                rx = v7.X;
                ry = v7.Y;
                Vector vector4 = VectSub(v4, vector2);
                Vector vector5 = VectSub(v8, vector2);
                Vector vector6 = VectAdd(v5, vector2);
                Vector vector7 = VectAdd(v9, vector2);
                float[] pointer = GetFloatCache(ref s_bungeePointerCache, 16);
                int pointerIndex = 0;
                WritePair(pointer, ref pointerIndex, vector3);
                WritePair(pointer, ref pointerIndex, v11);
                WritePair(pointer, ref pointerIndex, v4);
                WritePair(pointer, ref pointerIndex, v8);
                WritePair(pointer, ref pointerIndex, v5);
                WritePair(pointer, ref pointerIndex, v9);
                WritePair(pointer, ref pointerIndex, v10);
                WritePair(pointer, ref pointerIndex, v12);
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                whiteRGBA.AlphaChannel = 0.1f * color.AlphaChannel;
                ccolors[2] = whiteRGBA;
                ccolors[3] = whiteRGBA;
                ccolors[4] = whiteRGBA;
                ccolors[5] = whiteRGBA;
                float[] pointer2 = GetFloatCache(ref s_bungeePointerCache2, 20);
                int pointer2Index = 0;
                WritePair(pointer2, ref pointer2Index, v4);
                WritePair(pointer2, ref pointer2Index, v8);
                WritePair(pointer2, ref pointer2Index, vector4);
                WritePair(pointer2, ref pointer2Index, vector5);
                WritePair(pointer2, ref pointer2Index, v);
                WritePair(pointer2, ref pointer2Index, v2);
                WritePair(pointer2, ref pointer2Index, vector6);
                WritePair(pointer2, ref pointer2Index, vector7);
                WritePair(pointer2, ref pointer2Index, v5);
                WritePair(pointer2, ref pointer2Index, v9);
                RGBAColor rgbaColor = color;
                float highlightAdditive = 0.15f * color.AlphaChannel;
                color.RedColor += highlightAdditive;
                color.GreenColor += highlightAdditive;
                color.BlueColor += highlightAdditive;
                ccolors2[2] = color;
                ccolors2[3] = color;
                ccolors2[4] = rgbaColor;
                ccolors2[5] = rgbaColor;
                ccolors2[6] = color;
                ccolors2[7] = color;
                if (highlighted)
                {
                    Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                    VertexPositionColor[] highlightVertices = BuildColoredVertices(pointer, ccolors, 8);
                    Renderer.DrawTriangleStrip(highlightVertices, 8);
                }
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                VertexPositionColor[] mainVertices = BuildColoredVertices(pointer2, ccolors2, 10);
                Renderer.DrawTriangleStrip(mainVertices, 10);
            }
        }

        /// <summary>
        /// Builds an array of colored vertices from parallel position and color arrays.
        /// </summary>
        /// <param name="positions">Flat array of X/Y coordinate pairs.</param>
        /// <param name="colors">Per-vertex colors.</param>
        /// <param name="vertexCount">Number of vertices to build.</param>
        /// <returns>The populated vertex array.</returns>
        private static VertexPositionColor[] BuildColoredVertices(float[] positions, RGBAColor[] colors, int vertexCount)
        {
            VertexPositionColor[] vertices = GetVertexCache(ref s_bungeeVerticesCache, vertexCount);
            int positionIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = new(positions[positionIndex++], positions[positionIndex++], 0f);
                vertices[i] = new VertexPositionColor(position, colors[i].ToXNA());
            }
            return vertices;
        }

        /// <summary>
        /// Returns a cached vertex array, reallocating if the cache is too small.
        /// </summary>
        /// <param name="cache">Reference to the cached array.</param>
        /// <param name="vertexCount">Minimum required capacity.</param>
        /// <returns>The cached or newly allocated array.</returns>
        private static VertexPositionColor[] GetVertexCache(ref VertexPositionColor[] cache, int vertexCount)
        {
            if (cache == null || cache.Length < vertexCount)
            {
                cache = new VertexPositionColor[vertexCount];
            }
            return cache;
        }

        /// <summary>
        /// Returns a cached float array, reallocating if the cache is too small.
        /// </summary>
        /// <param name="cache">Reference to the cached array.</param>
        /// <param name="length">Minimum required capacity.</param>
        /// <returns>The cached or newly allocated array.</returns>
        private static float[] GetFloatCache(ref float[] cache, int length)
        {
            if (cache == null || cache.Length < length)
            {
                cache = new float[length];
            }
            return cache;
        }

        /// <summary>
        /// Writes a vector's X and Y components into a float buffer at the current index.
        /// </summary>
        /// <param name="buffer">The target float array.</param>
        /// <param name="index">Write position; advanced by 2 after the call.</param>
        /// <param name="v">The vector to write.</param>
        private static void WritePair(float[] buffer, ref int index, Vector v)
        {
            buffer[index++] = v.X;
            buffer[index++] = v.Y;
        }

        /// <summary>
        /// Draws an entire bungee rope by sampling a bezier curve through the given constraint points.
        /// </summary>
        /// <param name="b">The bungee instance being drawn.</param>
        /// <param name="pts">Array of constraint point positions along the rope.</param>
        /// <param name="count">Number of valid points in <paramref name="pts"/>.</param>
        /// <param name="points">Number of bezier sample points per segment.</param>
        private static void DrawBungee(Bungee b, Vector[] pts, int count, int points)
        {
            float alphaMultiplier = GetCutFadeAlpha(b);
            float stretchRedThreshold = ActivePhysicsConstants.BungeeStretchRedThreshold;
            float segmentLength = VectDistance(Vect(pts[0].X, pts[0].Y), Vect(pts[1].X, pts[1].Y));

            // Get selected rope colors from preferences
            int selectedRopeIndex = Preferences.GetIntForKey("PREFS_SELECTED_ROPE");
            RopeColorHelper.RopeDrawColors drawColors = RopeColorHelper.GetDrawColors(
                selectedRopeIndex,
                alphaMultiplier,
                b.highlighted,
                segmentLength,
                BUNGEE_REST_LEN,
                stretchRedThreshold);
            RGBAColor rgbaColor = drawColors.BaseColor1;
            RGBAColor rgbaColor2 = drawColors.BaseColor2;
            RGBAColor rgbaColor3 = drawColors.ShadeColor1;
            RGBAColor rgbaColor4 = drawColors.ShadeColor2;

            float relaxThresholdSoft = ActivePhysicsConstants.BungeeRelaxThresholdSoft;
            float relaxThresholdMedium = ActivePhysicsConstants.BungeeRelaxThresholdMedium;
            float relaxThresholdHard = ActivePhysicsConstants.BungeeRelaxThresholdHard;
            b.relaxed = segmentLength <= BUNGEE_REST_LEN + relaxThresholdSoft
                ? 0
                : segmentLength <= BUNGEE_REST_LEN + relaxThresholdMedium
                    ? 1
                    : segmentLength <= BUNGEE_REST_LEN + relaxThresholdHard ? 2 : 3;
            bool flag = false;
            int sampleCount = (count - 1) * points;
            float[] array = new float[sampleCount * 2];
            b.drawPtsCount = sampleCount * 2;
            float sampleStep = 1f / sampleCount;

            // Draw outline for non-default rope skins
            if (selectedRopeIndex >= 1 && count >= 3 && sampleCount > 0)
            {
                int outlineMaxPts = sampleCount + 2;
                float[] outlinePts = new float[outlineMaxPts * 2];
                int outlinePtCount = 0;
                float outlineT = 0f;
                for (; ; )
                {
                    if (outlineT > 1)
                    {
                        outlineT = 1f;
                    }
                    if (outlinePtCount + 2 > outlinePts.Length)
                    {
                        break;
                    }
                    Vector v = DrawHelper.CalcPathBezier(pts, count, outlineT);
                    outlinePts[outlinePtCount++] = v.X;
                    outlinePts[outlinePtCount++] = v.Y;
                    if (outlineT >= 1f)
                    {
                        break;
                    }
                    outlineT += sampleStep;
                }
                float olx = -1f, oly = -1f, orx = -1f, ory = -1f;
                RGBAColor outlineColor = RGBAColor.MakeRGBA(0, 0, 0, 0.4f * alphaMultiplier);
                Renderer.SetColor(outlineColor.ToXNA());
                int ptCount = outlinePtCount / 2;
                for (int i = 0; i < ptCount - 1; i++)
                {
                    int idx = i * 2;
                    DrawAntialiasedLineContinued(outlinePts[idx], outlinePts[idx + 1], outlinePts[idx + 2], outlinePts[idx + 3], 7f, outlineColor, ref olx, ref oly, ref orx, ref ory, false);
                }
            }

            float bezierT = 0f;
            int cachedPointCount = 0;
            int drawPointCount = 0;
            RGBAColor rgbaColor5 = rgbaColor3;
            RGBAColor rgbaColor6 = rgbaColor4;
            float redStep = (rgbaColor.RedColor - rgbaColor3.RedColor) / (sampleCount - 1);
            float greenStep = (rgbaColor.GreenColor - rgbaColor3.GreenColor) / (sampleCount - 1);
            float blueStep = (rgbaColor.BlueColor - rgbaColor3.BlueColor) / (sampleCount - 1);
            float redStepAlt = (rgbaColor2.RedColor - rgbaColor4.RedColor) / (sampleCount - 1);
            float greenStepAlt = (rgbaColor2.GreenColor - rgbaColor4.GreenColor) / (sampleCount - 1);
            float blueStepAlt = (rgbaColor2.BlueColor - rgbaColor4.BlueColor) / (sampleCount - 1);
            float lx = -1f;
            float ly = -1f;
            float rx = -1f;
            float ry = -1f;
            for (; ; )
            {
                if (bezierT > 1)
                {
                    bezierT = 1f;
                }
                if (count < 3)
                {
                    break;
                }
                Vector vector = DrawHelper.CalcPathBezier(pts, count, bezierT);
                array[cachedPointCount++] = vector.X;
                array[cachedPointCount++] = vector.Y;
                b.drawPts[drawPointCount++] = vector.X;
                b.drawPts[drawPointCount++] = vector.Y;
                if (cachedPointCount >= 8 || bezierT == 1)
                {
                    RGBAColor color = b.forceWhite ? RGBAColor.whiteRGBA : !flag ? rgbaColor6 : rgbaColor5;
                    Renderer.SetColor(color.ToXNA());
                    int segmentCount = cachedPointCount >> 1;
                    for (int i = 0; i < segmentCount - 1; i++)
                    {
                        DrawAntialiasedLineContinued(array[i * 2], array[(i * 2) + 1], array[(i * 2) + 2], array[(i * 2) + 3], 5f, color, ref lx, ref ly, ref rx, ref ry, b.highlighted);
                    }
                    array[0] = array[cachedPointCount - 2];
                    array[1] = array[cachedPointCount - 1];
                    cachedPointCount = 2;
                    flag = !flag;
                    rgbaColor5.RedColor += redStep * (segmentCount - 1);
                    rgbaColor5.GreenColor += greenStep * (segmentCount - 1);
                    rgbaColor5.BlueColor += blueStep * (segmentCount - 1);
                    rgbaColor6.RedColor += redStepAlt * (segmentCount - 1);
                    rgbaColor6.GreenColor += greenStepAlt * (segmentCount - 1);
                    rgbaColor6.BlueColor += blueStepAlt * (segmentCount - 1);
                }
                if (bezierT == 1)
                {
                    break;
                }
                bezierT += sampleStep;
            }

            b.drawPtsCount = drawPointCount;
            b.DrawChristmasLights(alphaMultiplier);
        }

        /// <summary>
        /// Returns the fade alpha for cut rope and chain draw paths.
        /// </summary>
        /// <param name="b">The bungee instance being drawn.</param>
        /// <returns>Opaque before cutting or during the force-white cut frame, then fades by remaining cut time.</returns>
        internal static float GetCutFadeAlpha(Bungee b)
        {
            return b.cut == -1 || b.forceWhite ? 1f : b.cutTime / 1.95f;
        }

        /// <summary>
        /// Returns the straight-alpha blend factors used while fading chain textures.
        /// </summary>
        /// <returns>Source and destination blend factors for chain texture fade.</returns>
        internal static (BlendingFactor Source, BlendingFactor Destination) GetChainFadeBlendFactors()
        {
            return (BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Builds white per-vertex colors for chain sprites with alpha fade applied only to alpha.
        /// </summary>
        /// <param name="spriteCount">Number of chain sprites.</param>
        /// <param name="alpha">Alpha to apply to each vertex.</param>
        /// <returns>Four colors per sprite.</returns>
        internal static RGBAColor[] BuildChainSpriteColors(int spriteCount, float alpha)
        {
            RGBAColor[] colors = new RGBAColor[spriteCount * 4];
            RGBAColor color = RGBAColor.whiteRGBA;
            color.AlphaChannel = alpha;
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            return colors;
        }

        /// <summary>
        /// Builds the chain sprite layout: one sprite at each sampled curve point
        /// and a second sprite centered between adjacent samples.
        /// </summary>
        /// <param name="pts">Control points along the chain.</param>
        /// <param name="count">Number of valid control points.</param>
        /// <param name="points">Number of bezier samples per control-point segment.</param>
        /// <param name="pointSpriteSize">Size of the sprite drawn at sampled points.</param>
        /// <param name="midpointSpriteSize">Size of the sprite drawn between sampled points.</param>
        /// <returns>The chain sprites to submit in draw order.</returns>
        internal static ChainSprite[] BuildChainSpritePlan(Vector[] pts, int count, int points, Vector pointSpriteSize, Vector midpointSpriteSize)
        {
            int sampleCount = (count - 1) * points;
            if (pts == null || count < 2 || sampleCount <= 0)
            {
                return [];
            }

            Vector[] samples = new Vector[sampleCount];
            float bezierT = 0f;
            float sampleStep = 1f / sampleCount;
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = DrawHelper.CalcPathBezier(pts, count, bezierT);
                bezierT += sampleStep;
            }

            ChainSprite[] sprites = new ChainSprite[sampleCount + Math.Max(0, sampleCount - 1)];
            int spriteIndex = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                float angle = i == 0 ? 0f : GetChainAngle(samples[i - 1], samples[i]);
                sprites[spriteIndex++] = new ChainSprite(ChainPointQuad, samples[i], angle, CreateCenteredRotatedQuad(samples[i], pointSpriteSize, angle));
            }
            for (int i = 0; i < sampleCount - 1; i++)
            {
                Vector center = Vect(
                    samples[i].X + ((samples[i + 1].X - samples[i].X) * 0.5f),
                    samples[i].Y + ((samples[i + 1].Y - samples[i].Y) * 0.5f));
                float angle = GetChainAngle(samples[i], samples[i + 1]);
                sprites[spriteIndex++] = new ChainSprite(ChainMidpointQuad, center, angle, CreateCenteredRotatedQuad(center, midpointSpriteSize, angle));
            }
            return sprites;
        }

        /// <summary>
        /// Draws a chain bungee using the two separate chain sprites.
        /// </summary>
        /// <param name="b">The bungee instance being drawn.</param>
        /// <param name="pts">Control points along the chain.</param>
        /// <param name="count">Number of valid control points.</param>
        /// <param name="points">Number of bezier samples per control-point segment.</param>
        private static void DrawChain(Bungee b, Vector[] pts, int count, int points)
        {
            CTRTexture2D texture = Application.GetTexture(Resources.Img.ObjExpChain);
            if (texture?.quadRects == null || texture.quads == null || texture.quadsCount < 2)
            {
                DrawBungee(b, pts, count, points);
                return;
            }

            ChainSprite[] sprites = BuildChainSpritePlan(
                pts,
                count,
                points,
                Vect(texture.quadRects[ChainPointQuad].w, texture.quadRects[ChainPointQuad].h),
                Vect(texture.quadRects[ChainMidpointQuad].w, texture.quadRects[ChainMidpointQuad].h));
            if (sprites.Length == 0)
            {
                return;
            }

            Quad3D[] vertices = new Quad3D[sprites.Length];
            Quad2D[] texCoordinates = new Quad2D[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                ChainSprite sprite = sprites[i];
                vertices[i] = sprite.VertexQuad;
                texCoordinates[i] = texture.quads[sprite.QuadIndex];
            }

            VertexPositionColorTexture[] vertexBuffer = new VertexPositionColorTexture[sprites.Length * 4];
            short[] indices = BuildQuadIndices(sprites.Length);
            RGBAColor[] colors = BuildChainSpriteColors(sprites.Length, GetCutFadeAlpha(b));
            Renderer.FillTexturedColoredVertices(vertices, texCoordinates, colors, vertexBuffer, sprites.Length);

            Renderer.SetColor(RGBAColor.whiteRGBA.ToXNA());
            (BlendingFactor source, BlendingFactor destination) = GetChainFadeBlendFactors();
            Renderer.SetBlendFunc(source, destination);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            Renderer.DrawTriangleList(vertexBuffer, indices, indices.Length);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Builds triangle-list indices for a sequence of quads.
        /// </summary>
        /// <param name="quadCount">Number of quads.</param>
        /// <returns>Six indices per quad.</returns>
        private static short[] BuildQuadIndices(int quadCount)
        {
            short[] indices = new short[quadCount * 6];
            for (int i = 0; i < quadCount; i++)
            {
                indices[i * 6] = (short)(i * 4);
                indices[(i * 6) + 1] = (short)((i * 4) + 1);
                indices[(i * 6) + 2] = (short)((i * 4) + 2);
                indices[(i * 6) + 3] = (short)((i * 4) + 3);
                indices[(i * 6) + 4] = (short)((i * 4) + 2);
                indices[(i * 6) + 5] = (short)((i * 4) + 1);
            }
            return indices;
        }

        /// <summary>
        /// Returns the sprite angle used by the original chain renderer for a segment.
        /// </summary>
        /// <param name="previous">Previous sample point.</param>
        /// <param name="current">Current sample point.</param>
        /// <returns>Rotation in radians.</returns>
        private static float GetChainAngle(Vector previous, Vector current)
        {
            return MathF.Atan2(previous.Y - current.Y, previous.X - current.X) + (MathF.PI / 2f);
        }

        /// <summary>
        /// Creates a rotated vertex quad centered on <paramref name="center"/>.
        /// </summary>
        /// <param name="center">Sprite center in world coordinates.</param>
        /// <param name="size">Sprite size.</param>
        /// <param name="angle">Rotation in radians.</param>
        /// <returns>The rotated quad vertices.</returns>
        private static Quad3D CreateCenteredRotatedQuad(Vector center, Vector size, float angle)
        {
            float halfWidth = size.X * 0.5f;
            float halfHeight = size.Y * 0.5f;
            Vector bl = Vect(center.X - halfWidth, center.Y - halfHeight);
            Vector br = Vect(center.X + halfWidth, center.Y - halfHeight);
            Vector tl = Vect(center.X - halfWidth, center.Y + halfHeight);
            Vector tr = Vect(center.X + halfWidth, center.Y + halfHeight);
            if (angle != 0f)
            {
                bl = RotateAround(bl, angle, center);
                br = RotateAround(br, angle, center);
                tl = RotateAround(tl, angle, center);
                tr = RotateAround(tr, angle, center);
            }
            return Quad3D.MakeQuad3DEx(bl.X, bl.Y, br.X, br.Y, tl.X, tl.Y, tr.X, tr.Y);
        }

        /// <summary>
        /// Rotates <paramref name="point"/> around <paramref name="center"/> without relying on global math lookup tables.
        /// </summary>
        /// <param name="point">Point to rotate.</param>
        /// <param name="angle">Rotation in radians.</param>
        /// <param name="center">Rotation center.</param>
        /// <returns>The rotated point.</returns>
        private static Vector RotateAround(Vector point, float angle, Vector center)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return Vect(
                center.X + (dx * cos) - (dy * sin),
                center.Y + (dx * sin) + (dy * cos));
        }

        /// <summary>
        /// Initializes a bungee rope between head and tail constraint points.
        /// </summary>
        /// <param name="h">Optional existing head constraint point; a new anchor is created when this is <see langword="null"/>.</param>
        /// <param name="hx">Initial head X position.</param>
        /// <param name="hy">Initial head Y position.</param>
        /// <param name="t">Optional existing tail constraint point; a new tail is created when this is <see langword="null"/>.</param>
        /// <param name="tx">Initial tail X position.</param>
        /// <param name="ty">Initial tail Y position.</param>
        /// <param name="len">Initial rope length used to roll out intermediate rope segments.</param>
        /// <returns>The initialized bungee instance.</returns>
        public Bungee InitWithHeadAtXYTailAtTXTYandLength(ConstraintedPoint h, float hx, float hy, ConstraintedPoint t, float tx, float ty, float len)
        {
            relaxationTimes = 30;
            lineWidth = 10f;
            cut = -1;
            bungeeMode = 0;
            highlighted = false;
            bungeeAnchor = h ?? new ConstraintedPoint();
            ownsAnchor = h == null;
            if (t != null)
            {
                tail = t;
                ownsTail = false;
            }
            else
            {
                tail = new ConstraintedPoint();
                tail.SetWeight(1f);
                ownsTail = true;
            }
            if (ownsAnchor)
            {
                bungeeAnchor.SetWeight(0.02f);
            }
            bungeeAnchor.pos = Vect(hx, hy);
            tail.pos = Vect(tx, ty);
            AddPart(bungeeAnchor);
            AddPart(tail);
            tail.AddConstraintwithRestLengthofType(bungeeAnchor, BUNGEE_REST_LEN, Constraint.CONSTRAINT.DISTANCE);
            Vector v = VectSub(tail.pos, bungeeAnchor.pos);
            int subdivisionCount = (int)((len / BUNGEE_REST_LEN) + 2f);
            v = VectDiv(v, subdivisionCount);
            RollplacingWithOffset(len, v);
            forceWhite = false;
            initialCandleAngle = -1f;
            chosenOne = false;
            hideTailParts = false;
            breakable = false;
            return this;
        }

        /// <summary>
        /// Marks this bungee to render as a chain.
        /// </summary>
        public void SetUnBreakable()
        {
            breakable = true;
        }

        /// <summary>
        /// Calculates the current polyline length across all bungee constraint points.
        /// </summary>
        /// <returns>The approximate current bungee length in world units.</returns>
        public int GetLength()
        {
            int totalLength = 0;
            Vector pos = vectZero;
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                if (i > 0)
                {
                    totalLength += (int)VectDistance(pos, constraintedPoint.pos);
                }
                pos = constraintedPoint.pos;
            }
            return totalLength;
        }

        /// <summary>
        /// Rolls additional rope length into the bungee without applying a placement offset.
        /// </summary>
        /// <param name="rollLen">Amount of rope length to add.</param>
        public void Roll(float rollLen)
        {
            RollplacingWithOffset(rollLen, vectZero);
        }

        /// <summary>
        /// Rolls additional rope length into the bungee and places new segments using an offset.
        /// </summary>
        /// <param name="rollLen">Amount of rope length to add.</param>
        /// <param name="off">Offset applied when placing new intermediate constraint points.</param>
        public void RollplacingWithOffset(float rollLen, Vector off)
        {
            ConstraintedPoint i = parts[^2];
            int tailRestLength = (int)tail.RestLengthFor(i);
            while (rollLen > 0f)
            {
                if (rollLen >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint constraintedPoint = parts[^2];
                    ConstraintedPoint constraintedPoint2 = new();
                    constraintedPoint2.SetWeight(0.02f);
                    constraintedPoint2.pos = VectAdd(constraintedPoint.pos, off);
                    AddPartAt(constraintedPoint2, parts.Count - 1);
                    tail.ChangeConstraintFromTowithRestLength(constraintedPoint, constraintedPoint2, tailRestLength);
                    constraintedPoint2.AddConstraintwithRestLengthofType(constraintedPoint, BUNGEE_REST_LEN, Constraint.CONSTRAINT.DISTANCE);
                    rollLen -= BUNGEE_REST_LEN;
                }
                else
                {
                    int newRestLength = (int)(rollLen + tailRestLength);
                    if (newRestLength > BUNGEE_REST_LEN)
                    {
                        rollLen = BUNGEE_REST_LEN;
                        tailRestLength = (int)(newRestLength - BUNGEE_REST_LEN);
                    }
                    else
                    {
                        ConstraintedPoint n2 = parts[^2];
                        tail.ChangeRestLengthToFor(newRestLength, n2);
                        rollLen = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Removes rope length from the tail side of the bungee.
        /// </summary>
        /// <param name="amount">Amount of rope length to remove.</param>
        /// <returns>Remaining amount that could not be removed.</returns>
        public float RollBack(float amount)
        {
            float remainingAmount = amount;
            ConstraintedPoint i = parts[^2];
            int currentRestLength = (int)tail.RestLengthFor(i);
            int partCount = parts.Count;
            while (remainingAmount > 0f)
            {
                if (remainingAmount >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint o = parts[partCount - 2];
                    ConstraintedPoint n2 = parts[partCount - 3];
                    tail.ChangeConstraintFromTowithRestLength(o, n2, currentRestLength);
                    parts.RemoveAt(parts.Count - 2);
                    partCount--;
                    remainingAmount -= BUNGEE_REST_LEN;
                }
                else
                {
                    int nextRestLength = (int)(currentRestLength - remainingAmount);
                    if (nextRestLength < 1)
                    {
                        remainingAmount = BUNGEE_REST_LEN;
                        currentRestLength = (int)(BUNGEE_REST_LEN + nextRestLength + ActivePhysicsConstants.BungeeRollBackOverflowPadding);
                    }
                    else
                    {
                        ConstraintedPoint n3 = parts[partCount - 2];
                        tail.ChangeRestLengthToFor(nextRestLength, n3);
                        remainingAmount = 0f;
                    }
                }
            }
            int count = tail.constraints.Count;
            for (int j = 0; j < count; j++)
            {
                Constraint constraint = tail.constraints[j];
                if (constraint != null && constraint.type == Constraint.CONSTRAINT.NOT_MORE_THAN)
                {
                    constraint.restLength = (partCount - 1) * (BUNGEE_REST_LEN + ActivePhysicsConstants.BungeeConstraintSlack);
                }
            }
            return remainingAmount;
        }

        /// <summary>
        /// Removes or detaches a bungee segment and weakens the remaining free points.
        /// </summary>
        /// <param name="part">Index of the segment part to remove.</param>
        public void RemovePart(int part)
        {
            forceWhite = false;
            ConstraintedPoint constraintedPoint = parts[part];
            ConstraintedPoint constraintedPoint2 = part + 1 >= parts.Count ? null : parts[part + 1];
            if (constraintedPoint2 == null)
            {
                constraintedPoint.RemoveConstraints();
            }
            else
            {
                for (int i = 0; i < constraintedPoint2.constraints.Count; i++)
                {
                    Constraint constraint = constraintedPoint2.constraints[i];
                    if (constraint.cp == constraintedPoint)
                    {
                        _ = constraintedPoint2.constraints.Remove(constraint);
                        ConstraintedPoint constraintedPoint3 = new();
                        constraintedPoint3.SetWeight(1E-05f);
                        constraintedPoint3.pos = constraintedPoint2.pos;
                        constraintedPoint3.prevPos = constraintedPoint2.prevPos;
                        AddPartAt(constraintedPoint3, part + 1);
                        constraintedPoint3.AddConstraintwithRestLengthofType(constraintedPoint, BUNGEE_REST_LEN, Constraint.CONSTRAINT.DISTANCE);
                        break;
                    }
                }
            }
            for (int j = 0; j < parts.Count; j++)
            {
                ConstraintedPoint constraintedPoint4 = parts[j];
                // Don't weaken an endpoint the rope doesn't own: tail is always external, and a
                // non-owned head (a candy point in a candiesConnected link) must keep its mass.
                // Owned anchors (normal/kicked grabs) still go limp, as before.
                if (constraintedPoint4 != tail && (constraintedPoint4 != bungeeAnchor || ownsAnchor))
                {
                    constraintedPoint4.SetWeight(1E-05f);
                }
            }
        }

        /// <summary>
        /// Marks the bungee as cut at a segment index and starts the cut fade.
        /// </summary>
        /// <param name="part">Index of the segment part where the bungee was cut.</param>
        public void SetCut(int part)
        {
            cut = part;
            cutTime = 2f;
            forceWhite = true;
        }

        /// <summary>
        /// Adds constraints that keep rope segments closer to the pinned anchor.
        /// </summary>
        public void Strengthen()
        {
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                if (constraintedPoint != null)
                {
                    if (bungeeAnchor.pin.X != -1f)
                    {
                        if (constraintedPoint != tail)
                        {
                            constraintedPoint.SetWeight(0.5f);
                        }
                        if (i != 0)
                        {
                            constraintedPoint.AddConstraintwithRestLengthofType(bungeeAnchor, i * (BUNGEE_REST_LEN + ActivePhysicsConstants.BungeeConstraintSlack), Constraint.CONSTRAINT.NOT_MORE_THAN);
                        }
                    }
                    i++;
                }
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            Update(delta, 1f);
        }

        /// <summary>
        /// Updates bungee physics with a custom Verlet integration coefficient.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        /// <param name="koeff">Coefficient passed to constraint-point physics updates.</param>
        public void Update(float delta, float koeff)
        {
            if (cutTime > 0)
            {
                _ = Mover.MoveVariableToTarget(ref cutTime, 0f, 1f, delta);
                if (cutTime < 1.95f && forceWhite)
                {
                    RemovePart(cut);
                }
            }
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                // Don't integrate an endpoint the rope doesn't own: tail is always external,
                // and a non-owned head (a candy point in a candiesConnected link) is integrated
                // by the candy system. Owned anchors (normal/kicked grabs) still integrate.
                if (constraintedPoint != tail && (constraintedPoint != bungeeAnchor || ownsAnchor))
                {
                    ConstraintedPoint.Qcpupdate(constraintedPoint, delta, koeff);
                }
            }
            for (int j = 0; j < relaxationTimes; j++)
            {
                int count2 = parts.Count;
                for (int k = 0; k < count2; k++)
                {
                    ConstraintedPoint.SatisfyConstraints(parts[k]);
                }
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            int count = parts.Count;
            int drawSamplePoints = ActivePhysicsConstants.BungeeDrawSamplePoints;
            Renderer.SetColor(s_Color1);
            if (cut == -1)
            {
                Vector[] array = new Vector[count];
                for (int i = 0; i < count; i++)
                {
                    ConstraintedPoint constraintedPoint = parts[i];
                    array[i] = constraintedPoint.pos;
                }
                s_lightCounter = 0;
                s_lightStartCoord = 8;
                s_lightEndSkip = 8;
                if (breakable)
                {
                    DrawChain(this, array, count, drawSamplePoints);
                }
                else
                {
                    DrawBungee(this, array, count, drawSamplePoints);
                }
                return;
            }
            Vector[] array2 = new Vector[count];
            Vector[] array3 = new Vector[count];
            bool flag = false;
            int tailPartCount = 0;
            for (int j = 0; j < count; j++)
            {
                ConstraintedPoint constraintedPoint2 = parts[j];
                bool flag2 = true;
                if (j > 0)
                {
                    ConstraintedPoint p = parts[j - 1];
                    if (!constraintedPoint2.HasConstraintTo(p))
                    {
                        flag2 = false;
                    }
                }
                if (constraintedPoint2.pin.X == -1f && !flag2)
                {
                    flag = true;
                    array2[j] = constraintedPoint2.pos;
                }
                if (!flag)
                {
                    array2[j] = constraintedPoint2.pos;
                }
                else
                {
                    array3[tailPartCount] = constraintedPoint2.pos;
                    tailPartCount++;
                }
            }
            int headPartCount = count - tailPartCount;
            s_lightCounter = 0;
            s_lightSavedEnd = 0;
            if (headPartCount > 0)
            {
                s_lightStartCoord = 8;
                s_lightEndSkip = tailPartCount > 0 ? 0 : 8;
                if (breakable)
                {
                    DrawChain(this, array2, headPartCount, drawSamplePoints);
                }
                else
                {
                    DrawBungee(this, array2, headPartCount, drawSamplePoints);
                }
            }
            if (tailPartCount > 0 && !hideTailParts)
            {
                s_lightStartCoord = headPartCount > 0 ? s_lightSavedEnd : 8;
                if (breakable)
                {
                    DrawChain(this, array3, tailPartCount, drawSamplePoints);
                }
                else
                {
                    DrawBungee(this, array3, tailPartCount, drawSamplePoints);
                }
            }
        }

        /// <summary>
        /// Draws Christmas lights along the rope.
        /// Matches the original iOS implementation: lights are placed at every 6 bezier sample points
        /// (12 coord indices), skipping 4 points at the start and end of each segment.
        /// Uses static state (<see cref="s_lightStartCoord" />, <see cref="s_lightEndSkip" />, etc.) set by <see cref="Draw" /> to coordinate
        /// light placement across head/tail segments when the rope is cut.
        /// </summary>
        /// <param name="alpha">Alpha multiplier applied to the light sprites.</param>
        private void DrawChristmasLights(float alpha)
        {
            if (!SpecialEvents.IsXmas || drawPtsCount < 4 || drawPts == null || alpha <= 0f)
            {
                return;
            }

            CTRTexture2D texture;
            try
            {
                texture = Application.GetTexture(Resources.Img.XmasLights);
            }
            catch
            {
                return;
            }

            CTRRectangle[] rects = texture.quadRects;
            int rectCount = texture.quadsCount > 0 ? texture.quadsCount : rects?.Length ?? 0;
            if (rectCount == 0)
            {
                return;
            }

            int totalCoords = drawPtsCount;
            int startCoord = s_lightStartCoord;
            int endCoord = totalCoords - s_lightEndSkip;

            if (startCoord >= endCoord)
            {
                return;
            }

            RGBAColor color = RGBAColor.whiteRGBA;
            if (alpha < 1f)
            {
                color.AlphaChannel = alpha;
            }
            Renderer.SetColor(color.ToXNA());

            lightRandomSeed ??= christmasRandom.Next(0, 1000);

            int lightIdx = s_lightCounter;
            for (int i = startCoord; i < endCoord; i += 12)
            {
                if (lightsCount != -1 && lightIdx >= lightsCount)
                {
                    break;
                }

                if (i + 1 >= drawPts.Length)
                {
                    break;
                }

                float x = drawPts[i];
                float y = drawPts[i + 1];

                if (lightsCount == -1)
                {
                    lightFrames[lightIdx] = christmasRandom.Next(rectCount);
                }

                int rectIndex = lightFrames[lightIdx] % rectCount;
                CTRRectangle rect = rects[rectIndex];

                DrawHelper.DrawImagePart(texture, rect, x - (rect.w / 2f), y - (rect.h / 2f));

                // Save overflow for tail continuation
                s_lightSavedEnd = i + 12 - endCoord + 2;

                lightIdx++;
            }

            s_lightCounter = lightIdx;
            if (lightsCount == -1)
            {
                lightsCount = lightIdx;
            }

            Renderer.SetColor(RGBAColor.whiteRGBA.ToXNA());
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (parts != null)
                {
                    foreach (ConstraintedPoint part in parts)
                    {
                        bool ownsPart = (part == bungeeAnchor && ownsAnchor) || (part == tail && ownsTail) || (part != bungeeAnchor && part != tail);
                        if (ownsPart)
                        {
                            part?.Dispose();
                        }
                    }
                    parts = null;
                }
                bungeeAnchor = null;
                tail = null;
                drawPts = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Moves the anchor to a new position, shifting all rope parts by the same delta.
        /// </summary>
        /// <param name="newPos">New world-space position for the bungee anchor.</param>
        public void MoveAnchor(Vector newPos)
        {
            Vector oldPos = bungeeAnchor != null ? bungeeAnchor.pos : Vect(0f, 0f);
            float dx = newPos.X - oldPos.X;
            float dy = newPos.Y - oldPos.Y;

            if (parts != null)
            {
                foreach (ConstraintedPoint part in parts)
                {
                    part.pos = Vect(part.pos.X + dx, part.pos.Y + dy);

                    // Keep Verlet history aligned so teleport doesn't inject fake velocity.
                    if (part.prevPos.X != vectUndefined.X)
                    {
                        part.prevPos = Vect(part.prevPos.X + dx, part.prevPos.Y + dy);
                    }

                    // Only adjust pins that are actually set (unset pin is -1, -1).
                    if (part.pin.X != -1f || part.pin.Y != -1f)
                    {
                        part.pin = Vect(part.pin.X + dx, part.pin.Y + dy);
                    }
                }
            }
        }

        /// <summary>Number of constraint relaxation passes used by the bungee solver.</summary>
        public const int BUNGEE_RELAXION_TIMES = 30;

        /// <summary>Whether the bungee should be rendered with highlight brightness.</summary>
        public bool highlighted;

        /// <summary>Rest length used between adjacent bungee constraint points.</summary>
        public static float BUNGEE_REST_LEN = ActivePhysicsConstants.BungeeRestLength;

        /// <summary>Head anchor constraint point for the bungee.</summary>
        public ConstraintedPoint bungeeAnchor;

        /// <summary>Tail constraint point for the bungee.</summary>
        public ConstraintedPoint tail;

        /// <summary>Cut segment index, or <c>-1</c> when the bungee is uncut.</summary>
        public int cut;

        /// <summary>Current relaxation bucket derived from the bungee stretch distance.</summary>
        public int relaxed;

        /// <summary>Initial candle angle used by candle-related bungee behavior.</summary>
        public float initialCandleAngle;

        /// <summary>Whether this bungee is marked as the selected or active special instance.</summary>
        public bool chosenOne;

        /// <summary>Current bungee behavior mode.</summary>
        public int bungeeMode;

        /// <summary>Whether the bungee should render in white during a cut transition.</summary>
        public bool forceWhite;

        /// <summary>Remaining cut fade time in seconds.</summary>
        public float cutTime;

        /// <summary>
        /// Flat array of bezier curve points in the format [x0, y0, x1, y1, x2, y2, ...].
        /// Used for rendering the rope and positioning Christmas lights.
        /// </summary>
        public float[] drawPts = new float[ActivePhysicsConstants.DrawPtsBufferSize];

        /// <summary>
        /// Number of valid coordinates in the <see cref="drawPts" /> array.
        /// </summary>
        public int drawPtsCount;

        /// <summary>Base rendered line width for the bungee.</summary>
        public float lineWidth;

        /// <summary>Whether tail-side rope parts should be hidden after the bungee is cut.</summary>
        public bool hideTailParts;

        /// <summary>Whether this bungee renders as a chain.</summary>
        public bool breakable;

        /// <summary>Texture quad used at each sampled chain point.</summary>
        private const int ChainPointQuad = 0;

        /// <summary>Texture quad used between sampled chain points.</summary>
        private const int ChainMidpointQuad = 1;

        /// <summary>
        /// A planned chain sprite draw.
        /// </summary>
        /// <param name="quadIndex">Texture quad index.</param>
        /// <param name="center">Sprite center.</param>
        /// <param name="rotation">Sprite rotation in radians.</param>
        /// <param name="vertexQuad">Rotated destination vertices.</param>
        internal readonly struct ChainSprite(int quadIndex, Vector center, float rotation, Quad3D vertexQuad)
        {
            /// <summary>Texture quad index.</summary>
            public int QuadIndex { get; } = quadIndex;

            /// <summary>Sprite center.</summary>
            public Vector Center { get; } = center;

            /// <summary>Sprite rotation in radians.</summary>
            public float Rotation { get; } = rotation;

            /// <summary>Rotated destination vertices.</summary>
            public Quad3D VertexQuad { get; } = vertexQuad;
        }

        /// <summary>Random number generator used for Christmas light frame selection.</summary>
        private static readonly Random christmasRandom = new();

        /// <summary>Seed captured on first Christmas light draw for reproducibility.</summary>
        private int? lightRandomSeed;

        /// <summary>
        /// Per-light frame indices, stored on first draw and reused for consistency.
        /// The number of lights can go up to 200.
        /// </summary>
        private readonly int[] lightFrames = new int[200];

        /// <summary>
        /// Number of lights determined on first draw (-1 = not yet determined).
        /// </summary>
        private int lightsCount = -1;

        /// <summary>Starting coordinate index for Christmas light placement in the current segment.</summary>
        private static int s_lightStartCoord;

        /// <summary>Number of coordinate indices to skip at the end of the current segment.</summary>
        private static int s_lightEndSkip;

        /// <summary>Running light index counter shared across head/tail draw calls.</summary>
        private static int s_lightCounter;

        /// <summary>Saved end overflow used to continue light placement into the tail segment.</summary>
        private static int s_lightSavedEnd;

        /// <summary>Whether this bungee owns and should dispose the anchor point.</summary>
        private bool ownsAnchor;

        /// <summary>Whether this bungee owns and should dispose the tail point.</summary>
        private bool ownsTail;

        /// <summary>Cached vertex array for main rope rendering.</summary>
        private static VertexPositionColor[] s_bungeeVerticesCache;

        /// <summary>Cached float array for outer glow vertex positions.</summary>
        private static float[] s_bungeePointerCache;

        /// <summary>Cached float array for inner rope vertex positions.</summary>
        private static float[] s_bungeePointerCache2;

        /// <summary>Per-vertex color array for the outer glow triangle strip.</summary>
        private static readonly RGBAColor[] ccolors =
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

        /// <summary>Per-vertex color array for the inner rope triangle strip.</summary>
        private static readonly RGBAColor[] ccolors2 =
        [
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA,
            RGBAColor.transparentRGBA
        ];

        /// <summary>Default dark base color used when setting the renderer before drawing.</summary>
        private static Color s_Color1 = new(0f, 0f, 0.4f, 1f);

        /// <summary>
        /// Bungee behavior modes.
        /// </summary>
        private enum BUNGEE_MODE
        {
            /// <summary>Normal bungee behavior.</summary>
            NORMAL,

            /// <summary>Locked bungee that does not respond to physics updates.</summary>
            LOCKED
        }
    }
}
