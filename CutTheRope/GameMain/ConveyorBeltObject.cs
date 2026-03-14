using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Manages a collection of conveyor belts in a level, handling their updates, rendering,
    /// item attachment, and coordinating pointer input across overlapping belts.
    /// </summary>
    internal sealed class ConveyorBeltObject
    {
        private readonly List<ConveyorBelt> list = [];
        private readonly List<ConveyorBelt> touchCandidates = [];

        /// <summary>
        /// Set by GameScene. Called when a Grab wraps and other ropes for the same candy should be cut.
        /// Parameters: candyNumber, the Grab that wrapped.
        /// </summary>
        public Action<int, Grab> OnDestroyRopesForCandy;

        /// <summary>
        /// Gets the number of conveyor belts in this collection.
        /// </summary>
        /// <returns>The count of belts.</returns>
        public int Count()
        {
            return list.Count;
        }

        /// <summary>
        /// Removes all conveyor belts and resets the collection state.
        /// </summary>
        public void Clear()
        {
            list.Clear();
            touchCandidates.Clear();
        }

        /// <summary>
        /// Adds a conveyor belt to the collection.
        /// </summary>
        /// <param name="belt">The belt to add.</param>
        public void Push(ConveyorBelt belt)
        {
            belt.OnTransporterMoves = TransporterMoves;
            belt.OnDestroyRopesForCandy = OnDestroyRopesForCandy;
            list.Add(belt);
        }

        /// <summary>
        /// Returns an enumerable for iterating over all belts in the collection.
        /// </summary>
        /// <returns>An enumerable of conveyor belts.</returns>
        public IEnumerable<ConveyorBelt> Iterator()
        {
            return list;
        }

        /// <summary>
        /// Draws all conveyor belts in the collection.
        /// </summary>
        public void Draw()
        {
            // Manual transporters sorted by activeSetTime (ascending),
            // then non-manual transporters in their existing order.
            touchCandidates.Clear();
            foreach (ConveyorBelt belt in list)
            {
                if (belt is { IsManual: true })
                {
                    touchCandidates.Add(belt);
                }
            }

            touchCandidates.Sort(static (a, b) => a.ActiveSetTime.CompareTo(b.ActiveSetTime));

            foreach (ConveyorBelt belt in touchCandidates)
            {
                belt.Draw();
            }

            foreach (ConveyorBelt belt in list)
            {
                if (!belt.IsManual)
                {
                    belt.Draw();
                }
            }
        }

        /// <summary>
        /// For each belt, checks each item for collision and binds if matching.
        /// Items already bound to any belt are skipped.
        /// </summary>
        public void ProcessItems<T>(IEnumerable<T> items)
        {
            foreach (T obj in items)
            {
                if (obj is not ITransporterItem item)
                {
                    continue;
                }

                // Skip if already bound to any belt
                bool alreadyBound = false;
                foreach (ConveyorBelt belt in list)
                {
                    if (belt.HasItem(item))
                    {
                        alreadyBound = true;
                        break;
                    }
                }
                if (alreadyBound)
                {
                    continue;
                }

                // Check collision against each belt with 0.6x radius
                float radius = item.CollisionRadius * 0.6f;
                Vector bindPoint = item.BindPoint;
                foreach (ConveyorBelt belt in list)
                {
                    if (belt.CollidesWithCircle(bindPoint, radius))
                    {
                        belt.BindObject(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates all conveyor belts.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame in seconds.</param>
        public void Update(float deltaTime)
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Update(deltaTime);
            }

            SortBelts();
        }

        /// <summary>
        /// Handles transporter-to-transporter handoff. Called after a belt moves its items.
        /// Checks if any item bound to another belt now overlaps this belt and should transfer.
        /// Matches iOS transporterMoves: delegate.
        /// </summary>
        private void TransporterMoves(ConveyorBelt movingBelt)
        {
            foreach (ConveyorBelt ownerBelt in list)
            {
                if (ownerBelt == movingBelt)
                {
                    continue;
                }

                for (int i = ownerBelt.BoundObjects.Count - 1; i >= 0; i--)
                {
                    ITransporterItem item = ownerBelt.BoundObjects[i];

                    if (movingBelt.HasItem(item))
                    {
                        continue;
                    }

                    float radius = item.CollisionRadius * 0.6f;

                    if (!movingBelt.CollidesWithCircle(item.BindPoint, radius))
                    {
                        continue;
                    }

                    if (AutoTransportersOwnObject(item))
                    {
                        continue;
                    }

                    bool canTake = !movingBelt.IsManual || movingBelt.ActiveSetTime >= ownerBelt.ActiveSetTime;
                    if (!canTake)
                    {
                        continue;
                    }

                    if (UnbindObjectFromTransporters(item))
                    {
                        movingBelt.BindObject(item);
                        CTRSoundMgr.PlaySound(Resources.Snd.TransporterMove);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an item from all belts in the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(ITransporterItem item)
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Remove(item);
            }
        }

        /// <summary>
        /// Handles pointer down events by selecting the most recently activated manual transporter first.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer.</param>
        /// <param name="pointerY">The y-coordinate of the pointer.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if a belt captured the pointer; false otherwise.</returns>
        public bool OnPointerDown(float pointerX, float pointerY, int pointerId)
        {
            touchCandidates.Clear();
            foreach (ConveyorBelt belt in list)
            {
                if (belt is { IsManual: true })
                {
                    touchCandidates.Add(belt);
                }
            }

            touchCandidates.Sort(static (a, b) => b.ActiveSetTime.CompareTo(a.ActiveSetTime));

            foreach (ConveyorBelt belt in touchCandidates)
            {
                if (belt.OnPointerDown(pointerX, pointerY, pointerId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles pointer up events, releasing any captured belt.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer.</param>
        /// <param name="pointerY">The y-coordinate of the pointer.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if a belt released the pointer; false otherwise.</returns>
        public bool OnPointerUp(float pointerX, float pointerY, int pointerId)
        {
            foreach (ConveyorBelt belt in list)
            {
                if (belt != null && belt.OnPointerUp(pointerX, pointerY, pointerId))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles pointer move events.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer.</param>
        /// <param name="pointerY">The y-coordinate of the pointer.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if a belt handled the movement; false otherwise.</returns>
        public bool OnPointerMove(float pointerX, float pointerY, int pointerId)
        {
            foreach (ConveyorBelt belt in list)
            {
                if (belt != null && belt.OnPointerMove(pointerX, pointerY, pointerId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sorts belts so that active manual belts have higher priority, followed by all manual belts.
        /// This ensures proper input handling when belts overlap.
        /// </summary>
        public void SortBelts()
        {
            int end = Count() - 1;
            for (int i = end; i >= 0; i--)
            {
                if (list[i].IsManual && list[i].IsActive())
                {
                    for (int j = i; j < end; j++)
                    {
                        SwapBelts(j, j + 1);
                    }
                    end--;
                }
            }
            SortByManualFlag();
        }

        private bool AutoTransportersOwnObject(ITransporterItem item)
        {
            foreach (ConveyorBelt belt in list)
            {
                if (!belt.IsManual && belt.HasItem(item))
                {
                    return true;
                }
            }

            return false;
        }

        private bool UnbindObjectFromTransporters(ITransporterItem item)
        {
            bool removed = false;
            foreach (ConveyorBelt belt in list)
            {
                if (belt.HasItem(item))
                {
                    belt.Remove(item);
                    removed = true;
                }
            }

            return removed;
        }

        /// <summary>
        /// Secondary sort pass that moves all manual belts to the end of the list.
        /// </summary>
        private void SortByManualFlag()
        {
            int end = Count() - 1;
            for (int i = end; i >= 0; i--)
            {
                if (!list[i].IsManual)
                {
                    for (int j = i; j < end; j++)
                    {
                        SwapBelts(j, j + 1);
                    }
                    end--;
                }
            }
        }

        /// <summary>
        /// Swaps two belts in the list by their indices.
        /// </summary>
        private void SwapBelts(int fromIndex, int toIndex)
        {
            (list[toIndex], list[fromIndex]) = (list[fromIndex], list[toIndex]);
        }

    }
}
