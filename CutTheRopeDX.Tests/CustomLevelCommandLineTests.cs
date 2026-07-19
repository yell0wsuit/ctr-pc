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
            Assert.True(System.IO.Path.IsPathRooted(result.LevelPath));
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
        public void Parse_RelativePath_IsResolvedAgainstWorkingDirectory()
        {
            CustomLevelCommandLineResult result = CustomLevelCommandLine.Parse(["--level", "level.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(
                System.IO.Path.GetFullPath("level.xml"),
                result.LevelPath);
        }
    }
}
