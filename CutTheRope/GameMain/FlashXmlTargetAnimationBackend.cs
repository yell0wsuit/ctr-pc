using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class FlashXmlTargetAnimationBackend : ITargetAnimationBackend, ITimelineDelegate
    {
        private const float PartDimensionScale = 0.65f;
        private const float WholeObjectScale = 1.75f;
        private readonly List<Image> parts = [];
        private readonly FlashXmlAnimationDefinition _definition;
        private ITimelineDelegate _externalTimelineDelegate;
        private int _activeTimelineId = -1;
        private Timeline _driverTimeline;
        private int _driverTimelineId = -1;

        public FlashXmlTargetAnimationBackend(string xmlPath = null)
        {
            string resolvedXmlPath = string.IsNullOrWhiteSpace(xmlPath)
                ? ContentPaths.GetContentPath(Path.Combine(ContentPaths.AnimationsDirectory, "om_nom_original.xml"))
                : xmlPath;

            _definition = FlashXmlImporter.ParseFile(resolvedXmlPath);

            TargetObject = GameObject.GameObject_createWithResIDQuad(Resources.Img.CharAnimationsSmooth, 0);
            TargetObject.color = RGBAColor.transparentRGBA;
            TargetObject.passColorToChilds = false;
            TargetObject.scaleX = WholeObjectScale;
            TargetObject.scaleY = WholeObjectScale;

            BuildParts(_definition);

            // Center the container on the idle-pose visual centroid so the anchor
            // point lands on Om Nom's visual center, matching how the classic
            // 640×640 sprite frame centers the character.  The classic body sits
            // ~10 screen-px below the anchor; dividing by WholeObjectScale
            // reproduces that offset here.
            (float vcX, float vcY) = ComputeIdleVisualCenter(_definition, parts);
            const float classicBodyScreenOffsetY = 20f;
            TargetObject.width = (int)MathF.Round(vcX * 2f);
            TargetObject.height = (int)MathF.Round((vcY - (classicBodyScreenOffsetY / WholeObjectScale)) * 2f);
        }

        public GameObject TargetObject { get; }

        public float GetTargetBaseScaleX()
        {
            return WholeObjectScale;
        }

        public float GetTargetBaseScaleY()
        {
            return WholeObjectScale;
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
                    return parts[i].GetCurrentTimelineIndex() == timelineId;
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

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            _ = t;
            _ = k;
            _ = i;
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
            Dictionary<string, Image> partsByName = new(definition.Parts.Count);
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition partDefinition = definition.Parts[i];

                // Keep per-part dimensions in tuned Flash->DX point space.
                FlashXmlImage part = FlashXmlImage.CreateWithResID(partDefinition.TextureResourceName, PartDimensionScale);
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

        private static (float X, float Y) ComputeIdleVisualCenter(
            FlashXmlAnimationDefinition definition,
            List<Image> builtParts)
        {
            float sumX = 0f;
            float sumY = 0f;
            int count = 0;

            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition partDef = definition.Parts[i];
                if (!partDef.Timelines.TryGetValue(IdleLoopTimeline, out FlashXmlTimelineDefinition timeline)
                    || timeline.PositionKeyFrames.Count == 0)
                {
                    continue;
                }

                FlashXmlFloat2KeyFrame firstFrame = timeline.PositionKeyFrames[0];
                Image part = builtParts[i];

                sumX += firstFrame.X + (part.width / 2f);
                sumY += firstFrame.Y + (part.height / 2f);
                count++;
            }

            return count > 0
                ? (sumX / count, sumY / count)
                : (definition.StageWidth / 2f, definition.StageHeight / 2f);
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
            Image delegateDriver = FindBestDriverPartWithTimeline(timelineId);
            Timeline timeline = delegateDriver?.GetTimeline(timelineId);
            if (timeline == null)
            {
                return;
            }

            if (timelineId == IdleLoopTimeline)
            {
                timeline.delegateTimelineDelegate = _externalTimelineDelegate;
                return;
            }

            if (FlashXmlTargetTimelineRules.ShouldBindFollowupDelegate(timelineId))
            {
                timeline.delegateTimelineDelegate = this;
                _driverTimeline = timeline;
                _driverTimelineId = timelineId;
            }
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

                if (timelineId == IdleLoopTimeline)
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
        private const int ExcitedTimeline = FlashXmlTargetTimelineRules.ExcitedTimeline;
        private const int MouthOpeningTimeline = FlashXmlTargetTimelineRules.MouthOpeningTimeline;
        private const int MouthClosingTimeline = FlashXmlTargetTimelineRules.MouthClosingTimeline;
        private const int SadTimeline = FlashXmlTargetTimelineRules.SadTimeline;
        private const int ChewingTimeline = FlashXmlTargetTimelineRules.ChewingTimeline;
        private const int SleepingTimeline = FlashXmlTargetTimelineRules.SleepingTimeline;
        private const int GreetingTimeline = FlashXmlTargetTimelineRules.GreetingTimeline;
    }
}
