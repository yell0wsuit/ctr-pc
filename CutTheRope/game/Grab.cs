using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x02000080 RID: 128
	internal class Grab : CTRGameObject
	{
		// Token: 0x0600053D RID: 1341 RVA: 0x000266CC File Offset: 0x000248CC
		private static void drawGrabCircle(Grab s, float x, float y, float radius, int vertexCount, RGBAColor color)
		{
			OpenGL.glColor4f(color.toXNA());
			OpenGL.glLineWidth(3.0);
			OpenGL.glDisableClientState(0);
			OpenGL.glEnableClientState(13);
			OpenGL.glColorPointer_setAdditive(s.vertexCount * 8);
			OpenGL.glVertexPointer_setAdditive(2, 5, 0, s.vertexCount * 16);
			for (int i = 0; i < s.vertexCount; i += 2)
			{
				GLDrawer.drawAntialiasedLine(s.vertices[i * 2], s.vertices[i * 2 + 1], s.vertices[i * 2 + 2], s.vertices[i * 2 + 3], 3f, color);
			}
			OpenGL.glDrawArrays(8, 0, 8);
			OpenGL.glEnableClientState(0);
			OpenGL.glDisableClientState(13);
			OpenGL.glLineWidth(1.0);
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0002678C File Offset: 0x0002498C
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.rope = null;
				this.wheelOperating = -1;
				CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
				this.baloon = cTRRootController.isSurvival();
			}
			return this;
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x000267C8 File Offset: 0x000249C8
		public virtual float getRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
		{
			Vector v3 = CutTheRope.iframework.helpers.MathHelper.vectSub(v1, c);
			return CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES(CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(CutTheRope.iframework.helpers.MathHelper.vectSub(v2, c)) - CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(v3));
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x000267F5 File Offset: 0x000249F5
		public virtual void handleWheelTouch(Vector v)
		{
			this.lastWheelTouch = v;
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00026800 File Offset: 0x00024A00
		public virtual void handleWheelRotate(Vector v)
		{
			if (this.lastWheelTouch.x - v.x == 0f && this.lastWheelTouch.y - v.y == 0f)
			{
				return;
			}
			CTRSoundMgr._playSound(36);
			float num = this.getRotateAngleForStartEndCenter(this.lastWheelTouch, v, CutTheRope.iframework.helpers.MathHelper.vect(this.x, this.y));
			if ((double)num > 180.0)
			{
				num -= 360f;
			}
			else if ((double)num < -180.0)
			{
				num += 360f;
			}
			this.wheelImage2.rotation += num;
			this.wheelImage3.rotation += num;
			this.wheelHighlight.rotation += num;
			num = ((num > 0f) ? CutTheRope.iframework.helpers.MathHelper.MIN((double)CutTheRope.iframework.helpers.MathHelper.MAX(1.0, (double)num), 4.5) : CutTheRope.iframework.helpers.MathHelper.MAX((double)CutTheRope.iframework.helpers.MathHelper.MIN(-1.0, (double)num), -4.5));
			float num2 = 0f;
			if (this.rope != null)
			{
				num2 = (float)this.rope.getLength();
			}
			if (this.rope != null)
			{
				if (num > 0f)
				{
					if (num2 < 1650f)
					{
						this.rope.roll(num);
					}
				}
				else if (num != 0f && this.rope.parts.Count > 3)
				{
					this.rope.rollBack(0f - num);
				}
				this.wheelDirty = true;
			}
			this.lastWheelTouch = v;
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00026994 File Offset: 0x00024B94
		public override void update(float delta)
		{
			base.update(delta);
			if (this.launcher && this.rope != null)
			{
				this.rope.bungeeAnchor.pos = CutTheRope.iframework.helpers.MathHelper.vect(this.x, this.y);
				this.rope.bungeeAnchor.pin = this.rope.bungeeAnchor.pos;
				if (this.launcherIncreaseSpeed)
				{
					if (Mover.moveVariableToTarget(ref this.launcherSpeed, 200.0, 30.0, (double)delta))
					{
						this.launcherIncreaseSpeed = false;
					}
				}
				else if (Mover.moveVariableToTarget(ref this.launcherSpeed, 130.0, 30.0, (double)delta))
				{
					this.launcherIncreaseSpeed = true;
				}
				this.mover.setMoveSpeed(this.launcherSpeed);
			}
			if (this.hideRadius)
			{
				this.radiusAlpha -= 1.5f * delta;
				if ((double)this.radiusAlpha <= 0.0)
				{
					this.radius = -1f;
					this.hideRadius = false;
				}
			}
			if (this.bee != null)
			{
				Vector vector2 = this.mover.path[this.mover.targetPoint];
				Vector pos = this.mover.pos;
				Vector vector = CutTheRope.iframework.helpers.MathHelper.vectSub(vector2, pos);
				float t = 0f;
				if (CutTheRope.iframework.helpers.MathHelper.ABS(vector.x) > 15f)
				{
					float num = 10f;
					t = ((vector.x > 0f) ? num : (0f - num));
				}
				Mover.moveVariableToTarget(ref this.bee.rotation, t, 60f, delta);
			}
			if (this.wheel && this.wheelDirty)
			{
				float num2 = ((this.rope == null) ? 0f : ((float)this.rope.getLength() * 0.7f));
				if (num2 == 0f)
				{
					this.wheelImage2.scaleX = (this.wheelImage2.scaleY = 0f);
					return;
				}
				this.wheelImage2.scaleX = (this.wheelImage2.scaleY = CutTheRope.iframework.helpers.MathHelper.MAX(0f, CutTheRope.iframework.helpers.MathHelper.MIN(1.2, 1.0 - (double)FrameworkTypes.RT((double)(num2 / 1400f), (double)num2 / 700.0))));
			}
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x00026BEC File Offset: 0x00024DEC
		public virtual void updateSpider(float delta)
		{
			if (this.hasSpider && this.shouldActivate)
			{
				this.shouldActivate = false;
				this.spiderActive = true;
				CTRSoundMgr._playSound(33);
				this.spider.playTimeline(0);
			}
			if (!this.hasSpider || !this.spiderActive)
			{
				return;
			}
			if (this.spider.getCurrentTimelineIndex() != 0)
			{
				this.spiderPos += delta * 117f;
			}
			float num = 0f;
			bool flag = false;
			if (this.rope != null)
			{
				int i = 0;
				while (i < this.rope.drawPtsCount)
				{
					Vector vector = CutTheRope.iframework.helpers.MathHelper.vect(this.rope.drawPts[i], this.rope.drawPts[i + 1]);
					Vector vector2 = CutTheRope.iframework.helpers.MathHelper.vect(this.rope.drawPts[i + 2], this.rope.drawPts[i + 3]);
					float num2 = CutTheRope.iframework.helpers.MathHelper.MAX(2f * Bungee.BUNGEE_REST_LEN / 3f, CutTheRope.iframework.helpers.MathHelper.vectDistance(vector, vector2));
					if (this.spiderPos >= num && (this.spiderPos < num + num2 || i > this.rope.drawPtsCount - 3))
					{
						float num3 = this.spiderPos - num;
						Vector v = CutTheRope.iframework.helpers.MathHelper.vectSub(vector2, vector);
						v = CutTheRope.iframework.helpers.MathHelper.vectMult(v, num3 / num2);
						this.spider.x = vector.x + v.x;
						this.spider.y = vector.y + v.y;
						if (i > this.rope.drawPtsCount - 3)
						{
							flag = true;
						}
						if (this.spider.getCurrentTimelineIndex() != 0)
						{
							this.spider.rotation = CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES(CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(v)) + 270f;
							break;
						}
						break;
					}
					else
					{
						num += num2;
						i += 2;
					}
				}
			}
			if (flag)
			{
				this.spiderPos = -1f;
			}
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00026DC4 File Offset: 0x00024FC4
		public virtual void drawBack()
		{
			if ((double)this.moveLength > 0.0)
			{
				this.moveBackground.draw();
			}
			else
			{
				this.back.draw();
			}
			OpenGL.glDisable(0);
			if (this.radius != -1f || this.hideRadius)
			{
				RGBAColor rGBAColor = RGBAColor.MakeRGBA(0.2, 0.5, 0.9, (double)this.radiusAlpha);
				Grab.drawGrabCircle(this, this.x, this.y, this.radius, this.vertexCount, rGBAColor);
			}
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00026E70 File Offset: 0x00025070
		public void drawBungee()
		{
			Bungee bungee = this.rope;
			if (bungee != null)
			{
				bungee.draw();
			}
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x00026E90 File Offset: 0x00025090
		public override void draw()
		{
			base.preDraw();
			OpenGL.glEnable(0);
			Bungee bungee = this.rope;
			if (this.wheel)
			{
				this.wheelHighlight.visible = this.wheelOperating != -1;
				this.wheelImage3.visible = this.wheelOperating == -1;
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				this.wheelImage.draw();
				OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			}
			OpenGL.glDisable(0);
			if (bungee != null)
			{
				bungee.draw();
			}
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			if ((double)this.moveLength <= 0.0)
			{
				this.front.draw();
			}
			else if (this.moverDragging != -1)
			{
				this.grabMoverHighlight.draw();
			}
			else
			{
				this.grabMover.draw();
			}
			if (this.wheel)
			{
				this.wheelImage2.draw();
			}
			base.postDraw();
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x00026F80 File Offset: 0x00025180
		public virtual void drawSpider()
		{
			this.spider.draw();
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x00026F8D File Offset: 0x0002518D
		public virtual void setRope(Bungee r)
		{
			this.rope = r;
			this.radius = -1f;
			if (this.hasSpider)
			{
				this.shouldActivate = true;
			}
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x00026FB0 File Offset: 0x000251B0
		public virtual void setLauncher()
		{
			this.launcher = true;
			this.launcherIncreaseSpeed = true;
			this.launcherSpeed = 130f;
			Mover mover = new Mover().initWithPathCapacityMoveSpeedRotateSpeed(100, this.launcherSpeed, 0f);
			mover.setPathFromStringandStart(NSObject.NSS("RC30"), CutTheRope.iframework.helpers.MathHelper.vect(this.x, this.y));
			this.setMover(mover);
			mover.start();
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0002701C File Offset: 0x0002521C
		public virtual void reCalcCircle()
		{
			GLDrawer.calcCircle(this.x, this.y, this.radius, this.vertexCount, this.vertices);
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00027044 File Offset: 0x00025244
		public virtual void setRadius(float r)
		{
			this.radius = r;
			if (this.radius == -1f)
			{
				int r2 = CutTheRope.iframework.helpers.MathHelper.RND_RANGE(76, 77);
				this.back = Image.Image_createWithResIDQuad(r2, 0);
				this.back.doRestoreCutTransparency();
				this.back.anchor = (this.back.parentAnchor = 18);
				this.front = Image.Image_createWithResIDQuad(r2, 1);
				this.front.anchor = (this.front.parentAnchor = 18);
				this.addChild(this.back);
				this.addChild(this.front);
				this.back.visible = false;
				this.front.visible = false;
			}
			else
			{
				this.back = Image.Image_createWithResIDQuad(74, 0);
				this.back.doRestoreCutTransparency();
				this.back.anchor = (this.back.parentAnchor = 18);
				this.front = Image.Image_createWithResIDQuad(74, 1);
				this.front.anchor = (this.front.parentAnchor = 18);
				this.addChild(this.back);
				this.addChild(this.front);
				this.back.visible = false;
				this.front.visible = false;
				this.radiusAlpha = 1f;
				this.hideRadius = false;
				this.vertexCount = (int)CutTheRope.iframework.helpers.MathHelper.MAX(16f, this.radius);
				this.vertexCount /= 2;
				if (this.vertexCount % 2 != 0)
				{
					this.vertexCount++;
				}
				this.vertices = new float[this.vertexCount * 2];
				GLDrawer.calcCircle(this.x, this.y, this.radius, this.vertexCount, this.vertices);
			}
			if (this.wheel)
			{
				this.wheelImage = Image.Image_createWithResIDQuad(81, 0);
				this.wheelImage.anchor = (this.wheelImage.parentAnchor = 18);
				this.addChild(this.wheelImage);
				this.wheelImage.visible = false;
				this.wheelImage2 = Image.Image_createWithResIDQuad(81, 1);
				this.wheelImage2.passTransformationsToChilds = false;
				this.wheelHighlight = Image.Image_createWithResIDQuad(81, 2);
				this.wheelHighlight.anchor = (this.wheelHighlight.parentAnchor = 18);
				this.wheelImage2.addChild(this.wheelHighlight);
				this.wheelImage3 = Image.Image_createWithResIDQuad(81, 3);
				this.wheelImage3.anchor = (this.wheelImage3.parentAnchor = (this.wheelImage2.anchor = (this.wheelImage2.parentAnchor = 18)));
				this.wheelImage2.addChild(this.wheelImage3);
				this.addChild(this.wheelImage2);
				this.wheelImage2.visible = false;
				this.wheelDirty = true;
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00027334 File Offset: 0x00025534
		public virtual void setMoveLengthVerticalOffset(float l, bool v, float o)
		{
			this.moveLength = l;
			this.moveVertical = v;
			this.moveOffset = o;
			if ((double)this.moveLength > 0.0)
			{
				this.moveBackground = HorizontallyTiledImage.HorizontallyTiledImage_createWithResID(82);
				this.moveBackground.setTileHorizontallyLeftCenterRight(0, 2, 1);
				this.moveBackground.width = (int)(l + 142f);
				this.moveBackground.rotationCenterX = 0f - CutTheRope.iframework.helpers.MathHelper.round((double)this.moveBackground.width / 2.0) + 74f;
				this.moveBackground.x = -74f;
				this.grabMoverHighlight = Image.Image_createWithResIDQuad(82, 3);
				this.grabMoverHighlight.visible = false;
				this.grabMoverHighlight.anchor = (this.grabMoverHighlight.parentAnchor = 18);
				this.addChild(this.grabMoverHighlight);
				this.grabMover = Image.Image_createWithResIDQuad(82, 4);
				this.grabMover.visible = false;
				this.grabMover.anchor = (this.grabMover.parentAnchor = 18);
				this.addChild(this.grabMover);
				this.grabMover.addChild(this.moveBackground);
				if (this.moveVertical)
				{
					this.moveBackground.rotation = 90f;
					this.moveBackground.y = 0f - this.moveOffset;
					this.minMoveValue = this.y - this.moveOffset;
					this.maxMoveValue = this.y + (this.moveLength - this.moveOffset);
					this.grabMover.rotation = 90f;
					this.grabMoverHighlight.rotation = 90f;
				}
				else
				{
					this.minMoveValue = this.x - this.moveOffset;
					this.maxMoveValue = this.x + (this.moveLength - this.moveOffset);
					this.moveBackground.x += 0f - this.moveOffset;
				}
				this.moveBackground.anchor = 17;
				this.moveBackground.x += this.x;
				this.moveBackground.y += this.y;
				this.moveBackground.visible = false;
			}
			this.moverDragging = -1;
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0002758C File Offset: 0x0002578C
		public virtual void setBee()
		{
			this.bee = Image.Image_createWithResIDQuad(98, 1);
			this.bee.blendingMode = 1;
			this.bee.doRestoreCutTransparency();
			this.bee.parentAnchor = 18;
			Animation animation = Animation.Animation_createWithResID(98);
			animation.parentAnchor = (animation.anchor = 9);
			animation.doRestoreCutTransparency();
			animation.addAnimationDelayLoopFirstLast(0.03, Timeline.LoopType.TIMELINE_PING_PONG, 2, 4);
			animation.playTimeline(0);
			animation.jumpTo(CutTheRope.iframework.helpers.MathHelper.RND_RANGE(0, 2));
			this.bee.addChild(animation);
			Vector quadOffset = Image.getQuadOffset(98, 0);
			this.bee.x = 0f - quadOffset.x;
			this.bee.y = 0f - quadOffset.y;
			this.bee.rotationCenterX = quadOffset.x - (float)(this.bee.width / 2);
			this.bee.rotationCenterY = quadOffset.y - (float)(this.bee.height / 2);
			this.bee.scaleX = (this.bee.scaleY = 0.7692308f);
			this.addChild(this.bee);
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x000276C0 File Offset: 0x000258C0
		public virtual void setSpider(bool s)
		{
			this.hasSpider = s;
			this.shouldActivate = false;
			this.spiderActive = false;
			this.spider = Animation.Animation_createWithResID(64);
			this.spider.doRestoreCutTransparency();
			this.spider.anchor = 18;
			this.spider.x = this.x;
			this.spider.y = this.y;
			this.spider.visible = false;
			this.spider.addAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 6);
			this.spider.setDelayatIndexforAnimation(0.4f, 5, 0);
			this.spider.addAnimationWithIDDelayLoopFirstLast(1, 0.1f, Timeline.LoopType.TIMELINE_REPLAY, 7, 10);
			this.spider.switchToAnimationatEndOfAnimationDelay(1, 0, 0.05f);
			this.addChild(this.spider);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0002778F File Offset: 0x0002598F
		public virtual void destroyRope()
		{
			this.rope.release();
			this.rope = null;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x000277A3 File Offset: 0x000259A3
		public override void dealloc()
		{
			if (this.vertices != null)
			{
				NSObject.free(this.vertices);
			}
			this.destroyRope();
			base.dealloc();
		}

		// Token: 0x0400041F RID: 1055
		public const float SPIDER_SPEED = 117f;

		// Token: 0x04000420 RID: 1056
		public Image back;

		// Token: 0x04000421 RID: 1057
		public Image front;

		// Token: 0x04000422 RID: 1058
		public Bungee rope;

		// Token: 0x04000423 RID: 1059
		public float radius;

		// Token: 0x04000424 RID: 1060
		public float radiusAlpha;

		// Token: 0x04000425 RID: 1061
		public bool hideRadius;

		// Token: 0x04000426 RID: 1062
		public float[] vertices;

		// Token: 0x04000427 RID: 1063
		public int vertexCount;

		// Token: 0x04000428 RID: 1064
		public bool wheel;

		// Token: 0x04000429 RID: 1065
		public Image wheelHighlight;

		// Token: 0x0400042A RID: 1066
		public Image wheelImage;

		// Token: 0x0400042B RID: 1067
		public Image wheelImage2;

		// Token: 0x0400042C RID: 1068
		public Image wheelImage3;

		// Token: 0x0400042D RID: 1069
		public int wheelOperating;

		// Token: 0x0400042E RID: 1070
		public Vector lastWheelTouch;

		// Token: 0x0400042F RID: 1071
		public float moveLength;

		// Token: 0x04000430 RID: 1072
		public bool moveVertical;

		// Token: 0x04000431 RID: 1073
		public float moveOffset;

		// Token: 0x04000432 RID: 1074
		public HorizontallyTiledImage moveBackground;

		// Token: 0x04000433 RID: 1075
		public Image grabMoverHighlight;

		// Token: 0x04000434 RID: 1076
		public Image grabMover;

		// Token: 0x04000435 RID: 1077
		public int moverDragging;

		// Token: 0x04000436 RID: 1078
		public float minMoveValue;

		// Token: 0x04000437 RID: 1079
		public float maxMoveValue;

		// Token: 0x04000438 RID: 1080
		public bool hasSpider;

		// Token: 0x04000439 RID: 1081
		public bool spiderActive;

		// Token: 0x0400043A RID: 1082
		public Animation spider;

		// Token: 0x0400043B RID: 1083
		public float spiderPos;

		// Token: 0x0400043C RID: 1084
		public bool shouldActivate;

		// Token: 0x0400043D RID: 1085
		public bool wheelDirty;

		// Token: 0x0400043E RID: 1086
		public bool launcher;

		// Token: 0x0400043F RID: 1087
		public float launcherSpeed;

		// Token: 0x04000440 RID: 1088
		public bool launcherIncreaseSpeed;

		// Token: 0x04000441 RID: 1089
		public float initial_rotation;

		// Token: 0x04000442 RID: 1090
		public float initial_x;

		// Token: 0x04000443 RID: 1091
		public float initial_y;

		// Token: 0x04000444 RID: 1092
		public RotatedCircle initial_rotatedCircle;

		// Token: 0x04000445 RID: 1093
		public bool baloon;

		// Token: 0x04000446 RID: 1094
		public Image bee;

		// Token: 0x020000C8 RID: 200
		private enum SPIDER_ANI
		{
			// Token: 0x040008F8 RID: 2296
			SPIDER_START_ANI,
			// Token: 0x040008F9 RID: 2297
			SPIDER_WALK_ANI,
			// Token: 0x040008FA RID: 2298
			SPIDER_BUSTED_ANI,
			// Token: 0x040008FB RID: 2299
			SPIDER_CATCH_ANI
		}
	}
}
