using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Browser
{
    /// <summary>Preference storage backed by <c>localStorage</c>.</summary>
    /// <remarks>
    /// Blob names keep their desktop file names and gain a namespace prefix, so a save
    /// exported from one host stays recognizable in the other. localStorage is
    /// synchronous, so <see cref="Preferences"/> needs no changes to use it.
    /// </remarks>
    internal sealed class LocalStoragePreferenceStore : IPreferenceStore
    {
        private const string Prefix = "ctrdx:";
        private const string SlotPrefix = Prefix + "ctrsave_slot";

        /// <inheritdoc />
        public string Read(string name)
        {
            return StorageInterop.Read(Prefix + name);
        }

        /// <inheritdoc />
        public void Write(string name, string contents)
        {
            StorageInterop.Write(Prefix + name, contents);
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateBoxSlots()
        {
            return [.. StorageInterop
                .KeysWithPrefix(SlotPrefix)
                .Select(key => key[Prefix.Length..])];
        }
    }
}
