using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Single articulated segment of a mechanical hand.
    /// Owns segment visuals, optional rotate button, and rotation timeline behavior.
    /// </summary>
    internal sealed class MechanicalHandSegment : BaseElement, ITimelineDelegate
    {
        /// <summary>
        /// Initializes a segment with placement, geometry, and rotation capability.
        /// </summary>
        /// <param name="pos">Local segment origin.</param>
        /// <param name="segmentLength">Segment length in world units.</param>
        /// <param name="angleDegrees">Initial segment angle in degrees.</param>
        /// <param name="isRotatable">Whether the segment exposes a rotate button.</param>
        /// <returns>The initialized segment instance.</returns>
        public MechanicalHandSegment InitWithPositionLengthAngleRotatable(Vector pos, float segmentLength, float angleDegrees, bool isRotatable)
        {
            x = pos.X;
            y = pos.Y;
            rotation = angleDegrees;
            prevRotation = angleDegrees;
            length = segmentLength;
            endPosition = Vect(length, 0f);
            rotatable = isRotatable;
            endsWithHand = true;
            passTransformationsToChilds = true;

            MechanicalHandClaw claw = new()
            {
                anchor = 18,
                parentAnchor = 18,
                x = endPosition.X,
                y = endPosition.Y
            };
            _ = AddChild(claw);

            _base = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 4);
            buttonNone = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 2);
            if (rotatable)
            {
                Image buttonUp = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 1);
                Image buttonDown = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 0);
                button = (MechanicalHandButton)new MechanicalHandButton().InitWithUpElementDownElementandID(buttonUp, buttonDown, 0);
                button.anchor = 18;
                button.segment = this;
            }
            else
            {
                button = null;
            }

            _base.anchor = buttonNone.anchor = 18;

            armImage = TiledImage.TiledImage_createWithResID(Resources.Img.ObjRoboHand);
            armImage.SetTile(3);
            armImage.width = (int)length;
            armImage.height = (int)Image.GetQuadSize(Resources.Img.ObjRoboHand, 3).Y;
            armImage.anchor = 17;
            return this;
        }

        /// <summary>
        /// Starts a 90-degree rotation animation when the segment can rotate.
        /// Queues one extra rotation if tapped again during cooldown.
        /// </summary>
        public void Rotate()
        {
            if (canRotateOnceAgain)
            {
                rotateOnceAgain = true;
            }

            if (!rotatable || GetCurrentTimeline() != null)
            {
                return;
            }

            canRotateOnceAgain = false;
            rotateOnceAgain = false;
            rotationTime = 0f;

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)rotation, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation((int)(rotation + DEG_90), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25f));
            timeline.delegateTimelineDelegate = this;
            _ = AddTimeline(timeline);
            PlayTimeline(0);
            targetRotation = rotation + DEG_90;
        }

        /// <summary>
        /// Gets the per-frame rotation delta used to rotate attached candy visuals.
        /// </summary>
        /// <returns>Rotation delta in degrees.</returns>
        public float RotationDelta()
        {
            return rotation - prevRotation;
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            RemoveTimeline(0);
            if (rotateOnceAgain)
            {
                Rotate();
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (drawBase)
            {
                _base.Draw();
            }

            PreDraw();
            armImage.Draw();
            RestoreTransformations(this);

            if (rotatable)
            {
                button?.Draw();
            }
            else
            {
                buttonNone.Draw();
            }

            bool hasScale = scaleX != 1f || scaleY != 1f;
            bool hasRotation = rotation != 0f;
            if (hasScale || hasRotation)
            {
                Renderer.PushMatrix();

                {
                    float centerX = drawX + (width >> 1) + rotationCenterX;
                    float centerY = drawY + (height >> 1) + rotationCenterY;
                    Renderer.Translate(centerX, centerY, 0f);
                    if (hasRotation)
                    {
                        Renderer.Rotate(rotation, 0f, 0f, 1f);
                    }
                    if (hasScale)
                    {
                        Renderer.Scale(scaleX, scaleY, 1f);
                    }
                    Renderer.Translate(-centerX, -centerY, 0f);
                }

            }

            PostDraw();
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            prevRotation = rotation;
            base.Update(delta);
            rotationTime += delta;
            if (rotationTime > ROTATE_ONCE_AGAIN_TIME)
            {
                canRotateOnceAgain = true;
            }

            _base.x = buttonNone.x = drawX;
            _base.y = buttonNone.y = drawY;
            if (rotatable)
            {
                button.x = drawX;
                button.y = drawY;
            }
            armImage.x = drawX;
            armImage.y = drawY;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _base = null;
                buttonNone = null;
                button = null;
                armImage = null;
                theHand = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>Seconds after a rotation starts when one queued extra rotation may be accepted.</summary>
        private const float ROTATE_ONCE_AGAIN_TIME = 0.125f;

        /// <summary>Base joint visual for the first segment.</summary>
        private Image _base;

        /// <summary>Non-rotatable joint button visual.</summary>
        private Image buttonNone;

        /// <summary>Tiled arm visual stretched to the segment length.</summary>
        private TiledImage armImage;

        /// <summary>Segment length in world units.</summary>
        private float length;

        /// <summary>Rotation from the previous update.</summary>
        private float prevRotation;

        /// <summary>Local end position of this segment relative to its origin.</summary>
        public Vector endPosition;

        /// <summary>Whether this segment exposes a rotate button.</summary>
        private bool rotatable;

        /// <summary>Whether this segment currently owns the claw child at its end.</summary>
        public bool endsWithHand;

        /// <summary>Whether this segment should draw the base joint visual.</summary>
        public bool drawBase;

        /// <summary>Whether one additional rotation can be queued during the current rotation.</summary>
        private bool canRotateOnceAgain;

        /// <summary>Whether another 90-degree rotation should start after the current one finishes.</summary>
        private bool rotateOnceAgain;

        /// <summary>Elapsed time since the current rotation animation started.</summary>
        private float rotationTime;

        /// <summary>Target rotation in degrees for the current rotation animation.</summary>
        public float targetRotation;

        /// <summary>Rotate button attached to this segment, if the segment is rotatable.</summary>
        public MechanicalHandButton button;

        /// <summary>Mechanical hand that owns this segment.</summary>
        public MechanicalHand theHand;
    }
}
