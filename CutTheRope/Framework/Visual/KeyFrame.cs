using System.Collections.Generic;

namespace CutTheRope.Framework.Visual
{
    internal sealed class KeyFrame
    {
        public KeyFrame()
        {
            value = new KeyFrameValue();
        }

        public static KeyFrame MakeAction(List<CTRAction> actions, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.action.actionSet = actions;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_ACTION,
                transitionType = TransitionType.FRAME_TRANSITION_LINEAR,
                value = keyFrameValue
            };
        }

        public static KeyFrame MakeSingleAction(BaseElement target, string action, int p, int sp, float time)
        {
            return MakeAction([CTRAction.CreateAction(target, action, p, sp)], time);
        }

        public static KeyFrame MakePos(int x, int y, TransitionType transition, float time)
        {
            return MakePosCore(x, y, transition, time);
        }

        public static KeyFrame MakePos(float x, float y, TransitionType transition, float time)
        {
            return MakePosCore(x, y, transition, time);
        }

        private static KeyFrame MakePosCore(float x, float y, TransitionType transition, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.pos.x = x;
            keyFrameValue.pos.y = y;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_POSITION,
                transitionType = transition,
                value = keyFrameValue
            };
        }

        public static KeyFrame MakeScale(float x, float y, TransitionType transition, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.scale.scaleX = x;
            keyFrameValue.scale.scaleY = y;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_SCALE,
                transitionType = transition,
                value = keyFrameValue
            };
        }

        public static KeyFrame MakeRotation(int r, TransitionType transition, float time)
        {
            return MakeRotationCore(r, transition, time);
        }

        public static KeyFrame MakeRotation(float r, TransitionType transition, float time)
        {
            return MakeRotationCore(r, transition, time);
        }

        private static KeyFrame MakeRotationCore(float r, TransitionType transition, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.rotation.angle = r;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_ROTATION,
                transitionType = transition,
                value = keyFrameValue
            };
        }

        public static KeyFrame MakeSkew(float x, float y, TransitionType transition, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.skew.skewX = x;
            keyFrameValue.skew.skewY = y;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_SKEW,
                transitionType = transition,
                value = keyFrameValue
            };
        }

        public static KeyFrame MakeColor(RGBAColor c, TransitionType transition, float time)
        {
            KeyFrameValue keyFrameValue = new();
            keyFrameValue.color.rgba = c;
            return new KeyFrame
            {
                timeOffset = time,
                trackType = Track.TrackType.TRACK_COLOR,
                transitionType = transition,
                value = keyFrameValue
            };
        }

        public float timeOffset;

        public Track.TrackType trackType;

        public TransitionType transitionType;

        public KeyFrameValue value;

        public enum TransitionType
        {
            FRAME_TRANSITION_LINEAR,
            FRAME_TRANSITION_IMMEDIATE,
            FRAME_TRANSITION_EASE_IN,
            FRAME_TRANSITION_EASE_OUT,
            // Flash XML specific interpolation modes from iOS runtime.
            FRAME_TRANSITION_FLASH_LINEAR,
            FRAME_TRANSITION_FLASH_EASE_IN,
            FRAME_TRANSITION_FLASH_EASE_OUT,
            FRAME_TRANSITION_FLASH_EASE_IN_OUT,
            FRAME_TRANSITION_FLASH_EASE_MIRRORED,
            FRAME_TRANSITION_FLASH_HOLD,
            FRAME_TRANSITION_FLASH_IMMEDIATE
        }
    }
}
