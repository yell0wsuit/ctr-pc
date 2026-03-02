using System.Collections.Generic;

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
        private const int SleepZzzStartFrame = 7;
        private const int SleepZzzEndFrame = 43;

        private readonly CharAnimations target;
        private readonly bool isNightLevel;
        private readonly bool isXmas;
        private readonly Animation blink;
        private readonly Animation sleepAnimPrimary;
        private readonly Animation sleepAnimSecondary;

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
                (sleepAnimPrimary, sleepAnimSecondary) = CreateSleepOverlayAnimations();
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
            sleepAnimPrimary?.Update(delta);
            sleepAnimSecondary?.Update(delta);
        }

        /// <inheritdoc />
        public void SyncSleepOverlayPosition(float x, float y)
        {
            if (sleepAnimPrimary != null)
            {
                sleepAnimPrimary.x = x;
                sleepAnimPrimary.y = y;
            }
            if (sleepAnimSecondary != null)
            {
                sleepAnimSecondary.x = x;
                sleepAnimSecondary.y = y;
            }
        }

        /// <inheritdoc />
        public void SetSleepOverlayVisible(bool visible)
        {
            if (sleepAnimPrimary != null)
            {
                sleepAnimPrimary.visible = visible;
                if (visible)
                {
                    sleepAnimPrimary.PlayTimeline(0);
                }
                else
                {
                    sleepAnimPrimary.GetTimeline(0)?.StopTimeline();
                }
            }
            if (sleepAnimSecondary != null)
            {
                sleepAnimSecondary.visible = visible;
                if (visible)
                {
                    sleepAnimSecondary.PlayTimeline(0);
                }
                else
                {
                    sleepAnimSecondary.GetTimeline(0)?.StopTimeline();
                }
            }
        }

        /// <inheritdoc />
        public void DrawSleepOverlays()
        {
            if (sleepAnimPrimary?.visible == true)
            {
                sleepAnimPrimary.Draw();
            }
            if (sleepAnimSecondary?.visible == true)
            {
                sleepAnimSecondary.Draw();
            }
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
        /// Creates primary and secondary sleep overlay animations.
        /// </summary>
        /// <returns>Tuple containing primary and secondary sleep overlays.</returns>
        private static (Animation primary, Animation secondary) CreateSleepOverlayAnimations()
        {
            List<int> sleepFrames = [];
            for (int frame = SleepZzzStartFrame; frame <= SleepZzzEndFrame; frame++)
            {
                sleepFrames.Add(frame);
            }

            List<int> sleepHoldFrames = [];
            for (int i = 0; i < 15; i++)
            {
                sleepHoldFrames.Add(SleepZzzStartFrame);
            }

            List<int> primarySequence = [.. sleepFrames, .. sleepHoldFrames];
            List<int> secondarySequence = [.. sleepHoldFrames, .. sleepFrames];

            return (
                CreateSleepOverlayAnimation(primarySequence),
                CreateSleepOverlayAnimation(secondarySequence));
        }

        /// <summary>
        /// Creates a replaying sleep overlay animation from the provided sequence.
        /// </summary>
        /// <param name="sequence">Frame sequence including the first frame.</param>
        /// <returns>Configured sleep overlay animation.</returns>
        private static Animation CreateSleepOverlayAnimation(List<int> sequence)
        {
            List<int> tailSequence = sequence.Count > 1 ? sequence.GetRange(1, sequence.Count - 1) : [];

            Animation sleepOverlay = Animation.Animation_createWithResID(Resources.Img.CharAnimationsSleeping);
            sleepOverlay.anchor = sleepOverlay.parentAnchor = 18;
            sleepOverlay.DoRestoreCutTransparency();
            sleepOverlay.AddAnimationWithIDDelayLoopCountSequence(0, 1f / 30f, Timeline.LoopType.TIMELINE_REPLAY, sequence.Count, sequence[0], tailSequence);
            sleepOverlay.PlayTimeline(0);
            sleepOverlay.visible = false;

            return sleepOverlay;
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
