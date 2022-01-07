// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Threading.Tasks;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎模板模型接口
    /// </summary>
    public interface IViewEngineModel
    {
        /// <summary>
        /// 模型
        /// </summary>
        dynamic Model { get; set; }

        /// <summary>
        /// 写入字面量
        /// </summary>
        /// <param name="literal"></param>
        void WriteLiteral(string literal = null);

        /// <summary>
        /// 写入字面量
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        Task WriteLiteralAsync(string literal = null);

        /// <summary>
        /// 写入对象
        /// </summary>
        /// <param name="obj"></param>
        void Write(object obj = null);

        /// <summary>
        /// 写入对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task WriteAsync(object obj = null);

        /// <summary>
        /// 开始写入特性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="suffix"></param>
        /// <param name="suffixOffset"></param>
        /// <param name="attributeValuesCount"></param>
        void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);

        /// <summary>
        /// 开始写入特性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="suffix"></param>
        /// <param name="suffixOffset"></param>
        /// <param name="attributeValuesCount"></param>
        /// <returns></returns>
        Task BeginWriteAttributeAsync(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);

        /// <summary>
        /// 写入特性值
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="value"></param>
        /// <param name="valueOffset"></param>
        /// <param name="valueLength"></param>
        /// <param name="isLiteral"></param>
        void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral);

        /// <summary>
        /// 写入特性值
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="value"></param>
        /// <param name="valueOffset"></param>
        /// <param name="valueLength"></param>
        /// <param name="isLiteral"></param>
        /// <returns></returns>
        Task WriteAttributeValueAsync(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral);

        /// <summary>
        /// 结束写入特性
        /// </summary>
        void EndWriteAttribute();

        /// <summary>
        /// 结束写入特性
        /// </summary>
        /// <returns></returns>
        Task EndWriteAttributeAsync();

        /// <summary>
        /// 执行
        /// </summary>
        void Execute();

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        string Result();

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        Task<string> ResultAsync();
    }
}