using CutTheRope.Framework.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Root object for Flash XML animations, with playback-rate scaling applied during updates.
    /// </summary>
    internal sealed class FlashXmlStageRoot : GameObject
    {
        /// <summary>Default Flash XML playback rate multiplier.</summary>
        internal const float DefaultPlaybackRate = 1f;

        /// <summary>Playback speed multiplier applied to update deltas.</summary>
        internal float PlaybackRate { get; set; } = DefaultPlaybackRate;

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta * PlaybackRate);
        }
    }
}
