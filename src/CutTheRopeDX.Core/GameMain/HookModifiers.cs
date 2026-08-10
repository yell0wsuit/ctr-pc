using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Independent per-hook traits. Unlike the axes, these do not exclude one another and carry no
    /// behaviour of their own, so they stay plain flags rather than becoming objects.
    /// </summary>
    [Flags]
    internal enum HookModifiers
    {
        /// <summary>No modifiers.</summary>
        None = 0,

        /// <summary>The hook is not drawn.</summary>
        Invisible = 1,

        /// <summary>
        /// The hook is a chain anchor: it renders with the chain sprites and any rope it creates is
        /// axe-only. Distinct from <see cref="Bungee.cutOnlyByAxe"/>, which is a property of a rope -
        /// the candy connector is an axe-only rope with no hook at all.
        /// </summary>
        ChainAnchor = 2,
    }
}
