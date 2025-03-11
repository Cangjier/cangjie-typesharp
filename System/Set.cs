using System.Collections;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

public class Set:IEnumerable<Json>
{
    private readonly HashSet<Json> _hashSet;

    // 实现 JS 风格的构造函数重载
    public Set() => _hashSet = new HashSet<Json>();
    public Set(params Json[] items) => _hashSet = new HashSet<Json>(items);

    // 对应 JS Set.add()（返回实例本身以实现链式调用）
    public Set add(Json item)
    {
        _hashSet.Add(item);
        return this;
    }

    // 对应 JS Set.has()
    public bool has(Json item) => _hashSet.Contains(item);

    // 对应 JS Set.delete()
    public bool delete(Json item) => _hashSet.Remove(item);

    // 对应 JS Set.clear()
    public void clear() => _hashSet.Clear();

    // 对应 JS Set.size 属性
    public int size => _hashSet.Count;

    // 可选：实现迭代器（JS 的 for..of 遍历）
    public IEnumerator<Json> GetEnumerator() => _hashSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}