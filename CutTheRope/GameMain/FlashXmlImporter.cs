using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    public sealed class FlashXmlAnimationDefinition
    {
        public float StageWidth { get; init; }
        public float StageHeight { get; init; }
        public string TextureResourceName { get; init; } = string.Empty;
        public IReadOnlyList<FlashXmlPartDefinition> Parts { get; init; } = [];
        public IReadOnlyDictionary<int, float> RootTimelines { get; init; } = new Dictionary<int, float>();
        public IReadOnlyDictionary<int, FlashXmlRootTimelineDefinition> RootTimelineDefinitions { get; init; }
            = new Dictionary<int, FlashXmlRootTimelineDefinition>();
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
        public IReadOnlyList<int> EmptyTimelineIds { get; init; } = [];
    }

    public sealed class FlashXmlRootTimelineDefinition
    {
        public int Id { get; init; }
        public float DurationSeconds { get; init; }
        public IReadOnlyList<FlashXmlActionGroupKeyFrame> ActionKeyFrames { get; init; } = [];
    }

    public sealed class FlashXmlTimelineDefinition
    {
        public int Id { get; init; }
        public IReadOnlyList<FlashXmlFloat2KeyFrame> PositionKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat2KeyFrame> ScaleKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat1KeyFrame> RotationKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat2KeyFrame> SkewKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlFloat4KeyFrame> ColorKeyFrames { get; init; } = [];
        public IReadOnlyList<FlashXmlActionGroupKeyFrame> ActionKeyFrames { get; init; } = [];
    }

    public sealed class FlashXmlFloat1KeyFrame
    {
        public float Value { get; init; }
        public int Interpolation { get; init; }
        public float TimeOffset { get; init; }
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

        private static readonly ConcurrentDictionary<string, Lazy<FlashXmlAnimationDefinition>> parseCache = new();

        public static FlashXmlAnimationDefinition ParseFile(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException("XML path is required.", nameof(xmlPath));
            }

            Lazy<FlashXmlAnimationDefinition> cachedDefinition = parseCache.GetOrAdd(
                xmlPath,
                static path => new Lazy<FlashXmlAnimationDefinition>(
                    () => ParseFileCore(path),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return cachedDefinition.Value;
            }
            catch
            {
                _ = parseCache.TryRemove(xmlPath, out _);
                throw;
            }
        }

        private static FlashXmlAnimationDefinition ParseFileCore(string xmlPath)
        {
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
            Dictionary<int, FlashXmlRootTimelineDefinition> rootTimelineDefinitions = [];
            foreach (XElement timelineNode in root.Elements("Timeline"))
            {
                int timelineId = ParseIntOrZero(timelineNode.Attribute("ID")?.Value);
                string actionTrack = timelineNode.Element("Action")?.Value ?? string.Empty;
                List<FlashXmlActionGroupKeyFrame> groupedActions = ParseActionTrack(actionTrack);

                float duration = 0f;
                for (int i = 0; i < groupedActions.Count; i++)
                {
                    duration += groupedActions[i].TimeOffset;
                }

                rootTimelines[timelineId] = duration;
                rootTimelineDefinitions[timelineId] = new FlashXmlRootTimelineDefinition
                {
                    Id = timelineId,
                    DurationSeconds = duration,
                    ActionKeyFrames = groupedActions
                };
            }

            return new FlashXmlAnimationDefinition
            {
                StageWidth = ParseFloatOrZero(root.Attribute("width")?.Value),
                StageHeight = ParseFloatOrZero(root.Attribute("height")?.Value),
                TextureResourceName = ResolveTextureResourceName(root.Attribute("src")?.Value, "animation"),
                Parts = parts,
                RootTimelines = rootTimelines,
                RootTimelineDefinitions = rootTimelineDefinitions
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
                TextureResourceName = ResolveTextureResourceName(imageNode.Attribute("src")?.Value, "image"),
                QuadToDraw = ParseIntOrZero(imageNode.Attribute("quadToDraw")?.Value),
                AnchorX = ParseFloatOrZero(imageNode.Attribute("anchorX")?.Value),
                AnchorY = ParseFloatOrZero(imageNode.Attribute("anchorY")?.Value),
                RotationCenterX = ParseFloatOrZero(imageNode.Attribute("rotationCenterX")?.Value),
                RotationCenterY = ParseFloatOrZero(imageNode.Attribute("rotationCenterY")?.Value),
                Timelines = timelines,
                EmptyTimelineIds = ParseEmptyTimelineIds(imageNode.Element("EmptyTimelines")?.Value)
            };
        }

        private static List<int> ParseEmptyTimelineIds(string rawEmptyTimelines)
        {
            if (string.IsNullOrWhiteSpace(rawEmptyTimelines))
            {
                return [];
            }

            List<int> timelineIds = [];
            string[] tokens = rawEmptyTimelines.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                timelineIds.Add(ParseIntOrZero(tokens[i]));
            }

            return timelineIds;
        }

        private static string ResolveTextureResourceName(string rawSourceId, string sourceKind)
        {
            rawSourceId = string.IsNullOrWhiteSpace(rawSourceId)
                ? throw new InvalidOperationException($"Flash XML {sourceKind} src is missing.")
                : rawSourceId;

            return Resources.IsImage(rawSourceId)
                ? rawSourceId
                : throw new InvalidOperationException($"Unsupported Flash XML {sourceKind} src '{rawSourceId}'.");
        }

        private static FlashXmlTimelineDefinition ParseImageTimeline(XElement timelineNode)
        {
            return new FlashXmlTimelineDefinition
            {
                Id = ParseIntOrZero(timelineNode.Attribute("ID")?.Value),
                PositionKeyFrames = ParseFloat2Track(timelineNode.Element("Pos")?.Value, expectedArity: 2),
                ScaleKeyFrames = ParseFloat2Track(timelineNode.Element("Scale")?.Value, expectedArity: 2),
                RotationKeyFrames = ParseFloat1Track(timelineNode.Element("Rot")?.Value),
                SkewKeyFrames = ParseFloat2Track(timelineNode.Element("Skew")?.Value, expectedArity: 2),
                ColorKeyFrames = ParseFloat4Track(timelineNode.Element("Color")?.Value),
                ActionKeyFrames = ParseActionTrack(timelineNode.Element("Action")?.Value)
            };
        }

        private static List<FlashXmlFloat1KeyFrame> ParseFloat1Track(string rawTrack)
        {
            if (string.IsNullOrWhiteSpace(rawTrack))
            {
                return [];
            }

            List<FlashXmlFloat1KeyFrame> keyFrames = [];
            string[] tokens = rawTrack.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                ParsedToken parsed = ParseToken(tokens[i]);
                keyFrames.Add(new FlashXmlFloat1KeyFrame
                {
                    Value = ParseFloatOrZero(parsed.Payload),
                    Interpolation = parsed.Interpolation,
                    TimeOffset = parsed.TimeOffset
                });
            }

            return keyFrames;
        }

        private static List<FlashXmlFloat2KeyFrame> ParseFloat2Track(string rawTrack, int expectedArity)
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
                    X = ParseFloatOrZero(values[0]),
                    Y = ParseFloatOrZero(values[1]),
                    Interpolation = parsed.Interpolation,
                    TimeOffset = parsed.TimeOffset
                });
            }

            return keyFrames;
        }

        private static List<FlashXmlFloat4KeyFrame> ParseFloat4Track(string rawTrack)
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
                    A = ParseFloatOrZero(values[0]),
                    B = ParseFloatOrZero(values[1]),
                    C = ParseFloatOrZero(values[2]),
                    D = ParseFloatOrZero(values[3]),
                    Interpolation = parsed.Interpolation,
                    TimeOffset = parsed.TimeOffset
                });
            }

            return keyFrames;
        }

        private static List<FlashXmlActionGroupKeyFrame> ParseActionTrack(string rawTrack)
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

                // Flash action export encodes one keyframe as:
                // first action has the keyframe delay, following actions use @0.
                // Start a new group only when we see a positive delay token.
                if (current == null || parsed.TimeOffset > GroupEpsilon)
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

            return new ParsedToken(payload, ParseIntOrZero(interpolationRaw), ParseFloatOrZero(timeRaw));
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
