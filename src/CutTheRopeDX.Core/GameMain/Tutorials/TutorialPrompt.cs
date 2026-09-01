using System;

using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Lifecycle states owned by a tutorial prompt.</summary>
    internal enum TutorialPromptState
    {
        /// <summary>The prompt is waiting for its trigger.</summary>
        Armed,
        /// <summary>The prompt has fired and is consuming its authored delay.</summary>
        Delaying,
        /// <summary>The prompt visual timeline is playing.</summary>
        Playing,
        /// <summary>The prompt visual timeline completed.</summary>
        Done,
        /// <summary>A sibling in the same group fired first.</summary>
        Cancelled,
    }

    /// <summary>Owns one tutorial prompt's immutable metadata and legal state transitions.</summary>
    internal sealed class TutorialPrompt
    {
        private float delayRemaining;

        /// <summary>Initializes a tutorial prompt in the armed state.</summary>
        /// <param name="visual">Visual element displayed by the prompt.</param>
        /// <param name="trigger">Immutable trigger conditions.</param>
        /// <param name="group">Optional mutually exclusive group name.</param>
        /// <param name="delay">Seconds to wait after firing.</param>
        /// <param name="fadeIn">Authored fade-in duration.</param>
        /// <param name="hold">Authored full-opacity duration.</param>
        /// <param name="fadeOut">Authored fade-out duration.</param>
        /// <param name="isText">Whether the visual belongs to the text draw list.</param>
        /// <param name="timelineIndex">Timeline played when the prompt starts.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> or <paramref name="trigger"/> is null.</exception>
        internal TutorialPrompt(
            BaseElement visual,
            TutorialTrigger trigger,
            string group,
            float delay,
            float fadeIn,
            float hold,
            float fadeOut,
            bool isText,
            int timelineIndex = 0)
        {
            Visual = visual ?? throw new ArgumentNullException(nameof(visual));
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Group = group;
            Delay = delay;
            FadeIn = fadeIn;
            Hold = hold;
            FadeOut = fadeOut;
            IsText = isText;
            TimelineIndex = timelineIndex;
            delayRemaining = delay;
        }

        /// <summary>Gets the prompt visual.</summary>
        internal BaseElement Visual { get; }

        /// <summary>Gets the immutable trigger conditions.</summary>
        internal TutorialTrigger Trigger { get; }

        /// <summary>Gets the optional mutually exclusive group name.</summary>
        internal string Group { get; }

        /// <summary>Gets the authored delay in seconds.</summary>
        internal float Delay { get; }

        /// <summary>Gets the authored fade-in duration in seconds.</summary>
        internal float FadeIn { get; }

        /// <summary>Gets the authored full-opacity duration in seconds.</summary>
        internal float Hold { get; }

        /// <summary>Gets the authored fade-out duration in seconds.</summary>
        internal float FadeOut { get; }

        /// <summary>Gets whether the visual belongs to the text draw list.</summary>
        internal bool IsText { get; }

        /// <summary>Gets the visual timeline played when the prompt starts.</summary>
        internal int TimelineIndex { get; }

        /// <summary>Gets the current prompt state.</summary>
        internal TutorialPromptState State { get; private set; } = TutorialPromptState.Armed;

        /// <summary>Atomically begins the authored delay or visual playback.</summary>
        /// <returns><see langword="true"/> when the armed prompt transitioned; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>Consumes delay time and begins playback when the delay expires.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        /// <returns>Any elapsed time remaining after the delay is consumed.</returns>
        /// <exception cref="InvalidOperationException">Thrown unless the prompt is delaying.</exception>
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

        /// <summary>Advances the visual and marks the prompt done when its timeline stops.</summary>
        /// <param name="delta">Elapsed playback time in seconds.</param>
        /// <exception cref="InvalidOperationException">Thrown unless the prompt is playing.</exception>
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

        /// <summary>Transitions a playing prompt to done.</summary>
        /// <exception cref="InvalidOperationException">Thrown unless the prompt is playing.</exception>
        internal void MarkDone()
        {
            if (State != TutorialPromptState.Playing)
            {
                throw new InvalidOperationException($"Cannot finish tutorial prompt from {State}.");
            }

            State = TutorialPromptState.Done;
        }

        /// <summary>Cancels an armed or delaying prompt without starting its visual.</summary>
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
            Visual.PlayTimeline(TimelineIndex);
        }
    }
}
