using System.Collections;
using TidyHPC.LiteDB;
using TidyHPC.LiteDB.Metas;
using TidyHPC.LiteJson;
using VizGroup.V1.IOStorage;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// 数据库
/// </summary>
public class database
{
    public database(Database target)
    {
        Target = target;
    }

    /// <summary>
    /// 封装对象
    /// </summary>
    private Database Target { get; }

    /// <summary>
    /// 打开一个数据库
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<database> open(string filePath)
    {
        Database db = new();
        await db.Startup(filePath);
        return new database(db);
    }

    public async Task register(databaseInterface objectInterface)
    {
        if(await Target.ContainsInterface(objectInterface.name))
        {
            return;
        }
        ObjectInterface instance = new ObjectInterface();
        if (objectInterface.name == string.Empty)
        {
            throw new Exception("接口名称不能为空");
        }
        instance.FullName = objectInterface.name;
        foreach (var field in objectInterface.fields)
        {
            if (field.name == string.Empty)
            {
                throw new Exception("字段名称不能为空");
            }
            FieldMapType mapType = FieldMapType.None;
            if(field.isMaster)
            {
                mapType = FieldMapType.Master;
            }
            else if (field.isIndex)
            {
                mapType = FieldMapType.Index;
            }
            else if (field.isIndeArray)
            {
                mapType = FieldMapType.IndexArray;
            }
            else if (field.isIndexSet)
            {
                mapType = FieldMapType.IndexSmallHashSet;
            }
            instance.Fields.Add(new Field
            {
                Name = field.name,
                Type = field.type.ToLower() switch
                {
                    "int" => FieldType.Int32,
                    "int32"=> FieldType.Int32,
                    "int64" => FieldType.Int64,
                    "float" => FieldType.Float,
                    "double" => FieldType.Double,
                    "char" => FieldType.Char,
                    "string" => FieldType.ReferneceString,
                    "byte" => FieldType.Byte,
                    "bool" => FieldType.Boolean,
                    "guid" => FieldType.Guid,
                    "md5" => FieldType.MD5,
                    "datetime" => FieldType.DateTime,
                    _ => throw new Exception($"未知的字段类型: {field.type}")
                },
                ArrayLength = field.length,
                MapType = mapType
            });
        }
        await Target.RegisterInterface(instance);
    }

    public async Task<RecordIndex> insert(string interfaceName, Json record)
    {
        return await Target.Insert(interfaceName, record);
    }

    public async Task<Json> findByMaster(string interfaceName, Guid id)
    {
        return await Target.FindByMaster(interfaceName, id);
    }

    public async Task<Json> findByIndex(string interfaceName, string fieldName, object value)
    {
        if(value is Json jsonValue)
        {
            value = jsonValue.Node ?? throw new NullReferenceException();
        }
        return await Target.FindByIndex(interfaceName, fieldName, value);
    }

    public async Task<Json> findByIndexArray(string interfaceName, string fieldName, object value)
    {
        if (value is Json jsonValue)
        {
            value = jsonValue.Node ?? throw new NullReferenceException();
        }
        var addresses = await Target.GetRecordAddressesByIndexArray(interfaceName, fieldName, value);
        Json result = Json.NewArray();
        foreach (var address in addresses)
        {
            result.Add(await Target.FindByAddress(address));
        }
        return result;
    }

    public async Task<Json> findByIndexSet(string interfaceName, string fieldName, object value)
    {
        if (value is Json jsonValue)
        {
            value = jsonValue.Node ?? throw new NullReferenceException();
        }
        var addresses = await Target.GetRecordAddressesByIndexHashSet(interfaceName, fieldName, value);
        Json result = Json.NewArray();
        foreach (var address in addresses)
        {
            result.Add(await Target.FindByAddress(address));
        }
        return result;
    }

    public async Task<bool> containsByMaster(string interfaceName, Guid id)
    {
        return await Target.ContainsByMaster(interfaceName, id);
    }

    public async Task<bool> containsByIndex(string interfaceName, string fieldName, object value)
    {
        if (value is Json jsonValue)
        {
            value = jsonValue.Node ?? throw new NullReferenceException();
        }
        return await Target.ContainsByIndex(interfaceName, fieldName, value);
    }

    public async Task updatebyMaster(string interfaceName, Json record)
    {
        await Target.UpdateByMaster(interfaceName, record);
    }

    public async Task deleteByMaster(string interfaceName, Guid id)
    {
        await Target.DeleteByMaster(interfaceName, id);
    }
}

public class databaseField
{
    public static implicit operator databaseField(Json target)
    {
        return new(target);
    }

    public databaseField(Json target)
    {
        Target = target;
    }

    public Json Target { get; }

    public string name
    {
        get => Target.Read("name", string.Empty);
        set => Target.Set("name", value);
    }

    public string type
    {
        get => Target.Read("type", string.Empty);
        set => Target.Set("type", value);
    }

    public bool isIndex
    {
        get => Target.Read("isIndex", false);
        set => Target.Set("isIndex", value);
    }

    public bool isIndeArray
    {
        get => Target.Read("isIndexArray", false);
        set => Target.Set("isIndexArray", value);
    }

    public bool isIndexSet
    {
        get=> Target.Read("isIndexSet", false); 
        set => Target.Set("isIndexSet", value);
    }

    public int length
    {
        get => Target.Read("length", 0);
        set => Target.Set("length", value);
    }

    public bool isMaster
    {
        get => Target.Read("isMaster", false);
        set => Target.Set("isMaster", value);
    }
}

public class databaseFields: IEnumerable<databaseField>
{
    public static implicit operator databaseFields(Json target)
    {
        return new(target);
    }

    public databaseFields(Json target)
    {
        Target = target;
    }

    public Json Target { get; }

    public IEnumerator<databaseField> GetEnumerator()
    {
        foreach (var i in Target)
        {
            yield return i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class databaseInterface
{
    public static implicit operator databaseInterface(Json target)
    {
        return new(target);
    }

    public databaseInterface(Json target)
    {
        Target = target;
    }

    public Json Target { get; }

    public string name
    {
        get => Target.Read("name", string.Empty);
        set => Target.Set("name", value);
    }

    public databaseFields fields
    {
        get => Target.GetOrCreateArray("fields");
        set => Target.Set("fields", value);
    }
}


