using System;
using System.Collections.Generic;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>Snapshot of one rocket, its owning candy body, and current operating state.</summary>
    /// <param name="Rocket">Rocket identity used to key transition history.</param>
    /// <param name="Body">Candy body carried by the rocket.</param>
    /// <param name="State">Current <c>STATE_ROCKET_*</c> value.</param>
    internal readonly record struct TutorialRocketState(Rocket Rocket, CandyBody Body, int State);

    /// <summary>Supplies authoritative candy and rocket state to the tutorial evaluator.</summary>
    internal interface ITutorialWorld
    {
        /// <summary>Gets the active candy bodies in authored candy order.</summary>
        IReadOnlyList<CandyBody> ActiveBodies { get; }

        /// <summary>Tests whether one sampled tutorial state holds for a candy body.</summary>
        /// <param name="tutorialEvent">State event to evaluate.</param>
        /// <param name="body">Active body to evaluate.</param>
        /// <returns><see langword="true"/> when the authoritative state holds.</returns>
        bool Holds(TutorialEvent tutorialEvent, CandyBody body);

        /// <summary>Gets one current snapshot for each tutorial-observable rocket.</summary>
        IReadOnlyList<TutorialRocketState> Rockets { get; }
    }

    /// <summary>Owns tutorial prompt registration, firing, evaluation, playback, and draw order.</summary>
    internal sealed class TutorialDirector
    {
        private readonly ITutorialWorld world;
        private readonly List<TutorialPrompt> prompts = [];
        private readonly List<TutorialPrompt> texts = [];
        private readonly List<TutorialPrompt> images = [];
        private readonly Dictionary<TutorialEvent, List<TutorialPrompt>> promptsByEvent = [];
        private readonly Dictionary<string, List<TutorialPrompt>> promptsByGroup = [];
        private readonly Dictionary<Rocket, int> rocketHistory = [];
        private bool loadingComplete;

        /// <summary>Initializes a director against an authoritative tutorial world.</summary>
        /// <param name="world">World state boundary used for sampled conditions.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="world"/> is null.</exception>
        internal TutorialDirector(ITutorialWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>Registers a prompt in XML order before loading completes.</summary>
        /// <param name="prompt">Prompt to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prompt"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown after loading has completed.</exception>
        public void Add(TutorialPrompt prompt)
        {
            ArgumentNullException.ThrowIfNull(prompt);
            if (loadingComplete)
            {
                throw new InvalidOperationException("Cannot add a tutorial prompt after loading completes.");
            }

            prompts.Add(prompt);
            (prompt.IsText ? texts : images).Add(prompt);
            AddToIndex(promptsByEvent, prompt.Trigger.Event, prompt);
            if (prompt.Group is not null)
            {
                AddToIndex(promptsByGroup, prompt.Group, prompt);
            }
        }

        /// <summary>Seals registration and dispatches the start event once.</summary>
        public void CompleteLoading()
        {
            if (loadingComplete)
            {
                return;
            }

            loadingComplete = true;
            Fire(TutorialEvent.Start);
        }

        /// <summary>Dispatches an edge or sampled event to eligible armed prompts.</summary>
        /// <param name="tutorialEvent">Event that occurred.</param>
        /// <param name="actor">Causal candy body for a scoped event, or <see langword="null"/> for an actorless event.</param>
        public void Fire(TutorialEvent tutorialEvent, CandyBody actor = null)
        {
            Fire(tutorialEvent, actor, null);
        }

        private void Fire(
            TutorialEvent tutorialEvent,
            CandyBody actor,
            IReadOnlyList<CandyBody> sampledBodies)
        {
            if (tutorialEvent == TutorialEvent.Start && !loadingComplete)
            {
                return;
            }

            if (!promptsByEvent.TryGetValue(tutorialEvent, out List<TutorialPrompt> indexedPrompts))
            {
                return;
            }

            TutorialPrompt[] snapshot = [.. indexedPrompts];
            foreach (TutorialPrompt prompt in snapshot)
            {
                if (prompt.State != TutorialPromptState.Armed || !IsEligible(prompt, actor, sampledBodies))
                {
                    continue;
                }

                CancelGroupSiblings(prompt);
                _ = prompt.BeginDelayOrPlay();
            }
        }

        /// <summary>Samples required world state and advances prompt delay and playback.</summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        public void Update(float delta)
        {
            EvaluateSampledStates();
            EvaluateRocketIgnitions();

            foreach (TutorialPrompt prompt in prompts)
            {
                if (prompt.State == TutorialPromptState.Delaying)
                {
                    float playbackDelta = prompt.AdvanceDelay(delta);
                    if (prompt.State == TutorialPromptState.Playing && playbackDelta > 0f)
                    {
                        prompt.AdvancePlayback(playbackDelta);
                    }
                }
                else if (prompt.State == TutorialPromptState.Playing)
                {
                    prompt.AdvancePlayback(delta);
                }
            }
        }

        /// <summary>Draws tutorial text visuals in XML order.</summary>
        public void DrawTexts()
        {
            Draw(texts);
        }

        /// <summary>Draws tutorial image visuals in XML order.</summary>
        public void DrawImages()
        {
            Draw(images);
        }

        private static void AddToIndex<TKey>(
            Dictionary<TKey, List<TutorialPrompt>> index,
            TKey key,
            TutorialPrompt prompt)
        {
            if (!index.TryGetValue(key, out List<TutorialPrompt> values))
            {
                values = [];
                index.Add(key, values);
            }

            values.Add(prompt);
        }

        private static void Draw(List<TutorialPrompt> orderedPrompts)
        {
            foreach (TutorialPrompt prompt in orderedPrompts)
            {
                prompt.Visual.Draw();
            }
        }

        private void CancelGroupSiblings(TutorialPrompt selected)
        {
            if (selected.Group is null)
            {
                return;
            }

            foreach (TutorialPrompt sibling in promptsByGroup[selected.Group])
            {
                if (sibling != selected)
                {
                    sibling.Cancel();
                }
            }
        }

        private bool IsEligible(
            TutorialPrompt prompt,
            CandyBody actor,
            IReadOnlyList<CandyBody> sampledBodies)
        {
            TutorialTrigger trigger = prompt.Trigger;
            if (actor is not null)
            {
                return MatchesSubject(trigger.Subject, actor, sampledBodies)
                    && (trigger.Area is null || trigger.Area.Value.Contains(actor.Point.pos));
            }

            if (trigger.Area is null)
            {
                return true;
            }

            IReadOnlyList<CandyBody> activeBodies = sampledBodies ?? world.ActiveBodies;
            foreach (CandyBody body in activeBodies)
            {
                if (MatchesSubject(trigger.Subject, body, activeBodies)
                    && trigger.Area.Value.Contains(body.Point.pos))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesSubject(
            TutorialSubject subject,
            CandyBody body,
            IReadOnlyList<CandyBody> sampledBodies)
        {
            return subject switch
            {
                TutorialSubject.Any => true,
                TutorialSubject.Left => body.Role == CandyBodyRole.LeftHalf,
                TutorialSubject.Right => body.Role == CandyBodyRole.RightHalf,
                TutorialSubject.Primary => IsPrimary(body, sampledBodies),
                _ => false,
            };
        }

        private bool IsPrimary(CandyBody body, IReadOnlyList<CandyBody> sampledBodies)
        {
            IReadOnlyList<CandyBody> activeBodies = sampledBodies ?? world.ActiveBodies;
            if (activeBodies.Count == 0)
            {
                return false;
            }

            CandyBody first = activeBodies[0];
            return first.Owner is null
                ? body == first
                : body.Owner == first.Owner;
        }

        private void EvaluateSampledStates()
        {
            List<TutorialEvent> neededEvents = [];
            foreach ((TutorialEvent tutorialEvent, List<TutorialPrompt> indexedPrompts) in promptsByEvent)
            {
                if (TutorialEvents.Observation(tutorialEvent) == TutorialObservation.Sampled
                    && HasArmedPrompt(indexedPrompts))
                {
                    neededEvents.Add(tutorialEvent);
                }
            }

            if (neededEvents.Count == 0)
            {
                return;
            }

            IReadOnlyList<CandyBody> activeBodies = world.ActiveBodies;
            foreach (TutorialEvent tutorialEvent in neededEvents)
            {
                foreach (CandyBody body in activeBodies)
                {
                    if (world.Holds(tutorialEvent, body))
                    {
                        Fire(tutorialEvent, body, activeBodies);
                    }
                }
            }
        }

        private void EvaluateRocketIgnitions()
        {
            if (!promptsByEvent.TryGetValue(TutorialEvent.RocketIgnite, out List<TutorialPrompt> rocketPrompts)
                || !HasArmedPrompt(rocketPrompts))
            {
                return;
            }

            IReadOnlyList<TutorialRocketState> rockets = world.Rockets;
            HashSet<Rocket> currentRockets = [];
            foreach (TutorialRocketState rocketState in rockets)
            {
                _ = currentRockets.Add(rocketState.Rocket);
                bool wasFlying = rocketHistory.TryGetValue(rocketState.Rocket, out int priorState)
                    && priorState == Rocket.STATE_ROCKET_FLY;
                rocketHistory[rocketState.Rocket] = rocketState.State;
                if (!wasFlying && rocketState.State == Rocket.STATE_ROCKET_FLY)
                {
                    Fire(TutorialEvent.RocketIgnite, rocketState.Body);
                }
            }

            List<Rocket> staleRockets = [];
            foreach (Rocket rocket in rocketHistory.Keys)
            {
                if (!currentRockets.Contains(rocket))
                {
                    staleRockets.Add(rocket);
                }
            }

            foreach (Rocket rocket in staleRockets)
            {
                _ = rocketHistory.Remove(rocket);
            }
        }

        private static bool HasArmedPrompt(List<TutorialPrompt> indexedPrompts)
        {
            foreach (TutorialPrompt prompt in indexedPrompts)
            {
                if (prompt.State == TutorialPromptState.Armed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
