using System.IO;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain.Tutorials;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    public sealed class TutorialSchemaTests
    {
        [Theory]
        [MemberData(nameof(ClosedVocabulary))]
        public void ParsesClosedVocabulary(
            string xml,
            object expected,
            object kind,
            object observation)
        {
            TutorialEvent expectedEvent = (TutorialEvent)expected;

            Assert.Equal(expectedEvent, TutorialEvents.Parse(xml, "scenario.xml", "showOn"));
            Assert.Equal(kind, TutorialEvents.Kind(expectedEvent));
            Assert.Equal(observation, TutorialEvents.Observation(expectedEvent));
        }

        [Theory]
        [InlineData("onConveyor")]
        [InlineData("Start")]
        [InlineData("rocketignite")]
        [InlineData("unknown")]
        public void RejectsNamesOutsideClosedVocabulary(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialEvents.Parse(value, "scenario.xml", "showOn"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("showOn", exception.Message);
        }

        [Theory]
        [InlineData("oops")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("-0.01")]
        public void RejectsInvalidTiming(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParseNonNegativeFloat(value, 1f, "scenario.xml", "delay"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("delay", exception.Message);
        }

        [Theory]
        [InlineData("oops")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("-0.5")]
        [InlineData("-2")]
        public void RejectsAHoldThatIsNeitherNonNegativeNorTheForeverSentinel(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParseHoldDuration(value, 5f, "scenario.xml", "duration"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("duration", exception.Message);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("-1.0")]
        public void ParsesTheForeverHoldSentinel(string value)
        {
            Assert.Equal(
                TutorialValues.ForeverHold,
                TutorialValues.ParseHoldDuration(value, 5f, "scenario.xml", "duration"));
        }

        [Fact]
        public void ParsesAnOrdinaryHoldAndItsDefault()
        {
            Assert.Equal(2.5f, TutorialValues.ParseHoldDuration("2.5", 5f, "scenario.xml", "duration"));
            Assert.Equal(0f, TutorialValues.ParseHoldDuration("0", 5f, "scenario.xml", "duration"));
            Assert.Equal(5.2f, TutorialValues.ParseHoldDuration(null, 5.2f, "scenario.xml", "duration"));
        }

        [Theory]
        [InlineData("1.5")]
        [InlineData("-0.1")]
        [InlineData("oops")]
        public void RejectsAnOpacityOutsideZeroToOne(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParseUnitInterval(value, 1f, "scenario.xml", "opacity"));

            Assert.Contains("opacity", exception.Message);
        }

        [Fact]
        public void ParsesOpacityAcrossItsWholeRange()
        {
            Assert.Equal(0f, TutorialValues.ParseUnitInterval("0", 1f, "scenario.xml", "opacity"));
            Assert.Equal(0.4f, TutorialValues.ParseUnitInterval("0.4", 1f, "scenario.xml", "opacity"));
            Assert.Equal(1f, TutorialValues.ParseUnitInterval("1", 0.5f, "scenario.xml", "opacity"));
            Assert.Equal(1f, TutorialValues.ParseUnitInterval(null, 1f, "scenario.xml", "opacity"));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("oops")]
        public void RejectsAMultiplierThatIsNotPositive(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParsePositiveFloat(value, 1f, "scenario.xml", "size"));

            Assert.Contains("size", exception.Message);
        }

        [Fact]
        public void ParsesAPositiveMultiplierAndItsDefault()
        {
            Assert.Equal(1.5f, TutorialValues.ParsePositiveFloat("1.5", 1f, "scenario.xml", "size"));
            Assert.Equal(1f, TutorialValues.ParsePositiveFloat(null, 1f, "scenario.xml", "size"));
        }

        [Fact]
        public void ParsesAnRgbTripletTheWayPackConfigWritesOne()
        {
            // Pack configs already spell a color as 0-255 channels, e.g. boxHoleBgColor [70, 37, 0].
            RGBAColor parsed = TutorialValues.ParseColor("70,37,0", "scenario.xml", "color").Value;

            Assert.Equal(70f / 255f, parsed.RedColor, 3);
            Assert.Equal(37f / 255f, parsed.GreenColor, 3);
            Assert.Equal(0f, parsed.BlueColor, 3);
        }

        [Fact]
        public void ParsesAnRgbTripletCopiedWithItsSpaces()
        {
            RGBAColor parsed = TutorialValues.ParseColor("70, 37, 0", "scenario.xml", "color").Value;

            Assert.Equal(70f / 255f, parsed.RedColor, 3);
        }

        [Theory]
        [InlineData("70,37")]
        [InlineData("70,37,0,1")]
        [InlineData("256,0,0")]
        [InlineData("-1,0,0")]
        [InlineData("70.5,37,0")]
        public void RejectsAnRgbTripletOutsideThreeByteChannels(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParseColor(value, "scenario.xml", "color"));

            Assert.Contains("color", exception.Message);
        }

        [Theory]
        [InlineData("ff8800")]
        [InlineData("#fff")]
        [InlineData("#gggggg")]
        [InlineData("#ff88000")]
        public void RejectsAColorThatIsNeitherHexNorATriplet(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialValues.ParseColor(value, "scenario.xml", "color"));

            Assert.Contains("color", exception.Message);
        }

        [Fact]
        public void ParsesAHexColorIntoUnitChannels()
        {
            RGBAColor parsed = TutorialValues.ParseColor("#ff8000", "scenario.xml", "color").Value;

            Assert.Equal(1f, parsed.RedColor, 3);
            Assert.Equal(128f / 255f, parsed.GreenColor, 3);
            Assert.Equal(0f, parsed.BlueColor, 3);
            Assert.Null(TutorialValues.ParseColor(null, "scenario.xml", "color"));
        }

        [Fact]
        public void ParsesTimingUsingInvariantCulture()
        {
            Assert.Equal(1.25f, TutorialValues.ParseNonNegativeFloat("1.25", 0f, "scenario.xml", "fadeIn"));
            Assert.Equal(4f, TutorialValues.ParseNonNegativeFloat(null, 4f, "scenario.xml", "fadeIn"));
        }

        [Theory]
        [InlineData("1,2,0,4")]
        [InlineData("1,2,3,-4")]
        [InlineData("1,2,three,4")]
        [InlineData("1,2,NaN,4")]
        [InlineData("1,2,Infinity,4")]
        [InlineData("1,2,3")]
        public void RejectsMalformedOrNonPositiveAreas(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialArea.Parse(value, "scenario.xml", "inArea"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("inArea", exception.Message);
        }

        [Fact]
        public void ParsesFiniteAreaAndUsesHalfOpenContainment()
        {
            TutorialArea area = TutorialArea.Parse("1.5,2.5,3,4", "scenario.xml", "inArea");

            Assert.True(area.Contains(new Vector(1.5f, 2.5f)));
            Assert.True(area.Contains(new Vector(4.49f, 6.49f)));
            Assert.False(area.Contains(new Vector(4.5f, 6.49f)));
            Assert.False(area.Contains(new Vector(4.49f, 6.5f)));
        }

        [Theory]
        [InlineData(null, TutorialSubject.Any)]
        [InlineData("any", TutorialSubject.Any)]
        [InlineData("primary", TutorialSubject.Primary)]
        [InlineData("left", TutorialSubject.Left)]
        [InlineData("right", TutorialSubject.Right)]
        public void ParsesSubjects(string xml, object expected)
        {
            Assert.Equal(expected, TutorialSubjects.Parse(xml, "scenario.xml", "subject"));
        }

        [Theory]
        [InlineData("Any")]
        [InlineData("middle")]
        public void RejectsUnknownSubjects(string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialSubjects.Parse(value, "scenario.xml", "subject"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("subject", exception.Message);
        }

        [Fact]
        public void CandyMovedRequiresArea()
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialTrigger.Parse("candyMoved", null, "any", twoParts: false, "scenario.xml"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("inArea", exception.Message);
        }

        [Theory]
        [InlineData("left")]
        [InlineData("right")]
        public void SplitSubjectsRequireTwoParts(string subject)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => TutorialTrigger.Parse("start", null, subject, twoParts: false, "scenario.xml"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("subject", exception.Message);
        }

        [Fact]
        public void ParsesSemanticallyValidTrigger()
        {
            TutorialTrigger trigger = TutorialTrigger.Parse(
                "candyMoved",
                "1,2,3,4",
                "left",
                twoParts: true,
                "scenario.xml");

            Assert.Equal(TutorialEvent.CandyMoved, trigger.Event);
            Assert.Equal(TutorialSubject.Left, trigger.Subject);
            Assert.Equal(new TutorialArea(1f, 2f, 3f, 4f), trigger.Area);
        }

        public static TheoryData<string, object, object, object> ClosedVocabulary()
        {
            return new TheoryData<string, object, object, object>
            {
                { "start", TutorialEvent.Start, TutorialEventKind.Edge, TutorialObservation.Push },
                { "bubbleCapture", TutorialEvent.BubbleCapture, TutorialEventKind.Edge, TutorialObservation.Push },
                { "bubblePop", TutorialEvent.BubblePop, TutorialEventKind.Edge, TutorialObservation.Push },
                { "lanternCatch", TutorialEvent.LanternCatch, TutorialEventKind.Edge, TutorialObservation.Push },
                { "sockCatch", TutorialEvent.SockCatch, TutorialEventKind.Edge, TutorialObservation.Push },
                { "mouseGrab", TutorialEvent.MouseGrab, TutorialEventKind.Edge, TutorialObservation.Push },
                { "spiderSteal", TutorialEvent.SpiderSteal, TutorialEventKind.Edge, TutorialObservation.Push },
                { "handGrab", TutorialEvent.HandGrab, TutorialEventKind.Edge, TutorialObservation.Push },
                { "ropeCut", TutorialEvent.RopeCut, TutorialEventKind.Edge, TutorialObservation.Push },
                { "starCollected", TutorialEvent.StarCollected, TutorialEventKind.Edge, TutorialObservation.Push },
                { "candyEaten", TutorialEvent.CandyEaten, TutorialEventKind.Edge, TutorialObservation.Push },
                { "pipeEnter", TutorialEvent.PipeEnter, TutorialEventKind.Edge, TutorialObservation.Push },
                { "spikeHit", TutorialEvent.SpikeHit, TutorialEventKind.Edge, TutorialObservation.Push },
                { "electroHit", TutorialEvent.ElectroHit, TutorialEventKind.Edge, TutorialObservation.Push },
                { "gameWon", TutorialEvent.GameWon, TutorialEventKind.Edge, TutorialObservation.Push },
                { "gameLost", TutorialEvent.GameLost, TutorialEventKind.Edge, TutorialObservation.Push },
                { "rocketIgnite", TutorialEvent.RocketIgnite, TutorialEventKind.Edge, TutorialObservation.Diffed },
                { "bouncerHit", TutorialEvent.BouncerHit, TutorialEventKind.Edge, TutorialObservation.Push },
                { "pumpFire", TutorialEvent.PumpFire, TutorialEventKind.Edge, TutorialObservation.Push },
                { "steamBurst", TutorialEvent.SteamBurst, TutorialEventKind.Edge, TutorialObservation.Push },
                { "discSpin", TutorialEvent.DiscSpin, TutorialEventKind.Edge, TutorialObservation.Push },
                { "timeFreeze", TutorialEvent.TimeFreeze, TutorialEventKind.Edge, TutorialObservation.Push },
                { "timeUnfreeze", TutorialEvent.TimeUnfreeze, TutorialEventKind.Edge, TutorialObservation.Push },
                { "gravityFlip", TutorialEvent.GravityFlip, TutorialEventKind.Edge, TutorialObservation.Push },
                { "bubbled", TutorialEvent.Bubbled, TutorialEventKind.State, TutorialObservation.Sampled },
                { "inLantern", TutorialEvent.InLantern, TutorialEventKind.State, TutorialObservation.Sampled },
                { "carriedByAnt", TutorialEvent.CarriedByAnt, TutorialEventKind.State, TutorialObservation.Sampled },
                { "carriedBySnail", TutorialEvent.CarriedBySnail, TutorialEventKind.State, TutorialObservation.Sampled },
                { "timeFrozen", TutorialEvent.TimeFrozen, TutorialEventKind.State, TutorialObservation.Sampled },
                { "gravityInverted", TutorialEvent.GravityInverted, TutorialEventKind.State, TutorialObservation.Sampled },
                { "candyMoved", TutorialEvent.CandyMoved, TutorialEventKind.State, TutorialObservation.Sampled },
            };
        }
    }
}
