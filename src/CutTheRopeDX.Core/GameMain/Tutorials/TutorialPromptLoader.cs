using System;
using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal interface ITutorialVisualFactory
    {
        BaseElement CreateText(XElement node, float x, float y, float width);

        BaseElement CreateSign(XElement node, int quad, float x, float y);
    }

    internal sealed class TutorialPromptLoader
    {
        private readonly TutorialDirector director;
        private readonly ITutorialVisualFactory visualFactory;
        private readonly string source;
        private readonly string locale;
        private readonly bool twoParts;
        private readonly float scale;
        private readonly float offsetX;
        private readonly float offsetY;
        private readonly int mapOffsetX;
        private readonly int mapOffsetY;

        internal TutorialPromptLoader(
            TutorialDirector director,
            ITutorialVisualFactory visualFactory,
            string source,
            string locale,
            bool twoParts,
            float scale,
            float offsetX,
            float offsetY,
            int mapOffsetX,
            int mapOffsetY)
        {
            this.director = director ?? throw new ArgumentNullException(nameof(director));
            this.visualFactory = visualFactory ?? throw new ArgumentNullException(nameof(visualFactory));
            this.source = source;
            this.locale = LanguageHelper.IsUiLanguageCode(locale) ? locale : "en";
            this.twoParts = twoParts;
            this.scale = scale;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.mapOffsetX = mapOffsetX;
            this.mapOffsetY = mapOffsetY;
        }

        internal IReadOnlyList<TutorialPrompt> LoadAll(IEnumerable<XElement> nodes)
        {
            List<ParsedTutorial> parsedTutorials = [];
            foreach (XElement node in nodes)
            {
                parsedTutorials.Add(Parse(node));
            }

            List<TutorialPrompt> loadedPrompts = [];
            foreach (ParsedTutorial parsed in parsedTutorials)
            {
                if (parsed.Locale != locale)
                {
                    continue;
                }

                TutorialPrompt prompt = Instantiate(parsed);
                director.Add(prompt);
                loadedPrompts.Add(prompt);
            }

            director.CompleteLoading();
            return loadedPrompts;
        }

        internal static Timeline BuildEnvelope(BaseElement visual, float fadeIn, float hold, float fadeOut)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.transparentRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                0f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.solidOpaqueRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                fadeIn));
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.solidOpaqueRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                hold));
            timeline.AddKeyFrame(KeyFrame.MakeColor(
                RGBAColor.transparentRGBA,
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                fadeOut));
            visual.AddTimelinewithID(timeline, 0);
            return timeline;
        }

        internal static Timeline BuildSwipe(BaseElement visual)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(12);
            for (int pass = 0; pass < 2; pass++)
            {
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.transparentRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE,
                    0f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.solidOpaqueRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    0.5f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.solidOpaqueRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    1f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.solidOpaqueRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    1.1f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    RGBAColor.transparentRGBA,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    0.5f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE,
                    0f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    0.5f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    1f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x + 230f,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN,
                    0.5f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x + 440f,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
                    0.5f));
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x + 440f,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    0.6f));
            }
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
            visual.AddTimelinewithID(timeline, 1);
            visual.rotation = 10f;
            return timeline;
        }

        private ParsedTutorial Parse(XElement node)
        {
            string name = node.Name.LocalName;
            bool isText = name == "tutorialText";
            int quad = isText ? -1 : ParseTutorialQuad(name);
            TutorialTrigger trigger = TutorialTrigger.Parse(
                node.Attribute("showOn")?.Value,
                node.Attribute("inArea")?.Value,
                node.Attribute("subject")?.Value,
                twoParts,
                source);
            float delay = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("delay")?.Value,
                0f,
                source,
                "delay");
            float fadeIn = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("fadeIn")?.Value,
                1f,
                source,
                "fadeIn");
            float hold = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("duration")?.Value,
                isText ? 5f : 5.2f,
                source,
                "duration");
            float fadeOut = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("fadeOut")?.Value,
                0.5f,
                source,
                "fadeOut");
            string animationValue = node.Attribute("anim")?.Value;
            string animation = animationValue switch
            {
                null or "swipe" => animationValue,
                _ => throw TutorialValues.Invalid(source, "anim", animationValue),
            };

            return new ParsedTutorial(
                node,
                node.Attribute("locale")?.Value ?? string.Empty,
                isText,
                quad,
                trigger,
                node.Attribute("group")?.Value,
                delay,
                fadeIn,
                hold,
                fadeOut,
                animation);
        }

        private TutorialPrompt Instantiate(ParsedTutorial parsed)
        {
            XElement node = parsed.Node;
            float x = (ParseCoordinateIntOrZero(node.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseCoordinateIntOrZero(node.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            BaseElement visual = parsed.IsText
                ? visualFactory.CreateText(
                    node,
                    x,
                    y,
                    ParseIntOrZero(node.Attribute("width")?.Value) * scale)
                : visualFactory.CreateSign(node, parsed.Quad, x, y);
            visual.color = RGBAColor.transparentRGBA;
            if (!parsed.IsText && visual is GameObject gameObject)
            {
                gameObject.ParseMover(node);
            }

            _ = BuildEnvelope(visual, parsed.FadeIn, parsed.Hold, parsed.FadeOut);
            int timelineIndex = 0;
            if (parsed.Animation == "swipe")
            {
                _ = BuildSwipe(visual);
                timelineIndex = 1;
            }

            TutorialTrigger worldTrigger = parsed.Trigger with
            {
                Area = ConvertArea(node.Attribute("inArea")?.Value, parsed.Trigger.Area),
            };
            return new TutorialPrompt(
                visual,
                worldTrigger,
                parsed.Group,
                parsed.Delay,
                parsed.FadeIn,
                parsed.Hold,
                parsed.FadeOut,
                parsed.IsText,
                timelineIndex);
        }

        private TutorialArea? ConvertArea(string value, TutorialArea? parsedArea)
        {
            if (parsedArea is null)
            {
                return null;
            }

            string[] parts = value.Split(',');
            int x = ParseCoordinateIntOrZero(parts[0]);
            int y = ParseCoordinateIntOrZero(parts[1]);
            int width = ParseCoordinateIntOrZero(parts[2]);
            int height = ParseCoordinateIntOrZero(parts[3]);
            return new TutorialArea(
                (x * scale) + offsetX + mapOffsetX,
                (y * scale) + offsetY + mapOffsetY,
                width * scale,
                height * scale);
        }

        private int ParseTutorialQuad(string name)
        {
            return name.Length == 10
                && name.StartsWith("tutorial", StringComparison.Ordinal)
                && int.TryParse(name.AsSpan(8), out int number)
                && number is >= 1 and <= 11
                    ? number - 1
                    : throw TutorialValues.Invalid(source, "element", name);
        }

        private sealed record ParsedTutorial(
            XElement Node,
            string Locale,
            bool IsText,
            int Quad,
            TutorialTrigger Trigger,
            string Group,
            float Delay,
            float FadeIn,
            float Hold,
            float FadeOut,
            string Animation);
    }
}
