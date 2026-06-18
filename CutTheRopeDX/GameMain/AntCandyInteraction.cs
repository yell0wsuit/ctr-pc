namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure decisions for candy interaction with ant-conveyor segments.
    /// </summary>
    public static class AntCandyInteraction
    {
        /// <summary>Returns true when a segment may start carrying a candy.</summary>
        public static bool CanAttach(
            bool candyPresent,
            bool segmentInteracting,
            bool segmentCanInteract,
            bool candyWaitingForFly,
            bool isLastSegment,
            bool candyInsideBounds,
            bool candyHeldByHand)
        {
            return candyPresent
                && !segmentInteracting
                && segmentCanInteract
                && !candyWaitingForFly
                && !isLastSegment
                && candyInsideBounds
                && !candyHeldByHand;
        }

        /// <summary>Returns true when a carried candy has left its carrier after the snap grace period.</summary>
        public static bool ShouldDetach(
            bool candyCarriedBySegment,
            bool segmentInteracting,
            float interactionTime,
            bool candyInsideInternalBounds)
        {
            return candyCarriedBySegment
                && segmentInteracting
                && interactionTime > AntConveyorLogic.CarrierSnapTimeThreshold
                && !candyInsideInternalBounds;
        }

        /// <summary>Returns true when a detached candy should receive the slow-stop impulse.</summary>
        public static bool ShouldSlowStopAfterDetach(bool otherSegmentContainsCandyExternally)
        {
            return !otherSegmentContainsCandyExternally;
        }
    }
}
