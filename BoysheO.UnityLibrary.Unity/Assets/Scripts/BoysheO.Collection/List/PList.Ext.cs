using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BoysheO.Collection;

namespace BoysheO.Collection
{
    // 拓展额外的成员
    // 小部分逻辑和原生不同：
    // 1.容量设置什么值，只能传入参考值。实际容量取决于ArrayPool的实现。
    // 由于无法确定ArrayPool返回数组的容量变化（即使是默认ArrayPool也是按平台给容量的）。因此无法做到最优解。
    // 2.不支持以IEnumerable初始化。减少代码维护量。相关优化迁移到AddRange去
    //*参考源码CLR 8.0.5
    public partial class PList<T> : IDisposable, ISpanCollection<T>
    {
        /// <summary>
        /// *即用即弃
        /// </summary>
        public Span<T> Span => _items.AsSpan(0, _size);

        /// <summary>
        /// *即用即弃
        /// </summary>
        public ReadOnlySpan<T> ReadOnlySpan => _items.AsSpan(0, _size);

        public T[] GetInternalArray() => _items;

        //公开Version属性，便于外部判断列表是否发生了变动
        public int Version => _version;

        //这个Length是为了方便同步替换数组实现时，不用将Length改为Count
        public int Length
        {
            get { return _size; }
        }

        public void AddRange(ReadOnlySpan<T> span)
        {
            _version++;
            int count = span.Length;
            if (count > 0)
            {
                if (_items.Length - _size < count)
                {
                    Grow(checked(_size + count));
                }

                span.CopyTo(_items.AsSpan(_size, count));
                _size += count;
            }
        }

        public Span<T> AsSpan(int start, int count)
        {
            if (_size - start < count)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            return _items.AsSpan(start, count);
        }

        //尽管通过先获取span再获取ref也是ok的。这里提供一个方便函数
        public ref T GetRefValue(int index)
        {
            if (index < 0) ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (index >= _size) ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
            return ref _items[index];
        }

        public Span<T> AppendSpan(int count)
        {
            if (count <= 0) ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            _version++;
            var afterSize = _size + count;
            if (afterSize > _items.Length)
            {
                Grow(afterSize);
            }

            var span = _items.AsSpan(_size, count);
            span.Clear();
            _size += count;
            return span;
        }

        #region help use list as stack

        public bool TryPop([MaybeNullWhen(false)]out T item)
        {
            if (_size > 0)
            {
                item = _items[_size - 1];
                _size--;
                return true;
            }

            item = default;
            return false;
        }

        public void Push(T item)
        {
            Add(item);
        }

        public bool TryPeek([MaybeNullWhen(false)]out T item)
        {
            if (_size > 0)
            {
                item = _items[_size - 1];
                return true;
            }

            item = default;
            return false;
        }

        #endregion

        public void Sort(int index, int count, Comparison<T> comparison)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            }

            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size - index < count)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

            if (comparison == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
            }

            if (_size > 1)
            {
                ArraySortHelper.Sort(new Span<T>(_items, index, count), comparison);
            }

            _version++;
        }

        /// <summary>
        /// 特别说明：即使dispose，也可以继续使用。dispose可以归还数组
        /// </summary>
        public void Dispose()
        {
            Clear();
            _pool.Return(_items);
            _items = _pool.Rent(0);
        }

        public bool All(Func<T, bool> predicate)
        {
            return TrueForAll(predicate);
        }

        public bool Any(Func<T, bool> predicate)
        {
            for (var index = 0; index < _size; index++)
            {
                var item = _items[index];
                if (predicate(item)) return true;
            }

            return false;
        }

        public bool TryFind(Func<T, bool> match, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    value = _items[i];
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryFindLast(Func<T, bool> match, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = _size - 1; i >= 0; i--)
            {
                if (match(_items[i]))
                {
                    value = _items[i];
                    return true;
                }
            }

            value = default;
            return false;
        }

        public T First()
        {
            if (_size == 0) ThrowHelper.ThrowNoElementsException();
            return _items[0];
        }

        public T First(Func<T, bool> predicate)
        {
            if (!TryFind(predicate, out var v))
            {
                ThrowHelper.ThrowNoMatchException();
            }

            return v;
        }

        public T? FirstOrDefault()
        {
            if (_size == 0) return default;
            return _items[0];
        }

        public T? FirstOrDefault(Func<T, bool> predicate)
        {
            return Find(predicate);
        }

        public T Last()
        {
            if (_size == 0) ThrowHelper.ThrowNoElementsException();
            return _items[^1];
        }

        public T Last(Func<T, bool> predicate)
        {
            if (!TryFindLast(predicate, out T? t))
            {
                ThrowHelper.ThrowNoMatchException();
            }

            return t;
        }

        public T? LastOrDefault()
        {
            return _size == 0 ? default : _items[^1];
        }

        public T? LastOrDefault(Func<T, bool> predicate)
        {
            return FindLast(predicate);
        }

        /// <summary>
        /// remove duplicates by loop.element order is not changed.
        /// O(n*n)
        /// 建议：如果可以，使用Set系列而不是列表
        /// </summary>
        public void RemoveDuplicates(Func<T, T, bool> comparer)
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = i + 1; j < _size; j++)
                {
                    if (comparer(_items[i], _items[j]))
                    {
                        RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// remove duplicates by loop.element order is not changed.
        /// O(n*n)
        /// 建议：如果可以，使用Set系列而不是列表
        /// </summary>
        public void RemoveDuplicates(IEqualityComparer<T> comparer)
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = i + 1; j < _size; j++)
                {
                    if (comparer.Equals(_items[i], _items[j]))
                    {
                        RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// 如果列表是排序过的，并且排序时使用相同的comparer，那么使用此方法移除相同项会更快
        /// O(n)
        /// </summary>
        public void RemoveDuplicatesBaseSorted(Comparison<T> comparer)
        {
            int j = 0; // Index of the last unique element
            var span = Span;
            for (int i = 1; i < _size; i++)
            {
                if (comparer(span[i], span[j]) != 0)
                {
                    j++;
                    span[j] = span[i];
                }
            }

            _size = j + 1;
        }

        /// <summary>
        /// 如果列表是排序过的，并且排序时使用相同的comparer，那么使用此方法移除相同项会更快
        /// O(n)
        /// </summary>
        public void RemoveDuplicatesBaseSorted(Func<T, T, bool> comparer)
        {
            int j = 0; // Index of the last unique element
            var span = Span;
            for (int i = 1; i < _size; i++)
            {
                if (!comparer(span[i], span[j]))
                {
                    j++;
                    span[j] = span[i];
                }
            }

            _size = j + 1;
        }
    }
}