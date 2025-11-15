using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.game
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
                Vector v3 = highlighted ? vector : VectMult(vector, color.a == 1.0 ? 1.02 : 1.0);
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
                lx = v6.x;
                ly = v6.y;
                rx = v7.x;
                ry = v7.y;
                Vector vector4 = VectSub(v4, vector2);
                Vector vector5 = VectSub(v8, vector2);
                Vector vector6 = VectAdd(v5, vector2);
                Vector vector7 = VectAdd(v9, vector2);
                float[] pointer =
                [
                    vector3.x, vector3.y, v11.x, v11.y, v4.x, v4.y, v8.x, v8.y, v5.x, v5.y,
                    v9.x, v9.y, v10.x, v10.y, v12.x, v12.y
                ];
                RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
                whiteRGBA.a = 0.1f * color.a;
                ccolors[2] = whiteRGBA;
                ccolors[3] = whiteRGBA;
                ccolors[4] = whiteRGBA;
                ccolors[5] = whiteRGBA;
                float[] pointer2 =
                [
                    v4.x, v4.y, v8.x, v8.y, vector4.x, vector4.y, vector5.x, vector5.y, v.x, v.y,
                    v2.x, v2.y, vector6.x, vector6.y, vector7.x, vector7.y, v5.x, v5.y, v9.x, v9.y
                ];
                RGBAColor rgbaColor = color;
                float num = 0.15f * color.a;
                color.r += num;
                color.g += num;
                color.b += num;
                ccolors2[2] = color;
                ccolors2[3] = color;
                ccolors2[4] = rgbaColor;
                ccolors2[5] = rgbaColor;
                ccolors2[6] = color;
                ccolors2[7] = color;
                OpenGL.GlDisableClientState(0);
                OpenGL.GlEnableClientState(13);
                if (highlighted)
                {
                    OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                    OpenGL.GlColorPointer(4, 5, 0, ccolors);
                    OpenGL.GlVertexPointer(2, 5, 0, pointer);
                    OpenGL.GlDrawArrays(8, 0, 8);
                }
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                OpenGL.GlColorPointer(4, 5, 0, ccolors2);
                OpenGL.GlVertexPointer(2, 5, 0, pointer2);
                OpenGL.GlDrawArrays(8, 0, 10);
                OpenGL.GlEnableClientState(0);
                OpenGL.GlDisableClientState(13);
            }
        }

        private static void DrawBungee(Bungee b, Vector[] pts, int count, int points)
        {
            float num = b.cut == -1 || b.forceWhite ? 1f : b.cutTime / 1.95f;
            RGBAColor rgbaColor = RGBAColor.MakeRGBA(0.475 * (double)num, 0.305 * (double)num, 0.185 * (double)num, (double)num);
            RGBAColor rgbaColor2 = RGBAColor.MakeRGBA(0.6755555555555556 * (double)num, 0.44 * (double)num, 0.27555555555555555 * (double)num, (double)num);
            RGBAColor rgbaColor3 = RGBAColor.MakeRGBA(0.19 * (double)num, 0.122 * (double)num, 0.074 * (double)num, (double)num);
            RGBAColor rgbaColor4 = RGBAColor.MakeRGBA(0.304 * (double)num, 0.198 * (double)num, 0.124 * (double)num, (double)num);
            if (b.highlighted)
            {
                float num2 = 3f;
                rgbaColor.r *= num2;
                rgbaColor.g *= num2;
                rgbaColor.b *= num2;
                rgbaColor2.r *= num2;
                rgbaColor2.g *= num2;
                rgbaColor2.b *= num2;
                rgbaColor3.r *= num2;
                rgbaColor3.g *= num2;
                rgbaColor3.b *= num2;
                rgbaColor4.r *= num2;
                rgbaColor4.g *= num2;
                rgbaColor4.b *= num2;
            }
            float num3 = VectDistance(Vect(pts[0].x, pts[0].y), Vect(pts[1].x, pts[1].y));
            b.relaxed = (double)num3 <= BUNGEE_REST_LEN + 0.3
                ? 0
                : (double)num3 <= BUNGEE_REST_LEN + 1.0 ? 1 : (double)num3 <= BUNGEE_REST_LEN + 4.0 ? 2 : 3;
            if ((double)num3 > BUNGEE_REST_LEN + 7.0)
            {
                float num4 = num3 / BUNGEE_REST_LEN * 2f;
                rgbaColor3.r *= num4;
                rgbaColor4.r *= num4;
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
            float num11 = (rgbaColor.r - rgbaColor3.r) / (num5 - 1);
            float num12 = (rgbaColor.g - rgbaColor3.g) / (num5 - 1);
            float num13 = (rgbaColor.b - rgbaColor3.b) / (num5 - 1);
            float num14 = (rgbaColor2.r - rgbaColor4.r) / (num5 - 1);
            float num15 = (rgbaColor2.g - rgbaColor4.g) / (num5 - 1);
            float num16 = (rgbaColor2.b - rgbaColor4.b) / (num5 - 1);
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
                Vector vector = GLDrawer.CalcPathBezier(pts, count, num7);
                array[num8++] = vector.x;
                array[num8++] = vector.y;
                b.drawPts[num9++] = vector.x;
                b.drawPts[num9++] = vector.y;
                if (num8 >= 8 || (double)num7 == 1.0)
                {
                    RGBAColor color = b.forceWhite ? RGBAColor.whiteRGBA : !flag ? rgbaColor6 : rgbaColor5;
                    OpenGL.GlColor4f(color.ToXNA());
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
                    rgbaColor5.r += num11 * (num17 - 1);
                    rgbaColor5.g += num12 * (num17 - 1);
                    rgbaColor5.b += num13 * (num17 - 1);
                    rgbaColor6.r += num14 * (num17 - 1);
                    rgbaColor6.g += num15 * (num17 - 1);
                    rgbaColor6.b += num16 * (num17 - 1);
                }
                if ((double)num7 == 1.0)
                {
                    break;
                }
                num7 += num6;
            }
        }

        public Bungee InitWithHeadAtXYTailAtTXTYandLength(ConstraintedPoint h, float hx, float hy, ConstraintedPoint t, float tx, float ty, float len)
        {
            relaxationTimes = 30;
            lineWidth = 10f;
            cut = -1;
            bungeeMode = 0;
            highlighted = false;
            bungeeAnchor = h ?? new ConstraintedPoint();
            if (t != null)
            {
                tail = t;
            }
            else
            {
                tail = new ConstraintedPoint();
                tail.SetWeight(1f);
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
                    if (bungeeAnchor.pin.x != -1f)
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
            OpenGL.GlColor4f(s_Color1);
            if (cut == -1)
            {
                Vector[] array = new Vector[count];
                for (int i = 0; i < count; i++)
                {
                    ConstraintedPoint constraintedPoint = parts[i];
                    array[i] = constraintedPoint.pos;
                }
                OpenGL.GlLineWidth(lineWidth);
                DrawBungee(this, array, count, 4);
                OpenGL.GlLineWidth(1.0);
                return;
            }
            Vector[] array2 = new Vector[count];
            Vector[] array3 = new Vector[count];
            bool flag = false;
            int num = 0;
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
                if (constraintedPoint2.pin.x == -1f && !flag2)
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
                    array3[num] = constraintedPoint2.pos;
                    num++;
                }
            }
            OpenGL.GlLineWidth(lineWidth);
            int num2 = count - num;
            if (num2 > 0)
            {
                DrawBungee(this, array2, num2, 4);
            }
            if (num > 0 && !hideTailParts)
            {
                DrawBungee(this, array3, num, 4);
            }
            OpenGL.GlLineWidth(1.0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                bungeeAnchor?.Dispose();
                bungeeAnchor = null;
                tail?.Dispose();
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

        public float[] drawPts = new float[200];

        public int drawPtsCount;

        public float lineWidth;

        public bool hideTailParts;

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
