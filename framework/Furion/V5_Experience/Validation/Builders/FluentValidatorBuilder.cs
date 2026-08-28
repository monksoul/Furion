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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Furion.Validation;

/// <summary>
///     链式验证器构建器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class FluentValidatorBuilder<T> : FluentValidatorBuilder<T, FluentValidatorBuilder<T>>;

/// <summary>
///     链式验证器构建器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
/// <typeparam name="TSelf">派生类型自身类型</typeparam>
public abstract class FluentValidatorBuilder<T, TSelf> : IValidatorInitializer
    where TSelf : FluentValidatorBuilder<T, TSelf>
{
    /// <summary>
    ///     高优先级验证器区域的结束索引
    /// </summary>
    /// <remarks>同时也是普通验证器区域的起始索引。</remarks>
    internal int _highPriorityEndIndex;

    /// <summary>
    ///     跟踪最新添加的 <see cref="ValidatorBase" /> 实例
    /// </summary>
    internal ValidatorBase? _lastAddedValidator;

    /// <summary>
    ///     对象验证器区域的起始索引
    /// </summary>
    /// <remarks>同时也是普通验证器区域结束索引。</remarks>
    internal int _objectValidatorStartIndex;

    /// <summary>
    ///     <see cref="IServiceProvider" /> 委托
    /// </summary>
    internal Func<Type, object?>? _serviceProvider;

    /// <summary>
    ///     <inheritdoc cref="FluentValidatorBuilder{T,TSelf}" />
    /// </summary>
    internal FluentValidatorBuilder()
        : this(null, null)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="FluentValidatorBuilder{T,TSelf}" />
    /// </summary>
    /// <param name="items">共享数据</param>
    internal FluentValidatorBuilder(IDictionary<object, object?>? items)
        : this(null, items)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="FluentValidatorBuilder{T,TSelf}" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="items">共享数据</param>
    internal FluentValidatorBuilder(IServiceProvider? serviceProvider, IDictionary<object, object?>? items)
    {
        // 空检查
        if (serviceProvider is not null)
        {
            _serviceProvider = serviceProvider.GetService;
        }

        Items = items is not null ? new Dictionary<object, object?>(items) : new Dictionary<object, object?>();
        Validators = [];
    }

    /// <summary>
    ///     派生类型自身引用
    /// </summary>
    public TSelf This => (TSelf)this;

    /// <summary>
    ///     共享数据
    /// </summary>
    public IDictionary<object, object?> Items { get; }

    /// <summary>
    ///     验证器列表
    /// </summary>
    internal List<ValidatorBase> Validators { get; }

    /// <inheritdoc />
    void IValidatorInitializer.InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        InitializeServiceProvider(serviceProvider);

    /// <summary>
    ///     获取验证器列表
    /// </summary>
    /// <returns>
    ///     <see cref="IReadOnlyList{T}" />
    /// </returns>
    public IReadOnlyList<ValidatorBase> GetValidators() => Validators;

    /// <summary>
    ///     设置错误信息
    /// </summary>
    /// <param name="errorMessage">错误信息</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf WithMessage(string? errorMessage)
    {
        // 空检查
        if (_lastAddedValidator is null)
        {
            return This;
        }

        // 将错误信息设置给最新添加的验证器实例
        _lastAddedValidator.WithMessage(errorMessage);

        // 重置最新添加的验证器实例
        _lastAddedValidator = null;

        return This;
    }

    /// <summary>
    ///     设置错误信息
    /// </summary>
    /// <param name="resourceType">错误信息资源类型</param>
    /// <param name="resourceName">错误信息资源名称</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf WithMessage(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type resourceType, string resourceName)
    {
        // 空检查
        if (_lastAddedValidator is null)
        {
            return This;
        }

        // 将错误信息设置给最新添加的验证器实例
        _lastAddedValidator.WithMessage(resourceType, resourceName);

        // 重置最新添加的验证器实例
        _lastAddedValidator = null;

        return This;
    }

    /// <summary>
    ///     添加年龄（0-120 岁）验证器
    /// </summary>
    /// <param name="isAdultOnly">是否仅允许成年人（18 岁及以上），默认值为：<c>false</c></param>
    /// <param name="allowStringValues">是否允许字符串数值，默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Age(bool isAdultOnly = false, bool allowStringValues = false) =>
        AddValidator(new AgeValidator { IsAdultOnly = isAdultOnly, AllowStringValues = allowStringValues });

    /// <summary>
    ///     添加允许的值列表验证器
    /// </summary>
    /// <param name="values">允许的值列表</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf AllowedValues(params object?[] values) => AddValidator(new AllowedValuesValidator(values));

    /// <summary>
    ///     添加银行卡号验证器（Luhn 算法）
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf BankCard() => AddValidator(new BankCardValidator());

    /// <summary>
    ///     添加 Base64 字符串验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Base64String() => AddValidator(new Base64StringValidator());

    /// <summary>
    ///     添加中文姓名验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf ChineseName() => AddValidator(new ChineseNameValidator());

    /// <summary>
    ///     添加中文/汉字验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Chinese() => AddValidator(new ChineseValidator());

    /// <summary>
    ///     添加颜色验证器
    /// </summary>
    /// <param name="fullMode">
    ///     是否启用完整模式。在完整模式下，支持的颜色格式包括：十六进制颜色、RGB、RGBA、HSL 和 HSLA；若未启用，则仅支持十六进制颜色、RGB 和 RGBA。默认值为：<c>false</c>
    /// </param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Color(bool fullMode = false) =>
        AddValidator(new ColorValidator { FullMode = fullMode });

    /// <summary>
    ///     添加组合验证器
    /// </summary>
    /// <param name="validators">验证器列表</param>
    /// <param name="ruleMode"><see cref="RuleMode" />，默认值为：<see cref="RuleMode.FailFast" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Composite(ValidatorBase[] validators, RuleMode ruleMode = RuleMode.FailFast) =>
        AddValidator(new CompositeValidator<T>(validators, ruleMode));

    /// <summary>
    ///     添加组合验证器
    /// </summary>
    /// <param name="configure">验证器配置委托</param>
    /// <param name="ruleMode"><see cref="RuleMode" />，默认值为：<see cref="RuleMode.FailFast" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Composite(Action<FluentValidatorBuilder<T>> configure,
        RuleMode ruleMode = RuleMode.FailFast) => AddValidator(new CompositeValidator<T>(configure, ruleMode));

    /// <summary>
    ///     添加条件验证器
    /// </summary>
    /// <param name="buildConditions">条件构建器配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Conditional(Action<ConditionBuilder<T>> buildConditions) =>
        AddValidator(new ConditionalValidator<T>(buildConditions));

    /// <summary>
    ///     添加自定义验证特性验证器
    /// </summary>
    /// <param name="validatorType">执行自定义验证的类型</param>
    /// <param name="method">验证方法</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf CustomValidation(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        Type validatorType, string method) =>
        WithAttributes(new CustomValidationAttribute(validatorType, method));

    /// <summary>
    ///     添加 <see cref="System.DateOnly" /> 日期验证器
    /// </summary>
    /// <param name="formats">允许的日期格式列表（如 "yyyy-MM-dd"）</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf DateOnly(params string[] formats) => AddValidator(new DateOnlyValidator(formats));

    /// <summary>
    ///     添加 <see cref="System.DateOnly" /> 日期验证器
    /// </summary>
    /// <param name="formats">允许的日期格式列表（如 "yyyy-MM-dd"）</param>
    /// <param name="provider">格式提供器</param>
    /// <param name="style">日期解析样式，需与 <paramref name="formats" /> 搭配使用。默认值为：<see cref="DateTimeStyles.None" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf DateOnly(string[] formats, IFormatProvider? provider,
        DateTimeStyles style = DateTimeStyles.None) =>
        AddValidator(new DateOnlyValidator(formats) { Provider = provider, Style = style });

    /// <summary>
    ///     添加 <see cref="System.DateTime" /> 时刻验证器
    /// </summary>
    /// <param name="formats">允许的日期时间格式列表（如 "yyyy-MM-dd HH:mm:ss"）</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf DateTime(params string[] formats) => AddValidator(new DateTimeValidator(formats));

    /// <summary>
    ///     添加 <see cref="System.DateTime" /> 时刻验证器
    /// </summary>
    /// <param name="formats">允许的日期时间格式列表（如 "yyyy-MM-dd HH:mm:ss"）</param>
    /// <param name="provider">格式提供器</param>
    /// <param name="style">日期时间解析样式，需与 <paramref name="formats" /> 搭配使用。默认值为：<see cref="DateTimeStyles.None" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf DateTime(string[] formats, IFormatProvider? provider,
        DateTimeStyles style = DateTimeStyles.None) =>
        AddValidator(new DateTimeValidator(formats) { Provider = provider, Style = style });

    /// <summary>
    ///     添加验证值是否为有效的 <see cref="decimal" /> 类型验证器
    /// </summary>
    /// <param name="precision">总位数（精度），默认值为：<c>18</c></param>
    /// <param name="scale">小数位数（标度），默认值为：<c>2</c></param>
    /// <param name="allowNegative">是否允许负数，默认值为：<c>false</c></param>
    /// <param name="allowStringValues">是否允许字符串数值，默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Decimal(int precision = 18, int scale = 2, bool allowNegative = false,
        bool allowStringValues = false) =>
        AddValidator(new DecimalValidator(precision, scale)
        {
            AllowNegative = allowNegative, AllowStringValues = allowStringValues
        });

    /// <summary>
    ///     添加不允许的值列表验证器
    /// </summary>
    /// <param name="values">不允许的值列表</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf DeniedValues(params object?[] values) => AddValidator(new DeniedValuesValidator(values));

    /// <summary>
    ///     添加域名验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Domain() => AddValidator(new DomainValidator());

    /// <summary>
    ///     添加邮箱地址验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf EmailAddress() => AddValidator(new EmailAddressValidator());

    /// <summary>
    ///     添加空集合、数组和字符串验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Empty() => AddValidator(new EmptyValidator());

    /// <summary>
    ///     添加以特定字符/字符串结尾的验证器
    /// </summary>
    /// <param name="searchValue">检索的值</param>
    /// <param name="comparison"><see cref="StringComparison" />，默认值为：<see cref="StringComparison.Ordinal" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf EndsWith(string searchValue, StringComparison comparison = StringComparison.Ordinal) =>
        AddValidator(new EndsWithValidator(searchValue) { Comparison = comparison });

    /// <summary>
    ///     添加枚举验证器
    /// </summary>
    /// <param name="supportFlags">是否支持 Flags 模式。默认值为：<c>false</c>。</param>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Enum<TEnum>(bool supportFlags = false)
        where TEnum : struct, Enum =>
        AddValidator(new EnumValidator<TEnum> { SupportFlags = supportFlags });

    /// <summary>
    ///     添加枚举验证器
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <param name="supportFlags">是否支持 Flags 模式。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Enum(Type enumType, bool supportFlags = false) =>
        AddValidator(new EnumValidator(enumType) { SupportFlags = supportFlags });

    /// <summary>
    ///     添加相等验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf EqualTo(object? compareValue) => AddValidator(new EqualToValidator(compareValue));

    /// <summary>
    ///     添加永不通过验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Never() => AddValidator(new NeverValidator());

    /// <summary>
    ///     添加文件扩展名验证器
    /// </summary>
    /// <remarks>默认文件扩展名为：<c>png,jpg,jpeg,gif</c>。</remarks>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf FileExtensions() => AddValidator(new FileExtensionsValidator());

    /// <summary>
    ///     添加文件扩展名验证器
    /// </summary>
    /// <param name="extensions">文件扩展名</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf FileExtensions(string extensions) =>
        AddValidator(new FileExtensionsValidator { Extensions = extensions });

    /// <summary>
    ///     添加大于等于验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf GreaterThanOrEqualTo(IComparable compareValue) =>
        AddValidator(new GreaterThanOrEqualToValidator(compareValue));

    /// <summary>
    ///     添加大于验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf GreaterThan(IComparable compareValue) => AddValidator(new GreaterThanValidator(compareValue));

    /// <summary>
    ///     添加固定长度验证器
    /// </summary>
    /// <param name="length">长度</param>
    /// <param name="allowEmpty">是否允许空集合、数组和字符串，默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf HaveLength(int length, bool allowEmpty = false) =>
        AddValidator(new HaveLengthValidator(length) { AllowEmpty = allowEmpty });

    /// <summary>
    ///     添加身份证号验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf IDCard() => AddValidator(new IDCardValidator());

    /// <summary>
    ///     添加 IP 地址验证器
    /// </summary>
    /// <param name="allowIPv6">是否允许 IPv6 地址，默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf IpAddress(bool allowIPv6 = false) =>
        AddValidator(new IpAddressValidator { AllowIPv6 = allowIPv6 });

    /// <summary>
    ///     添加 JSON 格式验证器
    /// </summary>
    /// <param name="allowTrailingCommas">是否允许末尾多余逗号，默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Json(bool allowTrailingCommas = false) =>
        AddValidator(new JsonValidator { AllowTrailingCommas = allowTrailingCommas });

    /// <summary>
    ///     添加长度验证器
    /// </summary>
    /// <param name="minimumLength">最小允许长度</param>
    /// <param name="maximumLength">最大允许长度</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Length(int minimumLength, int maximumLength) =>
        AddValidator(new LengthValidator(minimumLength, maximumLength));

    /// <summary>
    ///     添加小于等于验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf LessThanOrEqualTo(IComparable compareValue) =>
        AddValidator(new LessThanOrEqualToValidator(compareValue));

    /// <summary>
    ///     添加小于验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf LessThan(IComparable compareValue) => AddValidator(new LessThanValidator(compareValue));

    /// <summary>
    ///     添加最大长度验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MaxLength() => AddValidator(new MaxLengthValidator());

    /// <summary>
    ///     添加最大长度验证器
    /// </summary>
    /// <param name="length">最大允许长度</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MaxLength(int length) => AddValidator(new MaxLengthValidator(length));

    /// <summary>
    ///     添加最大值验证器
    /// </summary>
    /// <param name="maximum">允许的最大字段值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Max(IComparable maximum) => AddValidator(new MaxValidator(maximum));

    /// <summary>
    ///     添加 MD5 字符串验证器
    /// </summary>
    /// <param name="allowShortFormat">是否允许截断的 128 位哈希值（16 字节的十六进制字符串，共 32 字符），默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MD5String(bool allowShortFormat = false) =>
        AddValidator(new MD5StringValidator { AllowShortFormat = allowShortFormat });

    /// <summary>
    ///     添加最小长度验证器
    /// </summary>
    /// <param name="length">最小允许长度</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MinLength(int length) => AddValidator(new MinLengthValidator(length));

    /// <summary>
    ///     添加最小值验证器
    /// </summary>
    /// <param name="minimum">允许的最小字段值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Min(IComparable minimum) => AddValidator(new MinValidator(minimum));

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Must(Func<T, bool> condition) => AddValidator(new MustValidator<T>(condition));

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Must(Func<T, ValidationContext<T>, bool> condition) =>
        AddValidator(new MustValidator<T>(condition));

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Must(Func<T, Task<bool>> condition) => AddValidator(new MustValidator<T>(condition));

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Must(Func<T, ValidationContext<T>, Task<bool>> condition) =>
        AddValidator(new MustValidator<T>(condition));

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="enumerable">集合</param>
    /// <param name="condition">条件委托</param>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustAny<TElement>(IEnumerable<TElement> enumerable, Func<T, TElement, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumerable);
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>(u => enumerable.Any(x => condition(u, x))));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="enumerable">集合</param>
    /// <param name="condition">条件委托</param>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustAny<TElement>(IEnumerable<TElement> enumerable,
        Func<T, TElement, ValidationContext<T>, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumerable);
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>((u, c) => enumerable.Any(x => condition(u, x, c))));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="enumerable">集合</param>
    /// <param name="condition">条件委托</param>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustAll<TElement>(IEnumerable<TElement> enumerable, Func<T, TElement, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumerable);
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>(u => enumerable.All(x => condition(u, x))));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <param name="enumerable">集合</param>
    /// <param name="condition">条件委托</param>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustAll<TElement>(IEnumerable<TElement> enumerable,
        Func<T, TElement, ValidationContext<T>, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumerable);
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>((u, c) => enumerable.All(x => condition(u, x, c))));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <remarks>仅当被验证的值不为 <c>null</c> 时才执行验证逻辑。</remarks>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustIfNotNull(Func<T, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>(u => u is null || condition(u)));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <remarks>仅当被验证的值不为 <c>null</c> 时才执行验证逻辑。</remarks>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustIfNotNull(Func<T, ValidationContext<T>, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>((u, c) => u is null || condition(u, c)));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <remarks>仅当被验证的值不为 <c>null</c>、非空集合、数组和字符串时才执行验证逻辑。</remarks>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustIfNotNullOrEmpty(Func<T, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>(u =>
            u is null || (u.TryGetCount(out var count) && count == 0) || condition(u)));
    }

    /// <summary>
    ///     添加自定义条件成立时委托验证器
    /// </summary>
    /// <remarks>仅当被验证的值不为 <c>null</c>、非空集合、数组和字符串时才执行验证逻辑。</remarks>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf MustIfNotNullOrEmpty(Func<T, ValidationContext<T>, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(condition);

        return AddValidator(new MustValidator<T>((u, c) =>
            u is null || (u.TryGetCount(out var count) && count == 0) || condition(u, c)));
    }

    /// <summary>
    ///     添加非空白字符串验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf NotBlank() => AddValidator(new NotBlankValidator());

    /// <summary>
    ///     添加非空集合、数组和字符串验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf NotEmpty() => AddValidator(new NotEmptyValidator());

    /// <summary>
    ///     添加不相等验证器
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf NotEqualTo(object? compareValue) => AddValidator(new NotEqualToValidator(compareValue));

    /// <summary>
    ///     添加非 null 验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf NotNull() => AddValidator(new NotNullValidator());

    /// <summary>
    ///     添加取反验证器
    /// </summary>
    /// <param name="validators">验证器列表</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Not(ValidatorBase[] validators) =>
        AddValidator(new NotValidator<T>(validators));

    /// <summary>
    ///     添加取反验证器
    /// </summary>
    /// <param name="configure">验证器配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Not(Action<FluentValidatorBuilder<T>> configure) =>
        AddValidator(new NotValidator<T>(configure));

    /// <summary>
    ///     添加 null 验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Null() => AddValidator(new NullValidator());

    /// <summary>
    ///     添加密码验证器
    /// </summary>
    /// <param name="strong">是否启用强密码验证模式，默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Password(bool strong = false) => AddValidator(new PasswordValidator { Strong = strong });

    /// <summary>
    ///     添加手机号（中国）验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf PhoneNumber() => AddValidator(new PhoneNumberValidator());

    /// <summary>
    ///     添加邮政编码（中国）验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf PostalCode() => AddValidator(new PostalCodeValidator());

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Range(int minimum, int maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(minimum, maximum), configure);

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Range(double minimum, double maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(minimum, maximum), configure);

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="type">数据字段值的类型</param>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Range([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        string minimum,
        string maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(type, minimum, maximum), configure);

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Between(int minimum, int maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(minimum, maximum), configure);

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Between(double minimum, double maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(minimum, maximum), configure);

    /// <summary>
    ///     添加指定数值范围约束验证器
    /// </summary>
    /// <param name="type">数据字段值的类型</param>
    /// <param name="minimum">允许的最小字段值</param>
    /// <param name="maximum">允许的最大字段值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Between([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        string minimum,
        string maximum, Action<RangeValidator>? configure = null) =>
        AddValidator(new RangeValidator(type, minimum, maximum), configure);

    /// <summary>
    ///     添加正则表达式验证器
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="matchTimeoutInMilliseconds">用于在操作超时前执行单个匹配操作的时间量。以毫秒为单位，默认值为：2000。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf RegularExpression([StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        int matchTimeoutInMilliseconds = 2000) =>
        AddValidator(
            new RegularExpressionValidator(pattern) { MatchTimeoutInMilliseconds = matchTimeoutInMilliseconds });

    /// <summary>
    ///     添加正则表达式验证器
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="matchTimeoutInMilliseconds">用于在操作超时前执行单个匹配操作的时间量。以毫秒为单位，默认值为：2000。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Matches([StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        int matchTimeoutInMilliseconds = 2000) =>
        AddValidator(
            new RegularExpressionValidator(pattern) { MatchTimeoutInMilliseconds = matchTimeoutInMilliseconds });

    /// <summary>
    ///     添加必填验证器
    /// </summary>
    /// <param name="allowEmptyStrings">是否允许空字符串。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Required(bool allowEmptyStrings = false) =>
        AddValidator(new RequiredValidator { AllowEmptyStrings = allowEmptyStrings });

    /// <summary>
    ///     添加敏感词验证器
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf SensitiveWord(Action<SensitiveWordValidator>? configure = null) =>
        AddValidator(new SensitiveWordValidator(), configure);

    /// <summary>
    ///     添加敏感词验证器
    /// </summary>
    /// <param name="dictionaryName">敏感词字典名称，不区分大小写</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf SensitiveWord(string dictionaryName, Action<SensitiveWordValidator>? configure = null) =>
        AddValidator(new SensitiveWordValidator(dictionaryName), configure);

    /// <summary>
    ///     添加敏感词验证器
    /// </summary>
    /// <param name="sanitizer">
    ///     <see cref="SensitiveWordSanitizer" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf SensitiveWord(SensitiveWordSanitizer sanitizer,
        Action<SensitiveWordValidator>? configure = null) =>
        AddValidator(new SensitiveWordValidator(sanitizer), configure);

    /// <summary>
    ///     添加单项验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Single() => AddValidator(new SingleValidator());

    /// <summary>
    ///     添加以特定字符/字符串开头的验证器
    /// </summary>
    /// <param name="searchValue">检索的值</param>
    /// <param name="comparison"><see cref="StringComparison" />，默认值为：<see cref="StringComparison.Ordinal" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf StartsWith(string searchValue, StringComparison comparison = StringComparison.Ordinal) =>
        AddValidator(new StartsWithValidator(searchValue) { Comparison = comparison });

    /// <summary>
    ///     添加包含特定字符/字符串的验证器
    /// </summary>
    /// <param name="searchValue">检索的值</param>
    /// <param name="comparison"><see cref="StringComparison" />，默认值为：<see cref="StringComparison.Ordinal" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf StringContains(string searchValue, StringComparison comparison = StringComparison.Ordinal) =>
        AddValidator(new StringContainsValidator(searchValue) { Comparison = comparison });

    /// <summary>
    ///     添加字符串长度验证器
    /// </summary>
    /// <param name="maximumLength">最大允许长度</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf StringLength(int maximumLength) => AddValidator(new StringLengthValidator(maximumLength));

    /// <summary>
    ///     添加字符串长度验证器
    /// </summary>
    /// <param name="minimumLength">最小允许长度</param>
    /// <param name="maximumLength">最大允许长度</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf StringLength(int minimumLength, int maximumLength) =>
        AddValidator(new StringLengthValidator(maximumLength) { MinimumLength = minimumLength });

    /// <summary>
    ///     添加不包含特定字符/字符串的验证器
    /// </summary>
    /// <param name="searchValue">检索的值</param>
    /// <param name="comparison"><see cref="StringComparison" />，默认值为：<see cref="StringComparison.Ordinal" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf
        StringNotContains(string searchValue, StringComparison comparison = StringComparison.Ordinal) =>
        AddValidator(new StringNotContainsValidator(searchValue) { Comparison = comparison });

    /// <summary>
    ///     添加强密码模式验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf StrongPassword() => AddValidator(new StrongPasswordValidator());

    /// <summary>
    ///     添加座机（电话）验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Telephone() => AddValidator(new TelephoneValidator());

    /// <summary>
    ///     添加时间格式 <see cref="System.TimeOnly" /> 时间验证器
    /// </summary>
    /// <param name="formats">允许的时间格式列表（如 "HH:mm:ss"）</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf TimeOnly(params string[] formats) => AddValidator(new TimeOnlyValidator(formats));

    /// <summary>
    ///     添加时间格式 <see cref="System.TimeOnly" /> 时间验证器
    /// </summary>
    /// <param name="formats">允许的时间格式列表（如 "HH:mm:ss"）</param>
    /// <param name="provider">格式提供器</param>
    /// <param name="style">时间解析样式，需与 <paramref name="formats" /> 搭配使用。默认值为：<see cref="DateTimeStyles.None" /></param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf TimeOnly(string[] formats, IFormatProvider? provider,
        DateTimeStyles style = DateTimeStyles.None) =>
        AddValidator(new TimeOnlyValidator(formats) { Provider = provider, Style = style });

    /// <summary>
    ///     添加统一社会信用代码验证器
    /// </summary>
    /// <param name="allowLooseMatch">是否使用宽松匹配模式，允许 15/18/20 位数字或字母。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf UnifiedSocialCreditCode(bool allowLooseMatch = false) =>
        AddValidator(new UnifiedSocialCreditCodeValidator { AllowLooseMatch = allowLooseMatch });

    /// <summary>
    ///     添加 URL 地址验证器
    /// </summary>
    /// <param name="supportsFtp">是否支持 FTP 协议。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf Url(bool supportsFtp = false) => AddValidator(new UrlValidator { SupportsFtp = supportsFtp });

    /// <summary>
    ///     添加用户名验证器
    /// </summary>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf UserName() => AddValidator(new UserNameValidator());

    /// <summary>
    ///     添加验证器代理
    /// </summary>
    /// <param name="constructorArgs"><typeparamref name="TValidator" /> 构造函数参数列表</param>
    /// <param name="configure">配置验证器实例</param>
    /// <typeparam name="TValidator">
    ///     <see cref="ValidatorBase" />
    /// </typeparam>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf ValidatorProxy<TValidator>(object?[]? constructorArgs, Action<TValidator>? configure = null)
        where TValidator : ValidatorBase
    {
        // 初始化 ValidatorProxy<TValidator> 实例
        var validatorProxy = new ValidatorProxy<TValidator>(constructorArgs);

        // 空检查
        if (configure is not null)
        {
            validatorProxy.Configure(configure);
        }

        return AddValidator(validatorProxy);
    }

    /// <summary>
    ///     添加验证特性验证器
    /// </summary>
    /// <param name="attribute">验证特性</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf WithAttribute(ValidationAttribute attribute) =>
        AddValidator(new AttributeValueValidator(attribute));

    /// <summary>
    ///     添加验证特性验证器
    /// </summary>
    /// <param name="attributes">验证特性列表</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf WithAttributes(params ValidationAttribute[] attributes) =>
        AddValidator(new AttributeValueValidator(attributes));

    /// <summary>
    ///     批量添加添加验证器
    /// </summary>
    /// <param name="validators">验证器列表</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf AddValidators(params IEnumerable<ValidatorBase> validators)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(validators);

        // 遍历集合并逐项添加
        foreach (var validator in validators)
        {
            AddValidator(validator);
        }

        return This;
    }

    /// <summary>
    ///     添加验证器
    /// </summary>
    /// <param name="validator">
    ///     <see cref="ValidatorBase" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <typeparamref name="TSelf" />
    /// </returns>
    public virtual TSelf AddValidator<TValidator>(TValidator validator, Action<TValidator>? configure = null)
        where TValidator : ValidatorBase
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(validator);

        // 检查派生类型 this 是否实现 IRuleSetContextProvider 接口
        if (this is IRuleSetContextProvider ruleSetProvider)
        {
            // 获取当前上下文中的规则集并设置
            validator.RuleSets = ruleSetProvider.GetCurrentRuleSets();
        }

        // 检查验证器是否实现 IValidatorInitializer 接口
        if (validator is IValidatorInitializer initializer)
        {
            // 同步 IServiceProvider 委托
            initializer.InitializeServiceProvider(_serviceProvider);
        }

        // 检查是否是对象验证器或其代理类
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (validator is IObjectValidator or ObjectValidatorProxy<T>)
        {
            Validators.Add(validator);

            // 记录对象验证器区域起始位置
            if (_objectValidatorStartIndex == _highPriorityEndIndex &&
                _objectValidatorStartIndex == Validators.Count - 1)
            {
                _objectValidatorStartIndex = Validators.Count - 1;
            }
        }
        // 检查是否是高优先级验证器
        else if (validator is IHighPriorityValidator highPriorityValidator)
        {
            // 只在 [0, _highPriorityEndIndex) 范围内查找插入位置（保持 Priority 升序）
            var insertIndex = _highPriorityEndIndex;
            for (var i = 0; i < _highPriorityEndIndex; i++)
            {
                // ReSharper disable once InvertIf
                if (Validators[i] is IHighPriorityValidator existing &&
                    existing.Priority > highPriorityValidator.Priority)
                {
                    insertIndex = i;
                    break;
                }
            }

            Validators.Insert(insertIndex, validator);

            // 高优先级区域扩大
            _highPriorityEndIndex++;

            // 对象验证器区域整体右移
            _objectValidatorStartIndex++;
        }
        else
        {
            Validators.Insert(_objectValidatorStartIndex, validator);

            // 对象验证器区域整体右移
            _objectValidatorStartIndex++;
        }

        // 调用自定义配置委托
        configure?.Invoke(validator);

        // 记录最新添加的验证器实例
        _lastAddedValidator = validator;

        return This;
    }

    /// <summary>
    ///     构建验证器列表
    /// </summary>
    /// <param name="configure">验证器配置委托</param>
    /// <returns>
    ///     <see cref="IReadOnlyList{T}" />
    /// </returns>
    public IReadOnlyList<ValidatorBase> Build(Action<TSelf>? configure = null)
    {
        // 调用验证器配置委托
        configure?.Invoke(This);

        return Validators;
    }

    /// <inheritdoc cref="IValidatorInitializer.InitializeServiceProvider" />
    internal virtual void InitializeServiceProvider(Func<Type, object?>? serviceProvider)
    {
        _serviceProvider = serviceProvider;

        // 遍历所有验证器并尝试同步 IServiceProvider 委托
        foreach (var validator in Validators)
        {
            // 检查验证器是否实现 IValidatorInitializer 接口
            if (validator is IValidatorInitializer initializer)
            {
                // 同步 IServiceProvider 委托
                initializer.InitializeServiceProvider(serviceProvider);
            }
        }
    }
}