using System;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Defines backend operations used by <see cref="TargetAnimationController"/>.
    /// </summary>
    internal interface ITargetAnimationBackend
    {
        /// <summary>Gets the primary Om Nom gameplay object.</summary>
        GameObject TargetObject { get; }

        /// <summary>Gets the default horizontal scale that should be applied to the target object.</summary>
        /// <returns>Default X scale for the target object.</returns>
        float GetTargetBaseScaleX();

        /// <summary>Gets the default vertical scale that should be applied to the target object.</summary>
        /// <returns>Default Y scale for the target object.</returns>
        float GetTargetBaseScaleY();

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
        /// Plays a backend-specific random idle variation.
        /// </summary>
        /// <param name="rng">Inclusive random function with signature <c>(min, max) => value</c>.</param>
        void PlayRandomIdleVariant(Func<int, int, int> rng);

        /// <summary>Whether this skin plays the greeting animation on initialization instead of idle.</summary>
        bool StartsWithGreeting { get; }

        /// <summary>
        /// Checks whether the requested target animation state is currently active.
        /// </summary>
        /// <param name="state">Animation state to query.</param>
        /// <returns><see langword="true" /> if the state is active; otherwise <see langword="false" />.</returns>
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
        /// <param name="delta">Elapsed time in seconds.</param>
        void UpdateSleepOverlays(float delta);

        /// <summary>Advances backend-specific non-sleep overlays by <paramref name="delta"/> seconds.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        void UpdateAdditionalOverlays(float delta);

        /// <summary>Moves all sleep overlay animations to the given position.</summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        void SyncSleepOverlayPosition(float x, float y);

        /// <summary>Updates the spawn position used by backend-specific non-sleep overlays.</summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        void SyncAdditionalOverlayPosition(float x, float y);

        /// <summary>Sets visibility and playback state of all sleep overlay animations.</summary>
        /// <param name="visible"><see langword="true"/> to show overlays; otherwise <see langword="false"/>.</param>
        void SetSleepOverlayVisible(bool visible);

        /// <summary>Draws all sleep overlay animations that are currently visible.</summary>
        void DrawSleepOverlays();

        /// <summary>
        /// Whether the backend handles the sleep breathing pulse internally
        /// (so GameScene should not apply its own scale/rotationCenter pulse).
        /// </summary>
        bool HandlesOwnSleepPulse { get; }
    }
}
