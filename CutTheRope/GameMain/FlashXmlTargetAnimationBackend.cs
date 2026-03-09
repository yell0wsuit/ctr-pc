using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class FlashXmlTargetAnimationBackend : ITargetAnimationBackend, ITimelineDelegate
    {
        private const float BaseTargetScale = 1.73f;
        private readonly List<Image> parts = [];
        private readonly FlashXmlAnimationDefinition _definition;
        private readonly FlashXmlIdleCadenceClock _idleCadenceClock = new();
        private ITimelineDelegate _externalTimelineDelegate;
        private int _activeTimelineId = -1;
        private Timeline _driverTimeline;
        private int _driverTimelineId = -1;
        private float _driverTimelineDurationSeconds;
        private float _driverTimelinePlaybackRate = 1f;

        public FlashXmlTargetAnimationBackend(string xmlPath = null)
        {
            string resolvedXmlPath = string.IsNullOrWhiteSpace(xmlPath)
                ? ContentPaths.GetAnimationXmlAbsolutePath("om_nom_original.xml")
                : xmlPath;

            _definition = FlashXmlImporter.ParseFile(resolvedXmlPath);

            TargetObject = GameObject.GameObject_createWithResIDQuad(Resources.Img.CharAnimationsSmooth, 0);
            TargetObject.color = RGBAColor.transparentRGBA;
            TargetObject.passColorToChilds = false;
            TargetObject.scaleX = BaseTargetScale;
            TargetObject.scaleY = BaseTargetScale;

            BuildParts(_definition);

            // Use the Flash stage center as the anchor point. All skins share the
            // same stage dimensions (550×400), so this keeps every skin at the same
            // position without per-skin centroid calculation.
            const float classicBodyScreenOffsetX = -6f;
            const float classicBodyScreenOffsetY = -6f;
            TargetObject.useCustomAnchor = true;
            TargetObject.customAnchorX = -classicBodyScreenOffsetX / BaseTargetScale;
            TargetObject.customAnchorY = -classicBodyScreenOffsetY / BaseTargetScale;
            TargetObject.width = (int)MathF.Round(_definition.StageWidth);
            TargetObject.height = (int)MathF.Round(_definition.StageHeight);
        }

        public GameObject TargetObject { get; }

        public float GetTargetBaseScaleX()
        {
            return BaseTargetScale;
        }

        public float GetTargetBaseScaleY()
        {
            return BaseTargetScale;
        }

        public void Initialize(ITimelineDelegate timelineDelegate)
        {
            _externalTimelineDelegate = timelineDelegate;
            Play(TargetAnimationState.IdleLoop);
        }

        public void Play(TargetAnimationState state)
        {
            if (!TryMapState(state, out int timelineId))
            {
                return;
            }

            PlayTimelineById(timelineId);
        }

        public void PlayRandomIdleVariant(Func<int, int, int> rng)
        {
            int timelineId = rng(0, 2) switch
            {
                0 => IdleVariationOneTimeline,
                1 => IdleVariationTwoTimeline,
                _ => IdleVariationThreeTimeline
            };

            PlayTimelineById(timelineId);
        }

        private void PlayTimelineById(int timelineId)
        {
            _activeTimelineId = timelineId;
            for (int i = 0; i < parts.Count; i++)
            {
                Timeline timeline = parts[i].GetTimeline(timelineId);
                if (timeline != null)
                {
                    parts[i].visible = true;
                    parts[i].PlayTimeline(timelineId);
                }
                else
                {
                    // Stop all timelines before playing the
                    // new one. Parts without the requested timeline are stopped and hidden so they
                    // don't keep ticking invisibly with stale pose from a previous one-shot.
                    if (parts[i].GetCurrentTimeline() != null)
                    {
                        parts[i].StopCurrentTimeline();
                    }

                    parts[i].visible = false;
                }
            }

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
            return _definition.RootTimelines.TryGetValue(SleepingTimeline, out float duration)
                ? duration
                : 0f;
        }

        public void ResetBlink()
        {
        }

        public void TriggerBlink()
        {
        }

        public void UpdateSleepOverlays(float delta)
        {
        }

        public void SyncSleepOverlayPosition(float x, float y)
        {
        }

        public void SetSleepOverlayVisible(bool visible)
        {
        }

        public void DrawSleepOverlays()
        {
        }

        public bool HandlesOwnSleepPulse => true;

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (_driverTimelineId != IdleLoopTimeline
                || _driverTimeline == null
                || !ReferenceEquals(t, _driverTimeline)
                || _externalTimelineDelegate == null)
            {
                return;
            }

            int syntheticTicks = _idleCadenceClock.Advance(t.time, _driverTimelineDurationSeconds, _driverTimelinePlaybackRate);
            for (int tick = 0; tick < syntheticTicks; tick++)
            {
                _externalTimelineDelegate.TimelinereachedKeyFramewithIndex(t, k, 1);
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

            if (FlashXmlTargetTimelineRules.TryGetFollowupTimeline(finishedTimelineId, out int followupTimelineId)
                && FindFirstPartWithTimeline(followupTimelineId) != null)
            {
                PlayTimelineById(followupTimelineId);
                return;
            }

            if (_activeTimelineId != IdleLoopTimeline)
            {
                PlayTimelineById(IdleLoopTimeline);
            }
        }

        private void BuildParts(FlashXmlAnimationDefinition definition)
        {
            // First pass: create all parts so cross-part action targets can be resolved.
#pragma warning disable IDE0028
            Dictionary<string, Image> partsByName = new(definition.Parts.Count);
#pragma warning restore IDE0028
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition partDefinition = definition.Parts[i];

                FlashXmlImage part = FlashXmlImage.CreateWithResID(partDefinition.TextureResourceName);
                part.PlaybackRate = 0.7f;
                part.anchor = 9;
                part.parentAnchor = 9;
                part.visible = ShouldStartVisible(partDefinition);
                part.useCustomAnchor = true;
                part.customAnchorX = partDefinition.AnchorX;
                part.customAnchorY = partDefinition.AnchorY;
                part.rotationCenterX = partDefinition.RotationCenterX;
                part.rotationCenterY = partDefinition.RotationCenterY;
                part.SetDrawQuad(partDefinition.QuadToDraw);

                _ = TargetObject.AddChild(part);
                parts.Add(part);

                if (!string.IsNullOrEmpty(partDefinition.Name))
                {
                    partsByName[partDefinition.Name] = part;
                }
            }

            // Second pass: build timelines now that all parts exist for cross-part linking.
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                BuildTimelines((FlashXmlImage)parts[i], definition.Parts[i], partsByName);
            }
        }

        private static bool ShouldStartVisible(FlashXmlPartDefinition partDefinition)
        {
            return partDefinition.Timelines.ContainsKey(IdleLoopTimeline);
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
            _idleCadenceClock.Reset();
            Image delegateDriver = FindBestDriverPartWithTimeline(timelineId);
            Timeline timeline = delegateDriver?.GetTimeline(timelineId);
            if (timeline == null)
            {
                return;
            }

            if (timelineId == IdleLoopTimeline)
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
                _driverTimelineDurationSeconds = GetTimelineDurationSeconds(delegateDriver, timelineId);
                _driverTimelinePlaybackRate = delegateDriver is FlashXmlImage flashXmlImage
                    ? flashXmlImage.PlaybackRate
                    : 1f;
                return;
            }

            if (FlashXmlTargetTimelineRules.ShouldBindFollowupDelegate(timelineId))
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

        private static void BuildTimelines(FlashXmlImage part, FlashXmlPartDefinition partDefinition, Dictionary<string, Image> partsByName)
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

                if (timelineId is IdleLoopTimeline or SleepingTimeline)
                {
                    timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                }

                part.AddTimelinewithID(timeline, timelineId);
            }
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

        private static bool TryMapState(TargetAnimationState state, out int timelineId)
        {
            timelineId = state switch
            {
                TargetAnimationState.IdleLoop => IdleLoopTimeline,
                TargetAnimationState.IdleVariationOne => IdleVariationOneTimeline,
                TargetAnimationState.IdleVariationTwo => IdleVariationTwoTimeline,
                TargetAnimationState.Excited => ExcitedTimeline,
                TargetAnimationState.MouthOpening => MouthOpeningTimeline,
                TargetAnimationState.MouthClosing => MouthClosingTimeline,
                TargetAnimationState.Chewing => ChewingTimeline,
                TargetAnimationState.Sad => SadTimeline,
                TargetAnimationState.Sleeping => SleepingTimeline,
                TargetAnimationState.Greeting => GreetingTimeline,
                _ => -1
            };

            return timelineId >= 0;
        }

        private const int IdleLoopTimeline = FlashXmlTargetTimelineRules.IdleLoopTimeline;
        private const int IdleVariationOneTimeline = FlashXmlTargetTimelineRules.IdleVariationOneTimeline;
        private const int IdleVariationTwoTimeline = FlashXmlTargetTimelineRules.IdleVariationTwoTimeline;
        private const int IdleVariationThreeTimeline = FlashXmlTargetTimelineRules.IdleVariationThreeTimeline;
        private const int ExcitedTimeline = FlashXmlTargetTimelineRules.ExcitedTimeline;
        private const int MouthOpeningTimeline = FlashXmlTargetTimelineRules.MouthOpeningTimeline;
        private const int MouthClosingTimeline = FlashXmlTargetTimelineRules.MouthClosingTimeline;
        private const int SadTimeline = FlashXmlTargetTimelineRules.SadTimeline;
        private const int ChewingTimeline = FlashXmlTargetTimelineRules.ChewingTimeline;
        private const int SleepingTimeline = FlashXmlTargetTimelineRules.SleepingTimeline;
        private const int GreetingTimeline = FlashXmlTargetTimelineRules.GreetingTimeline;

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
    }
}
