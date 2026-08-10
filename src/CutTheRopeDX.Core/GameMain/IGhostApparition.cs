using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Marks a shape conjured by a <see cref="Ghost"/> at the ghost's own position - the bubble, rope
    /// hook, or bouncer it currently wears.
    /// </summary>
    /// <remarks>
    /// Platforms (DJ disc, conveyor belt) never carry an apparition. The morph clouds drawn around one
    /// are separate elements fixed at the spot the ghost occupies, so moving the apparition on its own
    /// tears it away from its own decoration and leaves the clouds hanging in mid-air. The apparition
    /// is scenery belonging to the ghost rather than cargo, so platforms leave it where it is.
    /// </remarks>
    internal interface IGhostApparition
    {
        /// <summary>Visual element whose morph timeline controls this apparition's lifetime.</summary>
        BaseElement Element { get; }
    }
}
