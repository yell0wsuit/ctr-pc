using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.game
{
    // Token: 0x02000083 RID: 131
    internal class LoadingView : View
    {
        // Token: 0x06000559 RID: 1369 RVA: 0x0002A13C File Offset: 0x0002833C
        public override void draw()
        {
            Global.MouseCursor.Enable(false);
            OpenGL.glEnable(0);
            OpenGL.glEnable(1);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            this.preDraw();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            int num7 = 126 + cTRRootController.getPack();
            float num2 = (float)Application.sharedResourceMgr().getPercentLoaded();
            Texture2D texture = Application.getTexture(num7);
            OpenGL.glColor4f(LoadingView.s_Color1);
            Vector quadSize = Image.getQuadSize(num7, 0);
            float num3 = FrameworkTypes.SCREEN_WIDTH / 2f - quadSize.x;
            GLDrawer.drawImageQuad(texture, 0, (double)num3, 0.0);
            OpenGL.glPushMatrix();
            float num4 = FrameworkTypes.SCREEN_WIDTH / 2f + quadSize.x / 2f;
            OpenGL.glTranslatef((double)num4, (double)(FrameworkTypes.SCREEN_HEIGHT / 2f), 0.0);
            OpenGL.glRotatef(180.0, 0.0, 0.0, 1.0);
            OpenGL.glTranslatef((double)(0f - num4), (double)((0f - FrameworkTypes.SCREEN_HEIGHT) / 2f), 0.0);
            GLDrawer.drawImageQuad(texture, 0, (double)(FrameworkTypes.SCREEN_WIDTH / 2f), 0.5);
            OpenGL.glPopMatrix();
            Texture2D texture2 = Application.getTexture(5);
            if (!this.game)
            {
                OpenGL.glEnable(4);
                OpenGL.setScissorRectangle(0.0, 0.0, (double)FrameworkTypes.SCREEN_WIDTH, (double)(1200f * num2) / 100.0);
            }
            OpenGL.glColor4f(Color.White);
            num3 = Image.getQuadOffset(5, 0).x;
            GLDrawer.drawImageQuad(texture2, 0, (double)num3, 80.0);
            num3 = Image.getQuadOffset(5, 1).x;
            GLDrawer.drawImageQuad(texture2, 1, (double)num3, 80.0);
            if (!this.game)
            {
                OpenGL.glDisable(4);
            }
            if (this.game)
            {
                Vector quadOffset = Image.getQuadOffset(5, 3);
                float num5 = (float)(1250.0 * (double)num2 / 100.0);
                GLDrawer.drawImageQuad(texture2, 3, quadOffset.x, 700f - num5);
            }
            else
            {
                float num6 = (float)(1120.0 * (double)num2 / 100.0);
                GLDrawer.drawImageQuad(texture2, 2, 1084.0, (double)num6 - 100.0);
            }
            this.postDraw();
            OpenGL.glColor4f(Color.White);
            OpenGL.glDisable(0);
            OpenGL.glDisable(1);
        }

        // Token: 0x0400044A RID: 1098
        public bool game;

        // Token: 0x0400044B RID: 1099
        private static Color s_Color1 = new(0.85f, 0.85f, 0.85f, 1f);
    }
}
