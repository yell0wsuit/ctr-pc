using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>The mutually exclusive forms a ghost can present.</summary>
    [Flags]
    internal enum GhostForm
    {
        None = 0,
        Idle = 1,
        Bubble = 2,
        Grab = 4,
        Bouncer = 8
    }

    /// <summary>Identifies the forms on the two sides of the current morph.</summary>
    internal readonly record struct GhostMorphPhase(GhostForm Outgoing, GhostForm Incoming);

    /// <summary>Owns an outgoing apparition until its disappearance timeline has finished.</summary>
    internal sealed record RetiringGhostApparition(
        GhostForm Outgoing,
        GhostForm Incoming,
        IGhostApparition Apparition);
}
