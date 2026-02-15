using System.Collections.Generic;

using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class MechanicalHandClaw : BaseElement
    {
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
        }

        public MechanicalHand TheHand()
        {
            BaseElement element = parent;
            for (int i = 0; i <= prevSegments && element != null; i++)
            {
                element = element.parent;
            }
            return element as MechanicalHand;
        }

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

        private void EnsureHandReference()
        {
            mechanicalHand ??= TheHand();
        }

        public Image clawIdle;

        public Image clawActive;

        public Image clawActiveFingers;

        private MechanicalHand mechanicalHand;

        public int prevSegments;
    }
}
