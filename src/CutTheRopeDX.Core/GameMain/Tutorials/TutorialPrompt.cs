using System;

using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal enum TutorialPromptState
    {
        Armed,
        Delaying,
        Playing,
        Done,
        Cancelled,
    }

    internal sealed class TutorialPrompt
    {
        private float delayRemaining;

        internal TutorialPrompt(
            BaseElement visual,
            TutorialTrigger trigger,
            string group,
            float delay,
            float fadeIn,
            float hold,
            float fadeOut,
            bool isText)
        {
            Visual = visual ?? throw new ArgumentNullException(nameof(visual));
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Group = group;
            Delay = delay;
            FadeIn = fadeIn;
            Hold = hold;
            FadeOut = fadeOut;
            IsText = isText;
            delayRemaining = delay;
        }

        internal BaseElement Visual { get; }

        internal TutorialTrigger Trigger { get; }

        internal string Group { get; }

        internal float Delay { get; }

        internal float FadeIn { get; }

        internal float Hold { get; }

        internal float FadeOut { get; }

        internal bool IsText { get; }

        internal TutorialPromptState State { get; private set; } = TutorialPromptState.Armed;

        internal bool BeginDelayOrPlay()
        {
            if (State != TutorialPromptState.Armed)
            {
                return false;
            }

            if (delayRemaining > 0f)
            {
                State = TutorialPromptState.Delaying;
            }
            else
            {
                BeginPlayback();
            }

            return true;
        }

        internal float AdvanceDelay(float delta)
        {
            if (State != TutorialPromptState.Delaying)
            {
                throw new InvalidOperationException($"Cannot advance tutorial delay from {State}.");
            }

            float consumed = MathF.Min(delayRemaining, delta);
            delayRemaining -= consumed;
            if (delayRemaining <= 0f)
            {
                BeginPlayback();
            }

            return delta - consumed;
        }

        internal void AdvancePlayback(float delta)
        {
            if (State != TutorialPromptState.Playing)
            {
                throw new InvalidOperationException($"Cannot advance tutorial playback from {State}.");
            }

            Visual.Update(delta);
            if (Visual.GetCurrentTimeline()?.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                MarkDone();
            }
        }

        internal void MarkDone()
        {
            if (State != TutorialPromptState.Playing)
            {
                throw new InvalidOperationException($"Cannot finish tutorial prompt from {State}.");
            }

            State = TutorialPromptState.Done;
        }

        internal void Cancel()
        {
            if (State is not (TutorialPromptState.Armed or TutorialPromptState.Delaying))
            {
                return;
            }

            State = TutorialPromptState.Cancelled;
        }

        private void BeginPlayback()
        {
            State = TutorialPromptState.Playing;
            Visual.PlayTimeline(0);
        }
    }
}
