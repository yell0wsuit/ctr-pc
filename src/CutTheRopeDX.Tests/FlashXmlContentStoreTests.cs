using System.Text;

using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class FlashXmlContentStoreTests
    {
        private sealed class AnimationStore : IContentStore
        {
            public string ReadPath { get; private set; }

            public byte[] Read(string relativePath)
            {
                ReadPath = relativePath;
                return Encoding.UTF8.GetBytes(
                    "<FlashAnimation width='10' height='20' src='zeptolab_logo_anim' />");
            }

        }

        [Fact]
        public void ParseFileReadsAnimationFromContentStore()
        {
            IContentStore previous = PlatformServices.Content;
            AnimationStore store = new();
            PlatformServices.Content = store;
            try
            {
                FlashXmlAnimationDefinition definition = FlashXmlImporter.ParseFile(
                    "/content/images/animations/browser_stream_test.xml");

                Assert.Equal("images/animations/browser_stream_test.xml", store.ReadPath);
                Assert.Equal(10f, definition.StageWidth);
                Assert.Equal(20f, definition.StageHeight);
            }
            finally
            {
                PlatformServices.Content = previous;
            }
        }
    }
}
