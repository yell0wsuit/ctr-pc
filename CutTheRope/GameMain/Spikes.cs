using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.GameMain
{
    internal sealed class Spikes : CTRGameObject, ITimelineDelegate, IButtonDelegation
    {
        public Spikes InitWithPosXYWidthAndAngleToggled(float px, float py, int w, double an, int t)
        {
            string textureResourceName = GetSpikeTexture(w, t != -1);
            if (textureResourceName == null || InitWithTexture(Application.GetTexture(textureResourceName)) == null)
            {
                return null;
            }
            if (t > 0)
            {
                DoRestoreCutTransparency();
                int num = (t - 1) * ButtonFramesPerToggle;
                int q = ButtonPressedQuadOffset + ((t - 1) * ButtonFramesPerToggle);
                Image image = Image_createWithResIDQuad(Resources.Img.ObjRotatableSpikesButton, num);
                Image image2 = Image_createWithResIDQuad(Resources.Img.ObjRotatableSpikesButton, q);
                image.DoRestoreCutTransparency();
                image2.DoRestoreCutTransparency();
                rotateButton = new Button().InitWithUpElementDownElementandID(image, image2, SpikesButtonId.Rotate);
                rotateButton.delegateButtonDelegate = this;
                rotateButton.anchor = rotateButton.parentAnchor = 18;
                _ = AddChild(rotateButton);
                Vector quadOffset = GetQuadOffset(Resources.Img.ObjRotatableSpikesButton, num);
                Vector quadSize = GetQuadSize(Resources.Img.ObjRotatableSpikesButton, num);
                Vector vector = VectSub(Vect(image.texture.preCutSize.x, image.texture.preCutSize.y), VectAdd(quadSize, quadOffset));
                rotateButton.SetTouchIncreaseLeftRightTopBottom(0f - quadOffset.x + (quadSize.x / 2f), 0f - vector.x + (quadSize.x / 2f), 0f - quadOffset.y + (quadSize.y / 2f), 0f - vector.y + (quadSize.y / 2f));
            }
            passColorToChilds = false;
            spikesNormal = false;
            origRotation = rotation = (float)an;
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
            float num = !electro ? texture.quadRects[quadToDraw].w : width - RTPD(400.0);
            num /= 2f;
            t1.x = x - num;
            t2.x = x + num;
            t1.y = t2.y = y - 5f;
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = y + 5f;
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
            float num = spikesNormal ? 90 : 0;
            float num2 = origRotation + num;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)rotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)num2, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, Math.Abs(num2 - rotation) / 90f * 0.3f));
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
                if (electroTimer == 0.0)
                {
                    TurnElectroOff();
                    return;
                }
            }
            else
            {
                _ = Mover.MoveVariableToTarget(ref electroTimer, 0f, 1f, delta);
                if (electroTimer == 0.0)
                {
                    TurnElectroOn();
                }
            }
        }

        public static void TimelineReachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
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

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

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

        public rotateAllSpikesWithID delegateRotateAllSpikesWithID;

        private SoundEffectInstance sndElectric;

        private static readonly string[] SpikeTextures =
        [
            Resources.Img.ObjSpikes01,
            Resources.Img.ObjSpikes02,
            Resources.Img.ObjSpikes03,
            Resources.Img.ObjSpikes04,
            Resources.Img.ObjElectrodes
        ];

        private static readonly string[] RotatableSpikeTextures =
        [
            Resources.Img.ObjRotatableSpikes01,
            Resources.Img.ObjRotatableSpikes02,
            Resources.Img.ObjRotatableSpikes03,
            Resources.Img.ObjRotatableSpikes04
        ];


        private const int ElectrodesWidthIndex = 5;

        private const int ButtonFramesPerToggle = 2;

        private const int ButtonPressedQuadOffset = 1;

        private static string GetSpikeTexture(int width, bool rotatable)
        {
            int index = width - 1;
            return index < 0
                ? null
                : rotatable
                ? index < RotatableSpikeTextures.Length ? RotatableSpikeTextures[index] : null
                : width == ElectrodesWidthIndex
                ? SpikeTextures.Length > 4 ? SpikeTextures[4] : null
                : index < SpikeTextures.Length ? SpikeTextures[index] : null;
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
