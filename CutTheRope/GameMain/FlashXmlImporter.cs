using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Parsed Flash XML animation data, including stage size, texture resources, parts, and root timelines.
    /// </summary>
    public sealed class FlashXmlAnimationDefinition
    {
        /// <summary>Width of the Flash stage in Flash point units.</summary>
        public float StageWidth { get; init; }

        /// <summary>Height of the Flash stage in Flash point units.</summary>
        public float StageHeight { get; init; }

        /// <summary>Texture resource name referenced by the animation root.</summary>
        public string TextureResourceName { get; init; } = string.Empty;

        /// <summary>Image parts that make up the animation.</summary>
        public IReadOnlyList<FlashXmlPartDefinition> Parts { get; init; } = [];

        /// <summary>Root timeline durations keyed by Flash timeline ID.</summary>
        public IReadOnlyDictionary<int, float> RootTimelines { get; init; } = new Dictionary<int, float>();

        /// <summary>Root timeline definitions keyed by Flash timeline ID.</summary>
        public IReadOnlyDictionary<int, FlashXmlRootTimelineDefinition> RootTimelineDefinitions { get; init; }
            = new Dictionary<int, FlashXmlRootTimelineDefinition>();
    }

    /// <summary>
    /// Parsed Flash XML image part and the timelines attached to that part.
    /// </summary>
    public sealed class FlashXmlPartDefinition
    {
        /// <summary>Name used by action keyframes to target this part.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Texture resource name that contains this part's quad.</summary>
        public string TextureResourceName { get; init; } = string.Empty;

        /// <summary>Atlas quad index to draw for the part's default pose.</summary>
        public int QuadToDraw { get; init; }

        /// <summary>Custom anchor X coordinate exported by Flash.</summary>
        public float AnchorX { get; init; }

        /// <summary>Custom anchor Y coordinate exported by Flash.</summary>
        public float AnchorY { get; init; }

        /// <summary>Rotation center X coordinate exported by Flash.</summary>
        public float RotationCenterX { get; init; }

        /// <summary>Rotation center Y coordinate exported by Flash.</summary>
        public float RotationCenterY { get; init; }

        /// <summary>Part timelines keyed by Flash timeline ID.</summary>
        public IReadOnlyDictionary<int, FlashXmlTimelineDefinition> Timelines { get; init; } = new Dictionary<int, FlashXmlTimelineDefinition>();

        /// <summary>Timeline IDs exported as empty timelines for this part.</summary>
        public IReadOnlyList<int> EmptyTimelineIds { get; init; } = [];
    }

    /// <summary>
    /// Root-level timeline used as a driver for timeline completion and action cadence.
    /// </summary>
    public sealed class FlashXmlRootTimelineDefinition
    {
        /// <summary>Flash timeline ID.</summary>
        public int Id { get; init; }

        /// <summary>Total timeline duration in seconds.</summary>
        public float DurationSeconds { get; init; }

        /// <summary>Action keyframes emitted on the root timeline.</summary>
        public IReadOnlyList<FlashXmlActionGroupKeyFrame> ActionKeyFrames { get; init; } = [];
    }

    /// <summary>
    /// Parsed per-part Flash timeline tracks.
    /// </summary>
    public sealed class FlashXmlTimelineDefinition
    {
        /// <summary>Flash timeline ID.</summary>
        public int Id { get; init; }

        /// <summary>Position keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlFloat2KeyFrame> PositionKeyFrames { get; init; } = [];

        /// <summary>Scale keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlFloat2KeyFrame> ScaleKeyFrames { get; init; } = [];

        /// <summary>Rotation keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlFloat1KeyFrame> RotationKeyFrames { get; init; } = [];

        /// <summary>Skew keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlFloat2KeyFrame> SkewKeyFrames { get; init; } = [];

        /// <summary>Color keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlFloat4KeyFrame> ColorKeyFrames { get; init; } = [];

        /// <summary>Action keyframes for this timeline.</summary>
        public IReadOnlyList<FlashXmlActionGroupKeyFrame> ActionKeyFrames { get; init; } = [];
    }

    /// <summary>
    /// Single-value Flash keyframe.
    /// </summary>
    public sealed class FlashXmlFloat1KeyFrame
    {
        /// <summary>Keyframe value.</summary>
        public float Value { get; init; }

        /// <summary>Flash interpolation code.</summary>
        public int Interpolation { get; init; }

        /// <summary>Delay from the previous keyframe in seconds.</summary>
        public float TimeOffset { get; init; }
    }

    /// <summary>
    /// Two-value Flash keyframe.
    /// </summary>
    public sealed class FlashXmlFloat2KeyFrame
    {
        /// <summary>First keyframe value.</summary>
        public float X { get; init; }

        /// <summary>Second keyframe value.</summary>
        public float Y { get; init; }

        /// <summary>Flash interpolation code.</summary>
        public int Interpolation { get; init; }

        /// <summary>Delay from the previous keyframe in seconds.</summary>
        public float TimeOffset { get; init; }
    }

    /// <summary>
    /// Four-value Flash keyframe.
    /// </summary>
    public sealed class FlashXmlFloat4KeyFrame
    {
        /// <summary>First keyframe value.</summary>
        public float A { get; init; }

        /// <summary>Second keyframe value.</summary>
        public float B { get; init; }

        /// <summary>Third keyframe value.</summary>
        public float C { get; init; }

        /// <summary>Fourth keyframe value.</summary>
        public float D { get; init; }

        /// <summary>Flash interpolation code.</summary>
        public int Interpolation { get; init; }

        /// <summary>Delay from the previous keyframe in seconds.</summary>
        public float TimeOffset { get; init; }
    }

    /// <summary>
    /// Group of Flash XML actions that should run on the same keyframe.
    /// </summary>
    public sealed class FlashXmlActionGroupKeyFrame
    {
        /// <summary>Flash interpolation code for the action keyframe.</summary>
        public int Interpolation { get; init; }

        /// <summary>Delay from the previous keyframe in seconds.</summary>
        public float TimeOffset { get; init; }

        /// <summary>Actions to execute on the keyframe.</summary>
        public IReadOnlyList<FlashXmlActionCommand> Actions { get; init; } = [];
    }

    /// <summary>
    /// Flash XML action command exported in an action track.
    /// </summary>
    public sealed class FlashXmlActionCommand
    {
        /// <summary>Flash action command identifier.</summary>
        public string Command { get; init; } = string.Empty;

        /// <summary>Action target name, or self for the current part.</summary>
        public string Target { get; init; } = string.Empty;

        /// <summary>First exported action parameter.</summary>
        public string Param1 { get; init; } = string.Empty;

        /// <summary>Second exported action parameter.</summary>
        public string Param2 { get; init; } = string.Empty;
    }

    /// <summary>
    /// Parser for Flash XML animation exports.
    /// </summary>
    public static class FlashXmlImporter
    {
        /// <summary>
        /// Time offset threshold used to decide when action tokens belong to the same keyframe group.
        /// </summary>
        private const float GroupEpsilon = 0.0001f;

        /// <summary>
        /// Process-local cache of parsed Flash XML animation definitions keyed by XML path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<FlashXmlAnimationDefinition>> parseCache = new();

        /// <summary>
        /// Parses a Flash XML animation file, using a process-local cache for repeated paths.
        /// </summary>
        /// <param name="xmlPath">Absolute or relative path to the Flash XML file.</param>
        /// <returns>The parsed Flash XML animation definition.</returns>
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

        /// <summary>
        /// Parses a Flash XML animation file without consulting the parse cache.
        /// </summary>
        /// <param name="xmlPath">Absolute or relative path to the Flash XML file.</param>
        /// <returns>The parsed Flash XML animation definition.</returns>
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

        /// <summary>
        /// Parses an image node into a part definition.
        /// </summary>
        /// <param name="imageNode">Flash XML image element to parse.</param>
        /// <returns>The parsed image part definition.</returns>
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

        /// <summary>
        /// Parses the semicolon-separated list of exported empty timeline IDs.
        /// </summary>
        /// <param name="rawEmptyTimelines">Raw empty timeline ID string from the XML node.</param>
        /// <returns>The parsed timeline IDs, or an empty list when no IDs are present.</returns>
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

        /// <summary>
        /// Resolves and validates a texture resource name exported by Flash XML.
        /// </summary>
        /// <param name="rawSourceId">Raw source identifier from the XML attribute.</param>
        /// <param name="sourceKind">Source kind used in error messages.</param>
        /// <returns>The validated texture resource name.</returns>
        private static string ResolveTextureResourceName(string rawSourceId, string sourceKind)
        {
            rawSourceId = string.IsNullOrWhiteSpace(rawSourceId)
                ? throw new InvalidOperationException($"Flash XML {sourceKind} src is missing.")
                : rawSourceId;

            return Resources.IsImage(rawSourceId)
                ? rawSourceId
                : throw new InvalidOperationException($"Unsupported Flash XML {sourceKind} src '{rawSourceId}'.");
        }

        /// <summary>
        /// Parses an image timeline node into per-track keyframe lists.
        /// </summary>
        /// <param name="timelineNode">Flash XML timeline element to parse.</param>
        /// <returns>The parsed image timeline definition.</returns>
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

        /// <summary>
        /// Parses a single-value Flash track into keyframes.
        /// </summary>
        /// <param name="rawTrack">Raw track string from the XML element.</param>
        /// <returns>The parsed single-value keyframes, or an empty list when the track is empty.</returns>
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

        /// <summary>
        /// Parses a two-value Flash track into keyframes when each token has the expected arity.
        /// </summary>
        /// <param name="rawTrack">Raw track string from the XML element.</param>
        /// <param name="expectedArity">Number of comma-separated values expected in each token.</param>
        /// <returns>The parsed two-value keyframes, or an empty list when the track is empty.</returns>
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

        /// <summary>
        /// Parses a four-value Flash track into keyframes.
        /// </summary>
        /// <param name="rawTrack">Raw track string from the XML element.</param>
        /// <returns>The parsed four-value keyframes, or an empty list when the track is empty.</returns>
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

        /// <summary>
        /// Parses a Flash action track and groups actions that belong to the same keyframe.
        /// </summary>
        /// <param name="rawTrack">Raw action track string from the XML element.</param>
        /// <returns>The parsed action keyframe groups, or an empty list when the track is empty.</returns>
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

        /// <summary>
        /// Parses a Flash keyframe token into payload, interpolation, and time offset parts.
        /// </summary>
        /// <param name="token">Raw token to parse.</param>
        /// <returns>The parsed token parts.</returns>
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

        /// <summary>
        /// Parsed payload, interpolation, and timing data from a Flash track token.
        /// </summary>
        /// <param name="payload">Payload before the interpolation marker.</param>
        /// <param name="interpolation">Flash interpolation code.</param>
        /// <param name="timeOffset">Delay from the previous keyframe in seconds.</param>
        private readonly struct ParsedToken(string payload, int interpolation, float timeOffset)
        {
            /// <summary>Payload before the interpolation marker.</summary>
            public string Payload { get; } = payload;

            /// <summary>Flash interpolation code.</summary>
            public int Interpolation { get; } = interpolation;

            /// <summary>Delay from the previous keyframe in seconds.</summary>
            public float TimeOffset { get; } = timeOffset;
        }

        /// <summary>
        /// Mutable builder for action groups that are finalized as immutable keyframes.
        /// </summary>
        private sealed class FlashXmlActionGroupBuilder
        {
            /// <summary>Flash interpolation code for the action keyframe.</summary>
            public int Interpolation { get; init; }

            /// <summary>Delay from the previous keyframe in seconds.</summary>
            public float TimeOffset { get; init; }

            /// <summary>Actions accumulated for the keyframe.</summary>
            public List<FlashXmlActionCommand> Actions { get; } = [];

            /// <summary>
            /// Creates an immutable action keyframe from the accumulated actions.
            /// </summary>
            /// <returns>The finalized Flash XML action group keyframe.</returns>
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
