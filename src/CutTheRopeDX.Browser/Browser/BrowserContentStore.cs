using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Content store backed by HTTP fetch, with an in-memory cache of everything that has
    /// arrived.
    /// </summary>
    /// <remarks>
    /// The tier-0 bundle is a single request covering every metadata file, so after boot
    /// essentially all non-binary reads are served from memory. Binary assets arrive per
    /// pack through <see cref="EnsureResidentAsync"/>.
    /// </remarks>
    /// <param name="contentBaseUrl">Root URL of the content tree, with a trailing slash.</param>
    internal sealed class BrowserContentStore(string contentBaseUrl) : IContentStore
    {
        private readonly Dictionary<string, byte[]> _cache = [];
        private readonly ResidencyTracker _tracker = new();

        /// <summary>Number of requested files that have not arrived yet.</summary>
        public int PendingCount => _tracker.PendingCount;

        private static string Normalize(string relativePath)
        {
            return relativePath.Replace('\\', '/');
        }

        /// <inheritdoc />
        public bool IsResident(string relativePath)
        {
            return _cache.ContainsKey(Normalize(relativePath));
        }

        /// <inheritdoc />
        public byte[] Read(string relativePath)
        {
            string key = Normalize(relativePath);
            return _cache.TryGetValue(key, out byte[] bytes)
                ? bytes
                : throw new InvalidOperationException(
                    $"Content '{key}' was read before it was resident. "
                    + "Add it to a residency tier or prefetch its pack first.");
        }

        /// <inheritdoc />
        public async Task EnsureResidentAsync(IEnumerable<string> relativePaths)
        {
            _tracker.Request(relativePaths.Select(Normalize));

            IReadOnlyCollection<string> batch = _tracker.TakePending();
            if (batch.Count == 0)
            {
                return;
            }

            foreach (string path in batch)
            {
                byte[] bytes = await FetchInterop.GetBytesAsync(contentBaseUrl + path);
                if (bytes.Length == 0)
                {
                    continue;
                }
                _cache[path] = bytes;
                _ = _tracker.MarkResident(path);
            }
        }

        /// <summary>
        /// Loads the tier-0 bundle, populating the cache with every metadata file it holds.
        /// </summary>
        /// <param name="url">URL of the bundle JSON.</param>
        public async Task LoadTier0Async(string url)
        {
            string json = await FetchInterop.FetchText(url)
                ?? throw new InvalidOperationException(
                    $"Could not load the tier-0 bundle from {url}. "
                    + "Run scripts/build_web_content.py first.");

            using JsonDocument document = JsonDocument.Parse(json);
            foreach (JsonProperty entry in document.RootElement.EnumerateObject())
            {
                _cache[entry.Name] = Encoding.UTF8.GetBytes(entry.Value.GetString());
                _ = _tracker.MarkResident(entry.Name);
            }
        }
    }
}
