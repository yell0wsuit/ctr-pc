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

        private readonly Dictionary<string, byte[]> _cache = [];

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
            string key = Normalize(relativePath);
            return _cache.TryGetValue(key, out byte[] bytes)
                ? bytes
                : throw new InvalidOperationException(
                    $"Content '{key}' is absent from the upfront browser content cache. "
                    + "Regenerate content/assets.json with scripts/build_web_content.py.");
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
                _cache[entry.Name] = Encoding.UTF8.GetBytes(entry.Value.GetString());
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
                    await PreloadSoundsAsync(audio, category.Name, paths);
                    continue;
                }

                int loaded = paths.Count(path => _cache.ContainsKey(path));
                FetchInterop.ReportContentProgress(category.Name, loaded, paths.Length);

                string[] missing = [.. paths.Where(path => !_cache.ContainsKey(path))];
                for (int offset = 0; offset < missing.Length; offset += PreloadConcurrency)
                {
                    Task<(string Path, byte[] Bytes)>[] requests = [.. missing
                        .Skip(offset)
                        .Take(PreloadConcurrency)
                        .Select(FetchAssetAsync)];
                    (string Path, byte[] Bytes)[] results = await Task.WhenAll(requests);
                    foreach ((string path, byte[] bytes) in results)
                    {
                        if (bytes.Length == 0)
                        {
                            throw new InvalidOperationException(
                                $"Could not preload content '{path}'.");
                        }

                        _cache[path] = bytes;
                        loaded++;
                        FetchInterop.ReportContentProgress(category.Name, loaded, paths.Length);
                    }
                }
            }
        }

        private async Task<(string Path, byte[] Bytes)> FetchAssetAsync(string path)
        {
            return (path, await FetchInterop.GetBytesAsync(contentBaseUrl + path));
        }

        private static async Task PreloadSoundsAsync(
            WebAudioBackend audio, string category, string[] paths)
        {
            int loaded = 0;
            FetchInterop.ReportContentProgress(category, loaded, paths.Length);
            for (int offset = 0; offset < paths.Length; offset += PreloadConcurrency)
            {
                string[] batch = [.. paths.Skip(offset).Take(PreloadConcurrency)];
                int[] results = await Task.WhenAll(batch.Select(audio.PreloadFileAsync));
                for (int index = 0; index < results.Length; index++)
                {
                    if (results[index] == 0)
                    {
                        throw new InvalidOperationException(
                            $"Could not preload audio content '{batch[index]}'.");
                    }

                    loaded++;
                    FetchInterop.ReportContentProgress(category, loaded, paths.Length);
                }
            }
        }
    }
}
