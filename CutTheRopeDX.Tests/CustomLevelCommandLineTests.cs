using System.IO;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CustomLevelCommandLineTests
    {
        [Fact]
        public void Parse_NoArguments_IsNotCustomLevel()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse([]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_UnrelatedArguments_IsNotCustomLevel()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--windowed"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelWithPath_ReturnsAbsolutePath()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--level", "/maps/test.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.True(Path.IsPathRooted(result.LevelPath));
            Assert.EndsWith("test.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_LevelWithoutValue_ReturnsError()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--level"]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelWithEmptyValue_ReturnsError()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--level", "   "]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void Parse_BareXmlPath_IsTreatedAsCustomLevel()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse([@"C:\maps\dropped.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.EndsWith("dropped.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_BareXmlPath_IgnoresCase()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["level.XML"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(Path.GetFullPath("level.XML"), result.LevelPath);
        }

        [Fact]
        public void Parse_BareNonXmlPath_IsNotCustomLevel()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["notes.txt"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelSwitchTakesPrecedenceOverBarePath()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["bare.xml", "--level", "chosen.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.EndsWith("chosen.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_RelativePath_IsResolvedAgainstWorkingDirectory()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--level", "level.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(
                Path.GetFullPath("level.xml"),
                result.LevelPath);
        }
    }
}
