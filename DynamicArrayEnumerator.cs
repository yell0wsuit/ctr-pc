using CutTheRope.ios;
using System;
using System.Collections;

internal class DynamicArrayEnumerator : IEnumerator
{
    // (get) Token: 0x06000018 RID: 24 RVA: 0x00002436 File Offset: 0x00000636
    object IEnumerator.Current
    {
        get
        {
            return this.Current;
        }
    }

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

    public DynamicArrayEnumerator(NSObject[] list, int highestIndex)
    {
        this._map = list;
        this._highestIndex = highestIndex;
    }

    public bool MoveNext()
    {
        this.position++;
        return this.position < this._highestIndex + 1;
    }

    public void Reset()
    {
        this.position = -1;
    }

    public NSObject[] _map;

    private int _highestIndex;

    private int position = -1;
}
