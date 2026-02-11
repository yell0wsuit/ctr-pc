using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a light bulb game object that can illuminate the stars
    /// and interact with the candy through ropes, bubbles, and socks.
    /// </summary>
    /// <remarks>
    /// The light bulb consists of multiple visual layers:
    /// <list type="bullet">
    ///   <item><description>Light glow - An additive-blended halo effect</description></item>
    ///   <item><description>Bottle - The glass jar container</description></item>
    ///   <item><description>Top - The lid of the jar</description></item>
    ///   <item><description>Firefly - An animated firefly inside the jar</description></item>
    /// </list>
    /// The light bulb can be attached to ropes via its constraint point and can
    /// capture or be captured by bubbles.
    /// </remarks>
    internal sealed class LightBulb : CTRGameObject
    {
        /// <summary>Sprite index for the light glow effect.</summary>
        private const int ImgObjLighterLight = 0;

        /// <summary>Sprite index for the glass jar.</summary>
        private const int ImgObjLighterBottle = 1;

        /// <summary>Sprite index for the bottle lid.</summary>
        private const int ImgObjLighterTop = 2;

        /// <summary>First frame index of the firefly animation sequence.</summary>
        private const int ImgObjLighterFireflyStart = 3;

        /// <summary>Last frame index of the firefly animation sequence.</summary>
        private const int ImgObjLighterFireflyEnd = 42;


        /// <summary>Base scale factor applied to the light bulb root object.</summary>
        private const float LightBulbRootScale = 1f;

        /// <summary>World-to-screen coordinate scale multiplier.</summary>
        private const float WorldScale = 2f;

        /// <summary>
        /// The radius of the light effect emitted by the bulb, used for
        /// illuminating stars.
        /// </summary>
        public readonly float lightRadius;

        /// <summary>
        /// Current rotational velocity of the light bulb in degrees per frame.
        /// Decays over time due to friction (0.98 multiplier).
        /// </summary>
        public float rotationVelocity;

        /// <summary>
        /// The physics constraint point that determines the bulb's position.
        /// Used for rope attachment and physics simulation.
        /// </summary>
        public readonly ConstraintedPoint constraint;

        /// <summary>
        /// Identifier string for this light bulb instance, used for level loading
        /// and object referencing.
        /// </summary>
        public readonly string bulbNumber;

        /// <summary>
        /// Reference to a hat/sock that has captured this light bulb.
        /// When not null, the light bulb becomes invisible.
        /// </summary>
        public Sock attachedSock;

        /// <summary>
        /// Reference to a bubble that is currently capturing or carrying the light bulb.
        /// </summary>
        public Bubble capturingBubble;

        /// <summary>
        /// Indicates whether the capturing bubble is a ghost bubble variant,
        /// which uses a different visual animation.
        /// </summary>
        public bool capturingGhostBubble;

        /// <summary>
        /// Speed at which the attached hat/sock moves, used for physics calculations.
        /// </summary>
        public float sockSpeed;


        /// <summary>The additive-blended light glow halo effect.</summary>
        private readonly GameObject lightGlow;

        /// <summary>Animated firefly sprite inside the jar.</summary>
        private readonly Animation firefly;

        /// <summary>The glass jar sprite.</summary>
        private readonly GameObject bottle;

        /// <summary>The bottle cap sprite.</summary>
        private readonly GameObject top;

        /// <summary>Animation displayed when captured by a normal bubble.</summary>
        private readonly Animation bubbleAnimation;

        /// <summary>Animation displayed when captured by a ghost bubble.</summary>
        private readonly CandyInGhostBubbleAnimation ghostBubbleAnimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightBulb"/> class.
        /// </summary>
        /// <param name="lightRadius">The radius of the light effect for gameplay mechanics.</param>
        /// <param name="constraint">The physics constraint point for positioning.</param>
        /// <param name="bulbNumber">An optional identifier for this light bulb instance.</param>
        public LightBulb(float lightRadius, ConstraintedPoint constraint, string bulbNumber)
        {
            // Initialize state
            this.lightRadius = lightRadius;
            rotationVelocity = 0f;
            this.constraint = constraint;
            this.bulbNumber = bulbNumber ?? string.Empty;
            attachedSock = null;
            capturingBubble = null;
            capturingGhostBubble = false;
            sockSpeed = 0f;
            scaleX = scaleY = LightBulbRootScale;

            // Create light glow with additive blending for a soft halo effect
            lightGlow = GameObject_createWithResIDQuad(Resources.Img.ObjLighter, ImgObjLighterLight);
            lightGlow.anchor = lightGlow.parentAnchor = 18; // Center anchor
            lightGlow.color = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.6f); // Semi-transparent white
            lightGlow.blendingMode = 2; // Additive blending (SRC_ALPHA, ONE)
            _ = AddChild(lightGlow);

            // Create bottle sprite with normal alpha blending
            bottle = GameObject_createWithResIDQuad(Resources.Img.ObjLighter, ImgObjLighterBottle);
            bottle.anchor = bottle.parentAnchor = 18;
            bottle.DoRestoreCutTransparency();
            bottle.blendingMode = 1; // Normal blending (SRC_ALPHA, ONE_MINUS_SRC_ALPHA)
            _ = AddChild(bottle);

            // Create bottle top/lid sprite
            top = GameObject_createWithResIDQuad(Resources.Img.ObjLighter, ImgObjLighterTop);
            top.anchor = top.parentAnchor = 18;
            top.DoRestoreCutTransparency();
            top.blendingMode = 1; // Normal blending
            _ = AddChild(top);

            // Create animated firefly that loops through frames 3-42
            firefly = Animation_createWithResID(Resources.Img.ObjLighter);
            firefly.anchor = firefly.parentAnchor = 18;
            firefly.blendingMode = 1; // Normal blending
            _ = firefly.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, ImgObjLighterFireflyStart, ImgObjLighterFireflyEnd);
            firefly.PlayTimeline(0);
            _ = AddChild(firefly);

            // Create bubble capture animation (shown when inside a normal bubble)
            bubbleAnimation = Animation_createWithResID(Resources.Img.ObjBubbleFlight);
            bubbleAnimation.anchor = bubbleAnimation.parentAnchor = 18;
            _ = bubbleAnimation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
            bubbleAnimation.PlayTimeline(0);
            bubbleAnimation.visible = false; // Hidden until captured
            _ = AddChild(bubbleAnimation);

            // Create ghost bubble animation (shown when inside a ghost bubble)
            ghostBubbleAnimation = CandyInGhostBubbleAnimation.CIGBAnimation_createWithResID(Resources.Img.ObjBubbleFlight);
            ghostBubbleAnimation.anchor = ghostBubbleAnimation.parentAnchor = 18;
            ghostBubbleAnimation.visible = false; // Hidden until captured
            ghostBubbleAnimation.AddSupportingCloudsTimelines();
            _ = ghostBubbleAnimation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
            ghostBubbleAnimation.PlayTimeline(0);
            _ = AddChild(ghostBubbleAnimation);

            // Set bounding box based on bottle dimensions (the main visual element)
            CTRRectangle bottleRect = bottle.texture.quadRects[ImgObjLighterBottle];
            int boundWidth = (int)bottleRect.w;
            int boundHeight = (int)bottleRect.h;
            width = boundWidth;
            height = boundHeight;
            anchor = parentAnchor = 18;
            bb = new CTRRectangle(0f, 0f, width, height);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
            rotatedBB = false;
            topLeftCalculated = false;

            // Apply initial glow scale and sync position to constraint
            ApplyGlowScale();
            SyncToConstraint();
        }

        /// <summary>
        /// Calculates and applies the appropriate scale to the light glow effect
        /// based on the configured light radius.
        /// </summary>
        /// <remarks>
        /// The glow scale is computed to make the glow texture match the desired
        /// light radius in world coordinates, with an additional 1.5x multiplier
        /// for visual appeal.
        /// </remarks>
        public void ApplyGlowScale()
        {
            float width = lightGlow.width;
            if (width <= 0f)
            {
                return; // Avoid division by zero
            }
            // Scale formula: (radius * worldScale / textureWidth) * 1.5 / rootScale
            float scale = lightRadius * WorldScale / width * 1.5f / LightBulbRootScale;
            lightGlow.scaleX = scale;
            lightGlow.scaleY = scale;
        }

        /// <summary>
        /// Updates the light bulb's position to match its physics constraint point.
        /// </summary>
        /// <remarks>
        /// This should be called after physics simulation to keep the visual
        /// representation in sync with the physics state.
        /// </remarks>
        public void SyncToConstraint()
        {
            x = constraint.pos.X;
            y = constraint.pos.Y;
            CalculateTopLeft(this);
        }

        /// <summary>
        /// Draws all visual components of the light bulb.
        /// </summary>
        /// <remarks>
        /// This method draws in a specific order for proper layering:
        /// light glow first (background), then bottle, top, firefly, and bubble animations.
        /// For scenes with stars, use <see cref="DrawLight"/> and <see cref="DrawBottleAndFirefly"/>
        /// separately to render stars between the glow and bottle layers.
        /// </remarks>
        public override void Draw()
        {
            if (!visible)
            {
                return;
            }

            DrawLight();
            DrawBottleAndFirefly();
        }

        /// <summary>
        /// Draws only the light glow effect.
        /// </summary>
        /// <remarks>
        /// Use this method to render other elements (e.g. stars)
        /// between the glow layer and the bottle layer. Call <see cref="DrawBottleAndFirefly"/>
        /// afterward to complete the rendering.
        /// </remarks>
        public void DrawLight()
        {
            if (!visible)
            {
                return;
            }

            PreDraw(); // Apply transformations
            lightGlow.Draw();
            // Reset blend mode to premultiplied alpha after additive glow
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Draws the bottle, top lid, firefly, and any active bubble animations.
        /// </summary>
        /// <remarks>
        /// This method should be called after <see cref="DrawLight"/> when using
        /// split rendering, or it will be called automatically by <see cref="Draw"/>.
        /// </remarks>
        public void DrawBottleAndFirefly()
        {
            if (!visible)
            {
                return;
            }

            // Draw main light bulb visual components
            bottle.Draw();
            top.Draw();
            firefly.Draw();

            // Draw bubble animation if currently captured by it
            if (bubbleAnimation.visible)
            {
                bubbleAnimation.Draw();
            }
            if (ghostBubbleAnimation.visible)
            {
                ghostBubbleAnimation.Draw();
            }

            PostDrawNoChildren(); // Restore transformations
        }

        /// <summary>
        /// Restores transformation state without drawing child objects.
        /// </summary>
        /// <remarks>
        /// Used instead of <c>PostDraw()</c> since we manually control child drawing order.
        /// </remarks>
        private void PostDrawNoChildren()
        {
            RestoreTransformations(this);
        }

        /// <summary>
        /// Updates the light bulb's state each frame.
        /// </summary>
        /// <param name="delta">Time elapsed since the last update in seconds.</param>
        /// <remarks>
        /// Handles:
        /// <list type="bullet">
        ///   <item><description>Visibility based on hat/sock attachment</description></item>
        ///   <item><description>Bubble animation visibility based on capture state</description></item>
        ///   <item><description>Rotational velocity decay (friction)</description></item>
        /// </list>
        /// </remarks>
        public override void Update(float delta)
        {
            base.Update(delta);

            // Hide when attached to a hat/sock (hat/sock draws the captured object)
            visible = attachedSock == null;

            // Show appropriate bubble animation based on capture state
            bool hasBubble = capturingBubble != null && attachedSock == null;
            bubbleAnimation.visible = hasBubble && !capturingGhostBubble;
            ghostBubbleAnimation.visible = hasBubble && capturingGhostBubble;

            // Apply rotation with velocity decay (simulates friction/air resistance)
            if (rotationVelocity != 0f)
            {
                rotation += MathF.Min(5f, rotationVelocity); // Cap rotation speed at 5 degrees/frame
                rotationVelocity *= 0.98f; // Apply friction decay
            }
        }
    }
}
