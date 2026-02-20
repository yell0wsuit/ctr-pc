using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Sfe
{
    internal sealed class ConstraintedPoint : MaterialPoint
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
            prevPos = vectUndefined;
            pin = Vect(PIN_UNSET_COORDINATE, PIN_UNSET_COORDINATE);
            constraints = [];
        }

        public void AddConstraintwithRestLengthofType(ConstraintedPoint c, float r, Constraint.CONSTRAINT t)
        {
            Constraint constraint = new()
            {
                cp = c,
                restLength = r,
                type = t
            };
            constraints.Add(constraint);
        }

        public void RemoveConstraint(ConstraintedPoint o)
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

        public void RemoveConstraints()
        {
            constraints = [];
        }

        public void ChangeConstraintFromTo(ConstraintedPoint o, ConstraintedPoint n)
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

        public void ChangeConstraintFromTowithRestLength(ConstraintedPoint o, ConstraintedPoint n, float l)
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

        public void ChangeRestLengthToFor(float l, ConstraintedPoint n)
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

        public bool HasConstraintTo(ConstraintedPoint p)
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

        public float RestLengthFor(ConstraintedPoint n)
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
            return MISSING_REST_LENGTH;
        }

        public override void ResetAll()
        {
            base.ResetAll();
            prevPos = vectUndefined;
            RemoveConstraints();
        }

        public override void Update(float delta)
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
            a = VectMult(totalForce, delta * delta);
            if (prevPos.X == UNDEFINED_COORDINATE)
            {
                prevPos = pos;
            }
            posDelta.X = pos.X - prevPos.X + a.X;
            posDelta.Y = pos.Y - prevPos.Y + a.Y;
            v = VectMult(posDelta, 1f / delta);
            prevPos = pos;
            pos = VectAdd(pos, posDelta);
        }

        public static void SatisfyConstraints(ConstraintedPoint p)
        {
            if (p == null)
            {
                return;
            }
            if (p.constraints == null)
            {
                return;
            }
            if (p.pin.X != PIN_UNSET_COORDINATE)
            {
                p.pos = p.pin;
                return;
            }
            int count = p.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = p.constraints[i];
                Vector vector = new(
                    constraint.cp.pos.X - p.pos.X,
                    constraint.cp.pos.Y - p.pos.Y);
                if (vector.X == 0f && vector.Y == 0f)
                {
                    vector = DEFAULT_NON_ZERO_CONSTRAINT_DIRECTION;
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
                float num3 = num > MIN_CONSTRAINT_DISTANCE ? num : MIN_CONSTRAINT_DISTANCE;
                float num4 = (num - restLength) / (num3 * (p.invWeight + num2));
                float num5 = p.invWeight * num4;
                vector.X *= num5;
                vector.Y *= num5;
                num5 = num2 * num4;
                vector2.X *= num5;
                vector2.Y *= num5;
                p.pos.X += vector.X;
                p.pos.Y += vector.Y;
                if (constraint.cp.pin.X == PIN_UNSET_COORDINATE)
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
            p.a = VectMult(p.totalForce, delta * QCP_FIXED_TIMESTEP * koeff);
            if (p.prevPos.X == UNDEFINED_COORDINATE)
            {
                p.prevPos = p.pos;
            }
            p.posDelta.X = p.pos.X - p.prevPos.X + p.a.X;
            p.posDelta.Y = p.pos.Y - p.prevPos.Y + p.a.Y;
            p.v = VectMult(p.posDelta, 1f / delta);
            p.prevPos = p.pos;
            p.pos = VectAdd(p.pos, p.posDelta);
        }

        private const float PIN_UNSET_COORDINATE = -1f;
        private const float MISSING_REST_LENGTH = -1f;
        private const float MIN_CONSTRAINT_DISTANCE = 1f;
        private const float QCP_FIXED_TIMESTEP = 0.016f;
        private static readonly Vector DEFAULT_NON_ZERO_CONSTRAINT_DIRECTION = new(1f, 1f);

        public Vector prevPos;

        public Vector pin;

        public List<Constraint> constraints;
    }
}
