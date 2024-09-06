using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common_Util.Log
{
    /// <summary>
    /// 日志输出的配置
    /// </summary>
    public class LogOutputConfig<TConfigItem>
        where TConfigItem : LogOutputConfigItemBase
    {

        /// <summary>
        /// <para>是否允许输出所有日志</para>
        /// <para>就算允许, 也会按照忽略的配置移除掉部分, 相当于黑名单</para>
        /// </summary>
        public bool AllOutput
        {
            get => allOutput;
            set
            {
                allOutput = value;
                IsTidyDone = false;
            }
        }
        private bool allOutput = true;

        /// <summary>
        /// 默认的配置项, 在没有找到对应的配置项时, 将使用此配置项. 如果此时此项为 null, 将被忽略
        /// </summary>
        public TConfigItem? DefaultConfigItem
        {
            get => defaultConfigItem;
            set
            {
                defaultConfigItem = value;
                IsTidyDone = false;
            }
        }
        private TConfigItem? defaultConfigItem;

        /// <summary>
        /// 配置信息, 在不允许输出所有日志时, 相当于白名单
        /// </summary>
        public TConfigItem[] ConfigItems
        {
            get => configItems;
            set
            {
                configItems = value;
                IsTidyDone = false;
            }
        }
        private TConfigItem[] configItems = Array.Empty<TConfigItem>();

        /// <summary>
        /// 需忽略的配置, 相当于黑名单
        /// </summary>
        public TConfigItem[] Ignore
        {
            get => ignore;
            set
            {
                ignore = value;
                IsTidyDone = false;
            }
        }
        private TConfigItem[] ignore = Array.Empty<TConfigItem>();



        #region 数据整理
        /// <summary>
        /// 数据是否已被整理
        /// </summary>
        protected bool IsTidyDone { get; set; }

        /// <summary>
        /// 整理数据
        /// </summary>
        protected virtual void Tidy()
        {
            // 设置 Ignore 值
            foreach (var item in ConfigItems)
            {
                item.Ignore = false;
            }
            foreach (var item in Ignore)
            {
                item.Ignore = true;   
            }
            // 将配置项按 级别 分类 子分类, 建树方便查询
            _tidyTree.Clear();
            foreach (var item in ConfigItems)
            {
                _tidyTree.Add(item);
            }
            foreach (var item in Ignore)
            {
                _tidyTree.Add(item);
            }
        }
        private LogOutputConfigTree<TConfigItem> _tidyTree = new LogOutputConfigTree<TConfigItem>();
        
        #region 测试
        /// <summary>
        /// 取得整理后的树的结构的字符串
        /// </summary>
        /// <returns></returns>
        public string GetTidyTreeString()
        {
            CheckTidy();

            StringBuilder sb = new StringBuilder();

            foreach (var levelNode in _tidyTree.Levels)
            {
                if (levelNode.Level == null)
                {
                    sb.AppendKeyValuePair("Level", "<null>" + (levelNode.Config?.Ignore == true ? " <忽略>" : ""));
                }
                else
                {
                    sb.AppendKeyValuePair("Level", levelNode.Level + (levelNode.Config?.Ignore == true ? " <忽略>" : ""));
                    foreach (var categoryNode in levelNode.Categorys)
                    {
                        sb.Append("\t");
                        if (categoryNode.Category == null)
                        {
                            sb.AppendKeyValuePair("Category", "<null>" + (categoryNode.Config?.Ignore == true ? " <忽略>" : ""));
                        }
                        else
                        {
                            sb.AppendKeyValuePair("Category", categoryNode.Category + (categoryNode.Config?.Ignore == true ? " <忽略>" : ""));
                            foreach (var subCategoryNode in categoryNode.SubCategories)
                            {
                                sb.Append("\t\t");
                                if (subCategoryNode.SubCategory == null)
                                {
                                    sb.AppendKeyValuePair("SubCategory", "<null>" + (subCategoryNode.Config?.Ignore == true ? " <忽略>" : ""));
                                }
                                else
                                {
                                    sb.AppendKeyValuePair("SubCategory", subCategoryNode.SubCategory + (subCategoryNode.Config?.Ignore == true ? " <忽略>" : ""));
                                }
                            }
                        }

                    }
                }
            }

            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 检查是否已经整理, 若未整理, 将调用 <see cref="Tidy"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckTidy()
        {
            if (!IsTidyDone)
            {
                Tidy();
            }
        }
        #endregion

        #region 对外方法
        /// <summary>
        /// 判断日志数据是否需要输出
        /// </summary>
        /// <param name="log"></param>
        public bool IsNeedOutput(LogData log)
        {
            CheckTidy();

            var config = _tidyTree.Find(log.Level, log.Category, log.SubCategory);
            if (config == null)
            {
                if (AllOutput)
                {
                    return true;
                }
                else
                {
                    config = DefaultConfigItem;
                }
            }
            if (config == null) 
            {
                return false;
            }
            return !config.Ignore;
        }
        /// <summary>
        /// 判断日志数据是否需要输出, 并返回对应的配置项, 如果不需要输出, 将 out null
        /// </summary>
        /// <param name="log"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool IsNeedOutput(LogData log, out TConfigItem? output)
        {
            CheckTidy();

            var config = _tidyTree.Find(log.Level, log.Category, log.SubCategory);
            if (config == null)
            {
                if (AllOutput)
                {
                    output = null;
                    return true;
                }
                else
                {
                    config = DefaultConfigItem;
                }
            }
            output = config;
            if (config == null)
            {
                return false;
            }
            return !config.Ignore;
        }

        /// <summary>
        /// 取得日志数据相应的配置项, 就算是被忽略项, 也会返回对应的配置项
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public TConfigItem? GetConfigItem(LogData log)
        {
            CheckTidy();

            var config = _tidyTree.Find(log.Level, log.Category, log.SubCategory);
            if (config == null)
            {
                if (AllOutput)
                {
                    return null;
                }
                else
                {
                    config = DefaultConfigItem;
                }
            }
            return config;
        }

        /// <summary>
        /// 取得界别相应的配置项, 就算是被忽略项, 也会返回对应的配置项
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public TConfigItem? GetLevelConfigItem(string? level)
        {
            CheckTidy();

            var config = _tidyTree.FindLevel(level);
            if (config == null)
            {
                if (AllOutput)
                {
                    return null;
                }
                else
                {
                    config = DefaultConfigItem;
                }
            }
            return config;
        }
        #endregion

    }
    public class LogOutputConfigItemBase
    {
        /// <summary>
        /// 日志的级别
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 日志的分类
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 日志的子分类
        /// </summary>
        public string? SubCategory { get; set; }

        /// <summary>
        /// 需忽略的配置
        /// </summary>
        internal bool Ignore { get; set; }
    }

    internal class LogOutputConfigTree<TConfigItem>
        where TConfigItem : LogOutputConfigItemBase
    {
        public List<LevelNode> Levels { get; set; } = new();

        /// <summary>
        /// 往树上添加节点
        /// </summary>
        /// <param name="item"></param>
        public void Add(TConfigItem item)
        {
            LevelNode? node_l = Levels.FirstOrDefault(i => i.Level == item.Level);
            if (node_l == null)
            {
                node_l = new LevelNode()
                {
                    Level = item.Level,
                };
                Levels.Add(node_l);
            }
            if (item.Level == null)
            {
                node_l.Config = item;
                return;
            }

            CategoryNode? node_c = node_l.Categorys.FirstOrDefault(i => i.Category == item.Category);
            if (node_c == null)
            {
                node_c = new CategoryNode()
                {
                    Category = item.Category,
                };
                node_l.Categorys.Add(node_c);
            }
            if (item.Category == null)
            {
                node_c.Config = item;
                return;
            }

            SubCategoryNode? node_s = node_c.SubCategories.FirstOrDefault(i => i.SubCategory == item.SubCategory);
            if (node_s == null)
            {
                node_s = new SubCategoryNode()
                {
                    SubCategory = item.SubCategory,
                };
                node_c.SubCategories.Add(node_s);
            }
            node_s.Config = item;
        }

        /// <summary>
        /// 清理所有节点
        /// </summary>
        public void Clear()
        {
            Levels.Clear();
        }

        /// <summary>
        /// 尝试找到对应的配置, 如果找不到对应的配置, 会依次往外找null节点
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <returns></returns>
        public TConfigItem? Find(string? level, string? category, string? subCategory)
        {
            level = level.IsEmpty() ? null : level;
            category = category.IsEmpty() ? null : category;
            subCategory = subCategory.IsEmpty() ? null : subCategory;

            LevelNode? nullOne_l = null;
            LevelNode? matchOne_l = null;
            foreach (var node in Levels)
            {
                if (node.Level == level)
                {
                    matchOne_l = node;
                }
                else if (node.Level == null)
                {
                    nullOne_l = node;
                }
            }
            if (nullOne_l == null && matchOne_l == null)
            {
                return null;
            }
            else if (matchOne_l == null)
            {
                return nullOne_l!.Config;
            }

            CategoryNode? nullOne_c = null;
            CategoryNode? matchOne_c = null;
            foreach (var node in matchOne_l.Categorys)
            {
                if (node.Category == category)
                {
                    matchOne_c = node;
                }
                else if (node.Category == null)
                {
                    nullOne_c = node;
                }
            }
            if (nullOne_c == null && matchOne_c == null)
            {
                return nullOne_l?.Config;
            }
            else if (matchOne_c == null)
            {
                return nullOne_c!.Config;
            }


            SubCategoryNode? nullOne_s = null;
            SubCategoryNode? matchOne_s = null;
            foreach (var node in matchOne_c.SubCategories)
            {
                if (node.SubCategory == subCategory)
                {
                    matchOne_s = node;
                }
                else if (node.SubCategory == null)
                {
                    nullOne_s = node;
                }
            }
            if (nullOne_s == null && matchOne_s == null)
            {
                return nullOne_c?.Config ?? nullOne_l?.Config;
            }
            else if (matchOne_s == null)
            {
                return nullOne_s!.Config;
            }
            else
            {
                return matchOne_s.Config;
            }
        }

        /// <summary>
        /// 尝试找到对应级别的配置
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public TConfigItem? FindLevel(string? level)
        {
            level = level.IsEmpty() ? null : level;

            LevelNode? nullOne_l = null;
            LevelNode? matchOne_l = null;
            foreach (var node in Levels)
            {
                if (node.Level == level)
                {
                    matchOne_l = node;
                }
                else if (node.Level == null)
                {
                    nullOne_l = node;
                }
            }
            if (nullOne_l == null && matchOne_l == null)
            {
                return null;
            }
            else if (matchOne_l == null)
            {
                return nullOne_l!.Config;
            }
            else
            {
                if (level == null)
                {
                    return matchOne_l.Config;
                }
                else
                {
                    // level 非空值的情况下, 配置数据不会存放在Level节点中, 而应该找
                    // 到Category层的值为null的节点, 也就是这个分支下的默认值
                    CategoryNode? node_c = null;
                    foreach (var node in matchOne_l.Categorys)
                    {
                        if (node.Category == null)
                        {
                            node_c = node;
                        }
                    }
                    return node_c?.Config;
                }
            }
        }

        public class LevelNode
        {
            public string? Level { get; set; }

            public List<CategoryNode> Categorys { get; set;} = new();

            public TConfigItem? Config { get; set; }
        }
        public class CategoryNode
        {
            public string? Category { get; set; }

            public List<SubCategoryNode> SubCategories { get; set; } = new();

            public TConfigItem? Config { get; set; }
        }
        public class SubCategoryNode
        {

            public string? SubCategory { get; set; }

            public TConfigItem? Config { get; set; }
        }
    }
}
