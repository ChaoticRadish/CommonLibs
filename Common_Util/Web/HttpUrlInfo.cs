using Common_Util.Interfaces.Behavior;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common_Util.Web
{
    /// <summary>
    /// Http 请求路径信息结构体
    /// </summary>
    public readonly struct HttpUrlInfo : ICompareEquivalentTo<HttpUrlInfo>
    {
        /// <summary>
        /// 表达空的值
        /// </summary>
        public static readonly HttpUrlInfo Empty = new HttpUrlInfo();
        /// <summary>
        /// 判断当前值是否表达: "空"
        /// </summary>
        public bool IsEmpty =>
            string.IsNullOrEmpty(Protocol)
            && string.IsNullOrEmpty(Host)
            && (Port == null || Port == 0)
            && Path == null
            && (QueryArgs == null || !QueryArgs.Any());

        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol { get; init; }
        /// <summary>
        /// 域名 / IP
        /// </summary>
        public string Host { get; init; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int? Port { get; init; }
        /// <summary>
        /// 路径, 其中的值已经过反转义
        /// </summary>
        /// <remarks>
        /// [] 表示空路径, [""] 表示根路径 "/", ["help",""] 表示路径 /help/ <br/>
        /// 允许中间包含空路径, 以产生双斜杠, 如 ["help", "", "get"] 表示 /help//get
        /// </remarks>
        public string[] Path { get; init; }
        /// <summary>
        /// 查询参数, 其中的键值均已经过反转义
        /// </summary>
        /// <remarks>
        /// 即使出现重复, 也会将其保留, 同时保持原有顺序
        /// </remarks>
        public IReadOnlyList<KeyValuePair<string, string>> QueryArgs { get; init; }
        /// <summary>
        /// 片段
        /// </summary>
        /// <remarks>
        /// Url 的片段部分, 存储原始值
        /// </remarks>
        public string? Fragment { get; init; }

        /// <summary>
        /// 将 <see cref=QueryArgs""/> 转换为字典, 去除其中重复参数, 仅保留首个值
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, string> GetQueryArgsDict()
        {
            return QueryArgs.GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.First().Value);
        }
        #region 字符串相关
        /// <summary>
        /// 取得基础 URL 字符串
        /// </summary>
        /// <returns></returns>
        public string BaseUrlString()
        {
            StringBuilder output = new();
            output.Append(Protocol).Append("://").Append(Host);
            if (Port != null)
            {
                output.Append(':').Append(Port);
            }
            return output.ToString();
        }
        /// <summary>
        /// 取得请求目标字符串
        /// </summary>
        /// <remarks>
        /// 包含 <see cref="Path"/> 和 <see cref="QueryArgs"/> 两部分
        /// </remarks>
        /// <returns></returns>
        public string RequestTargetString()
        {
            StringBuilder output = new();
            if (Path.Length > 0)
            {
                foreach (string path in Path)
                {
                    output.Append('/');
                    if (!string.IsNullOrEmpty(path))
                    {
                        output.Append(Uri.EscapeDataString(path));
                    }
                }
            }
            if (QueryArgs.Any())
            {
                output.Append('?');
                int i = 0;
                foreach (var kvp in QueryArgs)
                {
                    if (i > 0)
                    {
                        output.Append('&');
                    }
                    output.Append(HttpUtility.HtmlEncode(kvp.Key)).Append('=').Append(Uri.EscapeDataString(kvp.Value));
                }
            }
            return output.ToString();
        }

        public override string ToString()
        {
            StringBuilder output = new();
            output.Append(Protocol).Append("://");
            if (!Host.Contains(':'))
            {
                output.Append(Host);
            }
            else
            {
                output.Append('[').Append(Host).Append(']');
            }
            if (Port != null)
            {
                output.Append(':').Append(Port);
            }
            if (Path != null && Path.Length > 0)
            {
                foreach (string path in Path)
                {
                    output.Append('/');
                    if (!string.IsNullOrEmpty(path))
                    {
                        output.Append(Uri.EscapeDataString(path));
                    }
                }
            }
            if (QueryArgs != null && QueryArgs.Any())
            {
                output.Append('?');
                int i = 0;
                foreach (var kvp in QueryArgs)
                {
                    if (i > 0)
                    {
                        output.Append('&');
                    }
                    output.Append(HttpUtility.HtmlEncode(kvp.Key)).Append('=').Append(Uri.EscapeDataString(kvp.Value));
                    i++;
                }
            }
            return output.ToString();
        }
        #endregion

        #region 比较
        /// <summary>
        /// http 协议下使用的缺省值
        /// </summary>
        public static int? DefaultHttpPort { get; set; } = 80;
        /// <summary>
        /// https 协议下使用的缺省值
        /// </summary>
        public static int? DefaultHttpsPort { get; set; } = 443;
        private static bool compareEquivalent(string? a, string? b)
            => (a?.ToLower() ?? string.Empty) == (b?.ToLower() ?? string.Empty);
        private static bool compareEqual(string? a, string? b)
            => (a ?? string.Empty) == (b ?? string.Empty);
        private readonly int? getOrDefaultPort()
        {
            return Port ?? Protocol?.ToLower() switch
            {
                "http" => DefaultHttpPort,
                "https" => DefaultHttpsPort,
                _ => null,
            };
        }

        public bool IsEquivalent(HttpUrlInfo other)
        {
            if (GetHashCode() != other.GetHashCode()) return false;
            if (!(compareEquivalent(Protocol, other.Protocol)
                && compareEquivalent(Host, other.Host)
                && (getOrDefaultPort() == other.getOrDefaultPort())))
                return false;
            if ((Path != null && other.Path == null)
                || (Path == null && other.Path != null))
                return false;
            if (Path != null && other.Path != null)
            {
                if (Path.Length != other.Path.Length) return false;
                for (int i = 0; i < Path.Length; i++)
                {
                    if (Path[i] != other.Path[i])
                        return false;
                }
            }
            if ((QueryArgs != null && other.QueryArgs == null)
                || (QueryArgs == null && other.QueryArgs != null))
                return false;
            if (QueryArgs != null && other.QueryArgs != null)
            {
                if (QueryArgs.Count != other.QueryArgs.Count) return false;
                for (int i = 0; i < QueryArgs.Count; i++)
                {
                    if (QueryArgs[i].Key != other.QueryArgs[i].Key
                        || QueryArgs[i].Value != other.QueryArgs[i].Value)
                        return false;
                }
            }
            if (Fragment != other.Fragment) return false;
            return true;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is HttpUrlInfo other)
            {
                if (GetHashCode() != other.GetHashCode()) return false;
                if (!(compareEqual(Protocol, other.Protocol)
                    && compareEqual(Host, other.Host)
                    && (Port == other.Port)))
                    return false;
                if ((Path != null && other.Path == null)
                    || (Path == null && other.Path != null))
                    return false;
                if (Path != null && other.Path != null)
                {
                    if (Path.Length != other.Path.Length) return false;
                    for (int i = 0; i < Path.Length; i++)
                    {
                        if (Path[i] != other.Path[i]) 
                            return false;
                    }
                }
                if ((QueryArgs != null && other.QueryArgs == null)
                    || (QueryArgs == null && other.QueryArgs != null))
                    return false;
                if (QueryArgs != null && other.QueryArgs != null)
                {
                    if (QueryArgs.Count != other.QueryArgs.Count) return false;
                    for (int i = 0; i < QueryArgs.Count; i++)
                    {
                        if (QueryArgs[i].Key != other.QueryArgs[i].Key
                            || QueryArgs[i].Value != other.QueryArgs[i].Value) 
                            return false;
                    }
                }
                if (Fragment != other.Fragment) return false;
                return true;
            } 
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            if (IsEmpty) return 0;
            HashCode hc = new HashCode();
            hc.Add(Protocol?.ToLower());
            hc.Add(Host?.ToLower());
            hc.Add(getOrDefaultPort());
            hc.Add(Path == null ? 1 : 0);
            hc.Add(Path?.Length ?? 0);
            hc.Add(QueryArgs?.Count ?? 0);
            hc.Add(Fragment);
            return hc.ToHashCode();
        }


        public static bool operator ==(HttpUrlInfo left, HttpUrlInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HttpUrlInfo left, HttpUrlInfo right)
        {
            return !(left == right);
        }

        #endregion
    }
}
