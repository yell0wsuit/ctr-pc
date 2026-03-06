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
    internal sealed class FlashXmlTargetAnimationBackend : ITargetAnimationBackend
    {
        private const int IdleLoopTimeline = 0;
        private const int IdleVariationOneTimeline = 1;
        private const int IdleVariationTwoTimeline = 2;
        private const int ExcitedTimeline = 3;
        private const int SadTimeline = 5;
        private const int ChewingTimeline = 6;
        private const int MouthOpeningTimeline = 7;
        private const int MouthClosingTimeline = 8;
        private const int GreetingTimeline = 10;
        private const int SleepingTimeline = 15;
        private const float PartDimensionScale = 0.65f;
        private const float WholeObjectScale = 1.5384616f;
        private readonly List<Image> parts = [];
        private readonly FlashXmlAnimationDefinition _definition;

        public FlashXmlTargetAnimationBackend(string xmlPath = null)
        {
            string resolvedXmlPath = string.IsNullOrWhiteSpace(xmlPath)
                ? ContentPaths.GetContentPath(Path.Combine(ContentPaths.AnimationsDirectory, "om_nom_original.xml"))
                : xmlPath;

            _definition = FlashXmlImporter.ParseFile(resolvedXmlPath);

            TargetObject = GameObject.GameObject_createWithResIDQuad(Resources.Img.CharAnimationsSmooth, 0);
            TargetObject.color = RGBAColor.transparentRGBA;
            TargetObject.passColorToChilds = false;
            // Flash part coordinates are authored in stage space, not atlas pre-cut space.
            // Keep the container sized to stage dimensions so center anchoring math matches Flash.
            TargetObject.width = (int)MathF.Round(_definition.StageWidth);
            TargetObject.height = (int)MathF.Round(_definition.StageHeight);
            TargetObject.scaleX = WholeObjectScale;
            TargetObject.scaleY = WholeObjectScale;

            BuildParts(_definition);
            DumpPartPositions(_definition);
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
            Play(TargetAnimationState.IdleLoop);

            // Mirror original backend timing semantics: only one idle-loop timeline should
            // trigger delegate callbacks that drive blink/idle timers.
            Image delegateDriver = FindFirstPartWithTimeline(IdleLoopTimeline);
            Timeline timeline = delegateDriver?.GetTimeline(IdleLoopTimeline);
            if (timeline != null)
            {
                timeline.delegateTimelineDelegate = timelineDelegate;
            }
        }

        public void Play(TargetAnimationState state)
        {
            if (!TryMapState(state, out int timelineId))
            {
                return;
            }

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
                    // Match iOS setToInitialStateOfTimeline: stop all timelines before playing the
                    // new one. Parts without the requested timeline are stopped and hidden so they
                    // don't keep ticking invisibly with stale pose from a previous one-shot.
                    if (parts[i].GetCurrentTimeline() != null)
                    {
                        parts[i].StopCurrentTimeline();
                    }

                    parts[i].visible = false;
                }
            }
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

        private void BuildParts(FlashXmlAnimationDefinition definition)
        {
            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition partDefinition = definition.Parts[i];

                // Keep per-part dimensions in tuned Flash->DX point space.
                FlashXmlImage part = FlashXmlImage.CreateWithResID(partDefinition.TextureResourceName, PartDimensionScale);
                part.anchor = 9;
                part.parentAnchor = 9;
                part.visible = ShouldStartVisible(partDefinition);
                part.useCustomAnchor = true;
                part.customAnchorX = partDefinition.AnchorX;
                part.customAnchorY = partDefinition.AnchorY;
                part.rotationCenterX = partDefinition.RotationCenterX;
                part.rotationCenterY = partDefinition.RotationCenterY;
                part.SetDrawQuad(partDefinition.QuadToDraw);

                BuildTimelines(part, partDefinition);

                _ = TargetObject.AddChild(part);
                parts.Add(part);
            }
        }

        private static void DumpPartPositions(FlashXmlAnimationDefinition definition)
        {
            List<string> lines = BuildPartPositionDebugLines(definition);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }

        private static List<string> BuildPartPositionDebugLines(FlashXmlAnimationDefinition definition)
        {
            const float positionScaleX = 1f;
            const float positionScaleY = 1f;
            const int preferredTimelineId = IdleLoopTimeline;

            List<string> lines = [];
            lines.Add(
                $"[OmNomFlashPartPos] transform scale=({FormatDebugFloat(positionScaleX)},{FormatDebugFloat(positionScaleY)}) rounding=nearest-int preferredTimeline={preferredTimelineId}");

            for (int i = 0; i < definition.Parts.Count; i++)
            {
                FlashXmlPartDefinition part = definition.Parts[i];
                int timelineId = preferredTimelineId;
                FlashXmlFloat2KeyFrame positionFrame = null;

                if (part.Timelines.TryGetValue(preferredTimelineId, out FlashXmlTimelineDefinition preferredTimeline)
                    && preferredTimeline.PositionKeyFrames.Count > 0)
                {
                    positionFrame = preferredTimeline.PositionKeyFrames[0];
                }
                else
                {
                    int fallbackTimelineId = int.MaxValue;
                    foreach ((int candidateTimelineId, FlashXmlTimelineDefinition timeline) in part.Timelines)
                    {
                        if (timeline.PositionKeyFrames.Count == 0 || candidateTimelineId >= fallbackTimelineId)
                        {
                            continue;
                        }

                        fallbackTimelineId = candidateTimelineId;
                        positionFrame = timeline.PositionKeyFrames[0];
                    }

                    if (fallbackTimelineId != int.MaxValue)
                    {
                        timelineId = fallbackTimelineId;
                    }
                }

                if (positionFrame == null)
                {
                    lines.Add($"[OmNomFlashPartPos] part={part.Name}; timeline=none; xml=(n/a); dx=(n/a); rounded=(n/a);");
                    continue;
                }

                float transformedX = positionFrame.X * positionScaleX;
                float transformedY = positionFrame.Y * positionScaleY;
                int roundedX = (int)MathF.Round(transformedX);
                int roundedY = (int)MathF.Round(transformedY);

                lines.Add(
                    $"[OmNomFlashPartPos] part={part.Name}; timeline={timelineId}; xml=({FormatDebugFloat(positionFrame.X)},{FormatDebugFloat(positionFrame.Y)}); dx=({FormatDebugFloat(transformedX)},{FormatDebugFloat(transformedY)}); rounded=({roundedX},{roundedY}); scale=({FormatDebugFloat(positionScaleX)},{FormatDebugFloat(positionScaleY)}); anchor=({FormatDebugFloat(part.AnchorX)},{FormatDebugFloat(part.AnchorY)});");
            }

            return lines;
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

        private static string FormatDebugFloat(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void BuildTimelines(FlashXmlImage part, FlashXmlPartDefinition partDefinition)
        {
            foreach ((int timelineId, FlashXmlTimelineDefinition timelineDefinition) in partDefinition.Timelines)
            {
                int maxKeyFrames = Math.Max(
                    Math.Max(timelineDefinition.PositionKeyFrames.Count, timelineDefinition.ScaleKeyFrames.Count),
                    Math.Max(
                        timelineDefinition.ColorKeyFrames.Count,
                        Math.Max(timelineDefinition.ActionKeyFrames.Count, timelineDefinition.SkewKeyFrames.Count)));
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
                    FlashXmlFloat2KeyFrame skewFrame = FindSkewFrameAtTime(
                        timelineDefinition.SkewKeyFrames,
                        frame.TimeOffset,
                        i);
                    float signedScaleY = skewFrame != null
                        ? MapSkewToSignedScaleY(frame.Y, skewFrame.X, skewFrame.Y)
                        : frame.Y;

                    timeline.AddKeyFrame(KeyFrame.MakeScale(
                        frame.X,
                        signedScaleY,
                        MapTransition(frame.Interpolation),
                        frame.TimeOffset));
                }

                for (int i = 0; i < timelineDefinition.SkewKeyFrames.Count; i++)
                {
                    FlashXmlFloat2KeyFrame frame = timelineDefinition.SkewKeyFrames[i];
                    timeline.AddKeyFrame(KeyFrame.MakeRotation(
                        MapSkewToRotationDegrees(frame.X, frame.Y),
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
                        CTRAction ctrAction = BuildAction(part, action);
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

        private static CTRAction BuildAction(Image part, FlashXmlActionCommand action)
        {
            return action.Command switch
            {
                "AC_SDQ" => CTRAction.CreateAction(
                    part,
                    Image.ACTION_SET_DRAWQUAD,
                    ParseActionInt(action.Param1),
                    0),
                "AC_SV" => CTRAction.CreateAction(
                    part,
                    BaseElement.ACTION_SET_VISIBLE,
                    0,
                    ParseActionInt(action.Param2)),
                "AC_SAP" => CTRAction.CreateAction(
                    part,
                    BaseElement.ACTION_SET_CUSTOM_ANCHOR,
                    ParseActionFloat(action.Param1),
                    ParseActionFloat(action.Param2)),
                "AC_SRC" => CTRAction.CreateAction(
                    part,
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

        private static FlashXmlFloat2KeyFrame FindSkewFrameAtTime(
            IReadOnlyList<FlashXmlFloat2KeyFrame> skewFrames,
            float timeOffset,
            int fallbackIndex)
        {
            const float epsilon = 0.0001f;
            for (int i = 0; i < skewFrames.Count; i++)
            {
                if (MathF.Abs(skewFrames[i].TimeOffset - timeOffset) <= epsilon)
                {
                    return skewFrames[i];
                }
            }

            return fallbackIndex >= 0 && fallbackIndex < skewFrames.Count
                ? skewFrames[fallbackIndex]
                : null;
        }

        private static float MapSkewToRotationDegrees(float skewXDegrees, float skewYDegrees)
        {
            _ = skewXDegrees;
            // Flash exports skew as axis rotations; runtime rotation matches Y axis value.
            return skewYDegrees;
        }

        private static float MapSkewToSignedScaleY(float scaleY, float skewXDegrees, float skewYDegrees)
        {
            float delta = NormalizeDegrees(skewXDegrees - skewYDegrees);
            float sign = MathF.Abs(delta) > 90f ? -1f : 1f;
            return MathF.Abs(scaleY) * sign;
        }

        private static float NormalizeDegrees(float value)
        {
            float normalized = value % 360f;
            if (normalized > 180f)
            {
                normalized -= 360f;
            }
            else if (normalized < -180f)
            {
                normalized += 360f;
            }

            return normalized;
        }

        private static KeyFrame.TransitionType MapTransition(int interpolation)
        {
            return interpolation switch
            {
                // Match iOS Flash runtime interpolation codes:
                // 0=linear, 1=immediate, 2=ease-in, 3=ease-out, 4/5=custom easing, 6=hold.
                0 => KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
                1 => KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_IMMEDIATE,
                2 => KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN,
                3 => KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT,
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
    }
}
