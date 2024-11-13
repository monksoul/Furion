# 定义参数
Param(
    # 需要生成的表，不填则生成所有表
    [string[]] $Tables,
    # 数据库上下文名
    [string]$Context,
    # 数据库连接字符串名
    [string]$ConnectionName,
    # 要保存的目录
    [string]$OutputDir,
    # 数据库提供器
    [string]$DbProvider,
    # 入口项目
    [string]$EntryProject,
    # 实体项目
    [string]$CoreProject,
    # 数据库上下文定位器
    [string] $DbContextLocators,
    # 默认前缀
    [string]$Product,
    # 命名空间
    [string]$Namespace
    # 是否数据库命名
    #[string]$UseDatabaseNames
)

# 匹配数据库表代码注释
function ExtractTableHasComment($inputString) {
    $pattern = 'HasComment\("([^"]*)"\)'

    if ($inputString -match $pattern) {
        $matches = $Matches[1]
        if ($matches) {
            return $matches;
        }
    } else {
        return $null;
    }
}

# 匹配数据库列代码注释
function ParseCommentsFromCode($code) {
    $commentsDictionary = @{}

    $lines = $code.Split([Environment]::NewLine)

    $currentPropertyBlock = ''

    foreach ($line in $lines) {
        if ($line -match '(?s)entityBuilder\.Property\(e => e\.(?<propertyName>\w+)\)') {
            $currentPropertyBlock = $line
            $currentPropertyName = $Matches['propertyName']
        }
        elseif ($currentPropertyBlock -ne '' -and $line -match ';') {
            $currentPropertyBlock += $line
            $propertyBlock = [System.Text.RegularExpressions.Regex]::Escape($currentPropertyBlock)
            if ($currentPropertyBlock -match '(?s)\.HasComment\("(?<comment>[^"]*)"\)') {
                $commentsDictionary[$currentPropertyName] = $Matches['comment']
            } else {
                $commentsDictionary[$currentPropertyName] = $null
            }
            $currentPropertyBlock = ''
        }
        elseif ($currentPropertyBlock -ne '') {
            $currentPropertyBlock += $line
        }
    }

    return $commentsDictionary
}

# 生成属性注释
function AddXmlCommentsToProperties($content, $commentsDictionary) {
    $lines = $content.Split([Environment]::NewLine)

    $modifiedLines = @()

    foreach ($line in $lines) {
        if ($line.Trim() -match '^\s*public\s+(\S+)\s+(\w+)\s+\{.*') {
            $propertyName = $Matches[2]
            $propertyType = $Matches[1]

            if ($commentsDictionary.ContainsKey($propertyName)) {
                $modifiedLines += @"
/// <summary>
/// {$($commentsDictionary[$propertyName])}
/// </summary>
"@
            }
        }
        $modifiedLines += $line
    }

    $modifiedContent = $modifiedLines -join [Environment]::NewLine

    return $modifiedContent
}

$FurTools = "Furion Tools v4.9.5.21";

# 输出信息
$copyright = @"
// -----------------------------------------------------------------------------
//  ______          _               _______          _
// |  ____|        (_)             |__   __|        | |
// | |__ _   _ _ __ _  ___  _ __      | | ___   ___ | |___
// |  __| | | | '__| |/ _ \| '_ \     | |/ _ \ / _ \| / __|
// | |  | |_| | |  | | (_) | | | |    | | (_) | (_) | \__ \
// |_|   \__,_|_|  |_|\___/|_| |_|    |_|\___/ \___/|_|___/
//
// -----------------------------------------------------------------------------
"@;

# 获取当前目录
$pwd = pwd;
$rootPath = $pwd.Path;

# 获取当前操作系统
function GetSystemType {  
    # 检查PowerShell版本和特性  
    if ($PSVersionTable.PSEdition -eq "Core") {  
        # PowerShell Core（现在称为PowerShell 7+），支持跨平台  
        $runtimeOS = [Runtime.InteropServices.RuntimeInformation]::OSDescription  
          
        if ($runtimeOS.Contains("Linux")) {  
            return "Linux"  
        } elseif ($runtimeOS.Contains("Microsoft Windows")) {  
            return "Windows"  
        } elseif ($runtimeOS.Contains("macOS") -or $runtimeOS.Contains("Darwin")) {  
            return "macOS"  
        } else {  
            return "Unknown OS (PowerShell Core)"  
        }  
    } else {  
        # PowerShell Desktop 版本，主要运行在 Windows 上  
        return "Windows"  
    }  
}

$runtimeOS = GetSystemType;

# 初始化默认值
if ($Product -eq $null -or $Product -eq ""){
    $Product = "Furion";
}

if ($EntryProject -eq $null -or $EntryProject -eq ""){
    $EntryProject = "$Product.Web.Entry";
}

if ($CoreProject -eq $null -or $CoreProject -eq ""){
    $CoreProject = "$Product.Core";
}

if ($DbProvider -eq $null -or $DbProvider -eq ""){
    $DbProvider = "Microsoft.EntityFrameworkCore.SqlServer";
}

if ($Context -eq $null -or $Context -eq ""){
    $Context = $Product + "DbContext";
}

if ($ConnectionName -eq $null -or $ConnectionName -eq ""){
    $ConnectionName = "NonConfigureConnectionString";
}

if ($DbContextLocators -eq $null -or $DbContextLocators -eq ""){
    $DbContextLocators = "MasterDbContextLocator";
}

if ($OutputDir -eq $null -or $OutputDir -eq ""){
    $OutputDir = "$rootPath/$CoreProject/Entities";
}

if ($Namespace -eq $null -or $Namespace -eq ""){
    $Namespace = $CoreProject;
}

# 判断是否需要使用数据库命名
$UseDatabaseNames = $false;
if($args.Contains("-UseDatabaseNames")){
    $UseDatabaseNames = $true;
}

# 输出工具版权声明
$copyright;

Write-Output "$FurTools 启动中......";
Write-Output "$FurTools 启动成功！";

# 定义临时目录
$TempOutputDir = "$rootPath/$CoreProject/TempEntities";

# 临时目录不存在则创建
if (-not (Test-Path -Path $TempOutputDir)) {  
    New-Item -ItemType Directory -Path $TempOutputDir;
}

# 如果 dotnet ef dbcontext scaffold 命令不存在则提示安装

Write-Output "-----------------------------------------------------------------------------";
Write-Warning "请确保 dotnet tool install --global dotnet-ef --version 版本号 已经执行安装操作（没有则执行）";
Write-Output "-----------------------------------------------------------------------------";

Write-Warning "$FurTools 请键入操作类型：[G] 界面操作，[任意字符] 命令行操作";
$options = Read-Host "$FurTools 您的输入是";

# 选择 GUI 操作
if($options -eq "G")
{
    # -----------------------------------------------------------------------------
    # 构建 Winform GUI 客户端 [开始]
    # -----------------------------------------------------------------------------

    # 加载数据库表
    function loadDbTable(){
        # 获取选中的数据库连接字符串
        $connStr = $comboBox.SelectedItem;
        if ($connStr -eq $null -or $connStr -eq ""){
            [System.Windows.Forms.MessageBox]::Show("请选择数据库连接字符串后再操作");
            return;
        }

        # 打开数据库读取所有数据库表和视图

        # 创建一个数据库连接对象
        $conn = New-Object System.Data.SqlClient.SQLConnection;

        # 设置连接字符串
        $conn.ConnectionString = $connStr;

        # 打开数据库连接
        $conn.Open();

        # 创建 CMD执行命令
        $cmd = New-Object System.Data.SqlClient.SqlCommand("SELECT i.name+'.'+h.name FROM sys.objects h left join sys.schemas i on h.schema_id=i.SCHEMA_ID WHERE h.type IN('U','V') ORDER BY h.type,i.name,h.name", $conn);

        # 创建一个dataset
        $ds = New-Object System.Data.DataSet;

        # 创建一个适配器
        $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd);

        # 填充数据
        [void]$da.fill($ds)

        # 关闭数据库连接
        $conn.Close();

        # 销毁数据库链接
        $conn.Dispose();

        $rowCount = $ds.Tables[0].Rows.Count;

        # 填充 Listbox
        for($i = 0;$i -le $rowCount; $i++)
        {
            $rows = $ds.Tables[0].Rows[$i];
            if($rows -ne $null)
            {
                [void] $listBox.Items.Add($rows[0]);
            }
        }
    }

    # 加载连接设置
    function loadConnectionSettings($settingsPath){
        # 读取 Web 入口的 appsetting.json 配置的链接字符串
        # -----------------------------------------------------------------------------
        # [开始]
        # appsetting.json 内容
        $appsetting = Get-Content $settingsPath -raw;

        # 获取 appsetting.json 定义的节点
        $connectionDefine = [regex]::Matches($appsetting, '"ConnectionStrings"\s*.\s+\{(?<define>[\s\S]*?)\}');
        if($connectionDefine.Count -eq 0)
        {
            # Write-Warning "$FurTools 未找到 $settingsPath 中定义的数据库连接字符串！";
            # Write-Warning "$FurTools 程序终止！";
            return;
        }

        # 获取连接字符串所有定义
        $connectionDefineContent = $connectionDefine[0].Groups.Value[1];

        # 解析出每一个链接字符串
        $connections = [regex]::Matches($connectionDefineContent, '"(.*?)"\s*.\s*"(?<connectionStr>.*?)"');

        # 生成下拉
        for ($i = 0; $i -le $connections.Count - 1; $i++){
           $key = $connections[$i].Groups.Value[1];
           $value = $connections[$i].Groups.Value[2];
           if($connDic.ContainsKey($value) -eq $false){
               $newValue = $value.Replace("\\","\");
               $result = $comboBox.Items.Add($newValue);
               $connDic.Add($newValue,$key);
           }
        }
        # [结束]
        # -----------------------------------------------------------------------------
    }

    # 添加 Winform 应用程序
    Add-Type -AssemblyName System.Windows.Forms;
    Add-Type -AssemblyName System.Drawing;

    # 创建一个 Winform 窗口
    $mainForm = New-Object System.Windows.Forms.Form;
    $mainForm.Text = $FurTools;
    $mainForm.Size = New-Object System.Drawing.Size(800,600);
    $mainForm.StartPosition = "CenterScreen";

    # 创建组面板
    $baseSetting = New-Object System.Windows.Forms.GroupBox;
    $baseSetting.SuspendLayout();
    $baseSetting.Location = New-Object System.Drawing.Point(15, 15);
    $baseSetting.Size = New-Object System.Drawing.Size(760, 120);
    $baseSetting.Text = "基础设置";
    $baseSetting.TabIndex = 10;
    $baseSetting.TabStop = $false;
    $baseSetting.ResumeLayout($false);
    $baseSetting.PerformLayout();
    $mainForm.Controls.Add($baseSetting);

    # 构建数据库连接字符串提示
    $label = New-Object System.Windows.Forms.Label;
    $label.Location = New-Object System.Drawing.Point(15,35);
    $label.AutoSize = $true;
    $label.Size = New-Object System.Drawing.Size(280,20);
    $label.Text = '选择数据库连接字符串：';
    $label.TabIndex = 9;
    $baseSetting.Controls.Add($label);

    # 构建多数据库上下定位器文字符提示
    $locatorLabel = New-Object System.Windows.Forms.Label;
    $locatorLabel.Location = New-Object System.Drawing.Point(15,80);
    $locatorLabel.AutoSize = $true;
    $locatorLabel.Size = New-Object System.Drawing.Size(280,20);
    $locatorLabel.Text = '多数据库上下文定位器：';
    $locatorLabel.TabIndex = 9;
    $baseSetting.Controls.Add($locatorLabel);

    # 数据库上下文定位器文本框
    $locatorTextBox = New-Object System.Windows.Forms.TextBox;
    $locatorTextBox.Location = New-Object System.Drawing.Point(200,75);
    $locatorTextBox.Size = New-Object System.Drawing.Size(370,20);
    $locatorTextBox.TabIndex = 9;
    $locatorTextBox.Text = $DbContextLocators;
    $baseSetting.Controls.Add($locatorTextBox);

    # 连接字典
    $connDic = New-Object -TypeName 'System.Collections.Generic.Dictionary[System.String, System.String]';

    # 构建数据库连接字符串下拉
    $comboBox = New-Object System.Windows.Forms.ComboBox;
    $comboBox.Location = New-Object System.Drawing.Point(200,30);
    $comboBox.Size = New-Object System.Drawing.Size(370,20);
    $comboBox.TabIndex = 9;
    $comboBox.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList;
    # 绑定按钮事件
    $comboBoxClickEventHandler = [System.EventHandler] {
        $connStr = $comboBox.SelectedItem;
        if ($connStr -eq $null -or $connStr -eq ""){
            $btnGenerate.Enabled =$false;
        }
        else{
            $btnGenerate.Enabled =$true;
            $ConnectionName = $connDic[$connStr];
        }
    }
    $comboBox.Add_SelectedIndexChanged($comboBoxClickEventHandler);
    $baseSetting.Controls.Add($comboBox);

    # 读取 所有配置文件
    # -----------------------------------------------------------------------------
    # [开始]
    $jsons = Get-ChildItem $rootPath -Include "*.json" -Recurse;
    for ($i = 0; $i -le $jsons.Count - 1; $i++){
        $json = $jsons[$i];
        if(!($json.DirectoryName.Contains("bin") -or $json.DirectoryName.Contains("obj") -or $json.DirectoryName.Contains(".vscode") -or $json.FullName.Contains(".deps.json"))){
          loadConnectionSettings($json.FullName);
        }
    }
    # [结束]
    # -----------------------------------------------------------------------------

    # 构建加载数据库表按钮
    $btnLoad = New-Object System.Windows.Forms.Button;
    $btnLoad.Location = New-Object System.Drawing.Point(595,30);
    $btnLoad.Size = New-Object System.Drawing.Size(150, 25);
    $btnLoad.TabIndex = 9;
    $btnLoad.Text = "加载数据库表和视图";
    # 绑定按钮事件
    $btnLoadClickEventHandler = [System.EventHandler] {
        # 保存数据库上下文定位器
        $DbContextLocators = $locatorTextBox.Text;

        try{
            Write-Warning "$FurTools 正在加载数据库表和视图......"
            loadDbTable;
            Write-Warning "$FurTools 加载成功！"
        }
        catch{
            Write-Warning "$FurTools 加载数据库表和视图出错，请重试！";
        }
    }
    $btnLoad.Add_Click($btnLoadClickEventHandler);
    $baseSetting.Controls.Add($btnLoad);

    # 创建表和视图面板
    $tableSetting = New-Object System.Windows.Forms.GroupBox;
    $tableSetting.SuspendLayout();
    $tableSetting.Location = New-Object System.Drawing.Point(15, 155);
    $tableSetting.Size = New-Object System.Drawing.Size(760, 345);
    $tableSetting.Text = "数据库表和视图";
    $tableSetting.TabIndex = 10;
    $tableSetting.TabStop = $false;
    $tableSetting.ResumeLayout($false);
    $tableSetting.PerformLayout();
    $mainForm.Controls.Add($tableSetting);

    # 创建表和视图容器
    $listBox = New-Object System.Windows.Forms.Listbox;
    $listBox.BackColor = [System.Drawing.SystemColors]::Window;
    $listBox.FormattingEnabled = $true;
    $listBox.ItemHeight = 20;
    $listBox.TabIndex = 9;
    $listBox.Location = New-Object System.Drawing.Point(15,35);
    $listBox.Size = New-Object System.Drawing.Size(730,295);
    $listBox.SelectionMode = "MultiExtended";
    $tableSetting.Controls.Add($listBox);

    # 创建立即生成按钮和取消生成按钮
    $btnGenerate = New-Object System.Windows.Forms.Button;
    $btnGenerate.Location = New-Object System.Drawing.Point(530,520);
    $btnGenerate.Size = New-Object System.Drawing.Size(100, 25);
    $btnGenerate.TabIndex = 8;
    $btnGenerate.Text = "立即生成";
    $btnGenerate.Enabled =$false;
    $btnGenerate.BackColor = [System.Drawing.SystemColors]::ControlLight
    $btnGenerate.DialogResult = [System.Windows.Forms.DialogResult]::OK;
    $mainForm.AcceptButton = $btnGenerate;
    $mainForm.Controls.Add($btnGenerate);

    $btnCancel = New-Object System.Windows.Forms.Button;
    $btnCancel.Location = New-Object System.Drawing.Point(650,520);
    $btnCancel.Size = New-Object System.Drawing.Size(100, 25);
    $btnCancel.TabIndex = 8;
    $btnCancel.Text = "取消生成";
    $btnCancel.DialogResult = [System.Windows.Forms.DialogResult]::Cancel;
    $mainForm.CancelButton = $btnCancel;
    $mainForm.Controls.Add($btnCancel);

    # 显示窗口
    $mainForm.Topmost = $true;
    $dialogResult = $mainForm.ShowDialog();

    # 判断是否选择了立即生成
    if ($dialogResult -eq [System.Windows.Forms.DialogResult]::OK){
        # 设置选择的表
        $Tables = $listBox.SelectedItems;
        $connKey = $comboBox.SelectedItem;

        # 选择保存目录
        $app = New-Object -com Shell.Application;
        $selectFolder = $app.BrowseForFolder(0, "选择 $CoreProject 项目层目录", 0, "$rootPath/$CoreProject");

        # 赋值给保存文件夹
        $OutputDir = $selectFolder.Self.Path;
        $ConnectionName = $connDic[$connKey];

        if($OutputDir -eq $null -and $OutputDir -eq "")
        {
            Write-Warning "$FurTools 用户取消操作，程序终止！";
            return;
        }
    }
    else{
        Write-Warning "$FurTools 用户取消操作，程序终止！";
        return;
    }

    # -----------------------------------------------------------------------------
    # 构建 Winform GUI 客户端的 [结束]
    # -----------------------------------------------------------------------------
}
else{
    # 选择保存目录
    $selectFolder = "";
    if($runtimeOS -eq "Windows")
    {
        $app = New-Object -com Shell.Application;
        $selectFolder = $app.BrowseForFolder(0, "选择 $CoreProject 项目层目录", 0, "$rootPath\$CoreProject");
    }
    elseif($runtimeOS -eq "macOS")
    {
        $script = @'  
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
'@
        $selectFolder = osascript -e $script;
    }
    elseif($runtimeOS -eq "Linux")
    {
        $selectFolder = & /usr/bin/zenity --file-selection --directory;
    }
    else
    {
        Write-Warning "$FurTools 未知操作系统类型！";
        return;
    }
    
    if ([string]::IsNullOrEmpty($selectFolder))
    {
        Write-Warning "$FurTools 用户取消操作，程序终止！";
        return;
    }

    # 赋值给保存文件夹
    if($runtimeOS -eq "macOS")
    {
        $OutputDir = $selectFolder;
    }
    else
    {
        $OutputDir = $selectFolder.Self.Path;
    }

    if($OutputDir -eq $null -and $OutputDir -eq "")
    {
        Write-Warning "$FurTools 用户取消操作，程序终止！";
        return;
    }
}

if($ConnectionName -eq "NonConfigureConnectionString")
{
    Write-Warning "$FurTools 未找到连接字符串，程序终止！";
    return;
}

# 执行 dotnet ef dbcontext scaffold 命令

Write-Output "$FurTools 正在编译解决方案代码......";

# 声明完整的命令字符串
$CommandString = "";

try 
{
    # 处理数据库所有表生成情况
    if ($Tables.Count -eq 0)
    {
        $CommandString = "dotnet ef dbcontext scaffold Name=ConnectionStrings:$ConnectionName $DbProvider --project $EntryProject --output-dir $TempOutputDir --context $Context --namespace $Namespace --no-onconfiguring --no-pluralize";
        if($UseDatabaseNames)
        {
            $CommandString += " --use-database-names";
        }
        $CommandString += " --force";
    }
    # 处理特定表生成情况
    else
    {
        # 将配置的数据库表分割成数组
        $TableArray = $Tables.Split(',') | Where-Object { $_ -ne '' };
    
        # 构建 --table 参数的字符串  
        $TableParams = $TableArray | ForEach-Object { "--table $_" };
    
        $CommandString = "dotnet ef dbcontext scaffold Name=ConnectionStrings:$ConnectionName $DbProvider --project $EntryProject --output-dir $TempOutputDir --context $Context --namespace $Namespace $($TableParams -join ' ') --no-onconfiguring --no-pluralize";
        if($UseDatabaseNames)
        {
            $CommandString += " --use-database-names";
        }
        $CommandString += " --force";
    }

    # 执行命令字符串  
    Invoke-Expression $CommandString;

    Write-Output "$FurTools 编译成功！";

    Write-Output "$FurTools 开始生成实体文件......";

    # 获取 DbContext 生成的配置内容
     $dbContextContent = Get-Content "$TempOutputDir\$Context.cs" -raw;
}
catch
{
    Write-Warning "$FurTools 生成失败：$_.Message";
    return;
}

$entityConfigures = [regex]::Matches($dbContextContent, "modelBuilder.Entity\<(?<table>\w+)\>\(entity\s=\>\n*[\s\S]*?\{(?<content>(?:[^{}]|(?<open>{)|(?<-open>}))+(?(open)(?!)))\}\);");

# 定义字典集合
$dic = New-Object -TypeName 'System.Collections.Generic.Dictionary[System.String, System.String]';

# 将配置保存到字典中
for ($i = 0; $i -le $entityConfigures.Count - 1; $i++){
    $groups = $entityConfigures[$i].Groups;
    $tableName = $groups.Value[1];
    $configure = $groups.Value[2] -replace '(?ms)(entity\s*\.\s*)', 'entityBuilder.'

    $dic.Add($tableName, $configure);
}

# 定义实体文件头模板
$fileHeader = @"
// -----------------------------------------------------------------------------
// Generate By $FurTools
// -----------------------------------------------------------------------------

using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using $CoreProject;

"@;

# 定义实体配置模板

$entityConfigure = @"

        public void Configure(EntityTypeBuilder<#Table#> entityBuilder, DbContext dbContext, Type dbContextLocator)
        {#Code#
        }

"@

# 类正则表达式
$classRegex = "public\s+partial\s+class\s+(?<table>\w+)";

# 获取类属性正则表达式
$propRegex = "(namespace\s+.+\n*[\s\S]*?\{\n*[\s\S]*?public\s+partial\s+class\s+(?<table>\w+)\n*[\s\S]*?\{(?<content>[\s\S]*)\}\n*[\s\S]*\})|(namespace\s+.+;\n*[\s\S]*?public\s+partial\s+class\s+(?<table>\w+)\n*[\s\S]*?\{(?<content>[\s\S]*)\}?\n*[\s\S]*\})";

#递归获取 生成的所有临时实体文件
$files = Get-ChildItem $TempOutputDir -Include *.cs -recurse
for ($i = 0; $i -le $files.Count - 1; $i++){
    # 文件名
    $fileName = $files[$i].BaseName;
    # 文件路径
    $filePath = $files[$i].FullName;

    if ($fileName -eq $Context){
        continue;
    }

# 输出
    Write-Output "$FurTools 正在生成 $fileName.cs 实体代码......";

    # 读取生成模型内容
    $entityContent = Get-Content $filePath -raw;

    # 获取类属性定义
    $propsContent = [regex]::Match($entityContent, $propRegex).Groups["content"].value;

    # 生成属性设置
    $commentsDictionary = ParseCommentsFromCode -code $propsContent;
    $modifiedContent = AddXmlCommentsToProperties -content $propsContent -commentsDictionary $commentsDictionary;

    $extents = " : IEntity<$DbContextLocators>";
    $newPropsContent = $propsContent;
# 判断模型配置中是否包含配置
    if ($dic.ContainsKey($fileName)){
        $extents += ", IEntityTypeBuilder<$fileName, $DbContextLocators>";

        # 添加实体配置内容
        $newPropsContent = $propsContent + ($entityConfigure.Replace("#Table#",$fileName).Replace("#Code#",$dic[$fileName]));
    }

# 生成数据库表名注释
$tableDescription = ExtractTableHasComment -inputString $newPropsContent;
if($tableDescription -ne $null -and $tableDescription -ne ''){
    $tableDescription = @"

/// <summary>
/// $tableDescription
/// </summary>
"@;
}

# 生成继承关系和文件头
$finalClass = $fileHeader +  @"

namespace $Namespace;
$tableDescription
public partial class $fileName$extents
{$newPropsContent}
"@;

    # 写入文件
    Set-Content -Path $filePath -Value $finalClass -Encoding utf8;

    # 打印生成后代码
    Write-Output "$FurTools 成功生成 $fileName.cs 实体代码";
    $finalClass;

# 移动文件
    Move-Item $filePath "$OutputDir/$fileName.cs" -force
 }

# 删除临时数据库上下文
Remove-Item "$TempOutputDir/$Context.cs";

# 删除临时实体文件夹
Remove-Item $TempOutputDir -force;

Write-Warning "$FurTools 全部实体生成成功！";