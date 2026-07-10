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
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     <c>application/msgpack</c> 内容处理器
/// </summary>
/// <remarks>要使用 <c>application/msgpack</c> 内容处理器需在项目中安装 <c>MessagePack</c> 依赖包。https://www.nuget.org/packages/MessagePack。</remarks>
public class MessagePackContentProcessor : HttpContentProcessorBase
{
    /// <summary>
    ///     MessagePack 序列化器委托字典缓存
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, Func<object, byte[]>> _serializerCache = new();

    /// <summary>
    ///     初始化 MessagePack 序列化器委托
    /// </summary>
    internal static readonly Lazy<Func<object, byte[]>> _messagePackSerializerLazy = new(() =>
    {
        // 尝试加载 MessagePack 包中的 MessagePackSerializer 类型
        var messagePackSerializerType = Type.GetType("MessagePack.MessagePackSerializer, MessagePack");

        // 空检查
        if (messagePackSerializerType is null)
        {
            throw new InvalidOperationException("Please ensure the `MessagePack` package is installed.");
        }

        // 查找方法：public static byte[] Serialize<T>(T, MessagePackSerializerOptions?, CancellationToken);
        var serializeMethod = messagePackSerializerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(u =>
                u is { Name: "Serialize", IsGenericMethod: true } && u.ReturnType == typeof(byte[]) &&
                u.GetParameters().Length == 3 &&
                u.GetGenericArguments().Length == 1)!;

        // 返回调用委托
        return CreateSerializerDelegate(serializeMethod);
    });

    /// <summary>
    ///     MessagePack 序列化器委托
    /// </summary>
    internal static Func<object, byte[]> MessagePackSerializer => _messagePackSerializerLazy.Value;

    /// <inheritdoc />
    public override bool CanProcess(HttpContentProcessorContext context) =>
        context.ContentType.IsIn(["application/msgpack"], StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override HttpContent? Process(HttpContentProcessorContext context)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(context, out var httpContent))
        {
            return httpContent;
        }

        // 将原始请求内容转换为字节数组
        var content = context.RawContent as byte[] ?? MessagePackSerializer(context.RawContent!);

        // 初始化 ByteArrayContent 实例
        var byteArrayContent = new ByteArrayContent(content);
        byteArrayContent.Headers.ContentType =
            new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName };

        return byteArrayContent;
    }

    /// <summary>
    ///     创建 MessagePack 序列化器委托
    /// </summary>
    /// <param name="serializeMethod">
    ///     <see cref="MethodInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Func{T1, T2}" />
    /// </returns>
    internal static Func<object, byte[]> CreateSerializerDelegate(MethodInfo serializeMethod)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serializeMethod);

        return obj =>
        {
            // 获取对象类型
            var objType = obj.GetType();

            // 查找 MessagePack 序列化器委托字典缓存是否存在该类型
            var serializer = _serializerCache.GetOrAdd(objType, type =>
            {
                // 创建 MessagePack 序列化器委托
                return o => (byte[])serializeMethod.MakeGenericMethod(type)
                    .Invoke(null, [o, null, CancellationToken.None])!;
            });

            return serializer(obj);
        };
    }
}