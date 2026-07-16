using System.Net;

using CutTheRopeDX.Content.Assets;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class AssetDownloaderTests
    {
        [Fact]
        public async Task DownloadAsyncStreamsResponseToDestination()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            using HttpClient httpClient = new(
                new SequenceMessageHandler(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("archive"),
                    }));
            AssetDownloader downloader = new(httpClient);
            string destination = Path.Combine(temporaryDirectory.Path, "assets.zip");

            await downloader.DownloadAsync("https://example.test/assets.zip", destination, 3, TestContext.Current.CancellationToken);

            Assert.Equal("archive", File.ReadAllText(destination));
        }

        [Fact]
        public async Task DownloadAsyncRetriesTransientFailures()
        {
            using TestTemporaryDirectory temporaryDirectory = new();
            SequenceMessageHandler handler = new(
                new HttpResponseMessage(HttpStatusCode.InternalServerError),
                new HttpResponseMessage(HttpStatusCode.BadGateway),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("archive"),
                });
            using HttpClient httpClient = new(handler);
            AssetDownloader downloader = new(httpClient);
            string destination = Path.Combine(temporaryDirectory.Path, "assets.zip");

            await downloader.DownloadAsync("https://example.test/assets.zip", destination, 3, TestContext.Current.CancellationToken);

            Assert.Equal(3, handler.RequestCount);
            Assert.Equal("archive", File.ReadAllText(destination));
        }

        private sealed class SequenceMessageHandler(
            params HttpResponseMessage[] responses) : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses = new(responses);

            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                RequestCount++;
                return Task.FromResult(_responses.Dequeue());
            }
        }
    }
}
