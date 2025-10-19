using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200002A RID: 42
	internal class AnimationsPool : BaseElement, TimelineDelegate
	{
		// Token: 0x06000158 RID: 344 RVA: 0x00007279 File Offset: 0x00005479
		public AnimationsPool()
		{
			this.init();
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00007293 File Offset: 0x00005493
		public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00007295 File Offset: 0x00005495
		public virtual void timelineFinished(Timeline t)
		{
			if (this.getChildId(t.element) != -1)
			{
				this.removeList.Add(t.element);
			}
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000072B8 File Offset: 0x000054B8
		public override void update(float delta)
		{
			int count = this.removeList.Count;
			for (int i = 0; i < count; i++)
			{
				this.removeChild(this.removeList[i]);
			}
			this.removeList.Clear();
			base.update(delta);
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00007301 File Offset: 0x00005501
		public override void draw()
		{
			base.draw();
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00007309 File Offset: 0x00005509
		public virtual void particlesFinished(Particles p)
		{
			if (this.getChildId(p) != -1)
			{
				this.removeList.Add(p);
			}
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00007321 File Offset: 0x00005521
		public override void dealloc()
		{
			this.removeList.Clear();
			this.removeList = null;
			base.dealloc();
		}

		// Token: 0x040000FC RID: 252
		private List<BaseElement> removeList = new List<BaseElement>();
	}
}
