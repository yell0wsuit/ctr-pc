namespace CutTheRope.GameMain
{
    /// <summary>
    /// Logical animation states exposed by the target animation controller.
    /// </summary>
    internal enum TargetAnimationState
    {
        /// <summary>Default idle loop.</summary>
        IdleLoop,

        /// <summary>First idle variation.</summary>
        IdleVariationOne,

        /// <summary>Second idle variation.</summary>
        IdleVariationTwo,

        /// <summary>Excited reaction animation.</summary>
        Excited,

        /// <summary>Mouth opening transition.</summary>
        MouthOpening,

        /// <summary>Mouth closing transition.</summary>
        MouthClosing,

        /// <summary>Chewing animation.</summary>
        Chewing,

        /// <summary>Sad reaction animation.</summary>
        Sad,

        /// <summary>Sleeping animation.</summary>
        Sleeping,

        /// <summary>Greeting animation.</summary>
        Greeting,
    }
}
