using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Creates concrete text and image visuals for validated tutorial XML.</summary>
    internal interface ITutorialVisualFactory
    {
        /// <summary>Creates a localized tutorial text visual.</summary>
        /// <param name="node">Validated tutorial text XML.</param>
        /// <param name="x">World-space X position.</param>
        /// <param name="y">World-space Y position.</param>
        /// <param name="width">Scaled text width.</param>
        /// <returns>The created text visual.</returns>
        BaseElement CreateText(XElement node, float x, float y, float width);

        /// <summary>Creates a tutorial sign visual.</summary>
        /// <param name="node">Validated tutorial image XML.</param>
        /// <param name="quad">Zero-based tutorial-sign quad.</param>
        /// <param name="x">World-space X position.</param>
        /// <param name="y">World-space Y position.</param>
        /// <returns>The created sign visual.</returns>
        BaseElement CreateSign(XElement node, int quad, float x, float y);
    }

    /// <summary>Validates tutorial XML, filters locale copies, and constructs registered prompts.</summary>
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

        /// <summary>Initializes a loader for one map and coordinate transform.</summary>
        /// <param name="director">Director that receives loaded prompts.</param>
        /// <param name="visualFactory">Factory for concrete text and image visuals.</param>
        /// <param name="source">Map name used in validation errors.</param>
        /// <param name="locale">Requested locale code; unsupported codes fall back to English.</param>
        /// <param name="twoParts">Whether split-candy subjects are legal.</param>
        /// <param name="scale">Map-to-world coordinate scale.</param>
        /// <param name="offsetX">Base world-space X offset.</param>
        /// <param name="offsetY">Base world-space Y offset.</param>
        /// <param name="mapOffsetX">Additional authored-map X offset.</param>
        /// <param name="mapOffsetY">Additional authored-map Y offset.</param>
        /// <exception cref="ArgumentNullException">Thrown when the director or visual factory is null.</exception>
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

        /// <summary>Validates all nodes before instantiating the active locale and completing registration.</summary>
        /// <param name="nodes">Tutorial elements in XML order.</param>
        /// <returns>The locale-selected prompts in XML order.</returns>
        /// <exception cref="InvalidDataException">Thrown when any active or inactive locale copy is invalid.</exception>
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

        /// <summary>
        /// Builds a prompt's whole timeline at index zero: a color envelope, plus position
        /// keyframes when motion is authored, repeated for as many passes as the prompt asks for.
        /// </summary>
        /// <param name="visual">Visual that owns the timeline.</param>
        /// <param name="motion">Authored motion, or <see langword="null"/> for a stationary prompt.</param>
        /// <param name="fadeIn">Fade-in duration in seconds.</param>
        /// <param name="hold">Full-opacity duration, or <see cref="TutorialValues.ForeverHold"/>.</param>
        /// <param name="fadeOut">Fade-out duration in seconds.</param>
        /// <param name="peakColor">Color and opacity held at full visibility, or null for solid white.</param>
        /// <param name="repeat">Pass count, or <see cref="TutorialValues.ForeverRepeat"/> to loop.</param>
        /// <returns>The constructed timeline.</returns>
        internal static Timeline BuildEnvelope(
            BaseElement visual,
            TutorialMotion motion = null,
            float fadeIn = 1f,
            float hold = 5f,
            float fadeOut = 0.5f,
            RGBAColor? peakColor = null,
            int repeat = 1)
        {
            RGBAColor peak = peakColor ?? RGBAColor.solidOpaqueRGBA;
            RGBAColor clear = RGBAColor.MakeRGBA(peak.RedColor, peak.GreenColor, peak.BlueColor, 0f);
            bool holdsForever = hold == TutorialValues.ForeverHold;
            bool loops = repeat == TutorialValues.ForeverRepeat;
            int passes = loops ? 1 : repeat;

            // The cap is per track, not per timeline, so it is the busier of the two.
            int colorFrames = holdsForever ? 2 : 4;
            int motionFrames = motion?.KeyFrameCount ?? 0;
            Timeline timeline = new Timeline()
                .InitWithMaxKeyFramesOnTrack(Math.Max(colorFrames, motionFrames) * passes);

            for (int pass = 0; pass < passes; pass++)
            {
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    clear, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
                timeline.AddKeyFrame(KeyFrame.MakeColor(
                    peak, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, fadeIn));
                if (!holdsForever)
                {
                    timeline.AddKeyFrame(KeyFrame.MakeColor(
                        peak, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, hold));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(
                        clear, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, fadeOut));
                }

                motion?.AddKeyFrames(timeline, visual);
            }

            timeline.SetTimelineLoopType(
                loops ? Timeline.LoopType.TIMELINE_REPLAY : Timeline.LoopType.TIMELINE_NO_LOOP);
            visual.AddTimelinewithID(timeline, 0);
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
            float hold = TutorialValues.ParseHoldDuration(
                node.Attribute("duration")?.Value,
                isText ? 5f : 5.2f,
                source,
                "duration");
            float fadeOut = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("fadeOut")?.Value,
                0.5f,
                source,
                "fadeOut");
            float opacity = TutorialValues.ParseUnitInterval(
                node.Attribute("opacity")?.Value,
                1f,
                source,
                "opacity");
            RGBAColor? color = TutorialValues.ParseColor(node.Attribute("color")?.Value, source, "color");
            float angle = TutorialValues.ParseOptionalFiniteFloat(
                node.Attribute("angle")?.Value,
                0f,
                source,
                "angle");
            float sizeScale = TutorialValues.ParsePositiveFloat(
                node.Attribute("size")?.Value,
                1f,
                source,
                "size");
            float lineHeightScale = TutorialValues.ParsePositiveFloat(
                node.Attribute("lineHeight")?.Value,
                1f,
                source,
                "lineHeight");
            if (!isText)
            {
                // A sign has one glyph-free quad, so type-setting attributes have nothing to act on.
                RejectTextOnlyAttribute(node, "size");
                RejectTextOnlyAttribute(node, "lineHeight");
            }

            // Motion used to hide behind anim="swipe"; it is authored now, so a stale anim has to
            // fail rather than silently drop the animation it used to name.
            string animationValue = node.Attribute("anim")?.Value;
            if (animationValue is not null)
            {
                throw TutorialValues.Invalid(source, "anim", animationValue);
            }

            int repeat = TutorialValues.ParseRepeat(node.Attribute("repeat")?.Value, source, "repeat");
            if (hold == TutorialValues.ForeverHold && repeat != 1)
            {
                throw TutorialValues.Invalid(source, "repeat", node.Attribute("repeat")?.Value);
            }

            float moveDelay = TutorialValues.ParseNonNegativeFloat(
                node.Attribute("moveDelay")?.Value,
                0f,
                source,
                "moveDelay");
            float moveSpeed = TutorialValues.ParsePositiveFloat(
                node.Attribute("moveSpeed")?.Value,
                100f,
                source,
                "moveSpeed");
            // A path on its own still runs through the shared Mover, which loops it forever at a
            // constant speed and independently of the fade - what 17_1 has always done. Timeline
            // motion takes over only once an attribute the mover cannot express is authored, so the
            // two never drive the same prompt at once.
            bool timelineMotion = node.Attribute("path") is not null
                && (node.Attribute("ease") is not null
                    || node.Attribute("moveDelay") is not null
                    || node.Attribute("repeat") is not null);
            TutorialMotion motion = TutorialMotion.Parse(
                timelineMotion ? node.Attribute("path")?.Value : null,
                moveSpeed,
                node.Attribute("ease")?.Value,
                moveDelay,
                hold == TutorialValues.ForeverHold ? float.MaxValue : fadeIn + hold + fadeOut,
                source);

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
                motion,
                repeat,
                opacity,
                color,
                sizeScale,
                lineHeightScale,
                angle);
        }

        private void RejectTextOnlyAttribute(XElement node, string attribute)
        {
            string value = node.Attribute(attribute)?.Value;
            if (value is not null)
            {
                throw TutorialValues.Invalid(source, attribute, value);
            }
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
            // Text reads only the element color's alpha and takes its own RGB from the override, so
            // only a sign tints through the envelope.
            RGBAColor peak = parsed.IsText || parsed.Color is null
                ? RGBAColor.MakeRGBA(1f, 1f, 1f, parsed.Opacity)
                : RGBAColor.MakeRGBA(
                    parsed.Color.Value.RedColor,
                    parsed.Color.Value.GreenColor,
                    parsed.Color.Value.BlueColor,
                    parsed.Opacity);

            visual.color = RGBAColor.MakeRGBA(peak.RedColor, peak.GreenColor, peak.BlueColor, 0f);
            // A path with no timeline attribute travels on the shared mover, whichever visual
            // carries it, so identical XML moves text and signs identically.
            if (parsed.Motion is null)
            {
                switch (visual)
                {
                    case GameObject gameObject when !parsed.IsText:
                        gameObject.ParseMover(node);
                        break;
                    case TutorialText movingText:
                        movingText.ParseMover(node);
                        break;
                    default:
                        break;
                }
            }

            // Applied after the mover so the strictly parsed angle wins over its lenient read, and
            // so a text prompt rotates the same way a sign does.
            visual.rotation = parsed.Angle;

            if (visual is Text text)
            {
                text.sizeScale = parsed.SizeScale;
                text.lineHeightScale = parsed.LineHeightScale;
                text.colorOverride = parsed.Color;

                // Layout ran at the default size when the visual was created, so re-wrap it now that
                // the authored size multiplier is known.
                if (parsed.SizeScale != 1f || parsed.LineHeightScale != 1f)
                {
                    text.SetStringandWidth(text.GetString(), text.wrapWidth);
                }
            }

            _ = BuildEnvelope(
                visual,
                parsed.Motion,
                parsed.FadeIn,
                parsed.Hold,
                parsed.FadeOut,
                peak,
                parsed.Repeat);

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
                0,
                parsed.Opacity,
                parsed.Color,
                parsed.SizeScale,
                parsed.LineHeightScale);
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
            TutorialMotion Motion,
            int Repeat,
            float Opacity,
            RGBAColor? Color,
            float SizeScale,
            float LineHeightScale,
            float Angle);
    }
}
