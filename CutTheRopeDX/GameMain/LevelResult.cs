using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Captures the authoritative outcome of a completed level.</summary>
    /// <param name="ElapsedTime">Elapsed gameplay time when the win began.</param>
    /// <param name="StarsCollected">Number of stars collected when the win began.</param>
    /// <param name="StarBonus">Score awarded for collected stars.</param>
    /// <param name="TimeBonus">Score awarded for remaining time.</param>
    /// <param name="FinalScore">Final score after rounding the combined bonuses upward.</param>
    internal readonly record struct LevelResult(
        float ElapsedTime,
        int StarsCollected,
        int StarBonus,
        float TimeBonus,
        int FinalScore);

    /// <summary>Calculates completed-level results from live gameplay state.</summary>
    internal static class LevelResultCalculator
    {
        /// <summary>Calculates a completed-level result.</summary>
        /// <param name="elapsedTime">Elapsed gameplay time when the win began.</param>
        /// <param name="starsCollected">Number of stars collected when the win began.</param>
        /// <returns>The immutable result calculated from the supplied gameplay state.</returns>
        public static LevelResult Calculate(float elapsedTime, int starsCollected)
        {
            float timeBonus = MathF.Max(0f, 30f - elapsedTime) * 100f;
            int starBonus = 1000 * starsCollected;
            int finalScore = (int)MathF.Ceiling(timeBonus + starBonus);
            return new LevelResult(elapsedTime, starsCollected, starBonus, timeBonus, finalScore);
        }
    }
}
