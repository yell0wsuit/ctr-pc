using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Target animation backend that builds Om Nom animation timelines from Flash XML exports.
    /// </summary>
    internal sealed class FlashXmlTargetAnimationBackend : ITargetAnimationBackend, ITimelineDelegate
    {
        /// <summary>Base scale applied to Flash XML target roots to match the classic target size.</summary>
        private const float BaseTargetScale = 1.73f;

        /// <summary>Duration of one exported Flash frame in seconds.</summary>
        private const float FlashXmlFrameDurationSeconds = 1f / 30f;

        /// <summary>Timeline ID used by the sleep overlay export.</summary>
        private const int SleepOverlayTimeline = 0;

        /// <summary>Playback rate used by timelines marked as slow in the skin definition.</summary>
        private const float IosSlowPlaybackRate = 0.6f;

        /// <summary>Skin ID that enables the pirate bubble overlay.</summary>
        private const string PirateSkinName = "OM_NOM_PIRATE";

        /// <summary>Initial delay before spawning pirate bubble overlays.</summary>
        private const float PirateBubbleSpawnDelaySeconds = 0.932203f;

        /// <summary>Image parts that make up the target animation.</summary>
        private readonly List<Image> parts = [];

        /// <summary>Image parts that make up the sleep overlay animation.</summary>
        private readonly List<Image> _sleepOverlayParts = [];

        /// <summary>Active pirate bubble overlay instances.</summary>
        private readonly List<PirateBubbleOverlayInstance> _activeBubbleOverlays = [];

        /// <summary>Parsed Flash XML animation definition for the selected skin.</summary>
        private readonly FlashXmlAnimationDefinition _definition;

        /// <summary>Parsed Flash XML definition for pirate bubble overlays, or <see langword="null"/> for other skins.</summary>
        private readonly FlashXmlAnimationDefinition _bubbleOverlayDefinition;

        /// <summary>Clock that converts looping idle timeline progress into synthetic cadence callbacks.</summary>
        private readonly FlashXmlIdleCadenceClock _idleCadenceClock = new();

        /// <summary>Root object used for the sleep overlay animation.</summary>
        private readonly FlashXmlStageRoot _sleepOverlayObject;

        /// <summary>External delegate that receives target timeline callbacks.</summary>
        private ITimelineDelegate _externalTimelineDelegate;

        /// <summary>Currently active target timeline ID, or -1 when no timeline is active.</summary>
        private int _activeTimelineId = -1;

        /// <summary>Timeline used as the active driver for callbacks.</summary>
        private Timeline _driverTimeline;

        /// <summary>Timeline ID for the active driver timeline, or -1 when no driver is bound.</summary>
        private int _driverTimelineId = -1;

        /// <summary>Duration of the active driver timeline in seconds.</summary>
        private float _driverTimelineDurationSeconds;

        /// <summary>Playback-rate multiplier for the active driver timeline.</summary>
        private float _driverTimelinePlaybackRate = 1f;

        /// <summary>Whether the active driver timeline came from the root definition.</summary>
        private bool _driverTimelineUsesRootDefinition;

        /// <summary>X position used when spawning additional overlay roots.</summary>
        private float _additionalOverlayX;

        /// <summary>Y position used when spawning additional overlay roots.</summary>
        private float _additionalOverlayY;

        /// <summary>Remaining delay before the next pirate bubble overlay spawn, or -1 when disabled.</summary>
        private float _pendingPirateBubbleDelaySeconds = -1f;

        /// <summary>Whether idle-to-sleep transition playback is blocked until a wake animation has played.</summary>
        private bool _skipIdleToSleepTransitionUntilWake = true;

        /// <summary>
        /// Initializes a new Flash XML target animation backend for a skin.
        /// </summary>
        /// <param name="skinDefinition">Skin definition that provides animation paths and timeline IDs.</param>
        public FlashXmlTargetAnimationBackend(OmNomSkinDefinition skinDefinition)
        {
            SkinDefinition = skinDefinition;

            _definition = FlashXmlImporter.ParseFile(skinDefinition.AnimationXmlPath);

            TargetObject = CreateStageRoot(_definition);
            int idleLoopId = SkinDefinition.GetTimelineId(TargetAnimationState.IdleLoop);
            int sleepingId = SkinDefinition.GetTimelineId(TargetAnimationState.Sleeping);
            BuildParts(_definition, TargetObject, parts, idleLoopId, sleepingId);
            BuildRootTimelines(_definition, TargetObject, idleLoopId, sleepingId);

            FlashXmlAnimationDefinition sleepOverlayDefinition = FlashXmlImporter.ParseFile(
                ContentPaths.GetAnimationXmlAbsolutePath("fx_sleep.xml"));
            _sleepOverlayObject = CreateStageRoot(sleepOverlayDefinition);
            _sleepOverlayObject.visible = false;
            BuildParts(sleepOverlayDefinition, _sleepOverlayObject, _sleepOverlayParts, SleepOverlayTimeline, -1);
            BuildRootTimelines(sleepOverlayDefinition, _sleepOverlayObject, SleepOverlayTimeline, -1);

            _bubbleOverlayDefinition = string.Equals(SkinDefinition.Id, PirateSkinName, StringComparison.Ordinal)
                ? FlashXmlImporter.ParseFile(
                    ContentPaths.GetAnimationXmlAbsolutePath("fx_bubbles.xml"))
                : null;
        }

        /// <summary>Root object that owns the target animation parts and timelines.</summary>
        public GameObject TargetObject { get; }

        /// <summary>
        /// Creates and configures a Flash XML stage root.
        /// </summary>
        /// <param name="definition">Animation definition that provides stage dimensions.</param>
        /// <returns>The configured stage root.</returns>
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

        /// <summary>
        /// Gets the base X scale applied to the target root.
        /// </summary>
        /// <returns>The target root's base X scale.</returns>
        public float GetTargetBaseScaleX()
        {
            return BaseTargetScale;
        }

        /// <summary>
        /// Gets the base Y scale applied to the target root.
        /// </summary>
        /// <returns>The target root's base Y scale.</returns>
        public float GetTargetBaseScaleY()
        {
            return BaseTargetScale;
        }

        /// <summary>Whether this skin should start by playing its greeting animation.</summary>
        public bool StartsWithGreeting => SkinDefinition.StartWithGreeting;

        /// <summary>Skin definition that backs this target's animations.</summary>
        public OmNomSkinDefinition SkinDefinition { get; }

        /// <summary>Whether this backend is driven by Flash XML animation exports.</summary>
        public bool UsesFlashXmlAnimations => true;

        /// <summary>
        /// Initializes playback and binds the external timeline delegate.
        /// </summary>
        /// <param name="timelineDelegate">Delegate that receives timeline callbacks from the target animation.</param>
        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            _externalTimelineDelegate = timelineDelegate;

            if (SkinDefinition.StartWithGreeting && TryMapState(TargetAnimationState.Greeting, out _))
            {
                Play(TargetAnimationState.Greeting);
            }
            else
            {
                Play(TargetAnimationState.IdleLoop);
            }
        }

        /// <summary>
        /// Plays the timeline mapped to a target animation state.
        /// </summary>
        /// <param name="state">Target animation state to play.</param>
        public void Play(TargetAnimationState state)
        {
            Play(state, trimIdleToSleepTransition: true);
        }

        /// <inheritdoc />
        public void PlaySleeping(bool trimIdleToSleepTransition)
        {
            Play(TargetAnimationState.Sleeping, trimIdleToSleepTransition);
        }

        /// <summary>
        /// Plays the timeline mapped to a target animation state.
        /// </summary>
        /// <param name="state">Target animation state to play.</param>
        /// <param name="trimIdleToSleepTransition">Whether to apply configured idle-to-sleep trim when entering sleep.</param>
        private void Play(TargetAnimationState state, bool trimIdleToSleepTransition)
        {
            if (state == TargetAnimationState.Sleeping && TryPlayIdleToSleepTransition(trimIdleToSleepTransition))
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

        /// <summary>
        /// Plays one of the configured idle variant timelines.
        /// </summary>
        /// <param name="rng">Random integer provider called with inclusive minimum and maximum indexes.</param>
        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            if (SkinDefinition.IdleVariants.Length == 0)
            {
                return;
            }

            int index = rng(0, SkinDefinition.IdleVariants.Length - 1);
            PlayTimelineById(SkinDefinition.IdleVariants[index]);
        }

        /// <summary>
        /// Skips forward in the active timeline by a number of Flash frames.
        /// </summary>
        /// <param name="frameCount">Number of 30 FPS Flash frames to skip.</param>
        internal void SkipCurrentTimelineFrames(int frameCount)
        {
            if (_activeTimelineId < 0 || frameCount <= 0)
            {
                return;
            }

            float skipSeconds = frameCount * FlashXmlFrameDurationSeconds;
            SeekCurrentTimeline(skipSeconds);
        }

        /// <summary>
        /// Plays a Flash XML timeline by ID on the target root and all parts.
        /// </summary>
        /// <param name="timelineId">Flash XML timeline ID to play.</param>
        private void PlayTimelineById(int timelineId)
        {
            _activeTimelineId = timelineId;
            if (TargetObject is FlashXmlStageRoot stageRoot)
            {
                stageRoot.PlaybackRate = GetTimelinePlaybackRate(SkinDefinition, timelineId);
            }

            PlayTimeline(parts, timelineId);
            PlayRootTimeline(TargetObject, timelineId);
            BindDriverDelegateForTimeline(timelineId);
        }

        /// <summary>
        /// Determines whether the timeline for a target animation state is currently playing.
        /// </summary>
        /// <param name="state">Target animation state to inspect.</param>
        /// <returns><see langword="true"/> if the mapped timeline is active and playing; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Gets the delay before the sleep pulse overlay should be triggered.
        /// </summary>
        /// <returns>Delay in seconds before the sleep pulse overlay should play.</returns>
        public float GetSleepPulseDelaySeconds()
        {
            int sleepingTimelineId = SkinDefinition.GetTimelineId(TargetAnimationState.Sleeping);
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

        /// <summary>
        /// Resets blink state for backends that synthesize blinks.
        /// </summary>
        public void ResetBlink()
        {
        }

        /// <summary>
        /// Triggers a blink for backends that synthesize blinks.
        /// </summary>
        public void TriggerBlink()
        {
        }

        /// <summary>
        /// Updates sleep overlay animations.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        public void UpdateSleepOverlays(float delta)
        {
            if (_sleepOverlayObject.visible)
            {
                _sleepOverlayObject.Update(delta);
            }
        }

        /// <summary>
        /// Synchronizes the sleep overlay root position to the target.
        /// </summary>
        /// <param name="x">Screen-space X position for the overlay root.</param>
        /// <param name="y">Screen-space Y position for the overlay root.</param>
        public void SyncSleepOverlayPosition(float x, float y)
        {
            _sleepOverlayObject.x = x;
            _sleepOverlayObject.y = y;
        }

        /// <summary>
        /// Updates non-sleep overlay animations owned by the backend.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        public void UpdateAdditionalOverlays(float delta)
        {
            UpdatePirateBubbleOverlays(delta);
        }

        /// <summary>
        /// Synchronizes non-sleep overlay roots to the target.
        /// </summary>
        /// <param name="x">Screen-space X position for additional overlay roots.</param>
        /// <param name="y">Screen-space Y position for additional overlay roots.</param>
        public void SyncAdditionalOverlayPosition(float x, float y)
        {
            _additionalOverlayX = x;
            _additionalOverlayY = y;
        }

        /// <summary>
        /// Sets whether the sleep overlay is visible and playing.
        /// </summary>
        /// <param name="visible"><see langword="true"/> to show and restart the sleep overlay; otherwise, <see langword="false"/>.</param>
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

        /// <summary>
        /// Draws sleep overlays when they are visible.
        /// </summary>
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

        /// <summary>Whether this backend owns sleep pulse overlay playback.</summary>
        public bool HandlesOwnSleepPulse => true;

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (_driverTimeline == null || !ReferenceEquals(t, _driverTimeline))
            {
                return;
            }

            // Only the idle loop drives the external blink/idle-variant cadence. Other
            // root-driven timelines (e.g. chewing, ID 7) also emit a keyframe at index 1,
            // which would otherwise be misread as an idle tick and interrupt playback with
            // a random idle variant.
            int idleLoopTimelineId = SkinDefinition.GetTimelineId(TargetAnimationState.IdleLoop);

            if (_driverTimelineUsesRootDefinition)
            {
                if (TargetIdleCadence.DrivesIdleCadence(_driverTimelineId, idleLoopTimelineId))
                {
                    _externalTimelineDelegate?.TimelinereachedKeyFramewithIndex(t, k, i);
                }

                return;
            }

            if (TargetIdleCadence.DrivesIdleCadence(_driverTimelineId, idleLoopTimelineId)
                && _externalTimelineDelegate != null)
            {
                int syntheticTicks = _idleCadenceClock.Advance(t.time, _driverTimelineDurationSeconds, _driverTimelinePlaybackRate);
                for (int tick = 0; tick < syntheticTicks; tick++)
                {
                    _externalTimelineDelegate.TimelinereachedKeyFramewithIndex(t, k, 1);
                }
            }

        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            if (_driverTimeline == null || !ReferenceEquals(t, _driverTimeline))
            {
                return;
            }

            int finishedTimelineId = _driverTimelineId;

            _driverTimeline = null;
            _driverTimelineId = -1;

            if (TryGetCompletionTargetTimelineId(SkinDefinition, finishedTimelineId, out int followupTimelineId)
                && FindFirstPartWithTimeline(followupTimelineId) != null)
            {
                PlayTimelineById(followupTimelineId);
            }
        }

        /// <summary>
        /// Gets the follow-up timeline that should play after a timeline completes.
        /// </summary>
        /// <param name="skinDefinition">Skin definition containing follow-up timeline mappings.</param>
        /// <param name="finishedTimelineId">Timeline ID that just completed.</param>
        /// <param name="followupTimelineId">Follow-up timeline ID, when one is configured.</param>
        /// <returns><see langword="true"/> when a follow-up timeline is configured; otherwise, <see langword="false"/>.</returns>
        private static bool TryGetCompletionTargetTimelineId(OmNomSkinDefinition skinDefinition,
            int finishedTimelineId, out int followupTimelineId)
        {
            return skinDefinition.TryGetFollowupTimeline(finishedTimelineId, out followupTimelineId);
        }

        /// <summary>
        /// Gets the playback-rate multiplier for a timeline.
        /// </summary>
        /// <param name="skinDefinition">Skin definition containing slow timeline IDs.</param>
        /// <param name="activeTimelineId">Timeline ID to inspect.</param>
        /// <returns>The playback-rate multiplier for the timeline.</returns>
        private static float GetTimelinePlaybackRate(OmNomSkinDefinition skinDefinition, int activeTimelineId)
        {
            return activeTimelineId < 0
                ? 1f
                : Array.IndexOf(skinDefinition.SlowTimelineIds, activeTimelineId) >= 0
                ? IosSlowPlaybackRate
                : 1f;
        }

        /// <summary>
        /// Creates Flash XML image parts and attaches their timelines to a root object.
        /// </summary>
        /// <param name="definition">Parsed Flash XML animation definition.</param>
        /// <param name="rootObject">Root object that will own the created parts.</param>
        /// <param name="targetParts">Collection that receives the created image parts.</param>
        /// <param name="idleLoopTimelineId">Timeline ID that should loop as the idle animation.</param>
        /// <param name="sleepingTimelineId">Timeline ID that should loop as the sleeping animation.</param>
        internal static void BuildParts(FlashXmlAnimationDefinition definition, GameObject rootObject,
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

        /// <summary>
        /// Creates root driver timelines and attaches them to a root object.
        /// </summary>
        /// <param name="definition">Parsed Flash XML animation definition.</param>
        /// <param name="rootObject">Root object that receives the root timelines.</param>
        /// <param name="idleLoopTimelineId">Timeline ID that should loop as the idle animation.</param>
        /// <param name="sleepingTimelineId">Timeline ID that should loop as the sleeping animation.</param>
        internal static void BuildRootTimelines(FlashXmlAnimationDefinition definition, GameObject rootObject,
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

        /// <summary>
        /// Plays a timeline across all parts, hiding parts that do not contain the requested timeline.
        /// </summary>
        /// <param name="targetParts">Parts that should play or stop the timeline.</param>
        /// <param name="timelineId">Flash timeline ID to play.</param>
        internal static void PlayTimeline(List<Image> targetParts, int timelineId)
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

        /// <summary>
        /// Plays a root timeline if present, or stops the current root timeline otherwise.
        /// </summary>
        /// <param name="rootObject">Root object that owns the timeline.</param>
        /// <param name="timelineId">Flash timeline ID to play.</param>
        internal static void PlayRootTimeline(GameObject rootObject, int timelineId)
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

        /// <summary>
        /// Determines whether a timeline is currently playing on any target part.
        /// </summary>
        /// <param name="targetParts">Parts to inspect.</param>
        /// <param name="timelineId">Timeline ID to inspect.</param>
        /// <returns><see langword="true"/> when the timeline is active and playing on a part; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to play the idle-to-sleep transition timeline.
        /// </summary>
        /// <returns><see langword="true"/> when the transition started; otherwise, <see langword="false"/>.</returns>
        private bool TryPlayIdleToSleepTransition(bool trimIdleToSleepTransition)
        {
            if (!ShouldUseIdleToSleepTransition()
                || _activeTimelineId == SkinDefinition.GetTimelineId(TargetAnimationState.Sleeping)
                || !TryMapState(TargetAnimationState.IdleToSleep, out int idleToSleepTimelineId))
            {
                return false;
            }

            PlayTimelineById(idleToSleepTimelineId);

            if (trimIdleToSleepTransition)
            {
                float trimSeconds = GetIdleToSleepSkipSeconds(GetTimelineDurationSeconds(idleToSleepTimelineId));
                if (trimSeconds > 0f)
                {
                    SeekTimeline(parts, idleToSleepTimelineId, trimSeconds);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets whether the idle-to-sleep transition is currently allowed.
        /// </summary>
        /// <returns><see langword="true"/> when idle-to-sleep transition playback is allowed; otherwise, <see langword="false"/>.</returns>
        private bool ShouldUseIdleToSleepTransition()
        {
            return !_skipIdleToSleepTransitionUntilWake;
        }

        /// <summary>
        /// Gets the number of seconds to skip from the start of the idle-to-sleep transition.
        /// </summary>
        /// <param name="idleToSleepDurationSeconds">Total idle-to-sleep transition duration in seconds.</param>
        /// <returns>The clamped skip duration in seconds.</returns>
        private float GetIdleToSleepSkipSeconds(float idleToSleepDurationSeconds)
        {
            float skippedDurationSeconds = SkinDefinition.IdleToSleepTrimFrames * FlashXmlFrameDurationSeconds;
            return MathF.Min(skippedDurationSeconds, idleToSleepDurationSeconds);
        }

        /// <summary>
        /// Updates whether idle-to-sleep transition playback is allowed after a state change.
        /// </summary>
        /// <param name="state">State that was just requested.</param>
        private void UpdateIdleToSleepTransitionAvailability(TargetAnimationState state)
        {
            if (state is not TargetAnimationState.Sleeping and not TargetAnimationState.IdleLoop)
            {
                _skipIdleToSleepTransitionUntilWake = false;
            }
        }

        /// <summary>
        /// Seeks a playing timeline on all target parts.
        /// </summary>
        /// <param name="targetParts">Parts whose timeline should be seeked.</param>
        /// <param name="timelineId">Timeline ID to seek.</param>
        /// <param name="timeSeconds">Target timeline time in seconds.</param>
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

        /// <summary>
        /// Seeks the currently active timeline on the root object and parts.
        /// </summary>
        /// <param name="timeSeconds">Target timeline time in seconds.</param>
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

        /// <summary>
        /// Updates pirate bubble overlay scheduling and active overlay animations.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
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

        /// <summary>
        /// Creates and starts one pirate bubble overlay animation.
        /// </summary>
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

        /// <summary>
        /// Gets the randomized interval before the next pirate bubble overlay.
        /// </summary>
        /// <returns>Delay in seconds before the next pirate bubble overlay.</returns>
        private static float GetPirateBubbleLoopIntervalSeconds()
        {
            return CTRMathHelper.RND_RANGE(1, 4);
        }

        /// <summary>
        /// Updates pirate bubble overlay scheduling for a target animation state.
        /// </summary>
        /// <param name="state">Target animation state that was just requested.</param>
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

        /// <summary>
        /// Determines whether a part should be visible before playback starts.
        /// </summary>
        /// <param name="partDefinition">Part definition to inspect.</param>
        /// <param name="idleLoopTimelineId">Idle loop timeline ID.</param>
        /// <returns><see langword="true"/> when the part participates in the idle loop; otherwise, <see langword="false"/>.</returns>
        private static bool ShouldStartVisible(FlashXmlPartDefinition partDefinition, int idleLoopTimelineId)
        {
            return idleLoopTimelineId >= 0 && partDefinition.Timelines.ContainsKey(idleLoopTimelineId);
        }

        /// <summary>
        /// Finds the first target part that contains a timeline.
        /// </summary>
        /// <param name="timelineId">Timeline ID to find.</param>
        /// <returns>The first matching part, or <see langword="null"/> if none exists.</returns>
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

        /// <summary>
        /// Binds this backend as the timeline delegate for the best callback driver timeline.
        /// </summary>
        /// <param name="timelineId">Timeline ID that needs callback driving.</param>
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

            if (timelineId == SkinDefinition.GetTimelineId(TargetAnimationState.IdleLoop))
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
                _driverTimelineDurationSeconds = GetTimelineDurationSeconds(delegateDriver, timelineId);
                _driverTimelinePlaybackRate = GetTimelinePlaybackRate(SkinDefinition, timelineId);
                return;
            }

            if (SkinDefinition.ShouldBindFollowupDelegate(timelineId))
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
            }
        }

        /// <summary>
        /// Gets a part timeline duration from the parsed Flash XML definition.
        /// </summary>
        /// <param name="part">Part whose timeline duration should be read.</param>
        /// <param name="timelineId">Timeline ID to inspect.</param>
        /// <returns>The timeline duration in seconds, or 0 when the part has no matching definition.</returns>
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

        /// <summary>
        /// Gets the best known duration for a timeline.
        /// </summary>
        /// <param name="timelineId">Timeline ID to inspect.</param>
        /// <returns>The timeline duration in seconds, or 0 when the timeline is unknown.</returns>
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

        /// <summary>
        /// Finds the best part timeline to use as the callback driver for a timeline ID.
        /// </summary>
        /// <param name="timelineId">Timeline ID to inspect.</param>
        /// <returns>The best driver part, or <see langword="null"/> if no part contains the timeline.</returns>
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

        /// <summary>
        /// Computes the duration of a Flash XML timeline from its key-frame offsets.
        /// </summary>
        /// <param name="timelineDefinition">Timeline definition to inspect.</param>
        /// <returns>The computed duration in seconds.</returns>
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

        /// <summary>
        /// Sums the time offsets in a sequence of two-value key frames.
        /// </summary>
        /// <param name="frames">Frames whose time offsets should be summed.</param>
        /// <returns>The total duration represented by the frames.</returns>
        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat2KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        /// <summary>
        /// Sums the time offsets in a sequence of four-value key frames.
        /// </summary>
        /// <param name="frames">Frames whose time offsets should be summed.</param>
        /// <returns>The total duration represented by the frames.</returns>
        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat4KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        /// <summary>
        /// Sums the time offsets in a sequence of one-value key frames.
        /// </summary>
        /// <param name="frames">Frames whose time offsets should be summed.</param>
        /// <returns>The total duration represented by the frames.</returns>
        private static float SumTimeOffsets(IReadOnlyList<FlashXmlFloat1KeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        /// <summary>
        /// Sums the time offsets in a sequence of action key frames.
        /// </summary>
        /// <param name="frames">Frames whose time offsets should be summed.</param>
        /// <returns>The total duration represented by the frames.</returns>
        private static float SumTimeOffsets(IReadOnlyList<FlashXmlActionGroupKeyFrame> frames)
        {
            float total = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].TimeOffset;
            }

            return total;
        }

        /// <summary>
        /// Builds and attaches timelines for one Flash XML image part.
        /// </summary>
        /// <param name="part">Part that receives the generated timelines.</param>
        /// <param name="partDefinition">Parsed definition for the part.</param>
        /// <param name="partsByName">Lookup table for action targets by exported part name.</param>
        /// <param name="idleLoopTimelineId">Timeline ID that should loop as the idle animation.</param>
        /// <param name="sleepingTimelineId">Timeline ID that should loop as the sleeping animation.</param>
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

        /// <summary>
        /// Creates a timeline that hides a part for timelines missing from the Flash XML export.
        /// </summary>
        /// <returns>The hidden placeholder timeline.</returns>
        private static Timeline CreateHiddenTimeline()
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeSingleAction(new BaseElement(), BaseElement.ACTION_SET_VISIBLE, 0, 0, 0f));
            return timeline;
        }

        /// <summary>
        /// Creates a root driver timeline from root action key frames.
        /// </summary>
        /// <param name="timelineDefinition">Root timeline definition to convert.</param>
        /// <returns>The generated root driver timeline.</returns>
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

        /// <summary>
        /// Builds a runtime action from one exported Flash XML action command.
        /// </summary>
        /// <param name="part">Part that owns the action when the exported target is self.</param>
        /// <param name="action">Action command to convert.</param>
        /// <param name="partsByName">Lookup table for named action targets.</param>
        /// <returns>The generated action, or <see langword="null"/> when the action cannot be resolved.</returns>
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

        /// <summary>
        /// Parses a Flash XML action parameter as an integer.
        /// </summary>
        /// <param name="raw">Raw action parameter text.</param>
        /// <returns>The parsed integer value, or 0 when parsing fails.</returns>
        private static int ParseActionInt(string raw)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int integerValue)
                ? integerValue
                : float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue) ? (int)MathF.Round(floatValue) : 0;
        }

        /// <summary>
        /// Parses a Flash XML action parameter as a floating-point value.
        /// </summary>
        /// <param name="raw">Raw action parameter text.</param>
        /// <returns>The parsed floating-point value, or 0 when parsing fails.</returns>
        private static float ParseActionFloat(string raw)
        {
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue)
                ? floatValue
                : 0f;
        }

        /// <summary>
        /// Maps an exported Flash interpolation code to a runtime transition type.
        /// </summary>
        /// <param name="interpolation">Flash interpolation code.</param>
        /// <returns>The mapped runtime transition type.</returns>
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

        /// <summary>
        /// Maps a target animation state to a configured Flash XML timeline ID.
        /// </summary>
        /// <param name="state">Target animation state to map.</param>
        /// <param name="timelineId">Mapped timeline ID, when one exists.</param>
        /// <returns><see langword="true"/> when the state maps to a configured timeline; otherwise, <see langword="false"/>.</returns>
        private bool TryMapState(TargetAnimationState state, out int timelineId)
        {
            timelineId = SkinDefinition.GetTimelineId(state);
            return timelineId >= 0;
        }

        /// <summary>
        /// Converts looping idle timeline progress into wall-clock cadence ticks.
        /// </summary>
        private sealed class FlashXmlIdleCadenceClock
        {
            /// <summary>Wall-clock interval between synthetic idle cadence ticks.</summary>
            private const float IdleTickSeconds = 1f;

            /// <summary>Floating-point tolerance used for timeline wraparound checks.</summary>
            private const float Epsilon = 0.0001f;

            /// <summary>Accumulated idle wall-clock time not yet emitted as whole ticks.</summary>
            private float _accumulatedWallSeconds;

            /// <summary>Timeline time observed during the previous cadence advance.</summary>
            private float _lastTimelineTime;

            /// <summary>Whether the cadence clock has received its first timeline sample.</summary>
            private bool _initialized;

            /// <summary>
            /// Advances the cadence clock from the current timeline time.
            /// </summary>
            /// <param name="currentTimelineTime">Current time within the timeline, in seconds.</param>
            /// <param name="loopDurationSeconds">Loop duration used to account for wraparound, in seconds.</param>
            /// <param name="playbackRate">Timeline playback-rate multiplier.</param>
            /// <returns>The number of whole idle cadence ticks elapsed since the previous advance.</returns>
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

            /// <summary>
            /// Resets accumulated cadence state.
            /// </summary>
            public void Reset()
            {
                _accumulatedWallSeconds = 0f;
                _lastTimelineTime = 0f;
                _initialized = false;
            }
        }

        /// <summary>
        /// Active pirate bubble overlay root and its image parts.
        /// </summary>
        /// <param name="rootObject">Root object for the overlay animation.</param>
        /// <param name="parts">Image parts that make up the overlay animation.</param>
        private sealed class PirateBubbleOverlayInstance(GameObject rootObject, List<Image> parts)
        {
            /// <summary>Root object for the overlay animation.</summary>
            public GameObject RootObject { get; } = rootObject;

            /// <summary>Image parts that make up the overlay animation.</summary>
            public List<Image> Parts { get; } = parts;
        }
    }
}
