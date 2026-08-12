using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Named-blob persistence behind <see cref="Preferences"/>.
    /// </summary>
    /// <remarks>
    /// Deliberately synchronous. Preferences are read and written from ordinary game code
    /// with no await points available, and every target can satisfy that: a file on
    /// desktop, <c>localStorage</c> in a browser.
    /// </remarks>
    internal interface IPreferenceStore
    {
        /// <summary>Reads a stored blob, or <see langword="null"/> when it is absent.</summary>
        /// <param name="name">Blob name, e.g. <c>ctr_preferences.json</c>.</param>
        string Read(string name);

        /// <summary>Writes a blob, replacing any existing value.</summary>
        /// <param name="name">Blob name.</param>
        /// <param name="contents">Serialized contents.</param>
        void Write(string name, string contents);

        /// <summary>Returns the names of every stored per-box save slot.</summary>
        IEnumerable<string> EnumerateBoxSlots();
    }
}
