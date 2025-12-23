using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

namespace CutTheRope.Framework.Helpers
{
    internal class Mover : FrameworkTypes
    {
        public Mover(int l, float m_, float r_)
        {
            int num = (int)m_;
            int num2 = (int)r_;
            pathLen = 0;
            pathCapacity = l;
            rotateSpeed = num2;
            if (pathCapacity > 0)
            {
                path = new Vector[pathCapacity];
                for (int i = 0; i < path.Length; i++)
                {
                    path[i] = default;
                }
                moveSpeed = new float[pathCapacity];
                for (int j = 0; j < moveSpeed.Length; j++)
                {
                    moveSpeed[j] = num;
                }
            }
            paused = false;
        }

        public virtual void SetMoveSpeed(float ms)
        {
            for (int i = 0; i < pathCapacity; i++)
            {
                moveSpeed[i] = ms;
            }
        }

        public virtual void SetPathFromStringandStart(string p, Vector s)
        {
            if (p.CharacterAtIndex(0) == 'R')
            {
                bool flag = p.CharacterAtIndex(1) == 'C';
                int num = p.SubstringFromIndex(2).IntValue();
                int num2 = num / 2;
                float num3 = (float)(6.283185307179586 / num2);
                if (!flag)
                {
                    num3 = 0f - num3;
                }
                float num4 = 0f;
                for (int i = 0; i < num2; i++)
                {
                    float x = s.x + (num * (float)Math.Cos((double)num4));
                    float y = s.y + (num * (float)Math.Sin((double)num4));
                    AddPathPoint(Vect(x, y));
                    num4 += num3;
                }
                return;
            }
            AddPathPoint(s);
            if (p.CharacterAtIndex(p.Length() - 1) == ',')
            {
                p = p.SubstringToIndex(p.Length() - 1);
            }
            List<string> list = p.ComponentsSeparatedByString(',');
            for (int j = 0; j < list.Count; j += 2)
            {
                string nSString2 = list[j];
                string nSString3 = list[j + 1];
                AddPathPoint(Vect(s.x + nSString2.FloatValue(), s.y + nSString3.FloatValue()));
            }
        }

        public virtual void AddPathPoint(Vector v)
        {
            Vector[] array = path;
            int num = pathLen;
            pathLen = num + 1;
            array[num] = v;
        }

        public virtual void Start()
        {
            if (pathLen > 0)
            {
                pos = path[0];
                targetPoint = 1;
                CalculateOffset();
            }
        }

        public virtual void Pause()
        {
            paused = true;
        }

        public virtual void Unpause()
        {
            paused = false;
        }

        public virtual void SetRotateSpeed(float rs)
        {
            rotateSpeed = rs;
        }

        public virtual void JumpToPoint(int p)
        {
            targetPoint = p;
            pos = path[targetPoint];
            CalculateOffset();
        }

        public virtual void CalculateOffset()
        {
            Vector v = path[targetPoint];
            offset = VectMult(VectNormalize(VectSub(v, pos)), moveSpeed[targetPoint]);
        }

        public virtual void SetMoveSpeedforPoint(float ms, int i)
        {
            moveSpeed[i] = ms;
        }

        public virtual void SetMoveReverse(bool r)
        {
            reverse = r;
        }

        public virtual void Update(float delta)
        {
            if (paused)
            {
                return;
            }
            if (pathLen > 0)
            {
                float timeRemaining = delta;
                if (overrun != 0f)
                {
                    timeRemaining += overrun;
                    overrun = 0f;
                }
                int noProgressSteps = 0;
                int maxNoProgressSteps = pathLen + 1;
                while (timeRemaining > 0f)
                {
                    Vector v = path[targetPoint];
                    Vector toTarget = VectSub(v, pos);
                    float distance = VectLength(toTarget);
                    if (distance <= 0f)
                    {
                        AdvanceTarget();
                        CalculateOffset();
                        noProgressSteps++;
                        if (noProgressSteps > maxNoProgressSteps)
                        {
                            break;
                        }
                        continue;
                    }
                    noProgressSteps = 0;
                    float speed = moveSpeed[targetPoint];
                    if (speed <= 0f)
                    {
                        break;
                    }
                    float timeToTarget = distance / speed;
                    if (timeToTarget <= timeRemaining)
                    {
                        pos = v;
                        timeRemaining -= timeToTarget;
                        AdvanceTarget();
                        CalculateOffset();
                        continue;
                    }
                    Vector dir = VectMult(toTarget, 1f / distance);
                    pos = VectAdd(pos, VectMult(dir, speed * timeRemaining));
                    timeRemaining = 0f;
                }
                if (timeRemaining > 0f)
                {
                    overrun = timeRemaining;
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

        private void AdvanceTarget()
        {
            if (reverse)
            {
                targetPoint--;
                if (targetPoint < 0)
                {
                    targetPoint = pathLen - 1;
                }
                return;
            }
            targetPoint++;
            if (targetPoint >= pathLen)
            {
                targetPoint = 0;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                path = null;
                moveSpeed = null;
            }
            base.Dispose(disposing);
        }

        public static bool MoveVariableToTarget(ref float v, double t, double speed, double delta)
        {
            return MoveVariableToTarget(ref v, (float)t, (float)speed, (float)delta);
        }

        public static bool MoveVariableToTarget(ref float v, float t, float speed, float delta)
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

        private readonly int pathCapacity;

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
