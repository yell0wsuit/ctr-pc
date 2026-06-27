using System;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Facade for Om Nom animation playback that delegates to a pluggable backend.
    /// </summary>
    internal sealed class TargetAnimationController
    {
        /// <summary>
        /// Backend implementation used for all target animation operations.
        /// </summary>
        private readonly ITargetAnimationBackend backend;

        /// <summary>
        /// Initializes a controller around the provided backend implementation.
        /// </summary>
        /// <param name="backend">Backend implementation used for all animation operations.</param>
        private TargetAnimationController(ITargetAnimationBackend backend)
        {
            this.backend = backend;
        }

        /// <summary>
        /// Creates a controller with a custom backend implementation.
        /// </summary>
        /// <param name="backend">Backend implementation used for all animation operations.</param>
        /// <returns>A controller instance that delegates to <paramref name="backend"/>.</returns>
        public static TargetAnimationController Create(ITargetAnimationBackend backend)
        {
            return new TargetAnimationController(backend);
        }

        /// <summary>Gets the primary Om Nom gameplay object owned by the backend.</summary>
        public GameObject TargetObject => backend.TargetObject;

        /// <summary>Gets the skin definition for this target's skin, or <see langword="null"/> for the classic skin.</summary>
        public OmNomSkinDefinition SkinDefinition => backend.SkinDefinition;

        /// <summary>
        /// Whether the backend handles the sleep breathing pulse internally.
        /// </summary>
        public bool HandlesOwnSleepPulse => backend.HandlesOwnSleepPulse;

        /// <summary>Whether the backend is driven by Flash XML animation exports.</summary>
        public bool UsesFlashXmlAnimations => backend.UsesFlashXmlAnimations;

        /// <summary>Gets the backend-defined base horizontal scale for Om Nom.</summary>
        /// <returns>Default X scale for Om Nom.</returns>
        public float GetTargetBaseScaleX()
        {
            return backend.GetTargetBaseScaleX();
        }

        /// <summary>Gets the backend-defined base vertical scale for Om Nom.</summary>
        /// <returns>Default Y scale for Om Nom.</returns>
        public float GetTargetBaseScaleY()
        {
            return backend.GetTargetBaseScaleY();
        }

        /// <summary>
        /// Initializes backend timelines and binds timeline delegate callbacks.
        /// </summary>
        /// <param name="timelineDelegate">Timeline delegate receiving keyframe callbacks.</param>
        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            backend.Initialize(timelineDelegate);
        }

        /// <summary>
        /// Plays the greeting animation.
        /// </summary>
        public void PlayGreeting()
        {
            backend.Play(TargetAnimationState.Greeting);
        }

        /// <summary>
        /// Plays a directional chat greeting where Om Nom turns its head toward another Om Nom.
        /// </summary>
        /// <param name="direction">Directional greet state (<see cref="TargetAnimationState.GreetLeft"/> or <see cref="TargetAnimationState.GreetRight"/>).</param>
        public void PlayGreetingTurn(TargetAnimationState direction)
        {
            backend.Play(direction);
        }

        /// <summary>
        /// Plays one of the idle variation animations based on the provided random function.
        /// </summary>
        /// <param name="rng">Inclusive random function with signature <c>(min, max) => value</c>.</param>
        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            backend.PlayRandomIdleVariant(rng);
        }

        /// <summary>Whether this skin plays the greeting animation on initialization instead of idle.</summary>
        public bool StartsWithGreeting => backend.StartsWithGreeting;

        /// <summary>
        /// Plays the excited animation.
        /// </summary>
        public void PlayExcited()
        {
            backend.Play(TargetAnimationState.Excited);
        }

        /// <summary>
        /// Plays the mouth-opening animation.
        /// </summary>
        public void PlayMouthOpening()
        {
            backend.Play(TargetAnimationState.MouthOpening);
        }

        /// <summary>
        /// Plays the mouth-closing animation.
        /// </summary>
        public void PlayMouthClosing()
        {
            backend.Play(TargetAnimationState.MouthClosing);
        }

        /// <summary>
        /// Plays the chewing animation.
        /// </summary>
        public void PlayChewing()
        {
            backend.Play(TargetAnimationState.Chewing);
        }

        /// <summary>
        /// Plays the sad animation.
        /// </summary>
        public void PlaySad()
        {
            backend.Play(TargetAnimationState.Sad);
        }

        /// <summary>
        /// Plays the sleeping animation.
        /// </summary>
        public void PlaySleeping()
        {
            backend.PlaySleeping(trimIdleToSleepTransition: true);
        }

        /// <summary>
        /// Plays the sleeping animation without trimming the idle-to-sleep transition.
        /// </summary>
        public void PlaySleepingWithoutIdleToSleepTrim()
        {
            backend.PlaySleeping(trimIdleToSleepTransition: false);
        }

        /// <summary>
        /// Checks whether the idle loop animation is currently active.
        /// </summary>
        /// <returns><see langword="true" /> when idle loop is currently playing; otherwise <see langword="false" />.</returns>
        public bool IsIdleLoopPlaying()
        {
            return backend.IsPlaying(TargetAnimationState.IdleLoop);
        }

        /// <summary>
        /// Checks whether the sleeping animation is currently active.
        /// </summary>
        /// <returns><see langword="true" /> when sleeping animation is currently playing; otherwise <see langword="false" />.</returns>
        public bool IsSleepingAnimationPlaying()
        {
            return backend.IsPlaying(TargetAnimationState.Sleeping);
        }

        /// <summary>
        /// Gets the delay before night sleep pulse effects should begin.
        /// </summary>
        /// <returns>Delay in seconds.</returns>
        public float GetSleepPulseDelaySeconds()
        {
            return backend.GetSleepPulseDelaySeconds();
        }

        /// <summary>Resets the blink animation to frame 0 without showing it.</summary>
        public void ResetBlink()
        {
            backend.ResetBlink();
        }

        /// <summary>Shows the blink overlay and plays it from frame 0.</summary>
        public void TriggerBlink()
        {
            backend.TriggerBlink();
        }

        /// <summary>Advances all sleep overlay animations by <paramref name="delta"/> seconds.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        public void UpdateSleepOverlays(float delta)
        {
            backend.UpdateSleepOverlays(delta);
        }

        /// <summary>Advances backend-specific non-sleep overlays by <paramref name="delta"/> seconds.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        public void UpdateAdditionalOverlays(float delta)
        {
            backend.UpdateAdditionalOverlays(delta);
        }

        /// <summary>Moves all sleep overlay animations to the given position.</summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        public void SyncSleepOverlayPosition(float x, float y)
        {
            backend.SyncSleepOverlayPosition(x, y);
        }

        /// <summary>Updates the spawn position used by backend-specific non-sleep overlays.</summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        public void SyncAdditionalOverlayPosition(float x, float y)
        {
            backend.SyncAdditionalOverlayPosition(x, y);
        }

        /// <summary>Sets visibility and playback state of all sleep overlay animations.</summary>
        /// <param name="visible"><see langword="true"/> to show overlays; otherwise <see langword="false"/>.</param>
        public void SetSleepOverlayVisible(bool visible)
        {
            backend.SetSleepOverlayVisible(visible);
        }

        /// <summary>Draws all sleep overlay animations that are currently visible.</summary>
        public void DrawSleepOverlays()
        {
            backend.DrawSleepOverlays();
        }
    }
}
