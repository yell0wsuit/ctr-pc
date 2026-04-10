using System;
using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Physics
{
    /// <summary>
    /// Base collection of constrained points that updates particles and relaxes constraints.
    /// </summary>
    internal class ConstraintSystem : FrameworkTypes
    {
        /// <summary>
        /// Initializes an empty constraint system with a single relaxation pass.
        /// </summary>
        public ConstraintSystem()
        {
            relaxationTimes = 1;
            parts = [];
        }

        /// <summary>
        /// Appends a constrained point to the system.
        /// </summary>
        /// <param name="cp">Point to add.</param>
        public virtual void AddPart(ConstraintedPoint cp)
        {
            parts.Add(cp);
        }

        /// <summary>
        /// Inserts a constrained point at the specified index.
        /// </summary>
        /// <param name="cp">Point to insert.</param>
        /// <param name="p">Insertion index.</param>
        public virtual void AddPartAt(ConstraintedPoint cp, int p)
        {
            parts.Insert(p, cp);
        }

        /// <summary>
        /// Updates all points, then performs the configured number of constraint relaxation passes.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        public virtual void Update(float delta)
        {
            int count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = parts[i];
                constraintedPoint?.Update(delta);
            }
            int count2 = parts.Count;
            for (int j = 0; j < relaxationTimes; j++)
            {
                for (int k = 0; k < count2; k++)
                {
                    ConstraintedPoint.SatisfyConstraints(parts[k]);
                }
            }
        }

        /// <summary>
        /// Draws the constraint system.
        /// Derived types should override this to render their geometry.
        /// </summary>
        public virtual void Draw()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (parts != null)
                {
                    foreach (ConstraintedPoint part in parts)
                    {
                        part?.Dispose();
                    }
                    parts = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Points managed by this system.
        /// </summary>
        public List<ConstraintedPoint> parts;

        /// <summary>
        /// Number of times to run constraint relaxation after each update.
        /// </summary>
        public int relaxationTimes;
    }
}
