using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Mutually exclusive night-sleep behavior for one Om Nom.</summary>
    internal enum NightSleepPhase
    {
        /// <summary>Fully awake.</summary>
        Awake,

        /// <summary>Playing the transition into sleep and waiting to pulse.</summary>
        FallingAsleep,

        /// <summary>Sleeping with the breathing pulse active.</summary>
        Pulsing,

        /// <summary>Playing the wake transition.</summary>
        Waking,
    }

    /// <summary>Observable edge produced by a night illumination update.</summary>
    internal enum NightSleepTransition
    {
        /// <summary>No sleep/wake animation should start.</summary>
        None,

        /// <summary>The target began falling asleep.</summary>
        FellAsleep,

        /// <summary>The target began waking.</summary>
        Woke,
    }

    /// <summary>
    /// Single owner of night sleep phase, pulse timing, sound cadence, and shared sleep-overlay
    /// visibility for one Om Nom.
    /// </summary>
    internal sealed class NightSleepState
    {
        private bool illuminationObserved;

        /// <summary>Gets the current mutually exclusive night-sleep phase.</summary>
        public NightSleepPhase Phase { get; private set; } = NightSleepPhase.Awake;

        /// <summary>Gets whether illumination-dependent interactions are currently allowed.</summary>
        public bool IsAwake => Phase is NightSleepPhase.Awake or NightSleepPhase.Waking;

        /// <summary>Gets elapsed breathing-pulse time.</summary>
        public float PulseTime { get; private set; }

        /// <summary>Gets the remaining delay before breathing starts.</summary>
        public float PulseDelay { get; private set; }

        /// <summary>Gets the target-relative pulse pivot.</summary>
        public float PulseBaseY { get; private set; }

        /// <summary>Gets elapsed time toward the next sleep sound.</summary>
        public float SoundTime { get; private set; }

        /// <summary>Gets whether the controller's sleep overlay is currently presented.</summary>
        public bool OverlayVisible { get; private set; }

        /// <summary>Updates the authoritative phase from current illumination.</summary>
        /// <param name="isAwake">Whether a light currently illuminates the target.</param>
        /// <param name="pulseDelay">Backend-specific delay before pulsing.</param>
        /// <param name="pulseBaseY">Target-relative pulse pivot.</param>
        /// <returns>The animation edge caused by this observation.</returns>
        public NightSleepTransition ObserveAwake(bool isAwake, float pulseDelay, float pulseBaseY)
        {
            if (isAwake)
            {
                if (!illuminationObserved || !IsAwake)
                {
                    illuminationObserved = true;
                    Phase = NightSleepPhase.Waking;
                    ClearPresentation();
                    return NightSleepTransition.Woke;
                }

                if (Phase == NightSleepPhase.Waking)
                {
                    Phase = NightSleepPhase.Awake;
                }

                return NightSleepTransition.None;
            }

            if (!illuminationObserved || IsAwake)
            {
                illuminationObserved = true;
                Phase = NightSleepPhase.FallingAsleep;
                PulseTime = 0f;
                PulseDelay = MathF.Max(0f, pulseDelay);
                PulseBaseY = pulseBaseY;
                SoundTime = 0.9f;
                OverlayVisible = false;
                return NightSleepTransition.FellAsleep;
            }

            return NightSleepTransition.None;
        }

        /// <summary>Advances falling-asleep delay or the active breathing-pulse clock.</summary>
        /// <param name="delta">Elapsed seconds.</param>
        public void AdvancePulse(float delta)
        {
            delta = MathF.Max(0f, delta);
            if (Phase == NightSleepPhase.FallingAsleep)
            {
                PulseDelay = MathF.Max(0f, PulseDelay - delta);
                if (PulseDelay == 0f)
                {
                    Phase = NightSleepPhase.Pulsing;
                    PulseTime += delta;
                }
                return;
            }

            if (Phase == NightSleepPhase.Pulsing)
            {
                PulseTime += delta;
            }
        }

        /// <summary>Changes overlay ownership and reports whether the controller needs synchronization.</summary>
        /// <param name="visible">Desired overlay visibility.</param>
        /// <param name="feedingAsleep">Whether feeding sleep, rather than night sleep, owns the overlay.</param>
        /// <returns><see langword="true"/> when visibility changed.</returns>
        public bool SetOverlayVisible(bool visible, bool feedingAsleep)
        {
            if (visible && IsAwake && !feedingAsleep)
            {
                return false;
            }

            if (OverlayVisible == visible)
            {
                return false;
            }

            OverlayVisible = visible;
            return true;
        }

        /// <summary>Advances sleep-sound cadence and consumes a due sound edge.</summary>
        /// <param name="delta">Elapsed seconds.</param>
        /// <param name="interval">Seconds between sleep sounds.</param>
        /// <returns><see langword="true"/> when a sound is due.</returns>
        public bool AdvanceSound(float delta, float interval)
        {
            SoundTime += MathF.Max(0f, delta);
            if (SoundTime <= interval)
            {
                return false;
            }

            SoundTime = 0f;
            return true;
        }

        /// <summary>Sets the initial sound cadence used when post-eat sleep starts.</summary>
        /// <param name="soundTime">Initial elapsed sound time.</param>
        public void StartPostEatPresentation(float soundTime)
        {
            SoundTime = MathF.Max(0f, soundTime);
            OverlayVisible = false;
        }

        /// <summary>Clears pulse, sound, and overlay presentation without changing logical sleep phase.</summary>
        public void ClearPresentation()
        {
            PulseTime = 0f;
            PulseDelay = 0f;
            PulseBaseY = 0f;
            SoundTime = 0f;
            OverlayVisible = false;
        }
    }
}
