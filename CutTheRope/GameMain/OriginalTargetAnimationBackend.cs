using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Original Om Nom animation backend based on <see cref="CharAnimations"/> timelines.
    /// </summary>
    internal sealed class OriginalTargetAnimationBackend : ITargetAnimationBackend
    {
        public const int IdleLoopTimeline = 0;
        public const int IdleVariationOneTimeline = 1;
        public const int IdleVariationTwoTimeline = 2;
        public const int ExcitedTimeline = 3;
        public const int CheerfulTimeline = 4;
        public const int SadTimeline = 5;
        public const int ChewingTimeline = 6;
        public const int MouthOpeningTimeline = 7;
        public const int MouthClosingTimeline = 8;
        public const int MouthOpenedLoopTimeline = 9;
        public const int GreetingTimeline = 10;
        public const int XmasGreetingTimeline = 11;
        public const int XmasIdleVariationOneTimeline = 12;
        public const int XmasIdleVariationTwoTimeline = 13;
        public const int SleepingTimeline = 15;

        private const int SleepAnimStartFrame = 0;
        private const int SleepAnimEndFrame = 6;
        private const float SleepAnimFrameDelay = 0.05f;

        private const float DefaultFrameDelay = 0.05f;
        private const int ComplexIdleStartFrame = 68;
        private const int ComplexIdleLoopCount = 32;

        // ZZZ overlay position offsets from Om Nom's origin
        private const float ZzzOffsetX1 = 120f;
        private const float ZzzOffsetY1 = -120f;
        private const float ZzzOffsetX2 = 100f;
        private const float ZzzOffsetY2 = -100f;

        // Rotation pivot offset — the ZZZ orbits around a point 160 units left of its center
        private const float ZzzRotationCenterX = -160f;

        private static readonly (int Next, bool Visible, float Duration,
            float ScaleStart, float ScaleEnd,
            float RotStart, float RotEnd,
            float AlphaStart, float AlphaEnd)[] ZzzStates =
        [
            (1, true,  0.40f, 0.61f, 0.80f,  29f,  9f, 0f, 1f),   // fade in while growing
            (2, true,  0.30f, 0.80f, 0.89f,   9f, -2f, 1f, 1f),   // continue growing
            (3, true,  0.20f, 0.89f, 0.98f,  -2f,-13f, 1f, 1f),   // near full size
            (4, true,  0.50f, 0.98f, 0.59f, -13f,-33f, 1f, 0f),   // shrink and fade out
            (0, false, 0.40f,    0f,   0f,    0f,  0f, 0f, 0f),   // invisible pause
        ];

        private readonly CharAnimations target;
        private readonly bool isNightLevel;
        private readonly bool isXmas;
        private readonly Animation blink;
        private readonly Image zz1;
        private readonly Image zz2;
        private int _zz1State;
        private int _zz2State;
        private float _zz1Time;
        private float _zz2Time;

        /// <summary>
        /// Creates and configures the original timeline backend for Om Nom.
        /// </summary>
        /// <param name="isNightLevel">Whether sleep animations should be configured.</param>
        /// <param name="isXmas">Whether Christmas animation variants should be configured.</param>
        public OriginalTargetAnimationBackend(bool isNightLevel, bool isXmas)
        {
            target = CharAnimations.CharAnimations_createWithResID(Resources.Img.CharAnimations);
            target.DoRestoreCutTransparency();
            target.passColorToChilds = false;

            this.isNightLevel = isNightLevel;
            this.isXmas = isXmas;

            ConfigureTargetResources();
            ConfigureTargetTimelines();
            ConfigureTargetTransitions();

            blink = CreateBlinkAnimation();
            if (isNightLevel)
            {
                zz1 = CreateZzzOverlay();
                zz2 = CreateZzzOverlay();
                _zz1State = 0;
                _zz2State = 4;
            }
        }

        /// <inheritdoc />
        public GameObject TargetObject => target;

        /// <inheritdoc />
        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            target.PlayTimeline(IdleLoopTimeline);
            target.GetTimeline(IdleLoopTimeline).delegateTimelineDelegate = timelineDelegate;
            target.SetPauseAtIndexforAnimation(MouthClosingTimeline, MouthOpeningTimeline);
        }

        /// <inheritdoc />
        public void Play(TargetAnimationState state)
        {
            switch (state)
            {
                case TargetAnimationState.IdleLoop:
                    target.PlayTimeline(IdleLoopTimeline);
                    break;
                case TargetAnimationState.IdleVariationOne:
                    if (isXmas)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationOneTimeline);
                    }
                    else
                    {
                        target.PlayTimeline(IdleVariationOneTimeline);
                    }
                    break;
                case TargetAnimationState.IdleVariationTwo:
                    if (isXmas)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationTwoTimeline);
                    }
                    else
                    {
                        target.PlayTimeline(IdleVariationTwoTimeline);
                    }
                    break;
                case TargetAnimationState.Excited:
                    target.PlayAnimationtimeline(Resources.Img.CharAnimations2, ExcitedTimeline);
                    break;
                case TargetAnimationState.MouthOpening:
                    target.PlayTimeline(MouthOpeningTimeline);
                    break;
                case TargetAnimationState.MouthClosing:
                    target.PlayTimeline(MouthClosingTimeline);
                    break;
                case TargetAnimationState.Chewing:
                    target.PlayTimeline(ChewingTimeline);
                    break;
                case TargetAnimationState.Sad:
                    target.PlayAnimationtimeline(Resources.Img.CharAnimations3, SadTimeline);
                    break;
                case TargetAnimationState.Sleeping:
                    if (isNightLevel)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharAnimationsSleeping, SleepingTimeline);
                    }
                    break;
                case TargetAnimationState.Greeting:
                    if (isXmas)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharGreetingXmas, XmasGreetingTimeline);
                    }
                    else
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharAnimations2, GreetingTimeline);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc />
        public bool IsPlaying(TargetAnimationState state)
        {
            switch (state)
            {
                case TargetAnimationState.IdleLoop:
                    return target.GetCurrentTimelineIndex() == IdleLoopTimeline;
                case TargetAnimationState.IdleVariationOne:
                case TargetAnimationState.IdleVariationTwo:
                case TargetAnimationState.Excited:
                case TargetAnimationState.MouthOpening:
                case TargetAnimationState.MouthClosing:
                case TargetAnimationState.Chewing:
                case TargetAnimationState.Sad:
                case TargetAnimationState.Greeting:
                    return false;
                case TargetAnimationState.Sleeping:
                    if (!isNightLevel)
                    {
                        return false;
                    }

                    Animation sleepAnimation = target.GetAnimation(Resources.Img.CharAnimationsSleeping);
                    return sleepAnimation != null && sleepAnimation.GetCurrentTimelineIndex() == SleepingTimeline;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public float GetSleepPulseDelaySeconds()
        {
            return SleepAnimFrameDelay * (SleepAnimEndFrame - SleepAnimStartFrame + 1);
        }

        /// <inheritdoc />
        public void ResetBlink()
        {
            blink.PlayTimeline(0);
        }

        /// <inheritdoc />
        public void TriggerBlink()
        {
            blink.visible = true;
            blink.PlayTimeline(0);
        }

        /// <inheritdoc />
        public void UpdateSleepOverlays(float delta)
        {
            if (zz1 != null)
            {
                AdvanceZzzState(zz1, ref _zz1State, ref _zz1Time, delta);
            }
            if (zz2 != null)
            {
                AdvanceZzzState(zz2, ref _zz2State, ref _zz2Time, delta);
            }
        }

        /// <inheritdoc />
        public void SyncSleepOverlayPosition(float x, float y)
        {
            if (zz1 != null)
            {
                zz1.x = x + ZzzOffsetX1;
                zz1.y = y + ZzzOffsetY1;
            }
            if (zz2 != null)
            {
                zz2.x = x + ZzzOffsetX2;
                zz2.y = y + ZzzOffsetY2;
            }
        }

        /// <inheritdoc />
        public void SetSleepOverlayVisible(bool visible)
        {
            if (zz1 == null)
            {
                return;
            }

            if (visible)
            {
                _zz1State = 0;
                _zz1Time = 0f;
                _zz2State = 4;
                _zz2Time = 0f;
            }
            else
            {
                zz1.visible = false;
                zz2.visible = false;
            }
        }

        /// <inheritdoc />
        public void DrawSleepOverlays()
        {
            if (zz1?.visible == true)
            {
                zz1.Draw();
            }
            if (zz2?.visible == true)
            {
                zz2.Draw();
            }
        }

        /// <summary>
        /// Advances one ZZZ overlay through the state machine and applies the resulting transforms.
        /// </summary>
        /// <remarks>
        /// Each state linearly interpolates scale, rotation, and alpha over its duration,
        /// then transitions to the next state. States loop: 0→1→2→3→4→0.
        /// zz1 starts at state 0 (visible), zz2 starts at state 4 (invisible, 0.4 s phase offset).
        /// </remarks>
        private static void AdvanceZzzState(Image zzz, ref int state, ref float time, float delta)
        {
            time += delta;

            (int Next, bool Visible, float Duration, float ScaleStart, float ScaleEnd, float RotStart, float RotEnd, float AlphaStart, float AlphaEnd) s = ZzzStates[state];
            while (time >= s.Duration)
            {
                time -= s.Duration;
                state = s.Next;
                s = ZzzStates[state];
            }

            zzz.visible = s.Visible;
            if (!s.Visible)
            {
                return;
            }

            float t = time / s.Duration;
            float scale = s.ScaleStart + ((s.ScaleEnd - s.ScaleStart) * t);
            zzz.scaleX = scale;
            zzz.scaleY = scale;
            zzz.rotation = s.RotStart + ((s.RotEnd - s.RotStart) * t);
            float alpha = s.AlphaStart + ((s.AlphaEnd - s.AlphaStart) * t);
            zzz.color = new RGBAColor(1f, 1f, 1f, alpha);
        }

        /// <summary>
        /// Adds additional texture resources required for Om Nom timeline variants.
        /// </summary>
        private void ConfigureTargetResources()
        {
            target.AddImage(Resources.Img.CharAnimations2);
            target.AddImage(Resources.Img.CharAnimations3);
            if (isNightLevel)
            {
                target.AddImage(Resources.Img.CharAnimationsSleeping);
            }
            if (isXmas)
            {
                target.AddImage(Resources.Img.CharGreetingXmas);
                target.AddImage(Resources.Img.CharIdleXmas);
            }
        }

        /// <summary>
        /// Defines Om Nom timelines and frame ranges.
        /// </summary>
        private void ConfigureTargetTimelines()
        {
            target.AddAnimationWithIDDelayLoopFirstLast(IdleLoopTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
            target.AddAnimationWithIDDelayLoopFirstLast(IdleVariationOneTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);
            target.AddAnimationWithIDDelayLoopCountSequence(
                IdleVariationTwoTimeline,
                DefaultFrameDelay,
                Timeline.LoopType.TIMELINE_NO_LOOP,
                ComplexIdleLoopCount,
                ComplexIdleStartFrame,
                BuildComplexIdleTailSequence());

            if (isXmas)
            {
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharGreetingXmas, XmasGreetingTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 33);
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharIdleXmas, XmasIdleVariationOneTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 30);
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharIdleXmas, XmasIdleVariationTwoTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 31, 61);
            }

            target.AddAnimationWithIDDelayLoopFirstLast(MouthOpeningTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
            target.AddAnimationWithIDDelayLoopFirstLast(MouthClosingTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(MouthOpenedLoopTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
            target.AddAnimationWithIDDelayLoopFirstLast(ChewingTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, GreetingTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, ExcitedTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations2, CheerfulTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
            target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimations3, SadTimeline, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);

            if (isNightLevel)
            {
                target.AddAnimationWithIDDelayLoopFirstLast(Resources.Img.CharAnimationsSleeping, SleepingTimeline, SleepAnimFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, SleepAnimStartFrame, SleepAnimEndFrame);
            }
        }

        /// <summary>
        /// Configures automatic transitions between Om Nom timelines.
        /// </summary>
        private void ConfigureTargetTransitions()
        {
            target.SwitchToAnimationatEndOfAnimationDelay(MouthOpenedLoopTimeline, ChewingTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations2, CheerfulTimeline, Resources.Img.CharAnimations, MouthClosingTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharAnimations2, GreetingTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharAnimations, IdleVariationOneTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharAnimations, IdleVariationTwoTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharAnimations2, ExcitedTimeline, DefaultFrameDelay);
            target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharAnimations2, CheerfulTimeline, DefaultFrameDelay);

            if (isXmas)
            {
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharGreetingXmas, XmasGreetingTimeline, DefaultFrameDelay);
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharIdleXmas, XmasIdleVariationOneTimeline, DefaultFrameDelay);
                target.SwitchToAnimationatEndOfAnimationDelay(Resources.Img.CharAnimations, IdleLoopTimeline, Resources.Img.CharIdleXmas, XmasIdleVariationTwoTimeline, DefaultFrameDelay);
            }
        }

        /// <summary>
        /// Creates and attaches the blink overlay animation.
        /// </summary>
        /// <returns>Blink animation instance attached to Om Nom.</returns>
        private Animation CreateBlinkAnimation()
        {
            Animation blink = Animation.Animation_createWithResID(Resources.Img.CharAnimations);
            blink.parentAnchor = 9;
            blink.visible = false;
            blink.AddAnimationWithIDDelayLoopCountSequence(0, DefaultFrameDelay, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, [41, 42, 42, 42]);
            blink.SetActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", blink, 0, 0, 2, 0);
            blink.DoRestoreCutTransparency();
            _ = target.AddChild(blink);
            return blink;
        }

        /// <summary>
        /// Creates a single ZZZ overlay image for the night level sleep animation.
        /// Scale, rotation, and alpha are driven each frame by <see cref="AdvanceZzzState"/>.
        /// </summary>
        /// <returns>Configured ZZZ image, initially hidden.</returns>
        private static Image CreateZzzOverlay()
        {
            Image zzz = Image.Image_createWithResID(Resources.Img.FxSleep);
            zzz.rotationCenterX = ZzzRotationCenterX;
            zzz.visible = false;
            return zzz;
        }

        /// <summary>
        /// Builds tail frames for the complex idle sequence after the first frame.
        /// </summary>
        /// <returns>Frame list consumed by <c>AddAnimationWithIDDelayLoopCountSequence</c>.</returns>
        private static List<int> BuildComplexIdleTailSequence()
        {
            const int frameRangeLength = 15;
            const int totalLength = (frameRangeLength * 2) + 1;

#pragma warning disable IDE0028
            List<int> sequence = new(totalLength);
#pragma warning restore IDE0028

            for (int offset = 1; offset <= frameRangeLength; offset++)
            {
                sequence.Add(ComplexIdleStartFrame + offset);
            }

            sequence.Add(ComplexIdleStartFrame);

            for (int offset = 1; offset <= frameRangeLength; offset++)
            {
                sequence.Add(ComplexIdleStartFrame + offset);
            }

            return sequence;
        }
    }
}
