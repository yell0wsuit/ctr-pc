using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A single keyframe in a timeline track, holding a time offset, transition type, and parameter values.
    /// </summary>
    internal sealed class KeyFrame
    {
        /// <summary>
        /// Initializes a new <see cref="KeyFrame"/> with default values.
        /// </summary>
        public KeyFrame()
        {
            value = new KeyFrameValue();
        }

        /// <summary>
        /// Creates an action keyframe with the specified <paramref name="actions"/> and <paramref name="time"/> offset.
        /// </summary>
        /// <param name="actions">Actions to execute at this keyframe.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new action keyframe.</returns>
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

        /// <summary>
        /// Creates an action keyframe with a single <paramref name="action"/>.
        /// </summary>
        /// <param name="target">Target element for the action.</param>
        /// <param name="action">Action name.</param>
        /// <param name="p">Primary action parameter.</param>
        /// <param name="sp">Secondary action parameter.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new action keyframe wrapping a single <paramref name="action"/>.</returns>
        public static KeyFrame MakeSingleAction(BaseElement target, string action, int p, int sp, float time)
        {
            return MakeAction([CTRAction.CreateAction(target, action, p, sp)], time);
        }

        /// <summary>
        /// Creates a position keyframe with integer coordinates.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new position keyframe.</returns>
        public static KeyFrame MakePos(int x, int y, TransitionType transition, float time)
        {
            return MakePosCore(x, y, transition, time);
        }

        /// <summary>
        /// Creates a position keyframe with float coordinates.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new position keyframe.</returns>
        public static KeyFrame MakePos(float x, float y, TransitionType transition, float time)
        {
            return MakePosCore(x, y, transition, time);
        }

        /// <summary>
        /// Creates a position keyframe.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new position keyframe.</returns>
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

        /// <summary>
        /// Creates a scale keyframe.
        /// </summary>
        /// <param name="x">Horizontal scale factor.</param>
        /// <param name="y">Vertical scale factor.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new scale keyframe.</returns>
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

        /// <summary>
        /// Creates a rotation keyframe with an integer angle.
        /// </summary>
        /// <param name="r">Rotation angle in degrees.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new rotation keyframe.</returns>
        public static KeyFrame MakeRotation(int r, TransitionType transition, float time)
        {
            return MakeRotationCore(r, transition, time);
        }

        /// <summary>
        /// Creates a rotation keyframe with a float angle.
        /// </summary>
        /// <param name="r">Rotation angle in degrees.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new rotation keyframe.</returns>
        public static KeyFrame MakeRotation(float r, TransitionType transition, float time)
        {
            return MakeRotationCore(r, transition, time);
        }

        /// <summary>
        /// Creates a rotation keyframe.
        /// </summary>
        /// <param name="r">Rotation angle in degrees.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new rotation keyframe.</returns>
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

        /// <summary>
        /// Creates a skew keyframe.
        /// </summary>
        /// <param name="x">Horizontal skew factor.</param>
        /// <param name="y">Vertical skew factor.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new skew keyframe.</returns>
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

        /// <summary>
        /// Creates a color keyframe.
        /// </summary>
        /// <param name="c">Target color.</param>
        /// <param name="transition">Transition type.</param>
        /// <param name="time">Time offset in seconds.</param>
        /// <returns>A new color keyframe.</returns>
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

        /// <summary>
        /// Time offset in seconds from the start of the timeline or previous keyframe.
        /// </summary>
        public float timeOffset;

        /// <summary>
        /// Track type this keyframe belongs to.
        /// </summary>
        public Track.TrackType trackType;

        /// <summary>
        /// Interpolation mode between this keyframe and the next.
        /// </summary>
        public TransitionType transitionType;

        /// <summary>
        /// Parameter values for this keyframe.
        /// </summary>
        public KeyFrameValue value;

        /// <summary>
        /// Interpolation modes for keyframe transitions.
        /// </summary>
        public enum TransitionType
        {
            /// <summary>
            /// Linear interpolation.
            /// </summary>
            FRAME_TRANSITION_LINEAR,

            /// <summary>
            /// Instant transition with no interpolation.
            /// </summary>
            FRAME_TRANSITION_IMMEDIATE,

            /// <summary>
            /// Ease-in (slow start) interpolation.
            /// </summary>
            FRAME_TRANSITION_EASE_IN,

            /// <summary>
            /// Ease-out (slow end) interpolation.
            /// </summary>
            FRAME_TRANSITION_EASE_OUT,

            /// <summary>
            /// Flash XML linear interpolation.
            /// </summary>
            // Flash XML specific interpolation modes from iOS runtime.
            FRAME_TRANSITION_FLASH_LINEAR,

            /// <summary>
            /// Flash XML ease-in interpolation.
            /// </summary>
            FRAME_TRANSITION_FLASH_EASE_IN,

            /// <summary>
            /// Flash XML ease-out interpolation.
            /// </summary>
            FRAME_TRANSITION_FLASH_EASE_OUT,

            /// <summary>
            /// Flash XML ease-in-out interpolation.
            /// </summary>
            FRAME_TRANSITION_FLASH_EASE_IN_OUT,

            /// <summary>
            /// Flash XML mirrored ease interpolation.
            /// </summary>
            FRAME_TRANSITION_FLASH_EASE_MIRRORED,

            /// <summary>
            /// Flash XML hold (no change until next keyframe).
            /// </summary>
            FRAME_TRANSITION_FLASH_HOLD,

            /// <summary>
            /// Flash XML instant transition.
            /// </summary>
            FRAME_TRANSITION_FLASH_IMMEDIATE
        }
    }
}
