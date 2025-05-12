using Common_Util.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.DbEntity
{
    public static class FieldCopyHelper
    {

        private readonly static ConcurrentDictionary<(Type source, Type target, CopierCreateConfig config), object> CopierCaches = [];
        /// <summary>
        /// 创建字段拷贝器过程的配置
        /// </summary>
        /// <param name="NullCheck">当可空性不匹配时, 是否抛出异常</param>
        public record struct CopierCreateConfig(bool NullCheck);
        /// <summary>
        /// 创建从 <typeparamref name="TSource"/> 拷贝属性值到 <typeparamref name="target"/> 字段属性的方法, <br/>
        /// 拷贝目标为 <typeparamref name="target"/> 中带 <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> 标注的可写属性 <br/>
        /// 拷贝源为 <typeparamref name="TSource"/> 中, 属性名与上述目标属性相同, 且属性类型能够赋值到目标的属性, 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="cacheCopier">是否将编译得到的拷贝方法缓存起来</param>
        /// <returns></returns>
        public static Action<TSource, TTarget> GetCopier<TSource, TTarget>(CopierCreateConfig config = default, bool cacheCopier = false)
        {
            Action<TSource, TTarget>? action = null;

            (Type ts, Type tt, CopierCreateConfig config) key = (typeof(TSource), typeof(TTarget), config);
            if (CopierCaches.TryGetValue(key, out object? cache))
            {
                if (cache is Action<TSource, TTarget> _action)
                {
                    action = _action;
                }
            }
            if (action == null)
            {
                ParameterExpression pSource = Expression.Parameter(key.ts, "source");
                ParameterExpression pTarget = Expression.Parameter(key.tt, "target");
                List<Expression> setValueExprs = [];

                Dictionary<string, PropertyInfo> sourcePropertyDic
                    = key.ts.GetPropertiesEx()
                    .ToDictionary(p => p.Name, p => p);
                IEnumerable<PropertyInfo> targetProperties
                    = key.tt.GetPropertiesEx()
                    .Where(p => p.CanWrite && p.ExistCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>());

                foreach (PropertyInfo tP in targetProperties)
                {
                    if (sourcePropertyDic.TryGetValue(tP.Name, out var sP))
                    {
                        if (sP.PropertyType.IsAssignableTo(tP.PropertyType))
                        {
                            if (config.NullCheck)
                            {
                                bool sPMayBeNull = sP.PropertyType.CanBeNull() && sP.ExistCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>();
                                if (!sPMayBeNull) goto NullCheckPass;
                                bool tPAllowNull = tP.PropertyType.CanBeNull() && tP.ExistCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>();
                                if (sPMayBeNull && !tPAllowNull)
                                {
                                    throw new InvalidOperationException($"源属性 {sP} ({sP.PropertyType}) 可能是空值, 但 {tP} ({tP.PropertyType}) 不允许为空");
                                }
                            }
                        NullCheckPass:
                            Expression tPExpr = Expression.Property(pTarget, tP);
                            Expression sPExpr = Expression.Property(pSource, sP);
                            Expression assign = Expression.Assign(tPExpr, sPExpr);
                            setValueExprs.Add(assign);
                        }
                    }
                }

                BlockExpression block = Expression.Block(setValueExprs);
                var lambdaExpr = Expression.Lambda<Action<TSource, TTarget>>(block, [pSource, pTarget]);
                action = lambdaExpr.Compile();
            }
            if (cacheCopier)
            {
                CopierCaches.TryAdd(key, action);
            }
            return action;
        }
        /// <summary>
        /// 使用由 <see cref="GetCopier{TSource, TTarget}(CopierCreateConfig, bool)"/> 取得的拷贝器, 将 <paramref name="source"/> 中的字段值拷贝到 <paramref name="target"/>
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="config">创建拷贝器过程的配置</param>
        /// <param name="cacheCopier">如果对应的拷贝器需要创建, 是否将编译得到的拷贝方法缓存起来</param>
        public static void CopyTo<TSource, TTarget>(TSource source, TTarget target, CopierCreateConfig config = default, bool cacheCopier = false)
        {
            GetCopier<TSource, TTarget>(config, cacheCopier).Invoke(source, target);
        }
    } 
}
