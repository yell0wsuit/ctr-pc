using System.IO;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CommandLineTests
    {
        [Fact]
        public void ParseNoArgumentsIsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse([]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void ParseUnrelatedArgumentsIsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse(["--windowed"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void ParseLevelWithPathReturnsAbsolutePath()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "/maps/test.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.True(Path.IsPathRooted(result.LevelPath));
            Assert.EndsWith("test.xml", result.LevelPath);
        }

        [Fact]
        public void ParseLevelWithoutValueReturnsError()
        {
            CommandLineResult result = CommandLine.Parse(["--level"]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void ParseLevelWithEmptyValueReturnsError()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "   "]);

            Assert.True(result.IsCustomLevel);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void ParseBareXmlPathIsTreatedAsCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse([@"C:\maps\dropped.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
            Assert.EndsWith("dropped.xml", result.LevelPath);
        }

        [Fact]
        public void ParseBareXmlPathIgnoresCase()
        {
            CommandLineResult result = CommandLine.Parse(["level.XML"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(Path.GetFullPath("level.XML"), result.LevelPath);
        }

        [Fact]
        public void ParseBareNonXmlPathIsNotCustomLevel()
        {
            CommandLineResult result = CommandLine.Parse(["notes.txt"]);

            Assert.False(result.IsCustomLevel);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void ParseLevelSwitchTakesPrecedenceOverBarePath()
        {
            CommandLineResult result = CommandLine.Parse(["bare.xml", "--level", "chosen.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.EndsWith("chosen.xml", result.LevelPath);
        }

        [Fact]
        public void ParseRelativePathIsResolvedAgainstWorkingDirectory()
        {
            CommandLineResult result = CommandLine.Parse(["--level", "level.xml"]);

            Assert.True(result.IsCustomLevel);
            Assert.Equal(
                Path.GetFullPath("level.xml"),
                result.LevelPath);
        }

        [Fact]
        public void ParseSetsIsHeadlessWhenFlagPresent()
        {
            CommandLineResult result = CommandLine.Parse(["--headless"]);

            Assert.True(result.IsHeadless);
        }

        [Fact]
        public void ParseIsHeadlessFalseWhenFlagAbsent()
        {
            CommandLineResult result = CommandLine.Parse([]);

            Assert.False(result.IsHeadless);
        }

        [Fact]
        public void ParseCombinesHeadlessWithLevel()
        {
            CommandLineResult result = CommandLine.Parse(["--headless", "--level", "/tmp/a.xml"]);

            Assert.True(result.IsHeadless);
            Assert.True(result.IsCustomLevel);
            Assert.EndsWith("a.xml", result.LevelPath);
        }
    }
}
