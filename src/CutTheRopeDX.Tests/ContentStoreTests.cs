using System;
using System.IO;
using System.Text;

using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ContentStoreTests : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(), "ctrdx-store-" + Guid.NewGuid().ToString("N"));

        public ContentStoreTests()
        {
            _ = Directory.CreateDirectory(Path.Combine(_root, "maps"));
            File.WriteAllText(Path.Combine(_root, "maps", "1_1.xml"), "<map />");
        }

        public void Dispose()
        {
            Directory.Delete(_root, recursive: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ReadReturnsFileBytes()
        {
            FileContentStore store = new(_root);
            Assert.Equal("<map />", Encoding.UTF8.GetString(store.Read("maps/1_1.xml")));
        }

        [Fact]
        public void ReadAcceptsBackslashSeparators()
        {
            FileContentStore store = new(_root);
            Assert.Equal("<map />", Encoding.UTF8.GetString(store.Read("maps\\1_1.xml")));
        }

        [Fact]
        public void ReadThrowsForMissingContent()
        {
            FileContentStore store = new(_root);
            _ = Assert.Throws<FileNotFoundException>(() => store.Read("maps/nope.xml"));
        }

    }
}
