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
    [string]$Namespace,
    # 是否数据库命名
    [switch]$UseDatabaseNames
)

# -----------------------------------------------------------------------------
# 日志辅助函数
# -----------------------------------------------------------------------------
function Write-Info {
    param([string]$Message)
    Write-Host "[信息] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[成功] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[警告] $Message" -ForegroundColor Yellow
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "[错误] $Message" -ForegroundColor Red
}

function Write-Separator {
    Write-Host "-----------------------------------------------------------------------------" -ForegroundColor DarkGray
}

# 高亮显示关键步骤
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "=============================================================" -ForegroundColor DarkGray
    Write-Host "  $Message" -ForegroundColor White -BackgroundColor DarkBlue
    Write-Host "=============================================================" -ForegroundColor DarkGray
    Write-Host ""
}

# 高亮显示执行命令
function Write-Command {
    param([string]$Command)
    Write-Host ""
    Write-Host ">>> 执行命令：" -ForegroundColor Yellow
    Write-Host $Command -ForegroundColor Cyan
    Write-Host ""
}

# 匹配数据库表代码注释
function ExtractTableHasComment($inputString) {
    # 匹配 tb.HasComment("xxx") 形式，优先匹配表的注释
    $pattern = '\.ToTable\([^;]*?tb\.HasComment\("([^"]*)"\)'
    if ($inputString -match $pattern) {
        return $Matches[1]
    }
    # 兼容简化的 HasComment 匹配
    $pattern = 'HasComment\("([^"]*)"\)'
    if ($inputString -match $pattern) {
        return $Matches[1]
    }
    return $null
}

# 匹配数据库列代码注释
function ParseCommentsFromCode($code) {
    $commentsDictionary = @{}
    if ([string]::IsNullOrWhiteSpace($code)) { return $commentsDictionary }

    $lines = $code -split "`r?`n"
    $currentPropertyBlock = $null
    $currentPropertyName = $null

    foreach ($line in $lines) {
        # 检测 entityBuilder.Property(e => e.PropertyName) 开始
        if ($line -match 'entityBuilder\.Property\(e\s*=>\s*e\.(?<propertyName>\w+)\)') {
            $currentPropertyBlock = $line
            $currentPropertyName = $Matches['propertyName']
        }
        elseif ($null -ne $currentPropertyBlock -and $line -match ';') {
            # 当前属性块结束
            $currentPropertyBlock += $line
            if ($null -ne $currentPropertyName) {
                if ($currentPropertyBlock -match '\.HasComment\("(?<comment>[^"]*)"\)') {
                    $commentsDictionary[$currentPropertyName] = $Matches['comment']
                } else {
                    $commentsDictionary[$currentPropertyName] = $null
                }
            }
            $currentPropertyBlock = $null
            $currentPropertyName = $null
        }
        elseif ($null -ne $currentPropertyBlock) {
            # 继续拼接当前属性块的代码
            $currentPropertyBlock += $line
        }
    }
    # 处理末尾没有分号的情况
    if ($null -ne $currentPropertyBlock -and $null -ne $currentPropertyName) {
        if ($currentPropertyBlock -match '\.HasComment\("(?<comment>[^"]*)"\)') {
            $commentsDictionary[$currentPropertyName] = $Matches['comment']
        }
    }
    return $commentsDictionary
}

# 生成属性注释
function AddXmlCommentsToProperties($content, $commentsDictionary) {
    $lines = $content -split "`r?`n"
    $result = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        # 匹配属性定义行
        if ($line -match '^\s*public\s+(?:virtual\s+)?[\w<>,\.\s]+\s+(?<propertyName>\w+)\s*\{') {
            $propertyName = $Matches['propertyName']
            # 获取属性行的缩进
            $indentMatch = [regex]::Match($line, '^(\s*)')
            $indent = $indentMatch.Groups[1].Value
            # 检查前一个非空行是否已经是注释
            $hasComment = $false
            $j = $result.Count - 1
            while ($j -ge 0 -and $result[$j].Trim().Length -gt 0 -and $result[$j].Trim().StartsWith("///")) {
                $hasComment = $true
                $j--
            }
            if ($commentsDictionary.ContainsKey($propertyName) -and $commentsDictionary[$propertyName] -and -not $hasComment) {
                $comment = $commentsDictionary[$propertyName]
                $comment = $comment -replace "`r", '' -replace "`n", ' '
                $result.Add("${indent}/// <summary>")
                $result.Add("${indent}/// $comment")
                $result.Add("${indent}/// </summary>")
            }
        }
        $result.Add($line)
    }
    return $result -join [Environment]::NewLine
}

# 归一化缩进
function NormalizeIndent($code, $indent) {
    if ([string]::IsNullOrWhiteSpace($code)) { return "" }
    $lines = $code -split "`r?`n"
    # 计算非空行的最小前导空白
    $minIndent = $null
    foreach ($line in $lines) {
        if ($line.Trim().Length -eq 0) { continue }
        $leadingMatch = [regex]::Match($line, '^(\s*)')
        $leading = $leadingMatch.Groups[1].Value.Length
        if ($null -eq $minIndent -or $leading -lt $minIndent) {
            $minIndent = $leading
        }
    }
    if ($null -eq $minIndent) { $minIndent = 0 }
    $normalizedLines = @()
    foreach ($line in $lines) {
        if ($line.Trim().Length -eq 0) {
            $normalizedLines += ""
        } else {
            $contentWithoutIndent = $line.Substring($minIndent)
            $normalizedLines += "$indent$contentWithoutIndent"
        }
    }
    # 去除开头的空行
    while ($normalizedLines.Count -gt 0 -and $normalizedLines[0].Trim().Length -eq 0) {
        $normalizedLines = $normalizedLines[1..($normalizedLines.Count-1)]
    }
    # 去除结尾的空行
    while ($normalizedLines.Count -gt 0 -and $normalizedLines[$normalizedLines.Count-1].Trim().Length -eq 0) {
        $normalizedLines = $normalizedLines[0..($normalizedLines.Count-2)]
    }
    return $normalizedLines -join [Environment]::NewLine
}

$FurTools = "Furion Tools v4.9.9.92"

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
"@

# 获取当前目录
$rootPath = (Get-Location).Path

# 获取当前操作系统
function GetSystemType {
    if ($PSVersionTable.PSEdition -eq "Core") {
        $runtimeOS = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        if ($runtimeOS.Contains("Linux")) { return "Linux" }
        elseif ($runtimeOS.Contains("Microsoft Windows")) { return "Windows" }
        elseif ($runtimeOS.Contains("macOS") -or $runtimeOS.Contains("Darwin")) { return "macOS" }
        else { return "Unknown OS (PowerShell Core)" }
    } else {
        return "Windows"
    }
}

$runtimeOS = GetSystemType

# 初始化默认值
if ([string]::IsNullOrWhiteSpace($Product)) { $Product = "Furion" }
if ([string]::IsNullOrWhiteSpace($EntryProject)) { $EntryProject = "$Product.Web.Entry" }
if ([string]::IsNullOrWhiteSpace($CoreProject)) { $CoreProject = "$Product.Core" }
if ([string]::IsNullOrWhiteSpace($DbProvider)) { $DbProvider = "Microsoft.EntityFrameworkCore.SqlServer" }
if ([string]::IsNullOrWhiteSpace($Context)) { $Context = $Product + "DbContext" }
if ([string]::IsNullOrWhiteSpace($ConnectionName)) { $ConnectionName = "NonConfigureConnectionString" }
if ([string]::IsNullOrWhiteSpace($DbContextLocators)) { $DbContextLocators = "MasterDbContextLocator" }
if ([string]::IsNullOrWhiteSpace($OutputDir)) { $OutputDir = Join-Path $rootPath "$CoreProject/Entities" }
if ([string]::IsNullOrWhiteSpace($Namespace)) { $Namespace = $CoreProject }

# 输出工具版权声明
Write-Host $copyright -ForegroundColor Magenta

Write-Info "正在启动 $FurTools ..."
Write-Success "$FurTools 启动成功！"

# 定义临时目录
$TempOutputDir = Join-Path $rootPath "$CoreProject/TempEntities"

# 临时目录不存在则创建
if (Test-Path -Path $TempOutputDir) {
    Remove-Item -Path $TempOutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempOutputDir | Out-Null

Write-Separator
# 检查 dotnet ef 命令是否可用
dotnet ef --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "dotnet-ef 未安装"
    Write-Warn "安装命令: dotnet tool install --global dotnet-ef"
} else {
    Write-Success "dotnet-ef 已安装"
    dotnet ef --version
}
Write-Separator

# 仅当 Windows + SQL Server 时显示 GUI 选项
if ($runtimeOS -eq "Windows" -and $DbProvider -like "*SqlServer*") {
    Write-Warn "请选择操作模式："
    Write-Warn "  [G] 界面操作（仅适用于 Windows + SQL Server）"
    Write-Warn "  [任意其他字符] 命令行操作（支持所有环境）"
} else {
    Write-Warn "请选择操作模式："
    Write-Warn "  [任意字符] 命令行操作（当前环境不支持 GUI 模式）"
}
$options = Read-Host "$FurTools 您的输入是"

# 仅当输入 G 且环境同时满足 Windows 和 SQL Server 时才进入 GUI 模式
if ($options -eq "G" -and $runtimeOS -eq "Windows" -and $DbProvider -like "*SqlServer*") {
    # -----------------------------------------------------------------------------
    # 构建 Winform GUI 客户端 [开始]
    # -----------------------------------------------------------------------------

    # 加载数据库表
    function loadDbTable {
        $conn = $null
        $cmd = $null
        $da = $null
        $ds = $null
        try {
            $connStr = $comboBox.SelectedItem
            if ([string]::IsNullOrWhiteSpace($connStr)) {
                [System.Windows.Forms.MessageBox]::Show("请选择数据库连接字符串后再操作", "提示", "OK", "Warning")
                throw "未选择数据库连接字符串"
            }
            $conn = New-Object System.Data.SqlClient.SqlConnection
            $conn.ConnectionString = $connStr
            $conn.Open()
            $cmd = New-Object System.Data.SqlClient.SqlCommand(
                "SELECT i.name + '.' + h.name AS FullName FROM sys.objects h 
                 LEFT JOIN sys.schemas i ON h.schema_id = i.schema_id 
                 WHERE h.type IN ('U','V') 
                 ORDER BY h.type, i.name, h.name",
                $conn
            )
            $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
            $ds = New-Object System.Data.DataSet
            [void]$da.Fill($ds)

            $listBox.Items.Clear()
            foreach ($row in $ds.Tables[0].Rows) {
                if ($row -ne $null -and $row[0] -ne $null) {
                    [void]$listBox.Items.Add($row[0].ToString())
                }
            }
            Write-Success "表和视图加载成功！"
        }
        catch {
            $errorMsg = "连接失败`n`n连接字符串: '$connStr'`n`n错误详情:`n$($_.Exception.Message)`n`n$($_.Exception.StackTrace)"
            Write-ErrorMsg "详细错误：`n$errorMsg"
            $displayMsg = if ($errorMsg.Length -gt 1000) { $errorMsg.Substring(0, 1000) + "`n...(内容过长已截断)" } else { $errorMsg }
            [System.Windows.Forms.MessageBox]::Show($displayMsg, "数据库连接错误", "OK", "Error")
            throw
        }
        finally {
            if ($da -ne $null) { $da.Dispose() }
            if ($cmd -ne $null) { $cmd.Dispose() }
            if ($conn -ne $null) {
                if ($conn.State -eq 'Open') { $conn.Close() }
                $conn.Dispose()
            }
        }
    }

    # 加载连接设置
    function loadConnectionSettings($settingsPath) {
        $appsetting = [System.IO.File]::ReadAllText($settingsPath, [System.Text.Encoding]::UTF8)
        $connectionDefine = [regex]::Matches($appsetting, '"ConnectionStrings"\s*:\s*\{(?<define>[\s\S]*?)\}')
        if ($connectionDefine.Count -eq 0) { return }

        $connectionDefineContent = $connectionDefine[0].Groups["define"].Value
        $connections = [regex]::Matches($connectionDefineContent, '"(.*?)"\s*:\s*"(?<connectionStr>.*?)"')

        for ($i = 0; $i -lt $connections.Count; $i++) {
            $key = $connections[$i].Groups[1].Value
            $value = $connections[$i].Groups["connectionStr"].Value
            if (-not $connDic.ContainsKey($value)) {
                [void]$comboBox.Items.Add($value)
                $connDic.Add($value, $key)
            }
        }
    }

    # 添加 Winform 应用程序
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing

    # 创建一个 Winform 窗口
    $mainForm = New-Object System.Windows.Forms.Form
    $mainForm.Text = $FurTools
    $mainForm.Size = New-Object System.Drawing.Size(800,600)
    $mainForm.StartPosition = "CenterScreen"

    # 创建组面板
    $baseSetting = New-Object System.Windows.Forms.GroupBox
    $baseSetting.SuspendLayout()
    $baseSetting.Location = New-Object System.Drawing.Point(15, 15)
    $baseSetting.Size = New-Object System.Drawing.Size(760, 120)
    $baseSetting.Text = "基础设置"
    $baseSetting.TabIndex = 10
    $baseSetting.TabStop = $false
    $baseSetting.ResumeLayout($false)
    $baseSetting.PerformLayout()
    $mainForm.Controls.Add($baseSetting)

    # 构建数据库连接字符串提示
    $label = New-Object System.Windows.Forms.Label
    $label.Location = New-Object System.Drawing.Point(15,35)
    $label.AutoSize = $true
    $label.Size = New-Object System.Drawing.Size(280,20)
    $label.Text = '选择数据库连接字符串：'
    $label.TabIndex = 9
    $baseSetting.Controls.Add($label)

    # 构建多数据库上下定位器文字符提示
    $locatorLabel = New-Object System.Windows.Forms.Label
    $locatorLabel.Location = New-Object System.Drawing.Point(15,80)
    $locatorLabel.AutoSize = $true
    $locatorLabel.Size = New-Object System.Drawing.Size(280,20)
    $locatorLabel.Text = '多数据库上下文定位器：'
    $locatorLabel.TabIndex = 9
    $baseSetting.Controls.Add($locatorLabel)

    # 数据库上下文定位器文本框
    $locatorTextBox = New-Object System.Windows.Forms.TextBox
    $locatorTextBox.Location = New-Object System.Drawing.Point(200,75)
    $locatorTextBox.Size = New-Object System.Drawing.Size(370,20)
    $locatorTextBox.TabIndex = 9
    $locatorTextBox.Text = $DbContextLocators
    $baseSetting.Controls.Add($locatorTextBox)

    # 连接字典
    $connDic = New-Object -TypeName 'System.Collections.Generic.Dictionary[System.String, System.String]'

    # 构建数据库连接字符串下拉
    $comboBox = New-Object System.Windows.Forms.ComboBox
    $comboBox.Location = New-Object System.Drawing.Point(200,30)
    $comboBox.Size = New-Object System.Drawing.Size(370,20)
    $comboBox.TabIndex = 9
    $comboBox.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    # 绑定按钮事件
    $comboBoxClickEventHandler = [System.EventHandler] {
        $connStr = $comboBox.SelectedItem
        if ($connStr -eq $null -or $connStr -eq ""){
            $btnGenerate.Enabled =$false
        }
        else{
            $btnGenerate.Enabled =$true
            $ConnectionName = $connDic[$connStr]
        }
    }
    $comboBox.Add_SelectedIndexChanged($comboBoxClickEventHandler)
    $baseSetting.Controls.Add($comboBox)

    # 读取 所有配置文件
    # -----------------------------------------------------------------------------
    # [开始]
    $jsons = Get-ChildItem -Path $rootPath -Filter *.json -Recurse
    for ($i = 0; $i -le $jsons.Count - 1; $i++){
        $json = $jsons[$i]
        if(!($json.DirectoryName.Contains("bin") -or $json.DirectoryName.Contains("obj") -or $json.DirectoryName.Contains(".vscode") -or $json.FullName.Contains(".deps.json"))){
          loadConnectionSettings($json.FullName)
        }
    }
    # [结束]
    # -----------------------------------------------------------------------------

    # 构建加载数据库表按钮
    $btnLoad = New-Object System.Windows.Forms.Button
    $btnLoad.Location = New-Object System.Drawing.Point(595,30)
    $btnLoad.Size = New-Object System.Drawing.Size(150, 25)
    $btnLoad.TabIndex = 9
    $btnLoad.Text = "加载数据库表和视图"
    # 绑定按钮事件
    $btnLoadClickEventHandler = [System.EventHandler] {
        # 保存数据库上下文定位器
        $DbContextLocators = $locatorTextBox.Text

        try{
            Write-Info "正在加载数据库表和视图......"
            loadDbTable
            Write-Success "加载成功！"
        }
        catch{
            Write-Warn "加载数据库表和视图出错，请重试！"
        }
    }
    $btnLoad.Add_Click($btnLoadClickEventHandler)
    $baseSetting.Controls.Add($btnLoad)

    # 创建表和视图面板
    $tableSetting = New-Object System.Windows.Forms.GroupBox
    $tableSetting.SuspendLayout()
    $tableSetting.Location = New-Object System.Drawing.Point(15, 155)
    $tableSetting.Size = New-Object System.Drawing.Size(760, 345)
    $tableSetting.Text = "数据库表和视图"
    $tableSetting.TabIndex = 10
    $tableSetting.TabStop = $false
    $tableSetting.ResumeLayout($false)
    $tableSetting.PerformLayout()
    $mainForm.Controls.Add($tableSetting)

    # 创建表和视图容器
    $listBox = New-Object System.Windows.Forms.Listbox
    $listBox.BackColor = [System.Drawing.SystemColors]::Window
    $listBox.FormattingEnabled = $true
    $listBox.ItemHeight = 20
    $listBox.TabIndex = 9
    $listBox.Location = New-Object System.Drawing.Point(15,35)
    $listBox.Size = New-Object System.Drawing.Size(730,295)
    $listBox.SelectionMode = [System.Windows.Forms.SelectionMode]::MultiExtended
    $tableSetting.Controls.Add($listBox)

    # 创建立即生成按钮和取消生成按钮
    $btnGenerate = New-Object System.Windows.Forms.Button
    $btnGenerate.Location = New-Object System.Drawing.Point(530,520)
    $btnGenerate.Size = New-Object System.Drawing.Size(100, 25)
    $btnGenerate.TabIndex = 8
    $btnGenerate.Text = "立即生成"
    $btnGenerate.Enabled =$false
    $btnGenerate.BackColor = [System.Drawing.SystemColors]::ControlLight
    $btnGenerate.DialogResult = [System.Windows.Forms.DialogResult]::OK
    $mainForm.AcceptButton = $btnGenerate
    $mainForm.Controls.Add($btnGenerate)

    $btnCancel = New-Object System.Windows.Forms.Button
    $btnCancel.Location = New-Object System.Drawing.Point(650,520)
    $btnCancel.Size = New-Object System.Drawing.Size(100, 25)
    $btnCancel.TabIndex = 8
    $btnCancel.Text = "取消生成"
    $btnCancel.DialogResult = [System.Windows.Forms.DialogResult]::Cancel
    $mainForm.CancelButton = $btnCancel
    $mainForm.Controls.Add($btnCancel)

    # 显示窗口
    $mainForm.Topmost = $true
    $dialogResult = $mainForm.ShowDialog()

    # 判断是否选择了立即生成
    if ($dialogResult -eq [System.Windows.Forms.DialogResult]::OK){
        # 设置选择的表
        $Tables = $listBox.SelectedItems
        $connKey = $comboBox.SelectedItem

        # 选择保存目录
        $app = New-Object -com Shell.Application
        $selectFolder = $app.BrowseForFolder(0, "选择 $CoreProject 项目层目录", 0, (Join-Path $rootPath $CoreProject))

        # 赋值给保存文件夹
        $OutputDir = $selectFolder.Self.Path
        $ConnectionName = $connDic[$connKey]

        if ([string]::IsNullOrWhiteSpace($OutputDir))
        {
            Write-Warn "用户取消操作，程序终止！"
            return
        }
    }
    else{
        Write-Warn "用户取消操作，程序终止！"
        return
    }

    # -----------------------------------------------------------------------------
    # 构建 Winform GUI 客户端的 [结束]
    # -----------------------------------------------------------------------------
}
else{
    # 命令行模式
    # 选择保存目录
    $selectFolder = ""
    if($runtimeOS -eq "Windows")
    {
        $app = New-Object -com Shell.Application
        $selectFolderObj = $app.BrowseForFolder(0, "选择 $CoreProject 项目层目录", 0, (Join-Path $rootPath $CoreProject))
        if ($selectFolderObj -ne $null) {
            $selectFolder = $selectFolderObj.Self.Path
        }
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
        $selectFolder = (osascript -e $script).Trim()
    }
    elseif($runtimeOS -eq "Linux")
    {
        $selectFolder = (& /usr/bin/zenity --file-selection --directory).Trim()
    }
    else
    {
        Write-Warn "未知操作系统类型！"
        return
    }

    if ([string]::IsNullOrEmpty($selectFolder))
    {
        Write-Warn "用户取消操作，程序终止！"
        return
    }

    # 赋值给保存文件夹
    $OutputDir = $selectFolder

    if ([string]::IsNullOrWhiteSpace($OutputDir))
    {
        Write-Warn "用户取消操作，程序终止！"
        return
    }
}

if($ConnectionName -eq "NonConfigureConnectionString")
{
    Write-Warn "未找到连接字符串，程序终止！"
    return
}

# 执行 dotnet ef dbcontext scaffold 命令
Write-Step "编译解决方案代码"
Write-Info "正在编译解决方案代码......"

$CommandString = ""
try
{
    # 统一处理表参数
    $tableList = @()
    if ($Tables.Count -gt 0) {
        foreach ($t in $Tables) {
            $tableList += $t -split ','
        }
        $tableList = $tableList | Where-Object { $_ -ne '' } | ForEach-Object { $_.Trim() } | Select-Object -Unique
    }

    # 处理数据库所有表生成情况
    if ($tableList.Count -eq 0)
    {
        $CommandString = "dotnet ef dbcontext scaffold Name=ConnectionStrings:$ConnectionName $DbProvider --project $EntryProject --output-dir $TempOutputDir --context $Context --namespace $Namespace --no-onconfiguring --no-pluralize --verbose"
        if($UseDatabaseNames)
        {
            $CommandString += " --use-database-names"
        }
        $CommandString += " --force"
    }
    else
    {
        $TableParams = $tableList | ForEach-Object { "--table $_" }
        $CommandString = "dotnet ef dbcontext scaffold Name=ConnectionStrings:$ConnectionName $DbProvider --project $EntryProject --output-dir $TempOutputDir --context $Context --namespace $Namespace $($TableParams -join ' ') --no-onconfiguring --no-pluralize --verbose"
        if($UseDatabaseNames)
        {
            $CommandString += " --use-database-names"
        }
        $CommandString += " --force"
    }

    # 高亮显示执行命令
    Write-Command -Command $CommandString
    Invoke-Expression $CommandString

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef 命令执行失败（退出码: $LASTEXITCODE）"
    }

    Write-Success "编译成功！"
    Write-Step "开始生成实体文件"
    Write-Info "开始生成实体文件......"

    # 确保输出目录存在
    if (-not (Test-Path -Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
        Write-Info "已创建输出目录：$OutputDir"
    }

    # 显式 UTF-8 读取 DbContext 文件
    $dbContextContent = [System.IO.File]::ReadAllText("$TempOutputDir\$Context.cs", [System.Text.Encoding]::UTF8)

    # 提取每个实体的配置代码
    $entityConfigures = [regex]::Matches($dbContextContent, "modelBuilder\.Entity<(?<table>\w+)>\(\s*\w+\s*=>\s*\{(?<content>(?:[^{}]|(?<open>{)|(?<-open>}))+(?(open)(?!)))\}\);")
    $dic = New-Object -TypeName 'System.Collections.Generic.Dictionary[System.String, System.String]'

    for ($i = 0; $i -lt $entityConfigures.Count; $i++){
        $groups = $entityConfigures[$i].Groups
        $tableName = $groups["table"].Value
        $configure = $groups["content"].Value -replace '(?ms)(entity\s*\.\s*)', 'entityBuilder.'
        $dic.Add($tableName, $configure)
    }

    # 定义实体文件头模板
    $fileHeader = @"
// -----------------------------------------------------------------------------
// Generate By $FurTools
// -----------------------------------------------------------------------------

#nullable enable

using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using $CoreProject;

"@

    # 定义实体配置模板
    $entityConfigure = @"

    public void Configure(EntityTypeBuilder<#Table#> entityBuilder, DbContext dbContext, Type dbContextLocator)
    {
#Code#
    }

"@

    # 获取类属性正则表达式
    $propRegex = "(?:namespace\s+[\w\.]+\s*;\s*)?(?:public\s+partial\s+class\s+(?<table>\w+)\s*\{)(?<content>[\s\S]*?)\n\}"

    # 递归获取生成的所有临时实体文件
    $files = Get-ChildItem -Path $TempOutputDir -Filter *.cs -Recurse
    foreach ($file in $files) {
        $fileName = $file.BaseName
        if ($fileName -eq $Context) { continue }

        Write-Info "正在生成 $fileName.cs 实体代码......"

        # 显式 UTF-8 读取实体文件
        $entityContent = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        $propsMatch = [regex]::Match($entityContent, $propRegex)
        if (-not $propsMatch.Success) {
            Write-Warn "无法解析实体 $fileName 的属性，跳过注释添加。"
            $propsContent = ""
        } else {
            $propsContent = $propsMatch.Groups["content"].Value
        }

        # 获取该实体的配置代码
        $configureCode = if ($dic.ContainsKey($fileName)) { $dic[$fileName] } else { "" }

        # 从配置代码中提取列注释
        $commentsDictionary = ParseCommentsFromCode -code $configureCode

        # 给属性添加 XML 注释
        $modifiedPropsContent = AddXmlCommentsToProperties -content $propsContent -commentsDictionary $commentsDictionary

        # 构建实体继承和配置
        $extents = " : IEntity<$DbContextLocators>"
        $newPropsContent = $modifiedPropsContent
        if ($dic.ContainsKey($fileName)) {
            $extents += ", IEntityTypeBuilder<$fileName, $DbContextLocators>"
            # 归一化配置代码缩进
            $normalizedConfig = NormalizeIndent -code $configureCode -indent "        "
            $configBlock = $entityConfigure.Replace("#Table#", $fileName).Replace("#Code#", $normalizedConfig)
            $newPropsContent = $modifiedPropsContent + $configBlock
        }

        # 提取表注释
        $tableDescription = ""
        if ($dic.ContainsKey($fileName)) {
            $tableComment = ExtractTableHasComment -inputString $configureCode
            if ($tableComment) {
                $tableDescription = @"

/// <summary>
/// $tableComment
/// </summary>
"@
            }
        }

        # 组装最终文件内容
        $finalClass = $fileHeader + @"

namespace $Namespace;
$tableDescription
public partial class $fileName$extents
{$newPropsContent}
"@

        # 显式 UTF-8 无 BOM 写入文件
        [System.IO.File]::WriteAllText($file.FullName, $finalClass, [System.Text.Encoding]::UTF8)
        Write-Success "成功生成 $fileName.cs 实体代码"
        Move-Item -Path $file.FullName -Destination (Join-Path $OutputDir "$fileName.cs") -Force
    }

    # 删除临时数据库上下文
    Remove-Item "$TempOutputDir/$Context.cs"
    # 删除临时实体文件夹
    Remove-Item $TempOutputDir -Recurse -Force

    Write-Step "全部实体生成成功"
    Write-Success "全部实体生成成功！"
}
catch
{
    Write-ErrorMsg "生成失败：$($_.Exception.Message)"
    Write-ErrorMsg "脚本已终止，后续操作已取消"
}
finally
{
    # 无论成功、异常，确保清理临时目录
    if (Test-Path -Path $TempOutputDir) {
        Remove-Item -Path $TempOutputDir -Recurse -Force
    }
}