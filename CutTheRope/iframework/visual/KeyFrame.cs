using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal class KeyFrame
    {
        public KeyFrame()
        {
            value = new KeyFrameValue();
        }

        public static KeyFrame makeAction(DynamicArray actions, float time)
        {
            List<CTRAction> list = new();
            foreach (object obj in actions)
            {
                CTRAction action = (CTRAction)obj;
                list.Add(action);
            }
            return makeAction(list, time);
        }

        public static KeyFrame makeAction(List<CTRAction> actions, float time)
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

        public static KeyFrame makeSingleAction(BaseElement target, string action, int p, int sp, double time)
        {
            return makeSingleAction(target, action, p, sp, (float)time);
        }

        public static KeyFrame makeSingleAction(BaseElement target, string action, int p, int sp, float time)
        {
            return makeAction(new List<CTRAction> { CTRAction.createAction(target, action, p, sp) }, time);
        }

        public static KeyFrame makePos(double x, double y, TransitionType transition, double time)
        {
            return makePos((int)x, (int)y, transition, (float)time);
        }

        public static KeyFrame makePos(int x, int y, TransitionType transition, float time)
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

        public static KeyFrame makeScale(double x, double y, TransitionType transition, double time)
        {
            return makeScale((float)x, (float)y, transition, (float)time);
        }

        public static KeyFrame makeScale(float x, float y, TransitionType transition, float time)
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

        public static KeyFrame makeRotation(double r, TransitionType transition, double time)
        {
            return makeRotation((int)r, transition, (float)time);
        }

        public static KeyFrame makeRotation(int r, TransitionType transition, float time)
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

        public static KeyFrame makeColor(RGBAColor c, TransitionType transition, double time)
        {
            return makeColor(c, transition, (float)time);
        }

        public static KeyFrame makeColor(RGBAColor c, TransitionType transition, float time)
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
