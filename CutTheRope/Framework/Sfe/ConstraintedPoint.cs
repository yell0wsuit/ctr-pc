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

        public void AddConstraintwithRestLengthofType(
            ConstraintedPoint constrainedPoint,
            float restLength,
            Constraint.CONSTRAINT constraintType)
        {
            Constraint constraint = new()
            {
                cp = constrainedPoint,
                restLength = restLength,
                type = constraintType
            };
            constraints.Add(constraint);
        }

        public void RemoveConstraint(ConstraintedPoint constrainedPoint)
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                if (constraints[i].cp == constrainedPoint)
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

        public void ChangeConstraintFromTo(ConstraintedPoint fromPoint, ConstraintedPoint toPoint)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == fromPoint)
                {
                    constraint.cp = toPoint;
                    return;
                }
            }
        }

        public void ChangeConstraintFromTowithRestLength(ConstraintedPoint fromPoint, ConstraintedPoint toPoint, float restLength)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == fromPoint)
                {
                    constraint.cp = toPoint;
                    constraint.restLength = restLength;
                    return;
                }
            }
        }

        public void ChangeRestLengthToFor(float restLength, ConstraintedPoint constrainedPoint)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == constrainedPoint)
                {
                    constraint.restLength = restLength;
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

        public float RestLengthFor(ConstraintedPoint constrainedPoint)
        {
            int count = constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constraints[i];
                if (constraint != null && constraint.cp == constrainedPoint)
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

        public static void SatisfyConstraints(ConstraintedPoint constrainedPoint)
        {
            if (constrainedPoint == null)
            {
                return;
            }
            if (constrainedPoint.constraints == null)
            {
                return;
            }
            if (constrainedPoint.pin.X != PIN_UNSET_COORDINATE)
            {
                constrainedPoint.pos = constrainedPoint.pin;
                return;
            }
            int count = constrainedPoint.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = constrainedPoint.constraints[i];
                Vector deltaVector = new(
                    constraint.cp.pos.X - constrainedPoint.pos.X,
                    constraint.cp.pos.Y - constrainedPoint.pos.Y);
                if (deltaVector.X == 0f && deltaVector.Y == 0f)
                {
                    deltaVector = DEFAULT_NON_ZERO_CONSTRAINT_DIRECTION;
                }
                float deltaLength = VectLength(deltaVector);
                float restLength = constraint.restLength;
                Constraint.CONSTRAINT type = constraint.type;

                bool shouldApplyConstraint = (type == Constraint.CONSTRAINT.DISTANCE)
                    || (type == Constraint.CONSTRAINT.NOT_MORE_THAN && deltaLength > restLength)
                    || (type == Constraint.CONSTRAINT.NOT_LESS_THAN && deltaLength < restLength);

                if (!shouldApplyConstraint)
                {
                    continue;
                }

                Vector otherDeltaVector = deltaVector;
                float otherInvWeight = constraint.cp.invWeight;
                float safeDeltaLength = deltaLength > MIN_CONSTRAINT_DISTANCE ? deltaLength : MIN_CONSTRAINT_DISTANCE;
                float correctionFactor = (deltaLength - restLength) / (safeDeltaLength * (constrainedPoint.invWeight + otherInvWeight));
                float correctionScale = constrainedPoint.invWeight * correctionFactor;
                deltaVector.X *= correctionScale;
                deltaVector.Y *= correctionScale;
                correctionScale = otherInvWeight * correctionFactor;
                otherDeltaVector.X *= correctionScale;
                otherDeltaVector.Y *= correctionScale;
                constrainedPoint.pos.X += deltaVector.X;
                constrainedPoint.pos.Y += deltaVector.Y;
                if (constraint.cp.pin.X == PIN_UNSET_COORDINATE)
                {
                    constraint.cp.pos = VectSub(constraint.cp.pos, otherDeltaVector);
                }
            }
        }

        public static void Qcpupdate(ConstraintedPoint constrainedPoint, float delta, float coefficient)
        {
            constrainedPoint.totalForce = vectZero;
            if (!constrainedPoint.disableGravity)
            {
                constrainedPoint.totalForce = !VectEqual(globalGravity, vectZero)
                    ? VectAdd(constrainedPoint.totalForce, VectMult(globalGravity, constrainedPoint.weight))
                    : VectAdd(constrainedPoint.totalForce, constrainedPoint.gravity);
            }
            if (constrainedPoint.highestForceIndex != -1)
            {
                for (int i = 0; i <= constrainedPoint.highestForceIndex; i++)
                {
                    constrainedPoint.totalForce = VectAdd(constrainedPoint.totalForce, constrainedPoint.forces[i]);
                }
            }
            constrainedPoint.totalForce = VectMult(constrainedPoint.totalForce, constrainedPoint.invWeight);
            constrainedPoint.a = VectMult(constrainedPoint.totalForce, delta * QCP_FIXED_TIMESTEP * coefficient);
            if (constrainedPoint.prevPos.X == UNDEFINED_COORDINATE)
            {
                constrainedPoint.prevPos = constrainedPoint.pos;
            }
            constrainedPoint.posDelta.X = constrainedPoint.pos.X - constrainedPoint.prevPos.X + constrainedPoint.a.X;
            constrainedPoint.posDelta.Y = constrainedPoint.pos.Y - constrainedPoint.prevPos.Y + constrainedPoint.a.Y;
            constrainedPoint.v = VectMult(constrainedPoint.posDelta, 1f / delta);
            constrainedPoint.prevPos = constrainedPoint.pos;
            constrainedPoint.pos = VectAdd(constrainedPoint.pos, constrainedPoint.posDelta);
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
