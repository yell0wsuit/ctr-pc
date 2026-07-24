namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Platforms (conveyor belt, DJ disc) never move a grab that has its own movement. Static
    /// exclusion at bind time for autonomous movers (bee, launcher) and player drag rails;
    /// dynamic per-frame exclusion for a kicked (detached, free-falling) suction cup, which
    /// resumes following when it re-sticks.
    /// </summary>
    internal static class GrabPlatformBind
    {
        /// <summary>Static: may this grab be bound/captured by a platform at all?</summary>
        public static bool CanBind(bool hasOwnMover, bool isMoveableRail)
        {
            return !hasOwnMover && !isMoveableRail;
        }

        /// <summary>Dynamic: should the platform drive this grab this frame?</summary>
        public static bool FollowsPlatform(bool canBind, bool isKickedFree)
        {
            return canBind && !isKickedFree;
        }
    }
}
