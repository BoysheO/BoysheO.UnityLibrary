using System;
using System.Collections.Generic;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    public struct VOrderedSet<T>:IDisposable where T : notnull
    {
        private readonly long _order;
        private readonly POrderedSet<T> _buffer;

        private VOrderedSet(long order, POrderedSet<T> buffer)
        {
            _order = order;
            _buffer = buffer;
        }

        public bool IsAlive
        {
            get
            {
                return _order != 0 && _buffer != null && StrongOrderedPool<POrderedSet<T>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public POrderedSet<T> InternalBuffer
        {
            get
            {
                ThrowIfExpired();
                return _buffer;
            }
        }

        private void ThrowIfExpired()
        {
            if (_buffer == null || _order == 0)
                throw new ObjectDisposedException("",
                    $"you must get this buffer using static method {nameof(VOrderedSet<T>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<POrderedSet<T>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
        
        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Reset(null);
                _buffer.Dispose();
                StrongOrderedPool<POrderedSet<T>>.Share.Return(_buffer);
            }
        }

        public static VOrderedSet<T> Rent(IComparer<T>? comparer = null)
        {
            var ins = StrongOrderedPool<POrderedSet<T>>.Share.Rent(out var order);
            ins.Reset(comparer);
            return new VOrderedSet<T>(order, ins);
        }
    }
}