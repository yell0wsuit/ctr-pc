using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class TargetIdleStateTests
    {
        [Fact]
        public void CadenceCannotRunPastDueWhileAwaitingItsReaction()
        {
            TargetIdleState idle = new(blinkCountdown: 1, idleCountdown: 1);

            TargetIdleStep first = idle.AdvanceCadence();
            TargetIdleStep second = idle.AdvanceCadence();

            Assert.True(first.BlinkDue);
            Assert.True(first.IdleDue);
            Assert.True(second.BlinkDue);
            Assert.True(second.IdleDue);
            Assert.Equal(0, idle.BlinkCountdown);
            Assert.Equal(0, idle.IdleCountdown);
        }

        [Fact]
        public void ConsumingDueCadenceAtomicallySchedulesTheNextOne()
        {
            TargetIdleState idle = new(blinkCountdown: 1, idleCountdown: 1);
            _ = idle.AdvanceCadence();

            Assert.True(idle.ConsumeBlink(nextCountdown: 3));
            Assert.True(idle.ConsumeIdle(nextCountdown: 12));
            Assert.False(idle.ConsumeBlink(nextCountdown: 3));
            Assert.False(idle.ConsumeIdle(nextCountdown: 12));

            Assert.Equal(3, idle.BlinkCountdown);
            Assert.Equal(12, idle.IdleCountdown);
        }

        [Fact]
        public void ChatCanRescheduleBothTargetsThroughTheSameOwner()
        {
            TargetIdleState first = new(blinkCountdown: 3, idleCountdown: 1);
            TargetIdleState second = new(blinkCountdown: 3, idleCountdown: 2);

            first.ScheduleIdle(10);
            second.ScheduleIdle(15);

            Assert.Equal(10, first.IdleCountdown);
            Assert.Equal(15, second.IdleCountdown);
        }

        [Fact]
        public void PendingChatIsScheduledAndConsumedAtomically()
        {
            TargetIdleState idle = new(blinkCountdown: 3, idleCountdown: 10);

            Assert.True(idle.TryScheduleChat(TargetAnimationState.GreetLeft));
            Assert.True(idle.HasPendingChat);
            Assert.False(idle.TryScheduleChat(TargetAnimationState.GreetRight));

            Assert.True(idle.TryConsumeChat(out TargetAnimationState state));
            Assert.Equal(TargetAnimationState.GreetLeft, state);
            Assert.False(idle.HasPendingChat);
            Assert.False(idle.TryConsumeChat(out _));
        }
    }
}
