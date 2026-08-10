namespace CutTheRopeDX.GameMain
{
    /// <summary>Why a hook's rope was cut, which decides how the hook reacts.</summary>
    internal enum RopeCutReason
    {
        /// <summary>The rope was severed along its length by a cut trail, razor or axe.</summary>
        Severed,

        /// <summary>The candy at the rope's end was released, so the rope was cut at its end.</summary>
        CandyReleased,
    }
}
