using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintSystem : NSObject
    {
        public override NSObject Init()
        {
            if (base.Init() != null)
            {
                relaxationTimes = 1;
                parts = [];
            }
            return this;
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

        public override void Dealloc()
        {
            parts = null;
            base.Dealloc();
        }

        public List<ConstraintedPoint> parts;

        public int relaxationTimes;
    }
}
