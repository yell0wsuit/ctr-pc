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
                this.forces = new Vector[10];
                this.setWeight(1f);
                this.resetAll();
            }
            return this;
        }

        public virtual void setWeight(float w)
        {
            this.weight = w;
            this.invWeight = (float)(1.0 / (double)this.weight);
            this.gravity = CTRMathHelper.vect(0f, 784f * this.weight);
        }

        public override void dealloc()
        {
            this.forces = null;
            base.dealloc();
        }

        public virtual void resetForces()
        {
            this.forces = new Vector[10];
            this.highestForceIndex = -1;
        }

        public virtual void resetAll()
        {
            this.resetForces();
            this.v = CTRMathHelper.vectZero;
            this.a = CTRMathHelper.vectZero;
            this.pos = CTRMathHelper.vectZero;
            this.posDelta = CTRMathHelper.vectZero;
            this.totalForce = CTRMathHelper.vectZero;
        }

        public virtual void setForcewithID(Vector force, int n)
        {
            this.forces[n] = force;
            if (n > this.highestForceIndex)
            {
                this.highestForceIndex = n;
            }
        }

        public virtual void deleteForce(int n)
        {
            this.forces[n] = CTRMathHelper.vectZero;
        }

        public virtual Vector getForce(int n)
        {
            return this.forces[n];
        }

        public virtual void applyImpulseDelta(Vector impulse, float delta)
        {
            if (!CTRMathHelper.vectEqual(impulse, CTRMathHelper.vectZero))
            {
                Vector v = CTRMathHelper.vectMult(impulse, (float)((double)delta / 1.0));
                this.pos = CTRMathHelper.vectAdd(this.pos, v);
            }
        }

        public virtual void updatewithPrecision(float delta, float p)
        {
            int num = (int)(delta / p) + 1;
            if (num != 0)
            {
                delta /= (float)num;
            }
            for (int i = 0; i < num; i++)
            {
                this.update(delta);
            }
        }

        public virtual void update(float delta)
        {
            this.totalForce = CTRMathHelper.vectZero;
            if (!this.disableGravity)
            {
                if (!CTRMathHelper.vectEqual(MaterialPoint.globalGravity, CTRMathHelper.vectZero))
                {
                    this.totalForce = CTRMathHelper.vectAdd(this.totalForce, CTRMathHelper.vectMult(MaterialPoint.globalGravity, this.weight));
                }
                else
                {
                    this.totalForce = CTRMathHelper.vectAdd(this.totalForce, this.gravity);
                }
            }
            if (this.highestForceIndex != -1)
            {
                for (int i = 0; i <= this.highestForceIndex; i++)
                {
                    this.totalForce = CTRMathHelper.vectAdd(this.totalForce, this.forces[i]);
                }
            }
            this.totalForce = CTRMathHelper.vectMult(this.totalForce, this.invWeight);
            this.a = CTRMathHelper.vectMult(this.totalForce, (float)((double)delta / 1.0));
            this.v = CTRMathHelper.vectAdd(this.v, this.a);
            this.posDelta = CTRMathHelper.vectMult(this.v, (float)((double)delta / 1.0));
            this.pos = CTRMathHelper.vectAdd(this.pos, this.posDelta);
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
