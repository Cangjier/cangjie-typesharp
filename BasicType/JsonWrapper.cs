using System.Text;
using System.Text.RegularExpressions;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.BasicType;
public struct JsonWrapper
{
    public JsonWrapper(object? value)
    {
        if (value is Json jsonValue)
        {
            Target = jsonValue;
        }
        else
        {
            Target = new(value);
        }
    }

    public Json Target { get; }

    public void forEach(Action<Json> onItem)
    {
        Target.ForeachArray(item => onItem(item));
    }

    public List<object> map(Func<Json, Json> onItem)
    {
        List<object> result = [];
        Target.ForeachArray(item =>
        {
            result.Add(onItem(item));
        });
        return result;
    }

    public List<object> map(Func<Json, Json , Json> onItem)
    {
        List<object> result = [];
        Target.ForeachArray((index,item) =>
        {
            result.Add(onItem(item, index));
        });
        return result;
    }

    public List<object?> filter(Func<Json, Json> onItem)
    {
        List<object?> result = [];
        Target.ForeachArray(item =>
        {
            var itemResult = onItem(item);
            if (itemResult.IsFalse == false && itemResult.IsUndefined == false && itemResult.IsNull == false)
            {
                result.Add(item.Node);
            }
        });
        return result;
    }

    public Json find(Func<Json, Json> onItem)
    {
        foreach (var item in Target.GetArrayEnumerable())
        {
            var itemResult = onItem(item);
            if (itemResult.IsFalse == false && itemResult.IsUndefined == false && itemResult.IsNull == false)
            {
                return item;
            }
        }
        return Json.Undefined;
    }

    public int findIndex(Func<Json, Json> onItem)
    {
        int index = -1;
        foreach (var item in Target.GetArrayEnumerable())
        {
            index++;
            var itemResult = onItem(item);
            if (itemResult.IsFalse == false && itemResult.IsUndefined == false && itemResult.IsNull == false)
            {
                return index;
            }
        }
        return -1;
    }

    public void push(params Json[] values)
    {
        foreach (var value in values)
        {
            Target.Add(value);
        }
    }

    public string join(Json separator)
    {
        return Target.GetArrayEnumerable().Join(separator.AsString);
    }

    public bool endsWith(string value)
    {
        if (Target.IsString) return Target.AsString.EndsWith(value);
        else throw new InvalidOperationException("JsonWrapper: endsWith only support string type");
    }

    public bool startsWith(string value)
    {
        if (Target.IsString) return Target.AsString.StartsWith(value);
        else throw new InvalidOperationException("JsonWrapper: startsWith only support string type");
    }

    public string substring(int start, int end)
    {
        if (Target.IsString) return Target.AsString.Substring(start, end - start);
        else throw new InvalidOperationException("JsonWrapper: subString only support string type");
    }

    public string substring(int start)
    {
        if (Target.IsString) return Target.AsString.Substring(start);
        else throw new InvalidOperationException("JsonWrapper: subString only support string type");
    }

    public int indexOf(Json value)
    {
        if (Target.IsString && value.IsString)
        {
            return Target.AsString.IndexOf(value.AsString);
        }
        else if (Target.IsArray)
        {
            return Target.IndexOf(value);
        }
        return -1;
    }

    public int indexOf(Json value, Json start)
    {
        if (Target.IsString && value.IsString)
        {
            return Target.AsString.IndexOf(value.AsString, start.ToInt32);
        }
        else if (Target.IsArray)
        {
            return Target.IndexOf(value, start.ToInt32);
        }
        return -1;
    }

    public int lastIndexOf(Json value)
    {
        if (Target.IsString && value.IsString)
        {
            return Target.AsString.LastIndexOf(value.AsString);
        }
        else if (Target.IsArray)
        {
            return Target.LastIndexOf(value);
        }
        return -1;
    }

    public bool includes(Json value)
    {
        return Target.Contains(value);
    }

    public List<object> split(string separator)
    {
        if (Target.IsString)
        {
            if (separator == string.Empty)
            {
                List<object> result = [];
                foreach(var ch in Target.AsString)
                {
                    result.Add(ch.ToString());
                }
                return result;
            }
            else
            {
                return Target.AsString.Split(separator).Select(item => (object)item).ToList();
            }
        }
        else throw new InvalidOperationException("JsonWrapper: split only support string type");
    }

    /// <summary>
    /// 替换字符串
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string replace(string oldValue, string newValue)
    {
        if (Target.IsString) return Target.AsString.Replace(oldValue, newValue);
        else throw new InvalidOperationException("JsonWrapper: replace only support string type");
    }

    public string replace(Regex regex, string newValue)
    {
        if (Target.IsString) return regex.Replace(Target.AsString, newValue);
        else throw new InvalidOperationException("JsonWrapper: replace only support string type");
    }

    public string repeat(int count)
    {
        StringBuilder temp = new();
        var item = Target.ToString();
        for (int i = 0; i < count; i++)
        {
            temp.Append(item);
        }
        return temp.ToString();
    }

    public string padStart(int length)
    {
        return padStart(length, " ");
    }

    public string padStart(int length, string value)
    {
        if (Target.IsString) return Target.AsString.PadLeft(length, value[0]);
        else throw new InvalidOperationException("JsonWrapper: padStart only support string type");
    }

    public string padEnd(int length, string value)
    {
        if (Target.IsString) return Target.AsString.PadRight(length, value[0]);
        else throw new InvalidOperationException("JsonWrapper: padEnd only support string type");
    }

    public string padEnd(int length)
    {
        return padEnd(length, " ");
    }

    public Json length
    {
        get
        {
            if (Target.IsUndefined) return Json.Undefined;
            else if (Target.IsArray) return Target.AsArray.Count;
            else if (Target.IsString) return Target.AsString.Length;
            else if (Target.IsObject) return Target.AsObject.Count;
            else return Json.Undefined;
        }
    }

    public string toUpperCase()
    {
        if (Target.IsString) return Target.AsString.ToUpper();
        else if (Target.Is<char>()) return Target.As<char>().ToString().ToUpper();
        else throw new InvalidOperationException("JsonWrapper: toUpperCase only support string type");
    }

    public string toLowerCase()
    {
        if (Target.IsString) return Target.AsString.ToLower();
        else if (Target.Is<char>()) return Target.As<char>().ToString().ToLower();
        else throw new InvalidOperationException("JsonWrapper: toLowerCase only support string type");
    }

    public string trim()
    {
        if (Target.IsString) return Target.AsString.Trim();
        else throw new InvalidOperationException("JsonWrapper: trim only support string type");
    }

    public string trim(Json value)
    {
        return Target.AsString.Trim(value.AsString.ToArray());
    }

    public string trimStart()
    {
        if (Target.IsString) return Target.AsString.TrimStart();
        else throw new InvalidOperationException("JsonWrapper: trimStart only support string type");
    }

    public string trimStart(Json value)
    {
        if (value.IsString) return Target.AsString.TrimStart(value.AsString.ToArray());
        else throw new InvalidOperationException("JsonWrapper: trimStart only support string type");
    }

    public string trimEnd()
    {
        if (Target.IsString) return Target.AsString.TrimEnd();
        else throw new InvalidOperationException("JsonWrapper: trimEnd only support string type");
    }

    public string trimEnd(Json value)
    {
        if (value.IsString) return Target.AsString.TrimEnd(value.AsString.ToArray());
        else throw new InvalidOperationException("JsonWrapper: trimEnd only support string type");
    }

    public string toString()
    {
        return Target.ToString();
    }

    public string toString(int radix)
    {
        if (Target.IsNumber) return Convert.ToString((int)Target.AsNumber, radix);
        else throw new InvalidOperationException("JsonWrapper: toString only support number type");
    }

    public Json sort(Func<Json, Json, int> compare)
    {
        if (Target.IsArray)
        {
            Target.Sort((a, b) => compare(a, b));
            return Target;
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: sort only support array type");
        }
    }

    public Json sort()
    {
        if (Target.IsArray)
        {
            Target.Sort((a, b) =>
            {
                if (a.IsString && b.IsString)
                {
                    return string.Compare(a.AsString, b.AsString);
                }
                else if (a.IsNumber && b.IsNumber)
                {
                    return a.AsNumber.CompareTo(b.AsNumber);
                }
                else
                {
                    return 0;
                }
            });
            return Target;
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: sort only support array type");
        }
    }

    public Json splice(int start, int deleteCount, params Json[] items)
    {
        if (start < 0)
        {
            start = Target.Count + start;
        }
        if (Target.IsArray)
        {
            return Target.Splice(start, deleteCount, items);
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: splice only support array type");
        }
    }

    public Json splice(int start) => splice(start, -1);

    public Json slice(int start, int end)
    {
        if (start < 0)
        {
            start = Target.Count + start;
        }
        if (end < 0)
        {
            end = Target.Count + end;
        }
        if (Target.IsArray || Target.IsString)
        {
            return Target.Slice(start, end);
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: slice only support array or string type");
        }
    }

    public Json slice(int start) => slice(start, Target.Count);

    public Json reverse() => Target.Reverse();

    public Json some(Func<Json, Json> onItem)
    {
        foreach (var item in Target.GetArrayEnumerable())
        {
            var itemResult = onItem(item);
            if (itemResult.IsFalse == false && itemResult.IsUndefined == false && itemResult.IsNull == false)
            {
                return true;
            }
        }
        return false;
    }

    public Json concat(params Json[] items)
    {
        return Target.Concat(items);
    }

    public Json reduce(Func<Json, Json, Json> onItem, Json initialValue)
    {
        Json result = initialValue;
        foreach (var item in Target.GetArrayEnumerable())
        {
            result = onItem(result, item);
        }
        return result;
    }

    public Json reduce(Func<Json, Json, Json> onItem)
    {
        var array = Target.GetArrayEnumerable();
        Json result = array.First();
        foreach (var item in array.Skip(1))
        {
            result = onItem(result, item);
        }
        return result;
    }

    public Json reduceRight(Func<Json, Json, Json> onItem, Json initialValue)
    {
        Json result = initialValue;
        foreach (var item in Target.GetArrayEnumerable().Reverse())
        {
            result = onItem(result, item);
        }
        return result;
    }

    public Json reduceRight(Func<Json, Json, Json> onItem)
    {
        var array = Target.GetArrayEnumerable().Reverse();
        Json result = array.First();
        foreach (var item in array.Skip(1))
        {
            result = onItem(result, item);
        }
        return result;
    }

    public Json pop()
    {
        var last = Target[Target.Count - 1];
        Target.RemoveAt(Target.Count - 1);
        return last;
    }

    public Json shift()
    {
        var first = Target[0];
        Target.RemoveAt(0);
        return first;
    }

    public Json fill(Json item, int start, int end)
    {
        var self = Target;
        for (int i = start; i < end; i++)
        {
            self[i] = item;
        }
        return self;
    }

    public Json fill(Json item)
    {
        int count = Target.Count;
        var self = Target;
        for (int i = 0; i < count; i++)
        {
            self[i] = item;
        }
        return self;
    }

    public Json unshift(params Json[] items)
    {
        for (int i = items.Length - 1; i >= 0; i--)
        {
            Target.Insert(0, items[i]);
        }
        return Target;
    }

    public Json every(Func<Json, Json> onItem)
    {
        foreach (var item in Target.GetArrayEnumerable())
        {
            var itemResult = onItem(item);
            if (itemResult.IsFalse == false || itemResult.IsUndefined == false || itemResult.IsNull == false)
            {
                return false;
            }
        }
        return true;
    }

    public Json toFixed(Json digits)
    {
        if (Target.IsNumber)
        {
            return Math.Round(Target.AsNumber, digits.ToInt32);
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: toFixed only support number type");
        }
    }

    public Json toExponential(Json digits)
    {
        if (Target.IsNumber)
        {
            return Target.AsNumber.ToString($"E{digits.ToInt32}");
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: toExponential only support number type");
        }
    }

    public Json toPrecision(Json digits)
    {
        if (Target.IsNumber)
        {
            return Target.AsNumber.ToString($"F{digits.ToInt32}");
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: toPrecision only support number type");
        }
    }

    public Json toLocaleString()
    {
        return Target.Node?.ToString() ?? "null";
    }

    public Json flat(Json depth)
    {
        void _flat(Json target, Json depth, Json result)
        {
            if (depth.ToInt32 == 0)
            {
                result.Add(target);
            }
            else
            {
                foreach (var item in target.GetArrayEnumerable())
                {
                    _flat(item, depth - 1, result);
                }
            }
        }

        Json result = Json.NewArray();
        _flat(Target, depth, result);
        return result;
    }

    public Json flat()
    {
        void _flat(Json target, Json result)
        {
            foreach (var item in target.GetArrayEnumerable())
            {
                if (item.IsArray)
                {
                    _flat(item, result);
                }
                else
                {
                    result.Add(item);
                }
            }
        }

        Json result = Json.NewArray();
        _flat(Target, result);
        return result;
    }

    public Json flatMap(Func<Json, Json> onItem)
    {
        Json result = Json.NewArray();
        void _flat(Json target, Json result)
        {
            foreach (var item in target.GetArrayEnumerable())
            {
                var temp = onItem(item);
                if (temp.IsArray)
                {
                    _flat(temp, result);
                }
                else
                {
                    result.Add(temp);
                }
            }
        }
        _flat(Target, result);
        return result;
    }

    public Json flatMap(Func<Json, Json> onItem, Json depth)
    {
        Json result = Json.NewArray();
        void _flat(Json target, Json depth, Json result)
        {
            if (depth.ToInt32 == 0)
            {
                result.Add(target);
            }
            else
            {
                foreach (var item in target.GetArrayEnumerable())
                {
                    var temp = onItem(item);
                    if (temp.IsArray)
                    {
                        _flat(temp, depth - 1, result);
                    }
                    else
                    {
                        result.Add(temp);
                    }
                }
            }
        }
        _flat(Target, depth, result);
        return result;
    }

    public Json match(Json value)
    {
        if (Target.IsString == false)
        {
            throw new InvalidOperationException("JsonWrapper: match only support string type");
        }
        Regex regex;
        if (value.IsString)
        {
            regex = new(value.AsString);
        }
        else if (value.Is<Regex>())
        {
            regex = value.As<Regex>();
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: match only support string or regex type");
        }
        var match = regex.Match(Target.AsString);
        var matchResult = Json.Null;
        if (match.Success)
        {
            matchResult = Json.NewObject();
            matchResult.Set("0", match.Value);
            int index = 1;
            foreach (Group group in match.Groups)
            {
                if (group.Success)
                {
                    matchResult.Set(index.ToString(), group.Value);
                    index++;
                }
            }
            var groups = Json.NewObject();
            foreach (Group group in match.Groups)
            {
                if (group.Success && !string.IsNullOrEmpty(group.Name) && group.Name != "0" && !int.TryParse(group.Name, out _))
                {
                    groups.Set(group.Name, group.Value);
                }
            }
            if (groups.Count > 0)
            {
                matchResult.Set("groups", groups);
            }
        }
        return matchResult;
    }

    public bool test(Json value)
    {
        if (Target.Is<Regex>() == false)
        {
            throw new InvalidOperationException("JsonWrapper: test only support regex type");
        }
        Regex regex = Target.As<Regex>();
        if (value.IsString)
        {
            return regex.IsMatch(value.AsString);
        }
        else if (value.Is<Regex>())
        {
            return regex.ToString() == value.As<Regex>().ToString();
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: test only support string or regex type");
        }
    }

    public Json exec(Json value)
    {
        if (Target.Is<Regex>() == false)
        {
            throw new InvalidOperationException("JsonWrapper: exec only support regex type");
        }
        Regex regex = Target.As<Regex>();
        if (value.IsString)
        {
            var match = regex.Match(value.AsString);
            if (match.Success)
            {
                var result = Json.NewObject();
                result.Set("0", match.Value);
                int index = 1;
                foreach (Group group in match.Groups)
                {
                    if (group.Success)
                    {
                        result.Set(index.ToString(), group.Value);
                        index++;
                    }
                }
                var groups = result.GetOrCreateObject("groups");
                foreach (Group group in match.Groups)
                {
                    if (group.Success && !string.IsNullOrEmpty(group.Name) && group.Name != "0" && !int.TryParse(group.Name, out _))
                    {
                        groups.Set(group.Name, group.Value);
                    }
                }
                return result;
            }
            else
            {
                return Json.Null;
            }
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: exec only support string type");
        }
    }

    public int localeCompare(Json other)
    {
        if (Target.IsString == false || other.IsString == false)
        {
            throw new InvalidOperationException("JsonWrapper: localeCompare only support string type");
        }
        return Target.AsString.CompareTo(other.AsString);
    }

    /// <summary>
    /// 隐式转换代理，用于Script
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JsonWrapper op_ImplicitFrom(object value)
    {
        return new(value);
    }

    /// <summary>
    /// 隐式转换代理，用于Script
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static object? op_ImplicitTo(JsonWrapper value)
    {
        return value.Target;
    }
}
