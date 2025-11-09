using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.sfe
{
    internal class ConstraintedPoint : MaterialPoint
    {
        public override void dealloc()
        {
            this.constraints = null;
            base.dealloc();
        }

        public override NSObject init()
        {
            if (base.init() != null)
            {
                this.prevPos = MathHelper.vect(2.1474836E+09f, 2.1474836E+09f);
                this.pin = MathHelper.vect(-1f, -1f);
                this.constraints = new List<Constraint>();
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
            this.constraints.Add(constraint);
        }

        public virtual void removeConstraint(ConstraintedPoint o)
        {
            for (int i = 0; i < this.constraints.Count; i++)
            {
                if (this.constraints[i].cp == o)
                {
                    this.constraints.RemoveAt(i);
                    return;
                }
            }
        }

        public virtual void removeConstraints()
        {
            this.constraints = new List<Constraint>();
        }

        public virtual void changeConstraintFromTo(ConstraintedPoint o, ConstraintedPoint n)
        {
            int count = this.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = this.constraints[i];
                if (constraint != null && constraint.cp == o)
                {
                    constraint.cp = n;
                    return;
                }
            }
        }

        public virtual void changeConstraintFromTowithRestLength(ConstraintedPoint o, ConstraintedPoint n, float l)
        {
            int count = this.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = this.constraints[i];
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
            int count = this.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = this.constraints[i];
                if (constraint != null && constraint.cp == n)
                {
                    constraint.restLength = l;
                    return;
                }
            }
        }

        public virtual bool hasConstraintTo(ConstraintedPoint p)
        {
            int count = this.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = this.constraints[i];
                if (constraint != null && constraint.cp == p)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual float restLengthFor(ConstraintedPoint n)
        {
            int count = this.constraints.Count;
            for (int i = 0; i < count; i++)
            {
                Constraint constraint = this.constraints[i];
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
            this.prevPos = MathHelper.vect(2.1474836E+09f, 2.1474836E+09f);
            this.removeConstraints();
        }

        public override void update(float delta)
        {
            this.update(delta, 1f);
        }

        public virtual void update(float delta, float koeff)
        {
            this.totalForce = MathHelper.vectZero;
            if (!this.disableGravity)
            {
                if (!MathHelper.vectEqual(MaterialPoint.globalGravity, MathHelper.vectZero))
                {
                    this.totalForce = MathHelper.vectAdd(this.totalForce, MathHelper.vectMult(MaterialPoint.globalGravity, this.weight));
                }
                else
                {
                    this.totalForce = MathHelper.vectAdd(this.totalForce, this.gravity);
                }
            }
            if (this.highestForceIndex != -1)
            {
                for (int i = 0; i <= this.highestForceIndex; i++)
                {
                    this.totalForce = MathHelper.vectAdd(this.totalForce, this.forces[i]);
                }
            }
            this.totalForce = MathHelper.vectMult(this.totalForce, this.invWeight);
            this.a = MathHelper.vectMult(this.totalForce, (double)delta / 1.0 * (double)delta / 1.0);
            if (this.prevPos.x == 2.1474836E+09f)
            {
                this.prevPos = this.pos;
            }
            this.posDelta.x = this.pos.x - this.prevPos.x + this.a.x;
            this.posDelta.y = this.pos.y - this.prevPos.y + this.a.y;
            this.v = MathHelper.vectMult(this.posDelta, (float)(1.0 / (double)delta));
            this.prevPos = this.pos;
            this.pos = MathHelper.vectAdd(this.pos, this.posDelta);
        }

        public static void satisfyConstraints(ConstraintedPoint p)
        {
            if (p.pin.x != -1f)
            {
                p.pos = p.pin;
                return;
            }
            int count = p.constraints.Count;
            Vector vector = MathHelper.vectZero;
            Vector vector2 = MathHelper.vectZero;
            int i = 0;
            while (i < count)
            {
                Constraint constraint = p.constraints[i];
                vector.x = constraint.cp.pos.x - p.pos.x;
                vector.y = constraint.cp.pos.y - p.pos.y;
                if (vector.x == 0f && vector.y == 0f)
                {
                    vector = MathHelper.vect(1f, 1f);
                }
                float num = MathHelper.vectLength(vector);
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
                float num3 = ((num > 1f) ? num : 1f);
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
                    constraint.cp.pos = MathHelper.vectSub(constraint.cp.pos, vector2);
                    goto IL_01D6;
                }
                goto IL_01D6;
            }
        }

        public static void qcpupdate(ConstraintedPoint p, float delta, float koeff)
        {
            p.totalForce = MathHelper.vectZero;
            if (!p.disableGravity)
            {
                if (!MathHelper.vectEqual(MaterialPoint.globalGravity, MathHelper.vectZero))
                {
                    p.totalForce = MathHelper.vectAdd(p.totalForce, MathHelper.vectMult(MaterialPoint.globalGravity, p.weight));
                }
                else
                {
                    p.totalForce = MathHelper.vectAdd(p.totalForce, p.gravity);
                }
            }
            if (p.highestForceIndex != -1)
            {
                for (int i = 0; i <= p.highestForceIndex; i++)
                {
                    p.totalForce = MathHelper.vectAdd(p.totalForce, p.forces[i]);
                }
            }
            p.totalForce = MathHelper.vectMult(p.totalForce, p.invWeight);
            p.a = MathHelper.vectMult(p.totalForce, (float)((double)delta / 1.0 * 0.01600000075995922 * (double)koeff));
            if (p.prevPos.x == 2.1474836E+09f)
            {
                p.prevPos = p.pos;
            }
            p.posDelta.x = p.pos.x - p.prevPos.x + p.a.x;
            p.posDelta.y = p.pos.y - p.prevPos.y + p.a.y;
            p.v = MathHelper.vectMult(p.posDelta, (float)(1.0 / (double)delta));
            p.prevPos = p.pos;
            p.pos = MathHelper.vectAdd(p.pos, p.posDelta);
        }

        public Vector prevPos;

        public Vector pin;

        public List<Constraint> constraints;
    }
}
