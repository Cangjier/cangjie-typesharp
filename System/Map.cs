using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;

public class Map
{
    private Json target;

    public Map()
    {
        target = Json.NewObject();
    }

    private static string ToKey(Json key)
    {
        if (key.IsString) return key.AsString;
        if (key.IsNumber) return key.AsNumber.ToString();
        if (key.IsBoolean) return key.AsBoolean ? "true" : "false";
        if (key.IsNull) return "null";
        return key.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 键值对数量
    /// </summary>
    public Json size => target.Count;

    /// <summary>
    /// 设置键值对，返回自身以支持链式调用
    /// </summary>
    public Map set(Json key, Json value)
    {
        target[ToKey(key)] = value;
        return this;
    }

    /// <summary>
    /// 获取键对应的值，不存在时返回 null
    /// </summary>
    public Json get(Json key)
    {
        var k = ToKey(key);
        if (target.ContainsKey(k))
        {
            return target[k];
        }
        return Json.Null;
    }

    /// <summary>
    /// 判断是否包含指定键
    /// </summary>
    public bool has(Json key)
    {
        return target.ContainsKey(ToKey(key));
    }

    /// <summary>
    /// 删除指定键，返回是否删除成功
    /// </summary>
    public Json delete(Json key)
    {
        var k = ToKey(key);
        var result = target.Get(k);
        target.Remove(k);
        return result;
    }

    /// <summary>
    /// 清空所有键值对
    /// </summary>
    public void clear()
    {
        target.Clear();
    }

    /// <summary>
    /// 返回所有键组成的数组
    /// </summary>
    public Json keys()
    {
        return new Json(target.Keys);
    }

    /// <summary>
    /// 返回所有值组成的数组
    /// </summary>
    public Json values()
    {
        return new Json(target.Values);
    }

    /// <summary>
    /// 返回所有 [key, value] 条目组成的数组
    /// </summary>
    public Json entries()
    {
        var result = Json.NewArray();
        foreach (var key in target.Keys)
        {
            var entry = Json.NewArray();
            entry.Add(key);
            entry.Add(target[key]);
            result.Add(entry);
        }
        return result;
    }

    /// <summary>
    /// 遍历每个键值对，回调参数为 (value, key)
    /// </summary>
    public void forEach(Action<Json, Json> callback)
    {
        foreach (var key in target.Keys)
        {
            callback(target[key], key);
        }
    }


}