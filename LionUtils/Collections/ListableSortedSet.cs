using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LionUtils.Collections
{
    public class ListableSortedSet<T> : IList<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly HashSet<T> _entries;
        private T[] _elements;

        public ListableSortedSet() : this(1, null)
        {
        }

        public ListableSortedSet(IComparer<T> comparer) : this(1, comparer)
        {
        }

        public ListableSortedSet(int count) : this(count, null)
        {
        }

        public ListableSortedSet(int count, IComparer<T> comparer)
        {
            FindNextPowerOf2(ref count);
            _elements = new T[count];
            _comparer = comparer;
            _entries = new HashSet<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumer = new T[Count];
            Array.Copy(_elements, enumer, Count);
            return enumer.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            InnerAdd(item, true);
        }

        public void Clear()
        {
            _elements = new T[4];
            _entries.Clear();
        }

        public bool Contains(T item)
        {
            return _entries.Contains(item);
        }

        /// <inheritdoc cref="ICollection.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < Count; i++) array[i - arrayIndex] = _elements[i];
        }

        public bool Remove(T item)
        {
            _elements = _elements.Where(val => !Equals(val, item)).ToArray();
            return _entries.Remove(item);
        }

        public int Count => _entries.Count;
        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            for (var i = 0; i < Count; i++)
                if (Equals(_elements[i], item))
                    return i;

            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException(
                "Setting an indexed value is not supported for this Set, since it is sorted! " +
                "Use Add() to add values to the Collection.");
        }

        public void RemoveAt(int index)
        {
            Remove(_elements[index]);
        }

        public T this[int index] {
            get {
                if (index < Count) return _elements[index];
                throw new IndexOutOfRangeException();
            }
            set => throw new NotSupportedException(
                "Setting an indexed value is not supported for this Set! Use Add() to add values to the Collection.");
        }

        private void FindNextPowerOf2(ref int number)
        {
            var shift = 0;
            while (number != 0)
            {
                number = number >> 1;
                shift++;
            }

            number += 1;
            number = number << shift;
        }

        public void AddRange(ICollection<T> items)
        {
            foreach (var item in items) InnerAdd(item, false);

            Array.Sort(_elements, 0, Count, _comparer);
        }

        private void InnerAdd(T item, bool sort)
        {
            if (_entries.Contains(item)) return;

            if (Count + 1 >= _elements.Length)
            {
                var newArray = new T[_elements.Length * 2];
                Array.Copy(_elements, newArray, _elements.Length);
                _elements = newArray;
            }

            _elements[Count] = item;
            _entries.Add(item);
            if (sort) Array.Sort(_elements, 0, Count, _comparer);
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            CopyTo(result, 0);
            return result;
        }
    }
}