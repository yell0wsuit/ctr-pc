using System;

using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// An eased keyframe segment has to land on its target by the time it ends. It integrates a
    /// constant acceleration frame by frame, and a step that applies the wrong velocity leaves the
    /// element short of the keyframe until the segment boundary snaps it there - a visible jump at
    /// the end of every eased move in the game.
    /// </summary>
    public sealed class EasedKeyFrameIntegrationTests
    {
        private const float Frame = 0.016f;

        [Fact]
        public void AnEaseOutTracksItsAnalyticCurve()
        {
            // Constant deceleration from twice the average speed: x(t) = 2*avg*t - (avg/d)*t^2.
            AssertTracksCurve(
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
                (t, travel, seconds) =>
                {
                    float average = travel / seconds;
                    return (2f * average * t) - (average / seconds * t * t);
                });
        }

        [Fact]
        public void AnEaseInTracksItsAnalyticCurve()
        {
            // Constant acceleration from rest: x(t) = (avg/d)*t^2.
            AssertTracksCurve(
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN,
                (t, travel, seconds) => travel / seconds / seconds * t * t);
        }

        private static void AssertTracksCurve(
            KeyFrame.TransitionType transition,
            Func<float, float, float, float> analytic)
        {
            const float travel = 220f;
            const float seconds = 0.5f;
            BaseElement element = Play(transition, travel, seconds);

            float worst = 0f;
            float worstAt = 0f;
            for (int frame = 1; frame * Frame <= seconds; frame++)
            {
                element.Update(Frame);
                float t = frame * Frame;
                float error = MathF.Abs(element.x - analytic(t, travel, seconds));
                if (error > worst)
                {
                    worst = error;
                    worstAt = t;
                }
            }

            Assert.True(worst < 1f, $"drifted {worst:0.##} units from the curve at t={worstAt:0.###}");
        }

        [Fact]
        public void AnEasedMoveCoversItsWholeDistanceBeforeTheBoundary()
        {
            const float travel = 220f;
            BaseElement element = Play(
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
                travel,
                0.5f);

            // One frame short of the 0.5s segment, an exact integration is within a frame's worth
            // of travel. The defect left it 14 units short of 220.
            for (int frame = 0; frame < 31; frame++)
            {
                element.Update(Frame);
            }

            Assert.InRange(element.x, travel - 2f, travel);
        }

        private static BaseElement Play(
            KeyFrame.TransitionType transition,
            float travel,
            float seconds)
        {
            BaseElement element = new() { x = 0f, y = 0f };
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos(
                0f, 0f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline.AddKeyFrame(KeyFrame.MakePos(travel, 0f, transition, seconds));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
            element.AddTimelinewithID(timeline, 0);
            element.PlayTimeline(0);
            return element;
        }
    }
}
