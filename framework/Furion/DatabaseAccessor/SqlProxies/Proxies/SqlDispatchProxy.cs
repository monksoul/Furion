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
using Furion.Reflection;
using Furion.Templates.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Furion.DatabaseAccessor;

/// <summary>
/// Sql 执行代理类
/// </summary>
public class SqlDispatchProxy : DispatchProxyAsync, IDispatchProxy
{
    /// <summary>
    /// 实例对象
    /// </summary>
    public object Target { get; set; }

    /// <summary>
    /// 服务提供器
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// 当前异步上下文中的数据库上下文定位器
    /// </summary>
    private static readonly AsyncLocal<Type> _currentDbLocator = new();

    /// <summary>
    /// 拦截同步方法
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public override object Invoke(MethodInfo method, object[] args)
    {
        // 切换数据库上下文
        if (method.IsGenericMethod && method.Name == nameof(ISqlDispatchProxy.Change) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = method.GetGenericArguments().First();
            return default;
        }

        // 重置数据库上下文定位器
        if (method.Name == nameof(ISqlDispatchProxy.ResetIt) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = default;
            return default;
        }

        // 获取 Sql 代理方法信息
        var sqlProxyMethod = GetProxyMethod(method, args);
        return ExecuteSql(sqlProxyMethod);
    }

    /// <summary>
    /// 拦截异步方法
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async override Task InvokeAsync(MethodInfo method, object[] args)
    {
        // 切换数据库上下文
        if (method.IsGenericMethod && method.Name == nameof(ISqlDispatchProxy.Change) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = method.GetGenericArguments().First();
            return;
        }

        // 重置数据库上下文定位器
        if (method.Name == nameof(ISqlDispatchProxy.ResetIt) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = default;
            return;
        }

        // 获取 Sql 代理方法信息
        var sqlProxyMethod = GetProxyMethod(method, args);
        await ExecuteSqlAsync(sqlProxyMethod);
    }

    /// <summary>
    /// 拦截异步带返回值方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async override Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
    {
        // 切换数据库上下文
        if (method.IsGenericMethod && method.Name == nameof(ISqlDispatchProxy.Change) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = method.GetGenericArguments().First();
            return default;
        }

        // 重置数据库上下文定位器
        if (method.Name == nameof(ISqlDispatchProxy.ResetIt) && method.ReturnType == typeof(void))
        {
            _currentDbLocator.Value = default;
            return default;
        }

        // 获取 Sql 代理方法信息
        var sqlProxyMethod = GetProxyMethod(method, args);
        return await ExecuteSqlOfTAsync<T>(sqlProxyMethod);
    }

    /// <summary>
    /// 执行 Sql 操作
    /// </summary>
    /// <param name="sqlProxyMethod">代理方法</param>
    /// <returns></returns>
    private static object ExecuteSql(SqlProxyMethod sqlProxyMethod)
    {
        // 获取 ADO.NET 数据库操作对象
        var database = sqlProxyMethod.Context.Database;

        // 定义多次使用变量
        var returnType = sqlProxyMethod.ReturnType;
        var sql = sqlProxyMethod.FinalSql;
        var parameterModel = sqlProxyMethod.ParameterModel;
        var commandType = sqlProxyMethod.CommandType;

        // 处理 DataSet 返回值
        if (returnType == typeof(DataSet))
        {
            var (dataSet, _) = database.DataAdapterFill(sql, parameterModel, commandType);
            return dataSet;
        }
        // 处理 无返回值或返回受影响行数的情况
        else if (returnType == typeof(void) || (returnType == typeof(int) && sqlProxyMethod.RowEffects == true))
        {
            var (rowEffects, _) = database.ExecuteNonQuery(sql, parameterModel, commandType);
            return rowEffects;
        }
        // 处理 元组类型 返回值
        else if (returnType.IsValueTuple())
        {
            var (dataSet, _) = database.DataAdapterFill(sql, parameterModel, commandType);
            var result = dataSet.ToValueTuple(returnType);

            var tupleResult = (ITuple)result;
            var genericArguments = returnType.GetGenericArguments();

            // 查找 ValueTuple.Create<T1...TN> 静态方法
            var createTupleMethod = typeof(ValueTuple).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(u => u.Name == "Create" && u.GetGenericArguments().Length == tupleResult.Length);

            // 创建 ValueTuple.Create 参数
            var args = new object[tupleResult.Length];
            for (var i = 0; i < tupleResult.Length; i++)
            {
                // 处理单个值的情况
                if (!typeof(IEnumerable).IsAssignableFrom(genericArguments[i]))
                {
                    args[i] = ((IEnumerable)tupleResult[i])?.Cast<object>()?.FirstOrDefault();
                    continue;
                }

                // 处理数组类型
                if (genericArguments[i].IsArray)
                {
                    dynamic itemValue = tupleResult[i];
                    args[i] = itemValue == null ? null : itemValue.Count > 0 ? itemValue[0] : itemValue;
                    continue;
                }

                args[i] = tupleResult[i];
            }

            // 调用 ValueTuple.Create<T1..TN> 静态方法
            var tupleObject = createTupleMethod.MakeGenericMethod(genericArguments).Invoke(null, args);
            return tupleObject;
        }
        // 处理 基元类型 返回值
        else if (returnType.IsRichPrimitive())
        {
            var (result, _) = database.ExecuteScalar(sql, parameterModel, commandType);
            return result.ChangeType(returnType);
        }
        // 处理 存储过程带输出类型 返回值
        else if (returnType == typeof(ProcedureOutputResult) || (returnType.IsGenericType && typeof(ProcedureOutputResult<>).IsAssignableFrom(returnType.GetGenericTypeDefinition())))
        {
            var (dataSet, dbParameters) = database.DataAdapterFill(sql, parameterModel, commandType);

            // 处理返回值
            var result = !returnType.IsGenericType
                ? DbHelpers.WrapperProcedureOutput(database.ProviderName, dbParameters, dataSet)
                : DbHelpers.WrapperProcedureOutput(database.ProviderName, dbParameters, dataSet, returnType.GenericTypeArguments.First());
            return result;
        }
        else
        {
            var (dataTable, _) = database.ExecuteReader(sql, parameterModel, commandType);

            // 处理 DataTable 返回值
            if (returnType == typeof(DataTable)) return dataTable;
            else
            {
                var @object = dataTable.ToList(returnType);
                var listObject = ((IEnumerable)@object)?.Cast<object>();

                // 如果是集合参数
                if (typeof(IEnumerable).IsAssignableFrom(returnType))
                {
                    return returnType.IsArray ? ((dynamic)@object).ToArray() : @object;
                }

                // 否则取第一条
                return listObject?.FirstOrDefault();
            }
        }
    }

    /// <summary>
    /// 执行 Sql 操作
    /// </summary>
    /// <param name="sqlProxyMethod">代理方法</param>
    /// <returns></returns>
    private static async Task ExecuteSqlAsync(SqlProxyMethod sqlProxyMethod)
    {
        // 获取 ADO.NET 数据库操作对象
        var database = sqlProxyMethod.Context.Database;

        // 定义多次使用变量
        var sql = sqlProxyMethod.FinalSql;
        var parameterModel = sqlProxyMethod.ParameterModel;
        var commandType = sqlProxyMethod.CommandType;

        _ = await database.ExecuteNonQueryAsync(sql, parameterModel, commandType);
    }

    /// <summary>
    /// 执行 Sql 操作
    /// </summary>
    /// <param name="sqlProxyMethod">代理方法</param>
    /// <returns></returns>
    private static async Task<T> ExecuteSqlOfTAsync<T>(SqlProxyMethod sqlProxyMethod)
    {
        // 获取 ADO.NET 数据库操作对象
        var database = sqlProxyMethod.Context.Database;

        // 定义多次使用变量
        var returnType = sqlProxyMethod.ReturnType;
        var sql = sqlProxyMethod.FinalSql;
        var parameterModel = sqlProxyMethod.ParameterModel;
        var commandType = sqlProxyMethod.CommandType;

        // 处理 DataSet 返回值
        if (returnType == typeof(DataSet))
        {
            var (dataSet, _) = await database.DataAdapterFillAsync(sql, parameterModel, commandType);
            return (T)(dataSet as object);
        }
        // 处理返回受影响行数的情况
        else if (returnType == typeof(int) && sqlProxyMethod.RowEffects == true)
        {
            var (rowEffects, _) = database.ExecuteNonQuery(sql, parameterModel, commandType);
            return (T)(rowEffects as object);
        }
        // 处理 元组类型 返回值
        else if (returnType.IsValueTuple())
        {
            var (dataSet, _) = await database.DataAdapterFillAsync(sql, parameterModel, commandType);
            var result = dataSet.ToValueTuple(returnType);

            var tupleResult = (ITuple)result;
            var genericArguments = returnType.GetGenericArguments();

            // 查找 ValueTuple.Create<T1...TN> 静态方法
            var createTupleMethod = typeof(ValueTuple).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(u => u.Name == "Create" && u.GetGenericArguments().Length == tupleResult.Length);

            // 创建 ValueTuple.Create 参数
            var args = new object[tupleResult.Length];
            for (var i = 0; i < tupleResult.Length; i++)
            {
                // 处理单个值的情况
                if (!typeof(IEnumerable).IsAssignableFrom(genericArguments[i]))
                {
                    args[i] = ((IEnumerable)tupleResult[i])?.Cast<object>()?.FirstOrDefault();
                    continue;
                }

                // 处理数组类型
                if (genericArguments[i].IsArray)
                {
                    dynamic itemValue = tupleResult[i];
                    args[i] = itemValue == null ? null : itemValue.Count > 0 ? itemValue[0] : itemValue;
                    continue;
                }

                args[i] = tupleResult[i];
            }

            // 调用 ValueTuple.Create<T1..TN> 静态方法
            var tupleObject = createTupleMethod.MakeGenericMethod(genericArguments).Invoke(null, args);
            return (T)tupleObject;
        }
        // 处理 基元类型 返回值
        else if (returnType.IsRichPrimitive())
        {
            var (result, _) = await database.ExecuteScalarAsync(sql, parameterModel, commandType);
            return (T)result;
        }
        // 处理 存储过程带输出类型 返回值
        else if (returnType == typeof(ProcedureOutputResult) || (returnType.IsGenericType && typeof(ProcedureOutputResult<>).IsAssignableFrom(returnType.GetGenericTypeDefinition())))
        {
            var (dataSet, dbParameters) = await database.DataAdapterFillAsync(sql, parameterModel, commandType);

            // 处理返回值
            var result = !returnType.IsGenericType
                ? DbHelpers.WrapperProcedureOutput(database.ProviderName, dbParameters, dataSet)
                : DbHelpers.WrapperProcedureOutput(database.ProviderName, dbParameters, dataSet, returnType.GenericTypeArguments.First());

            return (T)result;
        }
        else
        {
            var (dataTable, _) = await database.ExecuteReaderAsync(sql, parameterModel, commandType);

            // 处理 DataTable 返回值
            if (returnType == typeof(DataTable)) return (T)(dataTable as object);
            else
            {
                var @object = await dataTable.ToListAsync(returnType);
                var listObject = ((IEnumerable)@object)?.Cast<object>();

                // 如果是集合参数
                if (typeof(IEnumerable).IsAssignableFrom(returnType))
                {
                    return (T)(returnType.IsArray ? ((dynamic)@object).ToArray() : @object);
                }

                // 否则取第一条
                return (T)(listObject?.FirstOrDefault());
            }
        }
    }

    /// <summary>
    /// 获取代理方法信息
    /// </summary>
    /// <param name="method">方法</param>
    /// <param name="args">参数列表</param>
    /// <returns>SqlProxyMethod</returns>
    private SqlProxyMethod GetProxyMethod(MethodInfo method, object[] args)
    {
        // 判断方法是否贴了注解
        if (!method.IsDefined(typeof(SqlProxyAttribute), true)) throw new InvalidOperationException("The method is missing the [SqlProxy] annotation.");

        // 获取 Sql 代理特性
        var sqlProxyAttribute = method.GetCustomAttribute<SqlProxyAttribute>(true);

        // 获取方法真实返回值类型
        var returnType = method.GetRealReturnType();

        // 获取方法所在类型
        var declaringType = method.DeclaringType;

        // 获取数据库上下文
        var dbContext = GetDbContext(method);

        // 转换方法参数
        var parameters = CombineDbParameter(method, args);

        // 定义最终 Sql 语句
        string finalSql;
        var commandType = CommandType.Text;
        var rowEffects = false;

        // 如果是存储过程类型
        if (sqlProxyAttribute is SqlProcedureAttribute sqlProduceAttribute)
        {
            finalSql = sqlProduceAttribute.Name;
            commandType = CommandType.StoredProcedure;
        }
        // 如果是函数类型
        else if (sqlProxyAttribute is SqlFunctionAttribute sqlFunctionAttribute)
        {
            finalSql = DbHelpers.GenerateFunctionSql(dbContext.Database.ProviderName,
                returnType.IsRichPrimitive() ? DbFunctionType.Scalar : DbFunctionType.Table,
                sqlFunctionAttribute.Name,
                parameters);
        }
        // 如果是纯Sql类型
        else if (sqlProxyAttribute is SqlExecuteAttribute sqlExecuteAttribute)
        {
            finalSql = sqlExecuteAttribute.Sql;
            commandType = sqlExecuteAttribute.CommandType;
            rowEffects = sqlExecuteAttribute.RowEffects;
        }
        else throw new NotSupportedException($"{sqlProxyAttribute.GetType().FullName} is an invalid annotation.");

        // 解析方法参数及参数值并渲染模板
        var methodParameterInfos = method.GetParameters().Select((u, i) => new MethodParameterInfo
        {
            Parameter = u,
            Name = u.Name,
            Value = args[i]
        });

        // 渲染模板
        finalSql = finalSql.Render(methodParameterInfos.ToDictionary(u => u.Name, u => u.Value));

        // 返回
        var sqlProxyMethod = new SqlProxyMethod
        {
            ParameterModel = parameters,
            Context = dbContext,
            ReturnType = returnType,
            IsAsync = method.IsAsync(),
            CommandType = commandType,
            FinalSql = finalSql,
            Method = method,
            Arguments = args,
            InterceptorId = string.IsNullOrWhiteSpace(sqlProxyAttribute.InterceptorId) ? method.Name : sqlProxyAttribute.InterceptorId,
            RowEffects = rowEffects
        };

        // 添加方法拦截
        CallMethodInterceptors(declaringType, sqlProxyMethod);

        return sqlProxyMethod;
    }

    /// <summary>
    /// 创建数据库命令参数字典
    /// </summary>
    /// <param name="method"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private static object CombineDbParameter(MethodInfo method, object[] arguments)
    {
        var parameters = method.GetParameters();
        if (parameters == null || parameters.Length == 0) return null;

        // 只支持要么全是基元类型，或全部都是类类型
        if (!parameters.All(u => u.ParameterType.IsRichPrimitive()) && !parameters.All(u => u.ParameterType.IsClass))
            throw new InvalidOperationException("Invalid type cast.");

        if (parameters.All(u => u.ParameterType.IsRichPrimitive()))
        {
            var dic = new Dictionary<string, object>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterValue = arguments.Length > i ? arguments[i] : null;
                dic.Add(parameter.Name, parameterValue);
            }
            return dic;
        }
        // 直接使用第一个模型转 DbParameters 参数
        else return arguments.First();
    }

    /// <summary>
    /// 获取数据库上下文
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private DbContext GetDbContext(MethodInfo method)
    {
        // 解析数据库上下文定位器
        var dbContextLocator = _currentDbLocator.Value
            ?? method.GetFoundAttribute<SqlDbContextLocatorAttribute>(true)?.Locator
            ?? typeof(MasterDbContextLocator);

        var dbContext = Db.GetDbContext(dbContextLocator, Services);

        // 设置 ADO.NET 超时时间
        var timeout = method.GetFoundAttribute<TimeoutAttribute>(true)?.Seconds;
        if (timeout != null && timeout.Value > 0) dbContext.Database.SetCommandTimeout(timeout.Value);

        return dbContext;
    }

    /// <summary>
    /// 添加方法拦截
    /// </summary>
    /// <param name="declaringType"></param>
    /// <param name="sqlProxyMethod"></param>
    private static void CallMethodInterceptors(Type declaringType, SqlProxyMethod sqlProxyMethod)
    {
        // 获取所有静态方法且贴有 [SqlInterceptor] 特性
        var interceptorMethods = declaringType.GetMethods()
                                                                .Where(u => u.IsDefined(typeof(SqlInterceptorAttribute), true));

        foreach (var method in interceptorMethods)
        {
            var interceptorAttribute = method.GetCustomAttribute<SqlInterceptorAttribute>(true);

            var interceptorIds = interceptorAttribute.InterceptorIds;

            // 如果拦截Id数组不为空且包含当前拦截Id，则跳过
            if (interceptorIds != null && interceptorIds.Length > 0 && !interceptorIds.Contains(sqlProxyMethod.InterceptorId, StringComparer.OrdinalIgnoreCase)) continue;

            var onInterceptor = (Action<SqlProxyMethod>)Delegate.CreateDelegate(typeof(Action<SqlProxyMethod>), method);
            onInterceptor(sqlProxyMethod);
        }
    }
}