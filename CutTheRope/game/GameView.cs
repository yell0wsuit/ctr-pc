using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x0200007F RID: 127
	internal class GameView : View
	{
		// Token: 0x06000538 RID: 1336 RVA: 0x00026543 File Offset: 0x00024743
		public override NSObject initFullscreen()
		{
			if (base.initFullscreen() == null)
			{
				return null;
			}
			return this;
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00026550 File Offset: 0x00024750
		public override void show()
		{
			base.show();
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00026558 File Offset: 0x00024758
		public override void hide()
		{
			base.hide();
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x00026560 File Offset: 0x00024760
		public override void draw()
		{
			Global.MouseCursor.Enable(true);
			int num = this.childsCount();
			for (int i = 0; i < num; i++)
			{
				BaseElement child = this.getChild(i);
				if (child != null && child.visible)
				{
					if (i == 3)
					{
						OpenGL.glDisable(0);
						OpenGL.glEnable(1);
						OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
						GLDrawer.drawSolidRectWOBorder(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.1, 0.1, 0.1, 0.5));
						OpenGL.glColor4f(Color.White);
						OpenGL.glEnable(0);
					}
					child.draw();
				}
			}
			GameScene gameScene = (GameScene)this.getChild(0);
			if ((double)gameScene.dimTime > 0.0)
			{
				float num2 = gameScene.dimTime / 0.15f;
				if (gameScene.restartState == 0)
				{
					num2 = 1f - num2;
				}
				OpenGL.glDisable(0);
				OpenGL.glEnable(1);
				OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				GLDrawer.drawSolidRectWOBorder(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT, RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num2));
				OpenGL.glColor4f(Color.White);
				OpenGL.glEnable(0);
			}
		}

		// Token: 0x0400041A RID: 1050
		public const int VIEW_ELEMENT_GAME_SCENE = 0;

		// Token: 0x0400041B RID: 1051
		public const int VIEW_ELEMENT_PAUSE_BUTTON = 1;

		// Token: 0x0400041C RID: 1052
		public const int VIEW_ELEMENT_RESTART_BUTTON = 2;

		// Token: 0x0400041D RID: 1053
		public const int VIEW_ELEMENT_PAUSE_MENU = 3;

		// Token: 0x0400041E RID: 1054
		public const int VIEW_ELEMENT_RESULTS = 4;
	}
}
