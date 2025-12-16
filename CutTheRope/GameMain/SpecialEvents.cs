using System;

namespace CutTheRope.GameMain
{
    internal static class SpecialEvents
    {
        // Christmas event
        public static bool IsJanuary => DateTime.Now.Month == 1;

        public static bool IsXmas => DateTime.Now.Month is 12 or 1;
    }
}
