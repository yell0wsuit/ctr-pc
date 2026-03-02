using System;
using System.Collections.Generic;

using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class TargetAnimationController
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

        public const int SleepAnimStartFrame = 0;
        public const int SleepAnimEndFrame = 6;
        public const float SleepAnimFrameDelay = 0.05f;

        private const float DefaultFrameDelay = 0.05f;
        private const int ComplexIdleStartFrame = 68;
        private const int ComplexIdleLoopCount = 32;
        private const int SleepZzzStartFrame = 7;
        private const int SleepZzzEndFrame = 43;

        private readonly CharAnimations target;
        private readonly bool isNightLevel;
        private readonly bool isXmas;

        private TargetAnimationController(CharAnimations target, bool isNightLevel, bool isXmas)
        {
            this.target = target;
            this.isNightLevel = isNightLevel;
            this.isXmas = isXmas;
        }

        public static TargetAnimationController Create(CharAnimations target, bool isNightLevel, bool isXmas)
        {
            TargetAnimationController controller = new(target, isNightLevel, isXmas);
            controller.ConfigureTargetResources();
            controller.ConfigureTargetTimelines();
            controller.ConfigureTargetTransitions();
            controller.Blink = controller.CreateBlinkAnimation();
            if (isNightLevel)
            {
                (controller.SleepAnimationPrimary, controller.SleepAnimationSecondary) = CreateSleepOverlayAnimations();
            }
            return controller;
        }

        public Animation Blink { get; private set; }

        public Animation SleepAnimationPrimary { get; private set; }

        public Animation SleepAnimationSecondary { get; private set; }

        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            target.PlayTimeline(IdleLoopTimeline);
            target.GetTimeline(IdleLoopTimeline).delegateTimelineDelegate = timelineDelegate;
            target.SetPauseAtIndexforAnimation(MouthClosingTimeline, MouthOpeningTimeline);
        }

        public void PlayGreeting()
        {
            if (isXmas)
            {
                target.PlayAnimationtimeline(Resources.Img.CharGreetingXmas, XmasGreetingTimeline);
                return;
            }

            target.PlayAnimationtimeline(Resources.Img.CharAnimations2, GreetingTimeline);
        }

        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            if (rng(0, 1) == 1)
            {
                if (isXmas)
                {
                    target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationOneTimeline);
                }
                else
                {
                    target.PlayTimeline(IdleVariationOneTimeline);
                }

                return;
            }

            if (isXmas)
            {
                target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationTwoTimeline);
            }
            else
            {
                target.PlayTimeline(IdleVariationTwoTimeline);
            }
        }

        public void PlayExcited()
        {
            target.PlayAnimationtimeline(Resources.Img.CharAnimations2, ExcitedTimeline);
        }

        public void PlayMouthOpening()
        {
            target.PlayTimeline(MouthOpeningTimeline);
        }

        public void PlayMouthClosing()
        {
            target.PlayTimeline(MouthClosingTimeline);
        }

        public void PlayChewing()
        {
            target.PlayTimeline(ChewingTimeline);
        }

        public void PlaySad()
        {
            target.PlayAnimationtimeline(Resources.Img.CharAnimations3, SadTimeline);
        }

        public void PlaySleeping()
        {
            if (!isNightLevel)
            {
                return;
            }

            target.PlayAnimationtimeline(Resources.Img.CharAnimationsSleeping, SleepingTimeline);
        }

        public bool IsIdleLoopPlaying()
        {
            return target.GetCurrentTimelineIndex() == IdleLoopTimeline;
        }

        public bool IsSleepingAnimationPlaying()
        {
            if (!isNightLevel)
            {
                return false;
            }

            Animation sleepAnimation = target.GetAnimation(Resources.Img.CharAnimationsSleeping);
            return sleepAnimation != null && sleepAnimation.GetCurrentTimelineIndex() == SleepingTimeline;
        }

        public static float GetSleepPulseDelaySeconds()
        {
            return SleepAnimFrameDelay * (SleepAnimEndFrame - SleepAnimStartFrame + 1);
        }

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

            List<int> primarySequence = [];
            primarySequence.AddRange(sleepFrames);
            primarySequence.AddRange(sleepHoldFrames);

            List<int> secondarySequence = [];
            secondarySequence.AddRange(sleepHoldFrames);
            secondarySequence.AddRange(sleepFrames);

            return (
                CreateSleepOverlayAnimation(primarySequence),
                CreateSleepOverlayAnimation(secondarySequence));
        }

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

        private static List<int> BuildComplexIdleTailSequence()
        {
            return
            [
                ComplexIdleStartFrame + 1,
                ComplexIdleStartFrame + 2,
                ComplexIdleStartFrame + 3,
                ComplexIdleStartFrame + 4,
                ComplexIdleStartFrame + 5,
                ComplexIdleStartFrame + 6,
                ComplexIdleStartFrame + 7,
                ComplexIdleStartFrame + 8,
                ComplexIdleStartFrame + 9,
                ComplexIdleStartFrame + 10,
                ComplexIdleStartFrame + 11,
                ComplexIdleStartFrame + 12,
                ComplexIdleStartFrame + 13,
                ComplexIdleStartFrame + 14,
                ComplexIdleStartFrame + 15,
                ComplexIdleStartFrame,
                ComplexIdleStartFrame + 1,
                ComplexIdleStartFrame + 2,
                ComplexIdleStartFrame + 3,
                ComplexIdleStartFrame + 4,
                ComplexIdleStartFrame + 5,
                ComplexIdleStartFrame + 6,
                ComplexIdleStartFrame + 7,
                ComplexIdleStartFrame + 8,
                ComplexIdleStartFrame + 9,
                ComplexIdleStartFrame + 10,
                ComplexIdleStartFrame + 11,
                ComplexIdleStartFrame + 12,
                ComplexIdleStartFrame + 13,
                ComplexIdleStartFrame + 14,
                ComplexIdleStartFrame + 15
            ];
        }
    }
}
