using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BoysheO.ObjectPool
{
    //Thread safe
    public class StrongOrderedPool<T> where T : class, new()
    {
        public static readonly StrongOrderedPool<T> Share = new();

        // private readonly Stack<T> objPool = new();
        private readonly ConcurrentBag<T> objPool = new();
        private readonly ConcurrentBag<StrongBox<long>> orderPool = new();
        private long _curOrder;
        private readonly object gate = new();
        private readonly ConditionalWeakTable<T, StrongBox<long>> obj2order = new();

        public T Rent(out long order)
        {
            var r = objPool.TryTake(out var ins);
            if (!r) ins = new T();
            r = orderPool.TryTake(out var orderBox);
            if (!r) orderBox = new StrongBox<long>();
            order = orderBox!.Value = Interlocked.Increment(ref _curOrder);
            lock (gate)
            {
                obj2order.Add(ins!, orderBox);
            }

            return ins!;
        }

        /// <summary>
        /// 获取其借出流水号
        /// </summary>
        public long GetOrder(T list)
        {
            lock (gate)
            {
                return obj2order.TryGetValue(list, out var v) ? v.Value : 0;
            }
        }

        /// <summary>
        /// 归还对象并销毁对应的借出流水号
        /// </summary>
        public void Return(T list)
        {
            bool r;
            StrongBox<long>? orderBox;
            lock (gate)
            {
                r = obj2order.TryGetValue(list, out orderBox);
                if (r) obj2order.Remove(list);
            }

            if (!r) return;
            objPool.Add(list);
            orderPool.Add(orderBox!);
        }
    }
}