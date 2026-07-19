using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class PendingChangeGateTests
    {
        private static readonly DateTime Start = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);

        private static PendingChangeGate CreateGate()
        {
            return new PendingChangeGate(TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void TryConsume_NoChange_ReturnsFalse()
        {
            PendingChangeGate gate = CreateGate();

            Assert.False(gate.TryConsume(Start));
        }

        [Fact]
        public void TryConsume_BeforeQuietPeriodElapses_ReturnsFalse()
        {
            PendingChangeGate gate = CreateGate();
            gate.NotifyChanged(Start);

            Assert.False(gate.TryConsume(Start.AddMilliseconds(50)));
        }

        [Fact]
        public void TryConsume_AfterQuietPeriodElapses_ReturnsTrue()
        {
            PendingChangeGate gate = CreateGate();
            gate.NotifyChanged(Start);

            Assert.True(gate.TryConsume(Start.AddMilliseconds(150)));
        }

        [Fact]
        public void TryConsume_BurstOfChanges_YieldsSingleReload()
        {
            PendingChangeGate gate = CreateGate();
            gate.NotifyChanged(Start);
            gate.NotifyChanged(Start.AddMilliseconds(10));
            gate.NotifyChanged(Start.AddMilliseconds(20));
            gate.NotifyChanged(Start.AddMilliseconds(30));

            Assert.False(gate.TryConsume(Start.AddMilliseconds(100)));
            Assert.True(gate.TryConsume(Start.AddMilliseconds(200)));
            Assert.False(gate.TryConsume(Start.AddMilliseconds(300)));
        }

        [Fact]
        public void TryConsume_SecondChangeAfterConsume_ReturnsTrueAgain()
        {
            PendingChangeGate gate = CreateGate();
            gate.NotifyChanged(Start);
            Assert.True(gate.TryConsume(Start.AddMilliseconds(150)));

            gate.NotifyChanged(Start.AddMilliseconds(200));
            Assert.True(gate.TryConsume(Start.AddMilliseconds(400)));
        }
    }
}
