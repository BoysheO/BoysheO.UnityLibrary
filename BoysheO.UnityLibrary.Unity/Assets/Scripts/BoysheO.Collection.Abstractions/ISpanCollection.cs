using System;

namespace BoysheO.Collection
{
    public interface ISpanCollection<T>:IReadOnlySpanCollection<T>
    {
        Span<T> Span { get; }
    }
}