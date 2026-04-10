using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Physics
{
    /// <summary>
    /// Base physics point that stores position, velocity, forces, and gravity state.
    /// Provides Euler-style integration used by simple simulated objects.
    /// </summary>
    internal class MaterialPoint : FrameworkTypes
    {
        /// <summary>
        /// Initializes a new <see cref="MaterialPoint"/> with default weight and zeroed state.
        /// </summary>
        public MaterialPoint()
        {
            forces = new Vector[10];
            SetWeight(1f);
            ResetAll();
        }

        /// <summary>
        /// Sets the point mass and updates cached inverse weight and gravity force.
        /// </summary>
        /// <param name="weightValue">Mass value used for force integration.</param>
        public virtual void SetWeight(float weightValue)
        {
            weight = weightValue;
            invWeight = 1 / weight;
            gravity = Vect(0f, ActivePhysicsConstants.GravityEarthY * weight);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                forces = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears all registered external forces and resets the tracked force range.
        /// </summary>
        public virtual void ResetForces()
        {
            forces = new Vector[10];
            highestForceIndex = -1;
        }

        /// <summary>
        /// Resets the point state, including forces, velocity, acceleration, and position.
        /// </summary>
        public virtual void ResetAll()
        {
            ResetForces();
            v = vectZero;
            a = vectZero;
            pos = vectZero;
            posDelta = vectZero;
            totalForce = vectZero;
        }

        /// <summary>
        /// Stores an external <paramref name="force"/> in the specified slot.
        /// </summary>
        /// <param name="force">Force vector to apply each update.</param>
        /// <param name="index">Force slot index.</param>
        public virtual void SetForcewithID(Vector force, int index)
        {
            forces[index] = force;
            if (index > highestForceIndex)
            {
                highestForceIndex = index;
            }
        }

        /// <summary>
        /// Removes the external force stored in the specified slot.
        /// </summary>
        /// <param name="index">Force slot index to clear.</param>
        public virtual void DeleteForce(int index)
        {
            forces[index] = vectZero;
        }

        /// <summary>
        /// Returns the external force stored in the specified slot.
        /// </summary>
        /// <param name="index">Force slot index.</param>
        /// <returns>Force vector currently stored in the slot.</returns>
        public virtual Vector GetForce(int index)
        {
            return forces[index];
        }

        /// <summary>
        /// Applies an instantaneous positional <paramref name="impulse"/> scaled by frame time.
        /// </summary>
        /// <param name="impulse">Impulse vector to apply.</param>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        public virtual void ApplyImpulseDelta(Vector impulse, float delta)
        {
            if (!VectEqual(impulse, vectZero))
            {
                Vector impulseDelta = VectMult(impulse, delta / ActivePhysicsConstants.TimeScale);
                pos = VectAdd(pos, impulseDelta);
            }
        }

        /// <summary>
        /// Advances the point by subdividing the frame into fixed-<paramref name="precision"/> steps.
        /// </summary>
        /// <param name="delta">Total elapsed time to simulate.</param>
        /// <param name="precision">Maximum time step size for each substep.</param>
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

        /// <summary>
        /// Integrates accumulated forces, gravity, velocity, and position for one frame.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
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
            a = VectMult(totalForce, delta / ActivePhysicsConstants.TimeScale);
            v = VectAdd(v, a);
            posDelta = VectMult(v, delta / ActivePhysicsConstants.TimeScale);
            pos = VectAdd(pos, posDelta);
        }

        /// <summary>
        /// Optional gravity override shared by all material points. Zero uses per-point gravity.
        /// </summary>
        public static Vector globalGravity;

        /// <summary>
        /// Current position.
        /// </summary>
        public Vector pos;

        /// <summary>
        /// Position delta produced during the last update step.
        /// </summary>
        public Vector posDelta;

        /// <summary>
        /// Current velocity.
        /// </summary>
        public Vector v;

        /// <summary>
        /// Current acceleration.
        /// </summary>
        public Vector a;

        /// <summary>
        /// Sum of all applied forces before integration.
        /// </summary>
        public Vector totalForce;

        /// <summary>
        /// Point mass.
        /// </summary>
        public float weight;

        /// <summary>
        /// Cached inverse of <see cref="weight"/>.
        /// </summary>
        public float invWeight;

        /// <summary>
        /// External force slots applied each update.
        /// </summary>
        public Vector[] forces;

        /// <summary>
        /// Highest force slot currently considered active.
        /// </summary>
        public int highestForceIndex;

        /// <summary>
        /// Default gravity force for this point based on its weight.
        /// </summary>
        public Vector gravity;

        /// <summary>
        /// Whether gravity should be excluded from updates.
        /// </summary>
        public bool disableGravity;
    }
}
