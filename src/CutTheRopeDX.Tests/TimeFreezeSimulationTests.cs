using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
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
            segment = System.Math.Min(segment, rope.parts.Count - 2);
            Vector from = rope.parts[segment].pos;
            Vector to = rope.parts[segment + 1].pos;
            Vector midpoint = new((from.X + to.X) / 2f, (from.Y + to.Y) / 2f);
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float length = System.MathF.Sqrt((dx * dx) + (dy * dy));
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
