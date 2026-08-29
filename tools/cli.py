#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import argparse
import os
import re
import subprocess
import sys
import shutil
from pathlib import Path
from typing import Dict, List, Optional

# -----------------------------------------------------------------------------
# ANSI 颜色定义
# -----------------------------------------------------------------------------
class Colors:
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    MAGENTA = '\033[95m'
    DARKGRAY = '\033[90m'
    DARKBLUE_BG = '\033[44m'
    WHITE = '\033[97m'
    RESET = '\033[0m'

def write_info(msg: str):
    print(f"{Colors.CYAN}[信息] {msg}{Colors.RESET}", flush=True)

def write_success(msg: str):
    print(f"{Colors.GREEN}[成功] {msg}{Colors.RESET}", flush=True)

def write_warn(msg: str):
    print(f"{Colors.YELLOW}[警告] {msg}{Colors.RESET}", flush=True)

def write_error(msg: str):
    print(f"{Colors.RED}[错误] {msg}{Colors.RESET}", flush=True)

def write_separator():
    print(f"{Colors.DARKGRAY}-----------------------------------------------------------------------------{Colors.RESET}", flush=True)

def write_step(msg: str):
    print()
    print(f"{Colors.DARKGRAY}============================================================={Colors.RESET}", flush=True)
    print(f"{Colors.DARKBLUE_BG}{Colors.WHITE}  {msg}  {Colors.RESET}", flush=True)
    print(f"{Colors.DARKGRAY}============================================================={Colors.RESET}", flush=True)
    print()

def write_command(cmd: str):
    print()
    print(f"{Colors.YELLOW}>>> 执行命令：{Colors.RESET}", flush=True)
    print(f"{Colors.CYAN}{cmd}{Colors.RESET}", flush=True)
    print()

# -----------------------------------------------------------------------------
# 参数解析
# -----------------------------------------------------------------------------
def parse_args():
    parser = argparse.ArgumentParser(description='Furion Entity Generator (Python)')
    parser.add_argument('-Tables', '--tables', nargs='*', default=[], help='需要生成的表，不填则生成所有表')
    parser.add_argument('-Context', '--context', default=None, help='数据库上下文名')
    parser.add_argument('-ConnectionName', '--connection-name', default=None, help='数据库连接字符串名')
    parser.add_argument('-OutputDir', '--output-dir', default=None, help='要保存的目录')
    parser.add_argument('-DbProvider', '--db-provider', default=None, help='数据库提供器')
    parser.add_argument('-EntryProject', '--entry-project', default=None, help='入口项目')
    parser.add_argument('-CoreProject', '--core-project', default=None, help='实体项目')
    parser.add_argument('-DbContextLocators', '--dbcontext-locators', default=None, help='数据库上下文定位器')
    parser.add_argument('-Product', '--product', default=None, help='默认前缀')
    parser.add_argument('-Namespace', '--namespace', default=None, help='命名空间')
    parser.add_argument('-UseDatabaseNames', '--use-database-names', action='store_true', help='是否数据库命名')
    return parser.parse_args()

# -----------------------------------------------------------------------------
# 系统检测
# -----------------------------------------------------------------------------
def get_system_type() -> str:
    if sys.platform.startswith('linux'):
        return 'Linux'
    elif sys.platform == 'darwin':
        return 'macOS'
    elif sys.platform in ['win32', 'cygwin']:
        return 'Windows'
    else:
        return 'Unknown'

# -----------------------------------------------------------------------------
# 文件夹选择
# -----------------------------------------------------------------------------
def select_folder(system_type: str, initial_dir: str) -> Optional[str]:
    if system_type == 'Windows':
        ps_script = f"""
        Add-Type -AssemblyName System.Windows.Forms
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        $folderBrowser.Description = "选择目录"
        $folderBrowser.SelectedPath = '{initial_dir}'
        if ($folderBrowser.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {{
            $folderBrowser.SelectedPath
        }}
        """
        try:
            result = subprocess.run(['powershell', '-NoProfile', '-Command', ps_script],
                                    capture_output=True, text=True, timeout=30)
            if result.returncode == 0 and result.stdout.strip():
                return result.stdout.strip()
        except Exception:
            pass
        return None
    elif system_type == 'macOS':
        script = '''
        tell application "Finder"
            activate
            try
                set selectedFolder to choose folder with prompt "Please select a folder:"
                set folderPath to POSIX path of selectedFolder
            on error
                set folderPath to ""
            end try
            return folderPath
        end tell
        '''
        try:
            result = subprocess.run(['osascript', '-e', script],
                                    capture_output=True, text=True, timeout=30)
            if result.returncode == 0:
                path = result.stdout.strip()
                return path if path else None
        except Exception:
            pass
        return None
    elif system_type == 'Linux':
        try:
            result = subprocess.run(['zenity', '--file-selection', '--directory'],
                                    capture_output=True, text=True, timeout=30)
            if result.returncode == 0:
                return result.stdout.strip()
        except Exception:
            pass
        return None
    else:
        return None

# -----------------------------------------------------------------------------
# 正则与代码处理
# -----------------------------------------------------------------------------
def extract_table_has_comment(code: str) -> Optional[str]:
    pattern = r'\.ToTable\([^;]*?tb\.HasComment\("([^"]*)"\)'
    m = re.search(pattern, code)
    if m:
        return m.group(1)
    pattern = r'HasComment\("([^"]*)"\)'
    m = re.search(pattern, code)
    return m.group(1) if m else None

def parse_comments_from_code(code: str) -> Dict[str, Optional[str]]:
    comments = {}
    if not code or not code.strip():
        return comments
    lines = code.splitlines()
    current_prop_block = None
    current_prop_name = None
    for line in lines:
        m = re.search(r'entityBuilder\.Property\(e\s*=>\s*e\.(?P<name>\w+)\)', line)
        if m:
            current_prop_block = line
            current_prop_name = m.group('name')
        elif current_prop_block is not None and ';' in line:
            current_prop_block += line
            if current_prop_name:
                m_comment = re.search(r'\.HasComment\("(?P<comment>[^"]*)"\)', current_prop_block)
                comments[current_prop_name] = m_comment.group('comment') if m_comment else None
            current_prop_block = None
            current_prop_name = None
        elif current_prop_block is not None:
            current_prop_block += line
    if current_prop_block is not None and current_prop_name:
        m_comment = re.search(r'\.HasComment\("(?P<comment>[^"]*)"\)', current_prop_block)
        comments[current_prop_name] = m_comment.group('comment') if m_comment else None
    return comments

def add_xml_comments_to_properties(content: str, comments: Dict[str, Optional[str]]) -> str:
    lines = content.splitlines()
    result = []
    for line in lines:
        m = re.search(r'^\s*public\s+(?:virtual\s+)?[\w<>,\.\s]+\s+(?P<name>\w+)\s*\{', line)
        if m:
            prop_name = m.group('name')
            indent = re.match(r'^(\s*)', line).group(1)
            has_comment = False
            j = len(result) - 1
            while j >= 0 and result[j].strip() and result[j].strip().startswith('///'):
                has_comment = True
                j -= 1
            if prop_name in comments and comments[prop_name] and not has_comment:
                comment = comments[prop_name].replace('\r', '').replace('\n', ' ')
                result.append(f"{indent}/// <summary>")
                result.append(f"{indent}/// {comment}")
                result.append(f"{indent}/// </summary>")
        result.append(line)
    return '\n'.join(result)

def normalize_indent(code: str, indent: str) -> str:
    if not code or not code.strip():
        return ""
    lines = code.splitlines()
    min_indent = None
    for line in lines:
        if line.strip():
            leading = len(line) - len(line.lstrip())
            if min_indent is None or leading < min_indent:
                min_indent = leading
    if min_indent is None:
        min_indent = 0
    normalized = []
    for line in lines:
        if line.strip():
            content = line[min_indent:] if min_indent > 0 else line
            normalized.append(indent + content)
        else:
            normalized.append("")
    while normalized and not normalized[0].strip():
        normalized.pop(0)
    while normalized and not normalized[-1].strip():
        normalized.pop()
    return '\n'.join(normalized)

def extract_entity_configs(dbcontext_content: str) -> Dict[str, str]:
    """手动提取 modelBuilder.Entity<Table>(entity => { ... }) 代码块"""
    configs = {}
    pattern = re.compile(r'modelBuilder\.Entity<(\w+)>\(\s*\w+\s*=>\s*\{')
    for m in pattern.finditer(dbcontext_content):
        table_name = m.group(1)
        start = m.end()
        brace_count = 1
        pos = start
        while pos < len(dbcontext_content) and brace_count > 0:
            if dbcontext_content[pos] == '{':
                brace_count += 1
            elif dbcontext_content[pos] == '}':
                brace_count -= 1
            pos += 1
        content = dbcontext_content[start:pos-1]
        content = content.replace('entity.', 'entityBuilder.')
        configs[table_name] = content
    return configs

# -----------------------------------------------------------------------------
# 主函数
# -----------------------------------------------------------------------------
def main():
    args = parse_args()

    fur_tools = "Furion Tools v4.9.9.88"
    copyright_text = r"""
// -----------------------------------------------------------------------------
//  ______          _               _______          _
// |  ____|        (_)             |__   __|        | |
// | |__ _   _ _ __ _  ___  _ __      | | ___   ___ | |___
// |  __| | | | '__| |/ _ \| '_ \     | |/ _ \ / _ \| / __|
// | |  | |_| | |  | | (_) | | | |    | | (_) | (_) | \__ \
// |_|   \__,_|_|  |_|\___/|_| |_|    |_|\___/ \___/|_|___/
//
// -----------------------------------------------------------------------------
"""
    print(f"{Colors.MAGENTA}{copyright_text}{Colors.RESET}", flush=True)

    root_path = os.getcwd()
    system_type = get_system_type()

    # 初始化默认值
    product = args.product or "Furion"
    entry_project = args.entry_project or f"{product}.Web.Entry"
    core_project = args.core_project or f"{product}.Core"
    db_provider = args.db_provider or "Microsoft.EntityFrameworkCore.SqlServer"
    context = args.context or f"{product}DbContext"
    connection_name = args.connection_name or "NonConfigureConnectionString"
    dbcontext_locators = args.dbcontext_locators or "MasterDbContextLocator"
    output_dir = args.output_dir or os.path.join(root_path, core_project, "Entities")
    namespace = args.namespace or core_project

    write_info(f"正在启动 {fur_tools} ...")
    write_success(f"{fur_tools} 启动成功！")

    # 临时目录
    temp_output_dir = os.path.join(root_path, core_project, "TempEntities")
    if os.path.exists(temp_output_dir):
        shutil.rmtree(temp_output_dir)
    os.makedirs(temp_output_dir)

    write_separator()
    result = subprocess.run(['dotnet', 'ef', '--version'], capture_output=True, text=True, encoding='utf-8', errors='ignore')
    if result.returncode != 0:
        write_error("dotnet-ef 未安装")
        write_warn("安装命令: dotnet tool install --global dotnet-ef")
    else:
        write_success("dotnet-ef 已安装")
        print(result.stdout.strip(), flush=True)
    write_separator()

    # 操作模式提示
    gui_available = (system_type == "Windows" and "sqlserver" in db_provider.lower())
    if gui_available:
        write_warn("请选择操作模式：")
        write_warn("  [G] 界面操作（仅适用于 Windows + SQL Server）")
        write_warn("  [任意其他字符] 命令行操作（支持所有环境）")
    else:
        write_warn("请选择操作模式：")
        write_warn("  [任意字符] 命令行操作（当前环境不支持 GUI 模式）")
    option = input(f"{fur_tools} 您的输入是: ").strip()

    if option == "G" and gui_available:
        write_info("GUI 模式启动（简化版），请选择保存目录...")
        selected = select_folder(system_type, os.path.join(root_path, core_project))
        if not selected:
            write_warn("用户取消操作，程序终止！")
            shutil.rmtree(temp_output_dir, ignore_errors=True)
            return
        output_dir = selected
        write_warn("注意：Python 版本 GUI 模式仅支持选择目录，其他参数请通过命令行传入。")
    else:
        selected = select_folder(system_type, os.path.join(root_path, core_project))
        if not selected:
            write_warn("用户取消操作，程序终止！")
            shutil.rmtree(temp_output_dir, ignore_errors=True)
            return
        output_dir = selected

    if connection_name == "NonConfigureConnectionString":
        write_warn("未找到连接字符串，程序终止！")
        shutil.rmtree(temp_output_dir, ignore_errors=True)
        return

    # 组装 ef 命令
    tables = args.tables or []
    table_list = []
    for t in tables:
        table_list.extend([x.strip() for x in t.split(',') if x.strip()])
    table_list = list(dict.fromkeys(table_list))

    cmd_parts = [
        "dotnet", "ef", "dbcontext", "scaffold",
        f"Name=ConnectionStrings:{connection_name}",
        db_provider,
        "--project", entry_project,
        "--output-dir", temp_output_dir,
        "--context", context,
        "--namespace", namespace,
        "--no-onconfiguring",
        "--no-pluralize",
        "--verbose",
    ]
    if table_list:
        for tbl in table_list:
            cmd_parts.extend(["--table", tbl])
    if args.use_database_names:
        cmd_parts.append("--use-database-names")
    cmd_parts.append("--force")
    command_str = " ".join(cmd_parts)

    write_step("编译解决方案代码")
    write_info("正在编译解决方案代码......")
    write_command(command_str)
    result = subprocess.run(command_str, shell=True, capture_output=True, text=True, encoding='utf-8', errors='ignore')
    if result.returncode != 0:
        write_error(f"dotnet ef 命令执行失败（退出码: {result.returncode}）")
        write_error(result.stderr)
        shutil.rmtree(temp_output_dir, ignore_errors=True)
        return

    write_success("编译成功！")
    write_step("开始生成实体文件")
    write_info("开始生成实体文件......")

    os.makedirs(output_dir, exist_ok=True)

    try:
        context_file = os.path.join(temp_output_dir, f"{context}.cs")
        with open(context_file, 'r', encoding='utf-8') as f:
            dbcontext_content = f.read()

        entity_configs = extract_entity_configs(dbcontext_content)

        # 文件头模板
        file_header = f"""// -----------------------------------------------------------------------------
// Generate By {fur_tools}
// -----------------------------------------------------------------------------

using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using {core_project};"""

        # 实体配置模板
        entity_configure_template = """
    public void Configure(EntityTypeBuilder<#Table#> entityBuilder, DbContext dbContext, Type dbContextLocator)
    {
#Code#
    }"""

        prop_regex = r'(?:namespace\s+[\w\.]+\s*;\s*)?(?:public\s+partial\s+class\s+(?P<table>\w+)\s*\{)(?P<content>[\s\S]*?)\n\}'

        for file_path in Path(temp_output_dir).glob('*.cs'):
            file_name = file_path.stem
            if file_name == context:
                continue

            write_info(f"正在生成 {file_name}.cs 实体代码......")
            with open(file_path, 'r', encoding='utf-8') as f:
                entity_content = f.read()

            props_match = re.search(prop_regex, entity_content)
            if props_match:
                props_content = props_match.group('content')
            else:
                write_warn(f"无法解析实体 {file_name} 的属性，跳过注释添加。")
                props_content = ""

            config_code = entity_configs.get(file_name, "")
            comments = parse_comments_from_code(config_code)
            modified_props = add_xml_comments_to_properties(props_content, comments)
            # 去除开头可能的多余换行
            modified_props = modified_props.lstrip('\n')
            # 去除结尾可能的多余换行
            modified_props = modified_props.rstrip('\n')

            extents = f" : IEntity<{dbcontext_locators}>"
            new_props_content = modified_props
            if file_name in entity_configs:
                extents += f", IEntityTypeBuilder<{file_name}, {dbcontext_locators}>"
                normalized_config = normalize_indent(config_code, "        ")
                config_block = entity_configure_template.replace("#Table#", file_name).replace("#Code#", normalized_config)
                # 在属性和方法之间插入一个空行
                new_props_content = modified_props + "\n" + config_block

            table_desc = ""
            if file_name in entity_configs:
                table_comment = extract_table_has_comment(config_code)
                if table_comment:
                    table_desc = f"/// <summary>\n/// {table_comment}\n/// </summary>"

            # 构建最终文件内容
            final_content = file_header + "\n\n"
            final_content += f"namespace {namespace};\n"
            if table_desc:
                final_content += "\n" + table_desc + "\n"
            else:
                final_content += "\n"
            final_content += f"public partial class {file_name}{extents}\n{{\n{new_props_content}\n}}\n"

            # 移除末尾所有换行符，确保文件末尾无多余空行
            final_content = final_content.rstrip('\n')

            final_path = os.path.join(output_dir, f"{file_name}.cs")
            with open(final_path, 'w', encoding='utf-8') as f:
                f.write(final_content)
            write_success(f"成功生成 {file_name}.cs 实体代码")

        os.remove(context_file)
    except Exception as e:
        write_error(f"生成实体文件时发生错误：{e}")
        raise
    finally:
        shutil.rmtree(temp_output_dir, ignore_errors=True)

    write_step("全部实体生成成功")
    write_success("全部实体生成成功！")

if __name__ == "__main__":
    main()