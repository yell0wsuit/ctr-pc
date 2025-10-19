using System;
using System.Collections;
using CutTheRope.ios;

// Token: 0x02000003 RID: 3
internal class DynamicArrayEnumerator : IEnumerator
{
	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000018 RID: 24 RVA: 0x00002436 File Offset: 0x00000636
	object IEnumerator.Current
	{
		get
		{
			return this.Current;
		}
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000019 RID: 25 RVA: 0x00002440 File Offset: 0x00000640
	public NSObject Current
	{
		get
		{
			NSObject nsobject;
			try
			{
				nsobject = this._map[this.position];
			}
			catch (IndexOutOfRangeException)
			{
				throw new InvalidOperationException();
			}
			return nsobject;
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00002478 File Offset: 0x00000678
	public DynamicArrayEnumerator(NSObject[] list, int highestIndex)
	{
		this._map = list;
		this._highestIndex = highestIndex;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002495 File Offset: 0x00000695
	public bool MoveNext()
	{
		this.position++;
		return this.position < this._highestIndex + 1;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x000024B5 File Offset: 0x000006B5
	public void Reset()
	{
		this.position = -1;
	}

	// Token: 0x04000007 RID: 7
	public NSObject[] _map;

	// Token: 0x04000008 RID: 8
	private int _highestIndex;

	// Token: 0x04000009 RID: 9
	private int position = -1;
}
