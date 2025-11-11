using CutTheRope.ios;
using System;
using System.Collections;

internal sealed class DynamicArray : NSObject, IEnumerable
{
    public NSObject this[int key] => map[key];

    public IEnumerator GetEnumerator()
    {
        return new DynamicArrayEnumerator(map, highestIndex);
    }

    public override NSObject Init()
    {
        _ = InitWithCapacityandOverReallocValue(10, 10);
        return this;
    }

    public NSObject InitWithCapacity(int c)
    {
        if (base.Init() != null)
        {
            size = c;
            highestIndex = -1;
            overRealloc = 0;
            mutationsCount = 0UL;
            map = new NSObject[size];
        }
        return this;
    }

    public NSObject InitWithCapacityandOverReallocValue(int c, int v)
    {
        if (InitWithCapacity(c) != null)
        {
            overRealloc = v;
        }
        return this;
    }

    public int Count()
    {
        return highestIndex + 1;
    }

    public int Capacity()
    {
        return size;
    }

    public void SetNewSize(int k)
    {
        int num = k + overRealloc;
        NSObject[] array = new NSObject[num];
        Array.Copy(map, array, Math.Min(map.Length, array.Length));
        map = array;
        size = num;
    }

    public int AddObject(NSObject obj)
    {
        int num = highestIndex + 1;
        SetObjectAt(obj, num);
        return num;
    }

    public void SetObjectAt(NSObject obj, int k)
    {
        if (k >= size)
        {
            SetNewSize(k + 1);
        }
        if (map[k] != null)
        {
            map[k].Release();
            map[k] = null;
        }
        if (highestIndex < k)
        {
            highestIndex = k;
        }
        map[k] = obj;
        map[k].Retain();
        mutationsCount += 1UL;
    }

    public NSObject FirstObject()
    {
        return ObjectAtIndex(0);
    }

    public NSObject LastObject()
    {
        return highestIndex == -1 ? null : ObjectAtIndex(highestIndex);
    }

    public NSObject ObjectAtIndex(int k)
    {
        return map[k];
    }

    public void UnsetAll()
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] != null)
            {
                UnsetObjectAtIndex(i);
            }
        }
    }

    public void UnsetObjectAtIndex(int k)
    {
        map[k].Release();
        map[k] = null;
        mutationsCount += 1UL;
    }

    public void InsertObjectatIndex(NSObject obj, int k)
    {
        if (k >= size || highestIndex + 1 >= size)
        {
            SetNewSize(size + 1);
        }
        highestIndex++;
        for (int num = highestIndex; num > k; num--)
        {
            map[num] = map[num - 1];
        }
        map[k] = obj;
        map[k].Retain();
        mutationsCount += 1UL;
    }

    public void RemoveObjectAtIndex(int k)
    {
        NSObject nSObject = map[k];
        nSObject?.Release();
        for (int i = k; i < highestIndex; i++)
        {
            map[i] = map[i + 1];
        }
        map[highestIndex] = null;
        highestIndex--;
        mutationsCount += 1UL;
    }

    public void RemoveAllObjects()
    {
        UnsetAll();
        highestIndex = -1;
    }

    public void RemoveObject(NSObject obj)
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] == obj)
            {
                RemoveObjectAtIndex(i);
                return;
            }
        }
    }

    public int GetFirstEmptyIndex()
    {
        for (int i = 0; i < size; i++)
        {
            if (map[i] == null)
            {
                return i;
            }
        }
        return size;
    }

    public int GetObjectIndex(NSObject obj)
    {
        for (int i = 0; i < size; i++)
        {
            if (map[i] == obj)
            {
                return i;
            }
        }
        return -1;
    }

    public override void Dealloc()
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] != null)
            {
                map[i].Release();
                map[i] = null;
            }
        }
        Free(map);
        map = null;
        base.Dealloc();
    }

    public const int DEFAULT_CAPACITY = 10;

    public NSObject[] map;

    public int size;

    public int highestIndex;

    public int overRealloc;

    public ulong mutationsCount;
}
