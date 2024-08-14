using System;
using System.Collections.Generic;

namespace BoysheO.Collection
{
    partial class PDictionary<TKey, TValue> : IDisposable
    {
        public int Version => _version;

        public void Add(KeyValuePair<TKey, TValue> kvp)
        {
            Add(kvp.Key,kvp.Value);
        }

        public void Reset(IEqualityComparer<TKey>? comparer)
        {
            Clear();
            _comparer = comparer;
        }

        /// <summary>
        /// 即使进行了Disposable，成员Comparer也不会被设置为null。这是为了预防意外重用本对象时造成不一样的行为。
        /// 要解除本对象对Comparer的引用，请先使用Reset(null)，再Dispose
        /// </summary>
        public void Dispose()
        {
            Clear();
            _buckets?.Dispose();
            _entries?.Dispose();
        }
    }
}