using System;

namespace CutTheRopeDX.Desktop
{
    internal sealed class BackingScaleState(double initialScale, double epsilon = 0.01d, int downscaleToOneConfirmationReadings = 1)
    {
        private readonly double _epsilon = epsilon > 0d ? epsilon : 0.01d;
        private readonly int _downscaleToOneConfirmationReadings = downscaleToOneConfirmationReadings > 0 ? downscaleToOneConfirmationReadings : 1;
        private int _pendingDownscaleToOneReadings;

        public double CurrentScale { get; private set; } = BackingScaleMath.NormalizeScale(initialScale);

        public bool TryUpdate(double candidateScale)
        {
            double normalizedCandidate = BackingScaleMath.NormalizeScale(candidateScale);
            if (Math.Abs(CurrentScale - normalizedCandidate) < _epsilon)
            {
                _pendingDownscaleToOneReadings = 0;
                return false;
            }

            if (ShouldConfirmDownscaleToOne(CurrentScale, normalizedCandidate))
            {
                _pendingDownscaleToOneReadings++;
                if (_pendingDownscaleToOneReadings < _downscaleToOneConfirmationReadings)
                {
                    return false;
                }
            }
            else
            {
                _pendingDownscaleToOneReadings = 0;
            }

            CurrentScale = normalizedCandidate;
            _pendingDownscaleToOneReadings = 0;
            return true;
        }

        private bool ShouldConfirmDownscaleToOne(double currentScale, double candidateScale)
        {
            return _downscaleToOneConfirmationReadings > 1 && currentScale > (1d + _epsilon) && Math.Abs(candidateScale - 1d) <= _epsilon;
        }
    }
}
