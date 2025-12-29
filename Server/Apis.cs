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
        public static Api Response { get; } = new("/api/v1/response", "/api/v1/response");

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
                public static Api Register { get; } = new("/api/v1/agents/register", "/api/v1/agents/register");

                /// <summary>
                /// 更新性能信息
                /// </summary>
                public static Api UpdatePerformance { get; } = new("/api/v1/agents/updateperformance", "/api/v1/agents/updateperformance");

                /// <summary>
                /// 更新插件信息
                /// </summary>
                public static Api UpdatePlugins { get; } = new("/api/v1/agents/updateplugins", "/api/v1/agents/updateplugins");

                /// <summary>
                /// 获取所有代理人
                /// </summary>
                public static Api Get { get; } = new("/api/v1/agents/get", "/api/v1/agents/get");

                /// <summary>
                /// 安装包
                /// </summary>
                public static Api InstallPackage { get; } = new("/api/v1/agents/installpackage", "/api/v1/agents/installpackage");
            }

            /// <summary>
            /// 客户端接口
            /// </summary>
            public class Client
            {
                /// <summary>
                /// 运行任务
                /// </summary>
                public static Api Run { get; } = new("/api/v1/agents/run", "/api/v1/agents/run");

                /// <summary>
                /// 安装包
                /// </summary>
                public static Api InstallPackage { get; } = new("/api/v1/agents/agent/installpackage", "/api/v1/agents/agent/installpackage");
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
            public static Api Run { get; } = new("/api/v1/tasks/run", "/api/v1/tasks/run");

            /// <summary>
            /// 发起任务，异步返回
            /// </summary>
            public static Api RunAsync { get; } = new("/api/v1/tasks/runasync", "/api/v1/tasks/runasync");

            /// <summary>
            /// 查询任务
            /// </summary>
            public static Api Query { get; } = new("/api/v1/tasks/query", "/api/v1/tasks/query");

            /// <summary>
            /// 发起插件任务，异步返回
            /// </summary>
            public static Api PluginRunAsync { get; } = new("/api/v1/tasks/plugin/runasync", "/api/v1/tasks/plugin/runasync");

            /// <summary>
            /// 更新进度
            /// </summary>
            public static Api UpdateProgress { get; } = new("/api/v1/tasks/updateprogress", "/api/v1/tasks/updateprogress");

            /// <summary>
            /// 订阅进度
            /// </summary>
            public static Api SubscribeProgress { get; } = new("/api/v1/tasks/subscribeprogress", "/api/v1/tasks/subscribeprogress");
        }

        /// <summary>
        /// WebApp接口
        /// </summary>
        public class WebApp
        {
            /// <summary>
            /// 广播消息
            /// </summary>
            public static Api Broadcast { get; } = new("/api/v1/app/broadcast", "/api/v1/app/broadcast");
        }
    }
}
