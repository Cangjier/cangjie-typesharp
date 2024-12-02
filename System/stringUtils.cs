using Cangjie.TypeSharp.BasicType;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public static class stringUtils
{
    public static string trimEnd(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trimEnd(value);
    }

    public static string trimStart(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trimStart(value);
    }

    public static string trim(Json self, Json value)
    {
        return new JsonWrapper(self.Node).trim(value);
    }
}
