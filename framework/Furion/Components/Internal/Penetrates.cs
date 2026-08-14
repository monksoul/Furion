// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection;

namespace Furion.Components;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 根组件上下文缓存
    /// </summary>
    /// <remarks>以根组件类型为键，确保共享同一上下文</remarks>
    private static readonly ConcurrentDictionary<Type, ComponentContext> _cachedRootContexts = new();

    /// <summary>
    /// 创建组件依赖链表
    /// </summary>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    internal static List<ComponentContext> CreateDependLinkList(Type componentType, object options = default)
    {
        // 获取或创建根组件上下文
        var rootContext = _cachedRootContexts.GetOrAdd(componentType, static (type, opts) =>
        {
            // 根组件上下文
            var context = new ComponentContext
            {
                ComponentType = type,
                IsRoot = true
            };

            if (opts != null)
            {
                context.SetProperty(type, opts);
            }

            return context;
        }, options);

        if (options != null && rootContext != null)
        {
            rootContext.SetProperty(componentType, options);
        }

        return BuildDependLinkList(componentType, rootContext);
    }

    /// <summary>
    /// 构建组件依赖链表
    /// </summary>
    /// <param name="componentType">根组件类型</param>
    /// <param name="rootContext">已缓存的根组件上下文</param>
    /// <returns></returns>
    private static List<ComponentContext> BuildDependLinkList(Type componentType, ComponentContext rootContext)
    {
        // 初始化组件依赖链
        var dependLinkList = new List<Type> { componentType };
        var componentContextLinkList = new List<ComponentContext>
        {
            rootContext
        };

        // 创建组件依赖链
        CreateDependLinkList(componentType, ref dependLinkList, ref componentContextLinkList);

        return componentContextLinkList;
    }

    /// <summary>
    /// 创建组件依赖链表
    /// </summary>
    /// <param name="componentType">组件类型</param>
    /// <param name="dependLinkList">依赖链表</param>
    /// <param name="componentContextLinkList">组件上下文链表</param>
    /// <param name="options">组件参数</param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void CreateDependLinkList(Type componentType, ref List<Type> dependLinkList, ref List<ComponentContext> componentContextLinkList, object options = default)
    {
        // 检查空值
        if (componentType == null) return;

        // 获取根组件上下文
        var rootComponentContext = componentContextLinkList.FirstOrDefault(u => u.IsRoot);

        // 使用内部状态跟踪依赖解析
        var resolutionState = new ComponentResolutionState(dependLinkList, componentContextLinkList, rootComponentContext);

        // 递归解析依赖关系
        ResolveComponentDependencies(componentType, rootComponentContext, ref resolutionState, options);
    }

    /// <summary>
    /// 组件依赖解析状态
    /// </summary>
    private class ComponentResolutionState
    {
        public ComponentResolutionState(List<Type> dependLinkList, List<ComponentContext> componentContextLinkList, ComponentContext rootContext)
        {
            DependLinkList = dependLinkList;
            ComponentContextLinkList = componentContextLinkList;
            RootContext = rootContext;
            ProcessedComponents = new HashSet<Type>(dependLinkList);
            CurrentPath = new Stack<Type>();
        }
        public List<Type> DependLinkList { get; }
        public List<ComponentContext> ComponentContextLinkList { get; }
        public ComponentContext RootContext { get; }
        public HashSet<Type> ProcessedComponents { get; }
        public Stack<Type> CurrentPath { get; }

        /// <summary>
        /// 获取组件在依赖链中的索引
        /// </summary>
        public int GetIndexOf(Type componentType)
        {
            return DependLinkList.IndexOf(componentType);
        }

        /// <summary>
        /// 获取组件上下文
        /// </summary>
        public ComponentContext GetComponentContext(Type componentType)
        {
            var index = GetIndexOf(componentType);
            return index > -1 ? ComponentContextLinkList[index] : RootContext;
        }

        /// <summary>
        /// 添加新组件到依赖链
        /// </summary>
        public ComponentContext AddComponent(Type componentType, Type parentType, bool isDependency)
        {
            var parentIndex = GetIndexOf(parentType);
            var parentContext = GetComponentContext(parentType);

            // 创建新组件上下文
            var newContext = new ComponentContext
            {
                CalledContext = parentContext,
                RootContext = RootContext,
                ComponentType = componentType
            };

            if (isDependency)
            {
                // 依赖组件：插入在父组件之前
                DependLinkList.Insert(parentIndex, componentType);
                ComponentContextLinkList.Insert(parentIndex, newContext);
            }
            else
            {
                // 链接组件：添加到末尾
                if (!ProcessedComponents.Contains(componentType))
                {
                    DependLinkList.Add(componentType);
                    ComponentContextLinkList.Add(newContext);
                }
            }

            ProcessedComponents.Add(componentType);
            return newContext;
        }
    }

    /// <summary>
    /// 递归解析组件依赖
    /// </summary>
    private static void ResolveComponentDependencies(Type componentType, ComponentContext currentContext, ref ComponentResolutionState state, object options)
    {
        if (componentType == null) return;

        // 检查循环依赖
        if (state.CurrentPath.Contains(componentType))
        {
            var path = string.Join(" -> ", state.CurrentPath.Reverse().Append(componentType).Select(t => t.Name));
            throw new InvalidOperationException($"Circular dependency detected: {path}");
        }

        state.CurrentPath.Push(componentType);
        try
        {
            // 获取 [DependsOn] 特性
            var dependsOnAttribute = componentType.GetCustomAttribute<DependsOnAttribute>(true);

            var rawDepends = dependsOnAttribute?.DependComponents ?? [];

            // 获取依赖组件列表
            var dependComponents = new List<Type>(rawDepends.Length);
            var dependSet = new HashSet<Type>();

            foreach (var c in rawDepends)
            {
                if (c != null && dependSet.Add(c)) dependComponents.Add(c);
            }

            var rawLinks = dependsOnAttribute?.Links ?? [];

            // 获取链接组件列表
            var linkComponents = new List<Type>(rawLinks.Length);
            var linkSet = new HashSet<Type>();

            foreach (var c in rawLinks)
            {
                if (c != null && linkSet.Add(c)) linkComponents.Add(c);
            }

            // 检查自引用
            if (dependSet.Contains(componentType) || linkSet.Contains(componentType))
            {
                throw new InvalidOperationException($"Component {componentType.Name} cannot reference itself.");
            }

            // 设置当前组件依赖
            currentContext.DependComponents = dependComponents.ToArray();
            currentContext.LinkComponents = linkComponents.ToArray();

            // 处理依赖组件
            foreach (var dependComponent in dependComponents)
            {
                if (dependComponent == null) continue;

                // 检查循环依赖
                if (state.CurrentPath.Contains(dependComponent))
                {
                    throw new InvalidOperationException($"Circular dependency detected between {componentType.Name} and {dependComponent.Name}");
                }

                ComponentContext dependContext;

                // 如果组件尚未处理
                if (!state.ProcessedComponents.Contains(dependComponent))
                {
                    dependContext = state.AddComponent(dependComponent, componentType, true);

                    // 为新组件设置属性
                    dependContext.SetProperty(dependComponent, options);
                }
                else
                {
                    dependContext = state.GetComponentContext(dependComponent);
                    // 检查是否出现后向依赖（可能导致循环）
                    if (state.GetIndexOf(dependComponent) > state.GetIndexOf(componentType))
                    {
                        throw new InvalidOperationException($"Circular reference detected between {componentType.Name} and {dependComponent.Name}");
                    }
                }

                // 递归处理依赖
                ResolveComponentDependencies(dependComponent, dependContext, ref state, options);
            }

            // 链接组件处理逻辑
            if (linkComponents.Count == 0) return;

            foreach (var linkComponent in linkComponents)
            {
                if (linkComponent == null) continue;

                // 不能链接到根节点
                if (linkComponent == state.RootContext.ComponentType)
                {
                    throw new InvalidOperationException($"Component {componentType.Name} cannot link to root component.");
                }

                // 检查循环依赖
                if (state.CurrentPath.Contains(linkComponent))
                {
                    throw new InvalidOperationException($"Circular dependency detected in links between {componentType.Name} and {linkComponent.Name}");
                }

                ComponentContext linkContext;

                // 递归处理链接组件
                if (!state.ProcessedComponents.Contains(linkComponent))
                {
                    linkContext = state.AddComponent(linkComponent, componentType, false);

                    // 为新组件设置属性
                    linkContext.SetProperty(linkComponent, options);
                }
                else
                {
                    linkContext = state.GetComponentContext(linkComponent);
                }

                // 递归处理链接
                ResolveComponentDependencies(linkComponent, linkContext, ref state, options);
            }
        }
        finally
        {
            state.CurrentPath.Pop();
        }
    }
}