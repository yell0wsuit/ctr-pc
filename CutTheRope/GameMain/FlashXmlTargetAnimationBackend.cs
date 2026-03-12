using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class FlashXmlTargetAnimationBackend : ITargetAnimationBackend, ITimelineDelegate
    {
        private const float BaseTargetScale = 1.73f;
        private const float FlashXmlFrameDurationSeconds = 1f / 30f;
        private const int SleepOverlayTimeline = 0;
        private const float IosSlowPlaybackRate = 0.6f;
        private const string PirateSkinName = "OM_NOM_PIRATE";
        private const float PirateBubbleSpawnDelaySeconds = 0.932203f;
        private readonly List<Image> parts = [];
        private readonly List<Image> _sleepOverlayParts = [];
        private readonly List<PirateBubbleOverlayInstance> _activeBubbleOverlays = [];
        private readonly FlashXmlAnimationDefinition _definition;
        private readonly FlashXmlAnimationDefinition _bubbleOverlayDefinition;
        private readonly FlashXmlIdleCadenceClock _idleCadenceClock = new();
        private readonly FlashXmlStageRoot _sleepOverlayObject;
        private readonly OmNomSkinDefinition _skinDefinition;
        private ITimelineDelegate _externalTimelineDelegate;
        private int _activeTimelineId = -1;
        private Timeline _driverTimeline;
        private int _driverTimelineId = -1;
        private float _driverTimelineDurationSeconds;
        private float _driverTimelinePlaybackRate = 1f;
        private bool _driverTimelineUsesRootDefinition;
        private float _additionalOverlayX;
        private float _additionalOverlayY;
        private float _pendingPirateBubbleDelaySeconds = -1f;
        private bool _skipIdleToSleepTransitionUntilWake = true;

        public FlashXmlTargetAnimationBackend(OmNomSkinDefinition skinDefinition)
        {
            _skinDefinition = skinDefinition;

            _definition = FlashXmlImporter.ParseFile(skinDefinition.AnimationXmlPath);

            TargetObject = CreateStageRoot(_definition);
            int idleLoopId = _skinDefinition.GetTimelineId(TargetAnimationState.IdleLoop);
            int sleepingId = _skinDefinition.GetTimelineId(TargetAnimationState.Sleeping);
            BuildParts(_definition, TargetObject, parts, idleLoopId, sleepingId);
            BuildRootTimelines(_definition, TargetObject, idleLoopId, sleepingId);

            FlashXmlAnimationDefinition sleepOverlayDefinition = FlashXmlImporter.ParseFile(
                ContentPaths.GetAnimationXmlAbsolutePath("fx_sleep.xml"));
            _sleepOverlayObject = CreateStageRoot(sleepOverlayDefinition);
            _sleepOverlayObject.visible = false;
            BuildParts(sleepOverlayDefinition, _sleepOverlayObject, _sleepOverlayParts, SleepOverlayTimeline, -1);
            BuildRootTimelines(sleepOverlayDefinition, _sleepOverlayObject, SleepOverlayTimeline, -1);

            _bubbleOverlayDefinition = string.Equals(_skinDefinition.Id, PirateSkinName, StringComparison.Ordinal)
                ? FlashXmlImporter.ParseFile(
                    ContentPaths.GetAnimationXmlAbsolutePath("fx_bubbles.xml"))
                : null;
        }

        public GameObject TargetObject { get; }

        private static FlashXmlStageRoot CreateStageRoot(FlashXmlAnimationDefinition definition)
        {
            FlashXmlStageRoot stageRoot = new();
            _ = stageRoot.InitWithTexture(Application.GetTexture(Resources.Img.CharAnimationsSmooth));
            stageRoot.SetDrawQuad(0);
            stageRoot.color = RGBAColor.transparentRGBA;
            stageRoot.passColorToChilds = false;
            stageRoot.scaleX = BaseTargetScale;
            stageRoot.scaleY = BaseTargetScale;

            // Use the Flash stage center as the anchor point. All skins share the
            // same stage dimensions (550x400), so this keeps every skin at the same
            // position without per-skin centroid calculation.
            const float classicBodyScreenOffsetX = -6f;
            const float classicBodyScreenOffsetY = -6f;
            stageRoot.useCustomAnchor = true;
            stageRoot.customAnchorX = -classicBodyScreenOffsetX / BaseTargetScale;
            stageRoot.customAnchorY = -classicBodyScreenOffsetY / BaseTargetScale;
            stageRoot.width = (int)MathF.Round(definition.StageWidth);
            stageRoot.height = (int)MathF.Round(definition.StageHeight);
            return stageRoot;
        }

        public float GetTargetBaseScaleX()
        {
            return BaseTargetScale;
        }

        public float GetTargetBaseScaleY()
        {
            return BaseTargetScale;
        }

        public bool StartsWithGreeting => _skinDefinition.StartWithGreeting;

        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            _externalTimelineDelegate = timelineDelegate;

            if (_skinDefinition.StartWithGreeting && TryMapState(TargetAnimationState.Greeting, out _))
            {
                Play(TargetAnimationState.Greeting);
            }
            else
            {
                Play(TargetAnimationState.IdleLoop);
            }
        }

        public void Play(TargetAnimationState state)
        {
            if (state == TargetAnimationState.Sleeping && TryPlayIdleToSleepTransition())
            {
                UpdatePirateBubbleScheduleForState(state);
                return;
            }

            if (!TryMapState(state, out int timelineId))
            {
                return;
            }

            PlayTimelineById(timelineId);
            UpdateIdleToSleepTransitionAvailability(state);
            UpdatePirateBubbleScheduleForState(state);
        }

        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            if (_skinDefinition.IdleVariants.Length == 0)
            {
                return;
            }

            int index = rng(0, _skinDefinition.IdleVariants.Length - 1);
            PlayTimelineById(_skinDefinition.IdleVariants[index]);
        }

        internal void SkipCurrentTimelineFrames(int frameCount)
        {
            if (_activeTimelineId < 0 || frameCount <= 0)
            {
                return;
            }

            float skipSeconds = frameCount * FlashXmlFrameDurationSeconds;
            SeekCurrentTimeline(skipSeconds);
        }

        private void PlayTimelineById(int timelineId)
        {
            _activeTimelineId = timelineId;
            if (TargetObject is FlashXmlStageRoot stageRoot)
            {
                stageRoot.PlaybackRate = GetTimelinePlaybackRate(_skinDefinition, timelineId);
            }

            PlayTimeline(parts, timelineId);
            PlayRootTimeline(TargetObject, timelineId);
            BindDriverDelegateForTimeline(timelineId);
        }

        public bool IsPlaying(TargetAnimationState state)
        {
            if (!TryMapState(state, out int timelineId))
            {
                return false;
            }

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].GetTimeline(timelineId) != null)
                {
                    return parts[i].GetCurrentTimelineIndex() == timelineId
                        && parts[i].GetCurrentTimeline()?.state == Timeline.TimelineState.TIMELINE_PLAYING;
                }
            }

            return false;
        }

        public float GetSleepPulseDelaySeconds()
        {
            int sleepingTimelineId = _skinDefinition.GetTimelineId(TargetAnimationState.Sleeping);
            float sleepingDuration = sleepingTimelineId >= 0 && _definition.RootTimelines.TryGetValue(sleepingTimelineId, out float duration)
                ? duration
                : 0f;

            if (!ShouldUseIdleToSleepTransition()
                || _activeTimelineId == sleepingTimelineId
                || !TryMapState(TargetAnimationState.IdleToSleep, out int idleToSleepTimelineId))
            {
                return sleepingDuration;
            }

            float idleToSleepDuration = GetTimelineDurationSeconds(idleToSleepTimelineId);
            float idleToSleepSkipSeconds = GetIdleToSleepSkipSeconds(idleToSleepDuration);

            return MathF.Max(0f, idleToSleepDuration - idleToSleepSkipSeconds) + sleepingDuration;
        }

        public void ResetBlink()
        {
        }

        public void TriggerBlink()
        {
        }

        public void UpdateSleepOverlays(float delta)
        {
            if (_sleepOverlayObject.visible)
            {
                _sleepOverlayObject.Update(delta);
            }
        }

        public void SyncSleepOverlayPosition(float x, float y)
        {
            _sleepOverlayObject.x = x;
            _sleepOverlayObject.y = y;
        }

        public void UpdateAdditionalOverlays(float delta)
        {
            UpdatePirateBubbleOverlays(delta);
        }

        public void SyncAdditionalOverlayPosition(float x, float y)
        {
            _additionalOverlayX = x;
            _additionalOverlayY = y;
        }

        public void SetSleepOverlayVisible(bool visible)
        {
            _sleepOverlayObject.visible = visible;
            if (visible)
            {
                PlayTimeline(_sleepOverlayParts, SleepOverlayTimeline);
                PlayRootTimeline(_sleepOverlayObject, SleepOverlayTimeline);
                _pendingPirateBubbleDelaySeconds = -1f;
                _activeBubbleOverlays.Clear();
            }
        }

        public void DrawSleepOverlays()
        {
            if (_sleepOverlayObject.visible)
            {
                _sleepOverlayObject.Draw();
            }

            for (int i = 0; i < _activeBubbleOverlays.Count; i++)
            {
                _activeBubbleOverlays[i].RootObject.Draw();
            }
        }

        public bool HandlesOwnSleepPulse => true;

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (_driverTimeline == null || !ReferenceEquals(t, _driverTimeline))
            {
                return;
            }

            if (_driverTimelineUsesRootDefinition)
            {
                _externalTimelineDelegate?.TimelinereachedKeyFramewithIndex(t, k, i);
                return;
            }

            if (_driverTimelineId == _skinDefinition.GetTimelineId(TargetAnimationState.IdleLoop)
                && _externalTimelineDelegate != null)
            {
                int syntheticTicks = _idleCadenceClock.Advance(t.time, _driverTimelineDurationSeconds, _driverTimelinePlaybackRate);
                for (int tick = 0; tick < syntheticTicks; tick++)
                {
                    _externalTimelineDelegate.TimelinereachedKeyFramewithIndex(t, k, 1);
                }
            }

        }

        public void TimelineFinished(Timeline t)
        {
            if (_driverTimeline == null || !ReferenceEquals(t, _driverTimeline))
            {
                return;
            }

            int finishedTimelineId = _driverTimelineId;

            _driverTimeline = null;
            _driverTimelineId = -1;

            if (TryGetCompletionTargetTimelineId(_skinDefinition, finishedTimelineId, out int followupTimelineId)
                && FindFirstPartWithTimeline(followupTimelineId) != null)
            {
                PlayTimelineById(followupTimelineId);
            }
        }

        private static bool TryGetCompletionTargetTimelineId(OmNomSkinDefinition skinDefinition,
            int finishedTimelineId, out int followupTimelineId)
        {
            return skinDefinition.TryGetFollowupTimeline(finishedTimelineId, out followupTimelineId);
        }

        private static float GetTimelinePlaybackRate(OmNomSkinDefinition skinDefinition, int activeTimelineId)
        {
            return activeTimelineId < 0
                ? 1f
                : Array.IndexOf(skinDefinition.SlowTimelineIds, activeTimelineId) >= 0
                ? IosSlowPlaybackRate
                : 1f;
        }

        private static void BuildParts(FlashXmlAnimationDefinition definition, GameObject rootObject,
            List<Image> targetParts, int idleLoopTimelineId, int sleepingTimelineId)
        {
            // First pass: create all parts so cross-part action targets can be resolved.
#pragma warning disable IDE0028
            Dictionary<string, Image> partsByName = new(definition.Parts.Count);
#pragma warning restore IDE0028
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition partDefinition = definition.Parts[i];

                FlashXmlImage part = FlashXmlImage.CreateWithResID(partDefinition.TextureResourceName);
                part.anchor = 9;
                part.parentAnchor = 9;
                part.visible = ShouldStartVisible(partDefinition, idleLoopTimelineId);
                part.useCustomAnchor = true;
                part.customAnchorX = partDefinition.AnchorX;
                part.customAnchorY = partDefinition.AnchorY;
                part.rotationCenterX = partDefinition.RotationCenterX;
                part.rotationCenterY = partDefinition.RotationCenterY;
                part.SetDrawQuad(partDefinition.QuadToDraw);

                _ = rootObject.AddChild(part);
                targetParts.Add(part);

                if (!string.IsNullOrEmpty(partDefinition.Name))
                {
                    partsByName[partDefinition.Name] = part;
                }
            }

            // Second pass: build timelines now that all parts exist for cross-part linking.
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                BuildTimelines((FlashXmlImage)targetParts[i], definition.Parts[i], partsByName,
                    idleLoopTimelineId, sleepingTimelineId);
            }
        }

        private static void BuildRootTimelines(FlashXmlAnimationDefinition definition, GameObject rootObject,
            int idleLoopTimelineId, int sleepingTimelineId)
        {
            foreach ((int timelineId, FlashXmlRootTimelineDefinition timelineDefinition) in definition.RootTimelineDefinitions)
            {
                Timeline timeline = CreateRootDriverTimeline(timelineDefinition);
                if (timelineId == idleLoopTimelineId || timelineId == sleepingTimelineId)
                {
                    timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                }

                rootObject.AddTimelinewithID(timeline, timelineId);
            }
        }

        private static void PlayTimeline(List<Image> targetParts, int timelineId)
        {
            for (int i = 0; i < targetParts.Count; i++)
            {
                Timeline timeline = targetParts[i].GetTimeline(timelineId);
                if (timeline != null)
                {
                    targetParts[i].visible = true;
                    targetParts[i].PlayTimeline(timelineId);
                }
                else
                {
                    // Stop all timelines before playing the new one. Parts without the
                    // requested timeline are stopped and hidden so they do not keep
                    // ticking invisibly with a stale pose from a previous one-shot.
                    if (targetParts[i].GetCurrentTimeline() != null)
                    {
                        targetParts[i].StopCurrentTimeline();
                    }

                    targetParts[i].visible = false;
                }
            }
        }

        private static void PlayRootTimeline(GameObject rootObject, int timelineId)
        {
            if (rootObject.GetTimeline(timelineId) != null)
            {
                rootObject.PlayTimeline(timelineId);
            }
            else if (rootObject.GetCurrentTimeline() != null)
            {
                rootObject.StopCurrentTimeline();
            }
        }

        private static bool IsTimelinePlaying(List<Image> targetParts, int timelineId)
        {
            for (int i = 0; i < targetParts.Count; i++)
            {
                Timeline timeline = targetParts[i].GetTimeline(timelineId);
                if (timeline == null)
                {
                    continue;
                }

                return targetParts[i].GetCurrentTimelineIndex() == timelineId
                    && targetParts[i].GetCurrentTimeline()?.state == Timeline.TimelineState.TIMELINE_PLAYING;
            }

            return false;
        }

        private bool TryPlayIdleToSleepTransition()
        {
            if (!ShouldUseIdleToSleepTransition()
                || _activeTimelineId == _skinDefinition.GetTimelineId(TargetAnimationState.Sleeping)
                || !TryMapState(TargetAnimationState.IdleToSleep, out int idleToSleepTimelineId))
            {
                return false;
            }

            PlayTimelineById(idleToSleepTimelineId);

            float trimSeconds = GetIdleToSleepSkipSeconds(GetTimelineDurationSeconds(idleToSleepTimelineId));
            if (trimSeconds > 0f)
            {
                SeekTimeline(parts, idleToSleepTimelineId, trimSeconds);
            }

            return true;
        }

        private bool ShouldUseIdleToSleepTransition()
        {
            return !_skipIdleToSleepTransitionUntilWake;
        }

        private float GetIdleToSleepSkipSeconds(float idleToSleepDurationSeconds)
        {
            float skippedDurationSeconds = _skinDefinition.IdleToSleepTrimFrames * FlashXmlFrameDurationSeconds;
            return MathF.Min(skippedDurationSeconds, idleToSleepDurationSeconds);
        }

        private void UpdateIdleToSleepTransitionAvailability(TargetAnimationState state)
        {
            if (state is not TargetAnimationState.Sleeping and not TargetAnimationState.IdleLoop)
            {
                _skipIdleToSleepTransitionUntilWake = false;
            }
        }

        private void SeekTimeline(List<Image> targetParts, int timelineId, float timeSeconds)
        {
            float durationSeconds = GetTimelineDurationSeconds(timelineId);
            float clampedTimeSeconds = durationSeconds > 0f
                ? MathF.Min(timeSeconds, MathF.Max(0f, durationSeconds - FlashXmlFrameDurationSeconds))
                : timeSeconds;

            for (int i = 0; i < targetParts.Count; i++)
            {
                Timeline timeline = targetParts[i].GetTimeline(timelineId);
                if (timeline == null || targetParts[i].GetCurrentTimelineIndex() != timelineId)
                {
                    continue;
                }

                timeline.DeactivateTracks();
                timeline.time = clampedTimeSeconds;
                Timeline.UpdateTimeline(timeline, 0f);
            }
        }

        private void SeekCurrentTimeline(float timeSeconds)
        {
            int timelineId = _activeTimelineId;
            float durationSeconds = GetTimelineDurationSeconds(timelineId);
            float clampedTimeSeconds = durationSeconds > 0f
                ? MathF.Min(timeSeconds, MathF.Max(0f, durationSeconds - FlashXmlFrameDurationSeconds))
                : timeSeconds;

            Timeline rootTimeline = TargetObject.GetTimeline(timelineId);
            if (rootTimeline != null && TargetObject.GetCurrentTimelineIndex() == timelineId)
            {
                rootTimeline.DeactivateTracks();
                rootTimeline.time = clampedTimeSeconds;
                Timeline.UpdateTimeline(rootTimeline, 0f);
            }

            SeekTimeline(parts, timelineId, clampedTimeSeconds);
        }

        private void UpdatePirateBubbleOverlays(float delta)
        {
            if (_bubbleOverlayDefinition == null)
            {
                return;
            }

            if (_pendingPirateBubbleDelaySeconds >= 0f)
            {
                _pendingPirateBubbleDelaySeconds -= delta;
                if (_pendingPirateBubbleDelaySeconds <= 0f)
                {
                    TriggerPirateBubbleOverlay();
                    float loopInterval = GetPirateBubbleLoopIntervalSeconds();
                    _pendingPirateBubbleDelaySeconds = loopInterval > 0f
                        ? _pendingPirateBubbleDelaySeconds + loopInterval
                        : -1f;
                }
            }

            for (int i = _activeBubbleOverlays.Count - 1; i >= 0; i--)
            {
                PirateBubbleOverlayInstance overlay = _activeBubbleOverlays[i];
                overlay.RootObject.Update(delta);
                if (!IsTimelinePlaying(overlay.Parts, SleepOverlayTimeline))
                {
                    _activeBubbleOverlays.RemoveAt(i);
                }
            }
        }

        private void TriggerPirateBubbleOverlay()
        {
            GameObject rootObject = CreateStageRoot(_bubbleOverlayDefinition);
            rootObject.x = _additionalOverlayX;
            rootObject.y = _additionalOverlayY;

            List<Image> overlayParts = [];
            BuildParts(_bubbleOverlayDefinition, rootObject, overlayParts, -1, -1);
            BuildRootTimelines(_bubbleOverlayDefinition, rootObject, -1, -1);
            PlayTimeline(overlayParts, SleepOverlayTimeline);
            PlayRootTimeline(rootObject, SleepOverlayTimeline);
            _activeBubbleOverlays.Add(new PirateBubbleOverlayInstance(rootObject, overlayParts));
        }

        private static float GetPirateBubbleLoopIntervalSeconds()
        {
            return CTRMathHelper.RND_RANGE(1, 4);
        }

        private void UpdatePirateBubbleScheduleForState(TargetAnimationState state)
        {
            if (_bubbleOverlayDefinition == null)
            {
                return;
            }

            if (state == TargetAnimationState.Sleeping)
            {
                _pendingPirateBubbleDelaySeconds = -1f;
                _activeBubbleOverlays.Clear();
                return;
            }

            if (_pendingPirateBubbleDelaySeconds < 0f)
            {
                _pendingPirateBubbleDelaySeconds = PirateBubbleSpawnDelaySeconds;
            }
        }

        private static bool ShouldStartVisible(FlashXmlPartDefinition partDefinition, int idleLoopTimelineId)
        {
            return idleLoopTimelineId >= 0 && partDefinition.Timelines.ContainsKey(idleLoopTimelineId);
        }

        private Image FindFirstPartWithTimeline(int timelineId)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].GetTimeline(timelineId) != null)
                {
                    return parts[i];
                }
            }

            return null;
        }

        private void BindDriverDelegateForTimeline(int timelineId)
        {
            _driverTimeline = null;
            _driverTimelineId = -1;
            _driverTimelineDurationSeconds = 0f;
            _driverTimelinePlaybackRate = 1f;
            _driverTimelineUsesRootDefinition = false;
            _idleCadenceClock.Reset();

            Timeline rootTimeline = TargetObject.GetTimeline(timelineId);
            if (rootTimeline != null)
            {
                rootTimeline.delegateTimelineDelegate = this;
                _driverTimeline = rootTimeline;
                _driverTimelineId = timelineId;
                _driverTimelineDurationSeconds = GetTimelineDurationSeconds(timelineId);
                _driverTimelineUsesRootDefinition = true;
                return;
            }

            Image delegateDriver = FindBestDriverPartWithTimeline(timelineId);
            Timeline timeline = delegateDriver?.GetTimeline(timelineId);
            if (timeline == null)
            {
                return;
            }

            if (timelineId == _skinDefinition.GetTimelineId(TargetAnimationState.IdleLoop))
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
                _driverTimelineDurationSeconds = GetTimelineDurationSeconds(delegateDriver, timelineId);
                _driverTimelinePlaybackRate = GetTimelinePlaybackRate(_skinDefinition, timelineId);
                return;
            }

            if (_skinDefinition.ShouldBindFollowupDelegate(timelineId))
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
            }
        }

        private float GetTimelineDurationSeconds(Image part, int timelineId)
        {
            for (int i = 0; i < parts.Count && i < _definition.Parts.Count; i++)
            {
                if (!ReferenceEquals(parts[i], part))
                {
                    continue;
                }

                if (_definition.Parts[i].Timelines.TryGetValue(timelineId, out FlashXmlTimelineDefinition timelineDefinition))
                {
                    return ComputeTimelineDurationSeconds(timelineDefinition);
                }

                break;
            }

            return 0f;
        }

        private float GetTimelineDurationSeconds(int timelineId)
        {
            if (_definition.RootTimelines.TryGetValue(timelineId, out float rootDuration))
            {
                return rootDuration;
            }

            Image driverPart = FindBestDriverPartWithTimeline(timelineId);
            return driverPart != null
                ? GetTimelineDurationSeconds(driverPart, timelineId)
                : 0f;
        }

        private Image FindBestDriverPartWithTimeline(int timelineId)
        {
            const float epsilon = 0.0001f;
            bool hasRootDuration = _definition.RootTimelines.TryGetValue(timelineId, out float rootDuration);

            Image bestPart = null;
            float bestScore = float.MaxValue;
            float bestDuration = -1f;

            for (int i = 0; i < parts.Count && i < _definition.Parts.Count; i++)
            {
                if (parts[i].GetTimeline(timelineId) == null)
                {
                    continue;
                }

                if (!_definition.Parts[i].Timelines.TryGetValue(timelineId, out FlashXmlTimelineDefinition timelineDefinition))
                {
                    continue;
                }

                float duration = ComputeTimelineDurationSeconds(timelineDefinition);
                if (hasRootDuration)
                {
                    float score = MathF.Abs(duration - rootDuration);
                    bool isBetter = score < bestScore - epsilon
                        || (MathF.Abs(score - bestScore) <= epsilon && duration > bestDuration + epsilon);
                    if (isBetter)
                    {
                        bestPart = parts[i];
                        bestScore = score;
                        bestDuration = duration;
                    }
                }
                else if (duration > bestDuration + epsilon)
                {
                    bestPart = parts[i];
                    bestDuration = duration;
                }
            }

            return bestPart ?? FindFirstPartWithTimeline(timelineId);
        }

        private static float ComputeTimelineDurationSeconds(FlashXmlTimelineDefinition timelineDefinition)
        {
            float positionDuration = SumTimeOffsets(timelineDefinition.PositionKeyFrames);
            float scaleDuration = SumTimeOffsets(timelineDefinition.ScaleKeyFrames);
            float rotationDuration = SumTimeOffsets(timelineDefinition.RotationKeyFrames);
            float skewDuration = SumTimeOffsets(timelineDefinition.SkewKeyFrames);
            float colorDuration = SumTimeOffsets(timelineDefinition.ColorKeyFrames);
            float actionDuration = SumTimeOffsets(timelineDefinition.ActionKeyFrames);
            return MathF.Max(
                MathF.Max(MathF.Max(positionDuration, scaleDuration), MathF.Max(rotationDuration, skewDuration)),
                MathF.Max(colorDuration, actionDuration));
        }

        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat2KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat4KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat1KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        private static float SumTimeOffsets(IReadOnlyList<FlashXmlActionGroupKeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        private static void BuildTimelines(FlashXmlImage part, FlashXmlPartDefinition partDefinition,
            Dictionary<string, Image> partsByName, int idleLoopTimelineId, int sleepingTimelineId)
        {
            foreach ((int timelineId, FlashXmlTimelineDefinition timelineDefinition) in partDefinition.Timelines)
            {
                int maxKeyFrames = Math.Max(
                    Math.Max(timelineDefinition.PositionKeyFrames.Count, timelineDefinition.ScaleKeyFrames.Count),
                    Math.Max(
                        Math.Max(timelineDefinition.RotationKeyFrames.Count, timelineDefinition.SkewKeyFrames.Count),
                        Math.Max(timelineDefinition.ColorKeyFrames.Count, timelineDefinition.ActionKeyFrames.Count)));
                if (maxKeyFrames == 0)
                {
                    continue;
                }

                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(maxKeyFrames + 2);

                for (int i = 0; i < timelineDefinition.PositionKeyFrames.Count; i++)
                {
                    FlashXmlFloat2KeyFrame frame = timelineDefinition.PositionKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakePos(
                        frame.X,
                        frame.Y,
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.ScaleKeyFrames.Count; i++)
                {
                    FlashXmlFloat2KeyFrame frame = timelineDefinition.ScaleKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakeScale(
                        frame.X,
                        frame.Y,
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.RotationKeyFrames.Count; i++)
                {
                    FlashXmlFloat1KeyFrame frame = timelineDefinition.RotationKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakeRotation(
                        frame.Value,
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.SkewKeyFrames.Count; i++)
                {
                    FlashXmlFloat2KeyFrame frame = timelineDefinition.SkewKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakeSkew(
                        frame.X,
                        frame.Y,
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.ColorKeyFrames.Count; i++)
                {
                    FlashXmlFloat4KeyFrame frame = timelineDefinition.ColorKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakeColor(
                        new RGBAColor(frame.A, frame.B, frame.C, frame.D),
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.ActionKeyFrames.Count; i++)
                {
                    FlashXmlActionGroupKeyFrame frame = timelineDefinition.ActionKeyFrames[i];
                    List<CTRAction> actions = [];

                    for (int actionIndex = 0; actionIndex < frame.Actions.Count; actionIndex++)
                    {
                        FlashXmlActionCommand action = frame.Actions[actionIndex];
                        CTRAction ctrAction = BuildAction(part, action, partsByName);
                        if (ctrAction != null)
                        {
                            actions.Add(ctrAction);
                        }
                    }

                    if (actions.Count > 0)
                    {
                        timeline.AddKeyFrame(KeyFrame.MakeAction(actions, frame.TimeOffset));
                    }
                }

                if (timelineId == idleLoopTimelineId || timelineId == sleepingTimelineId)
                {
                    timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                }

                part.AddTimelinewithID(timeline, timelineId);
            }

            for (int i = 0; i < partDefinition.EmptyTimelineIds.Count; i++)
            {
                int emptyTimelineId = partDefinition.EmptyTimelineIds[i];
                if (part.GetTimeline(emptyTimelineId) != null)
                {
                    continue;
                }

                Timeline hiddenTimeline = CreateHiddenTimeline();
                hiddenTimeline.GetTrack(Track.TrackType.TRACK_ACTION).keyFrames[0].value.action.actionSet[0].actionTarget = part;
                if (emptyTimelineId == idleLoopTimelineId || emptyTimelineId == sleepingTimelineId)
                {
                    hiddenTimeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                }

                part.AddTimelinewithID(hiddenTimeline, emptyTimelineId);
            }
        }

        private static Timeline CreateHiddenTimeline()
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeSingleAction(new BaseElement(), BaseElement.ACTION_SET_VISIBLE, 0, 0, 0f));
            return timeline;
        }

        private static Timeline CreateRootDriverTimeline(FlashXmlRootTimelineDefinition timelineDefinition)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(timelineDefinition.ActionKeyFrames.Count + 2);
            for (int i = 0; i < timelineDefinition.ActionKeyFrames.Count; i++)
            {
                FlashXmlActionGroupKeyFrame actionFrame = timelineDefinition.ActionKeyFrames[i];
                timeline.AddKeyFrame(KeyFrame.MakeAction([], actionFrame.TimeOffset));
            }

            return timeline;
        }

        private static CTRAction BuildAction(Image part, FlashXmlActionCommand action, Dictionary<string, Image> partsByName)
        {
            Image target;
            if (action.Target == "self")
            {
                target = part;
            }
            else if (!partsByName.TryGetValue(action.Target, out target))
            {
                return null;
            }

            return action.Command switch
            {
                "AC_SDQ" => CTRAction.CreateAction(
                    target,
                    Image.ACTION_SET_DRAWQUAD,
                    ParseActionInt(action.Param1),
                    0),
                "AC_SV" => CTRAction.CreateAction(
                    target,
                    BaseElement.ACTION_SET_VISIBLE,
                    0,
                    ParseActionInt(action.Param2)),
                "AC_SAP" => CTRAction.CreateAction(
                    target,
                    BaseElement.ACTION_SET_CUSTOM_ANCHOR,
                    ParseActionFloat(action.Param1),
                    ParseActionFloat(action.Param2)),
                "AC_SRC" => CTRAction.CreateAction(
                    target,
                    BaseElement.ACTION_SET_ROTATION_CENTER,
                    ParseActionFloat(action.Param1),
                    ParseActionFloat(action.Param2)),
                _ => null
            };
        }

        private static int ParseActionInt(string raw)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int integerValue)
                ? integerValue
                : float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue) ? (int)MathF.Round(floatValue) : 0;
        }

        private static float ParseActionFloat(string raw)
        {
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue)
                ? floatValue
                : 0f;
        }

        private static KeyFrame.TransitionType MapTransition(int interpolation)
        {
            return interpolation switch
            {
                // Match iOS Flash runtime interpolation codes:
                // 0=linear, 1=immediate, 2=ease-in, 3=ease-out, 4/5=custom easing, 6=hold.
                0 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_LINEAR,
                1 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_IMMEDIATE,
                2 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN,
                3 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_OUT,
                4 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN_OUT,
                5 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_MIRRORED,
                6 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_HOLD,
                _ => KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR
            };
        }

        private bool TryMapState(TargetAnimationState state, out int timelineId)
        {
            timelineId = _skinDefinition.GetTimelineId(state);
            return timelineId >= 0;
        }

        private sealed class FlashXmlIdleCadenceClock
        {
            private const float IdleTickSeconds = 1f;
            private const float Epsilon = 0.0001f;
            private float _accumulatedWallSeconds;
            private float _lastTimelineTime;
            private bool _initialized;

            public int Advance(float currentTimelineTime, float loopDurationSeconds, float playbackRate)
            {
                float timelineDelta;
                if (!_initialized)
                {
                    _initialized = true;
                    timelineDelta = MathF.Max(currentTimelineTime, 0f);
                }
                else
                {
                    timelineDelta = currentTimelineTime - _lastTimelineTime;
                    if (timelineDelta < -Epsilon && loopDurationSeconds > Epsilon)
                    {
                        timelineDelta += loopDurationSeconds;
                    }
                    else if (timelineDelta < 0f)
                    {
                        timelineDelta = 0f;
                    }
                }

                _lastTimelineTime = currentTimelineTime;
                if (timelineDelta <= Epsilon)
                {
                    return 0;
                }

                float effectivePlaybackRate = playbackRate > Epsilon ? playbackRate : 1f;
                _accumulatedWallSeconds += timelineDelta / effectivePlaybackRate;

                int tickCount = (int)(_accumulatedWallSeconds / IdleTickSeconds);
                if (tickCount > 0)
                {
                    _accumulatedWallSeconds -= tickCount * IdleTickSeconds;
                }

                return tickCount;
            }

            public void Reset()
            {
                _accumulatedWallSeconds = 0f;
                _lastTimelineTime = 0f;
                _initialized = false;
            }
        }

        private sealed class PirateBubbleOverlayInstance(GameObject rootObject, List<Image> parts)
        {
            public GameObject RootObject { get; } = rootObject;
            public List<Image> Parts { get; } = parts;
        }
    }
}
