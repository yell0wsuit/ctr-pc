using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using static CutTheRope.Framework.Helpers.CTRMathHelper;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Manages a collection of conveyor belts in a level, handling their updates, rendering,
    /// item attachment, and coordinating pointer input across overlapping belts.
    /// </summary>
    internal sealed class ConveyorBeltObject
    {
        private readonly Dictionary<int, Vector> pointerPositions = [];
        private readonly List<ConveyorBelt> list = [];
        private bool needsSort;

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
            pointerPositions.Clear();
            needsSort = false;
        }

        /// <summary>
        /// Adds a conveyor belt to the collection.
        /// </summary>
        /// <param name="belt">The belt to add.</param>
        public void Push(ConveyorBelt belt)
        {
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
            foreach (ConveyorBelt belt in list)
            {
                belt.Draw();
            }
        }

        /// <summary>
        /// Attaches a collection of items to any belts they overlap with.
        /// </summary>
        /// <param name="items">The items to attach.</param>
        public void AttachItems(IEnumerable<BaseElement> items)
        {
            foreach (BaseElement item in items)
            {
                if (item == null)
                {
                    continue;
                }
                AttachItemToBelts(item);
            }
        }

        /// <summary>
        /// Processes items to handle transitions between manual and automatic belts.
        /// </summary>
        /// <param name="items">The items to process.</param>
        public void ProcessItems(IEnumerable<BaseElement> items)
        {
            foreach (BaseElement item in items)
            {
                if (item == null)
                {
                    continue;
                }
                ProcessItem(item);
            }
        }

        /// <summary>
        /// Updates all conveyor belts and performs deferred sorting if needed.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame in seconds.</param>
        public void Update(float deltaTime)
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Update(deltaTime);
            }

            if (needsSort)
            {
                SortBelts();
                needsSort = false;
            }
        }

        /// <summary>
        /// Removes an item from all belts in the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(BaseElement item)
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Remove(item);
            }
        }

        /// <summary>
        /// Handles pointer down events, storing the position for later direction detection.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer.</param>
        /// <param name="pointerY">The y-coordinate of the pointer.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if a belt captured the pointer; false otherwise.</returns>
        public bool OnPointerDown(float pointerX, float pointerY, int pointerId)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ConveyorBelt belt = list[i];
                if (belt != null && belt.OnPointerDown(pointerX, pointerY, pointerId))
                {
                    pointerPositions[pointerId] = Vect(pointerX, pointerY);
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
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ConveyorBelt belt = list[i];
                if (belt != null && belt.OnPointerUp(pointerX, pointerY, pointerId))
                {
                    _ = pointerPositions.Remove(pointerId);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles pointer move events. When the drag exceeds a threshold, selects the belt
        /// whose direction best matches the drag direction for disambiguation.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer.</param>
        /// <param name="pointerY">The y-coordinate of the pointer.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if a belt handled the movement; false otherwise.</returns>
        public bool OnPointerMove(float pointerX, float pointerY, int pointerId)
        {
            if (pointerPositions.TryGetValue(pointerId, out Vector start))
            {
                Vector delta = Vect(pointerX - start.x, pointerY - start.y);
                float distanceSq = (delta.x * delta.x) + (delta.y * delta.y);
                if (distanceSq < 4f)
                {
                    return false;
                }

                float distance = VectLength(delta);
                Vector direction = distance > 0 ? Vect(delta.x / distance, delta.y / distance) : vectZero;

                float bestDot = -1f;
                ConveyorBelt bestBelt = null;
                foreach (ConveyorBelt belt in list)
                {
                    if (belt == null || !belt.Contains(start))
                    {
                        continue;
                    }
                    float dot = Math.Abs((direction.x * belt.Direction.x) + (direction.y * belt.Direction.y));
                    if (dot >= bestDot)
                    {
                        bestDot = dot;
                        bestBelt = belt;
                    }
                }

                _ = (bestBelt?.OnPointerDown(start.x, start.y, pointerId));

                _ = pointerPositions.Remove(pointerId);
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                ConveyorBelt belt = list[i];
                if (belt != null && belt.OnPointerMove(pointerX, pointerY, pointerId))
                {
                    RequestSort();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attaches an item to all belts that contain its position.
        /// </summary>
        private void AttachItemToBelts(BaseElement item)
        {
            Vector position = ConveyorBelt.GetItemPosition(item);
            foreach (ConveyorBelt belt in list)
            {
                if (belt.Contains(position))
                {
                    belt.AttachItem(item);
                }
            }
        }

        /// <summary>
        /// Processes an item to handle belt transitions. Items on manual belts can transfer
        /// to active manual belts or automatic belts when overlapping.
        /// </summary>
        private void ProcessItem(BaseElement item)
        {
            ConveyorBelt manualBelt = null;
            List<ConveyorBelt> overlappingBelts = [];

            Vector position = ConveyorBelt.GetItemPosition(item);
            float padding = ConveyorBelt.GetItemPadding(item);

            foreach (ConveyorBelt belt in list)
            {
                if (belt.ContainsWithPadding(position, padding))
                {
                    overlappingBelts.Add(belt);
                }
                if (belt.HasItem(item))
                {
                    manualBelt = belt;
                }
            }

            if (manualBelt != null && manualBelt.IsManual)
            {
                foreach (ConveyorBelt belt in overlappingBelts)
                {
                    if (belt.IsManual && belt.IsActive())
                    {
                        MoveItemToBelt(belt, item);
                        return;
                    }
                }

                foreach (ConveyorBelt belt in overlappingBelts)
                {
                    if (!belt.IsManual)
                    {
                        MoveItemToBelt(belt, item);
                    }
                }
            }
        }

        /// <summary>
        /// Transfers an item to a new belt, marking it for removal from all other belts.
        /// </summary>
        private void MoveItemToBelt(ConveyorBelt belt, BaseElement item)
        {
            if (!belt.HasItem(item) || belt.IsItemMarkedForRemoval(item))
            {
                foreach (ConveyorBelt candidate in list)
                {
                    if (candidate.HasItem(item))
                    {
                        candidate.MarkItemForRemoval(item);
                    }
                }

                belt.AttachItem(item);
                CTRSoundMgr.PlaySound(Resources.Snd.TransporterMove);
            }
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

        /// <summary>
        /// Requests a deferred sort of the belt list on the next update.
        /// </summary>
        private void RequestSort()
        {
            needsSort = true;
        }
    }
}
