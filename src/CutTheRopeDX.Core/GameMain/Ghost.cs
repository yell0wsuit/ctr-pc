using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Interactive ghost that can transform between idle, bubble, grab, and bouncer states.
    /// </summary>
    internal sealed class Ghost : BaseElement, ITimelineDelegate
    {
        /// <summary>
        /// Initializes a ghost and its morphing visuals at a level position.
        /// </summary>
        /// <param name="position">World position of the ghost.</param>
        /// <param name="possibleForms">Ghost forms that this ghost may cycle through.</param>
        /// <param name="grabRadius">Grab radius used when the ghost morphs into a grab.</param>
        /// <param name="bouncerAngle">Bouncer angle used when the ghost morphs into a bouncer.</param>
        /// <param name="bubbles">Scene bubble collection that receives ghost-created bubbles.</param>
        /// <param name="bungees">Scene grab collection that receives ghost-created grabs.</param>
        /// <param name="bouncers">Scene bouncer collection that receives ghost-created bouncers.</param>
        /// <param name="owner">Owning game scene.</param>
        /// <returns>The initialized ghost.</returns>
        public Ghost InitWithPositionPossibleFormsGrabRadiusBouncerAngleBubblesBungeesBouncers(
            Vector position,
            GhostForm possibleForms,
            float grabRadius,
            float bouncerAngle,
            List<Bubble> bubbles,
            List<Grab> bungees,
            List<Bouncer> bouncers,
            GameScene owner)
        {
            hostScene = owner;
            this.possibleForms = possibleForms | GhostForm.Idle;
            Form = GhostForm.Idle;
            Apparition = null;
            MorphPhase = null;
            retiringApparitions.Clear();
            this.bouncerAngle = bouncerAngle;
            this.grabRadius = grabRadius;
            gsBubbles = bubbles;
            gsBungees = bungees;
            gsBouncers = bouncers;
            x = position.X;
            y = position.Y;
            ghostImage = new BaseElement();
            _ = AddChild(ghostImage);
            morphingBubbles = new GhostMorphingParticles().InitWithTotalParticles(7);
            morphingBubbles.x = position.X;
            morphingBubbles.y = position.Y;
            _ = AddChild(morphingBubbles);

            Timeline appearTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            appearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            appearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, GHOST_MORPHING_APPEAR_TIME));
            ghostImage.AddTimelinewithID(appearTimeline, 10);
            ghostImage.PlayTimeline(10);

            Timeline disappearTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            disappearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            disappearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, GHOST_MORPHING_DISAPPEAR_TIME));
            ghostImage.AddTimelinewithID(disappearTimeline, 11);

            float random = RND_0_1;

            ghostImageBody = Image.Image_createWithResIDQuad(Resources.Img.ObjGhost, 0);
            ghostImageBody.x = position.X;
            ghostImageBody.y = position.Y;
            ghostImageBody.anchor = 18;
            _ = ghostImage.AddChild(ghostImageBody);

            Timeline bodyFloat = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            bodyFloat.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            bodyFloat.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 3), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, random));
            bodyFloat.delegateTimelineDelegate = this;
            ghostImageBody.AddTimelinewithID(bodyFloat, 13);
            ghostImageBody.PlayTimeline(13);

            ghostImageFace = Image.Image_createWithResIDQuad(Resources.Img.ObjGhost, 1);
            ghostImageFace.x = position.X;
            ghostImageFace.y = position.Y;
            ghostImageFace.anchor = 18;
            _ = ghostImage.AddChild(ghostImageFace);

            Timeline faceFloat = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            faceFloat.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            faceFloat.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 2), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, random + 0.005f));
            faceFloat.delegateTimelineDelegate = this;
            ghostImageFace.AddTimelinewithID(faceFloat, 13);
            ghostImageFace.PlayTimeline(13);

            return this;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Apparition = null;
                MorphPhase = null;
                retiringApparitions.Clear();
                ghostImageBody = null;
                ghostImageFace = null;
                ghostImage = null;
                morphingBubbles = null;
                morphingCloud = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            RetireFinishedApparitions();
            base.Update(delta);
            CompleteMorphIfFinished();
            if (Apparition is GhostGrab grab
                && grab.Rope != null
                && grab.Rope.cut != -1
                && grab.GetCurrentTimelineIndex() == 10)
            {
                ResetToForm(GhostForm.Idle);
            }
        }

        /// <summary>
        /// Morphs the ghost into a specific allowed state.
        /// </summary>
        /// <param name="newForm">Ghost form to activate.</param>
        public void ResetToForm(GhostForm newForm)
        {
            if (!IsSingleForm(newForm) || (newForm & possibleForms) == 0)
            {
                return;
            }

            GhostForm outgoingForm = Form;
            IGhostApparition outgoing = Apparition;
            MorphPhase = new GhostMorphPhase(outgoingForm, newForm);
            Apparition = null;
            if (outgoing != null)
            {
                BeginRetiringApparition(outgoing);
                retiringApparitions.Add(new RetiringGhostApparition(outgoingForm, newForm, outgoing));
            }
            else if (ghostImage.GetCurrentTimelineIndex() == 10)
            {
                ghostImage.PlayTimeline(11);
            }

            Form = newForm;
            Timeline morphIn = CreateMorphTimeline(appearing: true);
            switch (newForm)
            {
                case GhostForm.Idle:
                    ghostImage.PlayTimeline(10);
                    break;
                case GhostForm.Bubble:
                    {
                        GhostBubble ghostBubble = GhostBubble.CreateWithResIDQuad(Resources.Img.ObjBubble, RND_RANGE(1, 3));
                        ghostBubble.DoRestoreCutTransparency();
                        ghostBubble.bb = GameScene.GetBubbleBoundingBox();
                        ghostBubble.x = x;
                        ghostBubble.y = y;
                        ghostBubble.anchor = 18;
                        ghostBubble.popped = false;
                        Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjBubble, 0);
                        image.DoRestoreCutTransparency();
                        image.parentAnchor = image.anchor = 18;
                        _ = ghostBubble.AddChild(image);
                        Apparition = ghostBubble;
                        gsBubbles.Add(ghostBubble);
                        ghostBubble.passColorToChilds = true;
                        ghostBubble.AddTimelinewithID(morphIn, 10);
                        ghostBubble.PlayTimeline(10);
                        ghostBubble.AddSupportingCloudsTimelines();
                        break;
                    }
                case GhostForm.Grab:
                    GhostGrab grab = new GhostGrab().InitWithPosition(x, y);
                    grab.Wheel = null;
                    grab.Spider = null;
                    // A ghost apparition is only ever a plain or auto-radius hook, so it goes
                    // through the same resolver as an authored one.
                    grab.Source = GrabAxisResolver.Resolve(
                        new GrabAxisRequest(
                            Gun: false, Wheel: false, Kickable: false,
                            Radius: grabRadius, MoveLength: 0f, HasMover: false,
                            MoveVertical: false, MoveOffset: 0f, AnchorX: x, AnchorY: y)).Source;
                    grab.CreateAxisVisuals();
                    if (grabRadius == -1f)
                    {
                        ConstraintedPoint ropeAnchor = hostScene?.GetGhostRopeAnchor(Vect(x, y));
                        if (ropeAnchor != null)
                        {
                            Vector anchorPos = ropeAnchor.pos;
                            float ropeLength = VectLength(VectSub(Vect(x, y), anchorPos));
                            if (ropeLength <= 0f)
                            {
                                ropeLength = Bungee.BUNGEE_REST_LEN;
                            }
                            Bungee autoRope = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                                null,
                                x,
                                y,
                                ropeAnchor,
                                anchorPos.X,
                                anchorPos.Y,
                                ropeLength);
                            autoRope.bungeeAnchor.pin = autoRope.bungeeAnchor.pos;
                            grab.SetRope(autoRope);
                            hostScene?.RegisterRope(autoRope, grab);
                        }
                    }
                    gsBungees.Add(grab);
                    Apparition = grab;
                    grab.AddTimelinewithID(morphIn, 10);
                    grab.PlayTimeline(10);
                    break;
                case GhostForm.Bouncer:
                    GhostBouncer bouncer = (GhostBouncer)new GhostBouncer().InitWithPosXYWidthAndAngle(x, y, 1, bouncerAngle);
                    gsBouncers.Add(bouncer);
                    Apparition = bouncer;
                    bouncer.AddTimelinewithID(morphIn, 10);
                    bouncer.PlayTimeline(10);
                    break;
                case GhostForm.None:
                default:
                    throw new InvalidOperationException($"Unsupported ghost form {newForm}.");
            }

            morphingBubbles.StartSystem(GHOST_MORPHING_BUBBLES_COUNT);
            CTRSoundMgr.PlaySound(Resources.Snd.GhostPuff);
        }

        /// <summary>
        /// Cycles the ghost to the next allowed non-idle state.
        /// </summary>
        public void ResetToNextState()
        {
            // No non-idle states available; nothing to cycle to.
            if ((possibleForms & ~GhostForm.Idle) == 0)
            {
                return;
            }

            GhostForm nextForm = Form;
            do
            {
                nextForm = (GhostForm)((int)nextForm << 1);
                if ((int)nextForm == 16)
                {
                    nextForm = GhostForm.Bubble;
                }
            }
            while ((nextForm & possibleForms) == 0);

            // With only 1 non-idle property, the cycle wraps back to the current state.
            // Re-entering the only form would produce a visual puff without changing behavior,
            // so bail out instead.
            if (nextForm == Form)
            {
                return;
            }

            ResetToForm(nextForm);
        }

        /// <summary>Whether this ghost's current apparition is <paramref name="candidate"/>.</summary>
        public bool OwnsBubble(Bubble candidate)
        {
            return Apparition is GhostBubble bubble && ReferenceEquals(bubble, candidate);
        }

        /// <summary>
        /// Releases the exact ghost bubble claimed by candy and returns the ghost to its idle form.
        /// </summary>
        /// <returns><see langword="true"/> when this ghost owned the supplied bubble.</returns>
        public bool ReleaseBubble(Bubble candidate)
        {
            if (!OwnsBubble(candidate))
            {
                return false;
            }

            ResetToForm(GhostForm.Idle);
            return true;
        }

        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            float distance = VectLength(VectSub(Vect(tx, ty), Vect(x, y)));
            if (!IsBubbleCaptured && distance < GHOST_TOUCH_RADIUS)
            {
                ResetToNextState();
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline timeline, KeyFrame keyFrame, int index)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline timeline)
        {
            if (timeline.element == ghostImageFace)
            {
                Timeline faceLoop = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                faceLoop.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                faceLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 2), KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
                faceLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.38f));
                faceLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y + 2), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.38f));
                faceLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.38f));
                faceLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 2), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.38f));
                ghostImageFace.AddTimelinewithID(faceLoop, 12);
                ghostImageFace.PlayTimeline(12);
            }
            if (timeline.element == ghostImageBody)
            {
                Timeline bodyLoop = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                bodyLoop.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                bodyLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 3), KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
                bodyLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.38f));
                bodyLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y + 3), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.38f));
                bodyLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.38f));
                bodyLoop.AddKeyFrame(KeyFrame.MakePos((int)x, (int)(y - 3), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.38f));
                ghostImageBody.AddTimelinewithID(bodyLoop, 12);
                ghostImageBody.PlayTimeline(12);
            }
        }

        private bool IsBubbleCaptured => Apparition is GhostBubble bubble
            && hostScene?.IsBubbleClaimedByCandy(bubble) == true;

        private static bool IsSingleForm(GhostForm candidate)
        {
            int value = (int)candidate;
            return value > 0 && (value & (value - 1)) == 0;
        }

        private static Timeline CreateMorphTimeline(bool appearing)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                appearing ? RGBAColor.transparentRGBA : RGBAColor.solidOpaqueRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE,
                0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                appearing ? RGBAColor.solidOpaqueRGBA : RGBAColor.transparentRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                appearing ? GHOST_MORPHING_APPEAR_TIME : GHOST_MORPHING_DISAPPEAR_TIME));
            return timeline;
        }

        private void BeginRetiringApparition(IGhostApparition outgoing)
        {
            if (outgoing is GhostBubble bubble)
            {
                bubble.popped = true;
            }
            else if (outgoing is GhostGrab grab && grab.Rope != null)
            {
                grab.Rope.forceWhite = true;
                grab.Rope.cutTime = GHOST_MORPHING_APPEAR_TIME;
                if (grab.Rope.cut == -1)
                {
                    grab.Rope.cut = 0;
                }
            }

            BaseElement element = outgoing.Element;
            Timeline morphOut = CreateMorphTimeline(appearing: false);
            morphOut.delegateTimelineDelegate = this;
            element.AddTimelinewithID(morphOut, 11);
            element.PlayTimeline(11);
        }

        private void RetireFinishedApparitions()
        {
            for (int i = retiringApparitions.Count - 1; i >= 0; i--)
            {
                RetiringGhostApparition retirement = retiringApparitions[i];
                BaseElement element = retirement.Apparition.Element;
                if (element.GetCurrentTimelineIndex() != 11
                    || element.GetCurrentTimeline()?.state != Timeline.TimelineState.TIMELINE_STOPPED)
                {
                    continue;
                }

                RetireApparition(retirement.Apparition);
                retiringApparitions.RemoveAt(i);
            }
        }

        private void RetireApparition(IGhostApparition retired)
        {
            switch (retired)
            {
                case GhostBubble bubble:
                    _ = gsBubbles.Remove(bubble);
                    break;
                case GhostGrab grab:
                    hostScene?.UnregisterRope(grab.Rope);
                    grab.DestroyRope();
                    _ = gsBungees.Remove(grab);
                    break;
                case GhostBouncer bouncer:
                    _ = gsBouncers.Remove(bouncer);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported ghost apparition {retired.GetType().Name}.");
            }
        }

        private void CompleteMorphIfFinished()
        {
            if (MorphPhase == null)
            {
                return;
            }

            BaseElement incoming = Apparition?.Element ?? ghostImage;
            if (incoming.GetCurrentTimelineIndex() == 10
                && incoming.GetCurrentTimeline()?.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                MorphPhase = null;
            }
        }

        /// <summary>Duration of the ghost morph-in fade, in seconds.</summary>
        private const float GHOST_MORPHING_APPEAR_TIME = 0.36f;

        /// <summary>Duration of the ghost morph-out fade, in seconds.</summary>
        private const float GHOST_MORPHING_DISAPPEAR_TIME = 0.16f;

        /// <summary>Number of particles emitted when the ghost morphs.</summary>
        private const int GHOST_MORPHING_BUBBLES_COUNT = 7;

        /// <summary>Touch radius used for cycling ghost state.</summary>
        private const float GHOST_TOUCH_RADIUS = 80f;

        /// <summary>Authoritative current form.</summary>
        internal GhostForm Form { get; private set; }

        /// <summary>The one current non-idle apparition, if any.</summary>
        internal IGhostApparition Apparition { get; private set; }

        /// <summary>The outgoing/incoming form pair while the newest morph-in is active.</summary>
        internal GhostMorphPhase? MorphPhase { get; private set; }

        /// <summary>Grab radius used when the ghost morphs into a grab.</summary>
        public float grabRadius;

        /// <summary>Bouncer angle used when the ghost morphs into a bouncer.</summary>
        public float bouncerAngle;

        /// <summary>Root element for the ghost body and face images.</summary>
        public BaseElement ghostImage;

        /// <summary>Ghost body image.</summary>
        public Image ghostImageBody;

        /// <summary>Ghost face image.</summary>
        public Image ghostImageFace;

        /// <summary>Scene bubble collection that receives ghost-created bubbles.</summary>
        public List<Bubble> gsBubbles;

        /// <summary>Scene grab collection that receives ghost-created grabs.</summary>
        public List<Grab> gsBungees;

        /// <summary>Scene bouncer collection that receives ghost-created bouncers.</summary>
        public List<Bouncer> gsBouncers;

        /// <summary>Particles emitted during ghost morph transitions.</summary>
        public GhostMorphingParticles morphingBubbles;

        /// <summary>Cloud effect emitted during ghost morph transitions.</summary>
        public GhostMorphingCloud morphingCloud;

        /// <summary>Owning game scene used for ghost-created rope anchors.</summary>
        private GameScene hostScene;

        /// <summary>Allowed forms for this ghost, including idle.</summary>
        private GhostForm possibleForms;

        /// <summary>Outgoing apparitions waiting for safe post-iteration retirement.</summary>
        private readonly List<RetiringGhostApparition> retiringApparitions = [];
    }
}
