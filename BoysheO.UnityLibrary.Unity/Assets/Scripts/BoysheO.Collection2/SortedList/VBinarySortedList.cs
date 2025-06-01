using System;
using System.Collections.Generic;
using BoysheO.Collection;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    /// <summary>
    /// don't use again after disposable
    /// *特别注意，如果传入的Compare不能保证正确性(指根据compare降序排序后的列表，任意表中元素比它的前面的元素大，比它后面的元素小，如不满足此条件，则一定会出bug)，
    /// 则二分查找算法失效，此类将不能正常使用。
    /// *Notice This type can not work with incorrect comparer.There is no exception throw.
    /// </summary>
    public readonly struct VBinarySortedList<TK, TV> : IDisposable where TK : notnull
    {
        internal readonly long _order;
        internal readonly PBinarySortedList<TK, TV> _buffer;

        public PBinarySortedList<TK, TV> InternalBuffer
        {
            get
            {
                ThrowIfExpired();
                return _buffer;
            }
        }

        public bool IsAlive
        {
            get
            {
                return _order != 0 && _buffer != null &&
                       StrongOrderedPool<PBinarySortedList<TK, TV>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public int Count
        {
            get { return _buffer.Count; }
        }

        private VBinarySortedList(long order, PBinarySortedList<TK, TV> buffer)
        {
            _order = order;
            _buffer = buffer;
        }

        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Reset(null);
                _buffer.Dispose();
                StrongOrderedPool<PBinarySortedList<TK, TV>>.Share.Return(_buffer);
            }
        }

        public static VBinarySortedList<TK, TV> Rent(IComparer<TK>? comparer = null)
        {
            var ins = StrongOrderedPool<PBinarySortedList<TK, TV>>.Share.Rent(out var order);
            ins.Reset(comparer);
            return new VBinarySortedList<TK, TV>(order, ins);
        }

        private void ThrowIfExpired()
        {
            if (_buffer == null || _order == 0)
                throw new ObjectDisposedException("",
                    $"you must get this buffer using static method {nameof(VDictionary<TK, TV>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PBinarySortedList<TK, TV>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
    }
}