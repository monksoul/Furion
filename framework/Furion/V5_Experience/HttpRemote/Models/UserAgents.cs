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

namespace Furion.HttpRemote;

/// <summary>
///     主流浏览器 <c>User-Agent</c> 字符串常量静态类
/// </summary>
public static class UserAgents
{
    /// <summary>
    ///     随机获取一个浏览器的 <c>User-Agent</c>
    /// </summary>
    /// <remarks>支持桌面端或移动端切换。</remarks>
    /// <param name="isMobile">是否为移动端，默认值为：<c>false</c>（即桌面端）</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string GetRandom(bool isMobile = false)
    {
        // 初始化桌面端和移动端 User-Agent 列表
        var pcPool = new[] { Chrome.PC, Firefox.PC, Safari.PC, Edge.PC, Opera.PC, Generic.PC };
        var mobilePool = new[]
        {
            Chrome.Mobile, Firefox.Mobile, Safari.Mobile, Edge.Mobile, Opera.Mobile, Generic.Mobile
        };

        var pool = !isMobile ? pcPool : mobilePool;

        // 返回随机浏览器的 User-Agent
        return pool[Random.Shared.Next(pool.Length)];
    }

    /// <summary>
    ///     获取指定浏览器类型的 <c>User-Agent</c>
    /// </summary>
    /// <remarks>支持桌面端或移动端切换。</remarks>
    /// <param name="browser">浏览器枚举（Chrome/Firefox/Safari/Edge/Opera/Generic）</param>
    /// <param name="isMobile">是否为移动端，默认值为：<c>false</c>（即桌面端）</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string GetByBrowser(string? browser, bool isMobile = false) =>
        browser?.ToUpperInvariant() switch
        {
            "CHROME" => isMobile ? Chrome.Mobile : Chrome.PC,
            "FIREFOX" => isMobile ? Firefox.Mobile : Firefox.PC,
            "SAFARI" => isMobile ? Safari.Mobile : Safari.PC,
            "EDGE" => isMobile ? Edge.Mobile : Edge.PC,
            "OPERA" => isMobile ? Opera.Mobile : Opera.PC,
            _ => isMobile ? Generic.Mobile : Generic.PC
        };

    /// <summary>
    ///     Google Chrome
    /// </summary>
    public static class Chrome
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile =
            "Mozilla/5.0 (Linux; Android 16; Pixel 9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Mobile Safari/537.36";
    }

    /// <summary>
    ///     Mozilla Firefox
    /// </summary>
    public static class Firefox
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:153.0) Gecko/20100101 Firefox/153.0";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile = "Mozilla/5.0 (Android 16; Mobile; rv:153.0) Gecko/153.0 Firefox/153.0";
    }

    /// <summary>
    ///     Apple Safari
    /// </summary>
    public static class Safari
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.5 Safari/605.1.15";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile =
            "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.5 Mobile/15E148 Safari/604.1";
    }

    /// <summary>
    ///     Microsoft Edge
    /// </summary>
    public static class Edge
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36 Edg/150.0.0.0";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile =
            "Mozilla/5.0 (Linux; Android 16; SM-S928B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Mobile Safari/537.36 EdgA/150.0.0.0";
    }

    /// <summary>
    ///     Opera
    /// </summary>
    public static class Opera
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36 OPR/133.0.0.0";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile =
            "Mozilla/5.0 (Linux; Android 16; Pixel 9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Mobile Safari/537.36 OPR/100.0.0.0";
    }

    /// <summary>
    ///     通用
    /// </summary>
    public static class Generic
    {
        /// <summary>
        ///     桌面端
        /// </summary>
        public const string PC =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36";

        /// <summary>
        ///     移动端
        /// </summary>
        public const string Mobile =
            "Mozilla/5.0 (Linux; Android 16; Mobile) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Mobile Safari/537.36";
    }
}