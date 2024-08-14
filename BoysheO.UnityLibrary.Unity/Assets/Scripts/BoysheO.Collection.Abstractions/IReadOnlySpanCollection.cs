using System;

namespace BoysheO.Collection
{
    public interface IReadOnlySpanCollection<T>
    {
        ReadOnlySpan<T> ReadOnlySpan { get; }
    }
}