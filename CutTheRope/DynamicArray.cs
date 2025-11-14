using System;
using System.Collections;
using System.Collections.Generic;

internal sealed class DynamicArray<T> : IEnumerable<T>
{
    public T this[int key] => _map[key];

    public IEnumerator<T> GetEnumerator()
    {
        return new DynamicArrayEnumerator(_map, _highestIndex);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public DynamicArray()
    {
        _ = InitWithCapacityAndOverRealloc(10, 10);
    }

    public DynamicArray<T> InitWithCapacity(int capacity)
    {
        Capacity = capacity;
        _highestIndex = -1;
        _overRealloc = 0;
        _mutationsCount = 0;
        _map = new T[Capacity];
        return this;
    }

    public DynamicArray<T> InitWithCapacityAndOverRealloc(int capacity, int over)
    {
        _ = InitWithCapacity(capacity);
        _overRealloc = over;
        return this;
    }

    public int Count => _highestIndex + 1;

    public int Capacity { get; private set; }

    private void SetNewSize(int minRequired)
    {
        int newSize = minRequired + _overRealloc;
        T[] arr = new T[newSize];
        Array.Copy(_map, arr, Math.Min(_map.Length, arr.Length));
        _map = arr;
        Capacity = newSize;
    }

    public int AddObject(T obj)
    {
        int index = _highestIndex + 1;
        SetObjectAt(obj, index);
        return index;
    }

    public void SetObjectAt(T obj, int index)
    {
        if (index >= Capacity)
        {
            SetNewSize(index + 1);
        }

        _map[index] = obj;

        if (index > _highestIndex)
        {
            _highestIndex = index;
        }

        _mutationsCount++;
    }

    public T FirstObject()
    {
        return ObjectAtIndex(0);
    }

    public T LastObject()
    {
        return _highestIndex == -1 ? default : ObjectAtIndex(_highestIndex);
    }

    public T ObjectAtIndex(int index)
    {
        return _map[index];
    }

    public void UnsetAll()
    {
        for (int i = 0; i <= _highestIndex; i++)
        {
            _map[i] = default;
        }

        _mutationsCount++;
    }

    public void UnsetObjectAtIndex(int k)
    {
        _map[k] = default;
        _mutationsCount++;
    }

    public void InsertObjectAtIndex(T obj, int k)
    {
        if (k >= Capacity || _highestIndex + 1 >= Capacity)
        {
            SetNewSize(Capacity + 1);
        }

        _highestIndex++;

        for (int i = _highestIndex; i > k; i--)
        {
            _map[i] = _map[i - 1];
        }

        _map[k] = obj;
        _mutationsCount++;
    }

    public void RemoveObjectAtIndex(int index)
    {
        for (int i = index; i < _highestIndex; i++)
        {
            _map[i] = _map[i + 1];
        }

        _map[_highestIndex] = default;
        _highestIndex--;
        _mutationsCount++;
    }

    public void RemoveAllObjects()
    {
        UnsetAll();
        _highestIndex = -1;
    }

    public void RemoveObject(T obj)
    {
        for (int i = 0; i <= _highestIndex; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_map[i], obj))
            {
                RemoveObjectAtIndex(i);
                return;
            }
        }
    }

    public int GetFirstEmptyIndex()
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_map[i], default))
            {
                return i;
            }
        }
        return Capacity;
    }

    public int GetObjectIndex(T obj)
    {
        for (int i = 0; i <= _highestIndex; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_map[i], obj))
            {
                return i;
            }
        }
        return -1;
    }

    // --- Fields -----------------------------------------------------

    private T[] _map;
    private int _highestIndex;
    private int _overRealloc;
    private ulong _mutationsCount;

    // --- Enumerator --------------------------------------------------

    private sealed class DynamicArrayEnumerator(T[] map, int highestIndex) : IEnumerator<T>
    {
        private readonly T[] _map = map;
        private readonly int _maxIndex = highestIndex;
        private int _position = -1;

        public T Current => _position < 0 || _position > _maxIndex ? throw new InvalidOperationException() : _map[_position];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _position++;
            return _position <= _maxIndex;
        }

        public void Reset()
        {
            _position = -1;
        }

        public void Dispose()
        {
        }
    }
}
