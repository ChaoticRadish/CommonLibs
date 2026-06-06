# ============================================================================
# Git 自动化发布脚本 - 完整版
# 功能：从开发主干分支创建所有场景的 Release 分支
# ============================================================================

param(
    [Parameter(HelpMessage = "是否跳过工作区干净检查（危险操作）")]
    [switch]$SkipCleanCheck
)

# ============================================================================
# 配置区域
# ============================================================================

# 开发主干分支名称
$MainBranch = "dev/main"

# 场景配置：场景名 -> 需要包含的项目目录数组
# 注意：ChaoticKit 是基础库，几乎所有场景都需要包含
$ScenarioConfig = @{
    "all" = @(
        "ChaoticKit",
        "ChaoticKit.Data",
        "ChaoticKit.GDI",
        "ChaoticKit.Winform",
        "ChaoticKit.Wpf",
        "ChaoticKit.WpfWinformMix",
        "ChaoticKit.Maui",
        "ChaoticKit.Excel.NPOI",
        "ChaoticKit.Excel.NPOI.GDI",
        "ChaoticKit.Excel.NPOI.SkiaSharp",
        "ChaoticKit.NewtonsoftJson",
        "ChaoticKit.Test.Console"
    )
}

# 需要复制的根目录文件（如果存在）
$RootFilesToCopy = @(
    ".gitignore",
    "LICENSE.txt"
)

# ============================================================================
# 加载共享核心逻辑并执行
# ============================================================================

# 获取脚本所在目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 加载共享脚本
. (Join-Path $scriptDir "Release-Common.ps1")

# 执行发布分支创建
Invoke-ReleaseBranchCreation -ScenarioConfig $ScenarioConfig -MainBranch $MainBranch -RootFilesToCopy $RootFilesToCopy -SkipCleanCheck:$SkipCleanCheck
