using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintSystem : NSObject
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                this.relaxationTimes = 1;
                this.parts = new List<ConstraintedPoint>();
            }
            return this;
        }

        public virtual void addPart(ConstraintedPoint cp)
        {
            this.parts.Add(cp);
        }

        public virtual void addPartAt(ConstraintedPoint cp, int p)
        {
            this.parts.Insert(p, cp);
        }

        public virtual void update(float delta)
        {
            int count = this.parts.Count;
            for (int i = 0; i < count; i++)
            {
                ConstraintedPoint constraintedPoint = this.parts[i];
                constraintedPoint?.update(delta);
            }
            int count2 = this.parts.Count;
            for (int j = 0; j < this.relaxationTimes; j++)
            {
                for (int k = 0; k < count2; k++)
                {
                    ConstraintedPoint.satisfyConstraints(this.parts[k]);
                }
            }
        }

        public virtual void draw()
        {
            throw new NotImplementedException();
        }

        public override void dealloc()
        {
            this.parts = null;
            base.dealloc();
        }

        public List<ConstraintedPoint> parts;

        public int relaxationTimes;
    }
}
