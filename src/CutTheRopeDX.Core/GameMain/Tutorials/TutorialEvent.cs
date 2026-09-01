using System.Globalization;
using System.IO;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Named tutorial conditions accepted by the map XML schema.</summary>
    internal enum TutorialEvent
    {
        /// <summary>The level has finished loading its tutorial prompts.</summary>
        Start,
        /// <summary>A candy has entered a bubble.</summary>
        BubbleCapture,
        /// <summary>A bubble owned by a candy has popped.</summary>
        BubblePop,
        /// <summary>A lantern has captured a candy.</summary>
        LanternCatch,
        /// <summary>A sock has accepted a candy for transport.</summary>
        SockCatch,
        /// <summary>A mouse has grabbed a candy.</summary>
        MouseGrab,
        /// <summary>A spider has stolen a candy.</summary>
        SpiderSteal,
        /// <summary>A mechanical hand has captured a candy.</summary>
        HandGrab,
        /// <summary>A rope attached to a candy has been cut.</summary>
        RopeCut,
        /// <summary>A candy has collected a star.</summary>
        StarCollected,
        /// <summary>A target has eaten a candy.</summary>
        CandyEaten,
        /// <summary>A candy has entered a bamboo pipe.</summary>
        PipeEnter,
        /// <summary>A candy has hit ordinary spikes.</summary>
        SpikeHit,
        /// <summary>A candy has hit electro spikes.</summary>
        ElectroHit,
        /// <summary>The level has entered its won outcome.</summary>
        GameWon,
        /// <summary>The level has entered its lost outcome.</summary>
        GameLost,
        /// <summary>A rocket has transitioned into flight.</summary>
        RocketIgnite,
        /// <summary>A candy has collided with an active bouncer.</summary>
        BouncerHit,
        /// <summary>The player has operated a pump.</summary>
        PumpFire,
        /// <summary>The player has activated a steam tube.</summary>
        SteamBurst,
        /// <summary>The player has started rotating a disc.</summary>
        DiscSpin,
        /// <summary>The player has frozen time.</summary>
        TimeFreeze,
        /// <summary>The player has resumed time.</summary>
        TimeUnfreeze,
        /// <summary>The player has toggled gravity.</summary>
        GravityFlip,
        /// <summary>A candy currently occupies a bubble.</summary>
        Bubbled,
        /// <summary>A candy is currently held by a lantern.</summary>
        InLantern,
        /// <summary>A candy is currently carried by an ant.</summary>
        CarriedByAnt,
        /// <summary>A candy is currently carried by a snail.</summary>
        CarriedBySnail,
        /// <summary>Time is currently frozen.</summary>
        TimeFrozen,
        /// <summary>Gravity is currently inverted.</summary>
        GravityInverted,
        /// <summary>A candy occupies the required authored region.</summary>
        CandyMoved,
    }

    /// <summary>Describes whether an event is instantaneous or continuously observable.</summary>
    internal enum TutorialEventKind
    {
        /// <summary>The event occurs at a single authoritative transition.</summary>
        Edge,
        /// <summary>The event remains true while its authoritative state holds.</summary>
        State,
    }

    /// <summary>Describes how the director observes a tutorial event.</summary>
    internal enum TutorialObservation
    {
        /// <summary>The authoritative transition pushes the event directly.</summary>
        Push,
        /// <summary>The director detects the event by comparing keyed snapshots.</summary>
        Diffed,
        /// <summary>The director samples authoritative state for each active body.</summary>
        Sampled,
    }

    /// <summary>Parses and classifies the closed tutorial event vocabulary.</summary>
    internal static class TutorialEvents
    {
        /// <summary>Parses an exact, case-sensitive XML event name.</summary>
        /// <param name="value">Authored XML value.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <param name="attribute">Attribute name used in validation errors.</param>
        /// <returns>The matching tutorial event.</returns>
        /// <exception cref="InvalidDataException">Thrown when <paramref name="value"/> is not in the closed vocabulary.</exception>
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

        /// <summary>Returns whether an event is an edge or state condition.</summary>
        /// <param name="tutorialEvent">Event to classify.</param>
        /// <returns>The event kind.</returns>
        internal static TutorialEventKind Kind(TutorialEvent tutorialEvent)
        {
            return tutorialEvent >= TutorialEvent.Bubbled
                ? TutorialEventKind.State
                : TutorialEventKind.Edge;
        }

        /// <summary>Returns how the director observes an event.</summary>
        /// <param name="tutorialEvent">Event to classify.</param>
        /// <returns>The observation strategy.</returns>
        internal static TutorialObservation Observation(TutorialEvent tutorialEvent)
        {
            return tutorialEvent == TutorialEvent.RocketIgnite
                ? TutorialObservation.Diffed
                : Kind(tutorialEvent) == TutorialEventKind.State
                    ? TutorialObservation.Sampled
                    : TutorialObservation.Push;
        }
    }

    /// <summary>Provides strict invariant-culture value parsing for tutorial XML.</summary>
    internal static class TutorialValues
    {
        /// <summary>Parses an optional finite, non-negative floating-point value.</summary>
        /// <param name="value">Authored value, or <see langword="null"/> to use the default.</param>
        /// <param name="defaultValue">Value returned when no attribute is authored.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <param name="attribute">Attribute name used in validation errors.</param>
        /// <returns>The parsed or default value.</returns>
        /// <exception cref="InvalidDataException">Thrown when the authored value is malformed, non-finite, or negative.</exception>
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

        /// <summary>Parses a required finite floating-point value.</summary>
        /// <param name="value">Authored value.</param>
        /// <param name="source">Map source used in validation errors.</param>
        /// <param name="attribute">Attribute name used in validation errors.</param>
        /// <returns>The parsed finite value.</returns>
        /// <exception cref="InvalidDataException">Thrown when the authored value is malformed or non-finite.</exception>
        internal static float ParseFiniteFloat(string value, string source, string attribute)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
                && float.IsFinite(parsed)
                    ? parsed
                    : throw Invalid(source, attribute, value);
        }

        /// <summary>Creates the shared strict-schema validation exception.</summary>
        /// <param name="source">Map source containing the invalid value.</param>
        /// <param name="attribute">Invalid tutorial attribute.</param>
        /// <param name="value">Invalid authored value.</param>
        /// <returns>A consistently formatted validation exception.</returns>
        internal static InvalidDataException Invalid(string source, string attribute, string value)
        {
            return new InvalidDataException($"{source}: invalid tutorial {attribute}=\"{value}\"");
        }
    }
}
