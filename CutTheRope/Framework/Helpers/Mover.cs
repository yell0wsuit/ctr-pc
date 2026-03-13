using System;
using System.Globalization;

using CutTheRope.Framework.Core;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.Framework.Helpers
{
    internal class Mover : FrameworkTypes
    {
        public Mover(int l, float m_, float r_)
        {
            int defaultMoveSpeed = (int)m_;
            int defaultRotateSpeed = (int)r_;
            pathLen = 0;
            pathCapacity = l;
            rotateSpeed = defaultRotateSpeed;
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
                    moveSpeed[j] = defaultMoveSpeed;
                }
            }
            IsPaused = false;
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
            if (p[0] == 'R')
            {
                bool clockwise = p[1] == 'C';
                int radius = ParseIntOrZero(p[2..]);
                int pointsCount = radius / 2;
                if (pointsCount <= 0)
                {
                    AddPathPoint(s);
                    return;
                }
                float angleStep = MathF.Tau / pointsCount;
                if (!clockwise)
                {
                    angleStep = 0f - angleStep;
                }
                float theta = 0f;
                for (int i = 0; i < pointsCount; i++)
                {
                    float x = s.X + (radius * MathF.Cos(theta));
                    float y = s.Y + (radius * MathF.Sin(theta));
                    AddPathPoint(Vect(x, y));
                    theta += angleStep;
                }
                return;
            }
            AddPathPoint(s);
            if (p[^1] == ',')
            {
                p = p[..(p.Length - 1)];
            }
            string[] list = p.Split(',');
            for (int j = 0; j < list.Length; j += 2)
            {
                string xOffsetString = list[j];
                string yOffsetString = list[j + 1];
                AddPathPoint(Vect(s.X + (string.IsNullOrEmpty(xOffsetString) ? 0f : float.Parse(xOffsetString, CultureInfo.InvariantCulture)), s.Y + (string.IsNullOrEmpty(yOffsetString) ? 0f : float.Parse(yOffsetString, CultureInfo.InvariantCulture))));
            }
        }

        public virtual void AddPathPoint(Vector v)
        {
            Vector[] array = path;
            int insertIndex = pathLen;
            pathLen = insertIndex + 1;
            array[insertIndex] = v;
        }

        public virtual void Start()
        {
            if (pathLen > 0)
            {
                pos = path[0];
                targetPoint = pathLen > 1 ? 1 : 0;
                // CalculateOffset();
            }
        }

        public virtual void Pause()
        {
            IsPaused = true;
        }

        public virtual void Unpause()
        {
            IsPaused = false;
        }

        public bool IsPaused { get; private set; }

        public virtual void SetRotateSpeed(float rs)
        {
            rotateSpeed = rs;
        }

        public virtual void JumpToPoint(int p)
        {
            targetPoint = p;
            pos = path[targetPoint];
            // CalculateOffset();
        }

        //public virtual void CalculateOffset()
        //{
        // Vector v = path[targetPoint];
        // offset = VectMult(VectNormalize(VectSub(v, pos)), moveSpeed[targetPoint]);
        //}

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
            if (IsPaused)
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
                        // CalculateOffset();
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
                        // CalculateOffset();
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

        public float angle_;

        public float angle_initial;

        public bool use_angle_initial;
        public int targetPoint;

        private bool reverse;

        private float overrun;

        // private Vector offset;
    }
}
