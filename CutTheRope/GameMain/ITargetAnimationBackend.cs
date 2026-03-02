using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Defines backend operations used by <see cref="TargetAnimationController"/>.
    /// </summary>
    internal interface ITargetAnimationBackend
    {
        /// <summary>Gets the primary Om Nom gameplay object.</summary>
        GameObject TargetObject { get; }

        /// <summary>Gets the blink overlay animation.</summary>
        Animation Blink { get; }

        /// <summary>Gets the primary sleep overlay animation.</summary>
        Animation SleepAnimationPrimary { get; }

        /// <summary>Gets the secondary sleep overlay animation.</summary>
        Animation SleepAnimationSecondary { get; }

        /// <summary>
        /// Initializes backend timeline state and delegates.
        /// </summary>
        /// <param name="timelineDelegate">Timeline delegate receiving keyframe callbacks.</param>
        void Initialize(ITimelineDelegate timelineDelegate);

        /// <summary>
        /// Plays the requested target animation state.
        /// </summary>
        /// <param name="state">Animation state to play.</param>
        void Play(TargetAnimationState state);

        /// <summary>
        /// Checks whether the requested target animation state is currently active.
        /// </summary>
        /// <param name="state">Animation state to query.</param>
        /// <returns><c>true</c> if the state is active; otherwise <c>false</c>.</returns>
        bool IsPlaying(TargetAnimationState state);
    }
}
