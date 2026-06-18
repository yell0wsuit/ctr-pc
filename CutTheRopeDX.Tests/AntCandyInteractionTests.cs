using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class AntCandyInteractionTests
    {
        [Fact]
        public void CanAttach_TrueForAvailableSegmentAndCandyInsideBounds()
        {
            Assert.True(AntCandyInteraction.CanAttach(
                candyPresent: true,
                segmentInteracting: false,
                segmentCanInteract: true,
                candyWaitingForFly: false,
                isLastSegment: false,
                candyInsideBounds: true,
                candyHeldByHand: false));
        }

        [Fact]
        public void CanAttach_FalseWhenCandyOrSegmentStateBlocksInteraction()
        {
            Assert.False(AntCandyInteraction.CanAttach(false, false, true, false, false, true, false));
            Assert.False(AntCandyInteraction.CanAttach(true, segmentInteracting: true, true, false, false, true, false));
            Assert.False(AntCandyInteraction.CanAttach(true, false, segmentCanInteract: false, false, false, true, false));
            Assert.False(AntCandyInteraction.CanAttach(true, false, true, candyWaitingForFly: true, false, true, false));
            Assert.False(AntCandyInteraction.CanAttach(true, false, true, false, isLastSegment: true, true, false));
            Assert.False(AntCandyInteraction.CanAttach(true, false, true, false, false, candyInsideBounds: false, false));
        }

        [Fact]
        public void CanAttach_FalseWhenCandyHeldByHand()
        {
            // A mechanical hand owns the candy; ants must not steal it back onto the conveyor.
            Assert.False(AntCandyInteraction.CanAttach(true, false, true, false, false, true, candyHeldByHand: true));
        }

        [Fact]
        public void ShouldDetach_TrueOnlyAfterSnapTimeWhenCarriedCandyLeavesInternalBounds()
        {
            Assert.True(AntCandyInteraction.ShouldDetach(
                candyCarriedBySegment: true,
                segmentInteracting: true,
                interactionTime: AntConveyorLogic.CarrierSnapTimeThreshold + 0.01f,
                candyInsideInternalBounds: false));

            Assert.False(AntCandyInteraction.ShouldDetach(true, true, AntConveyorLogic.CarrierSnapTimeThreshold, false));
            Assert.False(AntCandyInteraction.ShouldDetach(true, true, AntConveyorLogic.CarrierSnapTimeThreshold + 0.01f, true));
            Assert.False(AntCandyInteraction.ShouldDetach(candyCarriedBySegment: false, true, AntConveyorLogic.CarrierSnapTimeThreshold + 0.01f, false));
            Assert.False(AntCandyInteraction.ShouldDetach(true, segmentInteracting: false, AntConveyorLogic.CarrierSnapTimeThreshold + 0.01f, false));
        }

        [Fact]
        public void ShouldSlowStopAfterDetach_FalseWhenAnotherSegmentContainsCandyExternally()
        {
            Assert.False(AntCandyInteraction.ShouldSlowStopAfterDetach(otherSegmentContainsCandyExternally: true));
            Assert.True(AntCandyInteraction.ShouldSlowStopAfterDetach(otherSegmentContainsCandyExternally: false));
        }
    }
}
