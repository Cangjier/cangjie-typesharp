using TidyHPC.LiteDB;
using TidyHPC.LiteDB.Metas;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Server.TaskQueues.Configs;

/// <summary>
/// 数据库
/// </summary>
public struct KeyValueInterface(Json target)
{
    /// <summary>
    /// 
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// 数据库中的接口名
    /// </summary>
    public static string InterfaceName { get; } = "/v2/KeyValue";

    /// <summary>
    /// DB接口描述
    /// </summary>
    public static ObjectInterface Interface { get; } = new ObjectInterface().Initialize(item =>
    {
        item.FullName = InterfaceName;
        item.AddMasterField("Id");
        item.AddIndexField("Key", FieldType.ReferneceString);
        item.AddField("Value", FieldType.ReferneceString);
    });

    /// <summary>
    /// 数据库主键
    /// </summary>
    public Guid id { get; set; } = Guid.Empty;

    /// <summary>
    /// 键名
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 键值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 从Json中反序列化
    /// </summary>
    /// <param name="self"></param>
    public void DeserializeFromJson(Json self)
    {
        id = Guid.Parse(self.Read("Id", string.Empty));
        Key = self.Read("Key", string.Empty);
        Value = self.Read("Value", string.Empty);
    }

    /// <summary>
    /// 序列化到Json
    /// </summary>
    /// <param name="self"></param>
    public void SerializeToJson(Json self)
    {
        self["Id"] = id.ToString();
        self["Key"] = Key;
        self["Value"] = Value;
    }

    /// <summary>
    /// Convert to string with json format
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var json = Json.NewObject();
        SerializeToJson(json);
        var result = json.ToString();
        return result;
    }

    /// <summary>
    /// 根据key查找
    /// </summary>
    /// <param name="db"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<KeyValueInterface?> FindByKey(Database db, string key)
    {
        Json json = Json.Null;
        try
        {
            json = await db.FindByIndex(InterfaceName, "Key", key);
        }
        catch
        {
            json = Json.Null;
        }
        if (json.IsNull)
        {
            return null;
        }
        var item = new KeyValueInterface();
        item.DeserializeFromJson(json);
        return item;
    }

    /// <summary>
    /// 插入到数据库
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task Insert(Database db)
    {
        var index = await db.Insert(InterfaceName, Json.Parse(ToString()));
        id = index.Master;
    }

    /// <summary>
    /// 更新到数据库
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task Update(Database db)
    {
        await db.UpdateByMaster(InterfaceName, Json.Parse(ToString()));
    }

    /// <summary>
    /// 删除该数据
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task Delete(Database db)
    {
        await db.DeleteByMaster(InterfaceName, id);
    }
}
