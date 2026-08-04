using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Where the level sits in its restart-dim animation.</summary>
    internal enum RestartPhase
    {
        /// <summary>Normal play; no restart in flight.</summary>
        Playing = -1,

        /// <summary>Dimming out before the scene is swapped. Terminal outcomes are suppressed.</summary>
        FadingOut = 0,

        /// <summary>Dimming back in after the swap.</summary>
        FadingIn = 1,
    }

    /// <summary>What the caller must do after advancing the restart machine.</summary>
    internal enum RestartStep
    {
        /// <summary>Nothing to do this frame.</summary>
        None,

        /// <summary>Tear down and rebuild the scene, then keep advancing.</summary>
        SwapScene,

        /// <summary>The restart finished; the level is playable again.</summary>
        Completed,
    }

    /// <summary>
    /// Single owner of a level's lifecycle: the restart-dim machine and the win/lose flags.
    /// These were five public fields read from a dozen places; desync between them silently
    /// disabled every terminal outcome in the game.
    /// </summary>
    internal sealed class LevelFlowState
    {
        /// <summary>Seconds each dim phase lasts.</summary>
        public const float DimDuration = 0.15f;

        /// <summary>Current restart phase.</summary>
        public RestartPhase Phase { get; private set; } = RestartPhase.Playing;

        /// <summary>Remaining dim time for the current phase.</summary>
        public float DimTime { get; private set; }

        /// <summary>Whether the win sequence has started.</summary>
        public bool WonTriggered { get; private set; }

        /// <summary>Whether the loss sequence has started.</summary>
        public bool LostTriggered { get; private set; }

        /// <summary>Whether a win/loss transition is currently playing.</summary>
        public bool TransitionActive { get; private set; }

        /// <summary>True while the screen is dimming out, when the dim overlay renders inverted.</summary>
        public bool IsFadingOut => Phase == RestartPhase.FadingOut;

        /// <summary>
        /// Whether a terminal outcome may fire. False only while dimming out, so a level being
        /// torn down cannot also be won or lost.
        /// </summary>
        public bool CanTriggerOutcome => Phase != RestartPhase.FadingOut;

        /// <summary>Whether neither terminal outcome has started yet.</summary>
        public bool CanTriggerTerminalOutcome => !WonTriggered && !LostTriggered;

        /// <summary>
        /// Whether Om Nom may react to candy or light. Suppressed during a win/loss transition
        /// so a sad Om Nom does not chase a surviving candy.
        /// </summary>
        /// <param name="targetAlreadyFed">Whether this Om Nom has already eaten.</param>
        /// <returns><see langword="true"/> when gameplay reactions are allowed.</returns>
        public bool CanReactToCandy(bool targetAlreadyFed = false)
        {
            return !TransitionActive && !targetAlreadyFed;
        }

        /// <summary>
        /// Full reset, including the restart phase. Call only at genuine level-load boundaries
        /// (<c>Reload</c>, <c>LoadNextMap</c>) - never from <c>Show</c>.
        /// </summary>
        /// <remarks>
        /// A restart legitimately spans <c>Hide</c>/<c>Show</c>: the machine sets
        /// <see cref="RestartPhase.FadingIn"/> and then swaps the scene. Resetting the phase
        /// during <c>Show</c> would wipe the in-flight restart. Use <see cref="ResetOutcome"/> there.
        /// </remarks>
        public void Reset()
        {
            Phase = RestartPhase.Playing;
            DimTime = 0f;
            ResetOutcome();
        }

        /// <summary>
        /// Clears the win/lose flags and the transition flag, leaving the restart phase alone.
        /// Call from scene setup, which also runs in the middle of a restart.
        /// </summary>
        public void ResetOutcome()
        {
            WonTriggered = false;
            LostTriggered = false;
            TransitionActive = false;
        }

        /// <summary>Starts the restart dim animation.</summary>
        public void BeginRestartDim()
        {
            Phase = RestartPhase.FadingOut;
            DimTime = DimDuration;
        }

        /// <summary>Marks the win sequence as started.</summary>
        public void MarkWon()
        {
            WonTriggered = true;
            TransitionActive = true;
        }

        /// <summary>Marks the loss sequence as started.</summary>
        public void MarkLost()
        {
            LostTriggered = true;
            TransitionActive = true;
        }

        /// <summary>Marks a transition active without committing to an outcome yet.</summary>
        /// <remarks>Used when a loss is scheduled after a delay: without this, a candy eaten
        /// during the delay would satisfy the win check and produce a false win.</remarks>
        public void MarkTransitionActive()
        {
            TransitionActive = true;
        }

        /// <summary>Ends the transition without clearing the win/lose flags.</summary>
        public void EndTransition()
        {
            TransitionActive = false;
        }

        /// <summary>
        /// Advances the restart machine by one frame.
        /// </summary>
        /// <param name="delta">Frame delta in seconds.</param>
        /// <returns>What the caller must do as a result.</returns>
        /// <remarks>
        /// Level-triggered, not edge-triggered: the phase advances whenever the dim is exhausted,
        /// rather than on the single frame <see cref="Mover.MoveVariableToTarget"/> reports it
        /// reached the target. That distinction is the original bug - the old code advanced only
        /// on the edge, and <c>MoveVariableToTarget</c> returns false when the value already sits
        /// at the target (<c>Mover.cs:307</c>), so a dim zeroed from outside stranded the machine
        /// forever. Testing the level instead makes that unreachable with no special-case guard.
        /// </remarks>
        public RestartStep Advance(float delta)
        {
            if (Phase == RestartPhase.Playing)
            {
                return RestartStep.None;
            }

            float dim = DimTime;
            _ = Mover.MoveVariableToTarget(ref dim, 0, 1, delta);
            DimTime = dim;

            if (DimTime > 0f)
            {
                return RestartStep.None;
            }

            if (Phase == RestartPhase.FadingOut)
            {
                Phase = RestartPhase.FadingIn;
                DimTime = DimDuration;
                return RestartStep.SwapScene;
            }

            Phase = RestartPhase.Playing;
            TransitionActive = false;
            return RestartStep.Completed;
        }
    }
}
