using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Auto-detects the two-Om-Nom "chat" greeting that plays once at level start, where a
    /// pair of Om Noms turn their heads toward each other. Enabled purely by geometry (no XML
    /// flag): the pair must sit roughly in a line on one axis. Direction follows position.
    /// </summary>
    internal static class ChatGreeting
    {
        /// <summary>
        /// How much larger the separation on one axis must be than the other for the pair to
        /// count as "in a line" on that axis. Pairs that are diagonal (both axes comparable)
        /// or coincident (both axes near zero) do not greet.
        /// </summary>
        private const float AxisDominanceFactor = 2.5f;

        /// <summary>
        /// Resolves the directional greet each Om Nom plays so the pair face each other, or
        /// <see langword="null"/> when their layout should not trigger a greeting.
        /// </summary>
        /// <param name="firstX">World X position of the first target.</param>
        /// <param name="firstY">World Y position of the first target.</param>
        /// <param name="secondX">World X position of the second target.</param>
        /// <param name="secondY">World Y position of the second target.</param>
        /// <returns>The greet states for the first and second targets, or <see langword="null"/> when they should not greet.</returns>
        public static (TargetAnimationState first, TargetAnimationState second)? ResolveStates(
            float firstX, float firstY, float secondX, float secondY)
        {
            float dx = Math.Abs(firstX - secondX);
            float dy = Math.Abs(firstY - secondY);

            bool inRow = dx >= AxisDominanceFactor * dy;     // aligned vertically, separated in X
            bool inColumn = dy >= AxisDominanceFactor * dx;  // aligned horizontally, separated in Y

            // Greet only when exactly one axis dominates. Both true means the pair is coincident
            // (dx == dy == 0); neither means they sit diagonally and cannot face each other.
            if (inRow == inColumn)
            {
                return null;
            }

            if (inRow)
            {
                // Side by side: each turns toward the other. Screen X grows rightward.
                return firstX <= secondX
                    ? (TargetAnimationState.GreetRight, TargetAnimationState.GreetLeft)
                    : (TargetAnimationState.GreetLeft, TargetAnimationState.GreetRight);
            }

            // Stacked: each turns toward the other. Screen Y grows downward.
            return firstY <= secondY
                ? (TargetAnimationState.GreetDown, TargetAnimationState.GreetUp)
                : (TargetAnimationState.GreetUp, TargetAnimationState.GreetDown);
        }
    }
}
