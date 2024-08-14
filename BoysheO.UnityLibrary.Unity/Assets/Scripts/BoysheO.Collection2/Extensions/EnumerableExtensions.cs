using System;
using System.Collections.Generic;
using BoysheO.Collection;

namespace BoysheO.Collection2.Linq
{
    public static class EnumerableExtensions
    {
        public static VList<T> ToVList<T>(this IEnumerable<T> source,
            Func<T, bool>? predicate = null)
        {
            var buff = VList<T>.Rent();
            if (predicate == null)
            {
                buff.InternalBuffer.AddRange(source);
            }
            else
            {
                if (source is IReadOnlySpanCollection<T> pList)
                {
                    buff.InternalBuffer.AddRange(pList.ReadOnlySpan);
                }
                else if (source is IReadOnlyOrderedSet<T> set)
                {
                    buff.InternalBuffer.AddRange(set.AsReadOnlySpanCollection.ReadOnlySpan);
                }
                else if (source is IReadOnlyList<T> list)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var x1 = list[i];
                        if (predicate(x1)) buff.Add(x1);
                    }
                }
                else if (source is IList<T> list2)
                {
                    for (var i = 0; i < list2.Count; i++)
                    {
                        var x1 = list2[i];
                        if (predicate(x1)) buff.Add(x1);
                    }
                }
                else
                {
                    foreach (var x1 in source)
                    {
                        if (predicate(x1)) buff.Add(x1);
                    }
                }
            }

            return buff;
        }

        public static VList<T> ToVList<T>(this VList<T> source,
            Func<T, bool>? predicate = null)
        {
            var buff = VList<T>.Rent();
            if (predicate == null)
            {
                buff.AddRange(source.Span);
            }
            else
            {
                foreach (var x1 in source)
                {
                    if (predicate(x1)) buff.Add(x1);
                }
            }

            return buff;
        }

        public static VDictionary<TK, TV> ToVDictionary<TS, TK, TV>(
            this IEnumerable<TS> source,
            Func<TS, TK> keySelector,
            Func<TS, TV> valueSelector)
        {
            var buff = VDictionary<TK, TV>.Rent();
            foreach (var x1 in source)
            {
                buff.InternalBuffer.Add(keySelector(x1), valueSelector(x1));
            }

            return buff;
        }

        public static VSortedList<TK, TV> ToVSortedList<TS, TK, TV>(
            this IEnumerable<TS> source,
            Func<TS, TK> keySelector,
            Func<TS, TV> valueSelector,
            IComparer<TK>? comparer) where TK : notnull
        {
            var buff = VSortedList<TK, TV>.Rent(comparer);
            var interBuff = buff.InternalBuffer;
            foreach (var x1 in source)
            {
                interBuff.Add(keySelector(x1), valueSelector(x1));
            }

            return buff;
        }

        public static VOrderedSet<T> ToVOrderedSet<T>(this IEnumerable<T> itor,
            IComparer<T>? comparer) where T : notnull
        {
            if (itor == null) throw new ArgumentNullException(nameof(itor));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            var buffer = VOrderedSet<T>.Rent(comparer);
            buffer.InternalBuffer.AddRange(itor);
            return buffer;
        }


        public static VOrderedSet<T> ToVOrderedSet<T>(this VList<T> lst,
            IComparer<T>? comparer) where T : notnull
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            var buffer = VOrderedSet<T>.Rent(comparer);
            buffer.InternalBuffer.AddRange(lst.Span);
            return buffer;
        }

        public static VOrderedSet<T> ToVOrderedSet<T>(this VList<T> lst) where T : notnull
        {
            return ToVOrderedSet(lst, Comparer<T>.Default);
        }
    }
}