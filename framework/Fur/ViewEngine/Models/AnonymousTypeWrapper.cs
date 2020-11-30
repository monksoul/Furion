using Fur.DependencyInjection;
using Fur.Extensions;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 匿名类型包装器
    /// </summary>
    [SkipScan]
    public class AnonymousTypeWrapper : DynamicObject
    {
        /// <summary>
        /// 匿名模型
        /// </summary>
        private readonly object model;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public AnonymousTypeWrapper(object model)
        {
            this.model = model;
        }

        /// <summary>
        /// 获取成员信息
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyInfo = model.GetType().GetProperty(binder.Name);

            if (propertyInfo == null)
            {
                result = null;
                return false;
            }

            result = propertyInfo.GetValue(model, null);

            if (result == null)
            {
                return true;
            }

            var type = result.GetType();

            if (result.IsAnonymous())
            {
                result = new AnonymousTypeWrapper(result);
            }

            if (type.IsArray)
            {
                result = ((IEnumerable<object>)result).Select(e => new AnonymousTypeWrapper(e)).ToList();
            }

            return true;
        }
    }
}