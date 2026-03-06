using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

using CutTheRope.Framework;

namespace CutTheRope.GameMain
{
    public sealed class FlashXmlAnimationDefinition
    {
        public float StageWidth { get; init; }
        public float StageHeight { get; init; }
        public string TextureResourceName { get; init; } = string.Empty;
        public IReadOnlyList<FlashXmlPartDefinition> Parts { get; init; } = [];
        public IReadOnlyDictionary<int, float> RootTimelines { get; init; } = new Dictionary<int, float>();
    }

    public sealed class FlashXmlPartDefinition
    {
        public string Name { get; init; } = string.Empty;
        public string TextureResourceName { get; init; } = string.Empty;
        public int QuadToDraw { get; init; }
        public float AnchorX { get; init; }
        public float AnchorY { get; init; }
        public float RotationCenterX { get; init; }
        public float RotationCenterY { get; init; }
        public IReadOnlyDictionary<int, FlashXmlTimelineDefinition> Timelines { get; init; } = new Dictionary<int, FlashXmlTimelineDefinition>();
    }

    public sealed class FlashXmlTimelineDefinition
    {
        public int Id { get; init; }
        public IReadOnlyList<FlashXmlFloat2KeyFrame> PositionKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat2KeyFrame> ScaleKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat2KeyFrame> SkewKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat4KeyFrame> ColorKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlActionGroupKeyFrame> ActionKeyFrames { get; init; } = [];
    }

    public sealed class FlashXmlFloat2KeyFrame
    {
        public float X { get; init; }
        public float Y { get; init; }
        public int Interpolation { get; init; }
        public float TimeOffset { get; init; }
    }

    public sealed class FlashXmlFloat4KeyFrame
    {
        public float A { get; init; }
        public float B { get; init; }
        public float C { get; init; }
        public float D { get; init; }
        public int Interpolation { get; init; }
        public float TimeOffset { get; init; }
    }

    public sealed class FlashXmlActionGroupKeyFrame
    {
        public int Interpolation { get; init; }
        public float TimeOffset { get; init; }
        public IReadOnlyList<FlashXmlActionCommand> Actions { get; init; } = [];
    }

    public sealed class FlashXmlActionCommand
    {
        public string Command { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
        public string Param1 { get; init; } = string.Empty;
        public string Param2 { get; init; } = string.Empty;
    }

    public static class FlashXmlImporter
    {
        private const float GroupEpsilon = 0.0001f;

        public static FlashXmlAnimationDefinition ParseFile(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException("XML path is required.", nameof(xmlPath));
            }

            XElement root = XDocument.Load(xmlPath).Root
                ?? throw new InvalidOperationException("Flash XML is missing a root element.");
            if (root.Name.LocalName != "FlashAnimation")
            {
                throw new InvalidOperationException($"Unexpected root element '{root.Name.LocalName}'.");
            }

            List<FlashXmlPartDefinition> parts = [];
            foreach (XElement imageNode in root.Elements("Image"))
            {
                parts.Add(ParseImageNode(imageNode));
            }

            Dictionary<int, float> rootTimelines = [];
            foreach (XElement timelineNode in root.Elements("Timeline"))
            {
                int timelineId = ParseInt(timelineNode.Attribute("ID")?.Value);
                string actionTrack = timelineNode.Element("Action")?.Value ?? string.Empty;
                IReadOnlyList<FlashXmlActionGroupKeyFrame> groupedActions = ParseActionTrack(actionTrack);

                float duration = 0f;
                for (int i = 0; i < groupedActions.Count; i++)
                {
                    duration += groupedActions[i].TimeOffset;
                }

                rootTimelines[timelineId] = duration;
            }

            return new FlashXmlAnimationDefinition
            {
                StageWidth = ParseFloat(root.Attribute("width")?.Value),
                StageHeight = ParseFloat(root.Attribute("height")?.Value),
                TextureResourceName = Resources.Img.CharAnimationsSmooth,
                Parts = parts,
                RootTimelines = rootTimelines
            };
        }

        private static FlashXmlPartDefinition ParseImageNode(XElement imageNode)
        {
            Dictionary<int, FlashXmlTimelineDefinition> timelines = [];
            foreach (XElement timelineNode in imageNode.Elements("Timeline"))
            {
                FlashXmlTimelineDefinition timeline = ParseImageTimeline(timelineNode);
                timelines[timeline.Id] = timeline;
            }

            return new FlashXmlPartDefinition
            {
                Name = imageNode.Attribute("name")?.Value ?? string.Empty,
                TextureResourceName = Resources.Img.CharAnimationsSmooth,
                QuadToDraw = ParseInt(imageNode.Attribute("quadToDraw")?.Value),
                AnchorX = ParseFloat(imageNode.Attribute("anchorX")?.Value),
                AnchorY = ParseFloat(imageNode.Attribute("anchorY")?.Value),
                RotationCenterX = ParseFloat(imageNode.Attribute("rotationCenterX")?.Value),
                RotationCenterY = ParseFloat(imageNode.Attribute("rotationCenterY")?.Value),
                Timelines = timelines
            };
        }

        private static FlashXmlTimelineDefinition ParseImageTimeline(XElement timelineNode)
        {
            return new FlashXmlTimelineDefinition
            {
                Id = ParseInt(timelineNode.Attribute("ID")?.Value),
                PositionKeyFrames = ParseFloat2Track(timelineNode.Element("Pos")?.Value, expectedArity: 2),
                ScaleKeyFrames = ParseFloat2Track(timelineNode.Element("Scale")?.Value, expectedArity: 2),
                SkewKeyFrames = ParseFloat2Track(timelineNode.Element("Skew")?.Value, expectedArity: 2),
                ColorKeyFrames = ParseFloat4Track(timelineNode.Element("Color")?.Value),
                ActionKeyFrames = ParseActionTrack(timelineNode.Element("Action")?.Value)
            };
        }

        private static IReadOnlyList<FlashXmlFloat2KeyFrame> ParseFloat2Track(string rawTrack, int expectedArity)
        {
            if (string.IsNullOrWhiteSpace(rawTrack))
            {
                return [];
            }

            List<FlashXmlFloat2KeyFrame> keyFrames = [];
            string[] tokens = rawTrack.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                ParsedToken parsed = ParseToken(tokens[i]);
                string[] values = parsed.Payload.Split(',', StringSplitOptions.TrimEntries);
                if (values.Length != expectedArity)
                {
                    continue;
                }

                keyFrames.Add(new FlashXmlFloat2KeyFrame
                {
                    X = ParseFloat(values[0]),
                    Y = ParseFloat(values[1]),
                    Interpolation = parsed.Interpolation,
                    TimeOffset = parsed.TimeOffset
                });
            }

            return keyFrames;
        }

        private static IReadOnlyList<FlashXmlFloat4KeyFrame> ParseFloat4Track(string rawTrack)
        {
            if (string.IsNullOrWhiteSpace(rawTrack))
            {
                return [];
            }

            List<FlashXmlFloat4KeyFrame> keyFrames = [];
            string[] tokens = rawTrack.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                ParsedToken parsed = ParseToken(tokens[i]);
                string[] values = parsed.Payload.Split(',', StringSplitOptions.TrimEntries);
                if (values.Length != 4)
                {
                    continue;
                }

                keyFrames.Add(new FlashXmlFloat4KeyFrame
                {
                    A = ParseFloat(values[0]),
                    B = ParseFloat(values[1]),
                    C = ParseFloat(values[2]),
                    D = ParseFloat(values[3]),
                    Interpolation = parsed.Interpolation,
                    TimeOffset = parsed.TimeOffset
                });
            }

            return keyFrames;
        }

        private static IReadOnlyList<FlashXmlActionGroupKeyFrame> ParseActionTrack(string rawTrack)
        {
            if (string.IsNullOrWhiteSpace(rawTrack))
            {
                return [];
            }

            string[] tokens = rawTrack.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            List<FlashXmlActionGroupKeyFrame> groups = [];
            FlashXmlActionGroupBuilder current = null;

            for (int i = 0; i < tokens.Length; i++)
            {
                ParsedToken parsed = ParseToken(tokens[i]);
                string[] actionParts = parsed.Payload.Split(',', StringSplitOptions.TrimEntries);
                if (actionParts.Length < 4)
                {
                    continue;
                }

                if (current == null || MathF.Abs(current.TimeOffset - parsed.TimeOffset) > GroupEpsilon)
                {
                    if (current != null)
                    {
                        groups.Add(current.Build());
                    }

                    current = new FlashXmlActionGroupBuilder
                    {
                        Interpolation = parsed.Interpolation,
                        TimeOffset = parsed.TimeOffset
                    };
                }

                current.Actions.Add(new FlashXmlActionCommand
                {
                    Command = actionParts[0],
                    Target = actionParts[1],
                    Param1 = actionParts[2],
                    Param2 = actionParts[3]
                });
            }

            if (current != null)
            {
                groups.Add(current.Build());
            }

            return groups;
        }

        private static ParsedToken ParseToken(string token)
        {
            int interpolationStart = token.LastIndexOf('(');
            int interpolationEnd = token.LastIndexOf(")@", StringComparison.Ordinal);
            if (interpolationStart < 0 || interpolationEnd <= interpolationStart)
            {
                throw new FormatException($"Invalid keyframe token: '{token}'.");
            }

            string payload = token[..interpolationStart];
            string interpolationRaw = token[(interpolationStart + 1)..interpolationEnd];
            string timeRaw = token[(interpolationEnd + 2)..];

            return new ParsedToken(payload, ParseInt(interpolationRaw), ParseFloat(timeRaw));
        }

        private static int ParseInt(string raw)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
        }

        private static float ParseFloat(string raw)
        {
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : 0f;
        }

        private readonly struct ParsedToken(string payload, int interpolation, float timeOffset)
        {
            public string Payload { get; } = payload;
            public int Interpolation { get; } = interpolation;
            public float TimeOffset { get; } = timeOffset;
        }

        private sealed class FlashXmlActionGroupBuilder
        {
            public int Interpolation { get; init; }
            public float TimeOffset { get; init; }
            public List<FlashXmlActionCommand> Actions { get; } = [];

            public FlashXmlActionGroupKeyFrame Build()
            {
                return new FlashXmlActionGroupKeyFrame
                {
                    Interpolation = Interpolation,
                    TimeOffset = TimeOffset,
                    Actions = [.. Actions]
                };
            }
        }
    }
}
