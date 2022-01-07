// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.IO;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    internal static class Penetrates
    {
        /// <summary>
        /// 获取模板文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string GetTemplateFileName(string fileName)
        {
            var templateSaveDir = Path.Combine(AppContext.BaseDirectory, "templates");
            if (!Directory.Exists(templateSaveDir)) Directory.CreateDirectory(templateSaveDir);

            if (!fileName.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase)) fileName += ".dll";
            var templatePath = Path.Combine(templateSaveDir, "~" + fileName);

            return templatePath;
        }
    }
}