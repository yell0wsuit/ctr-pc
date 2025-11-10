using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal class Mover : NSObject
    {
        public virtual Mover initWithPathCapacityMoveSpeedRotateSpeed(int l, float m_, float r_)
        {
            int num = (int)m_;
            int num2 = (int)r_;
            if (base.init() != null)
            {
                pathLen = 0;
                pathCapacity = l;
                rotateSpeed = num2;
                if (pathCapacity > 0)
                {
                    path = new Vector[pathCapacity];
                    for (int i = 0; i < path.Length; i++)
                    {
                        path[i] = default(Vector);
                    }
                    moveSpeed = new float[pathCapacity];
                    for (int j = 0; j < moveSpeed.Length; j++)
                    {
                        moveSpeed[j] = num;
                    }
                }
                paused = false;
            }
            return this;
        }

        public virtual void setMoveSpeed(float ms)
        {
            for (int i = 0; i < pathCapacity; i++)
            {
                moveSpeed[i] = ms;
            }
        }

        public virtual void setPathFromStringandStart(NSString p, Vector s)
        {
            if (p.characterAtIndex(0) == 'R')
            {
                bool flag = p.characterAtIndex(1) == 'C';
                int num = p.substringFromIndex(2).intValue();
                int num2 = num / 2;
                float num3 = (float)(6.283185307179586 / num2);
                if (!flag)
                {
                    num3 = 0f - num3;
                }
                float num4 = 0f;
                for (int i = 0; i < num2; i++)
                {
                    float x = s.x + num * (float)Math.Cos((double)num4);
                    float y = s.y + num * (float)Math.Sin((double)num4);
                    addPathPoint(vect(x, y));
                    num4 += num3;
                }
                return;
            }
            addPathPoint(s);
            if (p.characterAtIndex(p.length() - 1) == ',')
            {
                p = p.substringToIndex(p.length() - 1);
            }
            List<NSString> list = p.componentsSeparatedByString(',');
            for (int j = 0; j < list.Count; j += 2)
            {
                NSString nSString2 = list[j];
                NSString nSString3 = list[j + 1];
                addPathPoint(vect(s.x + nSString2.floatValue(), s.y + nSString3.floatValue()));
            }
        }

        public virtual void addPathPoint(Vector v)
        {
            Vector[] array = path;
            int num = pathLen;
            pathLen = num + 1;
            array[num] = v;
        }

        public virtual void start()
        {
            if (pathLen > 0)
            {
                pos = path[0];
                targetPoint = 1;
                calculateOffset();
            }
        }

        public virtual void pause()
        {
            paused = true;
        }

        public virtual void unpause()
        {
            paused = false;
        }

        public virtual void setRotateSpeed(float rs)
        {
            rotateSpeed = rs;
        }

        public virtual void jumpToPoint(int p)
        {
            targetPoint = p;
            pos = path[targetPoint];
            calculateOffset();
        }

        public virtual void calculateOffset()
        {
            Vector v = path[targetPoint];
            offset = vectMult(vectNormalize(vectSub(v, pos)), moveSpeed[targetPoint]);
        }

        public virtual void setMoveSpeedforPoint(float ms, int i)
        {
            moveSpeed[i] = ms;
        }

        public virtual void setMoveReverse(bool r)
        {
            reverse = r;
        }

        public virtual void update(float delta)
        {
            if (paused)
            {
                return;
            }
            if (pathLen > 0)
            {
                Vector v = path[targetPoint];
                bool flag = false;
                if (!vectEqual(pos, v))
                {
                    float num = delta;
                    if (overrun != 0f)
                    {
                        num += overrun;
                        overrun = 0f;
                    }
                    pos = vectAdd(pos, vectMult(offset, num));
                    if (!sameSign(offset.x, v.x - pos.x) || !sameSign(offset.y, v.y - pos.y))
                    {
                        overrun = vectLength(vectSub(pos, v));
                        float num2 = vectLength(offset);
                        overrun /= num2;
                        pos = v;
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    if (reverse)
                    {
                        targetPoint--;
                        if (targetPoint < 0)
                        {
                            targetPoint = pathLen - 1;
                        }
                    }
                    else
                    {
                        targetPoint++;
                        if (targetPoint >= pathLen)
                        {
                            targetPoint = 0;
                        }
                    }
                    calculateOffset();
                }
            }
            if (rotateSpeed != 0f)
            {
                if (use_angle_initial && targetPoint == 0)
                {
                    angle_ = angle_initial;
                    return;
                }
                angle_ += rotateSpeed * delta;
            }
        }

        public override void dealloc()
        {
            path = null;
            moveSpeed = null;
            base.dealloc();
        }

        public static bool moveVariableToTarget(ref float v, double t, double speed, double delta)
        {
            return moveVariableToTarget(ref v, (float)t, (float)speed, (float)delta);
        }

        public static bool moveVariableToTarget(ref float v, float t, float speed, float delta)
        {
            if (t != v)
            {
                if (t > v)
                {
                    v += speed * delta;
                    if (v > t)
                    {
                        v = t;
                    }
                }
                else
                {
                    v -= speed * delta;
                    if (v < t)
                    {
                        v = t;
                    }
                }
                if (t == v)
                {
                    return true;
                }
            }
            return false;
        }

        private float[] moveSpeed;

        private float rotateSpeed;

        public Vector[] path;

        public int pathLen;

        private int pathCapacity;

        public Vector pos;

        public double angle_;

        public double angle_initial;

        public bool use_angle_initial;

        private bool paused;

        public int targetPoint;

        private bool reverse;

        private float overrun;

        private Vector offset;
    }
}
