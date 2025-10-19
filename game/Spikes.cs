using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.game
{
	// Token: 0x02000095 RID: 149
	internal class Spikes : CTRGameObject, TimelineDelegate, ButtonDelegate
	{
		// Token: 0x060005EF RID: 1519 RVA: 0x00031B98 File Offset: 0x0002FD98
		public virtual NSObject initWithPosXYWidthAndAngleToggled(float px, float py, int w, double an, int t)
		{
			int textureResID = -1;
			if (t != -1)
			{
				textureResID = 93 + w - 1;
			}
			else
			{
				switch (w)
				{
				case 1:
					textureResID = 88;
					break;
				case 2:
					textureResID = 89;
					break;
				case 3:
					textureResID = 90;
					break;
				case 4:
					textureResID = 91;
					break;
				case 5:
					textureResID = 92;
					break;
				}
			}
			if (this.initWithTexture(Application.getTexture(textureResID)) == null)
			{
				return null;
			}
			if (t > 0)
			{
				this.doRestoreCutTransparency();
				int num = (t - 1) * 2;
				int q = 1 + (t - 1) * 2;
				Image image = Image.Image_createWithResIDQuad(97, num);
				Image image2 = Image.Image_createWithResIDQuad(97, q);
				image.doRestoreCutTransparency();
				image2.doRestoreCutTransparency();
				this.rotateButton = new Button().initWithUpElementDownElementandID(image, image2, 0);
				this.rotateButton.delegateButtonDelegate = this;
				this.rotateButton.anchor = (this.rotateButton.parentAnchor = 18);
				this.addChild(this.rotateButton);
				Vector quadOffset = Image.getQuadOffset(97, num);
				Vector quadSize = Image.getQuadSize(97, num);
				Vector vector = MathHelper.vectSub(MathHelper.vect(image.texture.preCutSize.x, image.texture.preCutSize.y), MathHelper.vectAdd(quadSize, quadOffset));
				this.rotateButton.setTouchIncreaseLeftRightTopBottom(0f - quadOffset.x + quadSize.x / 2f, 0f - vector.x + quadSize.x / 2f, 0f - quadOffset.y + quadSize.y / 2f, 0f - vector.y + quadSize.y / 2f);
			}
			this.passColorToChilds = false;
			this.spikesNormal = false;
			this.origRotation = (this.rotation = (float)an);
			this.x = px;
			this.y = py;
			this.setToggled(t);
			this.updateRotation();
			if (w == 5)
			{
				this.addAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 0);
				this.addAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 4);
				this.doRestoreCutTransparency();
			}
			this.touchIndex = -1;
			return this;
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00031DB0 File Offset: 0x0002FFB0
		public virtual void updateRotation()
		{
			float num = ((!this.electro) ? this.texture.quadRects[this.quadToDraw].w : ((float)this.width - FrameworkTypes.RTPD(400.0)));
			num /= 2f;
			this.t1.x = this.x - num;
			this.t2.x = this.x + num;
			this.t1.y = (this.t2.y = this.y - 5f);
			this.b1.x = this.t1.x;
			this.b2.x = this.t2.x;
			this.b1.y = (this.b2.y = this.y + 5f);
			this.angle = (double)MathHelper.DEGREES_TO_RADIANS(this.rotation);
			this.t1 = MathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
			this.t2 = MathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
			this.b1 = MathHelper.vectRotateAround(this.b1, this.angle, this.x, this.y);
			this.b2 = MathHelper.vectRotateAround(this.b2, this.angle, this.x, this.y);
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00031F3B File Offset: 0x0003013B
		public virtual void turnElectroOff()
		{
			this.electroOn = false;
			this.playTimeline(0);
			this.electroTimer = this.offTime;
			if (this.sndElectric != null)
			{
				this.sndElectric.Stop();
				this.sndElectric = null;
			}
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x00031F71 File Offset: 0x00030171
		public virtual void turnElectroOn()
		{
			this.electroOn = true;
			this.playTimeline(1);
			this.electroTimer = this.onTime;
			this.sndElectric = CTRSoundMgr._playSoundLooped(28);
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00031F9C File Offset: 0x0003019C
		public virtual void rotateSpikes()
		{
			this.spikesNormal = !this.spikesNormal;
			this.removeTimeline(2);
			float num = (float)(this.spikesNormal ? 90 : 0);
			float num2 = this.origRotation + num;
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline.addKeyFrame(KeyFrame.makeRotation((int)this.rotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
			timeline.addKeyFrame(KeyFrame.makeRotation((int)num2, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, Math.Abs(num2 - this.rotation) / 90f * 0.3f));
			timeline.delegateTimelineDelegate = this;
			this.addTimelinewithID(timeline, 2);
			this.playTimeline(2);
			this.updateRotationFlag = true;
			this.rotateButton.scaleX = 0f - this.rotateButton.scaleX;
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x0003205B File Offset: 0x0003025B
		public virtual void setToggled(int t)
		{
			this.toggled = t;
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00032064 File Offset: 0x00030264
		public virtual int getToggled()
		{
			return this.toggled;
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x0003206C File Offset: 0x0003026C
		public override void update(float delta)
		{
			base.update(delta);
			if (this.mover != null || this.updateRotationFlag)
			{
				this.updateRotation();
			}
			if (!this.electro)
			{
				return;
			}
			if (this.electroOn)
			{
				Mover.moveVariableToTarget(ref this.electroTimer, 0f, 1f, delta);
				if ((double)this.electroTimer == 0.0)
				{
					this.turnElectroOff();
					return;
				}
			}
			else
			{
				Mover.moveVariableToTarget(ref this.electroTimer, 0f, 1f, delta);
				if ((double)this.electroTimer == 0.0)
				{
					this.turnElectroOn();
				}
			}
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00032106 File Offset: 0x00030306
		public virtual void timelineReachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00032108 File Offset: 0x00030308
		public virtual void timelineFinished(Timeline t)
		{
			this.updateRotationFlag = false;
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00032111 File Offset: 0x00030311
		public virtual void onButtonPressed(int n)
		{
			if (n == 0)
			{
				this.delegateRotateAllSpikesWithID(this.toggled);
				if (this.spikesNormal)
				{
					CTRSoundMgr._playSound(42);
					return;
				}
				CTRSoundMgr._playSound(43);
			}
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x0003213E File Offset: 0x0003033E
		public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}

		// Token: 0x04000823 RID: 2083
		private const float SPIKES_HEIGHT = 10f;

		// Token: 0x04000824 RID: 2084
		private int toggled;

		// Token: 0x04000825 RID: 2085
		public double angle;

		// Token: 0x04000826 RID: 2086
		public Vector t1;

		// Token: 0x04000827 RID: 2087
		public Vector t2;

		// Token: 0x04000828 RID: 2088
		public Vector b1;

		// Token: 0x04000829 RID: 2089
		public Vector b2;

		// Token: 0x0400082A RID: 2090
		public bool electro;

		// Token: 0x0400082B RID: 2091
		public float initialDelay;

		// Token: 0x0400082C RID: 2092
		public float onTime;

		// Token: 0x0400082D RID: 2093
		public float offTime;

		// Token: 0x0400082E RID: 2094
		public bool electroOn;

		// Token: 0x0400082F RID: 2095
		public float electroTimer;

		// Token: 0x04000830 RID: 2096
		private bool updateRotationFlag;

		// Token: 0x04000831 RID: 2097
		private bool spikesNormal;

		// Token: 0x04000832 RID: 2098
		private float origRotation;

		// Token: 0x04000833 RID: 2099
		public Button rotateButton;

		// Token: 0x04000834 RID: 2100
		public int touchIndex;

		// Token: 0x04000835 RID: 2101
		public Spikes.rotateAllSpikesWithID delegateRotateAllSpikesWithID;

		// Token: 0x04000836 RID: 2102
		private SoundEffectInstance sndElectric;

		// Token: 0x020000CC RID: 204
		private enum SPIKES_ANIM
		{
			// Token: 0x04000905 RID: 2309
			SPIKES_ANIM_ELECTRODES_BASE,
			// Token: 0x04000906 RID: 2310
			SPIKES_ANIM_ELECTRODES_ELECTRIC,
			// Token: 0x04000907 RID: 2311
			SPIKES_ANIM_ROTATION_ADJUSTED
		}

		// Token: 0x020000CD RID: 205
		private enum SPIKES_ROTATION
		{
			// Token: 0x04000909 RID: 2313
			SPIKES_ROTATION_BUTTON
		}

		// Token: 0x020000CE RID: 206
		// (Invoke) Token: 0x06000689 RID: 1673
		public delegate void rotateAllSpikesWithID(int sid);
	}
}
