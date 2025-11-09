using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.game
{
    internal class Bungee : ConstraintSystem
    {
        private static void drawAntialiasedLineContinued(float x1, float y1, float x2, float y2, float size, RGBAColor color, ref float lx, ref float ly, ref float rx, ref float ry, bool highlighted)
        {
            Vector v = vect(x1, y1);
            Vector v2 = vect(x2, y2);
            Vector vector = vectSub(v2, v);
            if (!vectEqual(vector, vectZero))
            {
                Vector v3 = highlighted ? vector : vectMult(vector, (color.a == 1.0) ? 1.02 : 1.0);
                Vector v4 = vectPerp(vector);
                Vector vector2 = vectNormalize(v4);
                v4 = vectMult(vector2, size);
                Vector v5 = vectNeg(v4);
                Vector v6 = vectAdd(v4, vector);
                Vector v7 = vectAdd(v5, vector);
                v6 = vectAdd(v6, v);
                v7 = vectAdd(v7, v);
                Vector v8 = vectAdd(v4, v3);
                Vector v9 = vectAdd(v5, v3);
                Vector vector3 = vectMult(vector2, size + 6f);
                Vector v10 = vectNeg(vector3);
                Vector v11 = vectAdd(vector3, vector);
                Vector v12 = vectAdd(v10, vector);
                vector3 = vectAdd(vector3, v);
                v10 = vectAdd(v10, v);
                v11 = vectAdd(v11, v);
                v12 = vectAdd(v12, v);
                if (lx == -1f)
                {
                    v4 = vectAdd(v4, v);
                    v5 = vectAdd(v5, v);
                }
                else
                {
                    v4 = vect(lx, ly);
                    v5 = vect(rx, ry);
                }
                v8 = vectAdd(v8, v);
                v9 = vectAdd(v9, v);
                lx = v6.x;
                ly = v6.y;
                rx = v7.x;
                ry = v7.y;
                Vector vector4 = vectSub(v4, vector2);
                Vector vector5 = vectSub(v8, vector2);
                Vector vector6 = vectAdd(v5, vector2);
                Vector vector7 = vectAdd(v9, vector2);
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
                RGBAColor rGBAColor = color;
                float num = 0.15f * color.a;
                color.r += num;
                color.g += num;
                color.b += num;
                ccolors2[2] = color;
                ccolors2[3] = color;
                ccolors2[4] = rGBAColor;
                ccolors2[5] = rGBAColor;
                ccolors2[6] = color;
                ccolors2[7] = color;
                OpenGL.glDisableClientState(0);
                OpenGL.glEnableClientState(13);
                if (highlighted)
                {
                    OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
                    OpenGL.glColorPointer(4, 5, 0, ccolors);
                    OpenGL.glVertexPointer(2, 5, 0, pointer);
                    OpenGL.glDrawArrays(8, 0, 8);
                }
                OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                OpenGL.glColorPointer(4, 5, 0, ccolors2);
                OpenGL.glVertexPointer(2, 5, 0, pointer2);
                OpenGL.glDrawArrays(8, 0, 10);
                OpenGL.glEnableClientState(0);
                OpenGL.glDisableClientState(13);
            }
        }

        private static void drawBungee(Bungee b, Vector[] pts, int count, int points)
        {
            float num = (b.cut == -1 || b.forceWhite) ? 1f : (b.cutTime / 1.95f);
            RGBAColor rGBAColor = RGBAColor.MakeRGBA(0.475 * (double)num, 0.305 * (double)num, 0.185 * (double)num, (double)num);
            RGBAColor rGBAColor2 = RGBAColor.MakeRGBA(0.6755555555555556 * (double)num, 0.44 * (double)num, 0.27555555555555555 * (double)num, (double)num);
            RGBAColor rGBAColor3 = RGBAColor.MakeRGBA(0.19 * (double)num, 0.122 * (double)num, 0.074 * (double)num, (double)num);
            RGBAColor rGBAColor4 = RGBAColor.MakeRGBA(0.304 * (double)num, 0.198 * (double)num, 0.124 * (double)num, (double)num);
            if (b.highlighted)
            {
                float num2 = 3f;
                rGBAColor.r *= num2;
                rGBAColor.g *= num2;
                rGBAColor.b *= num2;
                rGBAColor2.r *= num2;
                rGBAColor2.g *= num2;
                rGBAColor2.b *= num2;
                rGBAColor3.r *= num2;
                rGBAColor3.g *= num2;
                rGBAColor3.b *= num2;
                rGBAColor4.r *= num2;
                rGBAColor4.g *= num2;
                rGBAColor4.b *= num2;
            }
            float num3 = vectDistance(vect(pts[0].x, pts[0].y), vect(pts[1].x, pts[1].y));
            if ((double)num3 <= BUNGEE_REST_LEN + 0.3)
            {
                b.relaxed = 0;
            }
            else if ((double)num3 <= BUNGEE_REST_LEN + 1.0)
            {
                b.relaxed = 1;
            }
            else if ((double)num3 <= BUNGEE_REST_LEN + 4.0)
            {
                b.relaxed = 2;
            }
            else
            {
                b.relaxed = 3;
            }
            if ((double)num3 > BUNGEE_REST_LEN + 7.0)
            {
                float num4 = num3 / BUNGEE_REST_LEN * 2f;
                rGBAColor3.r *= num4;
                rGBAColor4.r *= num4;
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
            RGBAColor rGBAColor5 = rGBAColor3;
            RGBAColor rGBAColor6 = rGBAColor4;
            float num11 = (rGBAColor.r - rGBAColor3.r) / (num5 - 1);
            float num12 = (rGBAColor.g - rGBAColor3.g) / (num5 - 1);
            float num13 = (rGBAColor.b - rGBAColor3.b) / (num5 - 1);
            float num14 = (rGBAColor2.r - rGBAColor4.r) / (num5 - 1);
            float num15 = (rGBAColor2.g - rGBAColor4.g) / (num5 - 1);
            float num16 = (rGBAColor2.b - rGBAColor4.b) / (num5 - 1);
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
                Vector vector = GLDrawer.calcPathBezier(pts, count, num7);
                array[num8++] = vector.x;
                array[num8++] = vector.y;
                b.drawPts[num9++] = vector.x;
                b.drawPts[num9++] = vector.y;
                if (num8 >= 8 || (double)num7 == 1.0)
                {
                    RGBAColor color = b.forceWhite ? RGBAColor.whiteRGBA : ((!flag) ? rGBAColor6 : rGBAColor5);
                    OpenGL.glColor4f(color.toXNA());
                    int num17 = num8 >> 1;
                    for (int i = 0; i < num17 - 1; i++)
                    {
                        drawAntialiasedLineContinued(array[i * 2], array[i * 2 + 1], array[i * 2 + 2], array[i * 2 + 3], 5f, color, ref lx, ref ly, ref rx, ref ry, b.highlighted);
                    }
                    array[0] = array[num8 - 2];
                    array[1] = array[num8 - 1];
                    num8 = 2;
                    flag = !flag;
                    num10++;
                    rGBAColor5.r += num11 * (num17 - 1);
                    rGBAColor5.g += num12 * (num17 - 1);
                    rGBAColor5.b += num13 * (num17 - 1);
                    rGBAColor6.r += num14 * (num17 - 1);
                    rGBAColor6.g += num15 * (num17 - 1);
                    rGBAColor6.b += num16 * (num17 - 1);
                }
                if ((double)num7 == 1.0)
                {
                    break;
                }
                num7 += num6;
            }
        }

        public virtual NSObject initWithHeadAtXYTailAtTXTYandLength(ConstraintedPoint h, float hx, float hy, ConstraintedPoint t, float tx, float ty, float len)
        {
            if (init() != null)
            {
                relaxationTimes = 30;
                lineWidth = 10f;
                cut = -1;
                bungeeMode = 0;
                highlighted = false;
                if (h != null)
                {
                    bungeeAnchor = h;
                }
                else
                {
                    bungeeAnchor = (ConstraintedPoint)new ConstraintedPoint().init();
                }
                if (t != null)
                {
                    tail = t;
                }
                else
                {
                    tail = (ConstraintedPoint)new ConstraintedPoint().init();
                    tail.setWeight(1f);
                }
                bungeeAnchor.setWeight(0.02f);
                bungeeAnchor.pos = vect(hx, hy);
                tail.pos = vect(tx, ty);
                addPart(bungeeAnchor);
                addPart(tail);
                tail.addConstraintwithRestLengthofType(bungeeAnchor, BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
                Vector v = vectSub(tail.pos, bungeeAnchor.pos);
                int num = (int)(len / BUNGEE_REST_LEN + 2f);
                v = vectDiv(v, num);
                rollplacingWithOffset(len, v);
                forceWhite = false;
                initialCandleAngle = -1f;
                chosenOne = false;
                hideTailParts = false;
            }
            return this;
        }

        public virtual int getLength()
        {
            int num = 0;
            Vector pos = vectZero;
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                if (i > 0)
                {
                    num += (int)vectDistance(pos, constraintedPoint.pos);
                }
                pos = constraintedPoint.pos;
            }
            return num;
        }

        public virtual void roll(float rollLen)
        {
            this.rollplacingWithOffset(rollLen, vectZero);
        }

        public virtual void rollplacingWithOffset(float rollLen, Vector off)
        {
            ConstraintedPoint i = parts[parts.Count - 2];
            int num = (int)tail.restLengthFor(i);
            while (rollLen > 0f)
            {
                if (rollLen >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint constraintedPoint = parts[parts.Count - 2];
                    ConstraintedPoint constraintedPoint2 = (ConstraintedPoint)new ConstraintedPoint().init();
                    constraintedPoint2.setWeight(0.02f);
                    constraintedPoint2.pos = vectAdd(constraintedPoint.pos, off);
                    addPartAt(constraintedPoint2, parts.Count - 1);
                    tail.changeConstraintFromTowithRestLength(constraintedPoint, constraintedPoint2, num);
                    constraintedPoint2.addConstraintwithRestLengthofType(constraintedPoint, BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
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
                        ConstraintedPoint n2 = parts[parts.Count - 2];
                        tail.changeRestLengthToFor(num2, n2);
                        rollLen = 0f;
                    }
                }
            }
        }

        public virtual float rollBack(float amount)
        {
            float num = amount;
            ConstraintedPoint i = parts[parts.Count - 2];
            int num2 = (int)tail.restLengthFor(i);
            int num3 = parts.Count;
            while (num > 0f)
            {
                if (num >= BUNGEE_REST_LEN)
                {
                    ConstraintedPoint o = parts[num3 - 2];
                    ConstraintedPoint n2 = parts[num3 - 3];
                    tail.changeConstraintFromTowithRestLength(o, n2, num2);
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
                        tail.changeRestLengthToFor(num4, n3);
                        num = 0f;
                    }
                }
            }
            int count = tail.constraints.Count;
            for (int j = 0; j < count; j++)
            {
                Constraint constraint = tail.constraints[j];
                if (constraint != null && constraint.type == Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN)
                {
                    constraint.restLength = (num3 - 1) * (BUNGEE_REST_LEN + 3f);
                }
            }
            return num;
        }

        public virtual void removePart(int part)
        {
            forceWhite = false;
            ConstraintedPoint constraintedPoint = parts[part];
            ConstraintedPoint constraintedPoint2 = (part + 1 >= parts.Count) ? null : parts[part + 1];
            if (constraintedPoint2 == null)
            {
                constraintedPoint.removeConstraints();
            }
            else
            {
                for (int i = 0; i < constraintedPoint2.constraints.Count; i++)
                {
                    Constraint constraint = constraintedPoint2.constraints[i];
                    if (constraint.cp == constraintedPoint)
                    {
                        constraintedPoint2.constraints.Remove(constraint);
                        ConstraintedPoint constraintedPoint3 = (ConstraintedPoint)new ConstraintedPoint().init();
                        constraintedPoint3.setWeight(1E-05f);
                        constraintedPoint3.pos = constraintedPoint2.pos;
                        constraintedPoint3.prevPos = constraintedPoint2.prevPos;
                        addPartAt(constraintedPoint3, part + 1);
                        constraintedPoint3.addConstraintwithRestLengthofType(constraintedPoint, BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
                        break;
                    }
                }
            }
            for (int j = 0; j < parts.Count; j++)
            {
                ConstraintedPoint constraintedPoint4 = parts[j];
                if (constraintedPoint4 != tail)
                {
                    constraintedPoint4.setWeight(1E-05f);
                }
            }
        }

        public virtual void setCut(int part)
        {
            cut = part;
            cutTime = 2f;
            forceWhite = true;
        }

        public virtual void strengthen()
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
                            constraintedPoint.setWeight(0.5f);
                        }
                        if (i != 0)
                        {
                            constraintedPoint.addConstraintwithRestLengthofType(bungeeAnchor, i * (BUNGEE_REST_LEN + 3f), Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
                        }
                    }
                    i++;
                }
            }
        }

        public override void update(float delta)
        {
            update(delta, 1f);
        }

        public virtual void update(float delta, float koeff)
        {
            if (cutTime > 0.0)
            {
                Mover.moveVariableToTarget(ref cutTime, 0f, 1f, delta);
                if (cutTime < 1.95f && forceWhite)
                {
                    removePart(cut);
                }
            }
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                if (constraintedPoint != tail)
                {
                    ConstraintedPoint.qcpupdate(constraintedPoint, delta, koeff);
                }
            }
            for (int j = 0; j < relaxationTimes; j++)
            {
                int count2 = parts.Count;
                for (int k = 0; k < count2; k++)
                {
                    ConstraintedPoint.satisfyConstraints(parts[k]);
                }
            }
        }

        public override void draw()
        {
            int count = parts.Count;
            OpenGL.glColor4f(s_Color1);
            if (cut == -1)
            {
                Vector[] array = new Vector[count];
                for (int i = 0; i < count; i++)
                {
                    ConstraintedPoint constraintedPoint = parts[i];
                    array[i] = constraintedPoint.pos;
                }
                OpenGL.glLineWidth(lineWidth);
                drawBungee(this, array, count, 4);
                OpenGL.glLineWidth(1.0);
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
                    if (!constraintedPoint2.hasConstraintTo(p))
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
            OpenGL.glLineWidth(lineWidth);
            int num2 = count - num;
            if (num2 > 0)
            {
                drawBungee(this, array2, num2, 4);
            }
            if (num > 0 && !hideTailParts)
            {
                drawBungee(this, array3, num, 4);
            }
            OpenGL.glLineWidth(1.0);
        }

        public override void dealloc()
        {
            base.dealloc();
        }

        private const float ROLLBACK_K = 0.5f;

        private const int BUNGEE_BEZIER_POINTS = 4;

        public const int BUNGEE_RELAXION_TIMES = 30;

        private const float MAX_BUNGEE_SEGMENTS = 20f;

        private const float DEFAULT_PART_WEIGHT = 0.02f;

        private const float STRENGTHENED_PART_WEIGHT = 0.5f;

        private const float CUT_DISSAPPEAR_TIMEOUT = 2f;

        private const float WHITE_TIMEOUT = 0.05f;

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

        private static RGBAColor[] ccolors =
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

        private static RGBAColor[] ccolors2 =
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
            BUNGEE_MODE_NORMAL,
            BUNGEE_MODE_LOCKED
        }
    }
}
