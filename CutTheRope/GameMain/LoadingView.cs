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
            Global.MouseCursor.Enable(false);
            OpenGL.GlEnable(0);
            OpenGL.GlEnable(1);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
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
            OpenGL.GlColor4f(s_Color1);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float num3 = (SCREEN_WIDTH / 2f) - quadSize.x;
            GLDrawer.DrawImageQuad(texture, 0, (double)num3, 0.0);
            OpenGL.GlPushMatrix();
            float num4 = (SCREEN_WIDTH / 2f) + (quadSize.x / 2f);
            OpenGL.GlTranslatef((double)num4, (double)(SCREEN_HEIGHT / 2f), 0.0);
            OpenGL.GlRotatef(180.0, 0.0, 0.0, 1.0);
            OpenGL.GlTranslatef((double)(0f - num4), (double)((0f - SCREEN_HEIGHT) / 2f), 0.0);
            GLDrawer.DrawImageQuad(texture, 0, (double)(SCREEN_WIDTH / 2f), 0.5);
            OpenGL.GlPopMatrix();
            CTRTexture2D texture2 = Application.GetTexture(Resources.Img.MenuLoading);
            if (!game)
            {
                OpenGL.GlEnable(4);
                OpenGL.SetScissorRectangle(0.0, 0.0, SCREEN_WIDTH, (double)(1200f * num2) / 100.0);
            }
            OpenGL.GlColor4f(Color.White);
            num3 = Image.GetQuadOffset(Resources.Img.MenuLoading, 0).x;
            GLDrawer.DrawImageQuad(texture2, 0, (double)num3, 80.0);
            num3 = Image.GetQuadOffset(Resources.Img.MenuLoading, 1).x;
            GLDrawer.DrawImageQuad(texture2, 1, (double)num3, 80.0);
            if (!game)
            {
                OpenGL.GlDisable(4);
            }
            if (game)
            {
                Vector quadOffset = Image.GetQuadOffset(Resources.Img.MenuLoading, 3);
                float num5 = (float)(1250.0 * (double)num2 / 100.0);
                GLDrawer.DrawImageQuad(texture2, 3, quadOffset.x, 700f - num5);
            }
            else
            {
                float num6 = (float)(1120.0 * (double)num2 / 100.0);
                GLDrawer.DrawImageQuad(texture2, 2, 1084.0, (double)num6 - 100.0);
            }
            PostDraw();
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlDisable(0);
            OpenGL.GlDisable(1);
        }

        public bool game;

        private float currentPercent;
        private bool initialized;
        private bool animationComplete;

        private static Color s_Color1 = new(0.85f, 0.85f, 0.85f, 1f);
    }
}
