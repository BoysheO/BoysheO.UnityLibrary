using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BoysheO.Collection;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    /// <summary>
    /// don't use again after disposable
    /// *由于这个类使用频率特别高，所以它的API也特别丰富
    /// *遇到需要高性能的情况，请直接使用InternalBuffer以跳过生命周期检查
    /// *老版本继承了IList等接口，容易导致意外的装箱，这个版本不再实现该接口，请直接使用InternalBuffer
    /// </summary>
    public readonly struct VList<T> : IDisposable
    {
        private readonly long _order;
        private readonly PList<T> _buffer;

        private VList(long order, PList<T> buffer)
        {
            _order = order;
            _buffer = buffer;
        }

        /// <summary>
        /// NoBoxing IReadOnlyList(but call IEnumerable.GetEnumerator() will boxing the internal enumerator)
        /// *DON'T keep the instance returned,using the instance after buff disposed cause undefined behavior* 
        /// </summary>
        public PList<T> InternalBuffer
        {
            get
            {
                ThrowIfOrderExpired();
                return _buffer;
            }
        }

        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Dispose();
                StrongOrderedPool<PList<T>>.Share.Return(_buffer);
            }
        }

        /// <summary>
        /// 尽管提供了IsAlive判断，但是使用者应当在业务流程上确保Buff生命周期正确且单一，而不是频繁使用IsAlive判定。频繁使用IsAlive违背本库设计初衷
        /// Although IsAlive verdicts are provided, users should ensure that the buff lifetime is correct and single in the business process, rather than using IsAlive verdicts frequently. Frequent use of IsAlive is contrary to the original design intent of this library
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return _buffer != null && _order != 0 &&
                       StrongOrderedPool<PList<T>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public static VList<T> Rent()
        {
            var ins = StrongOrderedPool<PList<T>>.Share.Rent(out var version);
            return new VList<T>(version, ins);
        }

        private void ThrowIfOrderExpired()
        {
            if (_buffer == null || _order == 0)
                throw new ObjectDisposedException("",
                    $"you must get this buffer using static method {nameof(VList<T>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PList<T>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }

        public void Push(T item)
        {
            ThrowIfOrderExpired();
            _buffer.Push(item);
        }

        public bool TryPop([MaybeNullWhen(false)]out T item)
        {
            ThrowIfOrderExpired();
            return _buffer.TryPop(out item);
        }

        public bool TryPeek([MaybeNullWhen(false)]out T item)
        {
            ThrowIfOrderExpired();
            return _buffer.TryPeek(out item);
        }

        public void RemoveLast()
        {
            ThrowIfOrderExpired();
            var lst = _buffer;
            lst.RemoveAt(lst.Count - 1);
        }

        /// <summary>
        /// Adds the elements of the given array to the end of this list. If
        /// required, the capacity of the list is increased to twice the previous
        /// capacity or the new size, whichever is larger.
        /// </summary>
        public VList<T> AddRange(T[] array)
        {
            ThrowIfOrderExpired();
            _buffer.AddRange(array.AsSpan());
            return this;
        }

        /// <summary>
        /// design for ArrayPool&lt;T&gt;
        /// </summary>
        public VList<T> AddRange(T[] ary, int count)
        {
            ThrowIfOrderExpired();
            _buffer.AddRange(ary.AsSpan(0, count));
            return this;
        }

        public VList<T> AddRange(ReadOnlySpan<T> span)
        {
            ThrowIfOrderExpired();
            _buffer.AddRange(span);
            return this;
        }

        public Span<T> Span
        {
            get
            {
                ThrowIfOrderExpired();
                return _buffer.Span;
            }
        }

        /// <summary>
        /// Sorts the elements in this list.  Uses the default comparer and
        /// Array.Sort.
        /// </summary>
        public VList<T> Sort()
        {
            ThrowIfOrderExpired();
            _buffer.Sort();
            return this;
        }

        /// <summary>
        /// Sorts the elements in this list.  Uses Array.Sort with the
        /// provided comparer.
        /// </summary>
        /// <param name="comparer"></param>
        public VList<T> Sort(IComparer<T> comparer)
        {
            ThrowIfOrderExpired();
            _buffer.Sort(comparer);
            return this;
        }

        /// <summary>
        /// Sorts the elements in a section of this list. The sort compares the
        /// elements to each other using the given IComparer interface. If
        /// comparer is null, the elements are compared to each other using
        /// the IComparable interface, which in that case must be implemented by all
        /// elements of the list.
        /// 
        /// This method uses the Array.Sort method to sort the elements.
        /// </summary>
        public VList<T> Sort(int index, int count, IComparer<T> comparer)
        {
            ThrowIfOrderExpired();
            _buffer.Sort(index, count, comparer);
            return this;
        }

        public VList<T> Sort(Func<T, T, int> comparison)
        {
            ThrowIfOrderExpired();
            var com = Unsafe.As<Func<T, T, int>, Comparison<T>>(ref comparison);
            _buffer.Sort(com);
            return this;
        }

        public bool All(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.All(predicate);
        }

        public bool Any(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.Any(predicate);
        }

        public T First()
        {
            ThrowIfOrderExpired();
            return _buffer.First();
        }

        public T First(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.First(predicate);
        }

        public T? FirstOrDefault()
        {
            ThrowIfOrderExpired();
            return _buffer.FirstOrDefault();
        }

        public T? FirstOrDefault(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.FirstOrDefault(predicate);
        }

        public T Last()
        {
            ThrowIfOrderExpired();
            return _buffer.Last();
        }

        public T Last(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.Last(predicate);
        }

        public T? LastOrDefault()
        {
            ThrowIfOrderExpired();
            return _buffer.LastOrDefault();
        }

        public T? LastOrDefault(Func<T, bool> predicate)
        {
            ThrowIfOrderExpired();
            return _buffer.LastOrDefault(predicate);
        }

        public VList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
        {
            ThrowIfOrderExpired();
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            var res = VList<TOutput>.Rent();
            for (int i = 0, count = _buffer.Count; i < count; i++)
            {
                res[i] = converter(_buffer[i]);
            }

            return res;
        }

        /// <summary>
        /// ToArray returns an array containing the contents of the List.
        /// This requires copying the List, which is an O(n) operation.
        /// </summary>
        public T[] ToArray()
        {
            ThrowIfOrderExpired();
            return _buffer.ToArray();
        }

        public PList<T>.Enumerator GetEnumerator()
        {
            ThrowIfOrderExpired();
            return _buffer.GetEnumerator();
        }

        #region interface
        

        public void Add(T item)
        {
            ThrowIfOrderExpired();
            _buffer.Add(item);
        }

        public void Clear()
        {
            ThrowIfOrderExpired();
            _buffer.Clear();
        }


        public bool Contains(T item)
        {
            ThrowIfOrderExpired();
            return _buffer.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowIfOrderExpired();
            _buffer.Span.CopyTo(array.AsSpan(arrayIndex));
        }

        public bool Remove(T item)
        {
            ThrowIfOrderExpired();
            return _buffer.Remove(item);
        }


        public int Count
        {
            get
            {
                ThrowIfOrderExpired();
                return ((ICollection) _buffer).Count;
            }
        }


        public int IndexOf(T item)
        {
            ThrowIfOrderExpired();
            return _buffer.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ThrowIfOrderExpired();
            _buffer.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ThrowIfOrderExpired();
            ((IList<T>) _buffer).RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                ThrowIfOrderExpired();
                return _buffer[index];
            }
            set
            {
                ThrowIfOrderExpired();
                _buffer[index] = value;
            }
        }

        #endregion
    }
}