using System;
using System.Collections;
using CutTheRope.ios;

// Token: 0x02000002 RID: 2
internal class DynamicArray : NSObject, IEnumerable
{
	// Token: 0x17000001 RID: 1
	public NSObject this[int key]
	{
		get
		{
			return this.map[key];
		}
	}

	// Token: 0x06000002 RID: 2 RVA: 0x0000205A File Offset: 0x0000025A
	public IEnumerator GetEnumerator()
	{
		return new DynamicArrayEnumerator(this.map, this.highestIndex);
	}

	// Token: 0x06000003 RID: 3 RVA: 0x0000206D File Offset: 0x0000026D
	public override NSObject init()
	{
		this.initWithCapacityandOverReallocValue(10, 10);
		return this;
	}

	// Token: 0x06000004 RID: 4 RVA: 0x0000207B File Offset: 0x0000027B
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

	// Token: 0x06000005 RID: 5 RVA: 0x000020B4 File Offset: 0x000002B4
	public virtual NSObject initWithCapacityandOverReallocValue(int c, int v)
	{
		if (this.initWithCapacity(c) != null)
		{
			this.overRealloc = v;
		}
		return this;
	}

	// Token: 0x06000006 RID: 6 RVA: 0x000020C7 File Offset: 0x000002C7
	public virtual int count()
	{
		return this.highestIndex + 1;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000020D1 File Offset: 0x000002D1
	public virtual int capacity()
	{
		return this.size;
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000020DC File Offset: 0x000002DC
	public virtual void setNewSize(int k)
	{
		int num = k + this.overRealloc;
		NSObject[] array = new NSObject[num];
		Array.Copy(this.map, array, Math.Min(this.map.Length, array.Length));
		this.map = array;
		this.size = num;
	}

	// Token: 0x06000009 RID: 9 RVA: 0x00002124 File Offset: 0x00000324
	public virtual int addObject(NSObject obj)
	{
		int num = this.highestIndex + 1;
		this.setObjectAt(obj, num);
		return num;
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002144 File Offset: 0x00000344
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

	// Token: 0x0600000B RID: 11 RVA: 0x000021B8 File Offset: 0x000003B8
	public virtual NSObject firstObject()
	{
		return this.objectAtIndex(0);
	}

	// Token: 0x0600000C RID: 12 RVA: 0x000021C1 File Offset: 0x000003C1
	public virtual NSObject lastObject()
	{
		if (this.highestIndex == -1)
		{
			return null;
		}
		return this.objectAtIndex(this.highestIndex);
	}

	// Token: 0x0600000D RID: 13 RVA: 0x000021DA File Offset: 0x000003DA
	public virtual NSObject objectAtIndex(int k)
	{
		return this.map[k];
	}

	// Token: 0x0600000E RID: 14 RVA: 0x000021E4 File Offset: 0x000003E4
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

	// Token: 0x0600000F RID: 15 RVA: 0x00002213 File Offset: 0x00000413
	public virtual void unsetObjectAtIndex(int k)
	{
		this.map[k].release();
		this.map[k] = null;
		this.mutationsCount += 1UL;
	}

	// Token: 0x06000010 RID: 16 RVA: 0x0000223C File Offset: 0x0000043C
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

	// Token: 0x06000011 RID: 17 RVA: 0x000022C8 File Offset: 0x000004C8
	public virtual void removeObjectAtIndex(int k)
	{
		NSObject nSObject = this.map[k];
		if (nSObject != null)
		{
			nSObject.release();
		}
		for (int i = k; i < this.highestIndex; i++)
		{
			this.map[i] = this.map[i + 1];
		}
		this.map[this.highestIndex] = null;
		this.highestIndex--;
		this.mutationsCount += 1UL;
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002337 File Offset: 0x00000537
	public virtual void removeAllObjects()
	{
		this.unsetAll();
		this.highestIndex = -1;
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002348 File Offset: 0x00000548
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

	// Token: 0x06000014 RID: 20 RVA: 0x0000237C File Offset: 0x0000057C
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

	// Token: 0x06000015 RID: 21 RVA: 0x000023AC File Offset: 0x000005AC
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

	// Token: 0x06000016 RID: 22 RVA: 0x000023D8 File Offset: 0x000005D8
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

	// Token: 0x04000001 RID: 1
	public const int DEFAULT_CAPACITY = 10;

	// Token: 0x04000002 RID: 2
	public NSObject[] map;

	// Token: 0x04000003 RID: 3
	public int size;

	// Token: 0x04000004 RID: 4
	public int highestIndex;

	// Token: 0x04000005 RID: 5
	public int overRealloc;

	// Token: 0x04000006 RID: 6
	public ulong mutationsCount;
}
