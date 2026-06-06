# ============================================================================
# Git 自动化发布脚本 - 仅发布核心库场景
# 功能：从开发主干分支创建 core 场景的 Release 分支
# ============================================================================

param(
    [Parameter(HelpMessage = "是否跳过工作区干净检查（危险操作）")]
    [switch]$SkipCleanCheck,
    
    [Parameter(HelpMessage = "是否创建 Git Tag（可选功能）")]
    [switch]$CreateTag
)

# ============================================================================
# 配置区域
# ============================================================================

# 开发主干分支名称
$MainBranch = "dev/main"

# 仅发布 core 场景
$ScenarioConfig = @{
    "core" = @(
        "ChaoticKit",
        "ChaoticKit.Data"
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
Invoke-ReleaseBranchCreation -ScenarioConfig $ScenarioConfig -MainBranch $MainBranch -RootFilesToCopy $RootFilesToCopy -SkipCleanCheck:$SkipCleanCheck -CreateTag:$CreateTag
