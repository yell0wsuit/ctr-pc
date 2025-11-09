using System;

namespace CutTheRope.windows
{
    // Token: 0x0200000A RID: 10
    public static class DateTimeJavaHelper
    {
        // Token: 0x06000048 RID: 72 RVA: 0x00003684 File Offset: 0x00001884
        public static long currentTimeMillis()
        {
            return (long)(DateTime.UtcNow - DateTimeJavaHelper.Jan1st1970).TotalMilliseconds;
        }

        // Token: 0x0400003C RID: 60
        private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
