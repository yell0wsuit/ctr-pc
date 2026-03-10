using System.Collections.Generic;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Immutable definition of a single Om Nom skin loaded from the skins manifest.
    /// </summary>
    internal sealed class OmNomSkinDefinition(
        string name,
        string animationXmlPath,
        IReadOnlyDictionary<TargetAnimationState, int> timelineMappings,
        IReadOnlyDictionary<int, int> followups,
        int[] idleVariants,
        int idleToSleepTrimFrames)
    {
        /// <summary>Localization key for display name.</summary>
        public string Name { get; } = name;

        /// <summary>Absolute path to the animation XML file.</summary>
        public string AnimationXmlPath { get; } = animationXmlPath;

        /// <summary>Maps animation states to timeline IDs in the XML.</summary>
        public IReadOnlyDictionary<TargetAnimationState, int> TimelineMappings { get; } = timelineMappings;

        /// <summary>Maps finished timeline ID to the next timeline ID to play.</summary>
        public IReadOnlyDictionary<int, int> Followups { get; } = followups;

        /// <summary>Timeline IDs to randomly pick from for idle variations.</summary>
        public int[] IdleVariants { get; } = idleVariants;

        /// <summary>Frames to skip from the start of the idle-to-sleep transition.</summary>
        public int IdleToSleepTrimFrames { get; } = idleToSleepTrimFrames;

        /// <summary>Gets the timeline ID for a given state, or -1 if unmapped.</summary>
        public int GetTimelineId(TargetAnimationState state)
        {
            return TimelineMappings.TryGetValue(state, out int id) ? id : -1;
        }

        /// <summary>Whether a followup timeline should play after the given timeline finishes.</summary>
        public bool TryGetFollowupTimeline(int finishedTimelineId, out int followupTimelineId)
        {
            return Followups.TryGetValue(finishedTimelineId, out followupTimelineId);
        }

        /// <summary>Whether the given timeline should bind a delegate for followup/cadence.</summary>
        public bool ShouldBindFollowupDelegate(int timelineId)
        {
            return Followups.ContainsKey(timelineId);
        }
    }
}
