using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Composes all state owned by one independently targetable Om Nom.</summary>
    /// <param name="blinkCountdown">Initial animation-frame blink countdown.</param>
    /// <param name="idleCountdown">Initial animation-frame idle/chat countdown.</param>
    internal sealed class TargetContext(int blinkCountdown, int idleCountdown)
    {
        public TargetAnimationController controller;

        public GameObject targetObject;

        public Image support;

        public float baseScaleX = 1f;

        public float baseScaleY = 1f;

        /// <summary>Gets the authoritative feeding behavior.</summary>
        public TargetFeedingState Feeding { get; } = new();

        /// <summary>Gets the authoritative night-sleep behavior and shared sleep presentation.</summary>
        public NightSleepState NightSleep { get; } = new();

        /// <summary>Gets the authoritative blink, idle, and chat cadence.</summary>
        public TargetIdleState Idle { get; } = new(blinkCountdown, idleCountdown);
    }
}
