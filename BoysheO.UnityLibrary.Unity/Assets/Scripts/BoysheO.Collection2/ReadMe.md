⚠本文档仍在编写中

# 综述 Overview
提供一组API，轻松使用池化列表，以减少GC压力。主要面向需要低GC的场景（Unity3D），不建议在高并发环境下使用。本库是PooledBuffer的重构替代品。
Provides a set of APIs to easily use Pooled List to reduce GC pressure.

# 性能 Performance
PList使用dotnet runtime8.0.5的实现池化。它比Unity的原生List还要快5%左右，可以放心使用。  
PDictionary使用Unity2022.3.14f1c1的Dictionary实现池化，因此它的性能与Unity原生字典性能差不多一致，实测约慢10%。这里的性能损失，是因为内部数组使用了PList替代，API调用和构造Span等行为造成了一点性能损失，这应该不会对整体代码的效率造成太大影响。  
之所以PDictionary不使用dotnet runtime8.0.5的实现，是因为后来的Dictionary加入了防Hash攻击功能，对Compare进行了特殊处理，这会让字典慢一倍。由于我们的设计场景是Unity，我认为放弃防Hash攻击是值得的，在Unity上对一个字典Hash攻击没有意义。  
PSortedList是使用PList为基础，二分排序为排序算法实现的字典类。
对于高并发场景，不建议使用。
# 快速入门 Quick Start
```C#
var buff = VList.Rent();
//do something with buff
buff.Disposable();
```
在高于等于C#8的版本中，可以使用using语句，以减少代码量。  
Using statement can be used in C#8 or higher version to reduce code.

```C#
using var buff = VList.Rent();
//do something with buff
```
# PooledLinq
PooledLinq提供了一组优化的LINQ扩展方法，以减少GC压力。
但是，使用委托本身就会产生gc，因此需要使用者自己权衡使用。
PooledLinq遵循如下设计原则：
1. 立即求值
2. 对原始Buff进行释放操作
3. 返回新的Buff

以上3条原则可以简化使用，如诸位要扩展自己的PooledLinq，应该遵循以上原则。

PooledLinq Provides a set of LINQ extension methods to reduce GC pressure.
However, using delegates itself will generate gc, so users need to weigh their own use.
PooledLinq follows the following design principles:
1. Immediate evaluation
2. Release the original Buff
3. Return a new Buff

The above 3 principles simplify the use. If you want to extend your own PooledLinq, you should follow the above principles.

```C#
using var buff = new []{1,2,3,4,5,6,7,8,9,10}.ToVList()
    .VWhere(x=>x%2==0)
    .VSelect(x=>x*2);
    //do something with buff
```

# 最大化收益
在开发实践中，使用IReadOnlyList等接口是常态需求，但是PooledBuffer本身是一个结构体，使用它来作为IReadOnlyList返回会导致装箱。因此PooledListBuffer提供了一个AsUnsafeReadOnlyList()方法，可以避免装箱，但是切记在buff销毁后，不要再使用该属性返回的对象。  
In development practice, using IReadOnlyList and other interfaces is a normal requirement, but PooledBuffer itself is a structure. Using it as an IReadOnlyList return will cause boxing. Therefore, PooledListBuffer provides an AsUnsafeReadOnlyList() method to avoid boxing, but remember not to use the object returned by this property after the buff is destroyed.  

# 性能提示 Performance Tips
每次对buff的访问都会有一点额外的安全性检查开销，如果在需要遍历大量数量的情况下，可以调用其Span属性以跳过安全检查来进行遍历。  
Accessing buff each time will have a little extra safety check overhead. If you need to traverse a large number of items, you can call its Span property to skip the safety check to traverse.

# 多线程安全
Rent操作和Disposable操作被设计成线程安全的，但是对buff的访问不是线程安全的，如果需要在多线程中访问buff，需要自行实现线程安全。  
Rent operation and Disposable operation are designed to be thread-safe, but access to buff is not thread-safe. If you need to access buff in multiple threads, you need to implement thread safety yourself.

```C#
object gate = new object();
using var buff = PooledListBuffer.Rent();
lock(gate)
{
    //do something with buff
}
```
但是，本库并不是为高并发场景设计的，高并发环境下池化会产生额外的性能消耗，因此在高并发场景下应多考虑依赖gc功能。用户应对自己的应用场景进行做出合理决策。

# 注意事项
* Todo 字典返回的Keys和Values尚未加入Version校验，换言之就是不要保留对字典的Keys属性和Values属性的引用，否则可能会导致Buff的泄漏

# 最佳实践 Best Practices
* 仅在需要借用Buff的时候借用，使用完毕后立即归还。并且尽可能不保留对Buff的引用。在安全的前提下尽可能使用Span操作  
* 如果一个函数接受一个PooledBuff参数输入，那么在函数内部不要对其进行释放操作，原则上假定调用方需要继续使用该Buff。
* 如果一个函数返回一个PooledBuff结果输出，那么调用方应负责对其进行释放操作。  
* 不要保留任何从Buff中获取的非元素对象（Keys，Values，Enumerable等），虽然已经对大部分对象进行了Version检查确保其不会在Buff销毁后工作，但是仍应遵守此原则

En:
* Only borrow Buff when needed, return it immediately after use. And try not to keep a reference to Buff. Use Span operation as much as possible under safe conditions.
* If a function accepts a PooledBuff parameter as input, do not release it in the function. In principle, it is assumed that the caller needs to continue to use the Buff.
* If a function returns a PooledBuff result as output, the caller is responsible for releasing it.
* Do not keep any non-element objects (Keys, Values, Enumerable, etc.) obtained from Buff. Although most objects have been Version checked to ensure that they will not work after Buff is destroyed, this principle should still be followed.


