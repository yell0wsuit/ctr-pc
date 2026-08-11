namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Selects how a rocket binds: a held candy (hand or mouse, judged per candy) gets the
    /// zero-rest-length direct-FLY bind so the rocket coexists with the holder and launches when
    /// released; a free candy gets the classic DIST reel-in.
    /// </summary>
    internal static class RocketBindPath
    {
        public static bool UsesDirectFlyPath(bool handHoldsThisCandy, bool mouseCarriesThisCandy)
        {
            return handHoldsThisCandy || mouseCarriesThisCandy;
        }
    }
}
