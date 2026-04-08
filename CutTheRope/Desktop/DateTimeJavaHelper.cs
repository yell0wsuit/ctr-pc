using System;

namespace CutTheRope.Desktop
{
    /// <summary>
    /// Provides Java-style time helpers for desktop code paths.
    /// </summary>
    /// <remarks>
    /// TODO: replace with <c>DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()</c>
    /// </remarks>
    public static class DateTimeJavaHelper
    {
        /// <summary>
        /// Returns the current UTC time as Unix time in milliseconds.
        /// </summary>
        /// <returns>The current UTC time in milliseconds since January 1, 1970.</returns>
        public static long CurrentTimeMillis()
        {
            // TODO: replace with `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()`
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        /// <summary>
        /// The Unix epoch used by <see cref="CurrentTimeMillis"/>.
        /// </summary>
        private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
