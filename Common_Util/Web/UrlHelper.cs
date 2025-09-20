using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common_Util.Web
{
    /// <summary>
    /// 请求路径的帮助类
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>
        /// 解析 URL 得到其中的数据
        /// </summary>
        /// <remarks>
        /// 适用于 {协议}://{IP/域名}[:{端口号}][/请求URL][?参数=值&参数=值] 格式
        /// </remarks>
        /// <param name="url">必须是符合标准的 url 字符串, 比如参数出现特殊符号时, 需要已经经过转义</param>
        /// <returns></returns>
        public static bool TryParseHttp(string? url, [NotNullWhen(true)] out HttpUrlInfo urlInfo)
        {
            if (string.IsNullOrEmpty(url))
                goto Failure;

            int readIndex = 0;

            string? found = null;
            
            string? protocol = null;
            string? host = null;

            string? portStr = null;
            int portInt = default;
            bool existPort = false;

            string? path = null;
            string[]? pathArr = null;
            bool existPath = false;

            string? query = null;
            List<KeyValuePair<string, string>>? queryArgs = null;
            bool existQuery = false;

            string? fragment = null;
            bool existFragment = false;

            if (!StringAnalysis.TryReadUntil(url, "://", readIndex, null, out protocol)
                || string.IsNullOrWhiteSpace(protocol))
            {
                goto Failure;
            }
            readIndex += protocol.Length + "://".Length;

            bool backFromIpv6 = false;  
        ReadHost:
            {
                if (backFromIpv6)   // 读取 IPv6 地址后回退, 以读取下一部分的标识
                {
                    if (!StringAnalysis.TryReadUntilOrEnd(url, [":", "/", "?", "#"], readIndex, null, out found, out string? temp))
                    {
                        goto Failure;
                    }
                    if (!string.IsNullOrEmpty(temp))
                    {
                        // IPv6 地址后面出现了其他文本
                        goto Failure;
                    }
                }
                else
                {
                    if (!StringAnalysis.TryReadUntilOrEnd(url, [":", "/", "?", "#", "["], readIndex, null, out found, out host))
                    {
                        goto Failure;
                    }
                    if (found != "[")
                    {
                        if (!string.IsNullOrEmpty(host))
                        {
                            readIndex += host.Length;
                        }
                        else
                        {
                            goto Failure;
                        }
                    }
                }
                switch (found)
                {
                    case null:
                        goto Combining;
                    case ":":
                        readIndex += 1;
                        goto ReadPort;
                    case "/":
                        // readIndex += 1; // 不需要跳过一个字符, 因为这是路径字符串的一部分
                        goto ReadPath;
                    case "?":
                        readIndex += 1;
                        goto ReadQuery;
                    case "#":
                        readIndex += 1;
                        goto ReadFragment;
                    case "[":
                        readIndex += 1;
                        if (!StringAnalysis.TryReadUntilOrEnd(url, ["]"], readIndex, null, out found, out host))
                        {
                            goto Failure;
                        }
                        readIndex += host.Length;
                        switch (found)
                        {
                            case null:  // 必须读取到 ], 表示 IPv6 地址的结束
                                goto Failure;
                            case "]":
                                readIndex += 1;
                                if (string.IsNullOrEmpty(host))
                                {
                                    goto Failure;
                                }
                                backFromIpv6 = true;
                                goto ReadHost;
                            default:
                                throw new Exceptions.General.ImpossibleForkException($"读取 IPv6 Host 未处理分支情况: {found}");
                        }

                    default:
                        throw new Exceptions.General.ImpossibleForkException($"读取 Host 未处理分支情况: {found}");
                }
            }

        ReadPort:
            {
                if (!StringAnalysis.TryReadUntilOrEnd(url, ["/", "?", "#"], readIndex, null, out found, out portStr))
                {
                    goto Failure;
                }
                if (!string.IsNullOrEmpty(portStr))
                {
                    if (!int.TryParse(portStr, out portInt)) goto Failure;
                    existPort = true;
                }
                readIndex += portStr.Length;
                switch (found)
                {
                    case null:
                        goto Combining;
                    case "/":
                        // readIndex += 1; // 不需要跳过一个字符, 因为这是路径字符串的一部分
                        goto ReadPath;
                    case "?":
                        readIndex += 1;
                        goto ReadQuery;
                    case "#":
                        readIndex += 1;
                        goto ReadFragment;
                    default:
                        throw new Exceptions.General.ImpossibleForkException($"读取 Port 未处理分支情况: {found}");
                }
            }

        ReadPath:
            {
                if (!StringAnalysis.TryReadUntilOrEnd(url, ["?", "#"], readIndex, null, out found, out path))
                {
                    goto Failure;
                }
                readIndex += path.Length;
                if (!string.IsNullOrEmpty(path))
                {
                    if (!splitPath(path, out pathArr)) goto Failure;
                    existPath = true;
                }
                switch (found)
                {
                    case null:
                        goto Combining;
                    case "?":
                        readIndex += 1;
                        goto ReadQuery;
                    case "#":
                        readIndex += 1;
                        goto ReadFragment;
                    default:
                        throw new Exceptions.General.ImpossibleForkException($"读取 Path 未处理分支情况: {found}");
                }
            }

        ReadQuery:
            {
                if (!StringAnalysis.TryReadUntilOrEnd(url, ["#"], readIndex, null, foundStr: out found, out query))
                {
                    goto Failure;
                }
                readIndex += query.Length;
                if (!string.IsNullOrEmpty(query))
                {
                    if (!splitQuery(query, out queryArgs)) goto Failure;
                    existQuery = true;
                }
                switch (found)
                {
                    case null:
                        goto Combining;
                    case "#":
                        readIndex += 1;
                        goto ReadFragment;
                    default:
                        throw new Exceptions.General.ImpossibleForkException($"读取 Query 未处理分支情况: {found}");
                }
            }

        ReadFragment:
            {
                fragment = url.Substring(readIndex);
                existFragment = true;
            }

        Combining:
            if (string.IsNullOrEmpty(host))
            {
                throw new Exceptions.General.ImpossibleForkException($"读取阶段代码异常, Host 此时不应为空");
            }
            urlInfo = new HttpUrlInfo()
            {
                Protocol = protocol,
                Host = host,
                Port = existPort ? portInt : null,
                Path = existPath ? (pathArr ?? []) : [],
                QueryArgs = existQuery ? (queryArgs ?? []) : [],
                Fragment = existFragment ? fragment : null
            };
            return true;
        Failure:
            urlInfo = default;
            return false;
        }

        /// <summary>
        /// 切分访问路径字符串
        /// </summary>
        /// <param name="path">已经是 url 编码的字符串, http / ftp 的 url 中访问路径的部分, 例如: /user/my/test, 非空时需要以斜杠 / 开头</param>
        /// <param name="output">拆分结果, 其中的值已经过反转义. 结果示例: ["user","my","test"]</param>
        /// <returns></returns>
        public static bool SplitPath(string? path, [MaybeNullWhen(false)] out string[] output)
            => splitPath(path, out output);
        private static bool splitPath(string? path, [MaybeNullWhen(false)] out string[] output)
        {
            if (string.IsNullOrEmpty(path))
            {
                output = [];
                return true;
            }
            if (!path.StartsWith('/'))
            {
                output = null;
                return false;
            }
            output = path.Split('/').Skip(1).Select(part => Uri.UnescapeDataString(part)).ToArray();
            return true;
        }
        /// <summary>
        /// 比较两组路径是否相同
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool PathEquals(IList<string>? path1, IList<string>? path2)
        {
            if (path1 == null && path2 == null) return true;
            else if (path1 != null && path2 == null) return false;
            else if (path1 == null && path2 != null) return false;
            else
            {
                if (path1!.Count != path2!.Count) return false;
                for (int index = 0; index < path1.Count; index++)
                {
                    if (path1[index] != path2[index]) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 切分查询参数字符串
        /// </summary>
        /// <param name="query">
        /// 已经是 url 编码的字符串, http 的 url 中查询参数的部分, 例如: arg0=1&amp;arg1=test&amp;arg2=yes<br/>
        /// 当没有等于号 = 时, 将整个都视为键, 值则取空字符串<br/>
        /// 当出现空段时, 则键值均取空字符串
        /// </param>
        /// <param name="output">拆分结果, 其中的值已经经过反转义. 结果示例: [{"arg0":"1"},{"arg1":"test"},{"arg2":"yes"}]</param>
        /// <returns></returns>
        public static bool SplitQuery(string? query, [MaybeNullWhen(false)] out List<KeyValuePair<string, string>> output)
            => splitQuery(query, out output);
        private static bool splitQuery(string? query, [MaybeNullWhen(false)] out List<KeyValuePair<string, string>> output)
        {
            if (string.IsNullOrEmpty(query))
            {
                output = [];
                return true;
            }
            string[] argStrs = query.Split('&');
            output = [];
            string key;
            string value;
            foreach (string pair in argStrs)
            {
                if (pair.Length == 0)
                {
                    key = string.Empty;
                    value = string.Empty;
                }
                else
                {
                    var splitIndex = pair.IndexOf('=');
                    if (splitIndex >= 0)
                    {
                        key = pair.Substring(0, splitIndex);
                        value = pair.Substring(splitIndex + 1);
                    }
                    else
                    {
                        key = pair;
                        value = string.Empty;
                    }
                }
                output.Add(new KeyValuePair<string, string>(Uri.UnescapeDataString(key), Uri.UnescapeDataString(value)));
            }

            return true;
        }
        /// <summary>
        /// 比较两组参数是否相同
        /// </summary>
        /// <remarks>
        /// 参数顺序需要相同
        /// </remarks>
        /// <param name="queryArgs1"></param>
        /// <param name="queryArgs2"></param>
        /// <returns></returns>
        public static bool QueryEquals(IList<KeyValuePair<string, string>>? queryArgs1, IList<KeyValuePair<string, string>>? queryArgs2)
        {
            if (queryArgs1 == null && queryArgs2 == null) return true;
            else if (queryArgs1 != null && queryArgs2 == null) return false;
            else if (queryArgs1 == null && queryArgs2 != null) return false;
            else
            {
                if (queryArgs1!.Count != queryArgs2!.Count) return false;
                for (int index = 0; index < queryArgs1.Count; index++)
                {
                    if (queryArgs1[index].Key != queryArgs2[index].Key || queryArgs1[index].Value != queryArgs2[index].Value) return false;
                }
                return true;
            }
        }
    }
}
