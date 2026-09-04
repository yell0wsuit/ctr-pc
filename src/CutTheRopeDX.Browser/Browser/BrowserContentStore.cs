using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Content store backed by an upfront HTTP preload and an in-memory cache.
    /// </summary>
    /// <remarks>
    /// The host loads tier-0 metadata and the complete generated asset catalog before Core
    /// starts, so every later read is synchronous and no per-pack browser loading exists.
    /// </remarks>
    /// <param name="contentBaseUrl">Root URL of the content tree, with a trailing slash.</param>
    internal sealed class BrowserContentStore(string contentBaseUrl) : IContentStore
    {
        private const int PreloadConcurrency = 8;

        private readonly ContentCache _cache = new();

        private static string Normalize(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');
            return normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ? normalized[..^4] + BrowserAssetPlatform.ImageExtension
                : normalized;
        }

        /// <inheritdoc />
        public byte[] Read(string relativePath)
        {
            return _cache.Read(Normalize(relativePath));
        }

        /// <summary>
        /// Loads the tier-0 bundle, populating the cache with every metadata file it holds.
        /// </summary>
        /// <param name="url">URL of the bundle JSON.</param>
        public async Task LoadTier0Async(string url)
        {
            FetchInterop.ReportContentProgress("metadata", 0, 1);
            string json = await FetchInterop.FetchText(url)
                ?? throw new InvalidOperationException(
                    $"Could not load the tier-0 bundle from {url}. "
                    + "Run scripts/build_web_content.py first.");

            using JsonDocument document = JsonDocument.Parse(json);
            foreach (JsonProperty entry in document.RootElement.EnumerateObject())
            {
                _cache.Set(entry.Name, Encoding.UTF8.GetBytes(entry.Value.GetString()));
            }
            FetchInterop.ReportContentProgress("metadata", 1, 1);
        }

        /// <summary>Loads every generated binary asset before the game is allowed to start.</summary>
        /// <param name="catalogUrl">URL of the generated asset catalog.</param>
        /// <param name="audio">Audio backend that owns decoded sound assets.</param>
        public async Task LoadAllAssetsAsync(string catalogUrl, WebAudioBackend audio)
        {
            string json = await FetchInterop.FetchText(catalogUrl)
                ?? throw new InvalidOperationException(
                    $"Could not load the asset catalog from {catalogUrl}. "
                    + "Run scripts/build_web_content.py first.");

            using JsonDocument document = JsonDocument.Parse(json);
            foreach (JsonProperty category in document.RootElement.EnumerateObject())
            {
                string[] paths = [.. category.Value
                    .EnumerateArray()
                    .Select(element => Normalize(element.GetString()))
                    .Distinct(StringComparer.Ordinal)];
                if (string.Equals(category.Name, "sounds", StringComparison.Ordinal))
                {
                    await PumpAsync(
                        category.Name, paths, paths.Length, 0,
                        path => PreloadSoundAsync(audio, path));
                    continue;
                }

                string[] missing = [.. paths.Where(path => !_cache.ContainsKey(path))];
                await PumpAsync(
                    category.Name, missing, paths.Length, paths.Length - missing.Length,
                    FetchAssetAsync);
            }

            // Nothing writes the cache after this point, so sealing it turns a timing
            // assumption into a structural one: the worker threads that scan level XML
            // read a collection that can no longer change underneath them.
            _cache.Freeze();
        }

        /// <summary>
        /// Loads a list of assets, keeping a fixed number of requests in flight until the list
        /// runs out.
        /// </summary>
        /// <remarks>
        /// Loading in fixed batches instead makes every request in a batch wait for the slowest
        /// one before the next batch starts, so the connection goes idle at each boundary. Pulling
        /// from a shared cursor keeps it saturated for the whole preload, which is most of what a
        /// player waits through on a first visit.
        /// </remarks>
        /// <param name="category">Asset category, for progress reporting.</param>
        /// <param name="work">The paths still to load.</param>
        /// <param name="total">Total assets in the category, including any already loaded.</param>
        /// <param name="done">How many of that total were already loaded.</param>
        /// <param name="load">Loads one path, throwing if it cannot.</param>
        private static Task PumpAsync(
            string category, string[] work, int total, int done, Func<string, Task> load)
        {
            FetchInterop.ReportContentProgress(category, done, total);
            return ParallelPump.RunAsync(
                work,
                PreloadConcurrency,
                load,
                completed => FetchInterop.ReportContentProgress(
                    category, done + completed, total));
        }

        private async Task FetchAssetAsync(string path)
        {
            byte[] bytes = await FetchInterop.GetBytesAsync(contentBaseUrl + path);
            _cache.Set(
                path,
                bytes.Length != 0
                    ? bytes
                    : throw new InvalidOperationException(
                        $"Could not preload content '{path}'."));
        }

        private static async Task PreloadSoundAsync(WebAudioBackend audio, string path)
        {
            if (await audio.PreloadFileAsync(path) == 0)
            {
                throw new InvalidOperationException($"Could not preload audio content '{path}'.");
            }
        }
    }
}
