using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Keyed byte store for preloaded content, sealed once loading is complete.
    /// </summary>
    /// <remarks>
    /// Loading writes this from the owner thread; afterwards worker threads read it while
    /// scanning level XML. Sealing it makes that safe by construction rather than by
    /// timing: once frozen there is no writer left to race, and a late write fails loudly
    /// instead of corrupting a dictionary that readers are walking.
    /// </remarks>
    internal sealed class ContentCache
    {
        private readonly Dictionary<string, byte[]> _entries = [];

        private FrozenDictionary<string, byte[]> _frozen;

        /// <summary>Stores one entry. Only valid before <see cref="Freeze"/>.</summary>
        public void Set(string key, byte[] value)
        {
            if (_frozen is not null)
            {
                throw new InvalidOperationException(
                    $"Content '{key}' was added after the cache was sealed. Everything the "
                    + "game reads must be loaded before it starts.");
            }

            _entries[key] = value;
        }

        /// <summary>Returns whether an entry has already been loaded.</summary>
        public bool ContainsKey(string key)
        {
            return _frozen is not null
                ? _frozen.ContainsKey(key)
                : _entries.ContainsKey(key);
        }

        /// <summary>Returns one entry's bytes.</summary>
        /// <param name="key">The normalized content key.</param>
        /// <exception cref="InvalidOperationException">The key was never loaded.</exception>
        public byte[] Read(string key)
        {
            IReadOnlyDictionary<string, byte[]> entries =
                (IReadOnlyDictionary<string, byte[]>)_frozen ?? _entries;
            return entries.TryGetValue(key, out byte[] bytes)
                ? bytes
                : throw new InvalidOperationException(
                    $"Content '{key}' is absent from the upfront browser content cache. "
                    + "Regenerate content/assets.json with scripts/build_web_content.py.");
        }

        /// <summary>Seals the cache, after which it is read-only and safe to share.</summary>
        public void Freeze()
        {
            _frozen ??= _entries.ToFrozenDictionary();
        }
    }
}
