using System;
namespace BoysheO.Collection2.Linq
{
    public static class DelegateExtensions
    {
        public static void Invoke(this VList<Action> lst)
        {
            using var copy = lst.ToVList();
            foreach (var ac in copy.InternalBuffer.Span)
            {
                ac();
            }
        }
        
        public static void Invoke<T>(this VList<Action<T>> lst,T arg)
        {
            using var copy = lst.ToVList();
            foreach (var ac in copy.InternalBuffer.Span)
            {
                ac(arg);
            }
        }
    }
}