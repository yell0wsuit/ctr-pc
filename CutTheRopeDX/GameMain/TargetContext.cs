using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// All per-Om-Nom state. Mouth state lives here because targeting is many-to-many.
    /// </summary>
    internal sealed class TargetContext
    {
        public TargetAnimationController controller;

        public GameObject targetObject;

        public Image support;

        public float baseScaleX = 1f;

        public float baseScaleY = 1f;

        /// <summary>Mouth currently open.</summary>
        public bool mouthOpen;

        /// <summary>Countdown before the mouth closes again.</summary>
        public float mouthCloseTimer;

        /// <summary>True once this Om Nom has eaten a candy; it will not reopen ("eats then sleeps").</summary>
        public bool asleep;

        // --- Night-level sleep state (per Om Nom; were scene singletons) ---
        public bool? isNightTargetAwake;

        public bool sleepPulseActive;

        public float sleepPulseTime;

        public float sleepPulseDelay;

        public float sleepPulseBaseY;

        public float sleepSoundTimer;

        public bool nightSleepOverlayVisible;

        public bool postEatSleepActive;

        public bool postEatSleepScheduled;

        public int blinkTimer;

        public int idlesTimer;
    }
}
