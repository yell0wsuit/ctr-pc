using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Time Travel bomb. Like the axe it is a candy-like physics body for ropes, bubbles, transport,
    /// and rockets, but its gameplay role is an explosive: anything that touches it, cuts across it,
    /// or detonates beside it sets it off, and the blast shoves every nearby body away.
    /// </summary>
    internal sealed class Bomb : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        /// <summary>Quad holding the intact bomb; the remaining quads are its debris fragments.</summary>
        private const int BodyQuad = 0;

        public readonly ConstraintedPoint constraint;

        public readonly string bombNumber;

        private readonly GameObject bodySprite;

        /// <summary>Generic bubble overlay shown while this bomb is carried by a bubble.</summary>
        public readonly Animation bubbleAnimation;

        /// <summary>Generic ghost-bubble overlay shown while this bomb is carried by a ghost bubble.</summary>
        public readonly CandyInGhostBubbleAnimation ghostBubbleAnimation;

        /// <summary>
        /// True once this bomb has detonated. A detonated bomb no longer triggers, is no longer
        /// pushed by other blasts, and is waiting on its delayed debris burst to be removed.
        /// </summary>
        public bool Exploded { get; set; }

        public Bomb(ConstraintedPoint constraint, string bombNumber)
        {
            this.constraint = constraint;
            this.bombNumber = bombNumber ?? string.Empty;

            bodySprite = GameObject_createWithResIDQuad(Resources.Img.ObjBomb, BodyQuad);
            bodySprite.anchor = bodySprite.parentAnchor = 18;
            bodySprite.blendingMode = 1;
            _ = AddChild(bodySprite);

            bubbleAnimation = BubbleAnimationFactory.CreateBubble();
            _ = AddChild(bubbleAnimation);

            ghostBubbleAnimation = BubbleAnimationFactory.CreateGhostBubble();
            _ = AddChild(ghostBubbleAnimation);

            width = bodySprite.width;
            height = bodySprite.height;
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

        /// <inheritdoc />
        /// <remarks>The original's <c>Bomb::draw</c> only writes the body position, so unlike the
        /// axe there is nothing to spin and the frozen and running paths are identical.</remarks>
        public override void Update(float delta)
        {
            base.Update(delta);
            SyncToConstraint();
        }

        public override void Draw()
        {
            if (!visible)
            {
                return;
            }

            PreDraw();
            bodySprite.Draw();

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

        public float CollisionRadius => BombDefinition.ContactTriggerDistance;

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
