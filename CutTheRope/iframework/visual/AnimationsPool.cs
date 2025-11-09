using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal class AnimationsPool : BaseElement, TimelineDelegate
    {
        public AnimationsPool()
        {
            this.init();
        }

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public virtual void timelineFinished(Timeline t)
        {
            if (this.getChildId(t.element) != -1)
            {
                this.removeList.Add(t.element);
            }
        }

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

        public override void draw()
        {
            base.draw();
        }

        public virtual void particlesFinished(Particles p)
        {
            if (this.getChildId(p) != -1)
            {
                this.removeList.Add(p);
            }
        }

        public override void dealloc()
        {
            this.removeList.Clear();
            this.removeList = null;
            base.dealloc();
        }

        private List<BaseElement> removeList = new();
    }
}
