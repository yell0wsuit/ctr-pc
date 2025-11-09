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
                this.pathLen = 0;
                this.pathCapacity = l;
                this.rotateSpeed = (float)num2;
                if (this.pathCapacity > 0)
                {
                    this.path = new Vector[this.pathCapacity];
                    for (int i = 0; i < this.path.Length; i++)
                    {
                        this.path[i] = default(Vector);
                    }
                    this.moveSpeed = new float[this.pathCapacity];
                    for (int j = 0; j < this.moveSpeed.Length; j++)
                    {
                        this.moveSpeed[j] = (float)num;
                    }
                }
                this.paused = false;
            }
            return this;
        }

        public virtual void setMoveSpeed(float ms)
        {
            for (int i = 0; i < this.pathCapacity; i++)
            {
                this.moveSpeed[i] = ms;
            }
        }

        public virtual void setPathFromStringandStart(NSString p, Vector s)
        {
            if (p.characterAtIndex(0) == 'R')
            {
                bool flag = p.characterAtIndex(1) == 'C';
                int num = p.substringFromIndex(2).intValue();
                int num2 = num / 2;
                float num3 = (float)(6.283185307179586 / (double)num2);
                if (!flag)
                {
                    num3 = 0f - num3;
                }
                float num4 = 0f;
                for (int i = 0; i < num2; i++)
                {
                    float x = s.x + (float)num * (float)Math.Cos((double)num4);
                    float y = s.y + (float)num * (float)Math.Sin((double)num4);
                    this.addPathPoint(CTRMathHelper.vect(x, y));
                    num4 += num3;
                }
                return;
            }
            this.addPathPoint(s);
            if (p.characterAtIndex(p.length() - 1) == ',')
            {
                p = p.substringToIndex(p.length() - 1);
            }
            List<NSString> list = p.componentsSeparatedByString(',');
            for (int j = 0; j < list.Count; j += 2)
            {
                NSString nSString2 = list[j];
                NSString nSString3 = list[j + 1];
                this.addPathPoint(CTRMathHelper.vect(s.x + nSString2.floatValue(), s.y + nSString3.floatValue()));
            }
        }

        public virtual void addPathPoint(Vector v)
        {
            Vector[] array = this.path;
            int num = this.pathLen;
            this.pathLen = num + 1;
            array[num] = v;
        }

        public virtual void start()
        {
            if (this.pathLen > 0)
            {
                this.pos = this.path[0];
                this.targetPoint = 1;
                this.calculateOffset();
            }
        }

        public virtual void pause()
        {
            this.paused = true;
        }

        public virtual void unpause()
        {
            this.paused = false;
        }

        public virtual void setRotateSpeed(float rs)
        {
            this.rotateSpeed = rs;
        }

        public virtual void jumpToPoint(int p)
        {
            this.targetPoint = p;
            this.pos = this.path[this.targetPoint];
            this.calculateOffset();
        }

        public virtual void calculateOffset()
        {
            Vector v = this.path[this.targetPoint];
            this.offset = CTRMathHelper.vectMult(CTRMathHelper.vectNormalize(CTRMathHelper.vectSub(v, this.pos)), this.moveSpeed[this.targetPoint]);
        }

        public virtual void setMoveSpeedforPoint(float ms, int i)
        {
            this.moveSpeed[i] = ms;
        }

        public virtual void setMoveReverse(bool r)
        {
            this.reverse = r;
        }

        public virtual void update(float delta)
        {
            if (this.paused)
            {
                return;
            }
            if (this.pathLen > 0)
            {
                Vector v = this.path[this.targetPoint];
                bool flag = false;
                if (!CTRMathHelper.vectEqual(this.pos, v))
                {
                    float num = delta;
                    if (this.overrun != 0f)
                    {
                        num += this.overrun;
                        this.overrun = 0f;
                    }
                    this.pos = CTRMathHelper.vectAdd(this.pos, CTRMathHelper.vectMult(this.offset, num));
                    if (!CTRMathHelper.sameSign(this.offset.x, v.x - this.pos.x) || !CTRMathHelper.sameSign(this.offset.y, v.y - this.pos.y))
                    {
                        this.overrun = CTRMathHelper.vectLength(CTRMathHelper.vectSub(this.pos, v));
                        float num2 = CTRMathHelper.vectLength(this.offset);
                        this.overrun /= num2;
                        this.pos = v;
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    if (this.reverse)
                    {
                        this.targetPoint--;
                        if (this.targetPoint < 0)
                        {
                            this.targetPoint = this.pathLen - 1;
                        }
                    }
                    else
                    {
                        this.targetPoint++;
                        if (this.targetPoint >= this.pathLen)
                        {
                            this.targetPoint = 0;
                        }
                    }
                    this.calculateOffset();
                }
            }
            if (this.rotateSpeed != 0f)
            {
                if (this.use_angle_initial && this.targetPoint == 0)
                {
                    this.angle_ = this.angle_initial;
                    return;
                }
                this.angle_ += (double)(this.rotateSpeed * delta);
            }
        }

        public override void dealloc()
        {
            this.path = null;
            this.moveSpeed = null;
            base.dealloc();
        }

        public static bool moveVariableToTarget(ref float v, double t, double speed, double delta)
        {
            return Mover.moveVariableToTarget(ref v, (float)t, (float)speed, (float)delta);
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
