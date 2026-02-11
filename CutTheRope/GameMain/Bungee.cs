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
                Vector v3 = highlighted ? vector : VectMult(vector, color.AlphaChannel == 1.0 ? 1.02 : 1.0);
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
                float num = 0.15f * color.AlphaChannel;
                color.RedColor += num;
                color.GreenColor += num;
                color.BlueColor += num;
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

        private static void DrawBungee(Bungee b, Vector[] pts, int count, int points, int segmentStartIndex)
        {
            float num = b.cut == -1 || b.forceWhite ? 1f : b.cutTime / 1.95f;

            // Get selected rope colors from preferences
            int selectedRopeIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_ROPE);
            RopeColorHelper.RopeColors ropeColors = RopeColorHelper.GetRopeColors(selectedRopeIndex);

            // Apply alpha multiplier to base colors
            RGBAColor rgbaColor = RGBAColor.MakeRGBA(
                ropeColors.Color1.RedColor * num,
                ropeColors.Color1.GreenColor * num,
                ropeColors.Color1.BlueColor * num,
                (double)num);
            RGBAColor rgbaColor2 = RGBAColor.MakeRGBA(
                ropeColors.Color2.RedColor * num,
                ropeColors.Color2.GreenColor * num,
                ropeColors.Color2.BlueColor * num,
                (double)num);

            // Create darker variants for shading (40% of base color)
            RGBAColor rgbaColor3 = RGBAColor.MakeRGBA(
                ropeColors.Color1.RedColor * 0.4 * num,
                ropeColors.Color1.GreenColor * 0.4 * num,
                ropeColors.Color1.BlueColor * 0.4 * num,
                (double)num);
            RGBAColor rgbaColor4 = RGBAColor.MakeRGBA(
                ropeColors.Color2.RedColor * 0.45 * num,
                ropeColors.Color2.GreenColor * 0.45 * num,
                ropeColors.Color2.BlueColor * 0.45 * num,
                (double)num);
            if (b.highlighted)
            {
                float num2 = 3f;
                rgbaColor.RedColor *= num2;
                rgbaColor.GreenColor *= num2;
                rgbaColor.BlueColor *= num2;
                rgbaColor2.RedColor *= num2;
                rgbaColor2.GreenColor *= num2;
                rgbaColor2.BlueColor *= num2;
                rgbaColor3.RedColor *= num2;
                rgbaColor3.GreenColor *= num2;
                rgbaColor3.BlueColor *= num2;
                rgbaColor4.RedColor *= num2;
                rgbaColor4.GreenColor *= num2;
                rgbaColor4.BlueColor *= num2;
            }
            float num3 = VectDistance(Vect(pts[0].X, pts[0].Y), Vect(pts[1].X, pts[1].Y));
            b.relaxed = (double)num3 <= BUNGEE_REST_LEN + 0.3
                ? 0
                : (double)num3 <= BUNGEE_REST_LEN + 1.0 ? 1 : (double)num3 <= BUNGEE_REST_LEN + 4.0 ? 2 : 3;
            if ((double)num3 > BUNGEE_REST_LEN + 7.0)
            {
                float num4 = num3 / BUNGEE_REST_LEN * 2f;
                rgbaColor3.RedColor *= num4;
                rgbaColor4.RedColor *= num4;
            }
            bool flag = false;
            int num5 = (count - 1) * points;
            float[] array = new float[num5 * 2];
            b.drawPtsCount = num5 * 2;
            float num6 = 1f / num5;
            float num7 = 0f;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            RGBAColor rgbaColor5 = rgbaColor3;
            RGBAColor rgbaColor6 = rgbaColor4;
            float num11 = (rgbaColor.RedColor - rgbaColor3.RedColor) / (num5 - 1);
            float num12 = (rgbaColor.GreenColor - rgbaColor3.GreenColor) / (num5 - 1);
            float num13 = (rgbaColor.BlueColor - rgbaColor3.BlueColor) / (num5 - 1);
            float num14 = (rgbaColor2.RedColor - rgbaColor4.RedColor) / (num5 - 1);
            float num15 = (rgbaColor2.GreenColor - rgbaColor4.GreenColor) / (num5 - 1);
            float num16 = (rgbaColor2.BlueColor - rgbaColor4.BlueColor) / (num5 - 1);
            float lx = -1f;
            float ly = -1f;
            float rx = -1f;
            float ry = -1f;
            for (; ; )
            {
                if ((double)num7 > 1.0)
                {
                    num7 = 1f;
                }
                if (count < 3)
                {
                    break;
                }
                Vector vector = DrawHelper.CalcPathBezier(pts, count, num7);
                array[num8++] = vector.X;
                array[num8++] = vector.Y;
                b.drawPts[num9++] = vector.X;
                b.drawPts[num9++] = vector.Y;
                if (num8 >= 8 || (double)num7 == 1.0)
                {
                    RGBAColor color = b.forceWhite ? RGBAColor.whiteRGBA : !flag ? rgbaColor6 : rgbaColor5;
                    Renderer.SetColor(color.ToXNA());
                    int num17 = num8 >> 1;
                    for (int i = 0; i < num17 - 1; i++)
                    {
                        DrawAntialiasedLineContinued(array[i * 2], array[(i * 2) + 1], array[(i * 2) + 2], array[(i * 2) + 3], 5f, color, ref lx, ref ly, ref rx, ref ry, b.highlighted);
                    }
                    array[0] = array[num8 - 2];
                    array[1] = array[num8 - 1];
                    num8 = 2;
                    flag = !flag;
                    num10++;
                    rgbaColor5.RedColor += num11 * (num17 - 1);
                    rgbaColor5.GreenColor += num12 * (num17 - 1);
                    rgbaColor5.BlueColor += num13 * (num17 - 1);
                    rgbaColor6.RedColor += num14 * (num17 - 1);
                    rgbaColor6.GreenColor += num15 * (num17 - 1);
                    rgbaColor6.BlueColor += num16 * (num17 - 1);
                }
                if ((double)num7 == 1.0)
                {
                    break;
                }
                num7 += num6;
            }

            b.drawPtsCount = num9;
            b.DrawChristmasLights(num9 / 2, num, segmentStartIndex);
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
            int num = (int)((len / BUNGEE_REST_LEN) + 2f);
            v = VectDiv(v, num);
            RollplacingWithOffset(len, v);
            forceWhite = false;
            initialCandleAngle = -1f;
            chosenOne = false;
            hideTailParts = false;
            return this;
        }

        public int GetLength()
        {
            int num = 0;
            Vector pos = vectZero;
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                if (i > 0)
                {
                    num += (int)VectDistance(pos, constraintedPoint.pos);
                }
                pos = constraintedPoint.pos;
            }
            return num;
        }

        public void Roll(float rollLen)
        {
            RollplacingWithOffset(rollLen, vectZero);
        }

        public void RollplacingWithOffset(float rollLen, Vector off)
        {
            ConstraintedPoint i = parts[^2];
            int num = (int)tail.RestLengthFor(i);
            while (rollLen > 0f)
            {
                if (rollLen >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint constraintedPoint = parts[^2];
                    ConstraintedPoint constraintedPoint2 = new();
                    constraintedPoint2.SetWeight(0.02f);
                    constraintedPoint2.pos = VectAdd(constraintedPoint.pos, off);
                    AddPartAt(constraintedPoint2, parts.Count - 1);
                    tail.ChangeConstraintFromTowithRestLength(constraintedPoint, constraintedPoint2, num);
                    constraintedPoint2.AddConstraintwithRestLengthofType(constraintedPoint, BUNGEE_REST_LEN, Constraint.CONSTRAINT.DISTANCE);
                    rollLen -= BUNGEE_REST_LEN;
                }
                else
                {
                    int num2 = (int)(rollLen + num);
                    if (num2 > BUNGEE_REST_LEN)
                    {
                        rollLen = BUNGEE_REST_LEN;
                        num = (int)(num2 - BUNGEE_REST_LEN);
                    }
                    else
                    {
                        ConstraintedPoint n2 = parts[^2];
                        tail.ChangeRestLengthToFor(num2, n2);
                        rollLen = 0f;
                    }
                }
            }
        }

        public float RollBack(float amount)
        {
            float num = amount;
            ConstraintedPoint i = parts[^2];
            int num2 = (int)tail.RestLengthFor(i);
            int num3 = parts.Count;
            while (num > 0f)
            {
                if (num >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint o = parts[num3 - 2];
                    ConstraintedPoint n2 = parts[num3 - 3];
                    tail.ChangeConstraintFromTowithRestLength(o, n2, num2);
                    parts.RemoveAt(parts.Count - 2);
                    num3--;
                    num -= BUNGEE_REST_LEN;
                }
                else
                {
                    int num4 = (int)(num2 - num);
                    if (num4 < 1)
                    {
                        num = BUNGEE_REST_LEN;
                        num2 = (int)(BUNGEE_REST_LEN + num4 + 1f);
                    }
                    else
                    {
                        ConstraintedPoint n3 = parts[num3 - 2];
                        tail.ChangeRestLengthToFor(num4, n3);
                        num = 0f;
                    }
                }
            }
            int count = tail.constraints.Count;
            for (int j = 0; j < count; j++)
            {
                Constraint constraint = tail.constraints[j];
                if (constraint != null && constraint.type == Constraint.CONSTRAINT.NOT_MORE_THAN)
                {
                    constraint.restLength = (num3 - 1) * (BUNGEE_REST_LEN + 3f);
                }
            }
            return num;
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
                            constraintedPoint.AddConstraintwithRestLengthofType(bungeeAnchor, i * (BUNGEE_REST_LEN + 3f), Constraint.CONSTRAINT.NOT_MORE_THAN);
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
            if (cutTime > 0.0)
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
            Renderer.SetColor(s_Color1);
            if (cut == -1)
            {
                Vector[] array = new Vector[count];
                for (int i = 0; i < count; i++)
                {
                    ConstraintedPoint constraintedPoint = parts[i];
                    array[i] = constraintedPoint.pos;
                }
                DrawBungee(this, array, count, 4, 0);
                return;
            }
            Vector[] array2 = new Vector[count];
            Vector[] array3 = new Vector[count];
            bool flag = false;
            int num = 0;
            int cutIndex = 0;
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
                    cutIndex = j;
                    array2[j] = constraintedPoint2.pos;
                }
                if (!flag)
                {
                    array2[j] = constraintedPoint2.pos;
                }
                else
                {
                    array3[num] = constraintedPoint2.pos;
                    num++;
                }
            }
            int num2 = count - num;
            if (num2 > 0)
            {
                DrawBungee(this, array2, num2, 4, 0);
            }
            if (num > 0 && !hideTailParts)
            {
                DrawBungee(this, array3, num, 4, cutIndex);
            }
        }

        /// <summary>
        /// Draws Christmas lights along the rope during the Christmas event.
        /// Lights are positioned at regular intervals along the rope's bezier curve,
        /// with colors that remain consistent even when the rope is cut.
        /// </summary>
        /// <param name="pointCount">Number of points in the bezier curve (drawPts array length / 2)</param>
        /// <param name="alpha">Alpha transparency value for fading effects (0.0 to 1.0)</param>
        /// <param name="segmentStartIndex">Starting segment index for cut rope pieces, used to maintain color consistency</param>
        private void DrawChristmasLights(int pointCount, float alpha, int segmentStartIndex)
        {
            // Early exit if Christmas mode is disabled, not enough points, or fully transparent
            if (!SpecialEvents.IsXmas || pointCount < 2 || drawPts == null || alpha <= 0f)
            {
                return;
            }

            // Load the Christmas lights texture atlas
            CTRTexture2D texture;
            try
            {
                texture = Application.GetTexture(Resources.Img.XmasLights);
            }
            catch
            {
                return;
            }

            // Get the sprite frames from the texture atlas
            CTRRectangle[] rects = texture.quadRects;
            int rectCount = texture.quadsCount > 0 ? texture.quadsCount : rects?.Length ?? 0;
            if (rectCount == 0)
            {
                return;
            }

            // Calculate spacing between lights (1.5x the base rope segment length)
            float lightSpacing = BUNGEE_REST_LEN * 1.5f;

            // Calculate cumulative distances along the rope's bezier curve
            // This allows us to position lights at equal intervals along the curved rope
            float[] distances = new float[pointCount];
            float totalDistance = 0f;

            for (int i = 1; i < pointCount; i++)
            {
                // drawPts is a flat array: [x0, y0, x1, y1, x2, y2, ...]
                int index = i * 2;
                int previousIndex = index - 2;
                float dx = drawPts[index] - drawPts[previousIndex];
                float dy = drawPts[index + 1] - drawPts[previousIndex + 1];
                totalDistance += MathF.Sqrt((dx * dx) + (dy * dy));
                distances[i] = totalDistance;
            }

            // Set the drawing color with alpha for fade effects
            RGBAColor color = RGBAColor.whiteRGBA;
            if (alpha < 1f)
            {
                color.AlphaChannel = alpha;
            }
            Renderer.SetColor(color.ToXNA());

            // Initialize random seed for consistent light color selection across frames
            // This seed remains the same for the lifetime of the rope
            lightRandomSeed ??= christmasRandom.Next(0, 1000);

            // Calculate offset based on segment position to maintain consistent colors after cutting
            // This ensures that when a rope is cut, the remaining pieces show the same light colors
            // at the same absolute positions along the original rope
            float segmentOffset = segmentStartIndex * BUNGEE_REST_LEN;

            // Start placing lights at half the spacing interval (centered distribution)
            float currentDistance = lightSpacing / 2f;

            // Draw lights at regular intervals along the rope
            while (currentDistance < totalDistance)
            {
                // Find which curve segment this light position falls on
                for (int i = 1; i < pointCount; i++)
                {
                    float segmentEnd = distances[i];
                    float segmentStart = distances[i - 1];

                    // Skip segments that end before the current light position
                    if (currentDistance > segmentEnd)
                    {
                        continue;
                    }

                    // Interpolate position within the segment using linear interpolation
                    float segmentDelta = Math.Max(segmentEnd - segmentStart, 0.0001f);
                    float t = (currentDistance - segmentStart) / segmentDelta;

                    // Calculate the actual x,y position along the bezier curve
                    int index = i * 2;
                    int previousIndex = index - 2;
                    float x = drawPts[previousIndex] + ((drawPts[index] - drawPts[previousIndex]) * t);
                    float y = drawPts[previousIndex + 1] + ((drawPts[index + 1] - drawPts[previousIndex + 1]) * t);

                    // Select light color based on absolute distance (including segment offset)
                    // This ensures consistent colors even after the rope is cut
                    int distanceIndex = (int)MathF.Round((currentDistance + segmentOffset) / lightSpacing);
                    int rectIndex = (lightRandomSeed.Value + distanceIndex) % rectCount;
                    CTRRectangle rect = rects[rectIndex];

                    // Draw the light sprite centered on the calculated position
                    DrawHelper.DrawImagePart(texture, rect, x - (rect.w / 2f), y - (rect.h / 2f));
                    break;
                }

                // Move to the next light position
                currentDistance += lightSpacing;
            }

            // Reset drawing color to default
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

        public const int BUNGEE_RELAXION_TIMES = 30;
        public bool highlighted;

        public static float BUNGEE_REST_LEN = 105f;

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
        public float[] drawPts = new float[200];

        /// <summary>
        /// Number of valid coordinates in the drawPts array (actual length is drawPtsCount * 2).
        /// </summary>
        public int drawPtsCount;

        public float lineWidth;

        public bool hideTailParts;

        /// <summary>
        /// Random number generator for Christmas light color selection.
        /// Shared across all Bungee instances to ensure variety.
        /// </summary>
        private static readonly Random christmasRandom = new();

        /// <summary>
        /// Per-rope random seed used to select light colors deterministically.
        /// Ensures lights maintain consistent colors across frames and after cutting.
        /// </summary>
        private int? lightRandomSeed;

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
