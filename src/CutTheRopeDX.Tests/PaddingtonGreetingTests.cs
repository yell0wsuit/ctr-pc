using System.Reflection;

using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the Paddington hat-tip greeting. Om Nom starts a January level already wearing the
    /// bear's hat, tips it once the delayed greeting fires, then sets it down beside him where it
    /// stays for the rest of the level.
    /// </summary>
    public sealed class PaddingtonGreetingTests
    {
        /// <summary>
        /// The sheet holds 40 quads, but the last is the hat on its own - the prop left standing
        /// after the greeting - so the animation itself runs 0-38.
        /// </summary>
        private const int GreetingFrameCount = 39;


        /// <summary>The greeting runs at 24 fps, not the 20 fps the rest of the sheet uses.</summary>
        private const float GreetingFrameDelay = 1f / 24f;

        [Fact]
        public void TheGreetingRunsEveryFrameExceptTheStandaloneHat()
        {
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);

            Track frames = GreetingTrack(backend);

            // No trailing hand-off keyframe: the switch back to the idle loop happens in the
            // timeline's finished callback so it lands together with the hat appearing.
            Assert.Equal(GreetingFrameCount, frames.keyFramesCount);
        }

        [Fact]
        public void TheHatAppearsInTheSameBreathAsOmNomReturningToIdle()
        {
            // The last greeting frame is the only one drawing Om Nom beside the hat, and the sheet
            // that replaces it is hatless - so if these two ever split, he flashes empty-handed.
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);
            CharAnimations target = (CharAnimations)backend.TargetObject;
            Image hat = HatOf(backend);

            backend.Play(TargetAnimationState.Greeting);
            GreetingTimelineOf(backend).OnFinished();

            Assert.True(hat.visible);
            Assert.False(target.GetAnimation(Resources.Img.CharAnimationsPaddington).visible);
            Assert.Equal(OriginalTargetAnimationBackend.IdleLoopTimeline, target.GetCurrentTimelineIndex());
        }

        [Fact]
        public void TheGreetingRunsFasterThanTheRestOfTheSheet()
        {
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);

            Track frames = GreetingTrack(backend);

            // The first frame lands at time zero; every frame after it carries the delay.
            Assert.Equal(GreetingFrameDelay, frames.keyFrames[1].timeOffset, 5);
        }

        [Fact]
        public void GreetingPlaysTheHatTipRatherThanTheChristmasGreeting()
        {
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);
            CharAnimations target = (CharAnimations)backend.TargetObject;

            backend.Play(TargetAnimationState.Greeting);

            Assert.True(target.GetAnimation(Resources.Img.CharAnimationsPaddington).visible);
            Assert.False(target.GetAnimation(Resources.Img.CharGreetingXmas).visible);
        }

        [Fact]
        public void PaddingtonIdlesNeverReachForTheChristmasSheet()
        {
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: false);
            CharAnimations target = (CharAnimations)backend.TargetObject;

            // The Christmas idles show the Santa hat Om Nom has swapped for the bear's, so the
            // picker must not even offer them - and every draw it does offer stays on the base sheet.
            int offeredMax = -1;
            backend.PlayRandomIdleVariant((_, max) =>
            {
                offeredMax = max;
                return 0;
            });
            Assert.Equal(1, offeredMax);

            for (int draw = 0; draw <= offeredMax; draw++)
            {
                int pick = draw;
                backend.PlayRandomIdleVariant((_, _) => pick);

                Assert.False(target.GetAnimation(Resources.Img.CharIdleXmas).visible);
            }
        }

        [Fact]
        public void TheHolidayEventAddsTheChristmasIdlesRatherThanReplacingTheBaseOnes()
        {
            // Off-Paddington December: the iOS release picks from four idles - the base sheet's two
            // plus the Christmas sheet's two - so asking for four draws has to reach both sheets.
            _ = HeadlessGame.Boot();
            OriginalTargetAnimationBackend backend = new(isNightLevel: false, isXmas: true);
            CharAnimations target = (CharAnimations)backend.TargetObject;

            bool reachedChristmasSheet = false;
            bool reachedBaseSheet = false;
            for (int draw = 0; draw <= 3; draw++)
            {
                backend.PlayRandomIdleVariant((_, _) => draw);

                if (target.GetAnimation(Resources.Img.CharIdleXmas).visible)
                {
                    reachedChristmasSheet = true;
                }
                else
                {
                    reachedBaseSheet = true;
                }
            }

            Assert.True(reachedChristmasSheet);
            Assert.True(reachedBaseSheet);
        }

        [Fact]
        public void OutsideTheHolidayTheIdlePickerOnlyOffersTheBasePair()
        {
            _ = HeadlessGame.Boot();
            OriginalTargetAnimationBackend backend = new(isNightLevel: false, isXmas: false);

            int highestDrawOffered = -1;
            backend.PlayRandomIdleVariant((_, max) =>
            {
                highestDrawOffered = max;
                return 0;
            });

            Assert.Equal(1, highestDrawOffered);
        }

        [Fact]
        public void TheHatIsOutOfSightUntilOmNomHasSetItDown()
        {
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);

            Assert.False(HatOf(backend).visible);
        }

        [Fact]
        public void AlreadyGreetedTheHatIsStandingThereFromTheStart()
        {
            // Re-entering a level whose greeting has been spent: no hat tip is coming, so the hat
            // has to be beside Om Nom already rather than waiting for an animation that never runs.
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: false);

            Assert.True(HatOf(backend).visible);
        }

        [Fact]
        public void TheHatHangsOffOmNomsOwnAnchor()
        {
            // It is drawn at his position rather than parented to him, so a mismatched anchor
            // leaves it floating away from him by the quad's trim offset.
            OriginalTargetAnimationBackend backend = Paddington(greetingPending: true);

            Assert.Equal(backend.TargetObject.anchor, HatOf(backend).anchor);
        }

        [Fact]
        public void TheHatTipIsScriptedSoItPlaysAloneAndUnvoiced()
        {
            // Drives two things in ShowGreeting: no monster_greeting over the hat tip, and no
            // substituting the chat greeting - which the classic skin cannot animate at all, so it
            // would swallow the hat tip and strand Om Nom holding the hat he never sets down.
            Assert.True(Paddington(greetingPending: true).HasScriptedGreeting);
        }

        [Fact]
        public void TheOrdinaryWaveIsNotScripted()
        {
            _ = HeadlessGame.Boot();

            Assert.False(new OriginalTargetAnimationBackend(isNightLevel: false, isXmas: true).HasScriptedGreeting);
        }

        [Fact]
        public void OffSeasonThereIsNoHatAtAll()
        {
            _ = HeadlessGame.Boot();
            OriginalTargetAnimationBackend backend = new(isNightLevel: false, isXmas: true);

            Assert.Null(HatOf(backend));
        }

        private static OriginalTargetAnimationBackend Paddington(bool greetingPending)
        {
            _ = HeadlessGame.Boot();
            return new OriginalTargetAnimationBackend(
                isNightLevel: false,
                isXmas: true,
                isPaddington: true,
                paddingtonGreetingPending: greetingPending);
        }

        private static Timeline GreetingTimelineOf(OriginalTargetAnimationBackend backend)
        {
            Animation greeting = ((CharAnimations)backend.TargetObject)
                .GetAnimation(Resources.Img.CharAnimationsPaddington);
            Assert.NotNull(greeting);

            Timeline timeline = greeting.GetTimeline(OriginalTargetAnimationBackend.PaddingtonGreetingTimeline);
            Assert.NotNull(timeline);
            return timeline;
        }

        private static Track GreetingTrack(OriginalTargetAnimationBackend backend)
        {
            Track frames = GreetingTimelineOf(backend).GetTrack(Track.TrackType.TRACK_ACTION);
            Assert.NotNull(frames);
            return frames;
        }

        private static Image HatOf(OriginalTargetAnimationBackend backend)
        {
            return (Image)typeof(OriginalTargetAnimationBackend)
                .GetField("padHat", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(backend);
        }
    }
}
