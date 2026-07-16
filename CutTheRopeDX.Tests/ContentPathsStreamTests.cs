using System;
using System.IO;
using System.Text;

using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class ContentPathsStreamTests
    {
        [Fact]
        public void OpenStream_PreservesRawFileBytes()
        {
            string relativePath = Path.Combine("test-data", $"{Guid.NewGuid():N}.json");
            string fullPath = Path.Combine(ContentPaths.GetContentRootAbsolute(), relativePath);
            byte[] expected = Encoding.UTF8.GetBytes(/*lang=json,strict*/ "[{\"name\":\"desktopvk\"}]");

            _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, expected);

            try
            {
                using Stream stream = ContentPaths.OpenStream(relativePath);
                using MemoryStream copy = new();
                stream.CopyTo(copy);

                Assert.Equal(expected, copy.ToArray());
            }
            finally
            {
                File.Delete(fullPath);
            }
        }
    }
}
