using Common_Util.Interfaces.Owner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.Owner
{
    public static class IParentOwnerExtensions
    {
        /// <summary>
        /// 逐层向父代遍历所有祖先
        /// </summary>
        /// <typeparam name="TSameTypeParentOwner"></typeparam>
        /// <param name="owner"></param>
        /// <param name="includeSelf">是否包含 <paramref name="owner"/> 本身</param>
        /// <param name="checkForCycles">检查遍历过程是否出现循环, 如果出现循环就中止 (遍历过程不会有重复项)</param>
        /// <param name="equalityComparer">用于判断是否重复导致循环</param>
        /// <returns></returns>
        public static IEnumerable<TSameTypeParentOwner> GetAncestors<TSameTypeParentOwner>(
            this TSameTypeParentOwner owner, 
            bool includeSelf = false, 
            bool checkForCycles = false,
            IEqualityComparer<TSameTypeParentOwner>? equalityComparer = null)
            where TSameTypeParentOwner : ISameTypeParentOwner<TSameTypeParentOwner>
        {
            var current = owner;
            if (!checkForCycles)
            {
                if (includeSelf)
                {
                    yield return current;
                }
                while (current.Parent != null)
                {
                    yield return current.Parent;
                    current = current.Parent;
                }
            }
            else
            {
                HashSet<TSameTypeParentOwner> set = new(equalityComparer ?? EqualityComparer<TSameTypeParentOwner>.Default);
                if (includeSelf)
                {
                    yield return current;
                    set.Add(current);
                }
                while (current.Parent != null)
                {
                    if (set.Contains(current.Parent))
                    {
                        yield break;
                    }
                    current = current.Parent;
                    yield return current;
                    set.Add(current);
                }
            }
        }
    }
}
