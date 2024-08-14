using System;
using System.Collections.Generic;
using BoysheO.Collection;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    /// <summary>
    /// don't use again after disposable
    /// </summary>
    public readonly struct VSortedList<TK, TV> : IDisposable where TK : notnull
    {
        internal readonly long _order;
        internal readonly PSortedList<TK, TV> _buffer;

        public PSortedList<TK, TV> InternalBuffer
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
                       StrongOrderedPool<PSortedList<TK, TV>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public int Count
        {
            get { return _buffer.Count; }
        }

        private VSortedList(long order, PSortedList<TK, TV> buffer)
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
                StrongOrderedPool<PSortedList<TK, TV>>.Share.Return(_buffer);
            }
        }

        public static VSortedList<TK, TV> Rent(IComparer<TK>? comparer = null)
        {
            var ins = StrongOrderedPool<PSortedList<TK, TV>>.Share.Rent(out var order);
            ins.Reset(comparer);
            return new VSortedList<TK, TV>(order, ins);
        }

        private void ThrowIfExpired()
        {
            if (_buffer == null || _order == 0)
                throw new ObjectDisposedException("",
                    $"you must get this buffer using static method {nameof(VDictionary<TK, TV>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PSortedList<TK, TV>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
    }
}