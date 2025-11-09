using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Audio;
using System;

namespace CutTheRope.game
{
    internal class Spikes : CTRGameObject, TimelineDelegate, ButtonDelegate
    {
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
                this.rotateButton.anchor = this.rotateButton.parentAnchor = 18;
                this.addChild(this.rotateButton);
                Vector quadOffset = Image.getQuadOffset(97, num);
                Vector quadSize = Image.getQuadSize(97, num);
                Vector vector = CTRMathHelper.vectSub(CTRMathHelper.vect(image.texture.preCutSize.x, image.texture.preCutSize.y), CTRMathHelper.vectAdd(quadSize, quadOffset));
                this.rotateButton.setTouchIncreaseLeftRightTopBottom(0f - quadOffset.x + quadSize.x / 2f, 0f - vector.x + quadSize.x / 2f, 0f - quadOffset.y + quadSize.y / 2f, 0f - vector.y + quadSize.y / 2f);
            }
            this.passColorToChilds = false;
            this.spikesNormal = false;
            this.origRotation = this.rotation = (float)an;
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

        public virtual void updateRotation()
        {
            float num = (!this.electro) ? this.texture.quadRects[this.quadToDraw].w : ((float)this.width - FrameworkTypes.RTPD(400.0));
            num /= 2f;
            this.t1.x = this.x - num;
            this.t2.x = this.x + num;
            this.t1.y = this.t2.y = this.y - 5f;
            this.b1.x = this.t1.x;
            this.b2.x = this.t2.x;
            this.b1.y = this.b2.y = this.y + 5f;
            this.angle = (double)CTRMathHelper.DEGREES_TO_RADIANS(this.rotation);
            this.t1 = CTRMathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
            this.t2 = CTRMathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
            this.b1 = CTRMathHelper.vectRotateAround(this.b1, this.angle, this.x, this.y);
            this.b2 = CTRMathHelper.vectRotateAround(this.b2, this.angle, this.x, this.y);
        }

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

        public virtual void turnElectroOn()
        {
            this.electroOn = true;
            this.playTimeline(1);
            this.electroTimer = this.onTime;
            this.sndElectric = CTRSoundMgr._playSoundLooped(28);
        }

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

        public virtual void setToggled(int t)
        {
            this.toggled = t;
        }

        public virtual int getToggled()
        {
            return this.toggled;
        }

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

        public virtual void timelineReachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public virtual void timelineFinished(Timeline t)
        {
            this.updateRotationFlag = false;
        }

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

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        private const float SPIKES_HEIGHT = 10f;

        private int toggled;

        public double angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public bool electro;

        public float initialDelay;

        public float onTime;

        public float offTime;

        public bool electroOn;

        public float electroTimer;

        private bool updateRotationFlag;

        private bool spikesNormal;

        private float origRotation;

        public Button rotateButton;

        public int touchIndex;

        public Spikes.rotateAllSpikesWithID delegateRotateAllSpikesWithID;

        private SoundEffectInstance sndElectric;

        private enum SPIKES_ANIM
        {
            SPIKES_ANIM_ELECTRODES_BASE,
            SPIKES_ANIM_ELECTRODES_ELECTRIC,
            SPIKES_ANIM_ROTATION_ADJUSTED
        }

        private enum SPIKES_ROTATION
        {
            SPIKES_ROTATION_BUTTON
        }

        // (Invoke) Token: 0x06000689 RID: 1673
        public delegate void rotateAllSpikesWithID(int sid);
    }
}
