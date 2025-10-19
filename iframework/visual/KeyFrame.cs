using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000039 RID: 57
	internal class KeyFrame
	{
		// Token: 0x06000207 RID: 519 RVA: 0x0000A2A6 File Offset: 0x000084A6
		public KeyFrame()
		{
			this.value = new KeyFrameValue();
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000A2BC File Offset: 0x000084BC
		public static KeyFrame makeAction(DynamicArray actions, float time)
		{
			List<Action> list = new List<Action>();
			foreach (object obj in actions)
			{
				Action action = (Action)obj;
				list.Add(action);
			}
			return KeyFrame.makeAction(list, time);
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000A320 File Offset: 0x00008520
		public static KeyFrame makeAction(List<Action> actions, float time)
		{
			KeyFrameValue keyFrameValue = new KeyFrameValue();
			keyFrameValue.action.actionSet = actions;
			return new KeyFrame
			{
				timeOffset = time,
				trackType = Track.TrackType.TRACK_ACTION,
				transitionType = KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR,
				value = keyFrameValue
			};
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000A360 File Offset: 0x00008560
		public static KeyFrame makeSingleAction(BaseElement target, string action, int p, int sp, double time)
		{
			return KeyFrame.makeSingleAction(target, action, p, sp, (float)time);
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000A36E File Offset: 0x0000856E
		public static KeyFrame makeSingleAction(BaseElement target, string action, int p, int sp, float time)
		{
			return KeyFrame.makeAction(new List<Action> { Action.createAction(target, action, p, sp) }, time);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000A38B File Offset: 0x0000858B
		public static KeyFrame makePos(double x, double y, KeyFrame.TransitionType transition, double time)
		{
			return KeyFrame.makePos((int)x, (int)y, transition, (float)time);
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000A39C File Offset: 0x0000859C
		public static KeyFrame makePos(int x, int y, KeyFrame.TransitionType transition, float time)
		{
			KeyFrameValue keyFrameValue = new KeyFrameValue();
			keyFrameValue.pos.x = (float)x;
			keyFrameValue.pos.y = (float)y;
			return new KeyFrame
			{
				timeOffset = time,
				trackType = Track.TrackType.TRACK_POSITION,
				transitionType = transition,
				value = keyFrameValue
			};
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000A3EA File Offset: 0x000085EA
		public static KeyFrame makeScale(double x, double y, KeyFrame.TransitionType transition, double time)
		{
			return KeyFrame.makeScale((float)x, (float)y, transition, (float)time);
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000A3F8 File Offset: 0x000085F8
		public static KeyFrame makeScale(float x, float y, KeyFrame.TransitionType transition, float time)
		{
			KeyFrameValue keyFrameValue = new KeyFrameValue();
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

		// Token: 0x06000210 RID: 528 RVA: 0x0000A444 File Offset: 0x00008644
		public static KeyFrame makeRotation(double r, KeyFrame.TransitionType transition, double time)
		{
			return KeyFrame.makeRotation((int)r, transition, (float)time);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000A450 File Offset: 0x00008650
		public static KeyFrame makeRotation(int r, KeyFrame.TransitionType transition, float time)
		{
			KeyFrameValue keyFrameValue = new KeyFrameValue();
			keyFrameValue.rotation.angle = (float)r;
			return new KeyFrame
			{
				timeOffset = time,
				trackType = Track.TrackType.TRACK_ROTATION,
				transitionType = transition,
				value = keyFrameValue
			};
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000A491 File Offset: 0x00008691
		public static KeyFrame makeColor(RGBAColor c, KeyFrame.TransitionType transition, double time)
		{
			return KeyFrame.makeColor(c, transition, (float)time);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000A49C File Offset: 0x0000869C
		public static KeyFrame makeColor(RGBAColor c, KeyFrame.TransitionType transition, float time)
		{
			KeyFrameValue keyFrameValue = new KeyFrameValue();
			keyFrameValue.color.rgba = c;
			return new KeyFrame
			{
				timeOffset = time,
				trackType = Track.TrackType.TRACK_COLOR,
				transitionType = transition,
				value = keyFrameValue
			};
		}

		// Token: 0x0400014B RID: 331
		public float timeOffset;

		// Token: 0x0400014C RID: 332
		public Track.TrackType trackType;

		// Token: 0x0400014D RID: 333
		public KeyFrame.TransitionType transitionType;

		// Token: 0x0400014E RID: 334
		public KeyFrameValue value;

		// Token: 0x020000AD RID: 173
		public enum TransitionType
		{
			// Token: 0x04000897 RID: 2199
			FRAME_TRANSITION_LINEAR,
			// Token: 0x04000898 RID: 2200
			FRAME_TRANSITION_IMMEDIATE,
			// Token: 0x04000899 RID: 2201
			FRAME_TRANSITION_EASE_IN,
			// Token: 0x0400089A RID: 2202
			FRAME_TRANSITION_EASE_OUT
		}
	}
}
