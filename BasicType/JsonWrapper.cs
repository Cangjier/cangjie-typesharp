using System.Text;
using System.Text.RegularExpressions;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.BasicType;
public struct JsonWrapper
{
    public JsonWrapper(object value)
    {
        if(value is Json jsonValue)
        {
            Target = jsonValue;
        }
        else
        {
            Target = new(value);
        }
    }

    public Json Target { get; }

    public void forEach(Action<object?> onItem)
    {
        Target.ForeachArray(item => onItem(item.Node));
    }

    public List<object> map(Func<object?, object> onItem)
    {
        List<object> result = [];
        Target.ForeachArray(item =>
        {
            result.Add(onItem(item.Node));
        });
        return result;
    }

    public List<object?> filter(Func<object?, Json> onItem)
    {
        List<object?> result = [];
        Target.ForeachArray(item =>
        {
            if (onItem(item.Node).IsFalse==false)
            {
                result.Add(item.Node);
            }
        });
        return result;
    }

    public Json find(Func<object?, Json> onItem)
    {
        foreach (var item in Target.GetArrayEnumerable())
        {
            if (onItem(item.Node).IsFalse==false)
            {
                return item;
            }
        }
        return Json.Null;
    }

    public int findIndex(Func<object?, Json> onItem)
    {
        int index = -1;
        foreach(var item in Target.GetArrayEnumerable())
        {
            index++;
            if (onItem(item.Node).IsFalse==false)
            {
                return index;
            }
        }
        return index;
    }

    public void push(object value)
    {
        Target.Add(value);
    }

    public int unshift(object value)
    {
        Target.Insert(0, new Json(value));
        return Target.Count;
    }

    public string join(string separator)
    {
        return Target.GetArrayEnumerable().Join(separator);
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

    public int indexOf(object value)
    {
        if (Target.IsString && value is string valueString)
        {
            return Target.AsString.IndexOf(valueString);
        }
        else if (Target.IsArray)
        {
            if(value is Json valueJson)
            {
                return Target.IndexOf(valueJson);
            }
            else
            {
                return Target.IndexOf(new(value));
            }
        }
        return -1;
    }

    public int lastIndexOf(object value)
    {
        if (Target.IsString && value is string valueString)
        {
            return Target.AsString.LastIndexOf(valueString);
        }
        else if (Target.IsArray)
        {
            if (value is Json valueJson)
            {
                return Target.LastIndexOf(valueJson);
            }
            else
            {
                return Target.LastIndexOf(new(value));
            }
        }
        return -1;
    }

    public bool includes(object value)
    {
        if (Target.IsString && value is string valueString)
        {
            return Target.AsString.Contains(valueString);
        }
        else if (Target.IsArray)
        {
            if(value is Json valueJson)
            {
                return Target.Contains(valueJson);
            }
            else
            {
                return Target.Contains(new(value));
            }
        }
        return false;
    }

    public List<object> split(string separator)
    {
        if (Target.IsString) return Target.AsString.Split(separator).Select(item => (object)item).ToList();
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

    public int length
    {
        get
        {
            if (Target.IsArray) return Target.AsArray.Count;
            else if (Target.IsString) return Target.AsString.Length;
            else throw new InvalidOperationException("JsonWrapper: length only support array or string type");
        }
    }

    public string toUpperCase()
    {
        if (Target.IsString) return Target.AsString.ToUpper();
        else if(Target.Is<char>())return Target.As<char>().ToString().ToUpper();
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

    public string toString()
    {
        return Target.ToString();
    }

    public Json sort(Func<object?, object?, int> compare)
    {
        if (Target.IsArray)
        {
            Target.Sort((a, b) => compare(a.Node, b.Node));
            return Target;
        }
        else
        {
            throw new InvalidOperationException("JsonWrapper: sort only support array type");
        }
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
