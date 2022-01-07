// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.ComponentModel;

namespace Furion.VirtualFileServer
{
    /// <summary>
    /// 文件提供器类型
    /// </summary>
    [Description("文件提供器类型")]
    public enum FileProviderTypes
    {
        /// <summary>
        /// 物理文件
        /// </summary>
        [Description("物理文件")]
        Physical,

        /// <summary>
        /// 嵌入资源文件
        /// </summary>
        [Description("嵌入资源文件")]
        Embedded
    }
}