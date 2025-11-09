using CutTheRope.ios;
using System;
using System.Collections;

internal class DynamicArray : NSObject, IEnumerable
{
    public NSObject this[int key]
    {
        get
        {
            return this.map[key];
        }
    }

    public IEnumerator GetEnumerator()
    {
        return new DynamicArrayEnumerator(this.map, this.highestIndex);
    }

    public override NSObject init()
    {
        this.initWithCapacityandOverReallocValue(10, 10);
        return this;
    }

    public virtual NSObject initWithCapacity(int c)
    {
        if (base.init() != null)
        {
            this.size = c;
            this.highestIndex = -1;
            this.overRealloc = 0;
            this.mutationsCount = 0UL;
            this.map = new NSObject[this.size];
        }
        return this;
    }

    public virtual NSObject initWithCapacityandOverReallocValue(int c, int v)
    {
        if (this.initWithCapacity(c) != null)
        {
            this.overRealloc = v;
        }
        return this;
    }

    public virtual int count()
    {
        return this.highestIndex + 1;
    }

    public virtual int capacity()
    {
        return this.size;
    }

    public virtual void setNewSize(int k)
    {
        int num = k + this.overRealloc;
        NSObject[] array = new NSObject[num];
        Array.Copy(this.map, array, Math.Min(this.map.Length, array.Length));
        this.map = array;
        this.size = num;
    }

    public virtual int addObject(NSObject obj)
    {
        int num = this.highestIndex + 1;
        this.setObjectAt(obj, num);
        return num;
    }

    public virtual void setObjectAt(NSObject obj, int k)
    {
        if (k >= this.size)
        {
            this.setNewSize(k + 1);
        }
        if (this.map[k] != null)
        {
            this.map[k].release();
            this.map[k] = null;
        }
        if (this.highestIndex < k)
        {
            this.highestIndex = k;
        }
        this.map[k] = obj;
        this.map[k].retain();
        this.mutationsCount += 1UL;
    }

    public virtual NSObject firstObject()
    {
        return this.objectAtIndex(0);
    }

    public virtual NSObject lastObject()
    {
        if (this.highestIndex == -1)
        {
            return null;
        }
        return this.objectAtIndex(this.highestIndex);
    }

    public virtual NSObject objectAtIndex(int k)
    {
        return this.map[k];
    }

    public virtual void unsetAll()
    {
        for (int i = 0; i <= this.highestIndex; i++)
        {
            if (this.map[i] != null)
            {
                this.unsetObjectAtIndex(i);
            }
        }
    }

    public virtual void unsetObjectAtIndex(int k)
    {
        this.map[k].release();
        this.map[k] = null;
        this.mutationsCount += 1UL;
    }

    public virtual void insertObjectatIndex(NSObject obj, int k)
    {
        if (k >= this.size || this.highestIndex + 1 >= this.size)
        {
            this.setNewSize(this.size + 1);
        }
        this.highestIndex++;
        for (int num = this.highestIndex; num > k; num--)
        {
            this.map[num] = this.map[num - 1];
        }
        this.map[k] = obj;
        this.map[k].retain();
        this.mutationsCount += 1UL;
    }

    public virtual void removeObjectAtIndex(int k)
    {
        NSObject nSObject = this.map[k];
        nSObject?.release();
        for (int i = k; i < this.highestIndex; i++)
        {
            this.map[i] = this.map[i + 1];
        }
        this.map[this.highestIndex] = null;
        this.highestIndex--;
        this.mutationsCount += 1UL;
    }

    public virtual void removeAllObjects()
    {
        this.unsetAll();
        this.highestIndex = -1;
    }

    public virtual void removeObject(NSObject obj)
    {
        for (int i = 0; i <= this.highestIndex; i++)
        {
            if (this.map[i] == obj)
            {
                this.removeObjectAtIndex(i);
                return;
            }
        }
    }

    public virtual int getFirstEmptyIndex()
    {
        for (int i = 0; i < this.size; i++)
        {
            if (this.map[i] == null)
            {
                return i;
            }
        }
        return this.size;
    }

    public virtual int getObjectIndex(NSObject obj)
    {
        for (int i = 0; i < this.size; i++)
        {
            if (this.map[i] == obj)
            {
                return i;
            }
        }
        return -1;
    }

    public override void dealloc()
    {
        for (int i = 0; i <= this.highestIndex; i++)
        {
            if (this.map[i] != null)
            {
                this.map[i].release();
                this.map[i] = null;
            }
        }
        NSObject.free(this.map);
        this.map = null;
        base.dealloc();
    }

    public const int DEFAULT_CAPACITY = 10;

    public NSObject[] map;

    public int size;

    public int highestIndex;

    public int overRealloc;

    public ulong mutationsCount;
}
