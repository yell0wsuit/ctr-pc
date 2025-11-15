using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
    internal sealed class GameView : View
    {
        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Draw()
        {
            Global.MouseCursor.Enable(true);
            int num = ChildsCount();
            for (int i = 0; i < num; i++)
            {
                BaseElement child = GetChild(i);
                if (child != null && child.visible)
                {
                    if (i == 3)
                    {
                        OpenGL.GlDisable(0);
                        OpenGL.GlEnable(1);
                        OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.1, 0.1, 0.1, 0.5));
                        OpenGL.GlColor4f(Color.White);
                        OpenGL.GlEnable(0);
                    }
                    child.Draw();
                }
            }
            GameScene gameScene = (GameScene)GetChild(0);
            if (gameScene.dimTime > 0.0)
            {
                float num2 = gameScene.dimTime / 0.15f;
                if (gameScene.restartState == 0)
                {
                    num2 = 1f - num2;
                }
                OpenGL.GlDisable(0);
                OpenGL.GlEnable(1);
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num2));
                OpenGL.GlColor4f(Color.White);
                OpenGL.GlEnable(0);
            }
        }

        public const int VIEW_ELEMENT_GAME_SCENE = 0;

        public const int VIEW_ELEMENT_PAUSE_BUTTON = 1;

        public const int VIEW_ELEMENT_RESTART_BUTTON = 2;

        public const int VIEW_ELEMENT_PAUSE_MENU = 3;

        public const int VIEW_ELEMENT_RESULTS = 4;
    }
}
