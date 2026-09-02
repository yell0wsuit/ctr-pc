using System.IO;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Selects which active candy body may satisfy a tutorial trigger.</summary>
    internal enum TutorialSubject
    {
        /// <summary>Any eligible candy body.</summary>
        Any,
        /// <summary>Any active body owned by the primary authored candy.</summary>
        Primary,
        /// <summary>The left half of a split candy.</summary>
        Left,
        /// <summary>The right half of a split candy.</summary>
        Right,
    }

    /// <summary>Parses the closed tutorial subject vocabulary.</summary>
    internal static class TutorialSubjects
    {
        /// <summary>Parses an exact, case-sensitive XML subject name.</summary>
        /// <param name="value">Authored value, or <see langword="null"/> for <see cref="TutorialSubject.Any"/>.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <param name="attribute">Attribute name used in validation errors.</param>
        /// <returns>The matching tutorial subject.</returns>
        /// <exception cref="InvalidDataException">Thrown when <paramref name="value"/> is not in the closed vocabulary.</exception>
        internal static TutorialSubject Parse(string value, string source, string attribute)
        {
            return value switch
            {
                null or "any" => TutorialSubject.Any,
                "primary" => TutorialSubject.Primary,
                "left" => TutorialSubject.Left,
                "right" => TutorialSubject.Right,
                _ => throw TutorialValues.Invalid(source, attribute, value),
            };
        }
    }

    /// <summary>Defines a half-open tutorial trigger rectangle in world coordinates.</summary>
    /// <param name="X">Left edge.</param>
    /// <param name="Y">Top edge.</param>
    /// <param name="Width">Positive width.</param>
    /// <param name="Height">Positive height.</param>
    internal readonly record struct TutorialArea(float X, float Y, float Width, float Height)
    {
        /// <summary>Parses four finite comma-separated area components.</summary>
        /// <param name="value">Authored <c>x,y,width,height</c> value.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <param name="attribute">Attribute name used in validation errors.</param>
        /// <returns>The parsed area.</returns>
        /// <exception cref="InvalidDataException">Thrown when a component is invalid or a dimension is not positive.</exception>
        internal static TutorialArea Parse(string value, string source, string attribute)
        {
            string[] parts = value?.Split(',');
            if (parts is null || parts.Length != 4)
            {
                throw TutorialValues.Invalid(source, attribute, value);
            }

            float x = TutorialValues.ParseFiniteFloat(parts[0], source, attribute);
            float y = TutorialValues.ParseFiniteFloat(parts[1], source, attribute);
            float width = TutorialValues.ParseFiniteFloat(parts[2], source, attribute);
            float height = TutorialValues.ParseFiniteFloat(parts[3], source, attribute);
            return width > 0f && height > 0f
                ? new TutorialArea(x, y, width, height)
                : throw TutorialValues.Invalid(source, attribute, value);
        }

        /// <summary>Tests whether a point lies inside the half-open area.</summary>
        /// <param name="point">World-space point to test.</param>
        /// <returns><see langword="true"/> when the point is inside; otherwise, <see langword="false"/>.</returns>
        internal bool Contains(Vector point)
        {
            return point.X >= X
                && point.X < X + Width
                && point.Y >= Y
                && point.Y < Y + Height;
        }
    }

    /// <summary>Immutable event, area, and candy-subject conditions for one tutorial prompt.</summary>
    /// <param name="Event">Named condition that can fire the prompt.</param>
    /// <param name="Area">Optional world-space containment requirement.</param>
    /// <param name="Subject">Candy body selection rule.</param>
    internal sealed record TutorialTrigger(
        TutorialEvent Event,
        TutorialArea? Area,
        TutorialSubject Subject)
    {
        /// <summary>Parses and semantically validates an authored tutorial trigger.</summary>
        /// <param name="showOn">Optional event name.</param>
        /// <param name="inArea">Optional map-space area.</param>
        /// <param name="subject">Optional candy subject.</param>
        /// <param name="twoParts">Whether the level supports left and right split subjects.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <returns>The validated trigger.</returns>
        /// <exception cref="InvalidDataException">Thrown when a value or cross-attribute combination is invalid.</exception>
        internal static TutorialTrigger Parse(
            string showOn,
            string inArea,
            string subject,
            bool twoParts,
            string source)
        {
            TutorialEvent tutorialEvent = TutorialEvents.Parse(showOn ?? "start", source, "showOn");
            TutorialArea? area = inArea is null
                ? null
                : TutorialArea.Parse(inArea, source, "inArea");
            TutorialSubject parsedSubject = TutorialSubjects.Parse(subject, source, "subject");

            return tutorialEvent == TutorialEvent.CandyMoved && area is null
                ? throw TutorialValues.Invalid(source, "inArea", inArea)
                : !twoParts && parsedSubject is TutorialSubject.Left or TutorialSubject.Right
                    ? throw TutorialValues.Invalid(source, "subject", subject)
                    : new TutorialTrigger(tutorialEvent, area, parsedSubject);
        }
    }
}
