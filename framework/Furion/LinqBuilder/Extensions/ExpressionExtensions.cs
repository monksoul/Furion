// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Furion.LinqBuilder
{
    /// <summary>
    /// 表达式拓展类
    /// </summary>
    [SuppressSniffer]
    public static class ExpressionExtensions
    {
        /// <summary>
        /// 组合两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <param name="mergeWay">组合方式</param>
        /// <returns>新的表达式</returns>
        public static Expression<TSource> Compose<TSource>(this Expression<TSource> expression, Expression<TSource> extendExpression, Func<Expression, Expression, Expression> mergeWay)
        {
            var parameterExpressionSetter = expression.Parameters
                .Select((u, i) => new { u, Parameter = extendExpression.Parameters[i] })
                .ToDictionary(d => d.Parameter, d => d.u);

            var extendExpressionBody = ParameterReplaceExpressionVisitor.ReplaceParameters(parameterExpressionSetter, extendExpression.Body);
            return Expression.Lambda<TSource>(mergeWay(expression.Body, extendExpressionBody), expression.Parameters);
        }

        /// <summary>
        /// 与操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> And<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.AndAlso);
        }

        /// <summary>
        /// 与操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> And<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.AndAlso);
        }

        /// <summary>
        /// 根据条件成立再与操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> AndIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
        }

        /// <summary>
        /// 根据条件成立再与操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> AndIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
        }

        /// <summary>
        /// 或操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> Or<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.OrElse);
        }

        /// <summary>
        /// 或操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> Or<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.OrElse);
        }

        /// <summary>
        /// 根据条件成立再或操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> OrIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
        }

        /// <summary>
        /// 根据条件成立再或操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> OrIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
        }

        /// <summary>
        /// 获取Lambda表达式属性名，只限 u=>u.Property 表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>属性名</returns>
        public static string GetExpressionPropertyName<TSource>(this Expression<Func<TSource, object>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }
            else if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (expression.Body is ParameterExpression parameterExpression)
            {
                return parameterExpression.Type.Name;
            }

            throw new InvalidCastException(nameof(expression));
        }

        /// <summary>
        /// 是否是空集合
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="sources">集合对象</param>
        /// <returns>是否为空集合</returns>
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> sources)
        {
            return sources == null || !sources.Any();
        }
    }
}