using System.Collections.Generic;

namespace BoysheO.Collection
{
    internal static class CollectionHelper<T>
    {
        private static readonly IReadOnlyList<T> EmptyList = new List<T>(capacity:0);
        
        //这一句实际是为了拿取它的内部SZGenericArrayEnumerator<T>.Empty
        public static IEnumerator<T> EmptyEnumerator => EmptyList.GetEnumerator();
    }
}