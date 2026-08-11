using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Thin managed wrapper over the fetch.js byte-loading module.</summary>
    internal static partial class FetchInterop
    {
        /// <summary>Imports fetch.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("fetch", "../fetch.js");
        }

        /// <summary>Fetches a URL into the JS-side stash, returning its length or -1 on failure.</summary>
        [JSImport("fetchBytes", "fetch")]
        public static partial Task<int> FetchBytes(string url);

        /// <summary>Copies the stashed bytes for a URL into a managed buffer, releasing the stash.</summary>
        [JSImport("takeStashed", "fetch")]
        public static partial int TakeStashed(
            string url, [JSMarshalAs<JSType.MemoryView>] Span<byte> destination);

        /// <summary>Fetches a URL as text, or null when the request fails.</summary>
        [JSImport("fetchText", "fetch")]
        public static partial Task<string> FetchText(string url);

        /// <summary>Updates the browser splash with content download progress.</summary>
        /// <param name="type">Asset category currently loading.</param>
        /// <param name="loaded">Number of assets loaded in the category.</param>
        /// <param name="total">Total number of assets in the category.</param>
        [JSImport("reportContentProgress", "fetch")]
        public static partial void ReportContentProgress(string type, int loaded, int total);

        /// <summary>Fetches a URL as bytes, returning an empty array on failure.</summary>
        /// <param name="url">Absolute or root-relative URL.</param>
        public static async Task<byte[]> GetBytesAsync(string url)
        {
            int length = await FetchBytes(url);
            if (length <= 0)
            {
                return [];
            }
            byte[] buffer = new byte[length];
            _ = TakeStashed(url, buffer);
            return buffer;
        }
    }
}
