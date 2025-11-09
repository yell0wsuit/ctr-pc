using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.game
{
    internal class Grab : CTRGameObject
    {
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

        public virtual float getRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
        {
            Vector v3 = CTRMathHelper.vectSub(v1, c);
            return CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(CTRMathHelper.vectSub(v2, c)) - CTRMathHelper.vectAngleNormalized(v3));
        }

        public virtual void handleWheelTouch(Vector v)
        {
            this.lastWheelTouch = v;
        }

        public virtual void handleWheelRotate(Vector v)
        {
            if (this.lastWheelTouch.x - v.x == 0f && this.lastWheelTouch.y - v.y == 0f)
            {
                return;
            }
            CTRSoundMgr._playSound(36);
            float num = this.getRotateAngleForStartEndCenter(this.lastWheelTouch, v, CTRMathHelper.vect(this.x, this.y));
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
            num = (num > 0f) ? CTRMathHelper.MIN((double)CTRMathHelper.MAX(1.0, (double)num), 4.5) : CTRMathHelper.MAX((double)CTRMathHelper.MIN(-1.0, (double)num), -4.5);
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

        public override void update(float delta)
        {
            base.update(delta);
            if (this.launcher && this.rope != null)
            {
                this.rope.bungeeAnchor.pos = CTRMathHelper.vect(this.x, this.y);
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
                Vector vector = CTRMathHelper.vectSub(vector2, pos);
                float t = 0f;
                if (CTRMathHelper.ABS(vector.x) > 15f)
                {
                    float num = 10f;
                    t = (vector.x > 0f) ? num : (0f - num);
                }
                Mover.moveVariableToTarget(ref this.bee.rotation, t, 60f, delta);
            }
            if (this.wheel && this.wheelDirty)
            {
                float num2 = (this.rope == null) ? 0f : ((float)this.rope.getLength() * 0.7f);
                if (num2 == 0f)
                {
                    this.wheelImage2.scaleX = this.wheelImage2.scaleY = 0f;
                    return;
                }
                this.wheelImage2.scaleX = this.wheelImage2.scaleY = CTRMathHelper.MAX(0f, CTRMathHelper.MIN(1.2, 1.0 - (double)FrameworkTypes.RT((double)(num2 / 1400f), (double)num2 / 700.0)));
            }
        }

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
                    Vector vector = CTRMathHelper.vect(this.rope.drawPts[i], this.rope.drawPts[i + 1]);
                    Vector vector2 = CTRMathHelper.vect(this.rope.drawPts[i + 2], this.rope.drawPts[i + 3]);
                    float num2 = CTRMathHelper.MAX(2f * Bungee.BUNGEE_REST_LEN / 3f, CTRMathHelper.vectDistance(vector, vector2));
                    if (this.spiderPos >= num && (this.spiderPos < num + num2 || i > this.rope.drawPtsCount - 3))
                    {
                        float num3 = this.spiderPos - num;
                        Vector v = CTRMathHelper.vectSub(vector2, vector);
                        v = CTRMathHelper.vectMult(v, num3 / num2);
                        this.spider.x = vector.x + v.x;
                        this.spider.y = vector.y + v.y;
                        if (i > this.rope.drawPtsCount - 3)
                        {
                            flag = true;
                        }
                        if (this.spider.getCurrentTimelineIndex() != 0)
                        {
                            this.spider.rotation = CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(v)) + 270f;
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

        public void drawBungee()
        {
            Bungee bungee = this.rope;
            bungee?.draw();
        }

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
            bungee?.draw();
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

        public virtual void drawSpider()
        {
            this.spider.draw();
        }

        public virtual void setRope(Bungee r)
        {
            this.rope = r;
            this.radius = -1f;
            if (this.hasSpider)
            {
                this.shouldActivate = true;
            }
        }

        public virtual void setLauncher()
        {
            this.launcher = true;
            this.launcherIncreaseSpeed = true;
            this.launcherSpeed = 130f;
            Mover mover = new Mover().initWithPathCapacityMoveSpeedRotateSpeed(100, this.launcherSpeed, 0f);
            mover.setPathFromStringandStart(NSObject.NSS("RC30"), CTRMathHelper.vect(this.x, this.y));
            this.setMover(mover);
            mover.start();
        }

        public virtual void reCalcCircle()
        {
            GLDrawer.calcCircle(this.x, this.y, this.radius, this.vertexCount, this.vertices);
        }

        public virtual void setRadius(float r)
        {
            this.radius = r;
            if (this.radius == -1f)
            {
                int r2 = CTRMathHelper.RND_RANGE(76, 77);
                this.back = Image.Image_createWithResIDQuad(r2, 0);
                this.back.doRestoreCutTransparency();
                this.back.anchor = this.back.parentAnchor = 18;
                this.front = Image.Image_createWithResIDQuad(r2, 1);
                this.front.anchor = this.front.parentAnchor = 18;
                this.addChild(this.back);
                this.addChild(this.front);
                this.back.visible = false;
                this.front.visible = false;
            }
            else
            {
                this.back = Image.Image_createWithResIDQuad(74, 0);
                this.back.doRestoreCutTransparency();
                this.back.anchor = this.back.parentAnchor = 18;
                this.front = Image.Image_createWithResIDQuad(74, 1);
                this.front.anchor = this.front.parentAnchor = 18;
                this.addChild(this.back);
                this.addChild(this.front);
                this.back.visible = false;
                this.front.visible = false;
                this.radiusAlpha = 1f;
                this.hideRadius = false;
                this.vertexCount = (int)CTRMathHelper.MAX(16f, this.radius);
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
                this.wheelImage.anchor = this.wheelImage.parentAnchor = 18;
                this.addChild(this.wheelImage);
                this.wheelImage.visible = false;
                this.wheelImage2 = Image.Image_createWithResIDQuad(81, 1);
                this.wheelImage2.passTransformationsToChilds = false;
                this.wheelHighlight = Image.Image_createWithResIDQuad(81, 2);
                this.wheelHighlight.anchor = this.wheelHighlight.parentAnchor = 18;
                this.wheelImage2.addChild(this.wheelHighlight);
                this.wheelImage3 = Image.Image_createWithResIDQuad(81, 3);
                this.wheelImage3.anchor = this.wheelImage3.parentAnchor = this.wheelImage2.anchor = this.wheelImage2.parentAnchor = 18;
                this.wheelImage2.addChild(this.wheelImage3);
                this.addChild(this.wheelImage2);
                this.wheelImage2.visible = false;
                this.wheelDirty = true;
            }
        }

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
                this.moveBackground.rotationCenterX = 0f - CTRMathHelper.round((double)this.moveBackground.width / 2.0) + 74f;
                this.moveBackground.x = -74f;
                this.grabMoverHighlight = Image.Image_createWithResIDQuad(82, 3);
                this.grabMoverHighlight.visible = false;
                this.grabMoverHighlight.anchor = this.grabMoverHighlight.parentAnchor = 18;
                this.addChild(this.grabMoverHighlight);
                this.grabMover = Image.Image_createWithResIDQuad(82, 4);
                this.grabMover.visible = false;
                this.grabMover.anchor = this.grabMover.parentAnchor = 18;
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

        public virtual void setBee()
        {
            this.bee = Image.Image_createWithResIDQuad(98, 1);
            this.bee.blendingMode = 1;
            this.bee.doRestoreCutTransparency();
            this.bee.parentAnchor = 18;
            Animation animation = Animation.Animation_createWithResID(98);
            animation.parentAnchor = animation.anchor = 9;
            animation.doRestoreCutTransparency();
            animation.addAnimationDelayLoopFirstLast(0.03, Timeline.LoopType.TIMELINE_PING_PONG, 2, 4);
            animation.playTimeline(0);
            animation.jumpTo(CTRMathHelper.RND_RANGE(0, 2));
            this.bee.addChild(animation);
            Vector quadOffset = Image.getQuadOffset(98, 0);
            this.bee.x = 0f - quadOffset.x;
            this.bee.y = 0f - quadOffset.y;
            this.bee.rotationCenterX = quadOffset.x - (float)(this.bee.width / 2);
            this.bee.rotationCenterY = quadOffset.y - (float)(this.bee.height / 2);
            this.bee.scaleX = this.bee.scaleY = 0.7692308f;
            this.addChild(this.bee);
        }

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

        public virtual void destroyRope()
        {
            this.rope.release();
            this.rope = null;
        }

        public override void dealloc()
        {
            if (this.vertices != null)
            {
                NSObject.free(this.vertices);
            }
            this.destroyRope();
            base.dealloc();
        }

        public const float SPIDER_SPEED = 117f;

        public Image back;

        public Image front;

        public Bungee rope;

        public float radius;

        public float radiusAlpha;

        public bool hideRadius;

        public float[] vertices;

        public int vertexCount;

        public bool wheel;

        public Image wheelHighlight;

        public Image wheelImage;

        public Image wheelImage2;

        public Image wheelImage3;

        public int wheelOperating;

        public Vector lastWheelTouch;

        public float moveLength;

        public bool moveVertical;

        public float moveOffset;

        public HorizontallyTiledImage moveBackground;

        public Image grabMoverHighlight;

        public Image grabMover;

        public int moverDragging;

        public float minMoveValue;

        public float maxMoveValue;

        public bool hasSpider;

        public bool spiderActive;

        public Animation spider;

        public float spiderPos;

        public bool shouldActivate;

        public bool wheelDirty;

        public bool launcher;

        public float launcherSpeed;

        public bool launcherIncreaseSpeed;

        public float initial_rotation;

        public float initial_x;

        public float initial_y;

        public RotatedCircle initial_rotatedCircle;

        public bool baloon;

        public Image bee;

        private enum SPIDER_ANI
        {
            SPIDER_START_ANI,
            SPIDER_WALK_ANI,
            SPIDER_BUSTED_ANI,
            SPIDER_CATCH_ANI
        }
    }
}
