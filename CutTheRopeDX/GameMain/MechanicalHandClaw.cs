using System.Collections.Generic;

using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Visual claw node attached to the end of a mechanical hand segment chain.
    /// Draws idle/active body and active fingers in separate passes.
    /// </summary>
    internal sealed class MechanicalHandClaw : BaseElement
    {
        /// <summary>
        /// Initializes claw sprites for idle, active body, and active fingers.
        /// </summary>
        public MechanicalHandClaw()
        {
            clawIdle = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 5);
            clawActive = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 6);
            clawActiveFingers = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 7);

            clawIdle.anchor = 18;
            clawActive.anchor = 18;
            clawActiveFingers.anchor = 18;

            clawIdle.DoRestoreCutTransparency();
            clawActive.DoRestoreCutTransparency();
            clawActiveFingers.DoRestoreCutTransparency();

            clawIdle.AddTimelinewithID(MechanicalHand.CatchBounceTimelineWithInitialScaleandAmplitude(clawIdle.scaleX, 0.25f), 0);
            clawIdle.AddTimelinewithID(MechanicalHand.CatchBounceTimelineWithInitialScaleandAmplitude(clawIdle.scaleX, 0.1f), 1);
            clawActive.AddTimelinewithID(MechanicalHand.CatchBounceTimelineWithInitialScaleandAmplitude(clawActive.scaleX, 0.1f), 1);
            clawActiveFingers.AddTimelinewithID(MechanicalHand.CatchBounceTimelineWithInitialScaleandAmplitude(clawActiveFingers.scaleX, 0.25f), 1);
        }

        /// <summary>
        /// Resolves the owning mechanical hand by walking up the segment chain.
        /// </summary>
        /// <returns>Owning hand instance, or <see langword="null" /> when detached.</returns>
        public MechanicalHand TheHand()
        {
            BaseElement element = parent;
            for (int i = 0; i <= prevSegments && element != null; i++)
            {
                element = element.parent;
            }
            return element as MechanicalHand;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            EnsureHandReference();
            if (mechanicalHand?.state == MechanicalHand.STATE_HAND_CANDY)
            {
                clawActive.Draw();
            }
            else
            {
                clawIdle.Draw();
            }
            PostDraw();
        }

        /// <summary>
        /// Draws the active fingers overlay pass with inherited segment transforms.
        /// </summary>
        public void DrawFingers()
        {
            EnsureHandReference();
            List<MechanicalHandSegment> handSegments = mechanicalHand?.segments;
            if (handSegments != null)
            {
                foreach (MechanicalHandSegment segment in handSegments)
                {
                    segment?.PreDraw();
                }
            }

            PreDraw();
            clawActiveFingers.Draw();
            PostDraw();

            if (handSegments == null)
            {
                return;
            }

            foreach (MechanicalHandSegment segment in handSegments)
            {
                if (segment == null)
                {
                    continue;
                }

                if (segment.passTransformationsToChilds)
                {
                    RestoreTransformations(segment);
                }
                if (segment.passColorToChilds)
                {
                    RestoreColor(segment);
                }
            }
        }

        /// <summary>
        /// Draws the active claw body pass for the currently grabbing hand.
        /// </summary>
        public void DrawActiveHand()
        {
            EnsureHandReference();
            List<MechanicalHandSegment> handSegments = mechanicalHand?.segments;
            if (handSegments != null)
            {
                foreach (MechanicalHandSegment segment in handSegments)
                {
                    segment?.PreDraw();
                }
            }

            PreDraw();
            if (mechanicalHand?.state == MechanicalHand.STATE_HAND_CANDY)
            {
                clawActive.Draw();
            }
            PostDraw();

            if (handSegments == null)
            {
                return;
            }

            foreach (MechanicalHandSegment segment in handSegments)
            {
                if (segment == null)
                {
                    continue;
                }

                if (segment.passTransformationsToChilds)
                {
                    RestoreTransformations(segment);
                }
                if (segment.passColorToChilds)
                {
                    RestoreColor(segment);
                }
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            clawActive.x = drawX;
            clawActive.y = drawY;
            clawActiveFingers.x = drawX;
            clawActiveFingers.y = drawY;
            clawIdle.x = drawX;
            clawIdle.y = drawY;

            clawActive.Update(delta);
            clawActiveFingers.Update(delta);
            clawIdle.Update(delta);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                clawActive = null;
                clawActiveFingers = null;
                clawIdle = null;
                mechanicalHand = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Caches the owning mechanical hand reference when it has not been resolved yet.
        /// </summary>
        private void EnsureHandReference()
        {
            mechanicalHand ??= TheHand();
        }

        /// <summary>Idle claw visual.</summary>
        public Image clawIdle;

        /// <summary>Active claw body visual.</summary>
        public Image clawActive;

        /// <summary>Active claw fingers overlay visual.</summary>
        public Image clawActiveFingers;

        /// <summary>Cached owning mechanical hand.</summary>
        private MechanicalHand mechanicalHand;

        /// <summary>Number of previous parent segments to walk when resolving the owning hand.</summary>
        public int prevSegments;
    }
}
