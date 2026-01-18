using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using static CutTheRope.Framework.Helpers.CTRMathHelper;

namespace CutTheRope.GameMain
{
    internal sealed class ConveyorBeltObject
    {
        private readonly Dictionary<int, Vector> pointerPositions = [];
        private readonly List<ConveyorBelt> list = [];
        private bool needsSort;

        public int Count()
        {
            return list.Count;
        }

        public void Clear()
        {
            list.Clear();
            pointerPositions.Clear();
            needsSort = false;
        }

        public void Push(ConveyorBelt belt)
        {
            list.Add(belt);
        }

        public IEnumerable<ConveyorBelt> Iterator()
        {
            return list;
        }

        public void Draw()
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Draw();
            }
        }

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

        public void Remove(BaseElement item)
        {
            foreach (ConveyorBelt belt in list)
            {
                belt.Remove(item);
            }
        }

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

        private void SwapBelts(int fromIndex, int toIndex)
        {
            (list[toIndex], list[fromIndex]) = (list[fromIndex], list[toIndex]);
        }

        private void RequestSort()
        {
            needsSort = true;
        }
    }
}
