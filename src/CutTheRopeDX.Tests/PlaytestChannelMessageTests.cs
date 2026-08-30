using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class PlaytestChannelMessageTests
    {
        [Fact]
        public void ReadyRoundTrips()
        {
            string json = PlaytestChannelMessage.FormatReady("a1b2c3d4", "ctrdx-playtest 1 1.0.0");

            bool ok = PlaytestChannelMessage.TryParse(json, out PlaytestMessageKind kind, out string nonce, out string payload);

            Assert.True(ok);
            Assert.Equal(PlaytestMessageKind.Ready, kind);
            Assert.Equal("a1b2c3d4", nonce);
            Assert.Equal("ctrdx-playtest 1 1.0.0", payload);
        }

        [Fact]
        public void ReadyPayloadParsesAsTheDesktopHandshakeLine()
        {
            // The browser handshake must stay byte-identical to the line desktop writes to stdout,
            // so the editor can parse both with one parser.
            string json = PlaytestChannelMessage.FormatReady("n", PlaytestHandshake.FormatLine("2.3.4"));

            _ = PlaytestChannelMessage.TryParse(json, out _, out _, out string line);

            Assert.Equal("ctrdx-playtest 1 2.3.4", line);
        }

        [Fact]
        public void LevelRoundTrips()
        {
            string xml = "<map><candy x=\"1\" y=\"2\" /></map>";
            string json = PlaytestChannelMessage.FormatLevel("nonce123", xml);

            bool ok = PlaytestChannelMessage.TryParse(json, out PlaytestMessageKind kind, out string nonce, out string payload);

            Assert.True(ok);
            Assert.Equal(PlaytestMessageKind.Level, kind);
            Assert.Equal("nonce123", nonce);
            Assert.Equal(xml, payload);
        }

        [Fact]
        public void LevelRoundTripsALargeCommunityScaleLevel()
        {
            // Real community levels reach ~112 KB; an uncapped editor can produce far more.
            string xml = "<map>" + string.Concat(System.Linq.Enumerable.Repeat(
                "<grab x=\"159\" y=\"337\" length=\"90\" wheel=\"false\" gun=\"false\" />", 20000)) + "</map>";
            string json = PlaytestChannelMessage.FormatLevel("n", xml);

            bool ok = PlaytestChannelMessage.TryParse(json, out _, out _, out string payload);

            Assert.True(ok);
            Assert.Equal(xml, payload);
        }

        [Fact]
        public void ErrorRoundTrips()
        {
            string json = PlaytestChannelMessage.FormatError("Level file contains no root element.");

            bool ok = PlaytestChannelMessage.TryParse(json, out PlaytestMessageKind kind, out _, out string payload);

            Assert.True(ok);
            Assert.Equal(PlaytestMessageKind.Error, kind);
            Assert.Equal("Level file contains no root element.", payload);
        }

        [Fact]
        public void ByeRoundTrips()
        {
            bool ok = PlaytestChannelMessage.TryParse(PlaytestChannelMessage.FormatBye(),
                out PlaytestMessageKind kind, out _, out _);

            Assert.True(ok);
            Assert.Equal(PlaytestMessageKind.Bye, kind);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not json at all")]
        [InlineData("[1,2,3]")]
        [InlineData("\"a string\"")]
        [InlineData("{}")]
        [InlineData("{\"type\":\"bye\"}")]
        [InlineData("{\"v\":\"1\",\"type\":\"bye\"}")]
        public void MalformedInputIsRejected(string json)
        {
            Assert.False(PlaytestChannelMessage.TryParse(json, out _, out _, out _));
        }

        [Fact]
        public void UnknownTypeIsIgnoredRatherThanThrowing()
        {
            // A future protocol talking to this build must degrade to silence, not a crash.
            Assert.False(PlaytestChannelMessage.TryParse(
                "{\"v\":1,\"type\":\"teleport\"}", out _, out _, out _));
        }

        [Fact]
        public void MismatchedVersionIsIgnored()
        {
            Assert.False(PlaytestChannelMessage.TryParse(
                "{\"v\":2,\"type\":\"bye\"}", out _, out _, out _));
        }

        // These literals are the contract between this repository and ctrdx-editor. The editor's
        // PlaytestChannelMessageTests assert the same strings. If you change one side, this fails.
        [Fact]
        public void WireFormatIsStable()
        {
            Assert.Equal("{\"v\":1,\"type\":\"bye\"}", PlaytestChannelMessage.FormatBye());
            Assert.Equal("{\"v\":1,\"type\":\"ready\",\"nonce\":\"abc\",\"line\":\"ctrdx-playtest 1 9.9.9\"}",
                PlaytestChannelMessage.FormatReady("abc", "ctrdx-playtest 1 9.9.9"));
            Assert.Equal("{\"v\":1,\"type\":\"level\",\"nonce\":\"abc\",\"xml\":\"<map/>\"}",
                PlaytestChannelMessage.FormatLevel("abc", "<map/>"));
            Assert.Equal("{\"v\":1,\"type\":\"error\",\"message\":\"boom\"}",
                PlaytestChannelMessage.FormatError("boom"));
        }
    }
}
