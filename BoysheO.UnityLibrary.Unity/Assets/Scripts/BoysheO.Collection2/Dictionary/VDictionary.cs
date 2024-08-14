using System;
using System.Collections.Generic;
using BoysheO.Collection;
using BoysheO.ObjectPool;

namespace BoysheO.Collection2
{
    /// <summary>
    /// don't use again after disposable
    /// *由于字典接口的特殊性，如果直接继承在PooledDictionaryBuffer上造出Dictionary的接口，使用时就很难避免产生gc，这和设计初衷相悖
    /// *所以请直接使用InternalDictionary，以享受它完整的低gc的API
    /// </summary>
    public readonly struct VDictionary<TK, TV>:IDisposable
    {
        private readonly long _order;
        private readonly PDictionary<TK, TV> _buffer;

        public PDictionary<TK, TV> InternalBuffer
        {
            get
            {
                ThrowIfExpired();
                return _buffer;
            }
        }

        private VDictionary(long order, PDictionary<TK, TV> buffer)
        {
            _buffer = buffer;
            _order = order;
        }

        public void Dispose()
        {
            if (IsAlive)
            {
                _buffer.Reset(null); //消去comparer引用
                _buffer.Dispose(); //归还池
                StrongOrderedPool<PDictionary<TK, TV>>.Share.Return(_buffer);
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
                return _buffer != null && _order!=0 &&
                       _order == StrongOrderedPool<PDictionary<TK, TV>>.Share.GetOrder(_buffer);
            }
        }

        public static VDictionary<TK, TV> Rent(IEqualityComparer<TK>? equalityComparer = null)
        {
            var dic = StrongOrderedPool<PDictionary<TK, TV>>.Share.Rent(out var order);
            dic.Reset(equalityComparer);
            return new VDictionary<TK, TV>(order, dic);
        }

        private void ThrowIfExpired()
        {
            if (_buffer == null || _order == 0)
                throw new ObjectDisposedException("",
                    $"you must get this buffer using static method {nameof(VDictionary<TK, TV>)}.{nameof(Rent)}()");
            if (_order != StrongOrderedPool<PDictionary<TK, TV>>.Share.GetOrder(_buffer))
                throw new ObjectDisposedException("this buffer is disposed");
        }
    }
}