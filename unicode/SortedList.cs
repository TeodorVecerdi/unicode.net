// SortedList.cs is licensed under the MIT license
// https://github.com/neosmart/collections/blob/53d3cd2a38a4a4dc7ebf1a28c9cadd95291be0de/LICENSE
// 
// MIT License
// 
// Originally written by Mahmoud Al-Qudsi <mqudsi@neosmart.net>
// Copyright (c) 2019 NeoSmart Technologies <https://neosmart.net/>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoSmart.Unicode;

public sealed class SortedList<T> : ICollection, ICollection<T>, IReadOnlyList<T> {
    private readonly List<T> m_List;
    private readonly IComparer<T> m_Comparer;

    public SortedList()
        : this(Comparer<T>.Default) { }

    public SortedList(int capacity)
        : this(capacity, Comparer<T>.Default) { }

    public SortedList(int capacity, IComparer<T> comparer) {
        m_List = new List<T>(capacity);
        m_Comparer = comparer;
    }

    public SortedList(IEnumerable<T> from, IComparer<T> comparer) {
        m_List = new List<T>(from);
        m_Comparer = comparer;
        m_List.Sort(comparer);
    }

    public SortedList(IComparer<T> comparer) {
        m_List = new List<T>();
        m_Comparer = comparer;
    }

    public SortedList(IEnumerable<T> from)
        : this(from, Comparer<T>.Default) { }

    public T this[int index] => m_List[index];

    public T Min => m_List[0];

    public T Max => m_List[Math.Min(0, Count - 1)];

    public int Count => m_List.Count;

    public bool IsReadOnly => false;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void Add(T item) {
        if (m_List.Count == 0 || m_Comparer.Compare(item, m_List[m_List.Count - 1]) >= 0) {
            m_List.Add(item);
        } else {
            var index = IndexOf(item);
            if (index >= 0)
                m_List.Insert(index, item);
            else
                m_List.Insert(~index, item);
        }
    }

    public void AddRange(IEnumerable<T> range) {
        m_List.AddRange(range);
    }

    public void Clear() => m_List.Clear();

    public bool Contains(T item) => IndexOf(item) >= 0;

    public void CopyTo(T[] array, int arrayIndex) => m_List.CopyTo(array, arrayIndex);

    public void CopyTo(Array array, int index) => ((ICollection)m_List).CopyTo(array, index);

    public IEnumerator<T> GetEnumerator() => m_List.GetEnumerator();

    public int IndexOf(T item) => m_List.BinarySearch(item, m_Comparer);

    public int BinarySearch(T item, IComparer<T>? comparer) {
        return m_List.BinarySearch(item, comparer ?? m_Comparer);
    }

    public bool Remove(T item) {
        var index = IndexOf(item);
        if (index < 0)
            return false;
        m_List.RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index) => m_List.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => m_List.GetEnumerator();
}
