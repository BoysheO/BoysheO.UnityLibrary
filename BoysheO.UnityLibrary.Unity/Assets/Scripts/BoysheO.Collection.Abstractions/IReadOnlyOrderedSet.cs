using System.Collections.Generic;
using BoysheO.Collection;

namespace BoysheO.Collection2
{
    public interface IReadOnlyOrderedSet<T>
    {
        IReadOnlySpanCollection<T> AsReadOnlySpanCollection { get; }
        IReadOnlyList<T> AsReadOnlyList { get; }
        ISet<T> AsSet { get; }
    }
}