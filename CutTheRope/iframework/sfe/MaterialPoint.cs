using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.sfe
{
    internal class MaterialPoint : NSObject
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                forces = new Vector[10];
                setWeight(1f);
                resetAll();
            }
            return this;
        }

        public virtual void setWeight(float w)
        {
            weight = w;
            invWeight = (float)(1.0 / weight);
            gravity = vect(0f, 784f * weight);
        }

        public override void dealloc()
        {
            forces = null;
            base.dealloc();
        }

        public virtual void resetForces()
        {
            forces = new Vector[10];
            highestForceIndex = -1;
        }

        public virtual void resetAll()
        {
            resetForces();
            v = vectZero;
            a = vectZero;
            pos = vectZero;
            posDelta = vectZero;
            totalForce = vectZero;
        }

        public virtual void setForcewithID(Vector force, int n)
        {
            forces[n] = force;
            if (n > highestForceIndex)
            {
                highestForceIndex = n;
            }
        }

        public virtual void deleteForce(int n)
        {
            forces[n] = vectZero;
        }

        public virtual Vector getForce(int n)
        {
            return forces[n];
        }

        public virtual void applyImpulseDelta(Vector impulse, float delta)
        {
            if (!vectEqual(impulse, vectZero))
            {
                Vector v = vectMult(impulse, (float)((double)delta / 1.0));
                pos = vectAdd(pos, v);
            }
        }

        public virtual void updatewithPrecision(float delta, float p)
        {
            int num = (int)(delta / p) + 1;
            if (num != 0)
            {
                delta /= num;
            }
            for (int i = 0; i < num; i++)
            {
                update(delta);
            }
        }

        public virtual void update(float delta)
        {
            totalForce = vectZero;
            if (!disableGravity)
            {
                if (!vectEqual(globalGravity, vectZero))
                {
                    totalForce = vectAdd(totalForce, vectMult(globalGravity, weight));
                }
                else
                {
                    totalForce = vectAdd(totalForce, gravity);
                }
            }
            if (highestForceIndex != -1)
            {
                for (int i = 0; i <= highestForceIndex; i++)
                {
                    totalForce = vectAdd(totalForce, forces[i]);
                }
            }
            totalForce = vectMult(totalForce, invWeight);
            a = vectMult(totalForce, (float)((double)delta / 1.0));
            v = vectAdd(v, a);
            posDelta = vectMult(v, (float)((double)delta / 1.0));
            pos = vectAdd(pos, posDelta);
        }

        public virtual void drawForces()
        {
        }

        protected const double TIME_SCALE = 1.0;

        private const double PIXEL_TO_SI_METERS_K = 80.0;

        public const double GCONST = 784.0;

        private const int MAX_FORCES = 10;

        public static Vector globalGravity;

        public Vector pos;

        public Vector posDelta;

        public Vector v;

        public Vector a;

        public Vector totalForce;

        public float weight;

        public float invWeight;

        public Vector[] forces;

        public int highestForceIndex;

        public Vector gravity;

        public bool disableGravity;
    }
}
