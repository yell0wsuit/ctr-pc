using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
    internal sealed class Grab : CTRGameObject
    {
        private static void DrawGrabCircle(Grab s, float x, float y, float radius, int vertexCount, RGBAColor color)
        {
            OpenGL.GlColor4f(color.ToXNA());
            OpenGL.GlLineWidth(3.0);
            OpenGL.GlDisableClientState(0);
            OpenGL.GlEnableClientState(13);
            OpenGL.GlColorPointer_setAdditive(s.vertexCount * 8);
            OpenGL.GlVertexPointer_setAdditive(2, 5, 0, s.vertexCount * 16);
            for (int i = 0; i < s.vertexCount; i += 2)
            {
                GLDrawer.DrawAntialiasedLine(s.vertices[i * 2], s.vertices[(i * 2) + 1], s.vertices[(i * 2) + 2], s.vertices[(i * 2) + 3], 3f, color);
            }
            OpenGL.GlDrawArrays(8, 0, 8);
            OpenGL.GlEnableClientState(0);
            OpenGL.GlDisableClientState(13);
            OpenGL.GlLineWidth(1.0);
        }

        public Grab()
        {
            rope = null;
            wheelOperating = -1;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            baloon = cTRRootController.IsSurvival();
        }

        public static float GetRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
        {
            Vector v3 = VectSub(v1, c);
            return RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(v2, c)) - VectAngleNormalized(v3));
        }

        public void HandleWheelTouch(Vector v)
        {
            lastWheelTouch = v;
        }

        public void HandleWheelRotate(Vector v)
        {
            if (lastWheelTouch.x - v.x == 0f && lastWheelTouch.y - v.y == 0f)
            {
                return;
            }
            CTRSoundMgr.PlaySound(36);
            float num = GetRotateAngleForStartEndCenter(lastWheelTouch, v, Vect(x, y));
            if ((double)num > 180.0)
            {
                num -= 360f;
            }
            else if ((double)num < -180.0)
            {
                num += 360f;
            }
            wheelImage2.rotation += num;
            wheelImage3.rotation += num;
            wheelHighlight.rotation += num;
            num = num > 0f ? MIN((double)MAX(1.0, (double)num), 4.5) : MAX((double)MIN(-1.0, (double)num), -4.5);
            float num2 = 0f;
            if (rope != null)
            {
                num2 = rope.GetLength();
            }
            if (rope != null)
            {
                if (num > 0f)
                {
                    if (num2 < 1650f)
                    {
                        rope.Roll(num);
                    }
                }
                else if (num != 0f && rope.parts.Count > 3)
                {
                    _ = rope.RollBack(0f - num);
                }
                wheelDirty = true;
            }
            lastWheelTouch = v;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (launcher && rope != null)
            {
                rope.bungeeAnchor.pos = Vect(x, y);
                rope.bungeeAnchor.pin = rope.bungeeAnchor.pos;
                if (launcherIncreaseSpeed)
                {
                    if (Mover.MoveVariableToTarget(ref launcherSpeed, 200.0, 30.0, (double)delta))
                    {
                        launcherIncreaseSpeed = false;
                    }
                }
                else if (Mover.MoveVariableToTarget(ref launcherSpeed, 130.0, 30.0, (double)delta))
                {
                    launcherIncreaseSpeed = true;
                }
                mover.SetMoveSpeed(launcherSpeed);
            }
            if (hideRadius)
            {
                radiusAlpha -= 1.5f * delta;
                if (radiusAlpha <= 0.0)
                {
                    radius = -1f;
                    hideRadius = false;
                }
            }
            if (bee != null)
            {
                Vector vector2 = mover.path[mover.targetPoint];
                Vector pos = mover.pos;
                Vector vector = VectSub(vector2, pos);
                float t = 0f;
                if (ABS(vector.x) > 15f)
                {
                    float num = 10f;
                    t = vector.x > 0f ? num : 0f - num;
                }
                _ = Mover.MoveVariableToTarget(ref bee.rotation, t, 60f, delta);
            }
            if (wheel && wheelDirty)
            {
                float num2 = rope == null ? 0f : rope.GetLength() * 0.7f;
                if (num2 == 0f)
                {
                    wheelImage2.scaleX = wheelImage2.scaleY = 0f;
                    return;
                }
                wheelImage2.scaleX = wheelImage2.scaleY = MAX(0f, MIN(1.2, 1.0 - (double)RT((double)(num2 / 1400f), (double)num2 / 700.0)));
            }
        }

        public void UpdateSpider(float delta)
        {
            if (hasSpider && shouldActivate)
            {
                shouldActivate = false;
                spiderActive = true;
                CTRSoundMgr.PlaySound(33);
                spider.PlayTimeline(0);
            }
            if (!hasSpider || !spiderActive)
            {
                return;
            }
            if (spider.GetCurrentTimelineIndex() != 0)
            {
                spiderPos += delta * 117f;
            }
            float num = 0f;
            bool flag = false;
            if (rope != null)
            {
                int i = 0;
                while (i < rope.drawPtsCount)
                {
                    Vector vector = Vect(rope.drawPts[i], rope.drawPts[i + 1]);
                    Vector vector2 = Vect(rope.drawPts[i + 2], rope.drawPts[i + 3]);
                    float num2 = MAX(2f * Bungee.BUNGEE_REST_LEN / 3f, VectDistance(vector, vector2));
                    if (spiderPos >= num && (spiderPos < num + num2 || i > rope.drawPtsCount - 3))
                    {
                        float num3 = spiderPos - num;
                        Vector v = VectSub(vector2, vector);
                        v = VectMult(v, num3 / num2);
                        spider.x = vector.x + v.x;
                        spider.y = vector.y + v.y;
                        if (i > rope.drawPtsCount - 3)
                        {
                            flag = true;
                        }
                        if (spider.GetCurrentTimelineIndex() != 0)
                        {
                            spider.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(v)) + 270f;
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
                spiderPos = -1f;
            }
        }

        public void DrawBack()
        {
            if (moveLength > 0.0)
            {
                moveBackground.Draw();
            }
            else
            {
                back.Draw();
            }
            OpenGL.GlDisable(0);
            if (radius != -1f || hideRadius)
            {
                RGBAColor rgbaColor = RGBAColor.MakeRGBA(0.2, 0.5, 0.9, radiusAlpha);
                DrawGrabCircle(this, x, y, radius, vertexCount, rgbaColor);
            }
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
        }

        public void DrawBungee()
        {
            Bungee bungee = rope;
            bungee?.Draw();
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlEnable(0);
            Bungee bungee = rope;
            if (wheel)
            {
                wheelHighlight.visible = wheelOperating != -1;
                wheelImage3.visible = wheelOperating == -1;
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                wheelImage.Draw();
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            }
            OpenGL.GlDisable(0);
            bungee?.Draw();
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            if (moveLength <= 0.0)
            {
                front.Draw();
            }
            else if (moverDragging != -1)
            {
                grabMoverHighlight.Draw();
            }
            else
            {
                grabMover.Draw();
            }
            if (wheel)
            {
                wheelImage2.Draw();
            }
            PostDraw();
        }

        public void DrawSpider()
        {
            spider.Draw();
        }

        public void SetRope(Bungee r)
        {
            rope = r;
            radius = -1f;
            if (hasSpider)
            {
                shouldActivate = true;
            }
        }

        public void SetLauncher()
        {
            launcher = true;
            launcherIncreaseSpeed = true;
            launcherSpeed = 130f;
            Mover mover = new(100, launcherSpeed, 0f);
            mover.SetPathFromStringandStart("RC30", Vect(x, y));
            SetMover(mover);
            mover.Start();
        }

        public void ReCalcCircle()
        {
            GLDrawer.CalcCircle(x, y, radius, vertexCount, vertices);
        }

        public void SetRadius(float r)
        {
            radius = r;
            if (radius == -1f)
            {
                int r2 = RND_RANGE(76, 77);
                back = Image_createWithResIDQuad(r2, 0);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(r2, 1);
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
            }
            else
            {
                back = Image_createWithResIDQuad(74, 0);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(74, 1);
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
                radiusAlpha = 1f;
                hideRadius = false;
                vertexCount = (int)MAX(16f, radius);
                vertexCount /= 2;
                if (vertexCount % 2 != 0)
                {
                    vertexCount++;
                }
                vertices = new float[vertexCount * 2];
                GLDrawer.CalcCircle(x, y, radius, vertexCount, vertices);
            }
            if (wheel)
            {
                wheelImage = Image_createWithResIDQuad(81, 0);
                wheelImage.anchor = wheelImage.parentAnchor = 18;
                _ = AddChild(wheelImage);
                wheelImage.visible = false;
                wheelImage2 = Image_createWithResIDQuad(81, 1);
                wheelImage2.passTransformationsToChilds = false;
                wheelHighlight = Image_createWithResIDQuad(81, 2);
                wheelHighlight.anchor = wheelHighlight.parentAnchor = 18;
                _ = wheelImage2.AddChild(wheelHighlight);
                wheelImage3 = Image_createWithResIDQuad(81, 3);
                wheelImage3.anchor = wheelImage3.parentAnchor = wheelImage2.anchor = wheelImage2.parentAnchor = 18;
                _ = wheelImage2.AddChild(wheelImage3);
                _ = AddChild(wheelImage2);
                wheelImage2.visible = false;
                wheelDirty = true;
            }
        }

        public void SetMoveLengthVerticalOffset(float l, bool v, float o)
        {
            moveLength = l;
            moveVertical = v;
            moveOffset = o;
            if (moveLength > 0.0)
            {
                moveBackground = HorizontallyTiledImage.HorizontallyTiledImage_createWithResID(82);
                moveBackground.SetTileHorizontallyLeftCenterRight(0, 2, 1);
                moveBackground.width = (int)(l + 142f);
                moveBackground.rotationCenterX = 0f - Round(moveBackground.width / 2.0) + 74f;
                moveBackground.x = -74f;
                grabMoverHighlight = Image_createWithResIDQuad(82, 3);
                grabMoverHighlight.visible = false;
                grabMoverHighlight.anchor = grabMoverHighlight.parentAnchor = 18;
                _ = AddChild(grabMoverHighlight);
                grabMover = Image_createWithResIDQuad(82, 4);
                grabMover.visible = false;
                grabMover.anchor = grabMover.parentAnchor = 18;
                _ = AddChild(grabMover);
                _ = grabMover.AddChild(moveBackground);
                if (moveVertical)
                {
                    moveBackground.rotation = 90f;
                    moveBackground.y = 0f - moveOffset;
                    minMoveValue = y - moveOffset;
                    maxMoveValue = y + (moveLength - moveOffset);
                    grabMover.rotation = 90f;
                    grabMoverHighlight.rotation = 90f;
                }
                else
                {
                    minMoveValue = x - moveOffset;
                    maxMoveValue = x + (moveLength - moveOffset);
                    moveBackground.x += 0f - moveOffset;
                }
                moveBackground.anchor = 17;
                moveBackground.x += x;
                moveBackground.y += y;
                moveBackground.visible = false;
            }
            moverDragging = -1;
        }

        public void SetBee()
        {
            bee = Image_createWithResIDQuad(98, 1);
            bee.blendingMode = 1;
            bee.DoRestoreCutTransparency();
            bee.parentAnchor = 18;
            Animation animation = Animation_createWithResID(98);
            animation.parentAnchor = animation.anchor = 9;
            animation.DoRestoreCutTransparency();
            _ = animation.AddAnimationDelayLoopFirstLast(0.03, Timeline.LoopType.TIMELINE_PING_PONG, 2, 4);
            animation.PlayTimeline(0);
            animation.JumpTo(RND_RANGE(0, 2));
            _ = bee.AddChild(animation);
            Vector quadOffset = GetQuadOffset(98, 0);
            bee.x = 0f - quadOffset.x;
            bee.y = 0f - quadOffset.y;
            bee.rotationCenterX = quadOffset.x - (bee.width / 2);
            bee.rotationCenterY = quadOffset.y - (bee.height / 2);
            bee.scaleX = bee.scaleY = 0.7692308f;
            _ = AddChild(bee);
        }

        public void SetSpider(bool s)
        {
            hasSpider = s;
            shouldActivate = false;
            spiderActive = false;
            spider = Animation_createWithResID(64);
            spider.DoRestoreCutTransparency();
            spider.anchor = 18;
            spider.x = x;
            spider.y = y;
            spider.visible = false;
            spider.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 6);
            spider.SetDelayatIndexforAnimation(0.4f, 5, 0);
            spider.AddAnimationWithIDDelayLoopFirstLast(1, 0.1f, Timeline.LoopType.TIMELINE_REPLAY, 7, 10);
            spider.SwitchToAnimationatEndOfAnimationDelay(1, 0, 0.05f);
            _ = AddChild(spider);
        }

        public void DestroyRope()
        {
            rope?.Dispose();
            rope = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertices != null)
                {
                    vertices = null;
                }
                DestroyRope();
                bee?.Dispose();
                bee = null;
                spider?.Dispose();
                spider = null;
            }
            base.Dispose(disposing);
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
