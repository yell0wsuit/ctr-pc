using CutTheRope.ios;
using System;
using System.Collections;

internal class DynamicArray : NSObject, IEnumerable
{
    public NSObject this[int key] => map[key];

    public IEnumerator GetEnumerator()
    {
        return new DynamicArrayEnumerator(map, highestIndex);
    }

    public override NSObject init()
    {
        initWithCapacityandOverReallocValue(10, 10);
        return this;
    }

    public virtual NSObject initWithCapacity(int c)
    {
        if (base.init() != null)
        {
            size = c;
            highestIndex = -1;
            overRealloc = 0;
            mutationsCount = 0UL;
            map = new NSObject[size];
        }
        return this;
    }

    public virtual NSObject initWithCapacityandOverReallocValue(int c, int v)
    {
        if (initWithCapacity(c) != null)
        {
            overRealloc = v;
        }
        return this;
    }

    public virtual int count()
    {
        return highestIndex + 1;
    }

    public virtual int capacity()
    {
        return size;
    }

    public virtual void setNewSize(int k)
    {
        int num = k + overRealloc;
        NSObject[] array = new NSObject[num];
        Array.Copy(map, array, Math.Min(map.Length, array.Length));
        map = array;
        size = num;
    }

    public virtual int addObject(NSObject obj)
    {
        int num = highestIndex + 1;
        setObjectAt(obj, num);
        return num;
    }

    public virtual void setObjectAt(NSObject obj, int k)
    {
        if (k >= size)
        {
            setNewSize(k + 1);
        }
        if (map[k] != null)
        {
            map[k].release();
            map[k] = null;
        }
        if (highestIndex < k)
        {
            highestIndex = k;
        }
        map[k] = obj;
        map[k].retain();
        mutationsCount += 1UL;
    }

    public virtual NSObject firstObject()
    {
        return objectAtIndex(0);
    }

    public virtual NSObject lastObject()
    {
        return highestIndex == -1 ? null : objectAtIndex(highestIndex);
    }

    public virtual NSObject objectAtIndex(int k)
    {
        return map[k];
    }

    public virtual void unsetAll()
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] != null)
            {
                unsetObjectAtIndex(i);
            }
        }
    }

    public virtual void unsetObjectAtIndex(int k)
    {
        map[k].release();
        map[k] = null;
        mutationsCount += 1UL;
    }

    public virtual void insertObjectatIndex(NSObject obj, int k)
    {
        if (k >= size || highestIndex + 1 >= size)
        {
            setNewSize(size + 1);
        }
        highestIndex++;
        for (int num = highestIndex; num > k; num--)
        {
            map[num] = map[num - 1];
        }
        map[k] = obj;
        map[k].retain();
        mutationsCount += 1UL;
    }

    public virtual void removeObjectAtIndex(int k)
    {
        NSObject nSObject = map[k];
        nSObject?.release();
        for (int i = k; i < highestIndex; i++)
        {
            map[i] = map[i + 1];
        }
        map[highestIndex] = null;
        highestIndex--;
        mutationsCount += 1UL;
    }

    public virtual void removeAllObjects()
    {
        unsetAll();
        highestIndex = -1;
    }

    public virtual void removeObject(NSObject obj)
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] == obj)
            {
                removeObjectAtIndex(i);
                return;
            }
        }
    }

    public virtual int getFirstEmptyIndex()
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

    public virtual int getObjectIndex(NSObject obj)
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

    public override void dealloc()
    {
        for (int i = 0; i <= highestIndex; i++)
        {
            if (map[i] != null)
            {
                map[i].release();
                map[i] = null;
            }
        }
        free(map);
        map = null;
        base.dealloc();
    }

    public const int DEFAULT_CAPACITY = 10;

    public NSObject[] map;

    public int size;

    public int highestIndex;

    public int overRealloc;

    public ulong mutationsCount;
}
