using System.Collections.Generic;

using CutTheRope.iframework.core;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintedPoint : MaterialPoint
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                constraints = null;
            }
            base.Dispose(disposing);
        }

        public ConstraintedPoint()
        {
            prevPos = Vect(2.1474836E+09f, 2.1474836E+09f);
            pin = Vect(-1f, -1f);
            constraints = [];
        }

        public virtual void AddConstraintwithRestLengthofType(ConstraintedPoint c, float r, Constraint.CONSTRAINT t)
        {
            Constraint constraint = new()
            {
                cp = c,
                restLength = r,
                type = t
            };
            constraints.Add(constraint);
        }

        public virtual void RemoveConstraint(ConstraintedPoint o)
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

        public virtual void RemoveConstraints()
        {
            constraints = [];
        }

        public virtual void ChangeConstraintFromTo(ConstraintedPoint o, ConstraintedPoint n)
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

        public virtual void ChangeConstraintFromTowithRestLength(ConstraintedPoint o, ConstraintedPoint n, float l)
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

        public virtual void ChangeRestLengthToFor(float l, ConstraintedPoint n)
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

        public virtual bool HasConstraintTo(ConstraintedPoint p)
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

        public virtual float RestLengthFor(ConstraintedPoint n)
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

        public override void ResetAll()
        {
            base.ResetAll();
            prevPos = Vect(2.1474836E+09f, 2.1474836E+09f);
            RemoveConstraints();
        }

        public override void Update(float delta)
        {
            Update(delta, 1f);
        }

        public virtual void Update(float delta, float koeff)
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
            a = VectMult(totalForce, (double)delta / 1.0 * (double)delta / 1.0);
            if (prevPos.x == 2.1474836E+09f)
            {
                prevPos = pos;
            }
            posDelta.x = pos.x - prevPos.x + a.x;
            posDelta.y = pos.y - prevPos.y + a.y;
            v = VectMult(posDelta, (float)(1.0 / (double)delta));
            prevPos = pos;
            pos = VectAdd(pos, posDelta);
        }

        public static void SatisfyConstraints(ConstraintedPoint p)
        {
            if (p.pin.x != -1f)
            {
                p.pos = p.pin;
                return;
            }
            int count = p.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = p.constraints[i];
                Vector vector;
                vector.x = constraint.cp.pos.x - p.pos.x;
                vector.y = constraint.cp.pos.y - p.pos.y;
                if (vector.x == 0f && vector.y == 0f)
                {
                    vector = Vect(1f, 1f);
                }
                float num = VectLength(vector);
                float restLength = constraint.restLength;
                Constraint.CONSTRAINT type = constraint.type;

                bool shouldApplyConstraint = (type == Constraint.CONSTRAINT.DISTANCE)
                    || (type == Constraint.CONSTRAINT.NOT_MORE_THAN && num > restLength)
                    || (type == Constraint.CONSTRAINT.NOT_LESS_THAN && num < restLength);

                if (!shouldApplyConstraint)
                {
                    continue;
                }

                Vector vector2 = vector;
                float num2 = constraint.cp.invWeight;
                float num3 = num > 1f ? num : 1f;
                float num4 = (num - restLength) / (num3 * (p.invWeight + num2));
                float num5 = p.invWeight * num4;
                vector.x *= num5;
                vector.y *= num5;
                num5 = num2 * num4;
                vector2.x *= num5;
                vector2.y *= num5;
                p.pos.x += vector.x;
                p.pos.y += vector.y;
                if (constraint.cp.pin.x == -1f)
                {
                    constraint.cp.pos = VectSub(constraint.cp.pos, vector2);
                }
            }
        }

        public static void Qcpupdate(ConstraintedPoint p, float delta, float koeff)
        {
            p.totalForce = vectZero;
            if (!p.disableGravity)
            {
                p.totalForce = !VectEqual(globalGravity, vectZero)
                    ? VectAdd(p.totalForce, VectMult(globalGravity, p.weight))
                    : VectAdd(p.totalForce, p.gravity);
            }
            if (p.highestForceIndex != -1)
            {
                for (int i = 0; i <= p.highestForceIndex; i++)
                {
                    p.totalForce = VectAdd(p.totalForce, p.forces[i]);
                }
            }
            p.totalForce = VectMult(p.totalForce, p.invWeight);
            p.a = VectMult(p.totalForce, (float)((double)delta / 1.0 * 0.01600000075995922 * (double)koeff));
            if (p.prevPos.x == 2.1474836E+09f)
            {
                p.prevPos = p.pos;
            }
            p.posDelta.x = p.pos.x - p.prevPos.x + p.a.x;
            p.posDelta.y = p.pos.y - p.prevPos.y + p.a.y;
            p.v = VectMult(p.posDelta, (float)(1.0 / (double)delta));
            p.prevPos = p.pos;
            p.pos = VectAdd(p.pos, p.posDelta);
        }

        public Vector prevPos;

        public Vector pin;

        public List<Constraint> constraints;
    }
}
