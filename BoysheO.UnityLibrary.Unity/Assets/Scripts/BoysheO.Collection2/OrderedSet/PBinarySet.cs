using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BoysheO.Collection;

namespace BoysheO.Collection2
{
    /// <summary>
    /// *特别注意，如果传入的Compare不能保证正确性(指根据compare降序排序后的列表，任意表中元素比它的前面的元素大，比它后面的元素小，如不满足此条件，则一定会出bug)，
    /// 则二分查找算法失效，此类将不能正常使用。
    /// *Notice This type can not work with incorrect comparer.There is no exception throw.
    /// </summary>
    public class PBinarySet<T> : ISet<T>, IDisposable, IReadOnlyOrderedSet<T> where T : notnull
    {
        private readonly PBinarySortedList<T, ValueTuple> _data;

        /// <summary>
        /// 请即用即弃，对集合的操作可能会替换它的引用。（即使源码中没有这样做，也不保证以后源码不重构）
        /// </summary>
        public PBinarySortedList<T, ValueTuple> InternalData => _data;

#if NETSTANDARD2_0
#else
        [NotNull]
#endif
        public IComparer<T> Comparer => _data.Comparer;
#pragma warning disable CS3003
        public IReadOnlySpanCollection<T> AsReadOnlySpanCollection
        {
            get { return _data.InternalKey; }
        }
#pragma warning restore CS3003
        public IReadOnlyList<T> AsReadOnlyList => _data.InternalKey;

        ISet<T> IReadOnlyOrderedSet<T>.AsSet
        {
            get { return this; }
        }

        public PBinarySet() : this(null)
        {
        }

        public PBinarySet(IComparer<T>? comparer = null)
        {
            _data = new PBinarySortedList<T, ValueTuple>(comparer);
        }

        public PList<T>.Enumerator GetEnumerator()
        {
            return _data.InternalKey.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(ReadOnlySpan<T> item)
        {
            foreach (var x1 in item)
            {
                _data.TryAdd(x1, default);
            }
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            if (enumerable is IReadOnlyList<T> l1)
            {
                for (var index = 0; index < l1.Count; index++)
                {
                    var x1 = l1[index];
                    _data.TryAdd(x1, default);
                }
            }
            else if (enumerable is IList<T> l2)
            {
                for (var index = 0; index < l2.Count; index++)
                {
                    var x1 = l2[index];
                    _data.TryAdd(x1, default);
                }
            }
            else
            {
                foreach (var x1 in enumerable)
                {
                    _data.TryAdd(x1, default);
                }
            }
        }

        void ICollection<T>.Add(T item)
        {
            _data.TryAdd(item, default);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var x1 in other)
            {
                _data.Remove(x1);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            using var intersected = VBinarySet<T>.Rent(_data.Comparer);
            var buffer = intersected.InternalBuffer;
            foreach (var item in other)
            {
                if (Contains(item))
                {
                    buffer.Add(item);
                }
            }

            _data.Clear();
            foreach (var x1 in buffer)
            {
                _data.Add(x1, default);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            using var buffer = VBinarySet<T>.Rent(_data.Comparer);
            var otherSet = buffer.InternalBuffer;
            otherSet.UnionWith(other);
            return IsSubsetOf(otherSet) && Count < otherSet.Count;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return IsSupersetOf(other) && Count > other.Count();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            using var buffer = VBinarySet<T>.Rent(_data.Comparer);
            var otherSet = buffer.InternalBuffer;
            otherSet.UnionWith(other);
            return this.All(otherSet.Contains);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return other.All(Contains);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Any(Contains);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            using var buffer = VBinarySet<T>.Rent(_data.Comparer);
            var otherSet = buffer.InternalBuffer;
            otherSet.UnionWith(other);
            return Count == otherSet.Count && this.All(otherSet.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            using var buffer = VBinarySet<T>.Rent(_data.Comparer);
            var otherSet = buffer.InternalBuffer;
            using var buffer2 = VList<T>.Rent();
            var toAdd = buffer2.InternalBuffer;

            foreach (var item in otherSet)
            {
                if (!_data.ContainsKey(item))
                {
                    toAdd.Add(item);
                }
                else
                {
                    _data.Remove(item);
                }
            }

            foreach (var item in toAdd)
            {
                _data[item] = default;
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                _data[item] = default;
            }
        }

        public bool Add(T item)
        {
            return _data.TryAdd(item, default);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(T item)
        {
            return _data.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _data.InternalKey.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _data.Remove(item);
        }

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public void Dispose()
        {
            _data.Dispose();
        }

        public void Reset(IComparer<T>? equalityComparer)
        {
            _data.Reset(equalityComparer);
        }
    }
}