using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Idle cadence edges due on a target animation frame.</summary>
    /// <param name="BlinkDue">Whether the blink reaction is due.</param>
    /// <param name="IdleDue">Whether the idle/chat reaction is due.</param>
    internal readonly record struct TargetIdleStep(bool BlinkDue, bool IdleDue);

    /// <summary>Single owner of one Om Nom's blink, random-idle, and chat cadence.</summary>
    /// <param name="blinkCountdown">Initial animation-frame blink countdown.</param>
    /// <param name="idleCountdown">Initial animation-frame idle countdown.</param>
    internal sealed class TargetIdleState(int blinkCountdown, int idleCountdown)
    {
        private TargetAnimationState? pendingChatState;

        /// <summary>Gets animation frames remaining before blink is due.</summary>
        public int BlinkCountdown { get; private set; } = Math.Max(0, blinkCountdown);

        /// <summary>Gets animation frames remaining before an idle/chat reaction is due.</summary>
        public int IdleCountdown { get; private set; } = Math.Max(0, idleCountdown);

        /// <summary>Gets whether this target owns the pending second half of a chat greeting.</summary>
        public bool HasPendingChat => pendingChatState.HasValue;

        /// <summary>Advances both cadence counters, holding them at zero until consumed.</summary>
        /// <returns>The reactions currently due.</returns>
        public TargetIdleStep AdvanceCadence()
        {
            BlinkCountdown = Math.Max(0, BlinkCountdown - 1);
            IdleCountdown = Math.Max(0, IdleCountdown - 1);
            return new TargetIdleStep(BlinkCountdown == 0, IdleCountdown == 0);
        }

        /// <summary>Consumes a due blink and atomically schedules the next one.</summary>
        /// <param name="nextCountdown">Countdown for the next blink.</param>
        /// <returns><see langword="true"/> when a due blink was consumed.</returns>
        public bool ConsumeBlink(int nextCountdown)
        {
            if (BlinkCountdown != 0)
            {
                return false;
            }

            BlinkCountdown = Math.Max(0, nextCountdown);
            return true;
        }

        /// <summary>Consumes a due idle/chat edge and atomically schedules the next one.</summary>
        /// <param name="nextCountdown">Countdown for the next idle/chat reaction.</param>
        /// <returns><see langword="true"/> when a due reaction was consumed.</returns>
        public bool ConsumeIdle(int nextCountdown)
        {
            if (IdleCountdown != 0)
            {
                return false;
            }

            IdleCountdown = Math.Max(0, nextCountdown);
            return true;
        }

        /// <summary>Reschedules idle/chat cadence, including the other participant in a chat.</summary>
        /// <param name="nextCountdown">Countdown for the next idle/chat reaction.</param>
        public void ScheduleIdle(int nextCountdown)
        {
            IdleCountdown = Math.Max(0, nextCountdown);
        }

        /// <summary>Schedules this target as the second participant in a chat greeting.</summary>
        /// <param name="state">Directional greeting animation to play.</param>
        /// <returns><see langword="true"/> when no chat was already pending.</returns>
        public bool TryScheduleChat(TargetAnimationState state)
        {
            if (pendingChatState.HasValue)
            {
                return false;
            }

            pendingChatState = state;
            return true;
        }

        /// <summary>Atomically consumes the pending second chat greeting.</summary>
        /// <param name="state">The directional greeting animation when one was pending.</param>
        /// <returns><see langword="true"/> when a pending chat was consumed.</returns>
        public bool TryConsumeChat(out TargetAnimationState state)
        {
            if (!pendingChatState.HasValue)
            {
                state = default;
                return false;
            }

            state = pendingChatState.Value;
            pendingChatState = null;
            return true;
        }
    }
}
