using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.sfe
{
    internal class Constraint : NSObject
    {
        public ConstraintedPoint cp;

        public float restLength;

        public CONSTRAINT type;

        public enum CONSTRAINT
        {
            CONSTRAINT_DISTANCE,
            CONSTRAINT_NOT_MORE_THAN,
            CONSTRAINT_NOT_LESS_THAN
        }
    }
}
