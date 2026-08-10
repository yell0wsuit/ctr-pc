namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides what supplies a hook's position. Exactly one per hook. The suction cup is
    /// deliberately not on this axis - it answers a different question and can override any motion
    /// while it is detached; see <see cref="SuctionMount"/>.
    /// </summary>
    internal abstract class AnchorMotion
    {
        /// <summary>
        /// Gets whether a platform (conveyor belt, DJ disc) may capture this hook at all. Asked once,
        /// when the platform builds its bound-item list.
        /// </summary>
        public abstract bool CanBind { get; }

        /// <summary>
        /// Gets whether a platform should drive this hook this frame. Asked every frame, separately
        /// from <see cref="CanBind"/>, because a bound hook can stop following without unbinding.
        /// </summary>
        public virtual bool FollowsPlatform => CanBind;

        /// <summary>Advances the motion.</summary>
        /// <param name="grab">The hook this motion drives.</param>
        /// <param name="delta">Elapsed time in seconds.</param>
        public virtual void Update(Grab grab, float delta)
        {
        }
    }

    /// <summary>A hook that does not move on its own. Platforms are free to carry it.</summary>
    internal sealed class StaticMotion : AnchorMotion
    {
        /// <inheritdoc />
        public override bool CanBind => true;
    }
}
