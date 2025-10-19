using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x02000071 RID: 113
	internal class Bungee : ConstraintSystem
	{
		// Token: 0x0600045F RID: 1119 RVA: 0x000186BC File Offset: 0x000168BC
		private static void drawAntialiasedLineContinued(float x1, float y1, float x2, float y2, float size, RGBAColor color, ref float lx, ref float ly, ref float rx, ref float ry, bool highlighted)
		{
			Vector v = CutTheRope.iframework.helpers.MathHelper.vect(x1, y1);
			Vector v2 = CutTheRope.iframework.helpers.MathHelper.vect(x2, y2);
			Vector vector = CutTheRope.iframework.helpers.MathHelper.vectSub(v2, v);
			if (!CutTheRope.iframework.helpers.MathHelper.vectEqual(vector, CutTheRope.iframework.helpers.MathHelper.vectZero))
			{
				Vector v3 = (highlighted ? vector : CutTheRope.iframework.helpers.MathHelper.vectMult(vector, ((double)color.a == 1.0) ? 1.02 : 1.0));
				Vector v4 = CutTheRope.iframework.helpers.MathHelper.vectPerp(vector);
				Vector vector2 = CutTheRope.iframework.helpers.MathHelper.vectNormalize(v4);
				v4 = CutTheRope.iframework.helpers.MathHelper.vectMult(vector2, size);
				Vector v5 = CutTheRope.iframework.helpers.MathHelper.vectNeg(v4);
				Vector v6 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v4, vector);
				Vector v7 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v5, vector);
				v6 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v6, v);
				v7 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v7, v);
				Vector v8 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v4, v3);
				Vector v9 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v5, v3);
				Vector vector3 = CutTheRope.iframework.helpers.MathHelper.vectMult(vector2, size + 6f);
				Vector v10 = CutTheRope.iframework.helpers.MathHelper.vectNeg(vector3);
				Vector v11 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector3, vector);
				Vector v12 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v10, vector);
				vector3 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector3, v);
				v10 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v10, v);
				v11 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v11, v);
				v12 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v12, v);
				if (lx == -1f)
				{
					v4 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v4, v);
					v5 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v5, v);
				}
				else
				{
					v4 = CutTheRope.iframework.helpers.MathHelper.vect(lx, ly);
					v5 = CutTheRope.iframework.helpers.MathHelper.vect(rx, ry);
				}
				v8 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v8, v);
				v9 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v9, v);
				lx = v6.x;
				ly = v6.y;
				rx = v7.x;
				ry = v7.y;
				Vector vector4 = CutTheRope.iframework.helpers.MathHelper.vectSub(v4, vector2);
				Vector vector5 = CutTheRope.iframework.helpers.MathHelper.vectSub(v8, vector2);
				Vector vector6 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v5, vector2);
				Vector vector7 = CutTheRope.iframework.helpers.MathHelper.vectAdd(v9, vector2);
				float[] pointer = new float[]
				{
					vector3.x, vector3.y, v11.x, v11.y, v4.x, v4.y, v8.x, v8.y, v5.x, v5.y,
					v9.x, v9.y, v10.x, v10.y, v12.x, v12.y
				};
				RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
				whiteRGBA.a = 0.1f * color.a;
				Bungee.ccolors[2] = whiteRGBA;
				Bungee.ccolors[3] = whiteRGBA;
				Bungee.ccolors[4] = whiteRGBA;
				Bungee.ccolors[5] = whiteRGBA;
				float[] pointer2 = new float[]
				{
					v4.x, v4.y, v8.x, v8.y, vector4.x, vector4.y, vector5.x, vector5.y, v.x, v.y,
					v2.x, v2.y, vector6.x, vector6.y, vector7.x, vector7.y, v5.x, v5.y, v9.x, v9.y
				};
				RGBAColor rGBAColor = color;
				float num = 0.15f * color.a;
				color.r += num;
				color.g += num;
				color.b += num;
				Bungee.ccolors2[2] = color;
				Bungee.ccolors2[3] = color;
				Bungee.ccolors2[4] = rGBAColor;
				Bungee.ccolors2[5] = rGBAColor;
				Bungee.ccolors2[6] = color;
				Bungee.ccolors2[7] = color;
				OpenGL.glDisableClientState(0);
				OpenGL.glEnableClientState(13);
				if (highlighted)
				{
					OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
					OpenGL.glColorPointer(4, 5, 0, Bungee.ccolors);
					OpenGL.glVertexPointer(2, 5, 0, pointer);
					OpenGL.glDrawArrays(8, 0, 8);
				}
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				OpenGL.glColorPointer(4, 5, 0, Bungee.ccolors2);
				OpenGL.glVertexPointer(2, 5, 0, pointer2);
				OpenGL.glDrawArrays(8, 0, 10);
				OpenGL.glEnableClientState(0);
				OpenGL.glDisableClientState(13);
			}
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00018B54 File Offset: 0x00016D54
		private static void drawBungee(Bungee b, Vector[] pts, int count, int points)
		{
			float num = ((b.cut == -1 || b.forceWhite) ? 1f : (b.cutTime / 1.95f));
			RGBAColor rGBAColor = RGBAColor.MakeRGBA(0.475 * (double)num, 0.305 * (double)num, 0.185 * (double)num, (double)num);
			RGBAColor rGBAColor2 = RGBAColor.MakeRGBA(0.6755555555555556 * (double)num, 0.44 * (double)num, 0.27555555555555555 * (double)num, (double)num);
			RGBAColor rGBAColor3 = RGBAColor.MakeRGBA(0.19 * (double)num, 0.122 * (double)num, 0.074 * (double)num, (double)num);
			RGBAColor rGBAColor4 = RGBAColor.MakeRGBA(0.304 * (double)num, 0.198 * (double)num, 0.124 * (double)num, (double)num);
			if (b.highlighted)
			{
				float num2 = 3f;
				rGBAColor.r *= num2;
				rGBAColor.g *= num2;
				rGBAColor.b *= num2;
				rGBAColor2.r *= num2;
				rGBAColor2.g *= num2;
				rGBAColor2.b *= num2;
				rGBAColor3.r *= num2;
				rGBAColor3.g *= num2;
				rGBAColor3.b *= num2;
				rGBAColor4.r *= num2;
				rGBAColor4.g *= num2;
				rGBAColor4.b *= num2;
			}
			float num3 = CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(pts[0].x, pts[0].y), CutTheRope.iframework.helpers.MathHelper.vect(pts[1].x, pts[1].y));
			if ((double)num3 <= (double)Bungee.BUNGEE_REST_LEN + 0.3)
			{
				b.relaxed = 0;
			}
			else if ((double)num3 <= (double)Bungee.BUNGEE_REST_LEN + 1.0)
			{
				b.relaxed = 1;
			}
			else if ((double)num3 <= (double)Bungee.BUNGEE_REST_LEN + 4.0)
			{
				b.relaxed = 2;
			}
			else
			{
				b.relaxed = 3;
			}
			if ((double)num3 > (double)Bungee.BUNGEE_REST_LEN + 7.0)
			{
				float num4 = num3 / Bungee.BUNGEE_REST_LEN * 2f;
				rGBAColor3.r *= num4;
				rGBAColor4.r *= num4;
			}
			bool flag = false;
			int num5 = (count - 1) * points;
			float[] array = new float[num5 * 2];
			b.drawPtsCount = num5 * 2;
			float num6 = 1f / (float)num5;
			float num7 = 0f;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			RGBAColor rGBAColor5 = rGBAColor3;
			RGBAColor rGBAColor6 = rGBAColor4;
			float num11 = (rGBAColor.r - rGBAColor3.r) / (float)(num5 - 1);
			float num12 = (rGBAColor.g - rGBAColor3.g) / (float)(num5 - 1);
			float num13 = (rGBAColor.b - rGBAColor3.b) / (float)(num5 - 1);
			float num14 = (rGBAColor2.r - rGBAColor4.r) / (float)(num5 - 1);
			float num15 = (rGBAColor2.g - rGBAColor4.g) / (float)(num5 - 1);
			float num16 = (rGBAColor2.b - rGBAColor4.b) / (float)(num5 - 1);
			float lx = -1f;
			float ly = -1f;
			float rx = -1f;
			float ry = -1f;
			for (;;)
			{
				if ((double)num7 > 1.0)
				{
					num7 = 1f;
				}
				if (count < 3)
				{
					break;
				}
				Vector vector = GLDrawer.calcPathBezier(pts, count, num7);
				array[num8++] = vector.x;
				array[num8++] = vector.y;
				b.drawPts[num9++] = vector.x;
				b.drawPts[num9++] = vector.y;
				if (num8 >= 8 || (double)num7 == 1.0)
				{
					RGBAColor color = (b.forceWhite ? RGBAColor.whiteRGBA : ((!flag) ? rGBAColor6 : rGBAColor5));
					OpenGL.glColor4f(color.toXNA());
					int num17 = num8 >> 1;
					for (int i = 0; i < num17 - 1; i++)
					{
						Bungee.drawAntialiasedLineContinued(array[i * 2], array[i * 2 + 1], array[i * 2 + 2], array[i * 2 + 3], 5f, color, ref lx, ref ly, ref rx, ref ry, b.highlighted);
					}
					array[0] = array[num8 - 2];
					array[1] = array[num8 - 1];
					num8 = 2;
					flag = !flag;
					num10++;
					rGBAColor5.r += num11 * (float)(num17 - 1);
					rGBAColor5.g += num12 * (float)(num17 - 1);
					rGBAColor5.b += num13 * (float)(num17 - 1);
					rGBAColor6.r += num14 * (float)(num17 - 1);
					rGBAColor6.g += num15 * (float)(num17 - 1);
					rGBAColor6.b += num16 * (float)(num17 - 1);
				}
				if ((double)num7 == 1.0)
				{
					break;
				}
				num7 += num6;
			}
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0001905C File Offset: 0x0001725C
		public virtual NSObject initWithHeadAtXYTailAtTXTYandLength(ConstraintedPoint h, float hx, float hy, ConstraintedPoint t, float tx, float ty, float len)
		{
			if (this.init() != null)
			{
				this.relaxationTimes = 30;
				this.lineWidth = 10f;
				this.cut = -1;
				this.bungeeMode = 0;
				this.highlighted = false;
				if (h != null)
				{
					this.bungeeAnchor = h;
				}
				else
				{
					this.bungeeAnchor = (ConstraintedPoint)new ConstraintedPoint().init();
				}
				if (t != null)
				{
					this.tail = t;
				}
				else
				{
					this.tail = (ConstraintedPoint)new ConstraintedPoint().init();
					this.tail.setWeight(1f);
				}
				this.bungeeAnchor.setWeight(0.02f);
				this.bungeeAnchor.pos = CutTheRope.iframework.helpers.MathHelper.vect(hx, hy);
				this.tail.pos = CutTheRope.iframework.helpers.MathHelper.vect(tx, ty);
				this.addPart(this.bungeeAnchor);
				this.addPart(this.tail);
				this.tail.addConstraintwithRestLengthofType(this.bungeeAnchor, Bungee.BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
				Vector v = CutTheRope.iframework.helpers.MathHelper.vectSub(this.tail.pos, this.bungeeAnchor.pos);
				int num = (int)(len / Bungee.BUNGEE_REST_LEN + 2f);
				v = CutTheRope.iframework.helpers.MathHelper.vectDiv(v, (float)num);
				this.rollplacingWithOffset(len, v);
				this.forceWhite = false;
				this.initialCandleAngle = -1f;
				this.chosenOne = false;
				this.hideTailParts = false;
			}
			return this;
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x000191B4 File Offset: 0x000173B4
		public virtual int getLength()
		{
			int num = 0;
			Vector pos = CutTheRope.iframework.helpers.MathHelper.vectZero;
			int count = this.parts.Count;
			for (int i = 0; i < count; i++)
			{
				ConstraintedPoint constraintedPoint = this.parts[i];
				if (i > 0)
				{
					num += (int)CutTheRope.iframework.helpers.MathHelper.vectDistance(pos, constraintedPoint.pos);
				}
				pos = constraintedPoint.pos;
			}
			return num;
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x0001920D File Offset: 0x0001740D
		public virtual void roll(float rollLen)
		{
			this.rollplacingWithOffset(rollLen, CutTheRope.iframework.helpers.MathHelper.vectZero);
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x0001921C File Offset: 0x0001741C
		public virtual void rollplacingWithOffset(float rollLen, Vector off)
		{
			ConstraintedPoint i = this.parts[this.parts.Count - 2];
			int num = (int)this.tail.restLengthFor(i);
			while (rollLen > 0f)
			{
				if (rollLen >= Bungee.BUNGEE_REST_LEN)
				{
					ConstraintedPoint constraintedPoint = this.parts[this.parts.Count - 2];
					ConstraintedPoint constraintedPoint2 = (ConstraintedPoint)new ConstraintedPoint().init();
					constraintedPoint2.setWeight(0.02f);
					constraintedPoint2.pos = CutTheRope.iframework.helpers.MathHelper.vectAdd(constraintedPoint.pos, off);
					this.addPartAt(constraintedPoint2, this.parts.Count - 1);
					this.tail.changeConstraintFromTowithRestLength(constraintedPoint, constraintedPoint2, (float)num);
					constraintedPoint2.addConstraintwithRestLengthofType(constraintedPoint, Bungee.BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
					rollLen -= Bungee.BUNGEE_REST_LEN;
				}
				else
				{
					int num2 = (int)(rollLen + (float)num);
					if ((float)num2 > Bungee.BUNGEE_REST_LEN)
					{
						rollLen = Bungee.BUNGEE_REST_LEN;
						num = (int)((float)num2 - Bungee.BUNGEE_REST_LEN);
					}
					else
					{
						ConstraintedPoint n2 = this.parts[this.parts.Count - 2];
						this.tail.changeRestLengthToFor((float)num2, n2);
						rollLen = 0f;
					}
				}
			}
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00019344 File Offset: 0x00017544
		public virtual float rollBack(float amount)
		{
			float num = amount;
			ConstraintedPoint i = this.parts[this.parts.Count - 2];
			int num2 = (int)this.tail.restLengthFor(i);
			int num3 = this.parts.Count;
			while (num > 0f)
			{
				if (num >= Bungee.BUNGEE_REST_LEN)
				{
					ConstraintedPoint o = this.parts[num3 - 2];
					ConstraintedPoint n2 = this.parts[num3 - 3];
					this.tail.changeConstraintFromTowithRestLength(o, n2, (float)num2);
					this.parts.RemoveAt(this.parts.Count - 2);
					num3--;
					num -= Bungee.BUNGEE_REST_LEN;
				}
				else
				{
					int num4 = (int)((float)num2 - num);
					if (num4 < 1)
					{
						num = Bungee.BUNGEE_REST_LEN;
						num2 = (int)(Bungee.BUNGEE_REST_LEN + (float)num4 + 1f);
					}
					else
					{
						ConstraintedPoint n3 = this.parts[num3 - 2];
						this.tail.changeRestLengthToFor((float)num4, n3);
						num = 0f;
					}
				}
			}
			int count = this.tail.constraints.Count;
			for (int j = 0; j < count; j++)
			{
				Constraint constraint = this.tail.constraints[j];
				if (constraint != null && constraint.type == Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN)
				{
					constraint.restLength = (float)(num3 - 1) * (Bungee.BUNGEE_REST_LEN + 3f);
				}
			}
			return num;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x000194A0 File Offset: 0x000176A0
		public virtual void removePart(int part)
		{
			this.forceWhite = false;
			ConstraintedPoint constraintedPoint = this.parts[part];
			ConstraintedPoint constraintedPoint2 = ((part + 1 >= this.parts.Count) ? null : this.parts[part + 1]);
			if (constraintedPoint2 == null)
			{
				constraintedPoint.removeConstraints();
			}
			else
			{
				for (int i = 0; i < constraintedPoint2.constraints.Count; i++)
				{
					Constraint constraint = constraintedPoint2.constraints[i];
					if (constraint.cp == constraintedPoint)
					{
						constraintedPoint2.constraints.Remove(constraint);
						ConstraintedPoint constraintedPoint3 = (ConstraintedPoint)new ConstraintedPoint().init();
						constraintedPoint3.setWeight(1E-05f);
						constraintedPoint3.pos = constraintedPoint2.pos;
						constraintedPoint3.prevPos = constraintedPoint2.prevPos;
						this.addPartAt(constraintedPoint3, part + 1);
						constraintedPoint3.addConstraintwithRestLengthofType(constraintedPoint, Bungee.BUNGEE_REST_LEN, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
						break;
					}
				}
			}
			for (int j = 0; j < this.parts.Count; j++)
			{
				ConstraintedPoint constraintedPoint4 = this.parts[j];
				if (constraintedPoint4 != this.tail)
				{
					constraintedPoint4.setWeight(1E-05f);
				}
			}
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x000195BE File Offset: 0x000177BE
		public virtual void setCut(int part)
		{
			this.cut = part;
			this.cutTime = 2f;
			this.forceWhite = true;
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x000195DC File Offset: 0x000177DC
		public virtual void strengthen()
		{
			int count = this.parts.Count;
			for (int i = 0; i < count; i++)
			{
				ConstraintedPoint constraintedPoint = this.parts[i];
				if (constraintedPoint != null)
				{
					if (this.bungeeAnchor.pin.x != -1f)
					{
						if (constraintedPoint != this.tail)
						{
							constraintedPoint.setWeight(0.5f);
						}
						if (i != 0)
						{
							constraintedPoint.addConstraintwithRestLengthofType(this.bungeeAnchor, (float)i * (Bungee.BUNGEE_REST_LEN + 3f), Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
						}
					}
					i++;
				}
			}
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0001965E File Offset: 0x0001785E
		public override void update(float delta)
		{
			this.update(delta, 1f);
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x0001966C File Offset: 0x0001786C
		public virtual void update(float delta, float koeff)
		{
			if ((double)this.cutTime > 0.0)
			{
				Mover.moveVariableToTarget(ref this.cutTime, 0f, 1f, delta);
				if (this.cutTime < 1.95f && this.forceWhite)
				{
					this.removePart(this.cut);
				}
			}
			int count = this.parts.Count;
			for (int i = 0; i < count; i++)
			{
				ConstraintedPoint constraintedPoint = this.parts[i];
				if (constraintedPoint != this.tail)
				{
					ConstraintedPoint.qcpupdate(constraintedPoint, delta, koeff);
				}
			}
			for (int j = 0; j < this.relaxationTimes; j++)
			{
				int count2 = this.parts.Count;
				for (int k = 0; k < count2; k++)
				{
					ConstraintedPoint.satisfyConstraints(this.parts[k]);
				}
			}
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x0001973C File Offset: 0x0001793C
		public override void draw()
		{
			int count = this.parts.Count;
			OpenGL.glColor4f(Bungee.s_Color1);
			if (this.cut == -1)
			{
				Vector[] array = new Vector[count];
				for (int i = 0; i < count; i++)
				{
					ConstraintedPoint constraintedPoint = this.parts[i];
					array[i] = constraintedPoint.pos;
				}
				OpenGL.glLineWidth((double)this.lineWidth);
				Bungee.drawBungee(this, array, count, 4);
				OpenGL.glLineWidth(1.0);
				return;
			}
			Vector[] array2 = new Vector[count];
			Vector[] array3 = new Vector[count];
			bool flag = false;
			int num = 0;
			for (int j = 0; j < count; j++)
			{
				ConstraintedPoint constraintedPoint2 = this.parts[j];
				bool flag2 = true;
				if (j > 0)
				{
					ConstraintedPoint p = this.parts[j - 1];
					if (!constraintedPoint2.hasConstraintTo(p))
					{
						flag2 = false;
					}
				}
				if (constraintedPoint2.pin.x == -1f && !flag2)
				{
					flag = true;
					array2[j] = constraintedPoint2.pos;
				}
				if (!flag)
				{
					array2[j] = constraintedPoint2.pos;
				}
				else
				{
					array3[num] = constraintedPoint2.pos;
					num++;
				}
			}
			OpenGL.glLineWidth((double)this.lineWidth);
			int num2 = count - num;
			if (num2 > 0)
			{
				Bungee.drawBungee(this, array2, num2, 4);
			}
			if (num > 0 && !this.hideTailParts)
			{
				Bungee.drawBungee(this, array3, num, 4);
			}
			OpenGL.glLineWidth(1.0);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x000198BA File Offset: 0x00017ABA
		public override void dealloc()
		{
			base.dealloc();
		}

		// Token: 0x040002FD RID: 765
		private const float ROLLBACK_K = 0.5f;

		// Token: 0x040002FE RID: 766
		private const int BUNGEE_BEZIER_POINTS = 4;

		// Token: 0x040002FF RID: 767
		public const int BUNGEE_RELAXION_TIMES = 30;

		// Token: 0x04000300 RID: 768
		private const float MAX_BUNGEE_SEGMENTS = 20f;

		// Token: 0x04000301 RID: 769
		private const float DEFAULT_PART_WEIGHT = 0.02f;

		// Token: 0x04000302 RID: 770
		private const float STRENGTHENED_PART_WEIGHT = 0.5f;

		// Token: 0x04000303 RID: 771
		private const float CUT_DISSAPPEAR_TIMEOUT = 2f;

		// Token: 0x04000304 RID: 772
		private const float WHITE_TIMEOUT = 0.05f;

		// Token: 0x04000305 RID: 773
		public bool highlighted;

		// Token: 0x04000306 RID: 774
		public static float BUNGEE_REST_LEN = 105f;

		// Token: 0x04000307 RID: 775
		public ConstraintedPoint bungeeAnchor;

		// Token: 0x04000308 RID: 776
		public ConstraintedPoint tail;

		// Token: 0x04000309 RID: 777
		public int cut;

		// Token: 0x0400030A RID: 778
		public int relaxed;

		// Token: 0x0400030B RID: 779
		public float initialCandleAngle;

		// Token: 0x0400030C RID: 780
		public bool chosenOne;

		// Token: 0x0400030D RID: 781
		public int bungeeMode;

		// Token: 0x0400030E RID: 782
		public bool forceWhite;

		// Token: 0x0400030F RID: 783
		public float cutTime;

		// Token: 0x04000310 RID: 784
		public float[] drawPts = new float[200];

		// Token: 0x04000311 RID: 785
		public int drawPtsCount;

		// Token: 0x04000312 RID: 786
		public float lineWidth;

		// Token: 0x04000313 RID: 787
		public bool hideTailParts;

		// Token: 0x04000314 RID: 788
		private static RGBAColor[] ccolors = new RGBAColor[]
		{
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA
		};

		// Token: 0x04000315 RID: 789
		private static RGBAColor[] ccolors2 = new RGBAColor[]
		{
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA,
			RGBAColor.transparentRGBA
		};

		// Token: 0x04000316 RID: 790
		private static Color s_Color1 = new Color(0f, 0f, 0.4f, 1f);

		// Token: 0x020000C3 RID: 195
		private enum BUNGEE_MODE
		{
			// Token: 0x040008EA RID: 2282
			BUNGEE_MODE_NORMAL,
			// Token: 0x040008EB RID: 2283
			BUNGEE_MODE_LOCKED
		}
	}
}
