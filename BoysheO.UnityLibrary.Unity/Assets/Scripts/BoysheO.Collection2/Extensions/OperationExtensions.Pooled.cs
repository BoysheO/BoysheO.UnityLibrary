using System;
using System.Collections.Generic;

namespace BoysheO.Collection2.Linq
{
    /// <summary>
    /// Every operation will dispose the source PooledBuff and rent another PooledBuff to return
    /// Rules:
    ///  1. don't use the source PooledBuff after operation.
    ///  2. every customer operation should dispose the source PooledBuff and return another PooledBuff.
    /// It's faster and lowGc 
    /// </summary>
    public static class OperationExtensions
    {
        public static VList<TTar> VSelect<TSrc, TTar>(this VList<TSrc> source,
            Func<TSrc, TTar> selector)
        {
            var buff = VList<TTar>.Rent();
            foreach (var src in source.Span)
            {
                var tar = selector(src);
                buff.Add(tar);
            }

            source.Dispose();
            return buff;
        }

        public static VList<TTar> VSelect<TSrc, TTar>(this VList<TSrc> source,
            Func<int, TSrc, TTar> selector)
        {
            var buff = VList<TTar>.Rent();
            var len = source.Span.Length;
            for (var index = 0; index < len; index++)
            {
                var src = source.Span[index];
                var tar = selector(index, src);
                buff.Add(tar);
            }

            source.Dispose();
            return buff;
        }

        public static VList<T> VWhere<T>(this VList<T> source, Func<T, bool> predicate)
        {
            var buff = VList<T>.Rent();
            foreach (var x1 in source.Span)
            {
                if (predicate(x1)) buff.Add(x1);
            }

            source.Dispose();
            return buff;
        }

        public static VList<T> VSlice<T>(this VList<T> source, int start, int count)
        {
            var buf = VList<T>.Rent();
            var destination = buf.InternalBuffer.AppendSpan(count);
            source.Span.Slice(start, count).CopyTo(destination);
            source.Dispose();
            return buf;
        }

        public static VList<VList<T>> VChunk<T>(this VList<T> source, int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            var buff = VList<VList<T>>.Rent();
            var count = source.Count;
            for (int i = 0; i < count; i += size)
            {
                var subBuff = VList<T>.Rent();
                var destination = subBuff.InternalBuffer.AppendSpan(size);
                source.Span.Slice(i, size).CopyTo(destination);
                buff.Add(subBuff);
            }

            source.Dispose();
            return buff;
        }

        public static VList<T> VSelectMany<T>(this VList<VList<T>> source)
        {
            var buff = VList<T>.Rent();
            foreach (var x1 in source.Span)
            {
                buff.AddRange(x1.Span);
                x1.Dispose();
            }

            source.Dispose();
            return buff;
        }

        public static VList<KeyValuePair<TK, TV>> ToVList<TK, TV>(
            this VDictionary<TK, TV> source)
        {
            var buff = VList<KeyValuePair<TK, TV>>.Rent();
            foreach (var x1 in source.InternalBuffer)
            {
                buff.Add(x1);
            }

            source.Dispose();
            return buff;
        }

        public static VList<KeyValuePair<TK, TV>> ToVList<TK, TV>(
            this VSortedList<TK, TV> source) where TK : notnull
        {
            var buff = VList<KeyValuePair<TK, TV>>.Rent();
            var span = buff.InternalBuffer.AppendSpan(source.Count);
            int index = 0;
            foreach (var x1 in source.InternalBuffer)
            {
                span[index] = x1;
                index++;
            }

            source.Dispose();
            return buff;
        }

        public static VDictionary<TK, TV> ToVDictionary<TS, TK, TV>(
            this VList<TS> source,
            Func<TS, TK> keySelector,
            Func<TS, TV> valueSelector)
        {
            var buff = VDictionary<TK, TV>.Rent();
            var ibuff = buff.InternalBuffer;
            foreach (var x1 in source.Span)
            {
                ibuff.Add(keySelector(x1), valueSelector(x1));
            }

            source.Dispose();
            return buff;
        }

        public static VSortedList<TK, TV> ToVSortedList<TS, TK, TV>(
            this VList<TS> source,
            Func<TS, TK> keySelector,
            Func<TS, TV> valueSelector,
            IComparer<TK>? keyComparer) where TK : notnull
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));
            var buff = VSortedList<TK, TV>.Rent(keyComparer);
            var ibuff = buff.InternalBuffer;
            foreach (var x1 in source.Span)
            {
                ibuff.Add(keySelector(x1), valueSelector(x1));
            }

            source.Dispose();
            return buff;
        }
    }
}