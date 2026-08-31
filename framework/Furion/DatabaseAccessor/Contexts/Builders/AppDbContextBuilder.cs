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

using Furion.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Furion.DatabaseAccessor;

/// <summary>
/// 数据库上下文构建器
/// </summary>
internal static class AppDbContextBuilder
{
    /// <summary>
    /// 数据库实体相关类型
    /// </summary>
    private static readonly IReadOnlyList<Type> EntityCorrelationTypes;

    /// <summary>
    /// 数据库函数方法集合
    /// </summary>
    private static readonly IReadOnlyList<MethodInfo> DbFunctionMethods;

    /// <summary>
    /// 创建数据库实体方法
    /// </summary>
    private static readonly MethodInfo ModelBuildEntityMethod;

    /// <summary>
    /// 蛇形命名转换正则表达式
    /// </summary>
    private static readonly Regex SnakeCaseRegex = new(
        @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// 实体类型到监听器信息列表的缓存
    /// </summary>
    internal static readonly ConcurrentDictionary<(Type EntityType, Type DbContextLocator), List<EntityChangedListenerInfo>> EntityChangedListenerCache = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    static AppDbContextBuilder()
    {
        // 扫描程序集，获取数据库实体相关类型
        EntityCorrelationTypes = App.EffectiveTypes
            .Where(t => (typeof(IPrivateEntity).IsAssignableFrom(t) || typeof(IPrivateModelBuilder).IsAssignableFrom(t))
                && t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface && !t.IsDefined(typeof(ManualAttribute), true)).ToList().AsReadOnly();

        if (EntityCorrelationTypes.Count > 0)
        {
            DbContextLocatorCorrelationTypes = new ConcurrentDictionary<Type, DbContextCorrelationType>();

            // 获取模型构建器 Entity<T> 方法
            ModelBuildEntityMethod = typeof(ModelBuilder)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(u => u.Name == nameof(ModelBuilder.Entity) && u.GetParameters().Length == 0);
        }

        // 查找所有数据库函数，必须是公开静态方法，且所在父类也必须是公开静态类
        DbFunctionMethods = App.EffectiveTypes
            .Where(t => t.IsAbstract && t.IsSealed && t.IsClass && !t.IsDefined(typeof(ManualAttribute), true))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => !m.IsDefined(typeof(SuppressSnifferAttribute), false)
                    && m.IsDefined(typeof(QueryableFunctionAttribute), true))).ToList().AsReadOnly();
    }

    /// <summary>
    /// 配置数据库上下文实体
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    internal static void ConfigureDbContextEntity(ModelBuilder modelBuilder, DbContext dbContext, Type dbContextLocator)
    {
        // 获取当前数据库上下文关联的类型
        var dbContextCorrelationType = GetDbContextCorrelationType(dbContext, dbContextLocator);

        // 如果没有数据，则跳过
        if (dbContextCorrelationType.EntityTypes.Count == 0) goto EntityFunctions;

        // 获取当前数据库上下文的 [DbContextAttributes] 特性
        var dbContextType = dbContext.GetType();
        var appDbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);

        var entityTypes = dbContextCorrelationType.EntityTypes;

        // 初始化所有类型
        foreach (var entityType in entityTypes)
        {
            // 创建实体类型
            var entityBuilder = CreateEntityTypeBuilder(entityType, modelBuilder, dbContext, dbContextType, dbContextLocator, dbContextCorrelationType, appDbContextAttribute);
            if (entityBuilder == null) continue;

            // 配置无键实体构建器
            ConfigureEntityNoKeyType(entityType, entityBuilder, dbContextCorrelationType.EntityNoKeyTypes);

            // 实体构建成功注入拦截
            LoadModelBuilderOnCreating(modelBuilder, entityBuilder, dbContext, dbContextLocator, dbContextCorrelationType);

            // 配置数据库实体类型构建器
            ConfigureEntityTypeBuilder(entityType, entityBuilder, dbContext, dbContextLocator, dbContextCorrelationType);

            // 配置数据库实体种子数据
            ConfigureEntitySeedData(entityType, entityBuilder, dbContext, dbContextLocator, dbContextCorrelationType);

            // 实体完成配置注入拦截
            LoadModelBuilderOnCreated(modelBuilder, entityBuilder, dbContext, dbContextLocator, dbContextCorrelationType);
        }

        // 配置数据库函数
    EntityFunctions: ConfigureDbFunctions(modelBuilder, dbContextLocator);
    }

    /// <summary>
    /// 创建实体类型构建器
    /// </summary>
    /// <param name="type">数据库关联类型</param>
    /// <param name="modelBuilder">模型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextType">数据库上下文类型</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType"></param>
    /// <param name="appDbContextAttribute">数据库上下文特性</param>
    /// <returns>EntityTypeBuilder</returns>
    private static EntityTypeBuilder CreateEntityTypeBuilder(Type type, ModelBuilder modelBuilder, DbContext dbContext, Type dbContextType, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType, AppDbContextAttribute appDbContextAttribute = null)
    {
        // 反射创建实体类型构建器
        var entityTypeBuilder = ModelBuildEntityMethod?.MakeGenericMethod(type).Invoke(modelBuilder, null) as EntityTypeBuilder;

        // 配置动态表名
        if (!ConfigureEntityMutableTableName(type, entityTypeBuilder, dbContext, dbContextLocator, dbContextCorrelationType))
        {
            // 配置实体表名
            ConfigureEntityTableName(type, appDbContextAttribute, entityTypeBuilder, dbContext, dbContextType);
        }

        // 如果未启用多租户支持或租户设置为OnDatabase 或 OnSchema 方案，则忽略多租户字段，另外还需要排除多租户数据库上下文定位器，还要排除没有继承 PrivateEntityBase<> 类型的
        if (dbContextLocator != typeof(MultiTenantDbContextLocator)
            && (!typeof(IPrivateMultiTenant).IsAssignableFrom(dbContextType) || typeof(IMultiTenantOnDatabase).IsAssignableFrom(dbContextType) || typeof(IMultiTenantOnSchema).IsAssignableFrom(dbContextType))
            && type.HasImplementedRawGeneric(typeof(PrivateEntityBase<>))
            && type.GetProperty(Db.OnTableTenantId) != null)
        {
            entityTypeBuilder?.Ignore(Db.OnTableTenantId);
        }

        return entityTypeBuilder;
    }

    /// <summary>
    /// 配置实体表名
    /// </summary>
    /// <param name="type">实体类型</param>
    /// <param name="appDbContextAttribute">数据库上下文特性</param>
    /// <param name="entityTypeBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextType">数据库上下文类型</param>
    private static void ConfigureEntityTableName(Type type, AppDbContextAttribute appDbContextAttribute, EntityTypeBuilder entityTypeBuilder, DbContext dbContext, Type dbContextType)
    {
        // 获取表是否定义 [Table] 特性
        var tableAttribute = type.IsDefined(typeof(TableAttribute), true) ? type.GetCustomAttribute<TableAttribute>(true) : default;

        // 排除无键实体或已经贴了 [Table] 特性的类型
        if (typeof(IPrivateEntityNotKey).IsAssignableFrom(type) || !string.IsNullOrWhiteSpace(tableAttribute?.Schema)) return;

        // 获取真实表名
        var tableName = tableAttribute?.Name ?? type.Name;

        // 判断是否配置了 Schema，如果是直接采用。如果不是，继续判断是否启用基于 Schema 的多租户，如果是，则获取 Schema
        var dynamicSchema = tableAttribute?.Schema ?? (!typeof(IMultiTenantOnSchema).IsAssignableFrom(dbContextType)
            ? default
            : (dbContextType.GetMethod(nameof(IMultiTenantOnSchema.GetSchemaName))  // 支持显式和隐式
                    ?? typeof(IMultiTenantOnSchema).GetMethod(nameof(IMultiTenantOnSchema.GetSchemaName))
                )?.Invoke(dbContext, null)?.ToString());

        // 获取类型前缀 [TableFixs] 特性
        var tableFixsAttribute = !type.IsDefined(typeof(TableFixsAttribute), true)
            ? default
            : type.GetCustomAttribute<TableFixsAttribute>(true);

        // 获取表名最终前后缀
        var tablePrefix = (tableFixsAttribute?.Prefix ?? appDbContextAttribute?.TablePrefix)?.Trim();
        var tableSuffix = (tableFixsAttribute?.Suffix ?? appDbContextAttribute?.TableSuffix)?.Trim();

        // 表名使用蛇形命名
        if (appDbContextAttribute?.UseSnakeCaseNaming == true)
        {
            tableName = string.Join("_", SnakeCaseRegex.Split(tableName).Where(u => u.Length > 0));
        }

        // 如果没有定义 Table 特性且没有定义 TableFixs 特性，且没有实现 IMultiTenantOnSchema 接口且 AppDbContextAttribute 没有配置前后缀
        // 满足以上条件则不配置表名
        if (!(tableAttribute == null && tableFixsAttribute == null && string.IsNullOrWhiteSpace(tablePrefix) && string.IsNullOrWhiteSpace(tableSuffix)))
        {
            entityTypeBuilder?.Metadata.SetTableName($"{tablePrefix}{tableName}{tableSuffix}");
        }

        // 设置 Schema
        if (!string.IsNullOrWhiteSpace(dynamicSchema))
        {
            entityTypeBuilder?.Metadata.SetSchema(dynamicSchema);
        }
    }

    /// <summary>
    /// 配置实体动态表名
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    private static bool ConfigureEntityMutableTableName(Type entityType, EntityTypeBuilder entityBuilder, DbContext dbContext, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType)
    {
        var isSet = false;

        // 获取实体动态配置表配置
        var entityMutableTableTypes = dbContextCorrelationType.EntityMutableTableTypes
            .Where(u => u.GetInterfaces()
                .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntityMutableTable<>)) && i.GenericTypeArguments.Contains(entityType))).ToList();

        if (entityMutableTableTypes.Count == 0) return isSet;

        // 只应用于扫描的最后配置
        var lastEntityMutableTableType = entityMutableTableTypes[^1];

        // 支持显式和隐式查找
        var getTableNameMethod = lastEntityMutableTableType.GetMethod("GetTableName")
            ?? typeof(IPrivateEntityMutableTable<>).MakeGenericType(entityType).GetMethod("GetTableName");

        var instance = Activator.CreateInstance(lastEntityMutableTableType);
        var tableMeta = (string[]?)getTableNameMethod?.Invoke(instance, [dbContext, dbContextLocator]);

        if (tableMeta is { Length: > 0 })
        {
            // 设置动态表名
            entityBuilder?.ToTable(tableMeta[0], tableMeta.Length > 1 ? tableMeta[1] : null);
            isSet = true;
        }

        return isSet;
    }

    /// <summary>
    /// 配置无键实体类型
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="EntityNoKeyTypes">无键实体列表</param>
    private static void ConfigureEntityNoKeyType(Type entityType, EntityTypeBuilder entityBuilder, IReadOnlyList<Type> EntityNoKeyTypes)
    {
        if (!EntityNoKeyTypes.Contains(entityType)) return;

        // 配置视图、存储过程、函数无键实体
        entityBuilder?.HasNoKey();

        var configure = Activator.CreateInstance(entityType) as IPrivateEntityNotKey;
        entityBuilder?.ToView(configure?.GetName(), configure?.GetSchema());
    }

    /// <summary>
    /// 加载模型构建筛选器创建之前拦截
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    private static void LoadModelBuilderOnCreating(ModelBuilder modelBuilder, EntityTypeBuilder entityBuilder, DbContext dbContext, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType)
    {
        var filterInstances = GetModelBuilderFilterInstances(dbContextCorrelationType, dbContext, dbContextLocator);

        if (filterInstances.Count == 0) return;

        // 创建过滤器
        foreach (var filterInstance in filterInstances)
        {
            // 执行构建之前拦截
            filterInstance?.OnCreating(modelBuilder, entityBuilder, dbContext, dbContextLocator);
        }
    }

    /// <summary>
    /// 加载模型构建筛选器创建之后拦截
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    private static void LoadModelBuilderOnCreated(ModelBuilder modelBuilder, EntityTypeBuilder entityBuilder, DbContext dbContext, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType)
    {
        var filterInstances = GetModelBuilderFilterInstances(dbContextCorrelationType, dbContext, dbContextLocator);

        if (filterInstances.Count == 0) return;

        // 创建过滤器
        foreach (var filterInstance in filterInstances)
        {
            // 执行构建之后拦截
            filterInstance?.OnCreated(modelBuilder, entityBuilder, dbContext, dbContextLocator);
        }
    }

    /// <summary>
    /// 获取模型构建器筛选器实例
    /// </summary>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    /// <param name="dbContext">当前数据库上下文实例</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <returns>过滤器实例列表</returns>
    private static IReadOnlyList<IPrivateModelBuilderFilter> GetModelBuilderFilterInstances(DbContextCorrelationType dbContextCorrelationType, DbContext dbContext, Type dbContextLocator)
    {
        if (dbContextCorrelationType.ModelBuilderFilterTypes.Count == 0) return [];

        var instances = new List<IPrivateModelBuilderFilter>(dbContextCorrelationType.ModelBuilderFilterTypes.Count);

        foreach (var filterType in dbContextCorrelationType.ModelBuilderFilterTypes)
        {
            if (typeof(DbContext).IsAssignableFrom(filterType) && filterType == dbContext.GetType())
            {
                if (dbContext is IPrivateModelBuilderFilter filter)
                {
                    instances.Add(filter);
                }
            }
            else
            {
                var instance = Activator.CreateInstance(filterType) as IPrivateModelBuilderFilter;
                if (instance != null)
                {
                    instances.Add(instance);
                }
            }
        }

        return instances;
    }

    /// <summary>
    /// 配置数据库实体类型构建器
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    private static void ConfigureEntityTypeBuilder(Type entityType, EntityTypeBuilder entityBuilder, DbContext dbContext, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType)
    {
        // 获取该实体类型的配置类型
        var entityTypeBuilderTypes = dbContextCorrelationType.EntityTypeBuilderTypes
            .Where(u => u.GetInterfaces()
                .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntityTypeBuilder<>)) && i.GenericTypeArguments.Contains(entityType))).ToList();

        if (entityTypeBuilderTypes.Count == 0) return;

        // 调用数据库实体自定义配置
        foreach (var entityTypeBuilderType in entityTypeBuilderTypes)
        {
            // 支持显式和隐式接口查找
            var configureMethod = (entityTypeBuilderType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                .Concat(typeof(IPrivateEntityTypeBuilder<>).MakeGenericType(entityType).GetMethods()))
                                                               .FirstOrDefault(u => u.Name == "Configure"
                                                                    && u.GetParameters().Length > 0
                                                                    && u.GetParameters()[0].ParameterType == typeof(EntityTypeBuilder<>).MakeGenericType(entityType));

            if (configureMethod == null) continue;

            var instance = Activator.CreateInstance(entityTypeBuilderType);
            configureMethod.Invoke(instance, [entityBuilder, dbContext, dbContextLocator]);
        }
    }

    /// <summary>
    /// 配置数据库实体种子数据
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityBuilder">实体类型构建器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="dbContextCorrelationType">数据库实体关联类型</param>
    private static void ConfigureEntitySeedData(Type entityType, EntityTypeBuilder entityBuilder, DbContext dbContext, Type dbContextLocator, DbContextCorrelationType dbContextCorrelationType)
    {
        // 获取该实体类型的种子配置
        var entitySeedDataTypes = dbContextCorrelationType.EntitySeedDataTypes
            .Where(u => u.GetInterfaces()
                .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntitySeedData<>)) && i.GenericTypeArguments.Contains(entityType))).ToList();

        if (entitySeedDataTypes.Count == 0) return;

        var data = new List<object>();

        // 加载种子配置数据
        foreach (var entitySeedDataType in entitySeedDataTypes)
        {
            // 支持显式和隐式接口查找
            var hasDataMethod = entitySeedDataType.GetMethod("HasData")
                ?? typeof(IPrivateEntitySeedData<>).MakeGenericType(entityType).GetMethod("HasData");
            if (hasDataMethod == null) continue;

            var instance = Activator.CreateInstance(entitySeedDataType);
            var seedData = ((IList?)hasDataMethod?.Invoke(instance, [dbContext, dbContextLocator]))?.Cast<object>();
            if (seedData == null) continue;

            data.AddRange(seedData);
        }

        if (data.Count > 0)
        {
            entityBuilder?.HasData(data.ToArray());
        }
    }

    /// <summary>
    /// 配置数据库函数
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    private static void ConfigureDbFunctions(ModelBuilder modelBuilder, Type dbContextLocator)
    {
        var dbContextFunctionMethods = DbFunctionMethods.Where(u => IsInThisDbContext(dbContextLocator, u)).ToList();
        if (dbContextFunctionMethods.Count == 0) return;

        foreach (var method in dbContextFunctionMethods)
        {
            modelBuilder.HasDbFunction(method);
        }
    }

    /// <summary>
    /// 判断当前类型是否在数据库上下文中
    /// </summary>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="entityCorrelationType">数据库实体关联类型</param>
    /// <returns>bool</returns>
    private static bool IsInThisDbContext(Type dbContextLocator, Type entityCorrelationType)
    {
        // 处理自定义多租户的情况
        if (dbContextLocator == typeof(MultiTenantDbContextLocator) && Db.CustomizeMultiTenants && entityCorrelationType == typeof(Tenant)) return false;

        // 获取所有祖先类型和接口
        var ancestorTypes = entityCorrelationType.GetAncestorTypes().ToList();
        var interfaces = entityCorrelationType.GetInterfaces().Where(u => typeof(IPrivateEntity).IsAssignableFrom(u) || typeof(IPrivateModelBuilder).IsAssignableFrom(u)).ToList();

        // 祖先是泛型且泛型参数包含数据库上下文定位器
        if (ancestorTypes.Any(u => u.IsGenericType && u.GenericTypeArguments.Contains(dbContextLocator))) return true;

        // 接口是泛型且泛型参数包含数据库上下文定位器
        if (interfaces.Any(u => u.IsGenericType && u.GenericTypeArguments.Contains(dbContextLocator))) return true;

        return false;
    }

    /// <summary>
    /// 判断当前函数是否在数据库上下文中
    /// </summary>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <param name="method">标识为数据库的函数</param>
    /// <returns>bool</returns>
    private static bool IsInThisDbContext(Type dbContextLocator, MethodInfo method)
    {
        var queryableFunctionAttribute = method.GetCustomAttribute<QueryableFunctionAttribute>(true);

        // 如果数据库上下文定位器为默认定位器且该函数没有定义数据库上下文定位器，则返回 true
        if (dbContextLocator == typeof(MasterDbContextLocator) && queryableFunctionAttribute?.DbContextLocators.Length == 0) return true;

        // 判断是否包含当前数据库上下文
        if (queryableFunctionAttribute?.DbContextLocators.Contains(dbContextLocator) == true) return true;

        return false;
    }

    /// <summary>
    /// 数据库上下文定位器关联类型集合
    /// </summary>
    internal static ConcurrentDictionary<Type, DbContextCorrelationType> DbContextLocatorCorrelationTypes = new();

    /// <summary>
    /// 获取当前数据库上下文关联类型
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="dbContextLocator">数据库上下文定位器</param>
    /// <returns>DbContextCorrelationType</returns>
    private static DbContextCorrelationType GetDbContextCorrelationType(DbContext dbContext, Type dbContextLocator)
    {
        return DbContextLocatorCorrelationTypes.GetOrAdd(dbContextLocator, static (key, state) =>
        {
            var (dbContextInner, dbContextLocatorInner) = state;
            var result = new DbContextCorrelationType { DbContextLocator = dbContextLocatorInner };

            // 获取当前数据库上下文关联类型
            var dbContextEntityCorrelationTypes = EntityCorrelationTypes.Where(u => IsInThisDbContext(dbContextLocatorInner, u)).ToList();

            // 组装对象
            foreach (var entityCorrelationType in dbContextEntityCorrelationTypes)
            {
                // 只要继承 IEntityDependency 接口，都是实体，且不贴 [NotMapper] 特性
                if (typeof(IPrivateEntity).IsAssignableFrom(entityCorrelationType) && !entityCorrelationType.IsDefined(typeof(NotMappedAttribute), false))
                {
                    // 添加实体
                    result.EntityTypes.Add(entityCorrelationType);

                    // 添加无键实体
                    if (typeof(IPrivateEntityNotKey).IsAssignableFrom(entityCorrelationType))
                    {
                        result.EntityNoKeyTypes.Add(entityCorrelationType);
                    }
                }

                if (typeof(IPrivateModelBuilder).IsAssignableFrom(entityCorrelationType))
                {
                    // 添加模型构建器
                    if (entityCorrelationType.HasImplementedRawGeneric(typeof(IPrivateEntityTypeBuilder<>)))
                    {
                        result.EntityTypeBuilderTypes.Add(entityCorrelationType);
                    }

                    // 添加全局筛选器
                    if (entityCorrelationType.HasImplementedRawGeneric(typeof(IPrivateModelBuilderFilter)))
                    {
                        result.ModelBuilderFilterTypes.Add(entityCorrelationType);
                    }

                    // 添加种子数据
                    if (entityCorrelationType.HasImplementedRawGeneric(typeof(IPrivateEntitySeedData<>)))
                    {
                        result.EntitySeedDataTypes.Add(entityCorrelationType);
                    }

                    // 添加动态表类型
                    if (entityCorrelationType.HasImplementedRawGeneric(typeof(IPrivateEntityMutableTable<>)))
                    {
                        result.EntityMutableTableTypes.Add(entityCorrelationType);
                    }

                    // 添加实体数据改变监听
                    if (entityCorrelationType.HasImplementedRawGeneric(typeof(IPrivateEntityChangedListener<>)))
                    {
                        result.EntityChangedTypes.Add(entityCorrelationType);
                    }
                }
            }

            return result;
        }, (dbContext, dbContextLocator));
    }
}