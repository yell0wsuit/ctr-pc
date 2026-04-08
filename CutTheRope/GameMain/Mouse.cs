using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Physics;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a single mouse character that can grab, carry, and release candy
    /// within a game scene. Manages animation, movement, and interaction logic.
    /// </summary>
    internal sealed class Mouse : BaseElement, ITimelineDelegate
    {
        /// <summary>
        /// Plays mouth movement animations along a predefined path to simulate
        /// candy entry and exit movements.
        /// </summary>
        private sealed class MouthPathPlayer
        {
            /// <summary>
            /// Path points currently being played.
            /// </summary>
            private readonly List<PathPoint> path = [];

            /// <summary>
            /// Total path duration in seconds.
            /// </summary>
            private float duration;

            /// <summary>
            /// Elapsed path playback time in seconds.
            /// </summary>
            private float elapsed;

            /// <summary>
            /// Current interpolated offset along the path.
            /// </summary>
            private Vector currentOffset;

            /// <summary>
            /// Starts playing a path animation with the specified path points.
            /// </summary>
            /// <param name="newPath">The list of path points to animate through.</param>
            public void Play(List<PathPoint> newPath)
            {
                path.Clear();
                path.AddRange(newPath);
                duration = path.Count > 0 ? path[^1].Time : 0f;
                elapsed = 0f;
                IsPlaying = duration > 0f;
                currentOffset = path.Count > 0 ? path[0].Offset : default;
            }

            /// <summary>
            /// Updates the path animation and returns the current offset.
            /// </summary>
            /// <param name="delta">Elapsed time since the last update, in seconds.</param>
            /// <returns>The current position offset along the path.</returns>
            public Vector Update(float delta)
            {
                if (!IsPlaying || path.Count == 0)
                {
                    return currentOffset;
                }

                elapsed += delta;
                if (elapsed >= duration)
                {
                    elapsed = duration;
                    IsPlaying = false;
                    currentOffset = path[^1].Offset;
                    return currentOffset;
                }

                int startIndex = 0;
                while (startIndex < path.Count - 1 && path[startIndex + 1].Time <= elapsed)
                {
                    startIndex++;
                }

                PathPoint start = path[startIndex];
                PathPoint end = startIndex + 1 < path.Count ? path[startIndex + 1] : start;
                float span = end.Time - start.Time;
                if (span <= 0.0001f)
                {
                    return currentOffset;
                }

                float t = (elapsed - start.Time) / span;
                t = t < 0f ? 0f : (t > 1f ? 1f : t);
                currentOffset = new Vector(
                    start.Offset.X + ((end.Offset.X - start.Offset.X) * t),
                    start.Offset.Y + ((end.Offset.Y - start.Offset.Y) * t));
                return currentOffset;
            }

            /// <summary>
            /// Gets a value indicating whether the path animation is currently playing.
            /// </summary>
            public bool IsPlaying { get; private set; }
        }

        /// <summary>
        /// Contains shared sprite resources used across all mouse instances
        /// to optimize memory and rendering.
        /// </summary>
        internal struct SharedMouseSprites
        {
            /// <summary>
            /// The root container element for all mouse visual components.
            /// </summary>
            public BaseElement Container;

            /// <summary>
            /// The animation element for the mouse body.
            /// </summary>
            public Animation Body;

            /// <summary>
            /// The animation element for the mouse eyes (blinking).
            /// </summary>
            public Animation Eyes;
        }

        /// <summary>
        /// Represents a point along a movement path with timing information.
        /// </summary>
        /// <param name="offset">Position offset at this path point.</param>
        /// <param name="time">Time in seconds when this path point should be reached.</param>
        private readonly struct PathPoint(Vector offset, float time)
        {
            /// <summary>
            /// Gets the position offset at this path point.
            /// </summary>
            public Vector Offset { get; } = offset;

            /// <summary>
            /// Gets the time in seconds when this path point should be reached.
            /// </summary>
            public float Time { get; } = time;
        }

        /// <summary>
        /// Identifiers for mouse animation states, matching iOS BoxGap timeline indices.
        /// </summary>
        private enum MouseAnimationId
        {
            /// <summary>Entry animation without candy.</summary>
            EntryEmpty = 0,

            /// <summary>Entry animation while carrying candy.</summary>
            EntryWithCandy = 1,

            /// <summary>Idle animation without candy.</summary>
            IdleEmpty = 2,

            /// <summary>Idle animation while carrying candy.</summary>
            Idle = 3,

            /// <summary>Exit animation without candy.</summary>
            ExitEmpty = 4,

            /// <summary>Exit animation while carrying candy.</summary>
            ExitWithCandy = 5,

            /// <summary>Bounce animation used while active.</summary>
            Bounce = 6
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class.
        /// </summary>
        /// <param name="manager">The manager that controls this mouse.</param>
        public Mouse(MiceObject manager)
        {
            this.manager = manager;
            index = 0;
            grabRadius = 0f;
            activeDuration = 0f;
            angleDeg = 0f;
            IsActive = false;
            retreating = false;
            elapsedActive = 0f;
            carriedStar = null;
            carriedCandy = null;
            grabAnimating = false;
            entryOffsets = new Vector[4];
            exitOffsets = new Vector[3];
            mouthPathPlayer = new MouthPathPlayer();
            sharedSprites = null;
            mouseGroup = new BaseElement
            {
                anchor = 18,
                parentAnchor = 18
            };
            holeSprite = Image.Image_createWithResIDQuad(Resources.Img.ObjMouse, HoleQuad);
            holeSprite.anchor = 18;
            holeSprite.parentAnchor = 18;
            holeSprite.scaleX = 1f;
            holeSprite.scaleY = 1f;
            holeSprite.DoRestoreCutTransparency();
            holeSprite.blendingMode = 1; // Blend mode 1 (GL_ONE, GL_ONE_MINUS_SRC_ALPHA): for premultiplied alpha textures
            _ = AddChild(mouseGroup);
        }

        /// <summary>
        /// Initializes the mouse's position, appearance, and behavior parameters.
        /// </summary>
        /// <param name="px">X-coordinate of the mouse hole position.</param>
        /// <param name="py">Y-coordinate of the mouse hole position.</param>
        /// <param name="angle">Angle in degrees for the mouse's orientation.</param>
        /// <param name="radius">Grab radius for candy interaction detection.</param>
        /// <param name="duration">Maximum active duration before automatic retreat.</param>
        public void Initialize(float px, float py, float angle, float radius, float duration)
        {
            x = px;
            y = py;
            angleDeg = angle;
            grabRadius = radius;
            activeDuration = duration;
            IsActive = false;
            retreating = false;
            elapsedActive = 0f;
            carriedStar = null;
            carriedCandy = null;

            float angleRad = DEGREES_TO_RADIANS(angleDeg);
            Vector origin = default;
            Vector Rotate(Vector v)
            {
                return VectRotate(v, angleRad);
            }

            entryOffsets[0] = VectAdd(origin, Rotate(new Vector(0f, -4.4f)));

            // candy position after being grabbed
            entryOffsets[1] = VectAdd(origin, Rotate(new Vector(0f, -61f)));
            entryOffsets[2] = VectAdd(origin, Rotate(new Vector(0f, -70f)));
            entryOffsets[3] = VectAdd(origin, Rotate(new Vector(0f, -66f)));

            exitOffsets[0] = VectAdd(origin, Rotate(new Vector(0f, -36.4f)));
            exitOffsets[1] = VectAdd(origin, Rotate(new Vector(0f, -43.2f)));
            exitOffsets[2] = VectAdd(origin, Rotate(new Vector(0f, -9.2f)));

            // Create bounce timeline (iOS timeline 6): mouse pops out slightly then returns
            const float bounceHeight = 142f; // Reference height from mouse sprite
            Vector bounceOut = Rotate(new Vector(0f, -bounceHeight * 0.15f * 0.45f));
            Vector bounceBack = Rotate(new Vector(0f, bounceHeight * 0.15f * 0.4f));

            bounceTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(8);
            bounceTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakePos((int)bounceOut.X, (int)bounceOut.Y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakePos((int)bounceBack.X, (int)bounceBack.Y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
            bounceTimeline.AddKeyFrame(KeyFrame.MakePos(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.05f));
        }

        /// <summary>
        /// Spawns the mouse with shared sprite resources, optionally carrying candy.
        /// Plays the entry animation and sets up initial state.
        /// </summary>
        /// <param name="sprites">Shared sprite resources for rendering.</param>
        /// <param name="carriedCandy">Optional candy object to carry from spawn.</param>
        /// <param name="carriedStar">Optional star point constraint for the candy.</param>
        public void Spawn(SharedMouseSprites sprites, GameObject carriedCandy, ConstraintedPoint carriedStar)
        {
            sharedSprites = sprites;
            this.carriedCandy = carriedCandy;
            this.carriedStar = carriedStar;

            mouseGroup.RemoveAllChilds();
            _ = mouseGroup.AddChild(sprites.Container);
            sprites.Container.parent = mouseGroup;

            if (bounceTimeline != null)
            {
                sprites.Container.AddTimelinewithID(bounceTimeline, (int)MouseAnimationId.Bounce);
            }

            if (carriedCandy != null && carriedStar == null)
            {
                Vector offset = entryOffsets[3];
                carriedCandy.x = x + offset.X;
                carriedCandy.y = y + offset.Y;
            }

            PlayAnimation(carriedCandy != null ? MouseAnimationId.EntryWithCandy : MouseAnimationId.EntryEmpty);
            retreating = false;
            IsActive = false;
            elapsedActive = 0f;
            grabAnimating = false;

            sprites.Eyes.visible = false;

            if (carriedStar != null)
            {
                AttachExistingCandy(carriedStar, carriedCandy);
            }

            CTRSoundMgr.PlaySound(Resources.Snd.MouseRustle);
        }

        /// <summary>
        /// Checks if a target point is within the mouse's grab radius.
        /// </summary>
        /// <param name="target">The constrained point to check.</param>
        /// <returns>
        /// <see langword="true" /> if the target is within grab range; otherwise <see langword="false" />.
        /// </returns>
        public bool IsWithinGrabRadius(ConstraintedPoint target)
        {
            return VectDistance(Vect(x, y), target.pos) < grabRadius;
        }

        /// <summary>
        /// Commands the mouse to grab candy from a star point, disabling gravity
        /// and initiating the grab animation.
        /// </summary>
        /// <param name="star">The constrained star point to attach.</param>
        /// <param name="candy">The candy game object being grabbed.</param>
        public void GrabCandy(ConstraintedPoint star, GameObject candy)
        {
            carriedStar = star;
            carriedCandy = candy;

            star.disableGravity = true;
            star.v = default;
            Vector offset = entryOffsets[3];
            star.pos = Vect(x + offset.X, y + offset.Y);
            star.prevPos = star.pos;
            mouthPathPlayer.Play(CreateEntryPath());
            grabAnimating = true;

            SharedMouseSprites? sprites = sharedSprites;
            if (sprites.HasValue)
            {
                sprites.Value.Body.SetDrawQuad(CandyInMouthQuad);
                sprites.Value.Container.PlayTimeline((int)MouseAnimationId.Bounce);
            }

            CTRSoundMgr.PlaySound(Resources.Snd.MouseIdle);
        }

        /// <summary>
        /// Releases the currently carried candy, re-enabling gravity and clearing references.
        /// </summary>
        public void DropCandy()
        {
            if (carriedStar == null)
            {
                return;
            }

            carriedStar.disableGravity = false;
            carriedStar.prevPos = carriedStar.pos;
            carriedStar = null;
            carriedCandy = null;
            grabAnimating = false;
            CTRSoundMgr.PlaySound(Resources.Snd.MouseTap);
        }

        /// <summary>
        /// Drops the carried candy and initiates the mouse's retreat animation.
        /// </summary>
        public void DropCandyAndRetreat()
        {
            DropCandy();
            BeginRetreat();
        }

        /// <summary>
        /// Begins the retreat animation, deactivating the mouse and playing
        /// the appropriate exit animation based on whether candy is held.
        /// </summary>
        public void BeginRetreat()
        {
            if (retreating)
            {
                return;
            }

            SharedMouseSprites? sprites = sharedSprites;

            bool hasCandy = carriedStar != null;

            // iOS: only hide eyes when exiting empty (not when exiting with candy)
            if (!hasCandy && sprites.HasValue)
            {
                sprites.Value.Eyes.visible = false;
            }

            retreating = true;
            IsActive = false;
            elapsedActive = 0f;
            grabAnimating = false;

            // iOS: remove bounce timeline on retreat
            if (sprites.HasValue)
            {
                sprites.Value.Container.RemoveTimeline((int)MouseAnimationId.Bounce);
            }

            mouthPathPlayer.Play(CreateExitPath());
            PlayAnimation(hasCandy ? MouseAnimationId.ExitWithCandy : MouseAnimationId.ExitEmpty);
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);

            SharedMouseSprites? sprites = sharedSprites;
            if (sprites.HasValue && sprites.Value.Container.parent == mouseGroup)
            {
                sprites.Value.Container.rotation = angleDeg;
                sprites.Value.Container.scaleX = 1f;
                sprites.Value.Container.scaleY = 1f;
            }

            Vector mouthOffset = mouthPathPlayer.Update(delta);
            if (grabAnimating && !mouthPathPlayer.IsPlaying)
            {
                grabAnimating = false;
            }

            if (carriedStar != null)
            {
                carriedStar.pos = Vect(x + mouthOffset.X, y + mouthOffset.Y);
                carriedStar.prevPos = carriedStar.pos;
            }

            if (IsActive && !retreating && !grabAnimating)
            {
                elapsedActive += delta;
                if (elapsedActive >= activeDuration && !IsBounceTimelinePlaying())
                {
                    BeginRetreat();
                }
            }

            mouseGroup.x = x;
            mouseGroup.y = y;
        }

        /// <summary>
        /// Attaches an already-grabbed candy to the mouse, disabling gravity
        /// and initiating the entry path animation. Used when transferring
        /// candy between mice.
        /// </summary>
        /// <param name="star">The constrained star point to attach.</param>
        /// <param name="candy">The candy game object to attach.</param>
        public void AttachExistingCandy(ConstraintedPoint star, GameObject candy)
        {
            carriedStar = star;
            carriedCandy = candy;
            star.disableGravity = true;
            star.v = default;
            Vector offset = entryOffsets[3];
            star.pos = Vect(x + offset.X, y + offset.Y);
            star.prevPos = star.pos;
            mouthPathPlayer.Play(CreateEntryPath());
            grabAnimating = true;
        }

        /// <summary>
        /// Gets a value indicating whether the mouse is currently carrying candy.
        /// </summary>
        public bool HasCandy => carriedStar != null;

        /// <summary>
        /// Determines whether the mouse can be clicked at the specified coordinates
        /// to drop the candy.
        /// </summary>
        /// <param name="clickX">X-coordinate of the click.</param>
        /// <param name="clickY">Y-coordinate of the click.</param>
        /// <returns>
        /// <see langword="true" /> if the mouse is active, has candy, not retreating, and the
        /// click is within grab radius; otherwise <see langword="false" />.
        /// </returns>
        public bool IsClickable(float clickX, float clickY)
        {
            return IsActive && carriedStar != null && !retreating && VectDistance(Vect(x, y), Vect(clickX, clickY)) < grabRadius;
        }

        /// <summary>
        /// Renders the mouse hole sprite at the mouse's position.
        /// </summary>
        public void DrawHole()
        {
            holeSprite.x = x;
            holeSprite.y = y;
            holeSprite.Draw();
        }

        /// <summary>
        /// Renders the mouse and its associated sprites.
        /// </summary>
        public void DrawMouse()
        {
            mouseGroup.Draw();
        }

        /// <summary>
        /// Locks the mouse in an inactive and retreating state, preventing
        /// further interactions.
        /// </summary>
        public void Lock()
        {
            retreating = true;
            IsActive = false;
        }

        /// <summary>
        /// Cleans up the mouse state and removes sprite resources, resetting
        /// all carried items and animations.
        /// </summary>
        public void Cleanup()
        {
            retreating = true;
            IsActive = false;
            grabAnimating = false;
            carriedStar = null;
            carriedCandy = null;

            if (sharedSprites.HasValue)
            {
                mouseGroup.RemoveChild(sharedSprites.Value.Container);
                sharedSprites = null;
            }
            else
            {
                mouseGroup.RemoveAllChilds();
            }
        }

        /// <summary>
        /// Detaches and returns the currently carried candy and star, clearing
        /// internal references. Used when transferring candy between mice.
        /// </summary>
        /// <returns>
        /// A tuple containing the carried star point and candy object, both of which may be <see langword="null"/>.
        /// </returns>
        public (ConstraintedPoint star, GameObject candy) DetachCarriedCandy()
        {
            ConstraintedPoint star = carriedStar;
            GameObject candy = carriedCandy;
            carriedStar = null;
            carriedCandy = null;
            return (star, candy);
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            SharedMouseSprites? sprites = sharedSprites;
            MouseAnimationId currentId = sprites.HasValue && sprites.Value.Body.GetCurrentTimelineIndex() >= 0
                ? (MouseAnimationId)sprites.Value.Body.GetCurrentTimelineIndex()
                : MouseAnimationId.Idle;

            switch (currentId)
            {
                case MouseAnimationId.EntryEmpty:
                    // iOS case 0: become active, 50% chance of idle pose + eyes blink
                    elapsedActive = 0f;
                    IsActive = true;
                    if (RND(1) == 1)
                    {
                        PlayAnimation(MouseAnimationId.IdleEmpty);
                        EnableEyesBlink();
                    }
                    break;

                case MouseAnimationId.EntryWithCandy:
                    // iOS case 1: play idle (frame 24 = candy in mouth)
                    elapsedActive = 0f;
                    PlayAnimation(MouseAnimationId.Idle);
                    IsActive = true;
                    break;

                case MouseAnimationId.ExitEmpty:
                case MouseAnimationId.ExitWithCandy:
                    // iOS cases 4,5: remove mouse, advance to next
                    if (sharedSprites.HasValue)
                    {
                        mouseGroup.RemoveChild(sharedSprites.Value.Container);
                        sharedSprites = null;
                    }
                    else
                    {
                        mouseGroup.RemoveAllChilds();
                    }
                    manager.AdvanceToNextMouse();
                    break;
                case MouseAnimationId.IdleEmpty:
                case MouseAnimationId.Idle:
                case MouseAnimationId.Bounce:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Checks whether the bounce timeline (ID 6) is currently playing on the container.
        /// </summary>
        /// <returns><see langword="true" /> when the bounce timeline is playing; otherwise, <see langword="false" />.</returns>
        private bool IsBounceTimelinePlaying()
        {
            SharedMouseSprites? sprites = sharedSprites;
            if (!sprites.HasValue)
            {
                return false;
            }

            Timeline t = sprites.Value.Container.GetTimeline((int)MouseAnimationId.Bounce);
            return t != null && t.state == Timeline.TimelineState.TIMELINE_PLAYING;
        }

        /// <summary>
        /// Enables and plays the eye blinking animation for the mouse.
        /// </summary>
        private void EnableEyesBlink()
        {
            SharedMouseSprites? sprites = sharedSprites;
            if (!sprites.HasValue)
            {
                return;
            }

            sprites.Value.Eyes.visible = true;
            sprites.Value.Eyes.PlayTimeline(0);
        }

        /// <summary>
        /// Plays the specified animation and registers this instance as the timeline delegate.
        /// </summary>
        /// <param name="id">The animation identifier to play.</param>
        private void PlayAnimation(MouseAnimationId id)
        {
            SharedMouseSprites? sprites = sharedSprites;
            if (!sprites.HasValue)
            {
                return;
            }

            Timeline timeline = sprites.Value.Body.GetTimeline((int)id);
            _ = (timeline?.delegateTimelineDelegate = this);
            sprites.Value.Body.PlayTimeline((int)id);
        }

        /// <summary>
        /// Creates the path for candy entry animation, moving the candy from
        /// outside the hole into the mouse's mouth.
        /// </summary>
        /// <returns>A list of path points defining the entry trajectory.</returns>
        private List<PathPoint> CreateEntryPath()
        {
            return
            [
                new PathPoint(entryOffsets[0], 0f),
                new PathPoint(entryOffsets[1], 0.05f),
                new PathPoint(entryOffsets[2], 0.1f),
                new PathPoint(entryOffsets[3], 0.15f)
            ];
        }

        /// <summary>
        /// Creates the path for candy exit animation when the mouse retreats
        /// with candy back into the hole.
        /// </summary>
        /// <returns>A list of path points defining the exit trajectory.</returns>
        private List<PathPoint> CreateExitPath()
        {
            return
            [
                new PathPoint(exitOffsets[0], 0f),
                new PathPoint(exitOffsets[1], 0.05f),
                new PathPoint(exitOffsets[2], 0.1f)
            ];
        }

        /// <summary>
        /// Gets a value indicating whether the mouse is currently active and can interact.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Quad index for the mouse hole sprite.
        /// </summary>
        internal const int HoleQuad = 0;

        /// <summary>
        /// Quad index for the idle mouse body sprite.
        /// </summary>
        internal const int IdleQuad = 4;

        /// <summary>
        /// First quad index in the mouse eye blink sequence.
        /// </summary>
        internal const int EyesStartQuad = 5;

        /// <summary>
        /// Last quad index in the mouse eye blink sequence.
        /// </summary>
        internal const int EyesEndQuad = 13;

        /// <summary>
        /// Quad index for the body frame with candy in the mouse mouth.
        /// </summary>
        internal const int CandyInMouthQuad = 24;

        /// <summary>
        /// Quad index used to hide the mouse body.
        /// </summary>
        internal const int BlankQuad = 18;

        /// <summary>
        /// Candy offsets used while entering the mouse mouth.
        /// </summary>
        private readonly Vector[] entryOffsets;

        /// <summary>
        /// Candy offsets used while exiting with the mouse.
        /// </summary>
        private readonly Vector[] exitOffsets;

        /// <summary>
        /// Path player that animates the carried candy through the mouse mouth.
        /// </summary>
        private readonly MouthPathPlayer mouthPathPlayer;

        /// <summary>
        /// Container that positions the shared mouse sprites at this mouse hole.
        /// </summary>
        private readonly BaseElement mouseGroup;

        /// <summary>
        /// Sprite used to draw the mouse hole.
        /// </summary>
        private readonly Image holeSprite;

        /// <summary>
        /// Manager that owns and sequences this mouse.
        /// </summary>
        private readonly MiceObject manager;

        /// <summary>
        /// Timeline used for the short active-state bounce.
        /// </summary>
        private Timeline bounceTimeline;

        /// <summary>
        /// The logical index for this mouse, used for ordering and activation.
        /// </summary>
        public int index;

        /// <summary>
        /// The radius within which the mouse can grab candy.
        /// </summary>
        public float grabRadius;

        /// <summary>
        /// Maximum time in seconds the mouse stays active before auto-retreating.
        /// </summary>
        public float activeDuration;

        /// <summary>
        /// The angle in degrees for the mouse's orientation.
        /// </summary>
        public float angleDeg;

        /// <summary>
        /// Elapsed active time since the mouse became interactive.
        /// </summary>
        private float elapsedActive;

        /// <summary>
        /// Star point currently carried by the mouse.
        /// </summary>
        private ConstraintedPoint carriedStar;

        /// <summary>
        /// Candy object currently carried by the mouse.
        /// </summary>
        private GameObject carriedCandy;

        /// <summary>
        /// Shared sprite set currently attached to this mouse.
        /// </summary>
        private SharedMouseSprites? sharedSprites;

        /// <summary>
        /// Whether the mouse is currently retreating into its hole.
        /// </summary>
        private bool retreating;

        /// <summary>
        /// Whether the candy grab path animation is still playing.
        /// </summary>
        private bool grabAnimating;
    }
}
