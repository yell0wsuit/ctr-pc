using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Helpers;
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
        public void OpacityCapsTheEnvelopePeakInsteadOfFightingIt()
        {
            LoadResult result = Load("<tutorialText locale=\"en\" opacity=\"0.4\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Track colorTrack = prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(0.4f, prompt.Opacity);
            Assert.Equal(
                [0f, 0.4f, 0.4f, 0f],
                colorTrack.keyFrames.Select(keyFrame => keyFrame.value.color.rgba.AlphaChannel));
        }

        [Fact]
        public void AnAuthoredColorIsBakedIntoTheSignRatherThanMultipliedOverIt()
        {
            // The ink quads are pure black, so multiplying the envelope over them would leave them
            // black whatever color was authored. The color goes into the sign's own pixels instead,
            // which leaves the envelope free to fade opacity alone.
            LoadResult result = Load("<tutorial04 locale=\"en\" color=\"#ff0000\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Track colorTrack = prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(1f, Assert.Single(result.Factory.SignColors).Value.RedColor, 3);
            Assert.Equal(0f, Assert.Single(result.Factory.SignColors).Value.GreenColor, 3);
            Assert.All(
                colorTrack.keyFrames,
                keyFrame =>
                {
                    Assert.Equal(1f, keyFrame.value.color.rgba.RedColor, 3);
                    Assert.Equal(1f, keyFrame.value.color.rgba.GreenColor, 3);
                    Assert.Equal(1f, keyFrame.value.color.rgba.BlueColor, 3);
                });
        }

        [Fact]
        public void AnUncoloredSignIsBuiltFromTheUntouchedAtlas()
        {
            LoadResult result = Load("<tutorial04 locale=\"en\" />");

            Assert.Null(Assert.Single(result.Factory.SignColors));
        }

        [Theory]
        [InlineData("tutorial10")]
        [InlineData("tutorial11")]
        public void RejectsAColorOnASignThatIsAlreadyDrawnInColor(string element)
        {
            // These two quads are full-color art; replacing their pixels would flatten them to a
            // silhouette, so authoring a color on them is a mistake rather than an effect.
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load($"<{element} locale=\"en\" color=\"#ff0000\" />"));

            Assert.Contains("color", exception.Message);
        }

        [Fact]
        public void AnAlreadyColoredSignStillLoadsWithoutAColor()
        {
            LoadResult result = Load("<tutorial10 locale=\"en\" />");

            _ = Assert.Single(result.Prompts);
        }

        [Fact]
        public void AnInvalidPromptNamesTheElementItCameFrom()
        {
            // The map name alone leaves an author hunting through every tutorial in the level.
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load("<tutorial10 locale=\"en\" x=\"330\" y=\"120\" color=\"#75e545\" />"));

            Assert.Contains("color=\"#75e545\"", exception.Message);
            Assert.Contains("<tutorial10 locale=\"en\" x=\"330\" y=\"120\"", exception.Message);
        }

        [Fact]
        public void SkippingInvalidPromptsDropsOnlyTheOffendingOne()
        {
            IReadOnlyList<TutorialPrompt> prompts = LoadTolerantly(
                "<tutorial10 locale=\"en\" color=\"#75e545\" />",
                "<tutorialText locale=\"en\" x=\"1\" y=\"2\" />");

            Assert.True(Assert.Single(prompts).IsText);
        }

        [Fact]
        public void AnInvalidPromptStillFailsTheLoadByDefault()
        {
            // What keeps a typo in shipped content from reaching a player as a prompt that never
            // plays: the content tests load every map through this same strict path.
            _ = Assert.Throws<InvalidDataException>(
                () => Load("<tutorial10 locale=\"en\" color=\"#75e545\" />"));
        }

        [Fact]
        public void SizeAndLineHeightReachThePrompt()
        {
            LoadResult result = Load("<tutorialText locale=\"en\" size=\"1.5\" lineHeight=\"2\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);

            Assert.Equal(1.5f, prompt.SizeScale);
            Assert.Equal(2f, prompt.LineHeightScale);
        }

        [Theory]
        [InlineData("size")]
        [InlineData("lineHeight")]
        public void TypesettingAttributesAreRejectedOnASign(string attribute)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load($"<tutorial04 locale=\"en\" {attribute}=\"2\" />"));

            Assert.Contains(attribute, exception.Message);
        }

        [Theory]
        [InlineData("tutorialText")]
        [InlineData("tutorial04")]
        public void AnAuthoredAngleRotatesEitherKindOfPrompt(string element)
        {
            LoadResult result = Load($"<{element} locale=\"en\" angle=\"15\" />");

            Assert.Equal(15f, Assert.Single(result.Prompts).Visual.rotation);
        }

        [Fact]
        public void APromptWithNoAngleIsUnrotated()
        {
            LoadResult result = Load("<tutorialText locale=\"en\" />");

            Assert.Equal(0f, Assert.Single(result.Prompts).Visual.rotation);
        }

        [Fact]
        public void RejectsAMalformedAngle()
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load("<tutorialText locale=\"en\" angle=\"sideways\" />"));

            Assert.Contains("angle", exception.Message);
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
        public void AuthoredMotionBuildsEasedLegsAfterItsDelay()
        {
            // 1_1's swipe, authored: two legs of 230 then 210 world units at 440/s, so travel
            // splits 0.523 / 0.477 and finishes 0.6s before the 3.1s pass ends.
            LoadResult result = Load(
                "<tutorial04 locale=\"en\" x=\"100\" y=\"50\" path=\"230,0,440,0\" moveSpeed=\"440\""
                + " ease=\"in,out\" moveDelay=\"1.5\" repeat=\"2\""
                + " fadeIn=\"0.5\" duration=\"2.1\" fadeOut=\"0.5\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);
            Timeline timeline = prompt.Visual.GetTimeline(0);
            Track positions = timeline.GetTrack(Track.TrackType.TRACK_POSITION);
            Track colors = timeline.GetTrack(Track.TrackType.TRACK_COLOR);

            Assert.Equal(0, prompt.TimelineIndex);
            Assert.Equal(8, colors.keyFramesCount);
            Assert.Equal(10, positions.keyFramesCount);

            float x = prompt.Visual.x;
            Assert.Equal([x, x, x + 230f, x + 440f, x + 440f], PositionsOfFirstPass(positions));
            Assert.Equal(
                [0f, 1.5f, 230f / 440f, 210f / 440f, 0.6f],
                TimesOfFirstPass(positions),
                new FloatComparer());
            Assert.Equal(
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN,
                positions.keyFrames[2].transitionType);
            Assert.Equal(
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
                positions.keyFrames[3].transitionType);
        }

        [Fact]
        public void AuthoredMotionReplacesTheMoverRatherThanCompoundingWithIt()
        {
            // Both driving one sign is what made 1_1's swipe race away from its start position.
            LoadResult result = Load(
                "<tutorial04 locale=\"en\" path=\"230,0\" moveSpeed=\"440\" ease=\"in\" />");
            GameObject sign = Assert.IsAssignableFrom<GameObject>(Assert.Single(result.Prompts).Visual);

            Assert.Null(sign.mover);
        }

        [Fact]
        public void APathWithoutTimelineAttributesStaysOnTheMover()
        {
            LoadResult result = Load("<tutorial04 locale=\"en\" path=\"-95,49,\" moveSpeed=\"100\" />");
            GameObject sign = Assert.IsAssignableFrom<GameObject>(Assert.Single(result.Prompts).Visual);

            Assert.NotNull(sign.mover);
            Assert.Null(sign.GetTimeline(0).GetTrack(Track.TrackType.TRACK_POSITION));
        }

        [Fact]
        public void APlainPathOnTextUsesTheMoverRatherThanTheTimeline()
        {
            LoadResult result = Load("<tutorialText locale=\"en\" path=\"90,0\" moveSpeed=\"100\" />");
            TutorialPrompt prompt = Assert.Single(result.Prompts);

            Assert.Null(prompt.Visual.GetTimeline(0).GetTrack(Track.TrackType.TRACK_POSITION));
        }

        [Fact]
        public void AForeverRepeatLoopsOnePassInsteadOfDuplicatingIt()
        {
            LoadResult result = Load(
                "<tutorial04 locale=\"en\" path=\"-95,49,0,0\" moveSpeed=\"100\" repeat=\"-1\" />");
            Timeline timeline = Assert.Single(result.Prompts).Visual.GetTimeline(0);

            Assert.Equal(4, timeline.GetTrack(Track.TrackType.TRACK_COLOR).keyFramesCount);
            Assert.Equal(4, timeline.GetTrack(Track.TrackType.TRACK_POSITION).keyFramesCount);

            // One pass is 1 + 5 + 0.5 = 6.5s of envelope; a looping timeline is still running well
            // past that, where a single-pass one would have stopped.
            BaseElement visual = Assert.Single(result.Prompts).Visual;
            visual.PlayTimeline(0);
            visual.Update(20f);

            Assert.Equal(Timeline.TimelineState.TIMELINE_PLAYING, timeline.state);
        }

        [Theory]
        [InlineData("repeat", "0")]
        [InlineData("repeat", "-2")]
        [InlineData("ease", "sideways")]
        [InlineData("moveSpeed", "0")]
        [InlineData("moveDelay", "-1")]
        public void RejectsInvalidMotion(string attribute, string value)
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load($"<tutorial04 locale=\"en\" path=\"10,0\" {attribute}=\"{value}\" />"));

            Assert.Contains(attribute, exception.Message);
        }

        [Fact]
        public void RejectsTravelThatOutlastsItsPass()
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load(
                    "<tutorial04 locale=\"en\" path=\"1000,0\" moveSpeed=\"10\" ease=\"none\" duration=\"1\" />"));

            Assert.Contains("path", exception.Message);
        }

        [Fact]
        public void RejectsAStaleSwipePresetRatherThanDroppingItsAnimation()
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => Load("<tutorial04 locale=\"en\" anim=\"swipe\" />"));

            Assert.Contains("anim", exception.Message);
        }

        private static float[] PositionsOfFirstPass(Track positions)
        {
            return [.. positions.keyFrames.Take(5).Select(keyFrame => keyFrame.value.pos.x)];
        }

        private static float[] TimesOfFirstPass(Track positions)
        {
            return [.. positions.keyFrames.Take(5).Select(keyFrame => keyFrame.timeOffset)];
        }

        private sealed class FloatComparer : IEqualityComparer<float>
        {
            public bool Equals(float left, float right)
            {
                return MathF.Abs(left - right) < 0.001f;
            }

            public int GetHashCode(float value)
            {
                return value.GetHashCode();
            }
        }

        [Theory]
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

        private static IReadOnlyList<TutorialPrompt> LoadTolerantly(params string[] elements)
        {
            TutorialPromptLoader loader = Loader(new FakeVisualFactory(), "en");
            return loader.LoadAll([.. elements.Select(XElement.Parse)], skipInvalid: true);
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
