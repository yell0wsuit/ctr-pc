using CutTheRope.iframework.core;

namespace CutTheRope.iframework.sfe
{
    internal class MaterialPoint : FrameworkTypes
    {
        public MaterialPoint()
        {
            forces = new Vector[10];
            SetWeight(1f);
            ResetAll();
        }

        public virtual void SetWeight(float w)
        {
            weight = w;
            invWeight = (float)(1.0 / weight);
            gravity = Vect(0f, 784f * weight);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                forces = null;
            }
            base.Dispose(disposing);
        }

        public virtual void ResetForces()
        {
            forces = new Vector[10];
            highestForceIndex = -1;
        }

        public virtual void ResetAll()
        {
            ResetForces();
            v = vectZero;
            a = vectZero;
            pos = vectZero;
            posDelta = vectZero;
            totalForce = vectZero;
        }

        public virtual void SetForcewithID(Vector force, int n)
        {
            forces[n] = force;
            if (n > highestForceIndex)
            {
                highestForceIndex = n;
            }
        }

        public virtual void DeleteForce(int n)
        {
            forces[n] = vectZero;
        }

        public virtual Vector GetForce(int n)
        {
            return forces[n];
        }

        public virtual void ApplyImpulseDelta(Vector impulse, float delta)
        {
            if (!VectEqual(impulse, vectZero))
            {
                Vector v = VectMult(impulse, (float)((double)delta / 1.0));
                pos = VectAdd(pos, v);
            }
        }

        public virtual void UpdatewithPrecision(float delta, float p)
        {
            int num = (int)(delta / p) + 1;
            if (num != 0)
            {
                delta /= num;
            }
            for (int i = 0; i < num; i++)
            {
                Update(delta);
            }
        }

        public virtual void Update(float delta)
        {
            totalForce = vectZero;
            if (!disableGravity)
            {
                totalForce = !VectEqual(globalGravity, vectZero) ? VectAdd(totalForce, VectMult(globalGravity, weight)) : VectAdd(totalForce, gravity);
            }
            if (highestForceIndex != -1)
            {
                for (int i = 0; i <= highestForceIndex; i++)
                {
                    totalForce = VectAdd(totalForce, forces[i]);
                }
            }
            totalForce = VectMult(totalForce, invWeight);
            a = VectMult(totalForce, (float)((double)delta / 1.0));
            v = VectAdd(v, a);
            posDelta = VectMult(v, (float)((double)delta / 1.0));
            pos = VectAdd(pos, posDelta);
        }

        public virtual void DrawForces()
        {
        }

        protected const double TIME_SCALE = 1.0;
        public const double GCONST = 784.0;
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
