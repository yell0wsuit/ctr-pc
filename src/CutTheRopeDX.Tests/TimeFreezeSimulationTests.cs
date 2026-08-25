using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TimeFreezeSimulationTests
    {
        private static GameScene FrozenSceneWithFallingCandy()
        {
            Scenario scenario = Scenario.New().Candy(160, 100).OmNom(160, 400).PauseSwitcher(60, 400);
            GameScene scene = scenario.Build();
            HeadlessGame.StepFrames(scene, 5);
            Freeze(scene);
            HeadlessGame.StepFrames(scene, 1);
            return scene;
        }

        private static void Freeze(GameScene scene)
        {
            Vector button = scene.ScreenPositionOf(scene.PauseSwitchers()[0]);
            _ = scene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = scene.TouchUpXYIndex(button.X, button.Y, 0);
        }

        [Fact]
        public void CandyDoesNotDriftWhileFrozen()
        {
            GameScene scene = FrozenSceneWithFallingCandy();
            Vector before = scene.Candy().WholeBody.Point.pos;

            HeadlessGame.StepFrames(scene, 120);

            Vector after = scene.Candy().WholeBody.Point.pos;
            Assert.Equal(before.X, after.X, 3);
            Assert.Equal(before.Y, after.Y, 3);
        }

        [Fact]
        public void FrozenCandySkipsNormalPointIntegrationBeforeTheFinalHold()
        {
            GameScene scene = FrozenSceneWithFallingCandy();
            ConstraintedPoint point = scene.Candy().WholeBody.Point;
            point.totalForce = new Vector(123f, 456f);

            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(new Vector(123f, 456f), point.totalForce);
            Assert.Equal(default, point.a);
            Assert.Equal(default, point.v);
            Assert.Equal(default, point.posDelta);
        }

        [Fact]
        public void RopeSegmentsKeepSimulatingWhileFrozenCandyStaysPinned()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 240, "first")
                .Rope(160, 100, 170, "first")
                .OmNom(160, 440)
                .PauseSwitcher(60, 440)
                .Build();
            HeadlessGame.StepFrames(scene, 5);
            Freeze(scene);
            Bungee rope = Assert.Single(scene.RegisteredRopes()).Rope;
            HeadlessGame.StepFrames(scene, 1);
            ConstraintedPoint middle = rope.parts[rope.parts.Count / 2];
            middle.prevPos = middle.pos;
            middle.pos = new Vector(middle.pos.X + 30f, middle.pos.Y);
            Vector displaced = middle.pos;
            Vector candyAt = scene.Candy().WholeBody.Point.pos;

            HeadlessGame.StepFrames(scene, 1);

            Assert.NotEqual(displaced, middle.pos);
            Assert.Equal(candyAt.X, scene.Candy().WholeBody.Point.pos.X, 3);
            Assert.Equal(candyAt.Y, scene.Candy().WholeBody.Point.pos.Y, 3);
        }

        [Fact]
        public void FrozenAxeStopsPhysicsAndBladeSpinButKeepsBubbleAnimationAdvancing()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 100)
                .Axe(200, 180)
                .OmNom(160, 440)
                .PauseSwitcher(60, 440)
                .Build();
            CandyContext axeContext = Assert.Single(scene.Candies(), context => context.Capabilities == CandyCapabilities.Axe);
            Axe axe = Assert.IsType<Axe>(axeContext.WholeBody.Visual);
            axe.constraint.v = new Vector(100f, 0f);
            Freeze(scene);
            float rotation = axe.GetChild(1).rotation;
            float bubbleTime = axe.bubbleAnimation.GetTimeline(0).time;

            HeadlessGame.StepFrames(scene, 4);

            Assert.Equal(rotation, axe.GetChild(1).rotation);
            Assert.NotEqual(bubbleTime, axe.bubbleAnimation.GetTimeline(0).time);
        }

        [Fact]
        public void FrozenCandyBubbleAnimationKeepsAdvancing()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Bubble(160, 200)
                .OmNom(160, 440)
                .PauseSwitcher(60, 440)
                .Build();
            CandyContext candy = scene.Candy();
            _ = Act.CaptureInBubble(scene, candy);
            Freeze(scene);
            Timeline timeline = candy.WholeBody.BubbleAnimation.GetTimeline(0);
            float before = timeline.time;

            HeadlessGame.StepFrames(scene, 4);

            Assert.NotEqual(before, timeline.time);
        }

        [Fact]
        public void FreezeStopsParticleUpdatesWithoutChangingDrawVisibility()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            AnimationsPool pool = scene.ParticleAnimations();
            bool authoredVisibility = pool.visible;

            Freeze(scene);

            Assert.False(pool.updateable);
            Assert.Equal(authoredVisibility, pool.visible);

            Freeze(scene);

            Assert.True(pool.updateable);
            Assert.Equal(authoredVisibility, pool.visible);
        }

        [Fact]
        public void CandyFallsAgainAfterUnfreezing()
        {
            GameScene scene = FrozenSceneWithFallingCandy();
            HeadlessGame.StepFrames(scene, 30);
            Vector frozenAt = scene.Candy().WholeBody.Point.pos;

            Freeze(scene);
            HeadlessGame.StepFrames(scene, 30);

            Assert.True(scene.Candy().WholeBody.Point.pos.Y > frozenAt.Y);
        }

        [Fact]
        public void MovingSpikesHoldStillWhileFrozen()
        {
            Scenario scenario = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 440)
                .MovingSpikes(160, 300)
                .PauseSwitcher(60, 440);
            GameScene scene = scenario.Build();
            Spikes moving = scene.SpikeStrips()[0];
            HeadlessGame.StepFrames(scene, 20);
            Assert.NotNull(moving.mover);
            Freeze(scene);
            float x = moving.x;
            float y = moving.y;

            HeadlessGame.StepFrames(scene, 120);

            Assert.Equal(x, moving.x, 3);
            Assert.Equal(y, moving.y, 3);
        }

        [Fact]
        public void OmNomDoesNotOpenHisMouthWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            TargetContext target = scene.Targets()[0];
            Freeze(scene);
            Interaction.PlaceCandyAt(
                scene.Candy(),
                new Vector(target.targetObject.x, target.targetObject.y - 100f));

            HeadlessGame.StepFrames(scene, 2);

            Assert.Equal(TargetFeedingPhase.Idle, target.Feeding.Phase);
        }

        [Fact]
        public void OmNomAnimationHoldsItsCurrentFrameUntilTimeResumes()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            Timeline timeline = Assert.IsType<Timeline>(scene.Targets()[0].targetObject.GetCurrentTimeline());
            HeadlessGame.StepFrames(scene, 4);
            Freeze(scene);
            float frozenAt = timeline.time;

            HeadlessGame.StepFrames(scene, 4);

            Assert.Equal(frozenAt, timeline.time);

            Freeze(scene);
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(timeline.time > frozenAt);
        }

        [Fact]
        public void BothTimeTravelOmNomsHoldTheirCurrentFramesWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(100, 400)
                .OmNom(220, 400)
                .PauseSwitcher(60, 440)
                .Build();
            Timeline first = Assert.IsType<Timeline>(scene.Targets()[0].targetObject.GetCurrentTimeline());
            Timeline second = Assert.IsType<Timeline>(scene.Targets()[1].targetObject.GetCurrentTimeline());
            HeadlessGame.StepFrames(scene, 4);
            Freeze(scene);
            float firstFrozenAt = first.time;
            float secondFrozenAt = second.time;

            HeadlessGame.StepFrames(scene, 4);

            Assert.Equal(firstFrozenAt, first.time);
            Assert.Equal(secondFrozenAt, second.time);

            Freeze(scene);
            HeadlessGame.StepFrames(scene, 1);

            Assert.True(first.time > firstFrozenAt);
            Assert.True(second.time > secondFrozenAt);
        }

        [Fact]
        public void IdleRocketDoesNotBindCandyWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200)
                .PauseSwitcher(60, 440)
                .Build();
            CandyContext candy = scene.Candy();
            Rocket rocket = scene.Rockets()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(rocket.x, rocket.y));
            Freeze(scene);

            HeadlessGame.StepFrames(scene, 2);

            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
        }

        [Fact]
        public void RocketControlPointUsesTheIosWeightInTheDefaultPhysicsModel()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200)
                .PauseSwitcher(60, 440)
                .Build();

            Rocket rocket = Assert.Single(scene.Rockets());

            Assert.Equal(0.5f, rocket.point.weight);
        }

        [Fact]
        public void ExperimentsRocketKeepsItsAuthoredImpulseInDx()
        {
            _ = Scenario.New()
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200, impulse: 20f)
                .Build();

            Assert.False(ActivePhysicsConstants.UseMobilePhysicsModel);
            Assert.Equal(600f, ActivePhysicsConstants.RocketReelSpeed);
            Assert.False(ActivePhysicsConstants.UseTimeTravelRocketModel);
            Assert.Equal(1f, ActivePhysicsConstants.RocketImpulseScale);
        }

        [Fact]
        public void MobileExperimentsRocketScalesItsAuthoredImpulseIntoDxWorldCoordinates()
        {
            _ = Scenario.New()
                .Design("useMobilePhysics", "true")
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200, impulse: 20f)
                .Build();

            Assert.True(ActivePhysicsConstants.UseMobilePhysicsModel);
            Assert.False(ActivePhysicsConstants.UseTimeTravelRocketModel);
            Assert.Equal(Scenario.Scale, ActivePhysicsConstants.RocketImpulseScale);
        }

        [Fact]
        public void TimeTravelRocketScalesItsAuthoredImpulseIntoDxWorldCoordinates()
        {
            GameScene scene = Scenario.New()
                .Design("useTimeTravelRocketPhysics", "true")
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200, impulse: 5f, impulseFactor: 0.6f)
                .Build();

            Rocket rocket = Assert.Single(scene.Rockets());
            Assert.True(ActivePhysicsConstants.UseTimeTravelRocketModel);
            Assert.Equal(5f, rocket.impulse);
            Assert.Equal(0.6f, rocket.impulseFactor);
            Assert.Equal(Scenario.Scale, ActivePhysicsConstants.RocketImpulseScale);
        }

        [Fact]
        public void ActiveRocketStoresTheIosVelocityOpposingForceOnItsCandy()
        {
            GameScene scene = Scenario.New()
                .Design("useTimeTravelRocketPhysics", "true")
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200, time: 2f)
                .PauseSwitcher(60, 440)
                .Build();
            _ = Act.BindRocket(scene, scene.Candy());
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            candyPoint.v = new Vector(30f, -12f);

            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(new Vector(-candyPoint.v.X, -candyPoint.v.Y), candyPoint.GetForce(0));
        }

        [Fact]
        public void CandyWithoutARocketClearsTheRocketForceSlot()
        {
            GameScene scene = Scenario.New()
                .Design("useTimeTravelRocketPhysics", "true")
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(260, 200)
                .PauseSwitcher(60, 440)
                .Build();
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            candyPoint.SetForcewithID(new Vector(10f, 20f), 0);

            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(default, candyPoint.GetForce(0));
        }

        [Fact]
        public void ExperimentsRocketDoesNotOwnTimeTravelsForceSlot()
        {
            GameScene scene = Scenario.New()
                .Design("useMobilePhysics", "true")
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200, time: 2f)
                .Build();
            _ = Act.BindRocket(scene, scene.Candy());
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            Vector unrelatedForce = new(10f, 20f);
            candyPoint.SetForcewithID(unrelatedForce, 0);

            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(unrelatedForce, candyPoint.GetForce(0));
        }

        [Fact]
        public void ExperimentsRocketUsesRecoveredVelocityDampingDivisors()
        {
            bool previous = ActivePhysicsConstants.UseMobilePhysicsModel;
            try
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = false;
                Assert.Equal(14f, ActivePhysicsConstants.ExperimentsRocketVelocityDamping);

                ActivePhysicsConstants.UseMobilePhysicsModel = true;
                Assert.Equal(14f, ActivePhysicsConstants.ExperimentsRocketVelocityDamping);
            }
            finally
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = previous;
            }
        }

        [Fact]
        public void MovingRocketContinuesItsAuthoredPathWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200, path: "80,0", moveSpeed: 30f)
                .PauseSwitcher(60, 440)
                .Build();
            Rocket rocket = scene.Rockets()[0];
            Freeze(scene);
            Vector before = new(rocket.x, rocket.y);

            HeadlessGame.StepFrames(scene, 60);

            Assert.NotEqual(before, new Vector(rocket.x, rocket.y));
        }

        [Fact]
        public void FlyingRocketDoesNotConsumeFuelWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200, time: 2f)
                .PauseSwitcher(60, 440)
                .Build();
            Rocket rocket = Act.BindRocket(scene, scene.Candy());
            Freeze(scene);
            float before = rocket.time;

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(before, rocket.time);
        }

        [Fact]
        public void RopesStillCutWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200, "first")
                .Rope(160, 120, 110, "first")
                .OmNom(160, 440)
                .PauseSwitcher(60, 440)
                .Build();
            HeadlessGame.StepFrames(scene, 5);
            Freeze(scene);
            Bungee rope = Assert.Single(scene.RegisteredRopes()).Rope;
            Assert.Equal(-1, rope.cut);
            int segment = rope.parts.Count / 2;
            segment = Math.Min(segment, rope.parts.Count - 2);
            Vector from = rope.parts[segment].pos;
            Vector to = rope.parts[segment + 1].pos;
            Vector midpoint = new((from.X + to.X) / 2f, (from.Y + to.Y) / 2f);
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float length = MathF.Sqrt((dx * dx) + (dy * dy));
            Assert.True(length > 0f);
            const float reach = 40f;
            Vector start = scene.ScreenPositionOf(new Vector(
                midpoint.X + (dy / length * reach),
                midpoint.Y - (dx / length * reach)));
            Vector end = scene.ScreenPositionOf(new Vector(
                midpoint.X - (dy / length * reach),
                midpoint.Y + (dx / length * reach)));

            _ = scene.TouchDownXYIndex(start.X, start.Y, 0);
            _ = scene.TouchMoveXYIndex(end.X, end.Y, 0);
            _ = scene.TouchUpXYIndex(end.X, end.Y, 0);

            Assert.NotEqual(-1, rope.cut);
        }

        [Fact]
        public void LoopingGameplaySoundsStopAndRestartAcrossTimeFreeze()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            RecordingAudioBackend backend = new();
            bool originalSoundPreference = Preferences.GetBooleanForKey("SOUND_ON");
            SoundMgr.SetBackend(backend);
            manager.StopAllSounds();
            Preferences.SetBooleanForKey(true, "SOUND_ON");

            try
            {
                GameScene scene = Scenario.New()
                    .Candy(160, 200)
                    .OmNom(160, 440)
                    .Rocket(160, 200, time: 2f)
                    .ElectroSpikes(260, 300)
                    .PauseSwitcher(60, 440)
                    .Build();
                Rocket rocket = Act.BindRocket(scene, scene.Candy());
                Spikes electro = scene.SpikeStrips()[0];
                Assert.True(Interaction.StepUntil(scene, () => electro.ElectricLoopPlaying));
                ISoundInstance originalRocketLoop = rocket.flyLoopSound;
                Assert.NotNull(originalRocketLoop);

                Freeze(scene);

                Assert.False(electro.ElectricLoopPlaying);
                Assert.Null(rocket.flyLoopSound);

                Freeze(scene);

                Assert.True(electro.ElectricLoopPlaying);
                Assert.NotNull(rocket.flyLoopSound);
                Assert.NotSame(originalRocketLoop, rocket.flyLoopSound);
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.SetBackend(null);
                Preferences.SetBooleanForKey(originalSoundPreference, "SOUND_ON");
            }
        }

        private sealed class RecordingAudioBackend : IAudioBackend
        {
            public AudioPlaybackState MusicState => AudioPlaybackState.Stopped;

            public ISoundEffect LoadSound(string contentPath)
            {
                return new RecordingSoundEffect();
            }

            public IMusicTrack LoadMusic(string contentPath)
            {
                throw new NotSupportedException();
            }

            public void PlayMusic(IMusicTrack track, bool repeating)
            {
            }

            public void StopMusic()
            {
            }

            public void PauseMusic()
            {
            }

            public void ResumeMusic()
            {
            }

            public bool TryInstallSongCompletionCallback(IMusicTrack track, EventHandler<EventArgs> onDecoderFinished)
            {
                return false;
            }
        }

        private sealed class RecordingSoundEffect : ISoundEffect
        {
            public ISoundInstance CreateInstance()
            {
                return new RecordingSoundInstance();
            }

            public void Dispose()
            {
            }
        }

        private sealed class RecordingSoundInstance : ISoundInstance
        {
            public bool IsLooped { get; set; }

            public float Volume { get; set; }

            public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Stopped;

            public void Play()
            {
                State = AudioPlaybackState.Playing;
            }

            public void Stop()
            {
                State = AudioPlaybackState.Stopped;
            }

            public void Pause()
            {
                State = AudioPlaybackState.Paused;
            }

            public void Resume()
            {
                State = AudioPlaybackState.Playing;
            }

            public void Dispose()
            {
            }
        }
    }
}
