# ============================================================================
# Git 自动化发布脚本 - 共享核心逻辑
# 此脚本包含所有共享的函数和主逻辑，被其他脚本调用
# ============================================================================

# 设置控制台编码为 UTF-8，确保中文正确显示
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

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
    # 检查是否有未提交的变更
    $status = git status --porcelain 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Git 命令执行失败" -Level Error
        return $false
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

function Copy-ProjectDirectory {
    param(
        [string]$SourceDir,
        [string]$TargetDir
    )
    
    if (-not (Test-DirectoryExists $SourceDir)) {
        Write-Log "目录不存在，跳过：$SourceDir" -Level Warning
        return $false
    }
    
    # 保持目录结构复制
    if (-not (Test-Path $TargetDir)) {
        New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
    }
    
    Copy-Item -Path "$SourceDir\*" -Destination $TargetDir -Recurse -Force
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
    [System.IO.File]::WriteAllText($OutputPath, $slnContent, [System.Text.UTF8Encoding]::new($false))
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

    [System.IO.File]::WriteAllText($OutputPath, $content, [System.Text.UTF8Encoding]::new($false))
}

# ============================================================================
# 主逻辑函数
# ============================================================================

function Invoke-ReleaseBranchCreation {
    param(
        [hashtable]$ScenarioConfig,
        [string]$MainBranch = "dev/main",
        [string[]]$RootFilesToCopy = @(".gitignore", "LICENSE.txt"),
        [switch]$SkipCleanCheck,
        [switch]$CreateTag
    )
    
    try {
        Write-Log "========================================" -Level Info
        Write-Log "Git 自动化发布脚本启动" -Level Info
        Write-Log "========================================" -Level Info
        
        # 1. 检查工作区是否干净
        if (-not $SkipCleanCheck) {
            Write-Log "检查工作区状态..." -Level Info
            if (-not (Test-WorkingDirectoryClean)) {
                Write-Log "请先提交或暂存所有变更后再执行此脚本" -Level Error
                Write-Log "或使用 -SkipCleanCheck 参数跳过检查（危险操作）" -Level Warning
                return $false
            }
            Write-Log "工作区干净，继续执行" -Level Success
        }
        else {
            Write-Log "已跳过工作区干净检查" -Level Warning
        }
        
        # 2. 确认当前分支
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
        
        # 3. 生成版本号
        $versionNumber = New-VersionNumber
        Write-Log "生成版本号：$versionNumber" -Level Success
        
        # 4. 获取源提交信息（用于溯源）
        $sourceCommitHash = git rev-parse HEAD
        $sourceCommitShort = $sourceCommitHash.Substring(0, 7)
        $sourceCommitMessage = git log -1 --pretty=format:"%s"
        Write-Log "源提交：$sourceCommitShort - $sourceCommitMessage" -Level Info
        
        # 5. 获取仓库根目录
        $repoRoot = (git rev-parse --show-toplevel)
        if ($LASTEXITCODE -ne 0) {
            Write-Log "无法获取仓库根目录" -Level Error
            return $false
        }
        
        # 6. 创建临时工作目录
        $tempDir = Join-Path $repoRoot ".local\temp\release-$versionNumber"
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        Write-Log "创建临时工作目录：$tempDir" -Level Info
        
        # 6. 遍历所有场景配置
        $successCount = 0
        $failedScenarios = @()
        
        foreach ($scenario in $ScenarioConfig.Keys) {
            Write-Log "----------------------------------------" -Level Info
            Write-Log "处理场景：$scenario" -Level Info
            Write-Log "----------------------------------------" -Level Info
            
            $projects = $ScenarioConfig[$scenario]
            $branchName = "release/$versionNumber/$scenario"
            
            # 检查分支是否已存在
            $branchExists = git show-ref --verify --quiet "refs/heads/$branchName" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Log "分支已存在：$branchName，跳过" -Level Warning
                $failedScenarios += "$scenario (分支已存在)"
                continue
            }
            
            # 创建场景临时目录
            $scenarioTempDir = Join-Path $tempDir $scenario
            if (Test-Path $scenarioTempDir) {
                Remove-Item -Path $scenarioTempDir -Recurse -Force
            }
            New-Item -ItemType Directory -Path $scenarioTempDir -Force | Out-Null
            
            # 复制项目目录
            $copiedProjects = @()
            foreach ($project in $projects) {
                $sourcePath = Join-Path $repoRoot $project
                $targetPath = Join-Path $scenarioTempDir $project
                
                if (Copy-ProjectDirectory -SourceDir $sourcePath -TargetDir $targetPath) {
                    $copiedProjects += $project
                    Write-Log "已复制项目：$project" -Level Success
                }
            }
            
            if ($copiedProjects.Count -eq 0) {
                Write-Log "场景 $scenario 没有成功复制任何项目，跳过" -Level Warning
                $failedScenarios += "$scenario (无有效项目)"
                continue
            }
            
            # 复制根目录文件
            foreach ($rootFile in $RootFilesToCopy) {
                $sourceFile = Join-Path $repoRoot $rootFile
                if (Test-Path $sourceFile) {
                    $targetFile = Join-Path $scenarioTempDir $rootFile
                    Copy-Item -Path $sourceFile -Destination $targetFile -Force
                    Write-Log "已复制根文件：$rootFile" -Level Success
                }
            }
            
            # 生成解决方案文件
            $slnPath = Join-Path $scenarioTempDir "ChaoticKit.sln"
            New-SolutionFile -OutputPath $slnPath -ProjectNames $copiedProjects -VersionNumber $versionNumber -ScenarioName $scenario
            Write-Log "已生成解决方案文件：ChaoticKit.sln" -Level Success
            
            # 创建 Orphan 分支
            Write-Log "创建 Orphan 分支：$branchName" -Level Info
            
            # 保存当前状态
            $originalBranch = Get-CurrentBranch
            
            try {
                # 创建 orphan 分支
                git checkout --orphan $branchName 2>&1 | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    throw "创建 orphan 分支失败"
                }
                
                # 清空工作区
                git rm -rf . 2>&1 | Out-Null
                
                # 复制文件到工作区
                foreach ($project in $copiedProjects) {
                    $sourcePath = Join-Path $scenarioTempDir $project
                    $targetPath = Join-Path $repoRoot $project
                    
                    if (-not (Test-Path $targetPath)) {
                        New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
                    }
                    
                    Copy-Item -Path "$sourcePath\*" -Destination $targetPath -Recurse -Force
                }
                
                # 复制根文件
                foreach ($rootFile in $RootFilesToCopy) {
                    $sourceFile = Join-Path $scenarioTempDir $rootFile
                    if (Test-Path $sourceFile) {
                        $targetFile = Join-Path $repoRoot $rootFile
                        Copy-Item -Path $sourceFile -Destination $targetFile -Force
                    }
                }
                
                # 复制解决方案文件
                $slnSource = Join-Path $scenarioTempDir "ChaoticKit.sln"
                $slnTarget = Join-Path $repoRoot "ChaoticKit.sln"
                Copy-Item -Path $slnSource -Destination $slnTarget -Force
                
                # 创建 RELEASE_INFO.md 文件（溯源信息）
                $releaseInfoPath = Join-Path $repoRoot "RELEASE_INFO.md"
                New-ReleaseInfoFile -OutputPath $releaseInfoPath -VersionNumber $versionNumber -Scenario $scenario -SourceBranch $MainBranch -SourceCommitHash $sourceCommitHash -SourceCommitMessage $sourceCommitMessage -Projects $copiedProjects
                Write-Log "已创建溯源文件：RELEASE_INFO.md" -Level Success
                
                # 添加所有文件到 Git
                git add . 2>&1 | Out-Null
                
                # 提交（增强提交信息）
                $commitMessage = @"
chore: release $versionNumber for $scenario

Based on: $MainBranch @ $sourceCommitShort
Source commit: $sourceCommitHash
Source message: $sourceCommitMessage
"@
                git commit -m $commitMessage 2>&1 | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    throw "提交失败"
                }
                
                Write-Log "成功创建分支：$branchName" -Level Success
                
                # 可选：创建 Git Tag
                if ($CreateTag) {
                    Write-Log "创建 Git Tag..." -Level Info
                    
                    # 切回 dev/main 分支
                    git checkout $MainBranch 2>&1 | Out-Null
                    
                    # 构建项目列表
                    $projectList = $copiedProjects | ForEach-Object { "  - $_" }
                    $projectListText = $projectList -join "`n"
                    
                    # 创建 Tag
                    $tagName = "release/$versionNumber/$scenario"
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
                    
                    git tag -a $tagName -m $tagMessage 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        Write-Log "已创建 Tag: $tagName" -Level Success
                    }
                    else {
                        Write-Log "创建 Tag 失败" -Level Warning
                    }
                    
                    # 切回 Release 分支
                    git checkout $branchName 2>&1 | Out-Null
                }
                
                $successCount++
            }
            catch {
                Write-Log "创建分支失败：$_" -Level Error
                $failedScenarios += "$scenario (创建失败)"
                
                # 尝试清理失败的分支
                git checkout $originalBranch 2>&1 | Out-Null
                git branch -D $branchName 2>&1 | Out-Null
            }
            finally {
                # 切回原始分支
                git checkout $originalBranch 2>&1 | Out-Null
            }
        }
        
        # 7. 清理临时目录
        Write-Log "清理临时目录..." -Level Info
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Log "临时目录已清理" -Level Success
        
        # 8. 输出总结
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
        
        Write-Log "提示：所有 Release 分支仅在本地创建，未推送到远程仓库" -Level Info
        Write-Log "请检查分支内容后，手动执行 git push 推送到远程" -Level Info
        
        return $true
    }
    catch {
        Write-Log "脚本执行异常：$_" -Level Error
        Write-Log $_.ScriptStackTrace -Level Error
        
        # 尝试恢复原始分支
        if ($originalBranch) {
            Write-Log "尝试恢复原始分支..." -Level Warning
            git checkout $originalBranch 2>&1 | Out-Null
        }
        
        return $false
    }
}
