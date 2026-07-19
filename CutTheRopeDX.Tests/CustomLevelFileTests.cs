using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CustomLevelFileTests
    {
        [Fact]
        public void TryLoad_ValidXml_ReturnsRootElement()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");
            File.WriteAllText(path, "<level><object name=\"candy\" /></level>");
            try
            {
                bool loaded = CustomLevelFile.TryLoad(path, out XElement map, out string error);

                Assert.True(loaded);
                Assert.Null(error);
                Assert.Equal("level", map.Name.LocalName);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void TryLoad_MissingFile_ReturnsErrorMentioningPath()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");

            bool loaded = CustomLevelFile.TryLoad(path, out XElement map, out string error);

            Assert.False(loaded);
            Assert.Null(map);
            Assert.Contains(path, error);
        }

        [Fact]
        public void TryLoad_MalformedXml_ReturnsError()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");
            File.WriteAllText(path, "<level><unclosed>");
            try
            {
                bool loaded = CustomLevelFile.TryLoad(path, out XElement map, out string error);

                Assert.False(loaded);
                Assert.Null(map);
                Assert.NotNull(error);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void TryLoad_EmptyPath_ReturnsError()
        {
            bool loaded = CustomLevelFile.TryLoad("  ", out XElement map, out string error);

            Assert.False(loaded);
            Assert.Null(map);
            Assert.NotNull(error);
        }
    }
}
