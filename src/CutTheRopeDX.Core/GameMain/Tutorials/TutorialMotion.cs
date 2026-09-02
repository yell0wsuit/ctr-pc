using System.Collections.Generic;
using System.Globalization;
using System.IO;

using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Easing applied to one leg of an authored tutorial path.</summary>
    internal enum TutorialEase
    {
        /// <summary>Constant speed across the leg.</summary>
        None,

        /// <summary>Slow start.</summary>
        In,

        /// <summary>Slow end.</summary>
        Out,
    }

    /// <summary>
    /// Authored travel for one tutorial prompt, expressed as timeline keyframes rather than a
    /// <c>Mover</c>. Prompts need eased legs, a pause before travel starts, and a pass count, none
    /// of which a mover offers, and the color envelope they play alongside is already a timeline.
    /// </summary>
    internal sealed class TutorialMotion
    {
        private readonly IReadOnlyList<(float X, float Y)> offsets;
        private readonly IReadOnlyList<TutorialEase> eases;
        private readonly IReadOnlyList<float> legSeconds;
        private readonly float delaySeconds;
        private readonly float trailingSeconds;

        private TutorialMotion(
            IReadOnlyList<(float X, float Y)> offsets,
            IReadOnlyList<TutorialEase> eases,
            IReadOnlyList<float> legSeconds,
            float delaySeconds,
            float trailingSeconds)
        {
            this.offsets = offsets;
            this.eases = eases;
            this.legSeconds = legSeconds;
            this.delaySeconds = delaySeconds;
            this.trailingSeconds = trailingSeconds;
        }

        /// <summary>Number of position keyframes one pass of this motion contributes.</summary>
        internal int KeyFrameCount =>
            1 + (delaySeconds > 0f ? 1 : 0) + offsets.Count + (trailingSeconds > 0f ? 1 : 0);

        /// <summary>Total seconds one pass of travel occupies, including its leading delay.</summary>
        internal float TravelSeconds
        {
            get
            {
                float total = delaySeconds;
                foreach (float seconds in legSeconds)
                {
                    total += seconds;
                }

                return total;
            }
        }

        /// <summary>
        /// Parses authored motion. Each <c>path</c> pair is an offset from the prompt's own
        /// position, matching how a mover path has always been read, so travel is authored the same
        /// way whether it moves a star or a tutorial sign.
        /// </summary>
        /// <param name="path">Comma-separated <c>dx,dy</c> pairs, or <see langword="null"/>.</param>
        /// <param name="moveSpeed">Travel speed in world units per second.</param>
        /// <param name="ease">Per-leg easing list, or one value applied to every leg.</param>
        /// <param name="moveDelay">Seconds into each pass before travel starts.</param>
        /// <param name="passSeconds">Seconds one pass lasts, from the color envelope.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <returns>The parsed motion, or <see langword="null"/> when no path is authored.</returns>
        /// <exception cref="InvalidDataException">Thrown when the path, easing, or timing is invalid.</exception>
        internal static TutorialMotion Parse(
            string path,
            float moveSpeed,
            string ease,
            float moveDelay,
            float passSeconds,
            string source)
        {
            if (path is null)
            {
                return null;
            }

            List<(float X, float Y)> offsets = ParseOffsets(path, source);
            List<TutorialEase> eases = ParseEases(ease, offsets.Count, source);

            List<float> legSeconds = [];
            float previousX = 0f;
            float previousY = 0f;
            foreach ((float x, float y) in offsets)
            {
                float distance = VectLength(Vect(x - previousX, y - previousY));
                legSeconds.Add(distance / moveSpeed);
                previousX = x;
                previousY = y;
            }

            TutorialMotion motion = new(offsets, eases, legSeconds, moveDelay, 0f);
            float trailing = passSeconds - motion.TravelSeconds;
            return trailing < 0f
                ? throw TutorialValues.Invalid(
                    source,
                    "path",
                    $"travel of {motion.TravelSeconds:0.###}s exceeds the {passSeconds:0.###}s pass")
                : new TutorialMotion(offsets, eases, legSeconds, moveDelay, trailing);
        }

        /// <summary>Appends one pass of position keyframes anchored at the visual's position.</summary>
        /// <param name="timeline">Timeline to append to.</param>
        /// <param name="visual">Visual whose position anchors the path.</param>
        internal void AddKeyFrames(Timeline timeline, BaseElement visual)
        {
            timeline.AddKeyFrame(KeyFrame.MakePos(
                visual.x, visual.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            if (delaySeconds > 0f)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x,
                    visual.y,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    delaySeconds));
            }

            for (int leg = 0; leg < offsets.Count; leg++)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x + offsets[leg].X,
                    visual.y + offsets[leg].Y,
                    Transition(eases[leg]),
                    legSeconds[leg]));
            }

            if (trailingSeconds > 0f)
            {
                (float lastX, float lastY) = offsets[^1];
                timeline.AddKeyFrame(KeyFrame.MakePos(
                    visual.x + lastX,
                    visual.y + lastY,
                    KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                    trailingSeconds));
            }
        }

        private static KeyFrame.TransitionType Transition(TutorialEase ease)
        {
            return ease switch
            {
                TutorialEase.In => KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN,
                TutorialEase.Out => KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
                TutorialEase.None => KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                _ => KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
            };
        }

        private static List<(float X, float Y)> ParseOffsets(string path, string source)
        {
            string trimmed = path.EndsWith(',') ? path[..^1] : path;
            string[] parts = trimmed.Split(',');
            if (parts.Length == 0 || parts.Length % 2 != 0)
            {
                throw TutorialValues.Invalid(source, "path", path);
            }

            List<(float X, float Y)> offsets = [];
            for (int pair = 0; pair < parts.Length; pair += 2)
            {
                offsets.Add((Coordinate(parts[pair], path, source), Coordinate(parts[pair + 1], path, source)));
            }

            return offsets;
        }

        private static float Coordinate(string value, string path, string source)
        {
            return value.Length == 0
                ? 0f
                : float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
                    && float.IsFinite(parsed)
                        ? parsed
                        : throw TutorialValues.Invalid(source, "path", path);
        }

        private static List<TutorialEase> ParseEases(string ease, int legs, string source)
        {
            List<TutorialEase> eases = [];
            if (ease is null)
            {
                for (int leg = 0; leg < legs; leg++)
                {
                    eases.Add(TutorialEase.None);
                }

                return eases;
            }

            string[] parts = ease.Split(',');
            foreach (string part in parts)
            {
                eases.Add(part switch
                {
                    "none" => TutorialEase.None,
                    "in" => TutorialEase.In,
                    "out" => TutorialEase.Out,
                    _ => throw TutorialValues.Invalid(source, "ease", ease),
                });
            }

            if (parts.Length == 1)
            {
                for (int leg = 1; leg < legs; leg++)
                {
                    eases.Add(eases[0]);
                }
            }

            return eases.Count == legs ? eases : throw TutorialValues.Invalid(source, "ease", ease);
        }
    }
}
