using System;
using System.Globalization;
using NUnit.Framework;

namespace BoysheO.Collection.Test;

public class PDictionaryTests
{
    //在0.0.4版本及以前，dictionary调用Dispose之后，内部bucket长度变为0，导致Insert时取模为0抛异常
    [TestCase]
    public void EmptyAdd()
    {
        var dic = new PDictionary<int, int>();
        dic.Add(1, 1);
        dic.Clear();
        dic.Dispose();
        dic.Add(1,2);
    }
}