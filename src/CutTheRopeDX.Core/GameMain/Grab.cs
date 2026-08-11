using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

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
            AutoRadiusSource source = s.RadiusSource;
            if (source == null)
            {
                return;
            }

            int segmentCount = source.VertexCount / 2;
            int totalVertices = segmentCount * 8;
            VertexPositionColor[] vertices = GetGrabCircleVertexCache(totalVertices);
            int writeIndex = 0;
            for (int i = 0; i < source.VertexCount; i += 2)
            {
                VertexPositionColor[] lineVertices = DrawHelper.BuildAntialiasedLineVertices(
                    source.Vertices[i * 2],
                    source.Vertices[(i * 2) + 1],
                    source.Vertices[(i * 2) + 2],
                    source.Vertices[(i * 2) + 3],
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

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            Source.Update(delta);
            // Transported grabs keep their rope anchor pinned to grab position.
            if (IsDrawnByTransporter)
            {
                SyncRopeAnchor();
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
            Wheel?.UpdateArmScale(this);
        }

        /// <summary>
        /// Draws the hook background layer and optional grab-radius outline.
        /// </summary>
        public virtual void DrawBack()
        {
            if (IsInvisible)
            {
                return;
            }
            if (Mount?.IsMounted == false)
            {
                Mount.SyncBackPosition(this);
            }
            if (GunSource != null)
            {
                return;
            }
            if (Rail != null)
            {
                Rail.Background.Draw();
            }
            else
            {
                back.Draw();
            }
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            if (RadiusSource?.ShouldDrawCircle == true)
            {
                RGBAColor rgbaColor = RGBAColor.MakeRGBA(0.2f, 0.5f, 0.9f, RadiusSource.RadiusAlpha);
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
            Bungee bungee = Rope;
            bungee?.Draw();
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (IsInvisible)
            {
                return;
            }
            if (Mount?.IsMounted == false)
            {
                Mount.SyncFrontPosition(this);
            }
            PreDraw();
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Bungee bungee = Rope;

            if (Wheel != null)
            {
                Wheel.Highlight.visible = Wheel.OperatingTouch != -1;
                Wheel.Indicator.visible = Wheel.OperatingTouch == -1;
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                Wheel.Base.Draw();
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            }

            if (GunSource is GunSource gunSource && gunSource.Back != null)
            {
                gunSource.Back.Draw();
                if (!gunSource.HasFired && gunSource.Arrow != null)
                {
                    gunSource.Arrow.Draw();
                }
            }

            Renderer.Disable(Renderer.GL_TEXTURE_2D);

            bungee?.Draw();
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);

            // Draw front gun
            GunSource?.Front?.Draw();

            if (Rail == null)
            {
                front?.Draw();
            }
            else if (Rail.DraggingTouch != -1)
            {
                Rail.MoverHighlight?.Draw();
            }
            else
            {
                Rail.Mover?.Draw();
            }
            Wheel?.Arm.Draw();
            PostDraw();
        }

        /// <summary>
        /// Attaches a rope to this grab and activates spider startup when needed.
        /// </summary>
        /// <param name="r">Rope to attach.</param>
        public void SetRope(Bungee r)
        {
            _ = Attachment.TryAttach(r);
            Spider?.Arm(ropeAttachedToCandy: true);
        }

        /// <summary>
        /// Returns the hook texture atlas for this grab's attached rope.
        /// </summary>
        /// <returns>The chain hook atlas for chain bungees; otherwise the default hook atlas.</returns>
        internal string GetHookTextureResource()
        {
            return Rope?.cutOnlyByAxe == true ? Resources.Img.ObjHookChain : Resources.Img.ObjHook;
        }

        /// <summary>
        /// Gets the rectangle around this grab that a cut stroke may not cut inside, or
        /// <see langword="null"/> when the whole rope is cuttable. A wheel and a gun each protect
        /// their own tap zone so operating them cannot sever the rope they control.
        /// </summary>
        public CTRRectangle? CutExclusionZone =>
            Wheel != null
                ? new CTRRectangle(
                    x - WheelControl.TapHalfExtent, y - WheelControl.TapHalfExtent,
                    WheelControl.TapHalfExtent * 2f, WheelControl.TapHalfExtent * 2f)
                : Source is GunSource
                    ? new CTRRectangle(
                        x - GUN_CUT_RADIUS, y - GUN_CUT_RADIUS,
                        GUN_CUT_RADIUS * 2f, GUN_CUT_RADIUS * 2f)
                    : null;

        /// <summary>
        /// Reacts to this grab's own rope being cut. The single place a hook's components learn about
        /// it; every cut site used to inline the spider and gun-cup handling itself.
        /// </summary>
        /// <param name="reason">Why the rope was cut.</param>
        public void OnRopeCut(RopeCutReason reason)
        {
            if (Spider?.ShouldBustOnRopeCut == true)
            {
                Spider.Bust();
            }

            Source.OnRopeCut(reason);
        }

        /// <summary>
        /// Pins this grab's rope anchor to its own position. Five separate copies of these two lines
        /// used to exist - transporter, launcher, path mover, disc rotation and rail drag - and they
        /// were byte-identical.
        /// </summary>
        public void SyncRopeAnchor()
        {
            Bungee attached = Rope;
            if (attached == null)
            {
                return;
            }

            attached.bungeeAnchor.pos = Vect(x, y);
            attached.bungeeAnchor.pin = attached.bungeeAnchor.pos;
        }

        /// <summary>
        /// Recomputes the cached grab-radius circle vertices.
        /// </summary>
        public void ReCalcCircle()
        {
            Source.OnAnchorMoved(Vect(x, y));
        }

        /// <summary>
        /// Creates the visual resources for whichever axes this grab was resolved onto. Purely
        /// visual: <see cref="GrabAxisResolver"/> has already decided which axes exist.
        /// </summary>
        public void CreateAxisVisuals()
        {
            if (Source is GunSource gunSource)
            {
                gunSource.Back = Image_createWithResIDQuad(Resources.Img.ObjGun, GunBackQuad);
                gunSource.Back.DoRestoreCutTransparency();
                gunSource.Back.anchor = gunSource.Back.parentAnchor = 18;
                _ = AddChild(gunSource.Back);
                gunSource.Back.visible = false;

                gunSource.Arrow = Image_createWithResIDQuad(Resources.Img.ObjGun, GunArrowQuad);
                gunSource.Arrow.DoRestoreCutTransparency();
                gunSource.Arrow.anchor = gunSource.Arrow.parentAnchor = 18;
                _ = AddChild(gunSource.Arrow);
                gunSource.Arrow.visible = false;

                gunSource.Front = Image_createWithResIDQuad(Resources.Img.ObjGun, GunFrontQuad);
                gunSource.Front.DoRestoreCutTransparency();
                gunSource.Front.anchor = gunSource.Front.parentAnchor = 18;
                _ = AddChild(gunSource.Front);
                gunSource.Front.visible = false;

                gunSource.Cup = Animation_createWithResID(Resources.Img.ObjGun);
                gunSource.Cup.DoRestoreCutTransparency();
                gunSource.Cup.AddAnimationWithIDDelayLoopFirstLast(GUN_CUP_SHOW, 0.1f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 10);
                gunSource.Cup.anchor = 18;
                _ = AddChild(gunSource.Cup);
                gunSource.Cup.visible = false;

                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                gunSource.Cup.AddTimelinewithID(timeline, GUN_CUP_HIDE);

                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
                timeline2.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline2.AddKeyFrame(KeyFrame.MakePos(0, 50, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
                gunSource.Cup.AddTimelinewithID(timeline2, GUN_CUP_DROP_AND_HIDE);
                Track track = timeline2.GetTrack(Track.TrackType.TRACK_POSITION);
                track.relative = true;
                return;
            }
            if (Mount != null)
            {
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
            else if (Source is PreAttachedSource)
            {
                string hookTexture = GetHookTextureResource();
                int hookBaseQuad = hookTexture == Resources.Img.ObjHookChain ? Hook01BackQuad : RandomHookBaseQuad();
                back = Image_createWithResIDQuad(hookTexture, hookBaseQuad);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(hookTexture, hookBaseQuad + 1);
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
            }
            else
            {
                // A chain auto-hook (breakable="false") uses the dedicated chain auto-hook atlas.
                string autoTexture = IsChainAnchor ? Resources.Img.ObjHookAutoChain : Resources.Img.ObjHook;
                int autoBackQuad = IsChainAnchor ? HookAutoChainBackQuad : HookAutoBackQuad;
                int autoFrontQuad = IsChainAnchor ? HookAutoChainFrontQuad : HookAutoFrontQuad;
                back = Image_createWithResIDQuad(autoTexture, autoBackQuad);
                back.DoRestoreCutTransparency();
                back.anchor = back.parentAnchor = 18;
                front = Image_createWithResIDQuad(autoTexture, autoFrontQuad);
                front.anchor = front.parentAnchor = 18;
                _ = AddChild(back);
                _ = AddChild(front);
                back.visible = false;
                front.visible = false;
            }

            if (Wheel is WheelControl wheelControl)
            {
                wheelControl.Base = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadBase);
                wheelControl.Base.anchor = wheelControl.Base.parentAnchor = 18;
                _ = AddChild(wheelControl.Base);
                wheelControl.Base.visible = false;
                wheelControl.Arm = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadArm);
                wheelControl.Arm.passTransformationsToChilds = false;
                wheelControl.Highlight = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadHighlight);
                wheelControl.Highlight.anchor = wheelControl.Highlight.parentAnchor = 18;
                _ = wheelControl.Arm.AddChild(wheelControl.Highlight);
                wheelControl.Indicator = Image_createWithResIDQuad(Resources.Img.ObjHook, RegulatedWheelQuadIndicator);
                wheelControl.Indicator.anchor = wheelControl.Indicator.parentAnchor = wheelControl.Arm.anchor = wheelControl.Arm.parentAnchor = 18;
                _ = wheelControl.Arm.AddChild(wheelControl.Indicator);
                _ = AddChild(wheelControl.Arm);
                wheelControl.Arm.visible = false;
            }
        }

        /// <summary>
        /// Creates the rail's three images, when this grab was resolved onto a rail. Purely visual:
        /// the rail's geometry belongs to <see cref="RailMotion"/>.
        /// </summary>
        public void CreateRailVisuals()
        {
            if (Rail is not RailMotion rail)
            {
                return;
            }

            float l = rail.Length;
            bool v = rail.IsVertical;
            float o = rail.Offset;

            HorizontallyTiledImage moveBackground = HorizontallyTiledImage.HorizontallyTiledImage_createWithResID(Resources.Img.ObjHook);
            moveBackground.SetTileHorizontallyLeftCenterRight(MovableRailLeftQuad, MovableRailCenterQuad, MovableRailRightQuad);
            moveBackground.width = (int)(l + 142f);
            moveBackground.rotationCenterX = 0f - Round(moveBackground.width / 2) + 74f;
            moveBackground.x = -74f;
            Image grabMoverHighlight = Image_createWithResIDQuad(Resources.Img.ObjHook, MovableHookHighlightQuad);
            grabMoverHighlight.visible = false;
            grabMoverHighlight.anchor = grabMoverHighlight.parentAnchor = 18;
            _ = AddChild(grabMoverHighlight);
            Image grabMover = Image_createWithResIDQuad(Resources.Img.ObjHook, MovableHookQuad);
            grabMover.visible = false;
            grabMover.anchor = grabMover.parentAnchor = 18;
            _ = AddChild(grabMover);
            _ = grabMover.AddChild(moveBackground);
            if (v)
            {
                moveBackground.rotation = DEG_90;
                moveBackground.y = 0f - o;
                grabMover.rotation = DEG_90;
                grabMoverHighlight.rotation = DEG_90;
            }
            else
            {
                moveBackground.x += 0f - o;
            }
            moveBackground.anchor = 17;
            moveBackground.x += x;
            moveBackground.y += y;
            moveBackground.visible = false;

            rail.Background = moveBackground;
            rail.Mover = grabMover;
            rail.MoverHighlight = grabMoverHighlight;
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

        /// <summary>Attaches a spider to this grab.</summary>
        public void SetSpider()
        {
            Animation spiderAnimation = Animation_createWithResID(Resources.Img.ObjSpider);
            spiderAnimation.DoRestoreCutTransparency();
            spiderAnimation.anchor = 18;
            spiderAnimation.x = x;
            spiderAnimation.y = y;
            spiderAnimation.visible = false;
            spiderAnimation.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 6);
            spiderAnimation.SetDelayatIndexforAnimation(0.4f, 5, 0);
            spiderAnimation.AddAnimationWithIDDelayLoopFirstLast(1, 0.1f, Timeline.LoopType.TIMELINE_REPLAY, 7, 10);
            spiderAnimation.SwitchToAnimationatEndOfAnimationDelay(1, 0, 0.05f);
            _ = AddChild(spiderAnimation);
            Spider = new SpiderRider { Animation = spiderAnimation };
        }

        /// <summary>
        /// Disposes the attached rope and clears the rope reference.
        /// </summary>
        public void DestroyRope()
        {
            Attachment.Release();
        }

        /// <summary>Switches the suction cup images between their stuck and detached quads.</summary>
        public void UpdateKickState()
        {
            bool detached = Mount?.IsMounted == false;
            back?.SetDrawQuad(detached ? 1 : 3);
            front?.SetDrawQuad(detached ? 2 : 4);
            if (Rope != null)
            {
                x = Rope.bungeeAnchor.pos.X;
                y = Rope.bungeeAnchor.pos.Y;
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
                DestroyRope();
                bee?.Dispose();
                bee = null;
                Spider?.Animation?.Dispose();
                Spider = null;
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
            if (Rope != null && Rope.cut == -1)
            {
                Rope.MoveAnchor(Vect(x, y));
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

        /// <summary>Gets the authority for this grab's rope and its condition.</summary>
        public RopeAttachment Attachment { get; } = new();

        /// <summary>Gets the rope attached to this grab, or <see langword="null"/> when there is none.</summary>
        public Bungee Rope => Attachment.Rope;

        /// <summary>Gets the object that decides whether this grab can produce a rope.</summary>
        public RopeSource Source { get; internal set; } = new PreAttachedSource();

        /// <summary>Gets this grab's radius source, or <see langword="null"/> when it has none.</summary>
        public AutoRadiusSource RadiusSource => Source as AutoRadiusSource;

        /// <summary>Gets or sets this grab's independent traits.</summary>
        public HookModifiers Modifiers { get; internal set; } = HookModifiers.None;

        /// <summary>
        /// Gets whether this grab is a chain (<c>breakable="false"</c>): it renders with the chain
        /// hook sprites and any rope it creates can only be cut by the axe. For auto-attaching grabs
        /// (those with a radius) this drives the <see cref="Resources.Img.ObjHookAutoChain"/> variant
        /// and is applied to the rope created on attach.
        /// </summary>
        public bool IsChainAnchor => Modifiers.HasFlag(HookModifiers.ChainAnchor);

        /// <summary>Gets whether this grab should skip drawing.</summary>
        public bool IsInvisible => Modifiers.HasFlag(HookModifiers.Invisible);

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

        /// <summary>Gets this grab's wheel, or <see langword="null"/> when it has none.</summary>
        public WheelControl Wheel { get; internal set; }

        /// <summary>Gets the object that supplies this grab's position.</summary>
        public AnchorMotion Motion { get; internal set; } = new StaticMotion();

        /// <summary>Gets this grab's rail, or <see langword="null"/> when it has none.</summary>
        public RailMotion Rail => Motion as RailMotion;

        /// <summary>Gets this grab's spider, or <see langword="null"/> when it has none.</summary>
        public SpiderRider Spider { get; internal set; }

        /// <summary>Initial grab rotation used when restoring state.</summary>
        public float initial_rotation;

        /// <summary>Initial X position used when restoring state.</summary>
        public float initial_x;

        /// <summary>Initial Y position used when restoring state.</summary>
        public float initial_y;

        /// <summary>Initial rotated-circle binding used when restoring state.</summary>
        public RotatedCircle initial_rotatedCircle;

        /// <summary>Gets this grab's gun source, or <see langword="null"/> when it is not a gun.</summary>
        public GunSource GunSource => Source as GunSource;

        /// <summary>Gets this grab's suction mount, or <see langword="null"/> when it has none.</summary>
        public SuctionMount Mount { get; internal set; }

        /// <summary>Bee visual attached to this grab.</summary>
        public Image bee;

        /// <summary>Automatic-radius hook back quad.</summary>
        private const int HookAutoBackQuad = 4;

        /// <summary>Automatic-radius hook front quad.</summary>
        private const int HookAutoFrontQuad = 5;

        /// <summary>Automatic-radius chain hook back quad (in the <see cref="Resources.Img.ObjHookAutoChain"/> atlas).</summary>
        private const int HookAutoChainBackQuad = 0;

        /// <summary>Automatic-radius chain hook front quad (in the <see cref="Resources.Img.ObjHookAutoChain"/> atlas).</summary>
        private const int HookAutoChainFrontQuad = 1;

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

        /// <summary>First random fixed hook back quad.</summary>
        private const int Hook01BackQuad = 0;

        /// <summary>Second random fixed hook back quad.</summary>
        private const int Hook02BackQuad = 2;

        /// <summary>Gun hook back quad.</summary>
        private const int GunBackQuad = 0;

        /// <summary>Gun hook arrow quad.</summary>
        internal const int GunArrowQuad = 1;

        /// <summary>Gun hook front quad.</summary>
        internal const int GunFrontQuad = 2;

        /// <summary>Gun hook front quad used after firing and while disabled.</summary>
        internal const int GunDisabledFrontQuad = 3;

        /// <summary>Selects one of the two fixed-hook sprite pairs.</summary>
        /// <returns>The selected back-layer quad.</returns>
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
