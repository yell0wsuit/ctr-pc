using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Screen-covering plates and repeating waves shown while gameplay time is stopped.
    /// </summary>
    internal sealed class PauseSwitcherWaves : BaseElement, ITimelineDelegate
    {
        private const float BurstInterval = 0.8f;
        private const float ChildTimeScale = 0.8f;
        private const int VerticalWavesPerEdge = 5;
        private const int HorizontalWavesPerEdge = 8;
        private const int FadeInTimeline = 0;
        private const int FadeOutTimeline = 1;

        private readonly AnimationsPool wavePool = new();
        private Image plate;
        private readonly FlashXmlOneShotEffect waveEffect =
            new("fx_pause.xml", Resources.Img.FxPause);
        private readonly Random random = new();
        private float sinceLastBurst;

        /// <summary>Number of wave animations currently alive.</summary>
        public int ActiveWaveCount => wavePool.ChildCount;

        /// <summary>Builds a hidden overlay covering the given area.</summary>
        /// <param name="coverWidth">Width swept by the waves.</param>
        /// <param name="coverHeight">Height swept by the waves.</param>
        /// <returns>The new overlay.</returns>
        public static PauseSwitcherWaves Create(float coverWidth, float coverHeight)
        {
            PauseSwitcherWaves overlay = new()
            {
                visible = false,
                color = RGBAColor.transparentRGBA,
                plate = Image.Image_createWithResIDQuad(Resources.Img.ObjPause, 2)
            };

            overlay.plate.anchor = overlay.plate.parentAnchor = 18;
            Image innerPlate = Image.Image_createWithResIDQuad(Resources.Img.ObjPause, 3);
            innerPlate.anchor = innerPlate.parentAnchor = 18;
            innerPlate.blendingMode = 2;
            _ = overlay.plate.AddChild(innerPlate);
            _ = overlay.AddChild(overlay.plate);
            _ = overlay.AddChild(overlay.wavePool);
            overlay.AddFadeTimelines();
            overlay.Resize(coverWidth, coverHeight);
            return overlay;
        }

        /// <summary>Sets the area swept by waves from each screen edge.</summary>
        /// <param name="coverWidth">Width swept by the waves.</param>
        /// <param name="coverHeight">Height swept by the waves.</param>
        public void Resize(float coverWidth, float coverHeight)
        {
            width = (int)MathF.Round(coverWidth);
            height = (int)MathF.Round(coverHeight);
            if (plate != null && plate.width > 0 && plate.height > 0)
            {
                plate.scaleX = coverWidth / plate.width;
                plate.scaleY = coverHeight / plate.height;
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta * ChildTimeScale);
            sinceLastBurst += delta;
            if (sinceLastBurst > BurstInterval)
            {
                sinceLastBurst = 0f;
                CreateWaves();
            }
        }

        /// <summary>Shows the overlay and fades it in.</summary>
        public void PlayFadeIn()
        {
            visible = true;
            PlayTimeline(FadeInTimeline);
        }

        /// <summary>Shows the overlay while it fades out.</summary>
        public void PlayFadeOut()
        {
            visible = true;
            PlayTimeline(FadeOutTimeline);
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            if (t == GetTimeline(FadeOutTimeline))
            {
                visible = false;
            }
        }

        /// <summary>Adds the stopping and restarting alpha fades.</summary>
        private void AddFadeTimelines()
        {
            Timeline fadeIn = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            fadeIn.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.MakeRGBA(1f, 1f, 1f, 0f),
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                0f));
            fadeIn.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.solidOpaqueRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                0.1f));
            fadeIn.delegateTimelineDelegate = this;
            AddTimelinewithID(fadeIn, FadeInTimeline);

            Timeline fadeOut = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            fadeOut.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.solidOpaqueRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                0f));
            fadeOut.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.MakeRGBA(1f, 1f, 1f, 0f),
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                0.1f));
            fadeOut.delegateTimelineDelegate = this;
            AddTimelinewithID(fadeOut, FadeOutTimeline);
        }

        /// <summary>Spawns one burst from all four screen edges.</summary>
        private void CreateWaves()
        {
            for (int i = 0; i < VerticalWavesPerEdge; i++)
            {
                waveEffect.SpawnInto(wavePool, RandomUpTo(width), height, 0);
            }
            for (int i = 0; i < VerticalWavesPerEdge; i++)
            {
                waveEffect.SpawnInto(wavePool, RandomUpTo(width), 0f, 0, 180f);
            }
            for (int i = 0; i < HorizontalWavesPerEdge; i++)
            {
                waveEffect.SpawnInto(wavePool, 0f, RandomUpTo(height), 0, 90f);
            }
            for (int i = 0; i < HorizontalWavesPerEdge; i++)
            {
                waveEffect.SpawnInto(wavePool, width, RandomUpTo(height), 0, -90f);
            }
        }

        /// <summary>Picks a coordinate along an edge, including both endpoints.</summary>
        /// <param name="extent">Length of the edge.</param>
        /// <returns>A coordinate on the edge.</returns>
        private float RandomUpTo(float extent)
        {
            return random.Next((int)extent + 1);
        }
    }
}
