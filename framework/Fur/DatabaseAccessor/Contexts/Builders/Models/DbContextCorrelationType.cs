using Fur.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文关联类型
    /// </summary>
    [SkipScan]
    internal sealed class DbContextCorrelationType
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        internal DbContextCorrelationType()
        {
            EntityTypes = new List<Type>();
            EntityNoKeyTypes = new List<Type>();
            EntityTypeBuilderTypes = new List<Type>();
            EntitySeedDataTypes = new List<Type>();
            EntityChangedTypes = new List<Type>();
            ModelBuilderFilterTypes = new List<Type>();
            EntityMutableTableTypes = new List<Type>();
            ModelBuilderFilterInstances = new List<IPrivateModelBuilderFilter>();
            DbFunctionMethods = new List<MethodInfo>();
        }

        /// <summary>
        /// 关联的数据库上下文
        /// </summary>
        internal Type DbContextLocator { get; set; }

        /// <summary>
        /// 所有关联类型
        /// </summary>
        internal List<Type> Types { get; set; }

        /// <summary>
        /// 实体类型集合
        /// </summary>
        internal List<Type> EntityTypes { get; set; }

        /// <summary>
        /// 无键实体类型集合
        /// </summary>
        internal List<Type> EntityNoKeyTypes { get; set; }

        /// <summary>
        /// 实体构建器类型集合
        /// </summary>
        internal List<Type> EntityTypeBuilderTypes { get; set; }

        /// <summary>
        /// 种子数据类型集合
        /// </summary>
        internal List<Type> EntitySeedDataTypes { get; set; }

        /// <summary>
        /// 实体数据改变类型
        /// </summary>
        internal List<Type> EntityChangedTypes { get; set; }

        /// <summary>
        /// 模型构建筛选器类型集合
        /// </summary>
        internal List<Type> ModelBuilderFilterTypes { get; set; }

        /// <summary>
        /// 可变表实体类型集合
        /// </summary>
        internal List<Type> EntityMutableTableTypes { get; set; }

        /// <summary>
        /// 数据库函数方法集合
        /// </summary>
        internal List<MethodInfo> DbFunctionMethods { get; set; }

        /// <summary>
        /// 模型构建器筛选器实例
        /// </summary>
        internal List<IPrivateModelBuilderFilter> ModelBuilderFilterInstances { get; set; }
    }
}