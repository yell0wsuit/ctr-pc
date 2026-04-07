using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Physics;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
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
        /// <param name="possibleStateMask">Bitmask of ghost states that this ghost may cycle through.</param>
        /// <param name="grabRadius">Grab radius used when the ghost morphs into a grab.</param>
        /// <param name="bouncerAngle">Bouncer angle used when the ghost morphs into a bouncer.</param>
        /// <param name="bubbles">Scene bubble collection that receives ghost-created bubbles.</param>
        /// <param name="bungees">Scene grab collection that receives ghost-created grabs.</param>
        /// <param name="bouncers">Scene bouncer collection that receives ghost-created bouncers.</param>
        /// <param name="owner">Owning game scene.</param>
        /// <returns>The initialized ghost.</returns>
        public Ghost InitWithPositionPossibleStatesMaskGrabRadiusBouncerAngleBubblesBungeesBouncers(
            Vector position,
            int possibleStateMask,
            float grabRadius,
            float bouncerAngle,
            List<Bubble> bubbles,
            List<Grab> bungees,
            List<Bouncer> bouncers,
            GameScene owner)
        {
            hostScene = owner;
            possibleStatesMask = possibleStateMask | 1;
            ghostState = 1;
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

            bubble = null;
            grab = null;
            bouncer = null;
            cyclingEnabled = true;
            candyBreak = false;
            return this;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                bubble = null;
                grab = null;
                bouncer = null;
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
            if (bubble != null && bubble.GetCurrentTimelineIndex() == 11 && bubble.GetCurrentTimeline().state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                _ = gsBubbles.Remove(bubble);
                bubble = null;
            }
            if (bouncer != null && bouncer.GetCurrentTimelineIndex() == 11 && bouncer.GetCurrentTimeline().state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                _ = gsBouncers.Remove(bouncer);
                bouncer = null;
            }
            if (grab != null && grab.GetCurrentTimelineIndex() == 11 && grab.GetCurrentTimeline().state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                grab.DestroyRope();
                _ = gsBungees.Remove(grab);
                grab = null;
            }
            base.Update(delta);
            if (grab != null && grab.rope != null && grab.rope.cut != -1 && grab.GetCurrentTimelineIndex() == 10)
            {
                cyclingEnabled = true;
                ResetToState(1);
            }
        }

        /// <summary>
        /// Morphs the ghost into a specific allowed state.
        /// </summary>
        /// <param name="newState">Ghost state bit to activate.</param>
        public void ResetToState(int newState)
        {
            if ((newState & possibleStatesMask) == 0)
            {
                return;
            }
            ghostState = newState;
            Timeline morphOut = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            morphOut.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            morphOut.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, GHOST_MORPHING_DISAPPEAR_TIME));
            morphOut.delegateTimelineDelegate = this;
            if (bubble != null)
            {
                if (bubble.GetCurrentTimelineIndex() == 11)
                {
                    _ = gsBubbles.Remove(bubble);
                    bubble = null;
                }
                else
                {
                    bubble.AddTimelinewithID(morphOut, 11);
                    bubble.PlayTimeline(11);
                    bubble.popped = true;
                }
            }
            if (grab != null)
            {
                Bungee rope = grab.rope;
                if (rope != null)
                {
                    grab.rope.forceWhite = true;
                    rope.cutTime = GHOST_MORPHING_APPEAR_TIME;
                    if (rope.cut == -1)
                    {
                        rope.cut = 0;
                    }
                }
                if (grab.GetCurrentTimelineIndex() == 11)
                {
                    grab.DestroyRope();
                    _ = gsBungees.Remove(grab);
                    grab = null;
                }
                else
                {
                    grab.AddTimelinewithID(morphOut, 11);
                    grab.PlayTimeline(11);
                }
            }
            if (bouncer != null)
            {
                if (bouncer.GetCurrentTimelineIndex() == 11)
                {
                    _ = gsBouncers.Remove(bouncer);
                    bouncer = null;
                }
                else
                {
                    bouncer.AddTimelinewithID(morphOut, 11);
                    bouncer.PlayTimeline(11);
                }
            }
            if (ghostImage.GetCurrentTimelineIndex() == 10)
            {
                ghostImage.PlayTimeline(11);
            }

            Timeline morphIn = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            morphIn.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0));
            morphIn.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, GHOST_MORPHING_APPEAR_TIME));

            switch (ghostState)
            {
                case 1:
                    ghostImage.PlayTimeline(10);
                    break;
                case 2:
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
                        bubble = ghostBubble;
                        gsBubbles.Add(ghostBubble);
                        ghostBubble.passColorToChilds = true;
                        ghostBubble.AddTimelinewithID(morphIn, 10);
                        ghostBubble.PlayTimeline(10);
                        ghostBubble.AddSupportingCloudsTimelines();
                        break;
                    }
                case 3:
                    break;
                case 4:
                    grab = new GhostGrab().InitWithPosition(x, y);
                    grab.wheel = false;
                    grab.spider = null;
                    grab.SetRadius(grabRadius);
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
                        }
                    }
                    gsBungees.Add(grab);
                    grab.AddTimelinewithID(morphIn, 10);
                    grab.PlayTimeline(10);
                    break;
                case 8:
                    bouncer = new GhostBouncer().InitWithPosXYWidthAndAngle(x, y, 1, bouncerAngle);
                    gsBouncers.Add(bouncer);
                    bouncer.AddTimelinewithID(morphIn, 10);
                    bouncer.PlayTimeline(10);
                    break;
                default:
                    break;
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
            if ((possibleStatesMask & ~1) == 0)
            {
                return;
            }

            int state = ghostState;
            do
            {
                state <<= 1;
                if (state == 16)
                {
                    state = 2;
                }
            }
            while ((state & possibleStatesMask) == 0);

            // With only 1 non-idle property, the cycle wraps back to the current state.
            // Calling ResetToState would orphan the existing object in its list before
            // its disappear animation completes, so bail out instead.
            if (state == ghostState)
            {
                return;
            }

            ResetToState(state);
        }

        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            float distance = VectLength(VectSub(Vect(tx, ty), Vect(x, y)));
            if (cyclingEnabled && !candyBreak && distance < GHOST_TOUCH_RADIUS)
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

        /// <summary>Duration of the ghost morph-in fade, in seconds.</summary>
        private const float GHOST_MORPHING_APPEAR_TIME = 0.36f;

        /// <summary>Duration of the ghost morph-out fade, in seconds.</summary>
        private const float GHOST_MORPHING_DISAPPEAR_TIME = 0.16f;

        /// <summary>Number of particles emitted when the ghost morphs.</summary>
        private const int GHOST_MORPHING_BUBBLES_COUNT = 7;

        /// <summary>Touch radius used for cycling ghost state.</summary>
        private const float GHOST_TOUCH_RADIUS = 80f;

        /// <summary>Current ghost state bit.</summary>
        public int ghostState;

        /// <summary>Bubble object currently owned by the ghost state.</summary>
        public Bubble bubble;

        /// <summary>Grab object currently owned by the ghost state.</summary>
        public Grab grab;

        /// <summary>Bouncer object currently owned by the ghost state.</summary>
        public Bouncer bouncer;

        /// <summary>Whether touch input may cycle this ghost to another state.</summary>
        public bool cyclingEnabled;

        /// <summary>Grab radius used when the ghost morphs into a grab.</summary>
        public float grabRadius;

        /// <summary>Whether the candy has broken and should block ghost cycling.</summary>
        public bool candyBreak;

        /// <summary>Bitmask of ghost states this ghost may cycle through.</summary>
        public int possibleStatesMask;

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
    }
}
