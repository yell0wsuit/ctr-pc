using CutTheRope.Framework.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class FlashXmlStageRoot : GameObject
    {
        internal const float DefaultPlaybackRate = 1f;

        internal float PlaybackRate { get; set; } = DefaultPlaybackRate;

        public override void Update(float delta)
        {
            base.Update(delta * PlaybackRate);
        }
    }
}
