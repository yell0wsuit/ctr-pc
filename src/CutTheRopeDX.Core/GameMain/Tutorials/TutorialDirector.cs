using System;
using System.Collections.Generic;

namespace CutTheRopeDX.GameMain.Tutorials
{
    internal readonly record struct TutorialRocketState(Rocket Rocket, CandyBody Body, int State);

    internal interface ITutorialWorld
    {
        IReadOnlyList<CandyBody> ActiveBodies { get; }

        bool Holds(TutorialEvent tutorialEvent, CandyBody body);

        IReadOnlyList<TutorialRocketState> Rockets { get; }
    }

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

        internal TutorialDirector(ITutorialWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

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

        public void CompleteLoading()
        {
            if (loadingComplete)
            {
                return;
            }

            loadingComplete = true;
            Fire(TutorialEvent.Start);
        }

        public void Fire(TutorialEvent tutorialEvent, CandyBody actor = null)
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
                if (prompt.State != TutorialPromptState.Armed || !IsEligible(prompt, actor))
                {
                    continue;
                }

                CancelGroupSiblings(prompt);
                _ = prompt.BeginDelayOrPlay();
            }
        }

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

        public void DrawTexts()
        {
            Draw(texts);
        }

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

        private bool IsEligible(TutorialPrompt prompt, CandyBody actor)
        {
            TutorialTrigger trigger = prompt.Trigger;
            if (actor is not null)
            {
                return MatchesSubject(trigger.Subject, actor)
                    && (trigger.Area is null || trigger.Area.Value.Contains(actor.Point.pos));
            }

            if (trigger.Area is null)
            {
                return true;
            }

            foreach (CandyBody body in world.ActiveBodies)
            {
                if (MatchesSubject(trigger.Subject, body) && trigger.Area.Value.Contains(body.Point.pos))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesSubject(TutorialSubject subject, CandyBody body)
        {
            return subject switch
            {
                TutorialSubject.Any => true,
                TutorialSubject.Left => body.Role == CandyBodyRole.LeftHalf,
                TutorialSubject.Right => body.Role == CandyBodyRole.RightHalf,
                TutorialSubject.Primary => IsPrimary(body),
                _ => false,
            };
        }

        private bool IsPrimary(CandyBody body)
        {
            IReadOnlyList<CandyBody> activeBodies = world.ActiveBodies;
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
                        Fire(tutorialEvent, body);
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
