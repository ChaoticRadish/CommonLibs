# Git 自动化发布脚本使用说明

## 脚本文件说明

本目录包含以下 PowerShell 脚本：

### 核心脚本

1. **Release-Common.ps1** - 共享核心逻辑脚本
   - 包含所有辅助函数（Write-Log, Test-WorkingDirectoryClean 等）
   - 包含主逻辑函数（Invoke-ReleaseBranchCreation）
   - **注意**：此脚本不应直接执行，而是被其他脚本调用

### 配置脚本（实际执行的脚本）

2. **New-ReleaseBranch-Core.ps1** - 仅发布核心库场景
   - 配置：仅包含 `core` 场景
   - 调用 Release-Common.ps1 执行发布

3. **New-ReleaseBranch.ps1** - 发布常用场景
   - 配置：包含常用场景（core, winform, wpf, all）
   - 调用 Release-Common.ps1 执行发布

4. **New-ReleaseBranch-All.ps1** - 发布完整场景
   - 配置：仅包含 `all` 场景（所有项目）
   - 调用 Release-Common.ps1 执行发布

## 使用方法

### 1. 基本使用

在项目根目录下执行：

```powershell
# 仅发布核心库场景
.\CodeManage\ReleaseScript\New-ReleaseBranch-Core.ps1

# 发布常用场景（core, winform, wpf, all）
.\CodeManage\ReleaseScript\New-ReleaseBranch.ps1

# 发布完整场景（all）
.\CodeManage\ReleaseScript\New-ReleaseBranch-All.ps1
```

### 2. 跳过工作区检查（危险操作）

如果确定要跳过工作区干净检查：

```powershell
.\CodeManage\ReleaseScript\New-ReleaseBranch-Core.ps1 -SkipCleanCheck
```

### 3. 执行前提条件

- 当前分支必须是 `dev/main`（开发主干分支）
- 工作区必须干净（无未提交的变更）
- 已安装 Git for Windows
- 使用 PowerShell 5.1 或更高版本

## 场景配置说明

### 各脚本包含的场景

#### New-ReleaseBranch-Core.ps1

| 场景名 | 包含的项目 |
|--------|-----------|
| core | ChaoticKit, ChaoticKit.Data |

#### New-ReleaseBranch.ps1（常用场景）

| 场景名 | 包含的项目 |
|--------|-----------|
| core | ChaoticKit, ChaoticKit.Data |
| winform | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Winform |
| wpf | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Wpf |
| all | 所有库项目（不含测试项目） |

#### New-ReleaseBranch-All.ps1

| 场景名 | 包含的项目 |
|--------|-----------|
| all | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Winform, ChaoticKit.Wpf, ChaoticKit.WpfWinformMix, ChaoticKit.Maui, ChaoticKit.Excel.NPOI, ChaoticKit.Excel.NPOI.GDI, ChaoticKit.Excel.NPOI.SkiaSharp, ChaoticKit.NewtonsoftJson, ChaoticKit.Test.Console |

### 如何修改场景配置

编辑配置脚本文件中的 `$ScenarioConfig` 变量：

```powershell
$ScenarioConfig = @{
    "场景名" = @(
        "项目1",
        "项目2",
        "项目3"
    )
}
```

### 如何创建自定义配置脚本

如果需要创建自定义的场景配置，可以复制 `New-ReleaseBranch-Core.ps1` 并修改：

```powershell
# 1. 复制脚本
Copy-Item .\CodeManage\ReleaseScript\New-ReleaseBranch-Core.ps1 .\CodeManage\ReleaseScript\New-ReleaseBranch-Custom.ps1

# 2. 编辑 New-ReleaseBranch-Custom.ps1，修改场景配置
$ScenarioConfig = @{
    "custom-scenario" = @(
        "ChaoticKit",
        "ChaoticKit.Data",
        "ChaoticKit.GDI"
    )
}

# 3. 执行自定义脚本
.\CodeManage\ReleaseScript\New-ReleaseBranch-Custom.ps1
```

**自定义脚本模板**：

```powershell
# ============================================================================
# Git 自动化发布脚本 - 自定义场景
# ============================================================================

param(
    [Parameter(HelpMessage = "是否跳过工作区干净检查（危险操作）")]
    [switch]$SkipCleanCheck
)

# 开发主干分支名称
$MainBranch = "dev/main"

# 自定义场景配置
$ScenarioConfig = @{
    "my-scenario" = @(
        "ChaoticKit",
        "ChaoticKit.Data",
        # 添加更多项目...
    )
}

# 需要复制的根目录文件
$RootFilesToCopy = @(
    ".gitignore",
    "LICENSE.txt"
)

# 加载共享核心逻辑并执行
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptDir "Release-Common.ps1")
Invoke-ReleaseBranchCreation -ScenarioConfig $ScenarioConfig -MainBranch $MainBranch -RootFilesToCopy $RootFilesToCopy -SkipCleanCheck:$SkipCleanCheck
```

## 版本号格式

版本号格式为 `yyyy.MMdd.HHmm`，例如：
- `2026.0604.1430` 表示 2026年6月4日 14:30 发布

每次执行脚本都会生成新的版本号，确保每个版本都有唯一标识。

## 生成的分支结构

脚本执行后会创建如下分支：

```
release/2026.0604.1430/core
release/2026.0604.1430/winform
release/2026.0604.1430/wpf
...
```

每个分支都是 Orphan 分支（无历史记录），仅包含：
- 指定场景的项目源码
- 项目文件（.csproj）
- 解决方案文件（ChaoticKit.sln）
- 根目录文件（.gitignore, LICENSE.txt 等）

**不包含：**
- 测试项目（ChaoticKit.LibTest.*）
- Git 历史记录
- 其他无关项目

## 业务项目如何引用

### 1. 添加 Submodule

在业务项目中执行：

```bash
# 引用核心库场景
git submodule add -b release/2026.0604.1430/core <仓库地址> libs/ChaoticKit

# 引用 Winform 场景
git submodule add -b release/2026.0604.1430/winform <仓库地址> libs/ChaoticKit
```

### 2. 更新 Submodule

当工具库发布新版本后，在业务项目中更新：

```bash
# 更新到最新版本
git submodule update --remote libs/ChaoticKit

# 或切换到特定版本
cd libs/ChaoticKit
git checkout release/2026.0605.1000/core
cd ../..
git add libs/ChaoticKit
git commit -m "chore: 更新 ChaoticKit 到 2026.0605.1000"
```

### 3. 克隆包含 Submodule 的业务项目

```bash
# 克隆时自动初始化 Submodule
git clone --recurse-submodules <业务项目地址>

# 或在已克隆的项目中初始化
git submodule init
git submodule update
```

## 推送到远程仓库

脚本执行完成后，Release 分支仅在本地创建。确认内容无误后，推送到远程：

```bash
# 推送单个分支
git push origin release/2026.0604.1430/core

# 推送所有 Release 分支（使用通配符）
git push origin 'release/2026.0604.1430/*'
```

## 注意事项

1. **不要修改 Release 分支**：Release 分支是自动生成的只读分支，任何修改都会在下次发布时被覆盖。

2. **版本号唯一性**：每次发布都会生成新版本号，不会覆盖旧版本。

3. **工作区保护**：脚本执行前会检查工作区是否干净，防止误删未提交的文件。

4. **异常恢复**：如果脚本执行失败，会自动切回原始分支并清理临时文件。

5. **测试项目剔除**：所有 `ChaoticKit.LibTest.*` 目录都会被自动剔除，不会出现在 Release 分支中。

## 常见问题

### Q: 为什么使用 Orphan 分支？

A: Orphan 分支没有历史记录，可以保持业务项目的克隆体积最小，同时避免业务项目看到工具库的开发历史。

### Q: 可以修改已发布的 Release 分支吗？

A: 不建议。Release 分支应该保持只读状态。如果发现问题，应该在开发主干分支修复后重新发布新版本。

### Q: 如何查看所有 Release 分支？

```bash
git branch --list 'release/*'
```

### Q: 如何删除本地 Release 分支？

```bash
git branch -D release/2026.0604.1430/core
```

### Q: 版本号可以自定义吗？

A: 当前脚本使用时间戳自动生成版本号。如需自定义，可以修改脚本中的 `New-VersionNumber` 函数。
