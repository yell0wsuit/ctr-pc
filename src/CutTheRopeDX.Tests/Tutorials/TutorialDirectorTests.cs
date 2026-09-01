using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    public sealed class TutorialDirectorTests
    {
        [Fact]
        public void StartFiresOnlyAfterLoadingCompletes()
        {
            FakeWorld world = new();
            TutorialDirector director = new(world);
            (TutorialPrompt prompt, CountingVisual visual) = MakePrompt(TutorialEvent.Start);

            director.Add(prompt);
            director.Update(0.25f);

            Assert.Equal(TutorialPromptState.Armed, prompt.State);
            Assert.Equal(0, visual.PlayCount);

            director.CompleteLoading();

            Assert.Equal(TutorialPromptState.Playing, prompt.State);
            Assert.Equal(1, visual.PlayCount);
        }

        [Fact]
        public void PromptTransitionsArmedDelayingPlayingDoneExactlyOnce()
        {
            FakeWorld world = new();
            TutorialDirector director = new(world);
            (TutorialPrompt prompt, CountingVisual visual) = MakePrompt(TutorialEvent.PumpFire, delay: 1f);
            director.Add(prompt);
            director.CompleteLoading();

            director.Fire(TutorialEvent.PumpFire);
            director.Fire(TutorialEvent.PumpFire);
            Assert.Equal(TutorialPromptState.Delaying, prompt.State);
            Assert.Equal(0, visual.PlayCount);

            director.Update(0.5f);
            Assert.Equal(TutorialPromptState.Delaying, prompt.State);
            Assert.Equal(0, visual.UpdateCount);

            director.Update(0.75f);
            Assert.Equal(TutorialPromptState.Playing, prompt.State);
            Assert.Equal(1, visual.PlayCount);
            Assert.Equal(1, visual.UpdateCount);
            Assert.Equal(0.25f, visual.UpdatedSeconds, 3);

            director.Update(1f);
            director.Fire(TutorialEvent.PumpFire);

            Assert.Equal(TutorialPromptState.Done, prompt.State);
            Assert.Equal(1, visual.PlayCount);
        }

        [Fact]
        public void SameGroupUsesXmlOrderAndCancelsSiblingsImmediately()
        {
            TutorialDirector director = new(new FakeWorld());
            (TutorialPrompt first, CountingVisual firstVisual) = MakePrompt(TutorialEvent.Start, group: "intro");
            (TutorialPrompt second, CountingVisual secondVisual) = MakePrompt(TutorialEvent.Start, group: "intro");
            director.Add(first);
            director.Add(second);

            director.CompleteLoading();

            Assert.Equal(TutorialPromptState.Playing, first.State);
            Assert.Equal(TutorialPromptState.Cancelled, second.State);
            Assert.Equal(1, firstVisual.PlayCount);
            Assert.Equal(0, secondVisual.PlayCount);
        }

        [Fact]
        public void IndependentGroupsCanStageMultiplePrompts()
        {
            TutorialDirector director = new(new FakeWorld());
            (TutorialPrompt first, _) = MakePrompt(TutorialEvent.PumpFire, group: "pump");
            (TutorialPrompt second, _) = MakePrompt(TutorialEvent.PumpFire, group: "gesture");
            director.Add(first);
            director.Add(second);
            director.CompleteLoading();

            director.Fire(TutorialEvent.PumpFire);

            Assert.Equal(TutorialPromptState.Playing, first.State);
            Assert.Equal(TutorialPromptState.Playing, second.State);
        }

        [Fact]
        public void CandyScopedPushFiltersAnyPrimaryLeftAndRight()
        {
            CandyBody primary = Body(CandyBodyRole.Whole, 10f, 10f);
            CandyContext owner = new(primary);
            CandyBody left = Body(CandyBodyRole.LeftHalf, 10f, 10f);
            CandyBody right = Body(CandyBodyRole.RightHalf, 10f, 10f);
            left.AttachTo(owner);
            right.AttachTo(owner);
            FakeWorld world = new() { Bodies = [primary, left, right] };
            TutorialDirector director = new(world);
            (TutorialPrompt any, _) = MakePrompt(TutorialEvent.RopeCut, TutorialSubject.Any);
            (TutorialPrompt primaryPrompt, _) = MakePrompt(TutorialEvent.RopeCut, TutorialSubject.Primary);
            (TutorialPrompt leftPrompt, _) = MakePrompt(TutorialEvent.RopeCut, TutorialSubject.Left);
            (TutorialPrompt rightPrompt, _) = MakePrompt(TutorialEvent.RopeCut, TutorialSubject.Right);
            director.Add(any);
            director.Add(primaryPrompt);
            director.Add(leftPrompt);
            director.Add(rightPrompt);
            director.CompleteLoading();

            director.Fire(TutorialEvent.RopeCut, left);

            Assert.Equal(TutorialPromptState.Playing, any.State);
            Assert.Equal(TutorialPromptState.Playing, primaryPrompt.State);
            Assert.Equal(TutorialPromptState.Playing, leftPrompt.State);
            Assert.Equal(TutorialPromptState.Armed, rightPrompt.State);
        }

        [Fact]
        public void ActorlessEventIgnoresSubjectWithoutArea()
        {
            TutorialDirector director = new(new FakeWorld());
            (TutorialPrompt prompt, _) = MakePrompt(TutorialEvent.PumpFire, TutorialSubject.Right);
            director.Add(prompt);
            director.CompleteLoading();

            director.Fire(TutorialEvent.PumpFire);

            Assert.Equal(TutorialPromptState.Playing, prompt.State);
        }

        [Fact]
        public void ActorlessEventResolvesSelectedBodyWhenAreaExists()
        {
            CandyBody left = Body(CandyBodyRole.LeftHalf, 5f, 5f);
            FakeWorld world = new() { Bodies = [left] };
            TutorialDirector director = new(world);
            (TutorialPrompt prompt, _) = MakePrompt(
                TutorialEvent.PumpFire,
                TutorialSubject.Left,
                new TutorialArea(0f, 0f, 10f, 10f));
            director.Add(prompt);
            director.CompleteLoading();

            director.Fire(TutorialEvent.PumpFire);

            Assert.Equal(TutorialPromptState.Playing, prompt.State);
        }

        [Fact]
        public void ScopedPushUsesCausalBodyForArea()
        {
            CandyBody outside = Body(CandyBodyRole.Whole, 20f, 20f);
            CandyBody inside = Body(CandyBodyRole.Whole, 5f, 5f);
            FakeWorld world = new() { Bodies = [inside, outside] };
            TutorialDirector director = new(world);
            (TutorialPrompt prompt, _) = MakePrompt(
                TutorialEvent.BubbleCapture,
                TutorialSubject.Any,
                new TutorialArea(0f, 0f, 10f, 10f));
            director.Add(prompt);
            director.CompleteLoading();

            director.Fire(TutorialEvent.BubbleCapture, outside);
            Assert.Equal(TutorialPromptState.Armed, prompt.State);

            director.Fire(TutorialEvent.BubbleCapture, inside);
            Assert.Equal(TutorialPromptState.Playing, prompt.State);
        }

        [Fact]
        public void StateEvaluationChecksEachActiveBody()
        {
            CandyBody primary = Body(CandyBodyRole.Whole, 0f, 0f);
            CandyBody right = Body(CandyBodyRole.RightHalf, 0f, 0f);
            FakeWorld world = new()
            {
                Bodies = [primary, right],
                HoldsResult = (_, body) => body == right,
            };
            TutorialDirector director = new(world);
            (TutorialPrompt prompt, _) = MakePrompt(TutorialEvent.Bubbled, TutorialSubject.Right);
            director.Add(prompt);
            director.CompleteLoading();

            director.Update(0f);

            Assert.Equal(2, world.HoldsCalls);
            Assert.Equal(TutorialPromptState.Playing, prompt.State);
        }

        [Fact]
        public void RocketIgnitionHistoryIsKeyedByRocketIdentity()
        {
            CandyBody primary = Body(CandyBodyRole.Whole, 0f, 0f);
            CandyContext owner = new(primary);
            CandyBody right = Body(CandyBodyRole.RightHalf, 0f, 0f);
            right.AttachTo(owner);
            Rocket rocketA = new() { state = Rocket.STATE_ROCKET_IDLE };
            Rocket rocketB = new() { state = Rocket.STATE_ROCKET_IDLE };
            FakeWorld world = new()
            {
                Bodies = [primary, right],
                RocketStates =
                [
                    new TutorialRocketState(rocketA, primary, rocketA.state),
                    new TutorialRocketState(rocketB, right, rocketB.state),
                ],
            };
            TutorialDirector director = new(world);
            (TutorialPrompt first, _) = MakePrompt(TutorialEvent.RocketIgnite, TutorialSubject.Primary);
            (TutorialPrompt second, _) = MakePrompt(TutorialEvent.RocketIgnite, TutorialSubject.Right);
            director.Add(first);
            director.Add(second);
            director.CompleteLoading();
            director.Update(0f);

            rocketA.state = Rocket.STATE_ROCKET_FLY;
            world.RefreshRocketStates();
            director.Update(0f);

            Assert.Equal(TutorialPromptState.Playing, first.State);
            Assert.Equal(TutorialPromptState.Armed, second.State);

            rocketB.state = Rocket.STATE_ROCKET_FLY;
            world.RefreshRocketStates();
            director.Update(0f);

            Assert.Equal(TutorialPromptState.Playing, second.State);
        }

        [Fact]
        public void UpdateSkipsStateAndRocketSamplingWithoutArmedConsumers()
        {
            FakeWorld world = new();
            TutorialDirector director = new(world);
            director.CompleteLoading();

            director.Update(1f);

            Assert.Equal(0, world.ActiveBodyReads);
            Assert.Equal(0, world.RocketReads);
            Assert.Equal(0, world.HoldsCalls);
        }

        [Fact]
        public void DrawListsPreserveXmlOrderWithinTextAndImages()
        {
            List<string> draws = [];
            TutorialDirector director = new(new FakeWorld());
            director.Add(MakePrompt(TutorialEvent.Start, isText: false, drawName: "image-1", draws: draws).Prompt);
            director.Add(MakePrompt(TutorialEvent.Start, isText: true, drawName: "text-1", draws: draws).Prompt);
            director.Add(MakePrompt(TutorialEvent.Start, isText: false, drawName: "image-2", draws: draws).Prompt);
            director.Add(MakePrompt(TutorialEvent.Start, isText: true, drawName: "text-2", draws: draws).Prompt);

            director.DrawTexts();
            director.DrawImages();

            Assert.Equal(["text-1", "text-2", "image-1", "image-2"], draws);
        }

        private static (TutorialPrompt Prompt, CountingVisual Visual) MakePrompt(
            TutorialEvent tutorialEvent,
            TutorialSubject subject = TutorialSubject.Any,
            TutorialArea? area = null,
            string group = null,
            float delay = 0f,
            bool isText = true,
            string drawName = null,
            List<string> draws = null)
        {
            CountingVisual visual = new(drawName, draws);
            TutorialPrompt prompt = new(
                visual,
                new TutorialTrigger(tutorialEvent, area, subject),
                group,
                delay,
                fadeIn: 1f,
                hold: 5f,
                fadeOut: 0.5f,
                isText);
            return (prompt, visual);
        }

        private static CandyBody Body(CandyBodyRole role, float x, float y)
        {
            ConstraintedPoint point = new() { pos = new Vector(x, y) };
            return new CandyBody(point, role);
        }

        private sealed class CountingVisual : BaseElement
        {
            private readonly string drawName;
            private readonly List<string> draws;

            internal CountingVisual(string drawName, List<string> draws)
            {
                this.drawName = drawName;
                this.draws = draws;
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.transparentRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    0f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.solidOpaqueRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    1f));
                AddTimelinewithID(timeline, 0);
            }

            internal int PlayCount { get; private set; }

            internal int UpdateCount { get; private set; }

            internal float UpdatedSeconds { get; private set; }

            public override void PlayTimeline(int timeline)
            {
                PlayCount++;
                base.PlayTimeline(timeline);
            }

            public override void Update(float delta)
            {
                UpdateCount++;
                UpdatedSeconds += delta;
                base.Update(delta);
            }

            public override void Draw()
            {
                draws?.Add(drawName);
            }
        }

        private sealed class FakeWorld : ITutorialWorld
        {
            internal IReadOnlyList<CandyBody> Bodies { get; init; } = [];

            internal IReadOnlyList<TutorialRocketState> RocketStates { get; set; } = [];

            internal Func<TutorialEvent, CandyBody, bool> HoldsResult { get; init; } = (_, _) => false;

            internal int ActiveBodyReads { get; private set; }

            internal int RocketReads { get; private set; }

            internal int HoldsCalls { get; private set; }

            public IReadOnlyList<CandyBody> ActiveBodies
            {
                get
                {
                    ActiveBodyReads++;
                    return Bodies;
                }
            }

            public IReadOnlyList<TutorialRocketState> Rockets
            {
                get
                {
                    RocketReads++;
                    return RocketStates;
                }
            }

            public bool Holds(TutorialEvent tutorialEvent, CandyBody body)
            {
                HoldsCalls++;
                return HoldsResult(tutorialEvent, body);
            }

            internal void RefreshRocketStates()
            {
                List<TutorialRocketState> refreshed = [];
                foreach (TutorialRocketState rocketState in RocketStates)
                {
                    refreshed.Add(rocketState with { State = rocketState.Rocket.state });
                }
                RocketStates = refreshed;
            }
        }
    }
}
