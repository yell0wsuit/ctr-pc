using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Sfe
{
    internal class MaterialPoint : FrameworkTypes
    {
        public MaterialPoint()
        {
            forces = new Vector[10];
            SetWeight(1f);
            ResetAll();
        }

        public virtual void SetWeight(float weightValue)
        {
            weight = weightValue;
            invWeight = 1 / weight;
            gravity = Vect(0f, PhysicsConstants.GravityEarthY * weight);
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

        public virtual void SetForcewithID(Vector force, int index)
        {
            forces[index] = force;
            if (index > highestForceIndex)
            {
                highestForceIndex = index;
            }
        }

        public virtual void DeleteForce(int index)
        {
            forces[index] = vectZero;
        }

        public virtual Vector GetForce(int index)
        {
            return forces[index];
        }

        public virtual void ApplyImpulseDelta(Vector impulse, float delta)
        {
            if (!VectEqual(impulse, vectZero))
            {
                Vector impulseDelta = VectMult(impulse, delta / PhysicsConstants.TimeScale);
                pos = VectAdd(pos, impulseDelta);
            }
        }

        public virtual void UpdatewithPrecision(float delta, float precision)
        {
            int numIterations = (int)(delta / precision) + 1;
            if (numIterations != 0)
            {
                delta /= numIterations;
            }
            for (int i = 0; i < numIterations; i++)
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
            a = VectMult(totalForce, delta / PhysicsConstants.TimeScale);
            v = VectAdd(v, a);
            posDelta = VectMult(v, delta / PhysicsConstants.TimeScale);
            pos = VectAdd(pos, posDelta);
        }

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
