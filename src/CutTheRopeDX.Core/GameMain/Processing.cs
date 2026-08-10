using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Full-screen processing overlay that blocks input while fading in or out.
    /// </summary>
    internal sealed class Processing : RectangleElement, ITimelineDelegate
    {
        /// <summary>
        /// Initializes the processing overlay.
        /// </summary>
        /// <param name="loading">Whether to include the spinning loading indicator and processing text.</param>
        /// <returns>The initialized processing overlay.</returns>
        public Processing InitWithLoading(bool loading)
        {
            width = (int)SCREEN_WIDTH_EXPANDED;
            height = (int)SCREEN_HEIGHT_EXPANDED + 1;
            x = 0f - SCREEN_OFFSET_X;
            y = 0f - SCREEN_OFFSET_Y;
            blendingMode = 0;
            if (loading)
            {
                Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuProcessingHd, 0);
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline.AddKeyFrame(KeyFrame.MakeRotation(360, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1f));
                timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                _ = image.AddTimeline(timeline);
                image.PlayTimeline(0);
                Text c = Text.CreateWithFontandString(Resources.Fnt.BigFont, Application.GetString("PROCESSING"));
                HBox hBox = new HBox().InitWithOffsetAlignHeight(10f, 16, image.height);
                hBox.parentAnchor = hBox.anchor = 18;
                _ = AddChild(hBox);
                _ = hBox.AddChild(image);
                _ = hBox.AddChild(c);
            }
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0, 0, 0, 0.4f), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            _ = AddTimeline(timeline2);
            timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline2.delegateTimelineDelegate = this;
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0, 0, 0, 0.4f), KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            _ = AddTimeline(timeline2);
            PlayTimeline(0);
            return this;
        }

        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            _ = base.OnTouchDownXY(tx, ty);
            return true;
        }

        /// <inheritdoc />
        public override bool OnTouchUpXY(float tx, float ty)
        {
            _ = base.OnTouchUpXY(tx, ty);
            return true;
        }

        /// <inheritdoc />
        public override bool OnTouchMoveXY(float tx, float ty)
        {
            _ = base.OnTouchMoveXY(tx, ty);
            return true;
        }

        /// <inheritdoc />
        public override void PlayTimeline(int t)
        {
            if (t == 0)
            {
                SetEnabled(true);
            }
            base.PlayTimeline(t);
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            SetEnabled(false);
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }
    }
}
