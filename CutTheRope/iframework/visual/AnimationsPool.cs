using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal sealed class AnimationsPool : BaseElement, ITimelineDelegate
    {
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            if (GetChildId(t.element) != -1)
            {
                removeList.Add(t.element);
            }
        }

        public override void Update(float delta)
        {
            int count = removeList.Count;
            for (int i = 0; i < count; i++)
            {
                RemoveChild(removeList[i]);
            }
            removeList.Clear();
            base.Update(delta);
        }

        public override void Draw()
        {
            base.Draw();
        }

        public void ParticlesFinished(Particles p)
        {
            if (GetChildId(p) != -1)
            {
                removeList.Add(p);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                removeList?.Clear();
                removeList = null;
            }
            base.Dispose(disposing);
        }

        private List<BaseElement> removeList = [];
    }
}
