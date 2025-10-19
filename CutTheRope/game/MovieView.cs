using System;
using CutTheRope.iframework.core;
using CutTheRope.windows;

namespace CutTheRope.game
{
	// Token: 0x02000089 RID: 137
	internal class MovieView : MenuView
	{
		// Token: 0x060005A0 RID: 1440 RVA: 0x0002E213 File Offset: 0x0002C413
		public override void update(float t)
		{
			Application.sharedMovieMgr().start();
			Global.MouseCursor.Enable(Application.sharedMovieMgr().isPaused());
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0002E233 File Offset: 0x0002C433
		public override void draw()
		{
			Global.XnaGame.DrawMovie();
		}
	}
}
