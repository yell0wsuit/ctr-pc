using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>Preference storage backed by JSON files in the save directory.</summary>
    /// <param name="directory">Absolute path to the save directory.</param>
    internal sealed class FilePreferenceStore(string directory) : IPreferenceStore
    {
        private const string SlotPrefix = "ctrsave_slot";
        private const string SlotExtension = ".json";

        /// <inheritdoc />
        public string Read(string name)
        {
            string path = Path.Combine(directory, name);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        /// <inheritdoc />
        public void Write(string name, string contents)
        {
            _ = Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, name), contents);
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateBoxSlots()
        {
            return Directory.Exists(directory)
                ? [.. Directory
                    .EnumerateFiles(directory, $"{SlotPrefix}*{SlotExtension}")
                    .Select(Path.GetFileName)]
                : [];
        }
    }
}
