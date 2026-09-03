using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Original Om Nom animation backend based on <see cref="CharAnimations"/> timelines.
    /// </summary>
    internal sealed class OriginalTargetAnimationBackend : ITargetAnimationBackend
    {
        /// <summary>Timeline ID for the default idle loop.</summary>
        public const int IdleLoopTimeline = 0;

        /// <summary>Timeline ID for the first idle variation.</summary>
        public const int IdleVariationOneTimeline = 1;

        /// <summary>Timeline ID for the second idle variation.</summary>
        public const int IdleVariationTwoTimeline = 2;

        /// <summary>Timeline ID for the excited animation.</summary>
        public const int ExcitedTimeline = 3;

        /// <summary>Timeline ID for the cheerful animation.</summary>
        public const int CheerfulTimeline = 4;

        /// <summary>Timeline ID for the sad animation.</summary>
        public const int SadTimeline = 5;

        /// <summary>Timeline ID for the chewing animation.</summary>
        public const int ChewingTimeline = 6;

        /// <summary>Timeline ID for the mouth-opening animation.</summary>
        public const int MouthOpeningTimeline = 7;

        /// <summary>Timeline ID for the mouth-closing animation.</summary>
        public const int MouthClosingTimeline = 8;

        /// <summary>Timeline ID for the opened-mouth loop animation.</summary>
        public const int MouthOpenedLoopTimeline = 9;

        /// <summary>Timeline ID for the default greeting animation.</summary>
        public const int GreetingTimeline = 10;

        /// <summary>Timeline ID for the Christmas greeting animation.</summary>
        public const int XmasGreetingTimeline = 11;

        /// <summary>Timeline ID for the first Christmas idle variation.</summary>
        public const int XmasIdleVariationOneTimeline = 12;

        /// <summary>Timeline ID for the second Christmas idle variation.</summary>
        public const int XmasIdleVariationTwoTimeline = 13;

        /// <summary>Timeline ID for the Paddington hat-tip greeting animation.</summary>
        public const int PaddingtonGreetingTimeline = 14;

        /// <summary>Timeline ID for the night-level sleeping animation.</summary>
        public const int SleepingTimeline = 15;

        /// <summary>First frame of the Paddington greeting animation.</summary>
        private const int PaddingtonGreetingStartFrame = 0;

        /// <summary>Last frame of the Paddington greeting animation.</summary>
        private const int PaddingtonGreetingEndFrame = 38;

        /// <summary>
        /// Frame delay for the Paddington greeting. The greeting runs at 24 fps rather than the
        /// 20 fps every other original-backend animation uses.
        /// </summary>
        private const float PaddingtonGreetingFrameDelay = 1f / 24f;

        /// <summary>
        /// Quad holding the hat on its own, with no Om Nom. It is the prop left standing beside
        /// Om Nom once he has tipped his hat and set it down.
        /// </summary>
        private const int PaddingtonHatQuad = 39;

        /// <summary>First frame of the night-level sleeping animation.</summary>
        private const int SleepAnimStartFrame = 0;

        /// <summary>Last frame of the night-level sleeping animation.</summary>
        private const int SleepAnimEndFrame = 6;

        /// <summary>Frame delay used by the night-level sleeping animation.</summary>
        private const float SleepAnimFrameDelay = 0.05f;

        /// <summary>Default frame delay for original Om Nom animations.</summary>
        private const float DefaultFrameDelay = 0.05f;

        /// <summary>First frame in the complex idle animation sequence.</summary>
        private const int ComplexIdleStartFrame = 68;

        /// <summary>Loop count used by the complex idle animation sequence.</summary>
        private const int ComplexIdleLoopCount = 32;

        /// <summary>X offset for the first ZZZ overlay from Om Nom's origin.</summary>
        private const float ZzzOffsetX1 = 120f;

        /// <summary>Y offset for the first ZZZ overlay from Om Nom's origin.</summary>
        private const float ZzzOffsetY1 = -120f;

        /// <summary>X offset for the second ZZZ overlay from Om Nom's origin.</summary>
        private const float ZzzOffsetX2 = 100f;

        /// <summary>Y offset for the second ZZZ overlay from Om Nom's origin.</summary>
        private const float ZzzOffsetY2 = -100f;

        /// <summary>Rotation pivot offset that makes ZZZ overlays orbit around a point left of their center.</summary>
        private const float ZzzRotationCenterX = -160f;

        /// <summary>State machine rows used by the ZZZ sleep overlay animation.</summary>
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

        /// <summary>Original Om Nom animation object.</summary>
        private readonly CharAnimations target;

        /// <summary>Whether this backend should use night-level sleeping resources and overlays.</summary>
        private readonly bool isNightLevel;

        /// <summary>Whether this backend should use Christmas animation variants.</summary>
        private readonly bool isXmas;

        /// <summary>Whether this backend should use the Paddington greeting and hat prop.</summary>
        private bool IsPaddington { get; }

        /// <summary>Hat prop revealed once the Paddington greeting has finished, or <see langword="null"/>.</summary>
        private readonly Image padHat;

        /// <summary>Whether a Paddington greeting is scheduled for this level.</summary>
        private readonly bool paddingtonGreetingPending;

        /// <summary>Whether the Paddington greeting has been started at least once.</summary>
        private bool _paddingtonGreetingStarted;

        /// <summary>Blink overlay animation attached to the target.</summary>
        private readonly Animation blink;

        /// <summary>First ZZZ sleep overlay image.</summary>
        private readonly Image zz1;

        /// <summary>Second ZZZ sleep overlay image.</summary>
        private readonly Image zz2;

        /// <summary>Current ZZZ state index for the first sleep overlay.</summary>
        private int _zz1State;

        /// <summary>Current ZZZ state index for the second sleep overlay.</summary>
        private int _zz2State;

        /// <summary>Elapsed time in the current state for the first sleep overlay.</summary>
        private float _zz1Time;

        /// <summary>Elapsed time in the current state for the second sleep overlay.</summary>
        private float _zz2Time;

        /// <summary>
        /// Creates and configures the original timeline backend for Om Nom.
        /// </summary>
        /// <param name="isNightLevel">Whether sleep animations should be configured.</param>
        /// <param name="isXmas">Whether Christmas animation variants should be configured.</param>
        /// <param name="isPaddington">Whether the Paddington greeting and hat prop should be configured.</param>
        /// <param name="paddingtonGreetingPending">Whether a Paddington greeting is scheduled for this level.
        /// When it is, Om Nom waits wearing the hat until the greeting fires; when it is not, the hat is
        /// already set down beside him and he starts on the normal idle loop.</param>
        public OriginalTargetAnimationBackend(
            bool isNightLevel,
            bool isXmas,
            bool isPaddington = false,
            bool paddingtonGreetingPending = false)
        {
            target = CharAnimations.CharAnimations_createWithResID(Resources.Img.CharAnimations);
            target.DoRestoreCutTransparency();
            target.passColorToChilds = false;

            this.isNightLevel = isNightLevel;
            this.isXmas = isXmas;
            IsPaddington = isPaddington;
            this.paddingtonGreetingPending = isPaddington && paddingtonGreetingPending;

            ConfigureTargetResources();
            ConfigureTargetTimelines();
            ConfigureTargetTransitions();

            if (IsPaddington)
            {
                padHat = CreatePaddingtonHat();
                padHat.visible = !this.paddingtonGreetingPending;
                AttachPaddingtonGreetingHandoff();
            }

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

        /// <summary>The classic skin has no XML skin definition.</summary>
        public OmNomSkinDefinition SkinDefinition => null;

        /// <inheritdoc />
        public float GetTargetBaseScaleX()
        {
            return 1f;
        }

        /// <inheritdoc />
        public float GetTargetBaseScaleY()
        {
            return 1f;
        }

        /// <inheritdoc />
        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            target.PlayTimeline(IdleLoopTimeline);
            target.GetTimeline(IdleLoopTimeline).delegateTimelineDelegate = timelineDelegate;
            target.SetPauseAtIndexforAnimation(MouthClosingTimeline, MouthOpeningTimeline);

            if (paddingtonGreetingPending)
            {
                // Hold on the first greeting frame so Om Nom is already wearing the hat when the
                // level fades in, and only tips it once the delayed greeting call arrives.
                target.PlayAnimationtimeline(Resources.Img.CharAnimationsPaddington, PaddingtonGreetingTimeline);
                target.GetAnimation(Resources.Img.CharAnimationsPaddington)?.StopCurrentTimeline();
            }
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
                    target.PlayTimeline(IdleVariationOneTimeline);
                    break;
                case TargetAnimationState.IdleVariationTwo:
                    target.PlayTimeline(IdleVariationTwoTimeline);
                    break;
                case TargetAnimationState.IdleVariationThree:
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
                case TargetAnimationState.IdleToSleep:
                    break;
                case TargetAnimationState.Sleeping:
                    if (isNightLevel)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharAnimationsSleeping, SleepingTimeline);
                    }
                    break;
                case TargetAnimationState.Greeting:
                    if (IsPaddington)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharAnimationsPaddington, PaddingtonGreetingTimeline);
                        _paddingtonGreetingStarted = true;
                    }
                    else if (isXmas)
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharGreetingXmas, XmasGreetingTimeline);
                    }
                    else
                    {
                        target.PlayAnimationtimeline(Resources.Img.CharAnimations2, GreetingTimeline);
                    }
                    break;
                case TargetAnimationState.GreetLeft:
                case TargetAnimationState.GreetRight:
                case TargetAnimationState.GreetUp:
                case TargetAnimationState.GreetDown:
                    // The classic (non-Flash) Om Nom has no directional chat animations.
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc />
        public void PlaySleeping(bool trimIdleToSleepTransition)
        {
            _ = trimIdleToSleepTransition;
            Play(TargetAnimationState.Sleeping);
        }

        /// <inheritdoc />
        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            // The Christmas idle sheet adds two more variations to the two the base sheet already
            // has, and Om Nom picks between all four. Paddington drops back to the base pair: the
            // Christmas idles show him in the Santa hat he has just swapped for the bear's.
            bool hasXmasIdleVariations = isXmas && !IsPaddington;

            switch (rng(0, hasXmasIdleVariations ? 3 : 1))
            {
                case 0:
                    Play(TargetAnimationState.IdleVariationOne);
                    break;
                case 1:
                    Play(TargetAnimationState.IdleVariationTwo);
                    break;
                case 2:
                    target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationOneTimeline);
                    break;
                default:
                    target.PlayAnimationtimeline(Resources.Img.CharIdleXmas, XmasIdleVariationTwoTimeline);
                    break;
            }
        }

        /// <inheritdoc />
        public bool StartsWithGreeting => false;

        /// <inheritdoc />
        public bool HasScriptedGreeting => IsPaddington;

        /// <inheritdoc />
        public bool UsesFlashXmlAnimations => false;

        /// <inheritdoc />
        public bool IsPlaying(TargetAnimationState state)
        {
            return state switch
            {
                TargetAnimationState.IdleLoop => target.GetCurrentTimelineIndex() == IdleLoopTimeline,
                TargetAnimationState.IdleVariationOne
                or TargetAnimationState.IdleVariationTwo
                or TargetAnimationState.IdleVariationThree
                or TargetAnimationState.Excited
                or TargetAnimationState.MouthOpening
                or TargetAnimationState.MouthClosing
                or TargetAnimationState.Chewing
                or TargetAnimationState.Sad
                or TargetAnimationState.IdleToSleep
                or TargetAnimationState.Greeting
                or TargetAnimationState.GreetLeft
                or TargetAnimationState.GreetRight
                or TargetAnimationState.GreetUp
                or TargetAnimationState.GreetDown => false,
                TargetAnimationState.Sleeping => isNightLevel
                    && target.GetAnimation(Resources.Img.CharAnimationsSleeping)?.GetCurrentTimelineIndex() == SleepingTimeline,
                _ => false
            };
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
        public void UpdateAdditionalOverlays(float delta)
        {
            _ = delta;

            // The greeting hands off to the idle loop by disabling the Paddington animation, which
            // is the moment Om Nom has finished setting the hat down next to him.
            if (_paddingtonGreetingStarted
                && padHat?.visible == false
                && target.GetAnimation(Resources.Img.CharAnimationsPaddington)?.visible == false)
            {
                padHat.visible = true;
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
        public void SyncAdditionalOverlayPosition(float x, float y)
        {
            if (padHat != null)
            {
                padHat.x = x;
                padHat.y = y;
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
            // Drawn after Om Nom so the hat reads as sitting in front of him.
            if (padHat?.visible == true)
            {
                padHat.Draw();
            }
        }

        /// <inheritdoc />
        public bool HandlesOwnSleepPulse => false;

        /// <summary>
        /// Advances one ZZZ overlay through the state machine and applies the resulting transforms.
        /// </summary>
        /// <remarks>
        /// Each state linearly interpolates scale, rotation, and alpha over its duration,
        /// then transitions to the next state. States loop: 0→1→2→3→4→0.
        /// zz1 starts at state 0 (visible), zz2 starts at state 4 (invisible, 0.4 s phase offset).
        /// </remarks>
        /// <param name="zzz">ZZZ overlay image to update.</param>
        /// <param name="state">Current state index for the overlay.</param>
        /// <param name="time">Elapsed time within the current state.</param>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
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
            if (IsPaddington)
            {
                target.AddImage(Resources.Img.CharAnimationsPaddington);
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

            if (IsPaddington)
            {
                target.AddAnimationWithIDDelayLoopFirstLast(
                    Resources.Img.CharAnimationsPaddington,
                    PaddingtonGreetingTimeline,
                    PaddingtonGreetingFrameDelay,
                    Timeline.LoopType.TIMELINE_NO_LOOP,
                    PaddingtonGreetingStartFrame,
                    PaddingtonGreetingEndFrame);
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
        /// <summary>
        /// Creates the Paddington hat prop that stands beside Om Nom after the greeting.
        /// </summary>
        /// <returns>Hat prop image, sharing Om Nom's anchor so it lands where he set it down.</returns>
        /// <summary>
        /// Hands off from the hat tip in a single callback, the way the iOS release does: it reveals
        /// the hat, drops Om Nom back onto the base sheet and restarts his idle loop together.
        /// </summary>
        /// <remarks>
        /// Splitting those apart blanks the hat for a frame. The greeting's last frame is the only
        /// one that draws Om Nom beside the hat, and the sheet that replaces it is hatless, so any
        /// gap between the sheet swap and the prop appearing shows Om Nom empty-handed.
        /// </remarks>
        private void AttachPaddingtonGreetingHandoff()
        {
            Timeline greeting = target
                .GetAnimation(Resources.Img.CharAnimationsPaddington)
                .GetTimeline(PaddingtonGreetingTimeline);

            greeting.OnFinished = () =>
            {
                padHat.visible = true;
                target.PlayTimeline(IdleLoopTimeline);
            };
        }

        private Image CreatePaddingtonHat()
        {
            Image hat = Image.Image_createWithResIDQuad(Resources.Img.CharAnimationsPaddington, PaddingtonHatQuad);
            hat.DoRestoreCutTransparency();
            // The hat is drawn straight at Om Nom's position rather than parented to him, so it has
            // to carry his anchor to land where the greeting's own last frame left it. AddImage
            // gives the greeting sheet the same anchor for the same reason.
            hat.parentAnchor = hat.anchor = target.anchor;
            return hat;
        }

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
        /// <returns>Frame list consumed by <see cref="Animation.AddAnimationWithIDDelayLoopCountSequence" />.</returns>
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
