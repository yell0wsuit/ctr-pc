using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Gameplay view that draws the active scene, pause overlay, restart controls, and results elements.
    /// </summary>
    internal sealed class GameView : View
    {
        /// <inheritdoc />
        public override void Show()
        {
            base.Show();
        }

        /// <inheritdoc />
        public override void Hide()
        {
            base.Hide();
        }

        /// <inheritdoc />
        public override void Draw()
        {
            Global.MouseCursor.Enable(true);
            int childCount = ChildsCount();
            for (int i = 0; i < childCount; i++)
            {
                BaseElement child = GetChild(i);
                if (child != null && child.visible)
                {
                    if (i == 3)
                    {
                        Renderer.Disable(Renderer.GL_TEXTURE_2D);
                        Renderer.Enable(Renderer.GL_BLEND);
                        Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.1f, 0.1f, 0.1f, 0.5f));
                        Renderer.SetColor(Color.White);
                        Renderer.Enable(Renderer.GL_TEXTURE_2D);
                    }
                    child.Draw();
                }
            }
            GameScene gameScene = (GameScene)GetChild(0);
            if (gameScene.dimTime > 0)
            {
                float dimAlpha = gameScene.dimTime / 0.15f;
                if (gameScene.restartState == 0)
                {
                    dimAlpha = 1f - dimAlpha;
                }
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                Renderer.Enable(Renderer.GL_BLEND);
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(1, 1, 1, dimAlpha));
                Renderer.SetColor(Color.White);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
            }
        }

        /// <summary>Child index for the active game scene.</summary>
        public const int VIEW_ELEMENT_GAME_SCENE = 0;

        /// <summary>Child index for the pause button.</summary>
        public const int VIEW_ELEMENT_PAUSE_BUTTON = 1;

        /// <summary>Child index for the restart button.</summary>
        public const int VIEW_ELEMENT_RESTART_BUTTON = 2;

        /// <summary>Child index for the pause menu overlay.</summary>
        public const int VIEW_ELEMENT_PAUSE_MENU = 3;

        /// <summary>Child index for the results view.</summary>
        public const int VIEW_ELEMENT_RESULTS = 4;
    }
}
