using System.Collections.Generic;

namespace CutTheRope.Framework.Visual
{
    internal sealed class KeyFrame
    {
        public KeyFrame()
        {
            value = new KeyFrameValue();
        }

        public static KeyFrame MakeAction(DynamicArray<CTRAction> actions, float time)
        {
            List<CTRAction> list = [.. actions];
            return MakeAction(list, time);
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

        public static KeyFrame MakeSingleAction(BaseElement target, string action, int p, int sp, double time)
        {
            return MakeSingleAction(target, action, p, sp, (float)time);
        }

        public static KeyFrame MakeSingleAction(BaseElement target, string action, int p, int sp, float time)
        {
            return MakeAction([CTRAction.CreateAction(target, action, p, sp)], time);
        }

        public static KeyFrame MakePos(double x, double y, TransitionType transition, double time)
        {
            return MakePos((int)x, (int)y, transition, (float)time);
        }

        public static KeyFrame MakePos(int x, int y, TransitionType transition, float time)
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

        public static KeyFrame MakeScale(double x, double y, TransitionType transition, double time)
        {
            return MakeScale((float)x, (float)y, transition, (float)time);
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

        public static KeyFrame MakeRotation(double r, TransitionType transition, double time)
        {
            return MakeRotation((int)r, transition, (float)time);
        }

        public static KeyFrame MakeRotation(int r, TransitionType transition, float time)
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

        public static KeyFrame MakeColor(RGBAColor c, TransitionType transition, double time)
        {
            return MakeColor(c, transition, (float)time);
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
            FRAME_TRANSITION_EASE_OUT
        }
    }
}
