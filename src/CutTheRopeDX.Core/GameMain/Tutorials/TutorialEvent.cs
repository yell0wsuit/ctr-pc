using System.Globalization;
using System.IO;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal enum TutorialEvent
    {
        Start,
        BubbleCapture,
        BubblePop,
        LanternCatch,
        SockCatch,
        MouseGrab,
        SpiderSteal,
        HandGrab,
        RopeCut,
        StarCollected,
        CandyEaten,
        GapEnter,
        PipeEnter,
        SpikeHit,
        ElectroHit,
        GameWon,
        GameLost,
        RocketIgnite,
        BouncerHit,
        PumpFire,
        SteamBurst,
        DiscSpin,
        TimeFreeze,
        TimeUnfreeze,
        GravityFlip,
        Bubbled,
        InLantern,
        CarriedByAnt,
        CarriedBySnail,
        TimeFrozen,
        GravityInverted,
        CandyMoved,
    }

    internal enum TutorialEventKind
    {
        Edge,
        State,
    }

    internal enum TutorialObservation
    {
        Push,
        Diffed,
        Sampled,
    }

    internal static class TutorialEvents
    {
        internal static TutorialEvent Parse(string value, string source, string attribute)
        {
            return value switch
            {
                "start" => TutorialEvent.Start,
                "bubbleCapture" => TutorialEvent.BubbleCapture,
                "bubblePop" => TutorialEvent.BubblePop,
                "lanternCatch" => TutorialEvent.LanternCatch,
                "sockCatch" => TutorialEvent.SockCatch,
                "mouseGrab" => TutorialEvent.MouseGrab,
                "spiderSteal" => TutorialEvent.SpiderSteal,
                "handGrab" => TutorialEvent.HandGrab,
                "ropeCut" => TutorialEvent.RopeCut,
                "starCollected" => TutorialEvent.StarCollected,
                "candyEaten" => TutorialEvent.CandyEaten,
                "gapEnter" => TutorialEvent.GapEnter,
                "pipeEnter" => TutorialEvent.PipeEnter,
                "spikeHit" => TutorialEvent.SpikeHit,
                "electroHit" => TutorialEvent.ElectroHit,
                "gameWon" => TutorialEvent.GameWon,
                "gameLost" => TutorialEvent.GameLost,
                "rocketIgnite" => TutorialEvent.RocketIgnite,
                "bouncerHit" => TutorialEvent.BouncerHit,
                "pumpFire" => TutorialEvent.PumpFire,
                "steamBurst" => TutorialEvent.SteamBurst,
                "discSpin" => TutorialEvent.DiscSpin,
                "timeFreeze" => TutorialEvent.TimeFreeze,
                "timeUnfreeze" => TutorialEvent.TimeUnfreeze,
                "gravityFlip" => TutorialEvent.GravityFlip,
                "bubbled" => TutorialEvent.Bubbled,
                "inLantern" => TutorialEvent.InLantern,
                "carriedByAnt" => TutorialEvent.CarriedByAnt,
                "carriedBySnail" => TutorialEvent.CarriedBySnail,
                "timeFrozen" => TutorialEvent.TimeFrozen,
                "gravityInverted" => TutorialEvent.GravityInverted,
                "candyMoved" => TutorialEvent.CandyMoved,
                _ => throw TutorialValues.Invalid(source, attribute, value),
            };
        }

        internal static TutorialEventKind Kind(TutorialEvent tutorialEvent)
        {
            return tutorialEvent >= TutorialEvent.Bubbled
                ? TutorialEventKind.State
                : TutorialEventKind.Edge;
        }

        internal static TutorialObservation Observation(TutorialEvent tutorialEvent)
        {
            return tutorialEvent == TutorialEvent.RocketIgnite
                ? TutorialObservation.Diffed
                : Kind(tutorialEvent) == TutorialEventKind.State
                    ? TutorialObservation.Sampled
                    : TutorialObservation.Push;
        }
    }

    internal static class TutorialValues
    {
        internal static float ParseNonNegativeFloat(
            string value,
            float defaultValue,
            string source,
            string attribute)
        {
            return value is null
                ? defaultValue
                : float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
                    && float.IsFinite(parsed)
                    && parsed >= 0f
                        ? parsed
                        : throw Invalid(source, attribute, value);
        }

        internal static float ParseFiniteFloat(string value, string source, string attribute)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
                && float.IsFinite(parsed)
                    ? parsed
                    : throw Invalid(source, attribute, value);
        }

        internal static InvalidDataException Invalid(string source, string attribute, string value)
        {
            return new InvalidDataException($"{source}: invalid tutorial {attribute}=\"{value}\"");
        }
    }
}
