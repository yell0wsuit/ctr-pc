using System;

using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Facade for Om Nom animation playback that delegates to a pluggable backend.
    /// </summary>
    internal sealed class TargetAnimationController
    {
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
        /// Creates a controller that uses the original sprite/timeline backend.
        /// </summary>
        /// <param name="target">Om Nom sprite animation object.</param>
        /// <param name="isNightLevel">Whether sleep animations should be configured.</param>
        /// <param name="isXmas">Whether Christmas animation variants should be configured.</param>
        /// <returns>A controller instance backed by <see cref="OriginalTargetAnimationBackend"/>.</returns>
        public static TargetAnimationController Create(CharAnimations target, bool isNightLevel, bool isXmas)
        {
            return new TargetAnimationController(new OriginalTargetAnimationBackend(target, isNightLevel, isXmas));
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

        /// <summary>Gets the blink overlay animation created by the backend.</summary>
        public Animation Blink => backend.Blink;

        /// <summary>Gets the primary sleep overlay animation created by the backend.</summary>
        public Animation SleepAnimationPrimary => backend.SleepAnimationPrimary;

        /// <summary>Gets the secondary sleep overlay animation created by the backend.</summary>
        public Animation SleepAnimationSecondary => backend.SleepAnimationSecondary;

        /// <summary>Gets the primary Om Nom gameplay object owned by the backend.</summary>
        public GameObject TargetObject => backend.TargetObject;

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
        /// Plays one of the idle variation animations based on the provided random function.
        /// </summary>
        /// <param name="rng">Inclusive random function with signature <c>(min, max) => value</c>.</param>
        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            if (rng(0, 1) == 1)
            {
                backend.Play(TargetAnimationState.IdleVariationOne);
            }
            else
            {
                backend.Play(TargetAnimationState.IdleVariationTwo);
            }
        }

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
            backend.Play(TargetAnimationState.Sleeping);
        }

        /// <summary>
        /// Checks whether the idle loop animation is currently active.
        /// </summary>
        /// <returns><c>true</c> when idle loop is currently playing; otherwise <c>false</c>.</returns>
        public bool IsIdleLoopPlaying()
        {
            return backend.IsPlaying(TargetAnimationState.IdleLoop);
        }

        /// <summary>
        /// Checks whether the sleeping animation is currently active.
        /// </summary>
        /// <returns><c>true</c> when sleeping animation is currently playing; otherwise <c>false</c>.</returns>
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
    }
}
