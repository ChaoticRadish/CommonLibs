using Common_Util.Extensions;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common_Util.Test.Console
{
    /// <summary>
    /// 专用于控制台的测试类基类
    /// </summary>
    /// <param name="name"></param>
    public abstract class TestBase(string name) : ITest, ILogger
    {
        /// <summary>
        /// 此测试的名字
        /// </summary>
        public string Name { get => name ?? GetType().FullName ?? GetType().Name; set => name = value; }
        private string? name = name;

        private DateTime _startTime;
        public void ResetStartTime()
        {
            _startTime = DateTime.Now;
        }

        public void Run()
        {
            ResetStartTime();
            WriteLine("执行测试: " + Name);
            RunImpl();
            RunImplAsync().GetAwaiter().GetResult();
        }

        public virtual void Setup() { }
        protected abstract void RunImpl();
        protected virtual Task RunImplAsync() { return Task.CompletedTask; }

        public virtual void Finish()
        {
            WriteLine("任意按键结束");
            if (Environment.UserInteractive && !System.Console.IsInputRedirected)
            {
                System.Console.ReadKey();
            }
        }

        #region 写文本到控制台

        /// <summary>
        /// 写一行空行
        /// </summary>
        /// <param name="obj"></param>
        public void WriteLine()
        {
            System.Console.WriteLine();
        }
        /// <summary>
        /// 写一行
        /// </summary>
        /// <param name="obj"></param>
        public void WriteLine(object? obj)
        {
            System.Console.WriteLine(obj ?? "<null>");
        }
        /// <summary>
        /// 写附带时间信息的一行
        /// <para>时间信息: 距离开始运行的时间点, 以ms为单位, 精确到指定位</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="digits">位数</param>
        public void WriteTimeLine(object? obj, int digits = 0)
        {
            double time = (DateTime.Now - _startTime).TotalMilliseconds;
            System.Console.Write($"[{Math.Round(time, digits)}]");
            System.Console.WriteLine(obj ?? "<null>");
        }
        /// <summary>
        /// 写一个空行
        /// </summary>
        /// <param name="count">空行数量</param>
        public void WriteEmptyLine(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                System.Console.WriteLine();
            }
        }
        /// <summary>
        /// 写一行键值对
        /// </summary>
        /// <param name="obj"></param>
        public void WriteLine(string key, object obj, string split = "=")
        {
            System.Console.WriteLine($"{key}{split}{obj}");
        }
        /// <summary>
        /// 写一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="title"></param>
        /// <param name="multiLine">列表内容是否写成多行</param>
        /// <param name="split">分隔符, 仅在多行下生效</param>
        public void WriteList<T>(List<T> list, string title, bool multiLine = false, string split = ", ", Func<T, string>? objToString = null)
        {
            System.Console.WriteLine(title);
            if (list == null)
            {
                System.Console.WriteLine("null list");
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                System.Console.Write(objToString == null ? list[i]?.ToString() : objToString(list[i]));
                if (!multiLine && i != list.Count - 1)
                {
                    System.Console.Write(split);
                }
                else
                {
                    System.Console.WriteLine();
                }
            }
        }
        /// <summary>
        /// 写一定数量的制表符 '\t'
        /// </summary>
        /// <param name="count"></param>
        public void WriteTable(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                System.Console.Write('\t');
            }
        }
        /// <summary>
        /// 以Xml格式写对象
        /// </summary>
        /// <param name="obj"></param>
        public void WriteXml(object obj)
        {
            XmlSerializer xmlSerializer = new(obj.GetType());
            xmlSerializer.Serialize(System.Console.Out, obj);
        }
        /// <summary>
        /// 写文件数据
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteFile(string fileName, long offset = 0)
        {
            FileInfo file = new(fileName);
            if (file.Exists)
            {
                System.Console.WriteLine($"{file.FullName} (起始位置偏移 {offset} 字节):");
            }
            else
            {
                System.Console.WriteLine($"文件不存在: {fileName}");
            }

            using FileStream fileStream = file.OpenRead();
            fileStream.Seek(offset, SeekOrigin.Begin);
            int temp = 0;
            while ((temp = fileStream.ReadByte()) >= 0)
            {
                System.Console.Write((char)temp);
            }
            System.Console.WriteLine();
            System.Console.WriteLine("文件读取结束");
        }

        /// <summary>
        /// 将 byte[] 的各个元素以 int 值的形式写到控制台
        /// </summary>
        /// <param name="bs"></param>
        public void WriteAsInt(byte[] bs)
        {
            if (bs == null)
            {
                System.Console.WriteLine("byte[] is null");
                return;
            }
            else
            {
                System.Console.WriteLine($"byte[] Length = {bs.Length}");
            }
            if (bs.Length < 18)
            {
                foreach (byte b in bs)
                {
                    System.Console.Write(b);
                    System.Console.Write('\t');
                }
                System.Console.WriteLine();
                return;
            }

            for (int i = 0; i < bs.Length; i++)
            {
                System.Console.Write(bs[i]);
                System.Console.Write('\t');
                if ((i + 1) % 12 == 0 || i == bs.Length - 1)
                {
                    System.Console.WriteLine();
                }
            }

        }

        /// <summary>
        /// 写 <paramref name="obj"/> 的完整数据到控制台
        /// </summary>
        /// <param name="obj"></param>
        public virtual void WriteFull(object obj)
        {
            System.Console.WriteLine(obj.FullInfoString());
        }


        /// <summary>
        /// 写一对信息 (调用 <see cref="object.ToString"/>)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="split"></param>
        public void WritePair(object key, object? value, string split = " => ")
        {
            System.Console.WriteLine($"{key ?? "<null>"}{split}{value ?? "null"}");
        }
        public void WritePair(object value, [CallerArgumentExpression(nameof(value))] string keyText = "", string split = " => ")
        {
            WritePair(keyText, value, split);
        }
        public void WriteFullInfoPair(object value, [CallerArgumentExpression(nameof(value))] string keyText = "", string split = " => ")
        {
            WritePair(key: keyText, value: value.FullInfoString(), split);
        }
        public void WriteJsonPair(object value, [CallerArgumentExpression(nameof(value))] string keyText = "", string split = " => ")
        {
            string valueStr;
            if (value == null)
            {
                valueStr = "<null>";
            }
            else
            {
                valueStr = System.Text.Json.JsonSerializer.Serialize(value, lazyDefaultJsonOption.Value);
            }
            WritePair(key: keyText, value: valueStr, split);
        }
        private static Lazy<System.Text.Json.JsonSerializerOptions> lazyDefaultJsonOption = new(() => 
        {
            return new()
            {
                WriteIndented = true,
            };
        });
        #endregion

        #region 测试用的文件
        /// <summary>
        /// 取得建议的文件名
        /// </summary>
        /// <param name="suffix"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetSuggestFilePath(string suffix, string? index = null)
        {
            StringBuilder sb = new();
            string typeName = GetType().Name;
            string? entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (entryAssemblyName != null && typeName.StartsWith(entryAssemblyName))
            {
                typeName = typeName[(entryAssemblyName.Length + 1)..];
            }
            string dir = AppDomain.CurrentDomain.BaseDirectory + "/TestFile";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            sb.Append(dir).Append('/').Append(typeName);
            if (!string.IsNullOrEmpty(index))
            {
                sb.Append('-').Append(index);
            }
            if (!string.IsNullOrEmpty(suffix))
            {
                sb.Append('.').Append(suffix);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 取得当前测试对应的文件夹
        /// </summary>
        /// <returns></returns>
        public string GetTestDir()
        {
            string dir = GetType().FullName ?? string.Empty;
            string? entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (entryAssemblyName != null && dir.StartsWith(entryAssemblyName))
            {
                dir = dir[(entryAssemblyName.Length + 1)..];
            }
            dir = dir.Trim('.');
            string fullDir = AppDomain.CurrentDomain.BaseDirectory + "/TestDirs/" + dir;
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }
            return fullDir;
        }
        #endregion

        /// <summary>
        /// 以通用的逻辑, 运行当前测试类型中的一个方法, 不支持传入参数
        /// </summary>
        /// <param name="methodName"></param>
        protected void RunTest(string methodName)
        {
            Type type = GetType();
            var method = type.GetMethod(methodName,
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Instance) ?? throw new Exception("未找到匹配的方法: " + methodName);
            if (method.GetParameters().Length > 0) throw new Exception("不支持传入参数到方法");
            Action action = () =>
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    method.Invoke(this, null);
                }
            };
            string testInfo = methodName;
            method.ExistCustomAttribute<TestMethodAttribute>((attr) =>
            {
                if (attr.TestInfo.IsNotEmpty())
                {
                    testInfo = attr.TestInfo;
                }
            });
            RunTest(action, testInfo);
        }

        /// <summary>
        /// 以通用逻辑, 运行当前测试类型中, 具有 <see cref="TestMethodAttribute"/> 标记的所有方法
        /// </summary>
        protected void RunTestMark()
        {
            Type type = GetType();
            var methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.ExistCustomAttribute<TestMethodAttribute>(out var attr))
                {
                    Action action = () =>
                    {
                        if (method.IsStatic)
                        {
                            method.Invoke(null, null);
                        }
                        else
                        {
                            method.Invoke(this, null);
                        }
                    };
                    RunTest(action, attr.TestInfo.WhenEmptyDefault(method.Name));
                }
            }

        }

        /// <summary>
        /// 以通用的逻辑, 运行传入方法
        /// </summary>
        /// <param name="test"></param>
        protected virtual void RunTest(Action test, string testInfo, int runCount = 1, bool outputUsingTime = false)
        {
            WriteEmptyLine();
            WriteLine($"[ {testInfo} ] 执行测试: 开始");

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                for (int i = 0; i < runCount || i == 0; i++)
                {
                    test();
                }
                stopwatch.Stop();
                if (outputUsingTime)
                {
                    WriteLine($"[ {testInfo} ] 执行测试: 耗时 {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
                }
                else
                {
                    WriteLine($"[ {testInfo} ] 执行测试: 结束");
                }
            }
            catch (Exception ex)
            {

                WriteLine($"[ {testInfo} ] 执行测试: 发生异常! \n{ex}");
            }

            WriteEmptyLine();
        }
        protected void RunTest<T>(Action<T> test, T arg, string testInfo)
        {
            RunTest(() => test(arg), testInfo);
        }



        /// <summary>
        /// 以通用的逻辑, 运行传入方法
        /// </summary>
        /// <param name="test"></param>
        /// <param name="testInfo"></param>
        /// <param name="toString">返回结果要使用 <see cref="object.ToString"/> 还是 <see cref="ObjectExtensions.FullInfoString(object)"/> 显示信息</param>
        protected virtual void RunTest<TResult>(Func<TResult> test, string testInfo, bool toString = true)
        {
            WriteEmptyLine();
            WriteLine($"[ {testInfo} ] 执行测试: 开始");

            try
            {
                TResult result = test();
                WriteLine($"[ {testInfo} ] 执行测试: 结束");
                WriteLine($"[ {testInfo} ] 测试结果: \n{(result == null ? "<null>" : (toString ? result.ToString() : result.FullInfoString()))}");
            }
            catch (Exception ex)
            {

                WriteLine($"[ {testInfo} ] 执行测试: 发生异常! \n{ex}");
            }

            WriteEmptyLine();
        }


        #region ILogger
        public ILevelLogger GetLevelLogger(string category, string subcategory = "") => LevelLoggerHelper.LogTo(this, category, subcategory);
        private LogStringBuilder logBuilder = new()
        {
            Config = new()
            {
                HeadSameLine = false,
            }
        };
        public LogStringBuilderConfig LogBuilderConfig { get => logBuilder.Config; }
        public bool ILoggerOutEmptyLine { get; set; } = true;
        public void Log(LogData log)
        {
            WriteLine(logBuilder.Build(log));
            if (ILoggerOutEmptyLine)
            {
                WriteLine();
            }
        }
        #endregion
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute(string? testInfo = null) : Attribute
    {
        public string TestInfo { get; } = testInfo ?? string.Empty;
    }
}
