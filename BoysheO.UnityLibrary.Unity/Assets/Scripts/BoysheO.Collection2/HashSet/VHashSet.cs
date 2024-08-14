using System;
using System.Collections.Generic;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    public readonly struct VHashSet<T>:IDisposable
    {
        private readonly long _order;
        private readonly PHashSet<T> _buffer;

        private VHashSet(long order, PHashSet<T> buffer)
        {
            _order = order;
            _buffer = buffer;
        }

        public bool IsAlive
        {
            get
            {
                return _order != 0 && _buffer != null && StrongOrderedPool<PHashSet<T>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public PHashSet<T> InternalBuffer
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
                    $"you must get this buffer using static method {nameof(VHashSet<T>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PHashSet<T>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
        
        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Reset(null);
                _buffer.Dispose();
                StrongOrderedPool<PHashSet<T>>.Share.Return(_buffer);
            }
        }

        public static VHashSet<T> Rent(IEqualityComparer<T>? equalityComparer = null)
        {
            var ins = StrongOrderedPool<PHashSet<T>>.Share.Rent(out var order);
            ins.Reset(equalityComparer);
            return new VHashSet<T>(order, ins);
        }
    }
}