using System;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Rope anchor hook object that can appear as a fixed hook, movable hook, wheel hook, gun hook, spider hook, or suction cup hook.
    /// </summary>
    internal class Grab : CTRGameObject, ITransporterItem, ITransporterBindAware, ITransporterSideSwitchAware, ITransporterScaleAware
    {
        /// <summary>
        /// Draws the circular grab radius using the cached antialiased line vertex buffer.
        /// </summary>
        /// <param name="s">Grab whose radius vertices should be drawn.</param>
        /// <param name="color">Color used for the radius outline.</param>
        protected static void DrawGrabCircle(Grab s, RGBAColor color)
        {
            int segmentCount = s.vertexCount / 2;
            int totalVertices = segmentCount * 8;
            VertexPositionColor[] vertices = GetGrabCircleVertexCache(totalVertices);
            int writeIndex = 0;
            for (int i = 0; i < s.vertexCount; i += 2)
            {
                VertexPositionColor[] lineVertices = DrawHelper.BuildAntialiasedLineVertices(
                    s.vertices[i * 2],
                    s.vertices[(i * 2) + 1],
                    s.vertices[(i * 2) + 2],
                    s.vertices[(i * 2) + 3],
                    3f,
                    color);
                Array.Copy(lineVertices, 0, vertices, writeIndex, 8);
                writeIndex += 8;
            }
            if (writeIndex > 0)
            {
                Renderer.DrawTriangleStrip(vertices, writeIndex);
            }
        }

        /// <summary>
        /// Initializes a new grab with default rope, gun, balloon, suction cup, and stick state.
        /// </summary>
        public Grab()
        {
            rope = null;
            wheelOperating = -1;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            baloon = cTRRootController.IsSurvival();
            gun = false;
            gunFired = false;
            invisible = false;
            kicked = false;
            kickActive = false;
            stickTimer = -1f;
        }

        /// <summary>
        /// Calculates the signed angle from one point to another around a center point.
        /// </summary>
        /// <param name="v1">Starting point.</param>
        /// <param name="v2">Ending point.</param>
        /// <param name="c">Rotation center.</param>
        /// <returns>The rotation angle in degrees.</returns>
        public static float GetRotateAngleForStartEndCenter(Vector v1, Vector v2, Vector c)
        {
            Vector v3 = VectSub(v1, c);
            return RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(v2, c)) - VectAngleNormalized(v3));
        }

        /// <summary>
        /// Records the starting touch point for wheel rotation.
        /// </summary>
        /// <param name="v">World-space touch point.</param>
        public void HandleWheelTouch(Vector v)
        {
            lastWheelTouch = v;
        }

        /// <summary>
        /// Rotates a wheel hook from the last touch point and rolls the attached rope when possible.
        /// </summary>
        /// <param name="v">Current world-space touch point.</param>
        public void HandleWheelRotate(Vector v)
        {
            if (lastWheelTouch.X - v.X == 0f && lastWheelTouch.Y - v.Y == 0f)
            {
                return;
            }
            CTRSoundMgr.PlaySound(Resources.Snd.Wheel);
            float rotateDelta = GetRotateAngleForStartEndCenter(lastWheelTouch, v, Vect(x, y));
            if (rotateDelta > DEG_180)
            {
                rotateDelta -= DEG_360;
            }
            else if (rotateDelta < -DEG_180)
            {
                rotateDelta += DEG_360;
            }
            wheelImage2.rotation += rotateDelta;
            wheelImage3.rotation += rotateDelta;
            wheelHighlight.rotation += rotateDelta;
            float maxWheelDelta = ActivePhysicsConstants.GrabWheelRotateDeltaMax;
            float minWheelDelta = ActivePhysicsConstants.GrabWheelRotateDeltaMin;
            rotateDelta = rotateDelta > 0f ? MIN(MAX(minWheelDelta, rotateDelta), maxWheelDelta) : MAX(MIN(0f - minWheelDelta, rotateDelta), 0f - maxWheelDelta);
            float ropeLength = 0f;
            if (rope != null)
            {
                ropeLength = rope.GetLength();
            }
            if (rope != null)
            {
                if (rotateDelta > 0f)
                {
                    if (ropeLength < ActivePhysicsConstants.GrabRopeRollMaxLength)
                    {
                        rope.Roll(rotateDelta);
                    }
                }
                else if (rotateDelta != 0f && rope.parts.Count > 3)
                {
                    _ = rope.RollBack(0f - rotateDelta);
                }
                wheelDirty = true;
            }
            lastWheelTouch = v;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            if (gunFired && gunCup != null)
            {
                gunCup.Update(delta);
            }
            // Transported grabs keep their rope anchor pinned to grab position
            // regardless of launcher state.
            if (IsDrawnByTransporter && rope != null)
            {
                rope.bungeeAnchor.pos = Vect(x, y);
                rope.bungeeAnchor.pin = rope.bungeeAnchor.pos;
            }
            if (launcher && rope != null)
            {
                rope.bungeeAnchor.pos = Vect(x, y);
                rope.bungeeAnchor.pin = rope.bungeeAnchor.pos;
                if (launcherIncreaseSpeed)
                {
                    if (Mover.MoveVariableToTarget(ref launcherSpeed, 200, 30, delta))
                    {
                        launcherIncreaseSpeed = false;
                    }
                }
                else if (Mover.MoveVariableToTarget(ref launcherSpeed, 130, 30, delta))
                {
                    launcherIncreaseSpeed = true;
                }
                mover.SetMoveSpeed(launcherSpeed);
            }
            if (hideRadius)
            {
                radiusAlpha -= 1.5f * delta;
                if (radiusAlpha <= 0)
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
                if (ABS(vector.X) > 15f)
                {
                    float rotationTarget = 10f;
                    t = vector.X > 0f ? rotationTarget : 0f - rotationTarget;
                }
                _ = Mover.MoveVariableToTarget(ref bee.rotation, t, 60f, delta);
            }
            if (wheel && wheelDirty)
            {
                float wheelScaleLength = rope == null ? 0f : rope.GetLength() * 0.7f;
                if (wheelScaleLength == 0f)
                {
                    wheelImage2.scaleX = wheelImage2.scaleY = 0f;
                    return;
                }
                wheelImage2.scaleX = wheelImage2.scaleY = MAX(0f, MIN(1.2f, 1 - RT(wheelScaleLength / 1400f, wheelScaleLength / 700)));
            }
        }

        /// <summary>
        /// Updates spider movement along the attached rope.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the previous update.</param>
        public void UpdateSpider(float delta)
        {
            if (hasSpider && shouldActivate)
            {
                shouldActivate = false;
                spiderActive = true;
                CTRSoundMgr.PlaySound(Resources.Snd.SpiderActivate);
                spider.PlayTimeline(0);
            }
            if (!hasSpider || !spiderActive)
            {
                return;
            }
            if (spider.GetCurrentTimelineIndex() != 0)
            {
                spiderPos += delta * ActivePhysicsConstants.SpiderTraversalSpeed;
            }
            float traversedLength = 0f;
            bool flag = false;
            if (rope != null)
            {
                int i = 0;
                while (i < rope.drawPtsCount)
                {
                    Vector vector = Vect(rope.drawPts[i], rope.drawPts[i + 1]);
                    Vector vector2 = Vect(rope.drawPts[i + 2], rope.drawPts[i + 3]);
                    float segmentLength = MAX(2f * Bungee.BUNGEE_REST_LEN / 3f, VectDistance(vector, vector2));
                    if (spiderPos >= traversedLength && (spiderPos < traversedLength + segmentLength || i > rope.drawPtsCount - 3))
                    {
                        float segmentProgress = spiderPos - traversedLength;
                        Vector v = VectSub(vector2, vector);
                        v = VectMult(v, segmentProgress / segmentLength);
                        spider.x = vector.X + v.X;
                        spider.y = vector.Y + v.Y;
                        if (i > rope.drawPtsCount - 3)
                        {
                            flag = true;
                        }
                        if (spider.GetCurrentTimelineIndex() != 0)
                        {
                            spider.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(v)) + DEG_270;
                            break;
                        }
                        break;
                    }
                    else
                    {
                        traversedLength += segmentLength;
                        i += 2;
                    }
                }
            }
            if (flag)
            {
                spiderPos = -1f;
            }
        }

        /// <summary>
        /// Draws the hook background layer and optional grab-radius outline.
        /// </summary>
        public virtual void DrawBack()
        {
            if (invisible)
            {
                return;
            }
            if (kickable && kicked && rope != null)
            {
                x = (rope.bungeeAnchor.pos.X * 0.8f) + (x * 0.2f);
                y = (rope.bungeeAnchor.pos.Y * 0.8f) + (y * 0.2f);
            }
            if (gun)
            {
                return;
            }
            if (moveLength > 0)
            {
                moveBackground.Draw();
            }
            else
            {
                back.Draw();
            }
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            if (radius != -1f || hideRadius)
            {
                RGBAColor rgbaColor = RGBAColor.MakeRGBA(0.2f, 0.5f, 0.9f, radiusAlpha);
                DrawGrabCircle(this, rgbaColor);
            }
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
        }

        /// <summary>
        /// Draws the attached rope behind the grab.
        /// </summary>
        public void DrawBungee()
        {
            Bungee bungee = rope;
            bungee?.Draw();
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (invisible)
            {
                return;
            }
            if (kickable && kicked && rope != null)
            {
                x = rope.bungeeAnchor.pos.X;
                y = rope.bungeeAnchor.pos.Y;
            }
            PreDraw();
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Bungee bungee = rope;

            if (wheel)
            {
                wheelHighlight.visible = wheelOperating != -1;
                wheelImage3.visible = wheelOperating == -1;
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                wheelImage.Draw();
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            }

            if (gunBack != null)
            {
                gunBack.Draw();
                if (!gunFired && gunArrow != null)
                {
                    gunArrow.Draw();
                }
            }

            Renderer.Disable(Renderer.GL_TEXTURE_2D);

            bungee?.Draw();
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);

            // Draw front gun
            gunFront?.Draw();

            if (moveLength <= 0)
            {
                front?.Draw();
            }
            else if (moverDragging != -1)
            {
                grabMoverHighlight?.Draw();
            }
            else
            {
                grabMover?.Draw();
            }
            if (wheel)
            {
                wheelImage2.Draw();
            }
            PostDraw();
        }

        /// <summary>
        /// Draws the spider attachment animation.
        /// </summary>
        public void DrawSpider()
        {
            spider.Draw();
        }

        /// <summary>
        /// Draws the fired gun cup overlay.
        /// </summary>
        public void DrawGunCup()
        {
            if (!gunFired)
            {
                return;
            }
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            gunCup?.Draw();
        }

        /// <summary>Updates the gun body to show whether the gun can currently fire.</summary>
        /// <param name="disabled">
        /// <see langword="true"/> to show the fired body; otherwise, <see langword="false"/>.
        /// </param>
        public void SetGunDisabled(bool disabled)
        {
            gunFront?.SetDrawQuad(gunFired || disabled ? GunDisabledFrontQuad : GunFrontQuad);
        }

        /// <summary>
        /// Attaches a rope to this grab and activates spider startup when needed.
        /// </summary>
        /// <param name="r">Rope to attach.</param>
        public void SetRope(Bungee r)
        {
            rope = r;
            radius = -1f;
            if (hasSpider)
            {
                shouldActivate = true;
            }
        }

        /// <summary>
        /// Configures this grab as a launcher that oscillates along a circular path.
        /// </summary>
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

        /// <summary>
        /// Recomputes the cached grab-radius circle vertices.
        /// </summary>
        public void ReCalcCircle()
        {
            DrawHelper.CalcCircle(x, y, radius, vertexCount, vertices);
        }

        /// <summary>
        /// Configures this grab's radius and creates the visual resources for its active mode.
        /// </summary>
        /// <param name="r">Grab radius, or -1 for a fixed hook without a visible radius.</param>
        public void SetRadius(float r)
        {
            radius = r;
            if (gun)
            {
                gunBack = Image_createWithResIDQuad(Resources.Img.ObjGun, GunBackQuad);
                gunBack.DoRestoreCutTransparency();
                gunBack.anchor = gunBack.parentAnchor = 18;
                _ = AddChild(gunBack);
                gunBack.visible = false;

                gunArrow = Image_createWithResIDQuad(Resources.Img.ObjGun, GunArrowQuad);
                gunArrow.DoRestoreCutTransparency();
                gunArrow.anchor = gunArrow.parentAnchor = 18;
                _ = AddChild(gunArrow);
                gunArrow.visible = false;

                gunFront = Image_createWithResIDQuad(Resources.Img.ObjGun, GunFrontQuad);
                gunFront.DoRestoreCutTransparency();
                gunFront.anchor = gunFront.parentAnchor = 18;
                _ = AddChild(gunFront);
                gunFront.visible = false;

                gunCup = Animation_createWithResID(Resources.Img.ObjGun);
                gunCup.DoRestoreCutTransparency();
                gunCup.AddAnimationWithIDDelayLoopFirstLast(GUN_CUP_SHOW, 0.1f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 10);
                gunCup.anchor = 18;
                _ = AddChild(gunCup);
                gunCup.visible = false;

                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                gunCup.AddTimelinewithID(timeline, GUN_CUP_HIDE);

                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                timeline2.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakePos(0, 50, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
                gunCup.AddTimelinewithID(timeline2, GUN_CUP_DROP_AND_HIDE);
                Track track = timeline2.GetTrack(Track.TrackType.TRACK_POSITION);
                track.relative = true;
                return;
            }
            if (kickable)
            {
                stainCounter = MAX_STAINS;
                back = Image_createWithResIDQuad(Resources.Img.ObjSticker, 3);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(Resources.Img.ObjSticker, 4);
                front.DoRestoreCutTransparency();
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
                UpdateKickState();
            }
            else if (radius == -1f)
            {
                int hookBaseQuad = RandomHookBaseQuad();
                back = Image_createWithResIDQuad(Resources.Img.ObjHook, hookBaseQuad);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(Resources.Img.ObjHook, hookBaseQuad + 1);
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
            }
            else
            {
                back = Image_createWithResIDQuad(Resources.Img.ObjHook, HookAutoBackQuad);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(Resources.Img.ObjHook, HookAutoFrontQuad);
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
                DrawHelper.CalcCircle(x, y, radius, vertexCount, vertices);
            }
            if (wheel)
            {
                wheelImage = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadBase);
                wheelImage.anchor = wheelImage.parentAnchor = 18;
                _ = AddChild(wheelImage);
                wheelImage.visible = false;
                wheelImage2 = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadArm);
                wheelImage2.passTransformationsToChilds = false;
                wheelHighlight = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadHighlight);
                wheelHighlight.anchor = wheelHighlight.parentAnchor = 18;
                _ = wheelImage2.AddChild(wheelHighlight);
                wheelImage3 = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadIndicator);
                wheelImage3.anchor = wheelImage3.parentAnchor = wheelImage2.anchor = wheelImage2.parentAnchor = 18;
                _ = wheelImage2.AddChild(wheelImage3);
                _ = AddChild(wheelImage2);
                wheelImage2.visible = false;
                wheelDirty = true;
            }
        }

        /// <summary>
        /// Configures this grab as a movable hook along a horizontal or vertical rail.
        /// </summary>
        /// <param name="l">Movable rail length.</param>
        /// <param name="v">Whether the rail is vertical.</param>
        /// <param name="o">Offset of the grab along the rail.</param>
        public void SetMoveLengthVerticalOffset(float l, bool v, float o)
        {
            moveLength = l;
            moveVertical = v;
            moveOffset = o;
            if (moveLength > 0)
            {
                moveBackground = HorizontallyTiledImage.HorizontallyTiledImage_createWithResID(Resources.Img.ObjHook);
                moveBackground.SetTileHorizontallyLeftCenterRight(MovableRailLeftQuad, MovableRailCenterQuad, MovableRailRightQuad);
                moveBackground.width = (int)(l + 142f);
                moveBackground.rotationCenterX = 0f - Round(moveBackground.width / 2) + 74f;
                moveBackground.x = -74f;
                grabMoverHighlight = Image_createWithResIDQuad(Resources.Img.ObjHook, MovableHookHighlightQuad);
                grabMoverHighlight.visible = false;
                grabMoverHighlight.anchor = grabMoverHighlight.parentAnchor = 18;
                _ = AddChild(grabMoverHighlight);
                grabMover = Image_createWithResIDQuad(Resources.Img.ObjHook, MovableHookQuad);
                grabMover.visible = false;
                grabMover.anchor = grabMover.parentAnchor = 18;
                _ = AddChild(grabMover);
                _ = grabMover.AddChild(moveBackground);
                if (moveVertical)
                {
                    moveBackground.rotation = DEG_90;
                    moveBackground.y = 0f - moveOffset;
                    minMoveValue = y - moveOffset;
                    maxMoveValue = y + (moveLength - moveOffset);
                    grabMover.rotation = DEG_90;
                    grabMoverHighlight.rotation = DEG_90;
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
            if (moveLength >= 0f)
            {
                kickable = false;
            }
        }

        /// <summary>
        /// Adds the bee visual overlay to this grab.
        /// </summary>
        public void SetBee()
        {
            bee = Image_createWithResIDQuad(Resources.Img.ObjBee, BeeQuad);
            bee.blendingMode = 1;
            bee.DoRestoreCutTransparency();
            bee.parentAnchor = 18;
            Animation animation = Animation_createWithResID(Resources.Img.ObjBee);
            animation.parentAnchor = animation.anchor = 9;
            animation.DoRestoreCutTransparency();
            _ = animation.AddAnimationDelayLoopFirstLast(0.03f, Timeline.LoopType.TIMELINE_PING_PONG, 2, 4);
            animation.PlayTimeline(0);
            animation.JumpTo(RND_RANGE(0, 2));
            _ = bee.AddChild(animation);
            Vector quadOffset = GetQuadOffset(Resources.Img.ObjBee, 0);
            if (VectEqual(quadOffset, vectZero))
            {
                CTRTexture2D beeTexture = Application.GetTexture(Resources.Img.ObjBee);
                if (beeTexture.preCutSize.X != vectUndefined.X && beeTexture.preCutSize.Y != vectUndefined.Y)
                {
                    Vector bodyOffset = beeTexture.quadOffsets[BeeQuad];
                    CTRRectangle bodyRect = beeTexture.quadRects[BeeQuad];
                    quadOffset = Vect(bodyOffset.X + (bodyRect.w / 2f) + 6f, bodyOffset.Y + bodyRect.h + 4f);
                }
            }
            bee.x = 0f - quadOffset.X;
            bee.y = 0f - quadOffset.Y;
            bee.rotationCenterX = quadOffset.X - (bee.width / 2);
            bee.rotationCenterY = quadOffset.Y - (bee.height / 2);
            bee.scaleX = bee.scaleY = 0.77f;
            _ = AddChild(bee);
        }

        /// <summary>
        /// Configures spider support for this grab.
        /// </summary>
        /// <param name="s">Whether this grab has an attached spider.</param>
        public void SetSpider(bool s)
        {
            hasSpider = s;
            shouldActivate = false;
            spiderActive = false;
            spider = Animation_createWithResID(Resources.Img.ObjSpider);
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

        /// <summary>
        /// Disposes the attached rope and clears the rope reference.
        /// </summary>
        public void DestroyRope()
        {
            rope?.Dispose();
            rope = null;
        }

        /// <summary>
        /// Updates suction cup visuals and synchronizes position to the attached rope.
        /// </summary>
        public void UpdateKickState()
        {
            if (kicked)
            {
                back?.SetDrawQuad(1);
                front?.SetDrawQuad(2);
            }
            else
            {
                back?.SetDrawQuad(3);
                front?.SetDrawQuad(4);
            }
            if (rope != null)
            {
                x = rope.bungeeAnchor.pos.X;
                y = rope.bungeeAnchor.pos.Y;
            }
        }

        /// <inheritdoc />
        public float PositionOnTransporter { get; set; }

        /// <inheritdoc />
        public Vector BindPoint => Vect(x, y);

        /// <inheritdoc />
        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
            ReCalcCircle();
        }

        /// <inheritdoc />
        public float CollisionRadius => 40f;

        /// <inheritdoc />
        public float MinScale => 0.5f;

        /// <inheritdoc />
        public float MaxScale => 1.0f;

        /// <inheritdoc />
        public float TransporterScale { get; set; } = 1.0f;

        /// <inheritdoc />
        public bool IsDrawnByTransporter { get; set; }

        /// <inheritdoc />
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

        /// <summary>Base spider traversal speed along the rope.</summary>
        public const float SPIDER_SPEED = 117f;

        /// <summary>Timeline ID for showing the gun cup.</summary>
        public const int GUN_CUP_SHOW = 0;

        /// <summary>Timeline ID for hiding the gun cup in place.</summary>
        public const int GUN_CUP_HIDE = 1;

        /// <summary>Timeline ID for dropping and hiding the gun cup.</summary>
        public const int GUN_CUP_DROP_AND_HIDE = 2;

        /// <summary>Movement length used by suction cup behavior.</summary>
        public const int KICK_MOVE_LENGTH = 10;

        /// <summary>Cut radius used by suction cup behavior.</summary>
        public const int KICK_CUT_RADIUS = 15;

        /// <summary>Cut radius used by the gun hook.</summary>
        public const int GUN_CUT_RADIUS = 15;

        /// <summary>Tap radius used by suction cup behavior.</summary>
        public const int KICK_TAP_RADIUS = 70;

        /// <summary>Tap radius used by the gun hook.</summary>
        public const int GUN_TAP_RADIUS = 75;

        /// <summary>Delay before a sticking suction cup grab becomes active.</summary>
        public const float STICK_DELAY = 0.05f;

        /// <summary>Maximum number of stain marks available to suction cup grabs.</summary>
        public const int MAX_STAINS = 10;

        /// <inheritdoc />
        public void DidMoveToOtherSide()
        {
            if (candyNumber != -1 && rope != null && rope.cut == -1)
            {
                rope.MoveAnchor(Vect(x, y));
            }
        }

        /// <inheritdoc />
        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        /// <inheritdoc />
        public void SetTransporterScale(float scale)
        {
            if (back != null)
            {
                back.scaleX = scale;
                back.scaleY = scale;
            }

            if (front != null)
            {
                front.scaleX = scale;
                front.scaleY = scale;
            }
        }

        /// <summary>Back visual layer for the hook.</summary>
        public Image back;

        /// <summary>Front visual layer for the hook.</summary>
        public Image front;

        // public Image dot;

        /// <summary>Rope attached to this grab.</summary>
        public Bungee rope;

        /// <summary>Index of the candy attached to this grab, or -1 when no candy is attached.</summary>
        public int candyNumber = -1;

        /// <summary>Grab radius used for rope creation and radius visualization.</summary>
        public float radius;

        /// <summary>Alpha multiplier for the grab-radius visualization.</summary>
        public float radiusAlpha;

        /// <summary>Whether the grab-radius visualization is fading out.</summary>
        public bool hideRadius;

        /// <summary>Cached radius circle vertex positions.</summary>
        public float[] vertices;

        /// <summary>Number of radius circle vertices stored in <see cref="vertices"/>.</summary>
        public int vertexCount;

        /// <summary>Reusable vertex buffer used when drawing grab radius circles.</summary>
        private static VertexPositionColor[] s_grabCircleVerticesCache;

        /// <summary>
        /// Gets a reusable vertex buffer with at least the requested capacity.
        /// </summary>
        /// <param name="vertexCount">Minimum number of vertices required.</param>
        /// <returns>A reusable vertex buffer.</returns>
        private static VertexPositionColor[] GetGrabCircleVertexCache(int vertexCount)
        {
            if (s_grabCircleVerticesCache == null || s_grabCircleVerticesCache.Length < vertexCount)
            {
                s_grabCircleVerticesCache = new VertexPositionColor[vertexCount];
            }
            return s_grabCircleVerticesCache;
        }

        /// <summary>Whether this grab uses the regulated wheel hook behavior.</summary>
        public bool wheel;

        /// <summary>Highlight visual shown while the wheel hook is being operated.</summary>
        public Image wheelHighlight;

        /// <summary>Base wheel hook visual.</summary>
        public Image wheelImage;

        /// <summary>Wheel hook arm visual.</summary>
        public Image wheelImage2;

        /// <summary>Wheel hook indicator visual.</summary>
        public Image wheelImage3;

        /// <summary>Identifier for the active wheel touch, or -1 when idle.</summary>
        public int wheelOperating;

        /// <summary>Last touch point used to compute wheel rotation deltas.</summary>
        public Vector lastWheelTouch;

        /// <summary>Length of the movable hook rail.</summary>
        public float moveLength;

        /// <summary>Whether the movable hook rail is vertical.</summary>
        public bool moveVertical;

        /// <summary>Offset of the grab along its movable rail.</summary>
        public float moveOffset;

        /// <summary>Tiled rail background for a movable hook.</summary>
        public HorizontallyTiledImage moveBackground;

        /// <summary>Movable hook highlight visual.</summary>
        public Image grabMoverHighlight;

        /// <summary>Movable hook foreground visual.</summary>
        public Image grabMover;

        /// <summary>Identifier for the active movable-hook drag, or -1 when idle.</summary>
        public int moverDragging;

        /// <summary>Minimum coordinate value allowed while moving along the rail.</summary>
        public float minMoveValue;

        /// <summary>Maximum coordinate value allowed while moving along the rail.</summary>
        public float maxMoveValue;

        /// <summary>Whether this grab has a spider attachment.</summary>
        public bool hasSpider;

        /// <summary>Whether the spider attachment is currently walking along the rope.</summary>
        public bool spiderActive;

        /// <summary>Spider attachment animation.</summary>
        public Animation spider;

        /// <summary>Current spider traversal distance along the rope.</summary>
        public float spiderPos;

        /// <summary>Whether the spider should activate on the next update.</summary>
        public bool shouldActivate;

        /// <summary>Whether the wheel arm scale needs to be recomputed.</summary>
        public bool wheelDirty;

        /// <summary>Whether this grab moves as a launcher.</summary>
        public bool launcher;

        /// <summary>Current launcher movement speed.</summary>
        public float launcherSpeed;

        /// <summary>Whether launcher speed is currently increasing.</summary>
        public bool launcherIncreaseSpeed;

        /// <summary>Initial grab rotation used when restoring state.</summary>
        public float initial_rotation;

        /// <summary>Initial X position used when restoring state.</summary>
        public float initial_x;

        /// <summary>Initial Y position used when restoring state.</summary>
        public float initial_y;

        /// <summary>Initial rotated-circle binding used when restoring state.</summary>
        public RotatedCircle initial_rotatedCircle;

        /// <summary>Whether this grab uses survival balloon behavior.</summary>
        public bool baloon;

        /// <summary>Whether this grab uses gun hook behavior.</summary>
        public bool gun;

        /// <summary>Whether the gun hook has fired its cup.</summary>
        public bool gunFired;

        /// <summary>Back visual layer for the gun hook.</summary>
        private Image gunBack;

        /// <summary>Aim arrow visual for the gun hook.</summary>
        public Image gunArrow;

        /// <summary>Front visual layer for the gun hook.</summary>
        public Image gunFront;

        /// <summary>Animated suction cup fired by the gun hook.</summary>
        public Animation gunCup;

        /// <summary>Initial gun hook rotation used when restoring state.</summary>
        public float gunInitialRotation;

        /// <summary>Initial candy rotation captured when the gun hook fires.</summary>
        public float gunCandyInitialRotation;

        /// <summary>Remaining stain marks available to the suction cup hook.</summary>
        public int stainCounter;

        /// <summary>Whether this grab uses suction cup behavior.</summary>
        public bool kickable;

        /// <summary>Whether suction cup behavior has been triggered.</summary>
        public bool kicked;

        /// <summary>Whether suction cup behavior is active.</summary>
        public bool kickActive;

        /// <summary>Whether this grab should skip drawing.</summary>
        public bool invisible;

        /// <summary>Timer used by suction cup sticking behavior.</summary>
        public float stickTimer;

        /// <summary>Bee visual attached to this grab.</summary>
        public Image bee;

        /// <summary>First random fixed hook back quad.</summary>
        private const int Hook01BackQuad = 0;

        /// <summary>Second random fixed hook back quad.</summary>
        private const int Hook02BackQuad = 2;

        /// <summary>Automatic-radius hook back quad.</summary>
        private const int HookAutoBackQuad = 4;

        /// <summary>Automatic-radius hook front quad.</summary>
        private const int HookAutoFrontQuad = 5;

        /// <summary>Movable rail left cap quad.</summary>
        private const int MovableRailLeftQuad = 6;

        /// <summary>Movable rail right cap quad.</summary>
        private const int MovableRailRightQuad = 7;

        /// <summary>Movable rail center tile quad.</summary>
        private const int MovableRailCenterQuad = 8;

        /// <summary>Movable hook highlight quad.</summary>
        private const int MovableHookHighlightQuad = 9;

        /// <summary>Movable hook foreground quad.</summary>
        private const int MovableHookQuad = 10;

        /// <summary>Regulated wheel base quad.</summary>
        private const int RegulatedWheelQuadBase = 11;

        /// <summary>Regulated wheel arm quad.</summary>
        private const int RegulatedWheelQuadArm = 12;

        /// <summary>Regulated wheel highlight quad.</summary>
        private const int RegulatedWheelQuadHighlight = 13;

        /// <summary>Regulated wheel indicator quad.</summary>
        private const int RegulatedWheelQuadIndicator = 14;

        /// <summary>Bee body quad.</summary>
        private const int BeeQuad = 1;

        /// <summary>Gun hook back quad.</summary>
        private const int GunBackQuad = 0;

        /// <summary>Gun hook arrow quad.</summary>
        private const int GunArrowQuad = 1;

        /// <summary>Gun hook front quad.</summary>
        private const int GunFrontQuad = 2;

        /// <summary>Gun hook front quad used after firing and while disabled.</summary>
        private const int GunDisabledFrontQuad = 3;

        /// <summary>
        /// Selects one of the fixed hook back quad variants.
        /// </summary>
        /// <returns>The selected fixed hook back quad index.</returns>
        private static int RandomHookBaseQuad()
        {
            return RND_RANGE(0, 1) == 0 ? Hook01BackQuad : Hook02BackQuad;
        }

        /// <summary>
        /// Spider animation identifiers.
        /// </summary>
        private enum SPIDER_ANI
        {
            /// <summary>Spider start animation.</summary>
            SPIDER_START_ANI,

            /// <summary>Spider walk animation.</summary>
            SPIDER_WALK_ANI,

            /// <summary>Spider busted animation.</summary>
            SPIDER_BUSTED_ANI,

            /// <summary>Spider catch animation.</summary>
            SPIDER_CATCH_ANI
        }
    }
}
