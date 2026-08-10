using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides whether and how a hook produces a rope. Exactly one source per hook; the three
    /// implementations are mutually exclusive by construction, which is what removes the loader's
    /// hand-written gun/radius exclusivity patches.
    /// </summary>
    internal abstract class RopeSource
    {
        /// <summary>Gets whether this source can still produce a rope.</summary>
        public abstract bool CanAttach { get; }

        /// <summary>Advances any per-frame state the source owns.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        public virtual void Update(float delta)
        {
        }

        /// <summary>Notifies the source that its hook moved.</summary>
        /// <param name="position">The hook's new world position.</param>
        public virtual void OnAnchorMoved(Vector position)
        {
        }

        /// <summary>Reacts to the hook's rope being cut.</summary>
        /// <param name="reason">Why the rope was cut.</param>
        public virtual void OnRopeCut(RopeCutReason reason)
        {
        }
    }

    /// <summary>
    /// A hook whose rope is authored in the level file. It is created already attached and can never
    /// produce another, so this source owns no state at all.
    /// </summary>
    internal sealed class PreAttachedSource : RopeSource
    {
        /// <inheritdoc />
        public override bool CanAttach => false;
    }
}
