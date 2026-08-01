using System.IO;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CommandLineTests
    {
        [Fact]
        public void Parse_NoArguments_IsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse([]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_UnrelatedArguments_IsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse(["--windowed"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelWithPath_ReturnsAbsolutePath()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "/maps/test.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.True(Path.IsPathRooted(result.LevelPath));
            Assert.EndsWith("test.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_LevelWithoutValue_ReturnsError()
        {
            CommandLineResult result = CommandLine.Parse(["--level"]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelWithEmptyValue_ReturnsError()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "   "]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void Parse_BareXmlPath_IsTreatedAsCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse([@"C:\maps\dropped.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.EndsWith("dropped.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_BareXmlPath_IgnoresCase()
        {
            CommandLineResult result = CommandLine.Parse(["level.XML"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(Path.GetFullPath("level.XML"), result.LevelPath);
        }

        [Fact]
        public void Parse_BareNonXmlPath_IsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse(["notes.txt"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Parse_LevelSwitchTakesPrecedenceOverBarePath()
        {
            CommandLineResult result = CommandLine.Parse(["bare.xml", "--level", "chosen.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.EndsWith("chosen.xml", result.LevelPath);
        }

        [Fact]
        public void Parse_RelativePath_IsResolvedAgainstWorkingDirectory()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "level.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(
                Path.GetFullPath("level.xml"),
                result.LevelPath);
        }

        [Fact]
        public void Parse_SetsIsHeadless_WhenFlagPresent()
        {
            CommandLineResult result = CommandLine.Parse(["--headless"]);

            Assert.True(result.IsHeadless);
        }

        [Fact]
        public void Parse_IsHeadlessFalse_WhenFlagAbsent()
        {
            CommandLineResult result = CommandLine.Parse([]);

            Assert.False(result.IsHeadless);
        }

        [Fact]
        public void Parse_CombinesHeadlessWithLevel()
        {
            CommandLineResult result = CommandLine.Parse(["--headless", "--level", "/tmp/a.xml"]);

            Assert.True(result.IsHeadless);
            Assert.True(result.IsCustomLevel);
            Assert.EndsWith("a.xml", result.LevelPath);
        }
    }
}
