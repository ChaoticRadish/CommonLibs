# ============================================================================
# Git 自动化发布脚本 - 共享核心逻辑
# 此脚本包含所有共享的函数和主逻辑，被其他脚本调用
# ============================================================================

# 设置控制台编码为 UTF-8，确保中文正确显示
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
# 注意：不要设置 $PSDefaultParameterValues['*:Encoding']。
# 该设置会被注入到 .NET 方法调用中，而 [System.Text.UTF8Encoding] 没有
# Encoding 参数，会导致 New-ReleaseInfoFile 抛异常。

# ============================================================================
# 辅助函数
# ============================================================================

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Level = "Info"
    )
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    $color = switch ($Level) {
        "Info"    { "White" }
        "Warning" { "Yellow" }
        "Error"   { "Red" }
        "Success" { "Green" }
    }
    
    Write-Host "[$timestamp] " -NoNewline
    Write-Host $Message -ForegroundColor $color
}

function Test-WorkingDirectoryClean {
    # 检查用户真实工作区是否有未提交的变更
    # 显式指定工作区与 Git 目录，避免受脚本当前所在目录影响
    $oldIndex = $env:GIT_INDEX_FILE
    $oldDir = $env:GIT_DIR
    $oldTree = $env:GIT_WORK_TREE
    try {
        $env:GIT_INDEX_FILE = $null
        $env:GIT_DIR = $script:RepoGitDir
        $env:GIT_WORK_TREE = $script:RepoRoot

        $status = git status --porcelain 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Git 命令执行失败" -Level Error
            return $false
        }
    }
    finally {
        $env:GIT_INDEX_FILE = $oldIndex
        $env:GIT_DIR = $oldDir
        $env:GIT_WORK_TREE = $oldTree
    }

    if ($status) {
        Write-Log "工作区不干净，存在未提交的变更：" -Level Error
        Write-Host $status
        return $false
    }

    return $true
}

function Get-CurrentBranch {
    $branch = git rev-parse --abbrev-ref HEAD 2>&1
    if ($LASTEXITCODE -ne 0) {
        return $null
    }
    return $branch
}

function New-VersionNumber {
    # 生成版本号：yyyy.MMdd.HHmm
    $now = Get-Date
    $year = $now.ToString("yyyy")
    $monthDay = $now.ToString("MMdd")
    $hourMinute = $now.ToString("HHmm")
    return "$year.$monthDay.$hourMinute"
}

function Test-DirectoryExists {
    param([string]$Path)
    return Test-Path -Path $Path -PathType Container
}

function Test-BranchExists {
    param([string]$BranchName)
    git show-ref --verify --quiet "refs/heads/$BranchName" 2>$null
    return ($LASTEXITCODE -eq 0)
}

function Test-TagExists {
    param([string]$TagName)
    git show-ref --verify --quiet "refs/tags/$TagName" 2>$null
    return ($LASTEXITCODE -eq 0)
}

function Copy-ProjectDirectory {
    param(
        [string]$SourceDir,
        [string]$TargetDir,
        [string]$ProjectName
    )

    if ([string]::IsNullOrEmpty($ProjectName)) {
        $ProjectName = Split-Path -Leaf $TargetDir
    }
    
    if (-not (Test-DirectoryExists $SourceDir)) {
        Write-Log "目录不存在，跳过：$SourceDir" -Level Warning
        return $false
    }
    
    # 保持目录结构复制
    if (-not (Test-Path $TargetDir)) {
        New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
    }
    
    # 锁定/权限问题会让 Copy-Item 以"非终止错误"的形式跳过个别文件，
    # 从而产出内容残缺的 Release 分支。因此这里显式校验关键文件。
    Copy-Item -Path "$SourceDir\*" -Destination $TargetDir -Recurse -Force -ErrorAction SilentlyContinue

    $csproj = Join-Path $TargetDir "$ProjectName.csproj"
    if (-not (Test-Path $csproj)) {
        Write-Log "项目文件复制失败（文件被占用或权限不足）：$csproj" -Level Error
        return $false
    }

    return $true
}

function New-SolutionFile {
    param(
        [string]$OutputPath,
        [string[]]$ProjectNames,
        [string]$VersionNumber,
        [string]$ScenarioName
    )
    
    # 使用 StringBuilder 构建解决方案文件内容
    $sb = [System.Text.StringBuilder]::new()
    
    # 文件头
    [void]$sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00")
    [void]$sb.AppendLine("# Visual Studio Version 17")
    [void]$sb.AppendLine("VisualStudioVersion = 17.10.35004.147")
    [void]$sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1")
    
    # 为每个项目生成 Project 条目
    $projectGuids = @{}
    $projectTypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"  # SDK 风格项目
    
    foreach ($projectName in $ProjectNames) {
        $guid = [System.Guid]::NewGuid().ToString().ToUpper()
        $projectGuids[$projectName] = $guid
        
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("Project(`"$projectTypeGuid`") = `"$projectName`", `"$projectName\$projectName.csproj`", `{$guid}`"")
        [void]$sb.AppendLine("EndProject")
    }
    
    # Global 部分
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("Global")
    [void]$sb.AppendLine("	GlobalSection(SolutionConfigurationPlatforms) = preSolution")
    [void]$sb.AppendLine("		Debug|Any CPU = Debug|Any CPU")
    [void]$sb.AppendLine("		Release|Any CPU = Release|Any CPU")
    [void]$sb.AppendLine("	EndGlobalSection")
    [void]$sb.AppendLine("	GlobalSection(ProjectConfigurationPlatforms) = postSolution")
    
    # 为每个项目添加配置
    foreach ($projectName in $ProjectNames) {
        $guid = $projectGuids[$projectName]
        [void]$sb.AppendLine("		{$guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
        [void]$sb.AppendLine("		{$guid}.Debug|Any CPU.Build.0 = Debug|Any CPU")
        [void]$sb.AppendLine("		{$guid}.Release|Any CPU.ActiveCfg = Release|Any CPU")
        [void]$sb.AppendLine("		{$guid}.Release|Any CPU.Build.0 = Release|Any CPU")
    }
    
    [void]$sb.AppendLine("	EndGlobalSection")
    [void]$sb.AppendLine("	GlobalSection(SolutionProperties) = preSolution")
    [void]$sb.AppendLine("		HideSolutionNode = FALSE")
    [void]$sb.AppendLine("	EndGlobalSection")
    [void]$sb.AppendLine("	GlobalSection(ExtensibilityGlobals) = postSolution")
    [void]$sb.AppendLine("		SolutionGuid = {[System.Guid]::NewGuid().ToString().ToUpper()}")
    [void]$sb.AppendLine("	EndGlobalSection")
    [void]$sb.AppendLine("EndGlobal")
    
    # 写入文件（使用 UTF-8 without BOM）
    $slnContent = $sb.ToString()
    [System.IO.File]::WriteAllText($OutputPath, $slnContent, (New-Object System.Text.UTF8Encoding($false)))
}

function New-ReleaseInfoFile {
    param(
        [string]$OutputPath,
        [string]$VersionNumber,
        [string]$Scenario,
        [string]$SourceBranch,
        [string]$SourceCommitHash,
        [string]$SourceCommitMessage,
        [string[]]$Projects
    )

    $sourceCommitShort = $SourceCommitHash.Substring(0, 7)
    $publishTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
    $publishedBy = $env:USERNAME

    # 构建项目列表
    $projectList = $Projects | ForEach-Object { "- $_" }
    $projectListText = $projectList -join "`n"

    # 构建内容
    $content = @"
# Release Info

- **Version**: $VersionNumber
- **Scenario**: $Scenario
- **Source Commit**: ``$SourceCommitHash`` (on branch ``$SourceBranch``)
- **Source Commit Message**: $SourceCommitMessage
- **Publish Time**: $publishTime
- **Published By**: $publishedBy

## Projects

$projectListText

## How to Trace

To view the source code of this release:

``````bash
# Switch to $SourceBranch branch
git checkout $SourceBranch

# View the source commit
git show $sourceCommitShort
``````
"@

    [System.IO.File]::WriteAllText($OutputPath, $content, (New-Object System.Text.UTF8Encoding($false)))
}

# ============================================================================
# 主逻辑函数
#
# 设计原则：
#   使用 Git 底层命令（plumbing）在 .local/temp 中组装发布内容，
#   再直接生成提交对象与分支引用。
#   全程不切换分支、不修改主仓库索引、不向工作区复制任何文件。
#
# 为什么不再使用 "git checkout --orphan + git rm -rf . + git add ."：
#   git rm -rf . 在 Windows 上遇到被独占锁定的已跟踪文件时（Visual Studio
#   占用 .csproj.user、杀毒软件扫描中等）会静默失败：退出码仍为 0，索引被
#   清空，但工作区文件一个都没删。随后 git add . 会把工作区里残留的全部内容
#   （测试项目、CodeManage、临时产物等）一并提交进 Release 分支；
#   切回主干后这些残留文件还会留在主干工作区，令工作区变脏。
#   plumbing 方式完全不触碰工作区，该类失败模式从根本上不存在。
# ============================================================================

function Invoke-ReleaseBranchCreation {
    param(
        [hashtable]$ScenarioConfig,
        [string]$MainBranch = "dev/main",
        [string[]]$RootFilesToCopy = @(".gitignore", "LICENSE.txt"),
        [switch]$SkipCleanCheck,
        [switch]$CreateTag
    )

    $script:RepoRoot = $null
    $script:RepoGitDir = $null
    $tempDir = $null
    $successCount = 0
    $failedScenarios = @()
    $createdBranches = @()
    $createdTags = @()

    try {
        Write-Log "========================================" -Level Info
        Write-Log "Git 自动化发布脚本启动" -Level Info
        Write-Log "========================================" -Level Info
        Write-Log "模式：plumbing（不切换分支，不修改工作区）" -Level Info

        # ---------- 1. 定位仓库 ----------
        $repoRoot = (git rev-parse --show-toplevel 2>&1)
        if ($LASTEXITCODE -ne 0 -or -not $repoRoot) {
            Write-Log "无法获取仓库根目录（请在 Git 仓库内执行）" -Level Error
            return $false
        }
        $repoRoot = $repoRoot.Trim()
        $script:RepoRoot = $repoRoot

        $gitDir = (git rev-parse --absolute-git-dir 2>&1)
        if ($LASTEXITCODE -ne 0 -or -not $gitDir) {
            Write-Log "无法获取 Git 目录" -Level Error
            return $false
        }
        $gitDir = $gitDir.Trim()
        $script:RepoGitDir = $gitDir

        Write-Log "仓库根目录：$repoRoot" -Level Info

        # ---------- 2. 检查工作区是否干净（可选） ----------
        if (-not $SkipCleanCheck) {
            Write-Log "检查工作区状态..." -Level Info
            if (-not (Test-WorkingDirectoryClean)) {
                Write-Log "请先提交或暂存所有变更后再执行此脚本" -Level Error
                Write-Log "或使用 -SkipCleanCheck 参数跳过检查" -Level Warning
                return $false
            }
            Write-Log "工作区干净，继续执行" -Level Success
        }
        else {
            Write-Log "已跳过工作区干净检查" -Level Warning
            Write-Log "注意：未提交的改动不会进入 Release 分支（分支只反映 HEAD 提交）" -Level Warning
        }

        # ---------- 3. 确认当前分支 ----------
        $currentBranch = Get-CurrentBranch
        if ($null -eq $currentBranch) {
            Write-Log "无法获取当前分支信息" -Level Error
            return $false
        }

        Write-Log "当前分支：$currentBranch" -Level Info

        if ($currentBranch -ne $MainBranch) {
            Write-Log "当前分支不是开发主干分支 '$MainBranch'" -Level Error
            Write-Log "请切换到 '$MainBranch' 分支后再执行此脚本" -Level Warning
            return $false
        }

        # ---------- 4. 版本号与源提交信息 ----------
        $versionNumber = New-VersionNumber
        Write-Log "生成版本号：$versionNumber" -Level Success

        $sourceCommitHash = (git rev-parse HEAD 2>&1)
        if ($LASTEXITCODE -ne 0) {
            Write-Log "无法获取 HEAD 提交" -Level Error
            return $false
        }
        $sourceCommitHash = $sourceCommitHash.Trim()
        $sourceCommitShort = $sourceCommitHash.Substring(0, 7)
        $sourceCommitMessage = (git log -1 --pretty=format:"%s" 2>&1)
        if ($LASTEXITCODE -ne 0) {
            $sourceCommitMessage = ""
        }
        Write-Log "源提交：$sourceCommitShort - $sourceCommitMessage" -Level Info

        # ---------- 5. 临时工作目录 ----------
        $tempDir = Join-Path $repoRoot ".local\temp\release-$versionNumber"
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        Write-Log "创建临时工作目录：$tempDir" -Level Info

        # ---------- 6. 遍历所有场景配置 ----------
        foreach ($scenario in $ScenarioConfig.Keys) {
            Write-Log "----------------------------------------" -Level Info
            Write-Log "处理场景：$scenario" -Level Info
            Write-Log "----------------------------------------" -Level Info

            $projects = $ScenarioConfig[$scenario]
            $branchName = "release/$versionNumber/$scenario"
            $newCommit = $null

            try {
                # 同名分支并非本次创建，失败时不应被删除
                if (Test-BranchExists -BranchName $branchName) {
                    Write-Log "分支已存在：$branchName，跳过" -Level Warning
                    $failedScenarios += "$scenario (分支已存在)"
                    continue
                }

                # ----- 6.1 组装发布内容到独立目录 -----
                $stagingDir = Join-Path $tempDir $scenario
                if (Test-Path $stagingDir) {
                    Remove-Item -Path $stagingDir -Recurse -Force
                }
                New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

                $copiedProjects = @()
                $copyFailures = @()
                foreach ($project in $projects) {
                    $sourcePath = Join-Path $repoRoot $project
                    $targetPath = Join-Path $stagingDir $project

                    if (Copy-ProjectDirectory -SourceDir $sourcePath -TargetDir $targetPath -ProjectName $project) {
                        $copiedProjects += $project
                        Write-Log "已复制项目：$project" -Level Success
                    }
                    else {
                        # 删除可能残留的部分副本，避免残缺内容进入发布分支
                        if (Test-Path $targetPath) {
                            Remove-Item -Path $targetPath -Recurse -Force -ErrorAction SilentlyContinue
                        }
                        $copyFailures += $project
                    }
                }

                if ($copiedProjects.Count -eq 0) {
                    Write-Log "场景 $scenario 没有成功复制任何项目，跳过" -Level Warning
                    $failedScenarios += "$scenario (无有效项目)"
                    continue
                }

                # 发布内容必须完整：任一项目复制失败都中止该场景，
                # 绝不产出缺少项目的残缺 Release 分支。
                if ($copyFailures.Count -gt 0) {
                    throw "以下项目复制失败，场景内容不完整：$($copyFailures -join ', ')。请关闭占用该文件的程序（如 Visual Studio）后重试，或从场景配置中移除该项目。"
                }

                foreach ($rootFile in $RootFilesToCopy) {
                    $sourceFile = Join-Path $repoRoot $rootFile
                    if (Test-Path $sourceFile) {
                        Copy-Item -Path $sourceFile -Destination (Join-Path $stagingDir $rootFile) -Force
                        Write-Log "已复制根文件：$rootFile" -Level Success
                    }
                }

                # 忽略规则由发布内容里的 .gitignore 决定（bin/obj 等靠它排除）。
                # 如果它没能进入发布内容，忽略规则就会失效，故显式提醒。
                if (-not (Test-Path (Join-Path $stagingDir ".gitignore"))) {
                    Write-Log "警告：发布内容中缺少 .gitignore，被忽略的文件（如 bin/obj）将无法排除。请确认 .gitignore 在 \$RootFilesToCopy 中且存在于仓库根目录。" -Level Warning
                }

                # ----- 6.2 生成解决方案文件 -----
                # 场景 all 包含全部项目时沿用主仓库的解决方案文件，
                # 避免因缺少 项目名 -> csproj 名称 映射而生成无法加载的工程条目。
                $slnSource = Join-Path $repoRoot "ChaoticKit.sln"
                $slnTarget = Join-Path $stagingDir "ChaoticKit.sln"
                if ($scenario -eq "all" -and (Test-Path $slnSource)) {
                    Copy-Item -Path $slnSource -Destination $slnTarget -Force
                    Write-Log "已复制解决方案文件：ChaoticKit.sln" -Level Success
                }
                else {
                    New-SolutionFile -OutputPath $slnTarget -ProjectNames $copiedProjects -VersionNumber $versionNumber -ScenarioName $scenario
                    Write-Log "已生成解决方案文件：ChaoticKit.sln" -Level Success
                }

                # ----- 6.3 生成溯源文件 -----
                $releaseInfoPath = Join-Path $stagingDir "RELEASE_INFO.md"
                New-ReleaseInfoFile -OutputPath $releaseInfoPath -VersionNumber $versionNumber -Scenario $scenario -SourceBranch $MainBranch -SourceCommitHash $sourceCommitHash -SourceCommitMessage $sourceCommitMessage -Projects $copiedProjects
                Write-Log "已创建溯源文件：RELEASE_INFO.md" -Level Success

                # ----- 6.4 用独立索引生成提交对象（不触碰工作区） -----
                $indexFile = Join-Path $tempDir "$scenario.index"
                $messageFile = Join-Path $tempDir "$scenario.commitmsg"
                $commitMessageText = "chore: release $versionNumber for $scenario`n`nBased on: $MainBranch @ $sourceCommitShort`nSource commit: $sourceCommitHash`nSource message: $sourceCommitMessage"
                [System.IO.File]::WriteAllText($messageFile, $commitMessageText, (New-Object System.Text.UTF8Encoding($false)))

                Write-Log "创建 Orphan 分支：$branchName" -Level Info

                $oldIndex = $env:GIT_INDEX_FILE
                $oldGitDir = $env:GIT_DIR
                $oldWorkTree = $env:GIT_WORK_TREE

                try {
                    # 独立索引 + 临时工作区：完全不使用主仓库索引与工作区
                    $env:GIT_INDEX_FILE = $indexFile
                    $env:GIT_DIR = $gitDir
                    $env:GIT_WORK_TREE = $stagingDir

                    # git -C 仅用于定位仓库，同时由上面的环境变量指向临时目录
                    $addOutput = (git -C $stagingDir add -A -v -- . 2>&1)
                    if ($LASTEXITCODE -ne 0) {
                        throw "暂存发布内容失败：$addOutput"
                    }

                    $treeHash = (git write-tree 2>&1)
                    if ($LASTEXITCODE -ne 0) {
                        throw "生成树对象失败：$treeHash"
                    }
                    $treeHash = $treeHash.Trim()

                    # 校验：树内容必须与预期完全一致，防止任何内容混入
                    $treeNames = @(git ls-tree -r --name-only $treeHash 2>&1)
                    if ($LASTEXITCODE -ne 0) {
                        throw "读取树对象失败"
                    }

                    $treeTopLevel = @($treeNames | ForEach-Object { ($_ -split '/')[0] } | Sort-Object -Unique)
                    $expectedTopLevel = @($copiedProjects + $RootFilesToCopy + @("ChaoticKit.sln", "RELEASE_INFO.md") | Sort-Object -Unique)
                    $unexpected = @($treeTopLevel | Where-Object { $expectedTopLevel -notcontains $_ })

                    if ($unexpected.Count -gt 0) {
                        throw "发布内容校验失败，出现预期外的顶层条目：$($unexpected -join ', ')"
                    }

                    Write-Log "发布内容校验通过（$($treeNames.Count) 个文件，$($treeTopLevel.Count) 个顶层条目）" -Level Success

                    # 生成无父提交的提交对象（等价于 Orphan 分支）
                    $newCommit = (git commit-tree $treeHash -F $messageFile 2>&1)
                    if ($LASTEXITCODE -ne 0) {
                        throw "生成提交对象失败：$newCommit"
                    }
                    $newCommit = ($newCommit | Select-Object -First 1).Trim()
                }
                finally {
                    $env:GIT_INDEX_FILE = $oldIndex
                    $env:GIT_DIR = $oldGitDir
                    $env:GIT_WORK_TREE = $oldWorkTree
                }

                # ----- 6.5 创建分支引用 -----
                git update-ref "refs/heads/$branchName" $newCommit 2>&1 | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    throw "创建分支引用失败：$branchName"
                }
                $createdBranches += $branchName

                Write-Log "成功创建分支：$branchName" -Level Success

                # ----- 6.6 可选：创建 Git Tag（HEAD 始终位于 $MainBranch） -----
                if ($CreateTag) {
                    Write-Log "创建 Git Tag..." -Level Info

                    if (Test-TagExists -TagName $branchName) {
                        Write-Log "Tag 已存在，跳过：$branchName" -Level Warning
                    }
                    else {
                        $projectListText = ($copiedProjects | ForEach-Object { "  - $_" }) -join "`n"
                        $tagMessage = @"
Release $versionNumber for scenario: $scenario

Source:
  Branch: $MainBranch
  Commit: $sourceCommitHash
  Message: $sourceCommitMessage

Projects:
$projectListText

Published by: $env:USERNAME
Published at: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@
                        git tag -a $branchName -m $tagMessage 2>&1 | Out-Null
                        if ($LASTEXITCODE -eq 0) {
                            $createdTags += $branchName
                            Write-Log "已创建 Tag: $branchName" -Level Success
                        }
                        else {
                            Write-Log "创建 Tag 失败：$branchName" -Level Warning
                        }
                    }
                }

                $successCount++
            }
            catch {
                Write-Log "创建分支失败：$_" -Level Error
                $failedScenarios += "$scenario (创建失败)"

                # 仅回收本次真正创建的引用
                if ($newCommit) {
                    git update-ref -d "refs/heads/$branchName" 2>&1 | Out-Null
                    $createdBranches = @($createdBranches | Where-Object { $_ -ne $branchName })
                }
            }
            finally {
                # 清理场景临时目录（索引文件与提交信息文件都在其中）
                $scenarioStaging = Join-Path $tempDir $scenario
                if (Test-Path $scenarioStaging) {
                    Remove-Item -Path $scenarioStaging -Recurse -Force -ErrorAction SilentlyContinue
                }
            }
        }

        # ---------- 7. 清理临时目录 ----------
        Write-Log "清理临时目录..." -Level Info
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        if (Test-Path $tempDir) {
            Write-Log "临时目录清理失败（可能被占用）：$tempDir" -Level Warning
        }
        else {
            Write-Log "临时目录已清理" -Level Success
        }

        # ---------- 8. 输出总结 ----------
        Write-Log "========================================" -Level Info
        Write-Log "执行完成" -Level Info
        Write-Log "========================================" -Level Info
        Write-Log "版本号：$versionNumber" -Level Info
        Write-Log "成功场景数：$successCount / $($ScenarioConfig.Count)" -Level Info

        if ($failedScenarios.Count -gt 0) {
            Write-Log "失败/跳过的场景：" -Level Warning
            foreach ($failed in $failedScenarios) {
                Write-Log "  - $failed" -Level Warning
            }
        }

        Write-Log "当前分支仍为 $currentBranch，工作区未被修改" -Level Info
        Write-Log "提示：所有 Release 分支仅在本地创建，未推送到远程仓库" -Level Info
        Write-Log "请检查分支内容后，手动执行 git push 推送到远程" -Level Info

        if ($createdTags.Count -gt 0) {
            Write-Log "已创建 Tag（需单独推送）：$($createdTags -join ', ')" -Level Info
        }

        return ($successCount -gt 0 -and $failedScenarios.Count -eq 0)
    }
    catch {
        Write-Log "脚本执行异常：$_" -Level Error
        Write-Log $_.ScriptStackTrace -Level Error

        # 回滚本次执行中已创建的引用，保持仓库状态可预期
        foreach ($branch in $createdBranches) {
            Write-Log "回滚分支：$branch" -Level Warning
            git update-ref -d "refs/heads/$branch" 2>&1 | Out-Null
        }
        foreach ($tag in $createdTags) {
            Write-Log "回滚 Tag：$tag" -Level Warning
            git tag -d $tag 2>&1 | Out-Null
        }

        if ($tempDir -and (Test-Path $tempDir)) {
            Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        }

        return $false
    }
}
