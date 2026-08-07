namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook carried along an authored path by its own mover - the bee. Its path is its own, so no
    /// platform may capture it.
    /// </summary>
    internal sealed class PathMotion : AnchorMotion
    {
        /// <inheritdoc />
        public override bool CanBind => false;
    }
}
