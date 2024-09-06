using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    /// <summary>
    /// 默认名帮助类 (线程不安全)
    /// </summary>
    public class DefaultNameHelper
    {
        #region 配置
        /// <summary>
        /// 需要替换为计数的标志
        /// </summary>
        public string ReplaceMark { get; set; } = "$$$";
        /// <summary>
        /// 计数转换为数字之后, 如果长度较短, 需要补字符到多少长度
        /// </summary>
        public int PadLeftToLength { get; set; } = 0;
        /// <summary>
        /// 需要补字符时, 使用什么字符来补
        /// </summary>
        public char PadLeftChar { get; set; } = '0';
        #endregion

        /// <summary>
        /// 字典: 键:名字格式 值:下一次计数需使用值
        /// </summary>
        public Dictionary<string, int> CounterDic { get; private set; } = new Dictionary<string, int>();
        /// <summary>
        /// 字典: 键:配置名 值:名字格式
        /// </summary>
        public Dictionary<string, string> FormatDic { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// 以输入和计数, 生成一个名字
        /// </summary>
        /// <param name="format"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual string Create(string format, int count)
        {
            if (format.Length == 0 || format.Length < ReplaceMark.Length) return format;    // 显然无法包含标志
            int index = format.IndexOf(ReplaceMark);
            if (index < 0) return format;   // 没有找到标志

            string countStr = count.ToString();
            if (PadLeftToLength > 0 || countStr.Length < PadLeftToLength)
            {
                countStr.PadLeft(PadLeftToLength, PadLeftChar);
            }

            return $"{format.Substring(0, index)}{countStr}{format.Substring(index + ReplaceMark.Length)}";

        }

        /// <summary>
        /// 获取默认的格式字符串
        /// </summary>
        /// <param name="name">配置名</param>
        /// <returns></returns>
        protected virtual string DefaultFormat(string name)
        {
            return $"{name} {ReplaceMark}";
        }

        /// <summary>
        /// 注册配置对应的格式和初始计数
        /// </summary>
        /// <param name="name">配置名</param>
        /// <param name="format">格式字符串</param>
        /// <param name="startCount">初始计数</param>
        public void Register(string name, string? format, int startCount)
        {
            // 检查输入
            if (name.IsEmpty())
            {
                throw new ArgumentException("输入的配置名为空字符串", nameof(name));
            }
            // 清理已有配置和计数
            if (FormatDic.ContainsKey(name))
            {
                string _f = FormatDic[name];
                CounterDic.Remove(_f);
                FormatDic.Remove(name);
            }
            format = format.WhenEmptyDefault(DefaultFormat(name));
            FormatDic.Add(name, format);
            CounterDic.Add(format, startCount);
        }

        /// <summary>
        /// 获取指定配置的下一个计数的名字
        /// </summary>
        /// <param name="name"></param>
        /// <param name="registerAsDefaultFormat">如果配置名未注册, 使用默认的格式注册</param>
        /// <returns></returns>
        public string Next(string name, bool registerAsDefaultFormat = true)
        {
            // 检查输入
            if (name.IsEmpty())
            {
                throw new ArgumentException("输入的配置名为空字符串", nameof(name));
            }
            if (!FormatDic.ContainsKey(name))
            {
                if (registerAsDefaultFormat)
                {
                    Register(name, DefaultFormat(name), 0);
                }
                else
                {
                    throw new Exception("未注册配置名");
                }
            }
            string format = FormatDic[name];
            string output;

            int currentCount = CounterDic[format];
            output = Create(format, currentCount);
            CounterDic[format]++;

            return output;
        }


        #region 静态
        private static DefaultNameHelper _staticHelper = new DefaultNameHelper();
        /// <summary>
        /// 使用静态的默认名帮助对象生成输入名字的下一个默认名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Default(string name)
        {
            if (!_staticHelper.FormatDic.ContainsKey(name))
            {
                _staticHelper.Register(name, null, 0);
            }
            return _staticHelper.Next(name);
        }
        #endregion
    }

}
