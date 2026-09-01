using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    public sealed class TutorialPromptLoaderTests
    {
        [Theory]
        [InlineData("tutorialText", 1f, 5f, 0.5f)]
        [InlineData("tutorial04", 1f, 5.2f, 0.5f)]
        public void DefaultsPreserveCurrentEnvelope(string name, float fadeIn, float hold, float fadeOut)
        {
            LoadResult result = Load($"<{name} locale=\"en\" x=\"1\" y=\"2\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Track colorTrack = prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(fadeIn, prompt.FadeIn);
            Assert.Equal(hold, prompt.Hold);
            Assert.Equal(fadeOut, prompt.FadeOut);
            Assert.Equal([0f, fadeIn, hold, fadeOut], colorTrack.keyFrames.Select(keyFrame => keyFrame.timeOffset));
        }

        [Fact]
        public void AuthoredTimingBuildsTheSharedEnvelope()
        {
            LoadResult result = Load(
                "<tutorialText locale=\"en\" fadeIn=\"0.25\" duration=\"2.5\" fadeOut=\"0\" delay=\"1.5\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Track colorTrack = prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(1.5f, prompt.Delay);
            Assert.Equal([0f, 0.25f, 2.5f, 0f], colorTrack.keyFrames.Select(keyFrame => keyFrame.timeOffset));
        }

        [Fact]
        public void AForeverHoldFadesUpAndNeverFadesOut()
        {
            LoadResult result = Load("<tutorialText locale=\"en\" duration=\"-1\" fadeIn=\"0.75\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Track colorTrack = prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(TutorialValues.ForeverHold, prompt.Hold);
            Assert.True(prompt.HoldsForever);
            Assert.Equal([0f, 0.75f], colorTrack.keyFrames.Select(keyFrame => keyFrame.timeOffset));

            // The point of the sentinel: once the fade-up stops, the prompt sits at full opacity
            // rather than fading back out.
            prompt.Visual.PlayTimeline(0);
            prompt.Visual.Update(0.75f);
            prompt.Visual.Update(30f);

            Assert.Equal(Timeline.TimelineState.TIMELINE_STOPPED, prompt.Visual.GetTimeline(0).state);
            Assert.Equal(1f, prompt.Visual.color.AlphaChannel, 3);
        }

        [Fact]
        public void ConvertsAreaFromMapCoordinatesUsingBaseAndMapOffsets()
        {
            LoadResult result = Load(
                "<tutorialText locale=\"en\" showOn=\"candyMoved\" inArea=\"1.9,2.9,3.9,4.9\" />",
                scale: 3f,
                offsetX: 100f,
                offsetY: 200f,
                mapOffsetX: 7,
                mapOffsetY: 11);

            TutorialArea area = Assert.Single(result.Prompts).Trigger.Area.Value;
            Assert.Equal(new TutorialArea(110f, 217f, 9f, 12f), area);
        }

        [Fact]
        public void ValidatesInactiveLocaleBeforeInstantiatingAnything()
        {
            FakeVisualFactory factory = new();
            TutorialPromptLoader loader = Loader(factory, locale: "en");
            XElement root = XElement.Parse(
                "<objects>"
                + "<tutorialText locale=\"ru\" showOn=\"NotAnEvent\" />"
                + "<tutorialText locale=\"en\" />"
                + "</objects>");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => loader.LoadAll(root.Elements()));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains("showOn", exception.Message);
            Assert.Empty(factory.CreatedNodes);
        }

        [Fact]
        public void UnsupportedLocaleFallsBackToEnglish()
        {
            FakeVisualFactory factory = new();
            TutorialPromptLoader loader = Loader(factory, locale: "vi");
            XElement root = XElement.Parse(
                "<objects>"
                + "<tutorialText locale=\"ru\" text=\"RU\" />"
                + "<tutorialText locale=\"en\" text=\"EN\" />"
                + "</objects>");

            IReadOnlyList<TutorialPrompt> prompts = loader.LoadAll(root.Elements());

            _ = Assert.Single(prompts);
            Assert.Equal("EN", Assert.Single(factory.CreatedNodes).Attribute("text")?.Value);
        }

        [Fact]
        public void ImagePreservesAuthoredMover()
        {
            LoadResult result = Load(
                "<tutorial04 locale=\"en\" x=\"10\" y=\"20\" angle=\"4\" "
                + "path=\"10,0\" moveSpeed=\"2\" rotateSpeed=\"3\" />");
            CTRGameObject visual = Assert.IsType<CTRGameObject>(Assert.Single(result.Prompts).Visual);

            Assert.NotNull(visual.mover);
            Assert.Equal(2, visual.mover.pathLen);
            Assert.Equal(4f, visual.rotation);
            Assert.Equal(30f, visual.mover.path[0].X);
            Assert.Equal(60f, visual.mover.path[0].Y);
        }

        [Fact]
        public void SwipeUsesTimelineOneWithTwoLegacyPassesAndNoEnvelopePlayback()
        {
            LoadResult result = Load("<tutorial04 locale=\"en\" x=\"100\" y=\"50\" anim=\"swipe\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Timeline swipe = prompt.Visual.GetTimeline(1);
            Track colors = swipe.GetTrack(Track.TrackType.TRACK_COLOR);
            Track positions = swipe.GetTrack(Track.TrackType.TRACK_POSITION);

            Assert.Equal(TutorialPromptState.Playing, prompt.State);
            Assert.Same(swipe, prompt.Visual.GetCurrentTimeline());
            Assert.Equal(1, prompt.TimelineIndex);
            Assert.Equal(10f, prompt.Visual.rotation);
            Assert.Equal(10, colors.keyFramesCount);
            Assert.Equal(12, positions.keyFramesCount);
            Assert.Equal(530f, positions.keyFrames[3].value.pos.x);
            Assert.Equal(740f, positions.keyFrames[4].value.pos.x);
            Assert.Equal(530f, positions.keyFrames[9].value.pos.x);
            Assert.Equal(740f, positions.keyFrames[10].value.pos.x);
            Assert.Equal(Timeline.TimelineState.TIMELINE_STOPPED, prompt.Visual.GetTimeline(0).state);
        }

        [Theory]
        [InlineData("anim", "wave")]
        [InlineData("delay", "NaN")]
        [InlineData("duration", "-2")]
        public void RejectsInvalidPresentationOrTiming(string attribute, string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load($"<tutorial04 locale=\"en\" {attribute}=\"{value}\" />"));

            Assert.Contains("scenario.xml", exception.Message);
            Assert.Contains(attribute, exception.Message);
        }

        private static LoadResult Load(
            string element,
            float scale = 3f,
            float offsetX = 0f,
            float offsetY = 0f,
            int mapOffsetX = 0,
            int mapOffsetY = 0)
        {
            FakeVisualFactory factory = new();
            TutorialPromptLoader loader = Loader(
                factory,
                "en",
                scale,
                offsetX,
                offsetY,
                mapOffsetX,
                mapOffsetY);
            XElement node = XElement.Parse(element);
            IReadOnlyList<TutorialPrompt> prompts = loader.LoadAll([node]);
            return new LoadResult(prompts, factory);
        }

        private static TutorialPromptLoader Loader(
            FakeVisualFactory factory,
            string locale,
            float scale = 3f,
            float offsetX = 0f,
            float offsetY = 0f,
            int mapOffsetX = 0,
            int mapOffsetY = 0)
        {
            return new TutorialPromptLoader(
                new TutorialDirector(new EmptyWorld()),
                factory,
                source: "scenario.xml",
                locale,
                twoParts: false,
                scale,
                offsetX,
                offsetY,
                mapOffsetX,
                mapOffsetY);
        }

        private sealed record LoadResult(
            IReadOnlyList<TutorialPrompt> Prompts,
            FakeVisualFactory Factory);
    }
}
