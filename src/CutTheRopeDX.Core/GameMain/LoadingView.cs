using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Loading screen view that draws the pack cover background and progress animation.
    /// </summary>
    internal sealed class LoadingView : View
    {
        /// <inheritdoc />
        public override void Show()
        {
            // Reset animation state when loading screen is shown
            initialized = false;
            currentPercent = 0f;
            animationComplete = false;
            base.Show();
        }

        /// <summary>
        /// Gets whether the smoothed loading animation has reached completion.
        /// </summary>
        /// <remarks>
        /// The progress animation is advanced in <see cref="Draw"/>, which headless runs never
        /// call. Without the device check the loading screen would never hand off to gameplay,
        /// because <c>animationComplete</c> could never become <see langword="true"/>.
        /// </remarks>
        /// <returns><see langword="true"/> when the progress animation is complete; otherwise, <see langword="false"/>.</returns>
        public bool IsAnimationComplete()
        {
            return animationComplete || !Renderer.IsAvailable;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PlatformServices.Cursor?.Enable(true);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            PreDraw();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            string boxCover = PackConfig.GetBoxCoverOrDefault(cTRRootController.GetPack());

            // Smooth interpolation for loading percentage
            float targetPercent = Application.SharedResourceMgr().GetPercentLoaded();

            // Initialize on first draw
            if (!initialized)
            {
                currentPercent = targetPercent;
                initialized = true;
            }

            if (currentPercent < targetPercent)
            {
                currentPercent += (targetPercent - currentPercent) * LoadProgressLerp;
                if (targetPercent - currentPercent < 0.5f)
                {
                    currentPercent = targetPercent; // Snap when close enough
                }
            }

            // The lerp exists only to soften the step between individual resource loads; it must
            // never outlive loading itself. Before this snap the bar trailed real progress by ~23
            // frames, so a 21-resource load showed a 43-frame screen — over half of it after every
            // resource was already in.
            if (targetPercent >= 100f)
            {
                currentPercent = 100f;
            }

            // Mark animation as complete when we've reached 100%
            if (currentPercent >= 99.5f && !animationComplete)
            {
                animationComplete = true;
            }

            float progressPercent = currentPercent;
            CTRTexture2D texture = Application.GetTexture(boxCover);
            Renderer.SetColor(s_Color1);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float leftQuadX = (SCREEN_WIDTH / 2f) - quadSize.X;
            DrawHelper.DrawImageQuad(texture, 0, leftQuadX, 0f);
            Renderer.PushMatrix();
            float mirrorPivotX = (SCREEN_WIDTH / 2f) + (quadSize.X / 2f);
            Renderer.Translate(mirrorPivotX, SCREEN_HEIGHT / 2f, 0f);
            Renderer.Rotate(180f, 0f, 0f, 1f);
            Renderer.Translate(-mirrorPivotX, -SCREEN_HEIGHT / 2f, 0f);
            DrawHelper.DrawImageQuad(texture, 0, SCREEN_WIDTH / 2f, 0.5f);
            Renderer.PopMatrix();
            CTRTexture2D texture2 = Application.GetTexture(Resources.Img.MenuLevelUi);
            if (!game)
            {
                Renderer.Enable(Renderer.GL_SCISSOR_TEST);
                Renderer.SetScissor(0f, 0f, SCREEN_WIDTH, 1200f * progressPercent / 100f);
            }
            Renderer.SetColor(Color.White);
            leftQuadX = Image.GetQuadOffset(Resources.Img.MenuLevelUi, 6).X;
            DrawHelper.DrawImageQuad(texture2, 6, leftQuadX, 80f);
            leftQuadX = Image.GetQuadOffset(Resources.Img.MenuLevelUi, 7).X;
            DrawHelper.DrawImageQuad(texture2, 7, leftQuadX, 80f);
            if (!game)
            {
                Renderer.Disable(Renderer.GL_SCISSOR_TEST);
            }
            if (game)
            {
                Vector quadOffset = Image.GetQuadOffset(Resources.Img.MenuLevelUi, 9);
                float rocketLiftOffset = 1250f * progressPercent / 100f;
                DrawHelper.DrawImageQuad(texture2, 9, quadOffset.X, 700f - rocketLiftOffset);
            }
            else
            {
                float loadingBarOffset = 1120f * progressPercent / 100f;
                DrawHelper.DrawImageQuad(texture2, 8, 1084f, loadingBarOffset - 100f);
            }
            PostDraw();
            Renderer.SetColor(Color.White);
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
        }

        /// <summary>
        /// Fraction of the remaining gap the progress bar closes each frame. Fast enough that the
        /// bar stays close to real progress; the snap at 100 is what guarantees it cannot lag past
        /// completion.
        /// </summary>
        private const float LoadProgressLerp = 0.5f;

        /// <summary>Whether the view is loading into gameplay instead of the menu flow.</summary>
        public bool game;

        /// <summary>Smoothed loading percentage currently displayed by the progress animation.</summary>
        private float currentPercent;

        /// <summary>Whether the progress animation has been initialized from the resource manager.</summary>
        private bool initialized;

        /// <summary>Whether the progress animation has reached its completion threshold.</summary>
        private bool animationComplete;

        /// <summary>Tint used for the mirrored pack-cover background.</summary>
        private static Color s_Color1 = new(0.85f, 0.85f, 0.85f, 1f);
    }
}
