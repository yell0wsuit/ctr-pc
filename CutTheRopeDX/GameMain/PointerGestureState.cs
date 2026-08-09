using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Owns every piece of mutable rope-cut gesture state for one pointer.
    /// </summary>
    internal sealed class PointerGestureState(FingerTrace trace)
    {
        private const float TraceDragThresholdSquared = 100f;

        /// <summary>Whether this pointer currently owns a rope-cut drag.</summary>
        public bool IsDragging { get; private set; }

        /// <summary>Latest pointer position in screen space.</summary>
        public Vector StartPosition { get; private set; }

        /// <summary>Screen-space pointer position preceding <see cref="StartPosition"/>.</summary>
        public Vector PreviousStartPosition { get; private set; }

        /// <summary>World-space position at which the pointer went down.</summary>
        public Vector TraceDownPosition { get; private set; }

        /// <summary>Whether movement has crossed the finger-trace activation threshold.</summary>
        public bool IsTraceDragging { get; private set; }

        /// <summary>Cut ribbons created by this pointer.</summary>
        public List<GameScene.FingerCut> Cuts { get; } = [];

        /// <summary>Visual finger trace assigned to this pointer.</summary>
        public FingerTrace Trace { get; } = trace;

        /// <summary>Begins a gesture unless this pointer is already active.</summary>
        public void Begin(Vector screenPosition, Vector worldPosition)
        {
            if (IsDragging)
            {
                return;
            }

            IsDragging = true;
            StartPosition = screenPosition;
            PreviousStartPosition = screenPosition;
            TraceDownPosition = worldPosition;
            IsTraceDragging = false;
        }

        /// <summary>
        /// Advances an active gesture and returns the start of the new cut segment.
        /// </summary>
        public bool Move(Vector screenPosition, Vector worldPosition, out Vector segmentStart)
        {
            segmentStart = StartPosition;
            if (!IsDragging)
            {
                return false;
            }

            PreviousStartPosition = StartPosition;
            StartPosition = screenPosition;

            if (Trace != null)
            {
                if (!IsTraceDragging)
                {
                    float dx = worldPosition.X - TraceDownPosition.X;
                    float dy = worldPosition.Y - TraceDownPosition.Y;
                    IsTraceDragging = (dx * dx) + (dy * dy) >= TraceDragThresholdSquared;
                }

                if (IsTraceDragging)
                {
                    Trace.Append(worldPosition);
                }
            }

            return true;
        }

        /// <summary>Completes a gesture while allowing its visible trace to fade out.</summary>
        public void End()
        {
            IsDragging = false;
            if (IsTraceDragging)
            {
                Trace?.End();
                IsTraceDragging = false;
            }
        }

        /// <summary>Cancels all active gesture ownership for this pointer.</summary>
        public void Cancel()
        {
            End();
        }

        /// <summary>Clears this pointer for a newly shown scene.</summary>
        public void Reset()
        {
            Cancel();
            StartPosition = default;
            PreviousStartPosition = default;
            TraceDownPosition = default;
            Cuts.Clear();
            Trace?.Reset();
        }

        /// <summary>Advances fading cut ribbons and the visual trace independently of gameplay simulation.</summary>
        public void UpdateVisuals(float delta)
        {
            for (int i = 0; i < Cuts.Count; i++)
            {
                GameScene.FingerCut cut = Cuts[i];
                float alpha = cut.c.AlphaChannel;
                if (Mover.MoveVariableToTarget(ref alpha, 0f, 10f, delta))
                {
                    Cuts.RemoveAt(i);
                    i--;
                }
                else
                {
                    cut.c.AlphaChannel = alpha;
                }
            }

            Trace?.Update(delta);
        }
    }
}
