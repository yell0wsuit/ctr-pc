using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal enum TutorialSubject
    {
        Any,
        Primary,
        Left,
        Right,
    }

    internal static class TutorialSubjects
    {
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

    internal readonly record struct TutorialArea(float X, float Y, float Width, float Height)
    {
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

        internal bool Contains(Vector point)
        {
            return point.X >= X
                && point.X < X + Width
                && point.Y >= Y
                && point.Y < Y + Height;
        }
    }

    internal sealed record TutorialTrigger(
        TutorialEvent Event,
        TutorialArea? Area,
        TutorialSubject Subject)
    {
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
