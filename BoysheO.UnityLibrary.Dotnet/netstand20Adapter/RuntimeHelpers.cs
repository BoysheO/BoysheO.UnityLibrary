using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

//base on https://stackoverflow.com/questions/53968920/how-do-i-check-if-a-type-fits-the-unmanaged-constraint-in-c
internal static class RuntimeHelpers
{
    private static readonly ConcurrentDictionary<Type, bool> _CachedTypes = new();

    public static bool IsReferenceOrContainsReferences<T>()
    {
        var t = typeof(T);
        return IsReferenceOrContainsReferences(t);
    }

    private static bool IsReferenceOrContainsReferences(Type t)
    {
        if (_CachedTypes.TryGetValue(t, out var result))
            return result;
        else if (t.IsPrimitive || t.IsPointer || t.IsEnum)
            result = true;
        else if (t.IsGenericType || !t.IsValueType)
            result = false;
        else
            result = t.GetFields(BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.Instance)
                .All(x => IsReferenceOrContainsReferences(x.FieldType));
        _CachedTypes.TryAdd(t, result);
        return result;
    }
}