using System;

namespace Fur.DependencyInjection
{
    /// <summary>
    /// 设置依赖注入方式
    /// </summary>
    [SkipScan, AttributeUsage(AttributeTargets.Class)]
    public class InjectionAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="expectInterfaces"></param>
        public InjectionAttribute(params Type[] expectInterfaces)
        {
            Action = InjectionActions.Add;
            Pattern = InjectionPatterns.SelfWithFirstInterface;
            ExpectInterfaces = expectInterfaces ?? Array.Empty<Type>();
            Order = 0;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="expectInterfaces"></param>
        public InjectionAttribute(InjectionActions action, params Type[] expectInterfaces)
        {
            Action = action;
            Pattern = InjectionPatterns.SelfWithFirstInterface;
            ExpectInterfaces = expectInterfaces ?? Array.Empty<Type>();
            Order = 0;
        }

        /// <summary>
        /// 添加服务方式，存在不添加，或继续添加
        /// </summary>
        public InjectionActions Action { get; set; }

        /// <summary>
        /// 注册选项
        /// </summary>
        public InjectionPatterns Pattern { get; set; }

        /// <summary>
        /// 注册别名
        /// </summary>
        /// <remarks>多服务时使用</remarks>
        public string Named { get; set; }

        /// <summary>
        /// 排序，排序越大，则在后面注册
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 排除接口
        /// </summary>
        public Type[] ExpectInterfaces { get; set; }

        /// <summary>
        /// 代理类型，必须继承 DispatchProxy、IDispatchProxy
        /// </summary>
        public Type Proxy { get; set; }
    }
}