namespace CutTheRope.Commons
{
    /// <summary>
    /// Provides remote configuration values for promotional and social network visibility settings.
    /// </summary>
    /// <remarks>Currently no-op, but it can be implemented in the future.</remarks>
    internal sealed class RemoteDataManager
    {
        /// <summary>
        /// Returns the box index to use for cross-promotion display.
        /// </summary>
        /// <returns>The selected cross-promo box index.</returns>
        public static int GetBoxForCrossPromo()
        {
            return 0;
        }

        /// <summary>
        /// Returns whether social network buttons should be hidden.
        /// </summary>
        /// <returns><see langword="true"/> when social network buttons should be hidden; otherwise <see langword="false"/>.</returns>
        public static bool GetHideSocialNetworks()
        {
            return false;
        }

        /// <summary>
        /// Returns whether the main promotional banner should be hidden.
        /// </summary>
        /// <returns><see langword="true"/> when the main promo banner should be hidden; otherwise <see langword="false"/>.</returns>
        public static bool GetHideMainPromo()
        {
            return false;
        }
    }
}
