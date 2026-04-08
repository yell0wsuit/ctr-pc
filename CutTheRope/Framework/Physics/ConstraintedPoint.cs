using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Physics
{
    /// <summary>
    /// Verlet-style physics point that can be pinned and linked to other points through constraints.
    /// </summary>
    internal sealed class ConstraintedPoint : MaterialPoint
    {
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                constraints = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes a new constrained point with no previous position, pin, or constraints.
        /// </summary>
        public ConstraintedPoint()
        {
            prevPos = vectUndefined;
            pin = Vect(PIN_UNSET_COORDINATE, PIN_UNSET_COORDINATE);
            constraints = [];
        }

        /// <summary>
        /// Adds a constraint from this point to another point.
        /// </summary>
        /// <param name="constrainedPoint">Other point referenced by the constraint.</param>
        /// <param name="restLength">Target distance for the relationship.</param>
        /// <param name="constraintType">Constraint rule to enforce.</param>
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

        /// <summary>
        /// Removes the first constraint that targets the specified point.
        /// </summary>
        /// <param name="constrainedPoint">Target point to remove.</param>
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

        /// <summary>
        /// Removes all constraints owned by this point.
        /// </summary>
        public void RemoveConstraints()
        {
            constraints = [];
        }

        /// <summary>
        /// Redirects a constraint from one target point to another.
        /// </summary>
        /// <param name="fromPoint">Existing target point.</param>
        /// <param name="toPoint">Replacement target point.</param>
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

        /// <summary>
        /// Redirects a constraint to a new target point and updates its rest length.
        /// </summary>
        /// <param name="fromPoint">Existing target point.</param>
        /// <param name="toPoint">Replacement target point.</param>
        /// <param name="restLength">New rest length to store.</param>
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

        /// <summary>
        /// Changes the rest length for the constraint that targets the specified point.
        /// </summary>
        /// <param name="restLength">New rest length.</param>
        /// <param name="constrainedPoint">Target point whose constraint should be updated.</param>
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

        /// <summary>
        /// Returns whether this point has a constraint to the specified point.
        /// </summary>
        /// <param name="p">Point to test.</param>
        /// <returns><see langword="true" /> if a matching constraint exists; otherwise <see langword="false" />.</returns>
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

        /// <summary>
        /// Returns the stored rest length for the constraint that targets the specified point.
        /// </summary>
        /// <param name="constrainedPoint">Target point to look up.</param>
        /// <returns>The configured rest length, or <c>-1</c> when no constraint exists.</returns>
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

        /// <inheritdoc />
        public override void ResetAll()
        {
            base.ResetAll();
            prevPos = vectUndefined;
            RemoveConstraints();
        }

        /// <inheritdoc />
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
            float accelerationScale = ActivePhysicsConstants.UseMobilePhysicsModel
                ? delta * QCP_FIXED_TIMESTEP
                : delta * delta;
            a = VectMult(totalForce, accelerationScale);
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

        /// <summary>
        /// Enforces all constraints owned by the specified point, including optional pinning.
        /// </summary>
        /// <param name="constrainedPoint">Point whose constraints should be satisfied.</param>
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

        /// <summary>
        /// Updates a constrained point using the QCP timestep variant used by rope simulation.
        /// </summary>
        /// <param name="constrainedPoint">Point to update.</param>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        /// <param name="coefficient">Additional timestep scaling coefficient.</param>
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

        /// <summary>
        /// Sentinel coordinate used when a point is not pinned.
        /// </summary>
        private const float PIN_UNSET_COORDINATE = -1f;

        /// <summary>
        /// Sentinel rest length used before a constraint distance has been initialized.
        /// </summary>
        private const float MISSING_REST_LENGTH = -1f;

        /// <summary>
        /// Minimum distance used when normalizing near-overlapping constraints.
        /// </summary>
        private const float MIN_CONSTRAINT_DISTANCE = 1f;

        /// <summary>
        /// Fixed timestep multiplier used by constrained point integration.
        /// </summary>
        private const float QCP_FIXED_TIMESTEP = 0.016f;

        /// <summary>
        /// Fallback constraint direction used when two points overlap exactly.
        /// </summary>
        private static readonly Vector DEFAULT_NON_ZERO_CONSTRAINT_DIRECTION = new(1f, 1f);

        /// <summary>
        /// Previous position used by Verlet integration.
        /// </summary>
        public Vector prevPos;

        /// <summary>
        /// Optional pinned world position. An unset pin leaves the point free to move.
        /// </summary>
        public Vector pin;

        /// <summary>
        /// Outgoing constraints owned by this point.
        /// </summary>
        public List<Constraint> constraints;
    }
}
