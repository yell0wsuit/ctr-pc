using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Time Travel axe blade. It is a candy-like physics body for ropes, transport, and rockets,
    /// but its gameplay role is a hazard and chain cutter.
    /// </summary>
    internal sealed class Axe : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        private const int BaseQuad = 0;
        private const int BladeQuad = 1;
        private const int PivotQuad = 2;

        public readonly ConstraintedPoint constraint;

        public readonly string axeNumber;

        private readonly GameObject baseSprite;

        private readonly GameObject bladeSprite;

        private readonly GameObject pivotSprite;

        /// <summary>Generic bubble overlay shown while this axe is carried by a bubble.</summary>
        public readonly Animation bubbleAnimation;

        /// <summary>Generic ghost-bubble overlay shown while this axe is carried by a ghost bubble.</summary>
        public readonly CandyInGhostBubbleAnimation ghostBubbleAnimation;

        public Axe(ConstraintedPoint constraint, string axeNumber)
        {
            this.constraint = constraint;
            this.axeNumber = axeNumber ?? string.Empty;

            baseSprite = GameObject_createWithResIDQuad(Resources.Img.ObjAxe, BaseQuad);
            baseSprite.anchor = baseSprite.parentAnchor = 18;
            baseSprite.blendingMode = 1;
            _ = AddChild(baseSprite);

            bladeSprite = GameObject_createWithResIDQuad(Resources.Img.ObjAxe, BladeQuad);
            bladeSprite.anchor = bladeSprite.parentAnchor = 18;
            bladeSprite.blendingMode = 1;
            _ = AddChild(bladeSprite);

            pivotSprite = GameObject_createWithResIDQuad(Resources.Img.ObjAxe, PivotQuad);
            pivotSprite.anchor = pivotSprite.parentAnchor = 18;
            pivotSprite.blendingMode = 1;
            _ = AddChild(pivotSprite);

            // Create bubble capture animation (shown when inside a normal bubble)
            bubbleAnimation = BubbleAnimationFactory.CreateBubble();
            _ = AddChild(bubbleAnimation);

            // Create ghost bubble animation (shown when inside a ghost bubble)
            ghostBubbleAnimation = BubbleAnimationFactory.CreateGhostBubble();
            _ = AddChild(ghostBubbleAnimation);

            width = baseSprite.width;
            height = baseSprite.height;
            anchor = parentAnchor = 18;
            bb = new CTRRectangle(0f, 0f, width, height);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
            rotatedBB = false;
            topLeftCalculated = false;

            SyncToConstraint();
        }

        public void SyncToConstraint()
        {
            x = constraint.pos.X;
            y = constraint.pos.Y;
            CalculateTopLeft(this);
        }

        public void SyncFromContext(CandyContext ctx)
        {
            visible = !ctx.HasNoWholeBodyInPlay && ctx.Lifecycle.Transport?.Sock == null;
            SyncToConstraint();
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            bladeSprite.rotation -= AxeSpin.RotationStepForVelocity(constraint.v);

            SyncToConstraint();
        }

        public override void Draw()
        {
            if (!visible)
            {
                return;
            }

            PreDraw();
            baseSprite.Draw();
            bladeSprite.Draw();
            pivotSprite.Draw();

            // Draw bubble animation if currently captured by it
            if (bubbleAnimation.visible)
            {
                bubbleAnimation.Draw();
            }
            if (ghostBubbleAnimation.visible)
            {
                ghostBubbleAnimation.Draw();
            }

            RestoreTransformations(this);
        }

        public float PositionOnTransporter { get; set; }

        public Vector BindPoint => Vect(x, y);

        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
            constraint.pos = point;
        }

        public float CollisionRadius => AxeDefinition.HazardCollisionDistance;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }
    }
}
