using CutTheRope.iframework.core;
using CutTheRope.ios;
using System.Collections.Generic;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintedPoint : MaterialPoint
    {
        public override void dealloc()
        {
            constraints = null;
            base.dealloc();
        }

        public override NSObject init()
        {
            if (base.init() != null)
            {
                prevPos = vect(2.1474836E+09f, 2.1474836E+09f);
                pin = vect(-1f, -1f);
                constraints = [];
            }
            return this;
        }

        public virtual void addConstraintwithRestLengthofType(ConstraintedPoint c, float r, Constraint.CONSTRAINT t)
        {
            Constraint constraint = new();
            constraint.init();
            constraint.cp = c;
            constraint.restLength = r;
            constraint.type = t;
            constraints.Add(constraint);
        }

        public virtual void removeConstraint(ConstraintedPoint o)
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                if (constraints[i].cp == o)
                {
                    constraints.RemoveAt(i);
                    return;
                }
            }
        }

        public virtual void removeConstraints()
        {
            constraints = [];
        }

        public virtual void changeConstraintFromTo(ConstraintedPoint o, ConstraintedPoint n)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == o)
                {
                    constraint.cp = n;
                    return;
                }
            }
        }

        public virtual void changeConstraintFromTowithRestLength(ConstraintedPoint o, ConstraintedPoint n, float l)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == o)
                {
                    constraint.cp = n;
                    constraint.restLength = l;
                    return;
                }
            }
        }

        public virtual void changeRestLengthToFor(float l, ConstraintedPoint n)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == n)
                {
                    constraint.restLength = l;
                    return;
                }
            }
        }

        public virtual bool hasConstraintTo(ConstraintedPoint p)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == p)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual float restLengthFor(ConstraintedPoint n)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == n)
                {
                    return constraint.restLength;
                }
            }
            return -1f;
        }

        public override void resetAll()
        {
            base.resetAll();
            prevPos = vect(2.1474836E+09f, 2.1474836E+09f);
            removeConstraints();
        }

        public override void update(float delta)
        {
            update(delta, 1f);
        }

        public virtual void update(float delta, float koeff)
        {
            totalForce = vectZero;
            if (!disableGravity)
            {
                totalForce = !vectEqual(globalGravity, vectZero) ? vectAdd(totalForce, vectMult(globalGravity, weight)) : vectAdd(totalForce, gravity);
            }
            if (highestForceIndex != -1)
            {
                for (int i = 0; i <= highestForceIndex; i++)
                {
                    totalForce = vectAdd(totalForce, forces[i]);
                }
            }
            totalForce = vectMult(totalForce, invWeight);
            a = vectMult(totalForce, (double)delta / 1.0 * (double)delta / 1.0);
            if (prevPos.x == 2.1474836E+09f)
            {
                prevPos = pos;
            }
            posDelta.x = pos.x - prevPos.x + a.x;
            posDelta.y = pos.y - prevPos.y + a.y;
            v = vectMult(posDelta, (float)(1.0 / (double)delta));
            prevPos = pos;
            pos = vectAdd(pos, posDelta);
        }

        public static void satisfyConstraints(ConstraintedPoint p)
        {
            if (p.pin.x != -1f)
            {
                p.pos = p.pin;
                return;
            }
            int count = p.constraints.Count;
            Vector vector = vectZero;
            Vector vector2 = vectZero;
            int i = 0;
            while (i < count)
            {
                Constraint constraint = p.constraints[i];
                vector.x = constraint.cp.pos.x - p.pos.x;
                vector.y = constraint.cp.pos.y - p.pos.y;
                if (vector.x == 0f && vector.y == 0f)
                {
                    vector = vect(1f, 1f);
                }
                float num = vectLength(vector);
                float restLength = constraint.restLength;
                Constraint.CONSTRAINT type = constraint.type;
                if (type != Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN)
                {
                    if (type != Constraint.CONSTRAINT.CONSTRAINT_NOT_LESS_THAN)
                    {
                        goto IL_00F8;
                    }
                    if (num < restLength)
                    {
                        goto IL_00F8;
                    }
                }
                else if (num > restLength)
                {
                    goto IL_00F8;
                }
            IL_01D6:
                i++;
                continue;
            IL_00F8:
                vector2 = vector;
                float num2 = constraint.cp.invWeight;
                float num3 = (num > 1f) ? num : 1f;
                float num4 = (num - restLength) / (num3 * (p.invWeight + num2));
                float num5 = p.invWeight * num4;
                vector.x *= num5;
                vector.y *= num5;
                num5 = num2 * num4;
                vector2.x *= num5;
                vector2.y *= num5;
                p.pos.x = p.pos.x + vector.x;
                p.pos.y = p.pos.y + vector.y;
                if (constraint.cp.pin.x == -1f)
                {
                    constraint.cp.pos = vectSub(constraint.cp.pos, vector2);
                    goto IL_01D6;
                }
                goto IL_01D6;
            }
        }

        public static void qcpupdate(ConstraintedPoint p, float delta, float koeff)
        {
            p.totalForce = vectZero;
            if (!p.disableGravity)
            {
                p.totalForce = !vectEqual(globalGravity, vectZero)
                    ? vectAdd(p.totalForce, vectMult(globalGravity, p.weight))
                    : vectAdd(p.totalForce, p.gravity);
            }
            if (p.highestForceIndex != -1)
            {
                for (int i = 0; i <= p.highestForceIndex; i++)
                {
                    p.totalForce = vectAdd(p.totalForce, p.forces[i]);
                }
            }
            p.totalForce = vectMult(p.totalForce, p.invWeight);
            p.a = vectMult(p.totalForce, (float)((double)delta / 1.0 * 0.01600000075995922 * (double)koeff));
            if (p.prevPos.x == 2.1474836E+09f)
            {
                p.prevPos = p.pos;
            }
            p.posDelta.x = p.pos.x - p.prevPos.x + p.a.x;
            p.posDelta.y = p.pos.y - p.prevPos.y + p.a.y;
            p.v = vectMult(p.posDelta, (float)(1.0 / (double)delta));
            p.prevPos = p.pos;
            p.pos = vectAdd(p.pos, p.posDelta);
        }

        public Vector prevPos;

        public Vector pin;

        public List<Constraint> constraints;
    }
}
