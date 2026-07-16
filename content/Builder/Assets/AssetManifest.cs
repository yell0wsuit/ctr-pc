using System.Text.Json;

namespace CutTheRopeDX.Content.Assets
{
    /// <summary>
    /// Describes the binary content assets expected in the source tree.
    /// </summary>
    public sealed class AssetManifest
    {
        private AssetManifest(Dictionary<string, string> files)
        {
            Files = files;
        }

        /// <summary>
        /// Gets the expected SHA-256 hash for each POSIX-relative asset path.
        /// </summary>
        public IReadOnlyDictionary<string, string> Files { get; }

        /// <summary>
        /// Reads an asset manifest from disk.
        /// </summary>
        /// <param name="path">Path to the JSON manifest.</param>
        /// <returns>The parsed manifest.</returns>
        public static AssetManifest Read(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using JsonDocument document = JsonDocument.Parse(stream);
            Dictionary<string, string> files = [];

            if (document.RootElement.TryGetProperty("files", out JsonElement fileEntries))
            {
                foreach (JsonProperty entry in fileEntries.EnumerateObject())
                {
                    files[entry.Name] = entry.Value.GetString() ?? string.Empty;
                }
            }

            return new AssetManifest(files);
        }
    }
}
