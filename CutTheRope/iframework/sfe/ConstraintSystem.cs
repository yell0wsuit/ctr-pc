using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintSystem : FrameworkTypes
    {
        public ConstraintSystem()
        {
            relaxationTimes = 1;
            parts = [];
        }

        public virtual void AddPart(ConstraintedPoint cp)
        {
            parts.Add(cp);
        }

        public virtual void AddPartAt(ConstraintedPoint cp, int p)
        {
            parts.Insert(p, cp);
        }

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

        public virtual void Draw()
        {
            throw new NotImplementedException();
        }

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

        public List<ConstraintedPoint> parts;

        public int relaxationTimes;
    }
}
