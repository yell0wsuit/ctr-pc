using CutTheRopeDX.Content.Commands;

using Xunit;

namespace CutTheRopeDX.Content.Tests
{
    public sealed class CommandLineTests
    {
        [Theory]
        [InlineData("fetch", ContentCommand.Fetch)]
        [InlineData("verify", ContentCommand.Verify)]
        public void ParseSelectsAssetCommand(string command, ContentCommand expected)
        {
            ContentCommandLine commandLine = ContentCommandLine.Parse(
                [command, "--source", "/content"]);

            Assert.Equal(expected, commandLine.Command);
            Assert.Equal("/content", commandLine.SourceDirectory);
            Assert.Empty(commandLine.BuilderArguments);
        }

        [Fact]
        public void ParseAcceptsShortSourceOption()
        {
            ContentCommandLine commandLine = ContentCommandLine.Parse(
                ["fetch", "-s", "/content"]);

            Assert.Equal("/content", commandLine.SourceDirectory);
        }

        [Fact]
        public void ParsePassesBuildArgumentsThroughUnchanged()
        {
            string[] arguments =
            [
                "-p",
                "DesktopVK",
                "-s",
                "/content",
                "-o",
                "/output",
            ];

            ContentCommandLine commandLine = ContentCommandLine.Parse(["build", .. arguments]);

            Assert.Equal(ContentCommand.Build, commandLine.Command);
            Assert.Equal(arguments, commandLine.BuilderArguments);
        }

        [Fact]
        public void ParseTreatsLegacyArgumentsAsBuildArguments()
        {
            string[] arguments = ["-p", "DesktopVK", "-s", "/content"];

            ContentCommandLine commandLine = ContentCommandLine.Parse(arguments);

            Assert.Equal(ContentCommand.Build, commandLine.Command);
            Assert.Equal(arguments, commandLine.BuilderArguments);
        }
    }
}
