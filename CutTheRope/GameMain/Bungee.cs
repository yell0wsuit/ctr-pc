using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    internal sealed class Bungee : ConstraintSystem
    {
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

        private static void WritePair(float[] buffer, ref int index, Vector v)
        {
            buffer[index++] = v.X;
            buffer[index++] = v.Y;
        }

        private static void DrawBungee(Bungee b, Vector[] pts, int count, int points)
        {
            float alphaMultiplier = b.cut == -1 || b.forceWhite ? 1f : b.cutTime / 1.95f;

            // Get selected rope colors from preferences
            int selectedRopeIndex = Preferences.GetIntForKey("PREFS_SELECTED_ROPE");
            RopeColorHelper.RopeColors ropeColors = RopeColorHelper.GetRopeColors(selectedRopeIndex);

            // Apply alpha multiplier to base colors
            RGBAColor rgbaColor = RGBAColor.MakeRGBA(
                ropeColors.Color1.RedColor * alphaMultiplier,
                ropeColors.Color1.GreenColor * alphaMultiplier,
                ropeColors.Color1.BlueColor * alphaMultiplier,
                alphaMultiplier
            );
            RGBAColor rgbaColor2 = RGBAColor.MakeRGBA(
                ropeColors.Color2.RedColor * alphaMultiplier,
                ropeColors.Color2.GreenColor * alphaMultiplier,
                ropeColors.Color2.BlueColor * alphaMultiplier,
                alphaMultiplier
            );

            // Create darker variants for shading
            // Only the default skin (0) uses dark shading; other skins use full brightness
            float darkFactor1 = selectedRopeIndex == 0 ? 0.4f : 1f;
            float darkFactor2 = selectedRopeIndex == 0 ? 0.45f : 1f;
            RGBAColor rgbaColor3 = RGBAColor.MakeRGBA(
                ropeColors.Color1.RedColor * darkFactor1 * alphaMultiplier,
                ropeColors.Color1.GreenColor * darkFactor1 * alphaMultiplier,
                ropeColors.Color1.BlueColor * darkFactor1 * alphaMultiplier,
                alphaMultiplier
            );
            RGBAColor rgbaColor4 = RGBAColor.MakeRGBA(
                ropeColors.Color2.RedColor * darkFactor2 * alphaMultiplier,
                ropeColors.Color2.GreenColor * darkFactor2 * alphaMultiplier,
                ropeColors.Color2.BlueColor * darkFactor2 * alphaMultiplier,
                alphaMultiplier
            );
            if (b.highlighted)
            {
                float highlightMultiplier = 3f;
                rgbaColor.RedColor *= highlightMultiplier;
                rgbaColor.GreenColor *= highlightMultiplier;
                rgbaColor.BlueColor *= highlightMultiplier;
                rgbaColor2.RedColor *= highlightMultiplier;
                rgbaColor2.GreenColor *= highlightMultiplier;
                rgbaColor2.BlueColor *= highlightMultiplier;
                rgbaColor3.RedColor *= highlightMultiplier;
                rgbaColor3.GreenColor *= highlightMultiplier;
                rgbaColor3.BlueColor *= highlightMultiplier;
                rgbaColor4.RedColor *= highlightMultiplier;
                rgbaColor4.GreenColor *= highlightMultiplier;
                rgbaColor4.BlueColor *= highlightMultiplier;
            }
            float relaxThresholdSoft = ActivePhysicsConstants.BungeeRelaxThresholdSoft;
            float relaxThresholdMedium = ActivePhysicsConstants.BungeeRelaxThresholdMedium;
            float relaxThresholdHard = ActivePhysicsConstants.BungeeRelaxThresholdHard;
            float stretchRedThreshold = ActivePhysicsConstants.BungeeStretchRedThreshold;
            float segmentLength = VectDistance(Vect(pts[0].X, pts[0].Y), Vect(pts[1].X, pts[1].Y));
            b.relaxed = segmentLength <= BUNGEE_REST_LEN + relaxThresholdSoft
                ? 0
                : segmentLength <= BUNGEE_REST_LEN + relaxThresholdMedium
                    ? 1
                    : segmentLength <= BUNGEE_REST_LEN + relaxThresholdHard ? 2 : 3;
            if (segmentLength > BUNGEE_REST_LEN + stretchRedThreshold)
            {
                float stretchRedScale = segmentLength / BUNGEE_REST_LEN * 2f;
                rgbaColor3.RedColor *= stretchRedScale;
                rgbaColor4.RedColor *= stretchRedScale;
            }
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
            bungeeAnchor.SetWeight(0.02f);
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
            return this;
        }

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

        public void Roll(float rollLen)
        {
            RollplacingWithOffset(rollLen, vectZero);
        }

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
                if (constraintedPoint4 != tail)
                {
                    constraintedPoint4.SetWeight(1E-05f);
                }
            }
        }

        public void SetCut(int part)
        {
            cut = part;
            cutTime = 2f;
            forceWhite = true;
        }

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

        public override void Update(float delta)
        {
            Update(delta, 1f);
        }

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
                if (constraintedPoint != tail)
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
                DrawBungee(this, array, count, drawSamplePoints);
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
                DrawBungee(this, array2, headPartCount, drawSamplePoints);
            }
            if (tailPartCount > 0 && !hideTailParts)
            {
                s_lightStartCoord = headPartCount > 0 ? s_lightSavedEnd : 8;
                DrawBungee(this, array3, tailPartCount, drawSamplePoints);
            }
        }

        /// <summary>
        /// Draws Christmas lights along the rope.
        /// Matches the original iOS implementation: lights are placed at every 6 bezier sample points
        /// (12 coord indices), skipping 4 points at the start and end of each segment.
        /// Uses static state (s_lightStartCoord, s_lightEndSkip, etc.) set by Draw() to coordinate
        /// light placement across head/tail segments when the rope is cut.
        /// </summary>
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

        public const int BUNGEE_RELAXION_TIMES = 30;
        public bool highlighted;

        public static float BUNGEE_REST_LEN = ActivePhysicsConstants.BungeeRestLength;

        public ConstraintedPoint bungeeAnchor;

        public ConstraintedPoint tail;

        public int cut;

        public int relaxed;

        public float initialCandleAngle;

        public bool chosenOne;

        public int bungeeMode;

        public bool forceWhite;

        public float cutTime;

        /// <summary>
        /// Flat array of bezier curve points in the format [x0, y0, x1, y1, x2, y2, ...].
        /// Used for rendering the rope and positioning Christmas lights.
        /// </summary>
        public float[] drawPts = new float[ActivePhysicsConstants.DrawPtsBufferSize];

        /// <summary>
        /// Number of valid coordinates in the drawPts array (actual length is drawPtsCount * 2).
        /// </summary>
        public int drawPtsCount;

        public float lineWidth;

        public bool hideTailParts;

        private static readonly Random christmasRandom = new();

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

        // Static state for coordinating lights across head/tail segments
        private static int s_lightStartCoord;
        private static int s_lightEndSkip;
        private static int s_lightCounter;
        private static int s_lightSavedEnd;

        private bool ownsAnchor;

        private bool ownsTail;

        private static VertexPositionColor[] s_bungeeVerticesCache;
        private static float[] s_bungeePointerCache;
        private static float[] s_bungeePointerCache2;

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

        private static Color s_Color1 = new(0f, 0f, 0.4f, 1f);

        private enum BUNGEE_MODE
        {
            NORMAL,
            LOCKED
        }
    }
}
