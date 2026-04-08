using System;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Provides helper properties to determine whether seasonal events
    /// should be active based on the current system date.
    /// </summary>
    internal static class SpecialEvents
    {
        #region Christmas event

        /// <summary>
        /// Gets a value indicating whether the current month is January.
        /// </summary>
        public static bool IsJanuary => DateTime.Now.Month == 1;

        /// <summary>
        /// Gets a value indicating whether the Christmas event period is active.
        /// Includes December and January.
        /// </summary>
        public static bool IsXmas => DateTime.Now.Month is 12 or 1;

        #endregion
    }
}
