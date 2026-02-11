using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class LoadingView : View
    {
        public override void Show()
        {
            // Reset animation state when loading screen is shown
            initialized = false;
            currentPercent = 0f;
            animationComplete = false;
            base.Show();
        }

        public bool IsAnimationComplete()
        {
            return animationComplete;
        }

        public override void Draw()
        {
            Global.MouseCursor.Enable(true);
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
                currentPercent += (targetPercent - currentPercent) * 0.16f; // Smooth lerp
                if (targetPercent - currentPercent < 0.5f)
                {
                    currentPercent = targetPercent; // Snap when close enough
                }
            }

            // Mark animation as complete when we've reached 100%
            if (currentPercent >= 99.5f && !animationComplete)
            {
                animationComplete = true;
            }

            float num2 = currentPercent;
            CTRTexture2D texture = Application.GetTexture(boxCover);
            Renderer.SetColor(s_Color1);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float num3 = (SCREEN_WIDTH / 2f) - quadSize.X;
            DrawHelper.DrawImageQuad(texture, 0, num3, 0f);
            Renderer.PushMatrix();
            float num4 = (SCREEN_WIDTH / 2f) + (quadSize.X / 2f);
            Renderer.Translate(num4, SCREEN_HEIGHT / 2f, 0f);
            Renderer.Rotate(180f, 0f, 0f, 1f);
            Renderer.Translate(-num4, -SCREEN_HEIGHT / 2f, 0f);
            DrawHelper.DrawImageQuad(texture, 0, SCREEN_WIDTH / 2f, 0.5f);
            Renderer.PopMatrix();
            CTRTexture2D texture2 = Application.GetTexture(Resources.Img.MenuLoading);
            if (!game)
            {
                Renderer.Enable(Renderer.GL_SCISSOR_TEST);
                Renderer.SetScissor(0f, 0f, SCREEN_WIDTH, 1200f * num2 / 100f);
            }
            Renderer.SetColor(Color.White);
            num3 = Image.GetQuadOffset(Resources.Img.MenuLoading, 0).X;
            DrawHelper.DrawImageQuad(texture2, 0, num3, 80f);
            num3 = Image.GetQuadOffset(Resources.Img.MenuLoading, 1).X;
            DrawHelper.DrawImageQuad(texture2, 1, num3, 80f);
            if (!game)
            {
                Renderer.Disable(Renderer.GL_SCISSOR_TEST);
            }
            if (game)
            {
                Vector quadOffset = Image.GetQuadOffset(Resources.Img.MenuLoading, 3);
                float num5 = 1250f * num2 / 100f;
                DrawHelper.DrawImageQuad(texture2, 3, quadOffset.X, 700f - num5);
            }
            else
            {
                float num6 = 1120f * num2 / 100f;
                DrawHelper.DrawImageQuad(texture2, 2, 1084f, num6 - 100f);
            }
            PostDraw();
            Renderer.SetColor(Color.White);
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
        }

        public bool game;

        private float currentPercent;
        private bool initialized;
        private bool animationComplete;

        private static Color s_Color1 = new(0.85f, 0.85f, 0.85f, 1f);
    }
}
