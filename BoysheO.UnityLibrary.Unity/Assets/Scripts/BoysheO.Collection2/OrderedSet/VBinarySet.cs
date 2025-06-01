using System;
using System.Collections.Generic;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    public readonly struct VBinarySet<T>:IDisposable where T : notnull
    {
        private readonly long _order;
        private readonly PBinarySet<T> _buffer;

        private VBinarySet(long order, PBinarySet<T> buffer)
        {
            _order = order;
            _buffer = buffer;
        }

        public bool IsAlive
        {
            get
            {
                return _order != 0 && _buffer != null && StrongOrderedPool<PBinarySet<T>>.Share.GetOrder(_buffer) == _order;
            }
        }

        public PBinarySet<T> InternalBuffer
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
                    $"you must get this buffer using static method {nameof(VBinarySet<T>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PBinarySet<T>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
        
        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Reset(null);
                _buffer.Dispose();
                StrongOrderedPool<PBinarySet<T>>.Share.Return(_buffer);
            }
        }

        public static VBinarySet<T> Rent(IComparer<T>? comparer = null)
        {
            var ins = StrongOrderedPool<PBinarySet<T>>.Share.Rent(out var order);
            ins.Reset(comparer);
            return new VBinarySet<T>(order, ins);
        }
    }
}