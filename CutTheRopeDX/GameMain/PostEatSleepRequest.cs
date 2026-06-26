using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Delayed-dispatch payload for a target's post-eat sleep transition.
    /// </summary>
    internal sealed class PostEatSleepRequest(TargetContext target) : FrameworkTypes
    {
        public TargetContext Target { get; } = target;
    }
}
