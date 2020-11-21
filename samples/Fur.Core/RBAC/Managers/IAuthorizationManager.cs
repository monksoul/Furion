namespace Fur.Core
{
    /// <summary>
    /// 权限管理器
    /// </summary>
    public interface IAuthorizationManager
    {
        /// <summary>
        /// 获取用户 Id
        /// </summary>
        /// <returns></returns>
        int GetUserId();

        /// <summary>
        /// 检查授权
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        bool CheckSecurity(string resourceId);
    }
}