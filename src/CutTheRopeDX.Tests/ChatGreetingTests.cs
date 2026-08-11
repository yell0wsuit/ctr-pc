using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ChatGreetingTests
    {
        [Fact]
        public void SideBySidePairTurnsTowardEachOther()
        {
            // first sits left of second (same row): first turns right, second turns left.
            (TargetAnimationState first, TargetAnimationState second)? states =
                ChatGreeting.ResolveStates(firstX: 100f, firstY: 400f, secondX: 500f, secondY: 402f);

            Assert.True(states.HasValue);
            Assert.Equal(TargetAnimationState.GreetRight, states.Value.first);
            Assert.Equal(TargetAnimationState.GreetLeft, states.Value.second);
        }

        [Fact]
        public void SideBySideDirectionFollowsPositionNotListOrder()
        {
            (TargetAnimationState first, TargetAnimationState second)? states =
                ChatGreeting.ResolveStates(firstX: 500f, firstY: 400f, secondX: 100f, secondY: 400f);

            Assert.True(states.HasValue);
            Assert.Equal(TargetAnimationState.GreetLeft, states.Value.first);
            Assert.Equal(TargetAnimationState.GreetRight, states.Value.second);
        }

        [Fact]
        public void StackedPairTurnsUpAndDown()
        {
            // first sits above second (same column, screen Y grows downward):
            // upper looks down, lower looks up.
            (TargetAnimationState first, TargetAnimationState second)? states =
                ChatGreeting.ResolveStates(firstX: 200f, firstY: 100f, secondX: 202f, secondY: 400f);

            Assert.True(states.HasValue);
            Assert.Equal(TargetAnimationState.GreetDown, states.Value.first);
            Assert.Equal(TargetAnimationState.GreetUp, states.Value.second);
        }

        [Fact]
        public void DiagonalPairDoesNotGreet()
        {
            // Comparable separation on both axes: they cannot face each other.
            Assert.Null(ChatGreeting.ResolveStates(firstX: 100f, firstY: 100f, secondX: 300f, secondY: 280f));
        }

        [Fact]
        public void CoincidentPairDoesNotGreet()
        {
            Assert.Null(ChatGreeting.ResolveStates(firstX: 200f, firstY: 200f, secondX: 200f, secondY: 200f));
        }
    }
}
