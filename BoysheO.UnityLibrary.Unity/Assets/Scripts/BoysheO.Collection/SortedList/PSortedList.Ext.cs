using System;
using System.Collections.Generic;

namespace BoysheO.Collection
{
    partial class PSortedList<TKey, TValue>:IDisposable
    {
        public PList<TKey> InternalKey => keys;
        public PList<TValue> InternalValues => values;

        public void Reset(IComparer<TKey>? comparer)
        {
            keys.Clear();
            values.Clear();
            this._comparer = comparer ?? Comparer<TKey>.Default;
        }
        
        public bool TryAdd(TKey key, TValue value)
        {
            var idx = keys.BinarySearch(key,Comparer);
            if (idx >= 0) return false;
            Insert(~idx,key,value);
            return true;
        }

        public TValue? GetValueOrDefault(TKey key)
        {
            TryGetValue(key, out var t);//return default if no key
            return t;
        }

        public void FastCopyTo(PSortedList<TKey, TValue> another)
        {
            if (another.Capacity != this.Capacity) throw new Exception("should have same capacity");
            if (another.Count != 0) throw new Exception("another should be empty");
            if (another.Comparer != this.Comparer) throw new Exception("should have same comparer");
            keys.Span.CopyTo(another.keys.AppendSpan(keys.Count));
            values.Span.CopyTo(another.values.AppendSpan(values.Count));
        }
        
        /// <summary>
        /// 即使进行了Disposable，成员Comparer也不会被设置为null。这是为了预防意外重用本对象时造成不一样的行为。
        /// 要解除本对象对Comparer的引用，请先使用Reset(null)，再Dispose
        /// </summary>
        public void Dispose()
        {
            keys.Dispose();
            values.Dispose();
        }
    }
}