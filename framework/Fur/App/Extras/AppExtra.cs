using Fur.DependencyInjection;

namespace Fur
{
    /// <summary>
    /// 官方包定义
    /// </summary>
    [SkipScan]
    internal static class AppExtra
    {
        /// <summary>
        /// Jwt 验证包
        /// </summary>
        internal const string AUTHENTICATION_JWTBEARER = "Fur.Extras.Authentication.JwtBearer";

        /// <summary>
        /// Mapster 映射包
        /// </summary>
        internal const string OBJECTMAPPER_MAPSTER = "Fur.Extras.ObjectMapper.Mapster";
    }
}