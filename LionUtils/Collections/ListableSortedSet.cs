using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace LionUtils.Collections
{
    /// <summary>
    /// Implementation for a sorted collection that hast properties of both a list and a set. 
    /// </summary>
    /// <typeparam name="T">BaseType that should be used in the Set.</typeparam>
    [DebuggerDisplay("Count = {nameof(Count)}")]
    public class ListableSortedSet<T> : IList<T>, ISet<T>, IReadOnlyList<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly HashSet<T> _entries;
        private T[] _elements;

        #region Constuctors

        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public ListableSortedSet() : this(1, null)
        {
        }

        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        /// <param name="comparer">Custom comparer used for sorting.</param>
        public ListableSortedSet(IComparer<T> comparer) : this(1, comparer)
        {
        }

        /// <summary>
        /// Creates an empty collection. 
        /// </summary>
        /// <param name="count">Starting size of the collection.</param>
        public ListableSortedSet(int count) : this(count, null)
        {
        }

        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        /// <param name="comparer">Custom comparer used for sorting.</param>
        /// <param name="count">Starting size of the collection.</param>
        public ListableSortedSet(int count, IComparer<T> comparer)
        {
            FindNextPowerOf2(ref count);
            _elements = new T[count];
            _comparer = comparer;
            _entries = new HashSet<T>();
        }

        #endregion Constructors

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


        public void AddRange(ICollection<T> items)
        {
            foreach (var item in items) InnerAdd(item, false);

            Array.Sort(_elements, 0, Count, _comparer);
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            CopyTo(result, 0);
            return result;
        }

        private bool InnerAdd(T item, bool sort)
        {
            if (_entries.Contains(item)) return false;

            if (Count + 1 >= _elements.Length)
            {
                var newArray = new T[_elements.Length * 2];
                Array.Copy(_elements, newArray, _elements.Length);
                _elements = newArray;
            }

            _elements[Count] = item;
            _entries.Add(item);
            if (sort) Array.Sort(_elements, 0, Count, _comparer);

            return true;
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

        #region IListBehavior

        
        /// <inheritdoc cref="IList.this"/>
        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException(
                "Setting an indexed value is not supported for this Set! Use Add() to add values to the Collection.");
        }

        /// <inheritdoc cref="IList{T}.IndexOf"/>
        public int IndexOf(T item)
        {
            for (var i = 0; i < Count; i++)
                if (Equals(_elements[i], item))
                    return i;

            return -1;
        }

        /// <inheritdoc cref="IList{T}.Insert"/>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException(
                "Setting an indexed value is not supported for this Set, since it is sorted! " +
                "Use Add() to add values to the Collection.");
        }


        /// <inheritdoc cref="IList{T}.this"/>
        public T this[int index]
        {
            get
            {
                if (index < Count) return _elements[index];
                throw new IndexOutOfRangeException();
            }
            set => throw new NotSupportedException(
                "Setting an indexed value is not supported for this Set! Use Add() to add values to the Collection.");
        }

        /// <inheritdoc cref="IList{T}.RemoveAt"/>
        public void RemoveAt(int index)
        {
            Remove(_elements[index]);
        }

        #endregion


        #region Set Behavior

        /// <inheritdoc cref="ISet{T}.ExceptWith"/>
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var t in this)
            {
                var enumerable = other as T[] ?? other.ToArray();
                if (enumerable.Contains(t))
                {
                    Remove(t);
                }
            }
        }

        /// <inheritdoc cref="ISet{T}.IntersectWith"/>
        public void IntersectWith(IEnumerable<T> other)
        {
            foreach (var t in this)
            {
                var enumerable = other as T[] ?? other.ToArray();
                if (!enumerable.Contains(t))
                {
                    Remove(t);
                }
            }
        }

        /// <inheritdoc cref="ISet{T}.IsProperSubsetOf"/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _entries.IsProperSubsetOf(other);
        }

        /// <inheritdoc cref="ISet{T}.IsProperSupersetOf"/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _entries.IsProperSupersetOf(other);
        }

        /// <inheritdoc cref="ISet{T}.IsSubsetOf"/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _entries.IsSubsetOf(other);
        }

        /// <inheritdoc cref="ISet{T}.IsSupersetOf"/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _entries.IsSupersetOf(other);
        }
        /// <inheritdoc cref="ISet{T}.Overlaps"/>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _entries.Overlaps(other);
        }

        /// <inheritdoc cref="ISet{T}.SetEquals"/>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _entries.SetEquals(other);
        }

        /// <inheritdoc cref="ISet{T}.SymmetricExceptWith"/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            bool changed = false;
            var enumerable = other as T[] ?? other.ToArray();
            for (int i = 0; i < enumerable.Count(); i++)
            {
                T t = enumerable[i];
                if (Contains(t))
                {
                    Remove(t);
                    changed = true;
                }
                else
                {
                    InnerAdd(t, false);
                    changed = true;
                }
            }

            if (changed)
                Array.Sort(_elements, 0, Count, _comparer);
        }


        /// <inheritdoc cref="ISet{T}.UnionWith"/>
        public void UnionWith(IEnumerable<T> other)
        {
            bool changed = false;

            //adds everything and manually sorts afterwards.
            foreach (var t in other)
            {
                changed |= InnerAdd(t, false);
            }

            if (changed)
                Array.Sort(_elements, 0, Count, _comparer);
        }


        /// <inheritdoc cref="ISet{T}.Add"/>
        bool ISet<T>.Add(T item)
        {
            return InnerAdd(item, true);
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}