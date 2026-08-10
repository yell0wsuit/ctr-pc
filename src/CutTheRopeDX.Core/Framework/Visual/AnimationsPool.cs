using System.Collections.Generic;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Manages a pool of child animations, automatically removing them when their timelines or particles finish.
    /// </summary>
    internal sealed class AnimationsPool : BaseElement, ITimelineDelegate
    {
        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            if (GetChildId(t.element) != -1)
            {
                removeList.Add(t.element);
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void Draw()
        {
            base.Draw();
        }

        /// <summary>
        /// Called when a particle system finishes; schedules it for removal.
        /// </summary>
        /// <param name="p">Particle system that finished.</param>
        public void ParticlesFinished(Particles p)
        {
            if (GetChildId(p) != -1)
            {
                removeList.Add(p);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                removeList?.Clear();
                removeList = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Elements scheduled for removal on the next update.
        /// </summary>
        private List<BaseElement> removeList = [];
    }
}
