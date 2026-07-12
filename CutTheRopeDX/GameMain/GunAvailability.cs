namespace CutTheRopeDX.GameMain
{
    /// <summary>Determines whether a gun grab is enabled and can fire at the candy.</summary>
    internal static class GunAvailability
    {
        /// <summary>Determines whether the gun should use its disabled appearance.</summary>
        /// <param name="candyInLantern">
        /// <see langword="true"/> when the target candy is captured in a lantern.
        /// </param>
        /// <returns><see langword="true"/> when the gun should appear disabled.</returns>
        public static bool IsDisabled(bool candyInLantern)
        {
            return candyInLantern;
        }

        /// <summary>Determines whether an unfired gun can create a rope to the candy.</summary>
        /// <param name="candyPresent"><see langword="true"/> when the candy is still available.</param>
        /// <param name="candyInLantern">
        /// <see langword="true"/> when the target candy is captured in a lantern.
        /// </param>
        /// <param name="gunFired"><see langword="true"/> when the gun has already fired.</param>
        /// <param name="ropeAbsent"><see langword="true"/> when the gun has no attached rope.</param>
        /// <returns><see langword="true"/> when the gun can fire.</returns>
        public static bool CanFire(
            bool candyPresent,
            bool candyInLantern,
            bool gunFired,
            bool ropeAbsent)
        {
            return candyPresent
                && !candyInLantern
                && !gunFired
                && ropeAbsent;
        }
    }
}
