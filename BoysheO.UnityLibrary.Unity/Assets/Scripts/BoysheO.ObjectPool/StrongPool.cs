using System.Collections.Generic;
using BoysheO.Extensions;

namespace BoysheO.ObjectPool
{
    public class StrongPool<T> where T : new()
    {
        public static StrongPool<T> Share
        {
            get => _pool ??= new();
        }

        private static StrongPool<T> _pool = null!;

        private readonly List<T> _plist = new();

        public T Rent()
        {
            if (_plist.TryPop(out T? item)) return item;
            return new();
        }

        public void Return(T item)
        {
            _plist.Push(item);
        }
    }
}