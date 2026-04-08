using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Audio;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Spike hazard that can be static, rotatable by a button group, or electrified with timed on/off cycles.
    /// </summary>
    internal sealed class Spikes : CTRGameObject, ITimelineDelegate, IButtonDelegation
    {
        /// <summary>
        /// Initializes spikes at a level position with the configured width, angle, and toggle group.
        /// </summary>
        /// <param name="px">World-space X position.</param>
        /// <param name="py">World-space Y position.</param>
        /// <param name="w">Spike width/type index.</param>
        /// <param name="an">Initial rotation angle in degrees.</param>
        /// <param name="t">Toggle group id, or -1 for non-rotatable spikes.</param>
        /// <returns>The initialized spikes, or <see langword="null"/> if the type or texture is invalid.</returns>
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

        /// <summary>
        /// Recomputes rotated collision edge points from the current position and rotation.
        /// </summary>
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

        /// <summary>
        /// Turns electrified spikes off and stops the looping electric sound.
        /// </summary>
        public void TurnElectroOff()
        {
            electroOn = false;
            PlayTimeline(0);
            electroTimer = offTime;
            sndElectric?.Stop();
            sndElectric = null;
        }

        /// <summary>
        /// Turns electrified spikes on and starts the looping electric sound.
        /// </summary>
        public void TurnElectroOn()
        {
            electroOn = true;
            PlayTimeline(1);
            electroTimer = onTime;
            sndElectric = CTRSoundMgr.PlaySoundLooped(Resources.Snd.Electric);
        }

        /// <summary>
        /// Toggles rotatable spikes between their original and perpendicular rotations.
        /// </summary>
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

        /// <summary>
        /// Sets the toggle group for this rotatable spike set.
        /// </summary>
        /// <param name="t">Toggle group id.</param>
        public void SetToggled(int t)
        {
            toggled = t;
        }

        /// <summary>
        /// Gets the toggle group for this rotatable spike set.
        /// </summary>
        /// <returns>The toggle group id.</returns>
        public int GetToggled()
        {
            return toggled;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            updateRotationFlag = false;
        }

        /// <summary>
        /// Handles a spike button press.
        /// </summary>
        /// <param name="n">Pressed spike button identifier.</param>
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

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(SpikesButtonId.FromButtonId(buttonId));
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline _, KeyFrame _1, int _2)
        {
        }

        /// <summary>Toggle group id for rotating linked spike sets.</summary>
        private int toggled;

        /// <summary>Current spike angle in radians.</summary>
        public float angle;

        /// <summary>Top-left rotated collision point.</summary>
        public Vector t1;

        /// <summary>Top-right rotated collision point.</summary>
        public Vector t2;

        /// <summary>Bottom-left rotated collision point.</summary>
        public Vector b1;

        /// <summary>Bottom-right rotated collision point.</summary>
        public Vector b2;

        /// <summary>Whether these spikes use the electrified variant.</summary>
        public bool electro;

        /// <summary>Initial delay before the first electrified spike state change.</summary>
        public float initialDelay;

        /// <summary>Duration electrified spikes remain on.</summary>
        public float onTime;

        /// <summary>Duration electrified spikes remain off.</summary>
        public float offTime;

        /// <summary>Whether electrified spikes are currently on.</summary>
        public bool electroOn;

        /// <summary>Timer for the current electrified spike on/off phase.</summary>
        public float electroTimer;

        /// <summary>Whether rotated collision points need to be refreshed during rotation animation.</summary>
        private bool updateRotationFlag;

        /// <summary>Whether rotatable spikes are in the perpendicular orientation.</summary>
        private bool spikesNormal;

        /// <summary>Original rotation in degrees.</summary>
        private float origRotation;

        /// <summary>Button used to rotate linked spike sets.</summary>
        public Button rotateButton;

        /// <summary>Active touch index for spike interaction, or -1 when idle.</summary>
        public int touchIndex;

        /// <summary>Delegate invoked to rotate all spikes in the same toggle group.</summary>
        public rotateAllSpikesWithID delegateRotateAllSpikesWithID;

        /// <summary>Looping electric sound instance while electrified spikes are on.</summary>
        private SoundEffectInstance sndElectric;

        /// <summary>First texture quad index for rotatable spike variants.</summary>
        private const int RotatableSpikeFirstQuad = 0;

        /// <summary>First texture quad index for rotate button variants.</summary>
        private const int ButtonFirstQuad = 4;

        /// <summary>First texture quad index for static spike variants.</summary>
        private const int StaticSpikeFirstQuad = 8;

        /// <summary>Width/type index used by the electrified spike variant.</summary>
        private const int ElectrodesWidthIndex = 5;

        /// <summary>Number of button frames per toggle group.</summary>
        private const int ButtonFramesPerToggle = 2;

        /// <summary>Offset from a button up quad to its pressed quad.</summary>
        private const int ButtonPressedQuadOffset = 1;

        /// <summary>
        /// Resolves the texture and quad for a spike width/type.
        /// </summary>
        /// <param name="width">Spike width/type index.</param>
        /// <param name="rotatable">Whether the spike uses rotatable visuals.</param>
        /// <returns>The texture resource name and quad index, or <see langword="null"/> texture when invalid.</returns>
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

        /// <summary>
        /// Electrode animation timeline identifiers.
        /// </summary>
        private enum SPIKES_ANIM
        {
            /// <summary>Base electrodes timeline.</summary>
            ELECTRODES_BASE,

            /// <summary>Electric electrodes timeline.</summary>
            ELECTRODES_ELECTRIC,

            /// <summary>Rotation adjustment timeline.</summary>
            ROTATION_ADJUSTED
        }

        /// <summary>
        /// Spike rotation button identifiers.
        /// </summary>
        private enum SPIKES_ROTATION
        {
            /// <summary>Rotate button identifier.</summary>
            BUTTON
        }

        /// <summary>
        /// Delegate used to rotate all spikes in a toggle group.
        /// </summary>
        /// <param name="sid">Toggle group id to rotate.</param>
        public delegate void rotateAllSpikesWithID(int sid);
    }
}
