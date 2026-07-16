namespace CutTheRopeDX.Content.Assets
{
    /// <summary>
    /// Downloads the binary asset archive with bounded retries.
    /// </summary>
    public sealed class AssetDownloader : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;

        /// <summary>
        /// Initializes a downloader with the default HTTP client.
        /// </summary>
        public AssetDownloader()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "CutTheRopeDX.Content/1.0");
            _ownsHttpClient = true;
        }

        /// <summary>
        /// Initializes a downloader with a caller-provided HTTP client.
        /// </summary>
        /// <param name="httpClient">HTTP client used for requests.</param>
        public AssetDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Downloads an archive to disk.
        /// </summary>
        /// <param name="url">Archive URL.</param>
        /// <param name="destinationPath">Destination file path.</param>
        /// <param name="retries">Maximum request attempts.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task DownloadAsync(
            string url,
            string destinationPath,
            int retries,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(retries, 1);

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync(
                        url,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                    _ = response.EnsureSuccessStatusCode();
                    await using Stream source =
                        await response.Content.ReadAsStreamAsync(cancellationToken);
                    await using FileStream destination = File.Create(destinationPath);
                    await source.CopyToAsync(destination, cancellationToken);
                    return;
                }
                catch (Exception exception) when (
                    attempt < retries &&
                    exception is HttpRequestException or TaskCanceledException or IOException)
                {
                    Console.Error.WriteLine(
                        $"Download attempt {attempt} failed ({exception.Message}); retrying...");
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient.Dispose();
            }
        }
    }
}
