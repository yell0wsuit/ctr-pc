using System.Collections.Generic;

using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// One rope in the level and the hook that owns it, if any. A null <see cref="Owner"/> means the
    /// candy connector, which joins two candy points and has no hook.
    /// </summary>
    /// <param name="rope">The rope.</param>
    /// <param name="owner">The owning hook, or <see langword="null"/> for the candy connector.</param>
    internal readonly struct RopeEntry(Bungee rope, Grab owner)
    {
        /// <summary>Gets the rope.</summary>
        public Bungee Rope { get; } = rope;

        /// <summary>Gets the owning hook, or <see langword="null"/> for the candy connector.</summary>
        public Grab Owner { get; } = owner;

        /// <summary>Gets whether this entry is the candy connector.</summary>
        public bool IsConnector => Owner == null;

        /// <summary>
        /// Gets the segment index to cut when a candy is released, or <see langword="null"/> when
        /// this rope does not end on that candy. A hook's rope only ever ends on a candy at its tail;
        /// the connector joins a candy at each end, so releasing either one cuts the matching end.
        /// </summary>
        /// <param name="candyPoint">The released candy's physics point.</param>
        /// <returns>The segment index to cut, or <see langword="null"/>.</returns>
        public int? CutPartForCandy(ConstraintedPoint candyPoint)
        {
            return Rope.tail == candyPoint ? Rope.parts.Count - 2 : IsConnector && Rope.bungeeAnchor == candyPoint ? 0 : null;
        }
    }

    /// <summary>
    /// An index over every rope in the level - hook ropes and the candy connector alike - so a
    /// rope-wide sweep is written once instead of once per collection.
    /// <para>
    /// This is an index, not an owner: a <see cref="Grab"/> still owns its rope through its
    /// <see cref="RopeAttachment"/> and the scene still owns the connector. Registering does not
    /// transfer lifetime.
    /// </para>
    /// </summary>
    internal sealed class RopeRegistry
    {
        private readonly List<RopeEntry> entries = [];

        /// <summary>Gets every registered rope.</summary>
        public IReadOnlyList<RopeEntry> All => entries;

        /// <summary>Registers a hook's rope.</summary>
        /// <param name="rope">The rope.</param>
        /// <param name="owner">The hook that owns it.</param>
        public void Register(Bungee rope, Grab owner)
        {
            if (rope != null)
            {
                entries.Add(new RopeEntry(rope, owner));
            }
        }

        /// <summary>Registers the candy connector, which has no owning hook.</summary>
        /// <param name="rope">The connector rope.</param>
        public void RegisterConnector(Bungee rope)
        {
            if (rope != null)
            {
                entries.Add(new RopeEntry(rope, null));
            }
        }

        /// <summary>Removes a rope from the index.</summary>
        /// <param name="rope">The rope to remove.</param>
        public void Unregister(Bungee rope)
        {
            _ = entries.RemoveAll(entry => entry.Rope == rope);
        }
    }
}
