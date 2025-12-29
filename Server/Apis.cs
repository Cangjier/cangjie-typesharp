using System.Web;

namespace Cangjie.TypeSharp.Server;

/// <summary>
/// Api集合
/// </summary>
public class Apis
{
    /// <summary>
    /// Api接口描述
    /// </summary>
    /// <param name="url"></param>
    /// <param name="pattern"></param>
    public record Api(string url, string pattern)
    {
        /// <summary>
        /// Implicit conversion from Api to string
        /// </summary>
        /// <param name="api"></param>
        public static implicit operator string(Api api) => api.url;

        private bool IsContainsQuery() => url.Contains('?');

        /// <summary>
        /// 拼接查询参数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Api WithQuery(string query) => IsContainsQuery() ?
            new($"{url}&{query}", pattern) :
            new($"{url}?{query}", pattern);

        /// <summary>
        /// 拼接查询参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Api WithQuery(string key, string value) => IsContainsQuery() ?
            new($"{url}&{key}={HttpUtility.UrlEncode(value)}", pattern) :
            new($"{url}?{key}={HttpUtility.UrlEncode(value)}", pattern);

        /// <summary>
        /// 拼接前缀
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Api WithPrefix(string url)
            => new($"{url.TrimEnd('/')}{this.url}", pattern);

    }

    /// <summary>
    /// V2版本
    /// </summary>
    public class V2
    {
        /// <summary>
        /// 响应结果
        /// </summary>
        public static Api Response { get; } = new("/api/V2/response", "/api/V2/response");

        /// <summary>
        /// 代理服务
        /// </summary>
        public class Agents
        {
            /// <summary>
            /// 服务端接口
            /// </summary>
            public class Server
            {
                /// <summary>
                /// 注册代理人
                /// </summary>
                public static Api Register { get; } = new("/api/V2/agents/register", "/api/V2/agents/register");

                /// <summary>
                /// 更新性能信息
                /// </summary>
                public static Api UpdatePerformance { get; } = new("/api/V2/agents/updateperformance", "/api/V2/agents/updateperformance");

                /// <summary>
                /// 更新插件信息
                /// </summary>
                public static Api UpdatePlugins { get; } = new("/api/V2/agents/updateplugins", "/api/V2/agents/updateplugins");

                /// <summary>
                /// 获取所有代理人
                /// </summary>
                public static Api Get { get; } = new("/api/V2/agents/get", "/api/V2/agents/get");

                /// <summary>
                /// 安装包
                /// </summary>
                public static Api InstallPackage { get; } = new("/api/V2/agents/installpackage", "/api/V2/agents/installpackage");
            }

            /// <summary>
            /// 客户端接口
            /// </summary>
            public class Client
            {
                /// <summary>
                /// 运行任务
                /// </summary>
                public static Api Run { get; } = new("/api/V2/agents/run", "/api/V2/agents/run");

                /// <summary>
                /// 安装包
                /// </summary>
                public static Api InstallPackage { get; } = new("/api/V2/agents/agent/installpackage", "/api/V2/agents/agent/installpackage");
            }
        }

        /// <summary>
        /// 任务服务
        /// </summary>
        public class Tasks
        {
            /// <summary>
            /// 运行任务，同步返回
            /// </summary>
            public static Api Run { get; } = new("/api/V2/tasks/run", "/api/V2/tasks/run");

            /// <summary>
            /// 发起任务，异步返回
            /// </summary>
            public static Api RunAsync { get; } = new("/api/V2/tasks/runasync", "/api/V2/tasks/runasync");

            /// <summary>
            /// 查询任务
            /// </summary>
            public static Api Query { get; } = new("/api/V2/tasks/query", "/api/V2/tasks/query");

            /// <summary>
            /// 发起插件任务，异步返回
            /// </summary>
            public static Api PluginRunAsync { get; } = new("/api/V2/tasks/plugin/runasync", "/api/V2/tasks/plugin/runasync");

            /// <summary>
            /// 更新进度
            /// </summary>
            public static Api UpdateProgress { get; } = new("/api/V2/tasks/updateprogress", "/api/V2/tasks/updateprogress");

            /// <summary>
            /// 订阅进度
            /// </summary>
            public static Api SubscribeProgress { get; } = new("/api/V2/tasks/subscribeprogress", "/api/V2/tasks/subscribeprogress");
        }
    }
}
