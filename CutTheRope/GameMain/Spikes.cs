using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.GameMain
{
    internal sealed class Spikes : CTRGameObject, ITimelineDelegate, IButtonDelegation
    {
        public Spikes InitWithPosXYWidthAndAngleToggled(float px, float py, int w, float an, int t)
        {
            (string textureName, int spikeQuad) = GetSpikeTextureAndQuad(w, t != -1);
            if (textureName == null || InitWithTexture(Application.GetTexture(textureName)) == null)
            {
                return null;
            }
            if (spikeQuad > 0)
            {
                SetDrawQuad(spikeQuad);
            }
            if (t > 0)
            {
                DoRestoreCutTransparency();
                int buttonQuad = ButtonFirstQuad + ((t - 1) * ButtonFramesPerToggle);
                int q = ButtonFirstQuad + ButtonPressedQuadOffset + ((t - 1) * ButtonFramesPerToggle);
                Image image = Image_createWithResIDQuad(Resources.Img.ObjSpikes, buttonQuad);
                Image image2 = Image_createWithResIDQuad(Resources.Img.ObjSpikes, q);
                image.DoRestoreCutTransparency();
                image2.DoRestoreCutTransparency();
                rotateButton = new Button().InitWithUpElementDownElementandID(image, image2, SpikesButtonId.Rotate);
                rotateButton.delegateButtonDelegate = this;
                rotateButton.anchor = rotateButton.parentAnchor = 18;
                _ = AddChild(rotateButton);
                Vector quadOffset = GetQuadOffset(Resources.Img.ObjSpikes, buttonQuad);
                Vector quadSize = GetQuadSize(Resources.Img.ObjSpikes, buttonQuad);
                Vector vector = VectSub(Vect(image.texture.preCutSize.X, image.texture.preCutSize.Y), VectAdd(quadSize, quadOffset));
                rotateButton.SetTouchIncreaseLeftRightTopBottom(0f - quadOffset.X + (quadSize.X / 2f), 0f - vector.X + (quadSize.X / 2f), 0f - quadOffset.Y + (quadSize.Y / 2f), 0f - vector.Y + (quadSize.Y / 2f));
            }
            passColorToChilds = false;
            spikesNormal = false;
            origRotation = rotation = an;
            x = px;
            y = py;
            SetToggled(t);
            UpdateRotation();
            if (w == ElectrodesWidthIndex)
            {
                AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 0);
                AddAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 4);
                DoRestoreCutTransparency();
            }
            touchIndex = -1;
            return this;
        }

        public void UpdateRotation()
        {
            float halfWidth = !electro ? texture.quadRects[quadToDraw].w : width - RTPD(400);
            halfWidth /= 2f;
            t1.X = x - halfWidth;
            t2.X = x + halfWidth;
            t1.Y = t2.Y = y - 5f;
            b1.X = t1.X;
            b2.X = t2.X;
            b1.Y = b2.Y = y + 5f;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        public void TurnElectroOff()
        {
            electroOn = false;
            PlayTimeline(0);
            electroTimer = offTime;
            sndElectric?.Stop();
            sndElectric = null;
        }

        public void TurnElectroOn()
        {
            electroOn = true;
            PlayTimeline(1);
            electroTimer = onTime;
            sndElectric = CTRSoundMgr.PlaySoundLooped(Resources.Snd.Electric);
        }

        public void RotateSpikes()
        {
            spikesNormal = !spikesNormal;
            RemoveTimeline(2);
            float rotationOffset = spikesNormal ? DEG_90 : 0;
            float targetRotation = origRotation + rotationOffset;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)rotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)targetRotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, MathF.Abs(targetRotation - rotation) / DEG_90 * 0.3f));
            timeline.delegateTimelineDelegate = this;
            AddTimelinewithID(timeline, 2);
            PlayTimeline(2);
            updateRotationFlag = true;
            rotateButton.scaleX = 0f - rotateButton.scaleX;
        }

        public void SetToggled(int t)
        {
            toggled = t;
        }

        public int GetToggled()
        {
            return toggled;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null || updateRotationFlag)
            {
                UpdateRotation();
            }
            if (!electro)
            {
                return;
            }
            if (electroOn)
            {
                _ = Mover.MoveVariableToTarget(ref electroTimer, 0f, 1f, delta);
                if (electroTimer == 0)
                {
                    TurnElectroOff();
                    return;
                }
            }
            else
            {
                _ = Mover.MoveVariableToTarget(ref electroTimer, 0f, 1f, delta);
                if (electroTimer == 0)
                {
                    TurnElectroOn();
                }
            }
        }

        public static void TimelineReachedKeyFramewithIndex(Timeline _, KeyFrame _1, int _2)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            updateRotationFlag = false;
        }

        public void OnButtonPressed(SpikesButtonId n)
        {
            if (n == SpikesButtonId.Rotate)
            {
                delegateRotateAllSpikesWithID(toggled);
                if (spikesNormal)
                {
                    CTRSoundMgr.PlaySound(Resources.Snd.SpikeRotateIn);
                    return;
                }
                CTRSoundMgr.PlaySound(Resources.Snd.SpikeRotateOut);
            }
        }

        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(SpikesButtonId.FromButtonId(buttonId));
        }

        public void TimelinereachedKeyFramewithIndex(Timeline _, KeyFrame _1, int _2)
        {
        }

        private int toggled;

        public float angle;

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

        public rotateAllSpikesWithID delegateRotateAllSpikesWithID;

        private SoundEffectInstance sndElectric;

        private const int RotatableSpikeFirstQuad = 0;
        private const int ButtonFirstQuad = 4;
        private const int StaticSpikeFirstQuad = 8;

        private const int ElectrodesWidthIndex = 5;

        private const int ButtonFramesPerToggle = 2;

        private const int ButtonPressedQuadOffset = 1;

        private static (string texture, int quad) GetSpikeTextureAndQuad(int width, bool rotatable)
        {
            if (width == ElectrodesWidthIndex)
            {
                return (Resources.Img.ObjElectrodes, 0);
            }
            int index = width - 1;
            if (index is < 0 or >= 4)
            {
                return (null, 0);
            }
            return rotatable
                ? (Resources.Img.ObjSpikes, RotatableSpikeFirstQuad + index)
                : (Resources.Img.ObjSpikes, StaticSpikeFirstQuad + index);
        }

        private enum SPIKES_ANIM
        {
            ELECTRODES_BASE,
            ELECTRODES_ELECTRIC,
            ROTATION_ADJUSTED
        }

        private enum SPIKES_ROTATION
        {
            BUTTON
        }

        // (Invoke) Token: 0x06000689 RID: 1673
        public delegate void rotateAllSpikesWithID(int sid);
    }
}
