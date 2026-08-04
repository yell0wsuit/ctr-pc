namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Which way a bamboo tube's mouth faces. A tube only swallows a candy travelling *into* a
    /// hole, so a scenario has to point the tube at the motion the candy actually has: falling for
    /// a plain candy, rising for one inside a bubble, sideways for one riding the ant conveyor.
    /// </summary>
    internal enum TubeMouth
    {
        /// <summary>Mouth faces up, for a candy that is falling (or held still).</summary>
        CatchesFalling,

        /// <summary>Mouth faces down, for a candy being lifted by a bubble.</summary>
        CatchesRising,

        /// <summary>Mouth faces left, for a candy travelling to the right.</summary>
        CatchesRightward,

        /// <summary>Mouth faces right, for a candy travelling to the left.</summary>
        CatchesLeftward,
    }
}
