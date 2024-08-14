using System;

namespace BoysheO.ObjectPool
{
    public class WeakPool<T> where T : new()
    {
        public static WeakPool<T> Share
        {
            get => _pool ??= new();
        }

        private static WeakPool<T> _pool = null!;

        private readonly WeakReference<StrongPool<T>> _objPool = new(new StrongPool<T>());

        public T Rent()
        {
            if (_objPool.TryGetTarget(out var lst))
            {
                return lst.Rent();
            }

            return new();
        }

        public void Return(T item)
        {
            if (!_objPool.TryGetTarget(out var lst))
            {
                lst = new();
                _objPool.SetTarget(lst);
            }

            lst.Return(item);
        }
    }
}