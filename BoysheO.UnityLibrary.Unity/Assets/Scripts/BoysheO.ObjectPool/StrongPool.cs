using System.Collections.Generic;

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
            var len = _plist.Count;
            if (len == 0) return new();
            var last = _plist[len-1];
            _plist.RemoveAt(len-1);
            return last;
        }

        public void Return(T item)
        {
            _plist.Add(item);
        }
    }
}