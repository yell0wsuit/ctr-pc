using System;
using System.Collections.Generic;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable definition of a single Om Nom skin loaded from the skins manifest.
    /// </summary>
    internal sealed class OmNomSkinDefinition(
        string id,
        string name,
        string animationXmlPath,
        IReadOnlyDictionary<TargetAnimationState, int> timelineMappings,
        IReadOnlyDictionary<int, int> followups,
        int[] idleVariants,
        int idleToSleepTrimFrames,
        int[] slowTimelineIds,
        bool startWithGreeting,
        string[] uniqueSounds)
    {
        /// <summary>Identifier for the skin.</summary>
        public string Id { get; } = id;

        /// <summary>Skin name/prefix.</summary>
        public string Name { get; } = name;

        /// <summary>Absolute path to the animation XML file.</summary>
        public string AnimationXmlPath { get; } = animationXmlPath;

        /// <summary>Maps animation states to timeline IDs in the XML.</summary>
        public IReadOnlyDictionary<TargetAnimationState, int> TimelineMappings { get; } = timelineMappings;

        /// <summary>Maps finished timeline ID to the next timeline ID to play.</summary>
        public IReadOnlyDictionary<int, int> Followups { get; } = followups;

        /// <summary>Timeline IDs to randomly pick from for idle variations.</summary>
        public int[] IdleVariants { get; } = idleVariants;

        /// <summary>Whether to play the greeting animation immediately on initialization.</summary>
        public bool StartWithGreeting { get; } = startWithGreeting;

        /// <summary>Frames to skip from the start of the idle-to-sleep transition.</summary>
        public int IdleToSleepTrimFrames { get; } = idleToSleepTrimFrames;

        /// <summary>Timeline IDs that should run at the slowed iOS Flash playback rate.</summary>
        public int[] SlowTimelineIds { get; } = slowTimelineIds;

        /// <summary>Classic Om Nom sounds that this skin overrides or explicitly uses.</summary>
        public string[] UniqueSounds { get; } = uniqueSounds;

        /// <summary>Gets the timeline ID for a given state, or -1 if unmapped.</summary>
        /// <param name="state">Animation state to resolve.</param>
        /// <returns>The timeline ID mapped to <paramref name="state"/>, or -1 when unmapped.</returns>
        public int GetTimelineId(TargetAnimationState state)
        {
            return TimelineMappings.TryGetValue(state, out int id) ? id : -1;
        }

        /// <summary>Whether this skin declares a unique behavior for the given Om Nom sound.</summary>
        /// <param name="soundResourceName">Sound resource name to check.</param>
        /// <returns><see langword="true"/> when the sound is explicitly listed as unique; otherwise <see langword="false"/>.</returns>
        public bool HasUniqueSound(string soundResourceName)
        {
            return Array.IndexOf(UniqueSounds, soundResourceName) >= 0;
        }

        /// <summary>Whether a followup timeline should play after the given timeline finishes.</summary>
        /// <param name="finishedTimelineId">Timeline that just finished.</param>
        /// <param name="followupTimelineId">Resolved followup timeline ID when one exists.</param>
        /// <returns><see langword="true"/> when a followup timeline mapping exists; otherwise <see langword="false"/>.</returns>
        public bool TryGetFollowupTimeline(int finishedTimelineId, out int followupTimelineId)
        {
            return Followups.TryGetValue(finishedTimelineId, out followupTimelineId);
        }

        /// <summary>Whether the given timeline should bind a delegate for followup/cadence.</summary>
        /// <param name="timelineId">Timeline to test for followup binding.</param>
        /// <returns><see langword="true"/> when the timeline has a configured followup; otherwise <see langword="false"/>.</returns>
        public bool ShouldBindFollowupDelegate(int timelineId)
        {
            return Followups.ContainsKey(timelineId);
        }
    }
}
