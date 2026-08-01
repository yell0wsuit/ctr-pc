namespace CutTheRopeDX.GameMain
{
    /// <summary>Lifecycle topology of one logical candy.</summary>
    internal enum CandyPresence
    {
        /// <summary>The whole body is active in the play field.</summary>
        Present,

        /// <summary>The whole body is temporarily unavailable during transport.</summary>
        Hidden,

        /// <summary>The logical candy was permanently removed for a recorded reason.</summary>
        Removed,

        /// <summary>The whole body is replaced by its owned split halves.</summary>
        Split,
    }

    /// <summary>Reason a whole candy or split half was permanently removed.</summary>
    internal enum CandyRemovalReason
    {
        /// <summary>The whole candy was successfully eaten by a target.</summary>
        Eaten,

        /// <summary>The body was destroyed by a gameplay hazard.</summary>
        Hazard,

        /// <summary>The body was stolen by a spider.</summary>
        Spider,

        /// <summary>The body left the playable area.</summary>
        OffScreen,
    }
}
