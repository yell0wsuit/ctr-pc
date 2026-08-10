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

    /// <summary>Where the level sits in its mutually exclusive win/loss lifecycle.</summary>
    internal enum LevelOutcomeState
    {
        /// <summary>No outcome has started.</summary>
        Playing,

        /// <summary>A delayed loss owns the outcome, but its loss sequence has not started yet.</summary>
        PendingLoss,

        /// <summary>The win presentation is active.</summary>
        Winning,

        /// <summary>The loss presentation is active.</summary>
        Losing,

        /// <summary>The win presentation completed.</summary>
        Won,

        /// <summary>The loss presentation completed and restart dimming has begun.</summary>
        Lost,
    }

    /// <summary>
    /// Single owner of a level's lifecycle: the independent restart-dim and outcome machines.
    /// </summary>
    internal sealed class LevelFlowState
    {
        /// <summary>Seconds each dim phase lasts.</summary>
        public const float DimDuration = 0.15f;

        /// <summary>Current restart phase.</summary>
        public RestartPhase Phase { get; private set; } = RestartPhase.Playing;

        /// <summary>Remaining dim time for the current phase.</summary>
        public float DimTime { get; private set; }

        /// <summary>Current authoritative win/loss state.</summary>
        public LevelOutcomeState Outcome { get; private set; } = LevelOutcomeState.Playing;

        /// <summary>Whether the level reached either win state.</summary>
        public bool WonTriggered => Outcome is LevelOutcomeState.Winning or LevelOutcomeState.Won;

        /// <summary>Whether the level reached either loss state.</summary>
        public bool LostTriggered => Outcome is LevelOutcomeState.Losing or LevelOutcomeState.Lost;

        /// <summary>Whether a win/loss transition is currently playing.</summary>
        public bool TransitionActive => Outcome is LevelOutcomeState.PendingLoss
            or LevelOutcomeState.Winning
            or LevelOutcomeState.Losing;

        /// <summary>Whether any win/loss outcome owns the level.</summary>
        public bool HasOutcome => Outcome != LevelOutcomeState.Playing;

        /// <summary>
        /// Whether player input may start a restart. An outcome presentation is a skippable
        /// cutscene, not a lock: the player may bail out of a sad Om Nom or a chewing animation
        /// and retry immediately. Only a dim already in flight refuses.
        /// </summary>
        /// <remarks>
        /// Must stay identical to <see cref="TryBeginRestartDim"/>'s precondition. The restart
        /// button reloads the map and only then calls <c>AnimateLevelRestart</c>, so a state this
        /// property accepts but that method rejects would reload the level and never re-show it.
        /// </remarks>
        public bool CanRestart => Phase == RestartPhase.Playing;

        /// <summary>True while the screen is dimming out, when the dim overlay renders inverted.</summary>
        public bool IsFadingOut => Phase == RestartPhase.FadingOut;

        /// <summary>
        /// Whether a new terminal outcome may claim the level. A level being torn down or already
        /// owned by an outcome cannot also be won or lost.
        /// </summary>
        public bool CanTriggerOutcome => Phase != RestartPhase.FadingOut
            && Outcome == LevelOutcomeState.Playing;

        /// <summary>
        /// Whether Om Nom may react to candy or light. Suppressed after any outcome claims the
        /// level so a sad Om Nom does not chase a surviving candy.
        /// </summary>
        /// <param name="targetAlreadyFed">Whether this Om Nom has already eaten.</param>
        /// <returns><see langword="true"/> when gameplay reactions are allowed.</returns>
        public bool CanReactToCandy(bool targetAlreadyFed = false)
        {
            return Outcome == LevelOutcomeState.Playing && !targetAlreadyFed;
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
        /// Returns the outcome machine to normal play, leaving the restart phase alone.
        /// Call from scene setup, which also runs in the middle of a restart.
        /// </summary>
        public void ResetOutcome()
        {
            Outcome = LevelOutcomeState.Playing;
        }

        /// <summary>Atomically starts a manual restart or completes an active loss into restart.</summary>
        /// <returns><see langword="true"/> when restart dimming started.</returns>
        /// <remarks>
        /// Rejecting a dim that is already in flight is what makes the restart button safe during
        /// an outcome: the loss timeline's own delayed <c>AnimateLevelRestart</c> lands here after
        /// a player-initiated dim has started and must not reset it back to full.
        /// </remarks>
        public bool TryBeginRestartDim()
        {
            if (Phase != RestartPhase.Playing)
            {
                return false;
            }

            // A loss that reaches the dim has finished presenting, whether its own timeline got
            // there or the player skipped ahead. Winning is left alone: the player is abandoning
            // the win rather than completing it, and Show resets the outcome moments later.
            if (Outcome == LevelOutcomeState.Losing)
            {
                Outcome = LevelOutcomeState.Lost;
            }
            Phase = RestartPhase.FadingOut;
            DimTime = DimDuration;
            return true;
        }

        /// <summary>Atomically claims a playing level for the win sequence.</summary>
        /// <returns><see langword="true"/> when the win sequence claimed the outcome.</returns>
        public bool TryBeginWin()
        {
            if (!CanTriggerOutcome)
            {
                return false;
            }

            Outcome = LevelOutcomeState.Winning;
            return true;
        }

        /// <summary>Atomically starts an immediate or previously scheduled loss sequence.</summary>
        /// <returns><see langword="true"/> when the loss sequence claimed the outcome.</returns>
        public bool TryBeginLoss()
        {
            if (Phase == RestartPhase.FadingOut
                || Outcome is not (LevelOutcomeState.Playing or LevelOutcomeState.PendingLoss))
            {
                return false;
            }

            Outcome = LevelOutcomeState.Losing;
            return true;
        }

        /// <summary>Atomically reserves a playing level for a delayed loss.</summary>
        /// <returns><see langword="true"/> when this call reserved the outcome.</returns>
        public bool TryScheduleLoss()
        {
            if (!CanTriggerOutcome)
            {
                return false;
            }

            Outcome = LevelOutcomeState.PendingLoss;
            return true;
        }

        /// <summary>Completes the active win presentation.</summary>
        /// <returns><see langword="true"/> when an active win became complete.</returns>
        /// <remarks>
        /// A restart in flight refuses the completion. The player skipped the chewing animation to
        /// retry, so the win must not report itself and pop the results box over the dim.
        /// </remarks>
        public bool CompleteWinTransition()
        {
            if (Phase != RestartPhase.Playing || Outcome != LevelOutcomeState.Winning)
            {
                return false;
            }

            Outcome = LevelOutcomeState.Won;
            return true;
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
            return RestartStep.Completed;
        }
    }
}
