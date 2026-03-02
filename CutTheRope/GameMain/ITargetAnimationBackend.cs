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

        /// <summary>
        /// Gets the delay before night sleep pulse effects should begin.
        /// </summary>
        /// <returns>Delay in seconds.</returns>
        float GetSleepPulseDelaySeconds();

        /// <summary>Resets the blink animation to frame 0 without showing it.</summary>
        void ResetBlink();

        /// <summary>Shows the blink overlay and plays it from frame 0.</summary>
        void TriggerBlink();

        /// <summary>Advances all sleep overlay animations by <paramref name="delta"/> seconds.</summary>
        void UpdateSleepOverlays(float delta);

        /// <summary>Moves all sleep overlay animations to the given position.</summary>
        void SyncSleepOverlayPosition(float x, float y);

        /// <summary>Sets visibility and playback state of all sleep overlay animations.</summary>
        void SetSleepOverlayVisible(bool visible);

        /// <summary>Draws all sleep overlay animations that are currently visible.</summary>
        void DrawSleepOverlays();
    }
}
