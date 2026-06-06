# Git 分支方案说明

本文档说明 ChaoticKit 通用工具库项目的 Git 分支策略和工作流程。

## 设计原则

### 1. 单一真实源（Single Source of Truth）

所有开发、测试、完整的代码结构仅存在于开发主干分支（`dev/main`）。这是唯一的真实源，所有其他分支都从这里派生。

### 2. 任何变更 = 新版本

只要代码有任何改动，发布时必须生成新的版本号。版本号格式为 `yyyy.MMdd.HHmm`，例如：`2026.0604.1430`。

这种基于时间戳的版本号策略确保：
- 每个版本都有唯一标识
- 版本号具有时间含义，易于追溯
- 避免语义化版本号（SemVer）的主观判断

### 3. 场景拆分与剔除

通过自动化脚本，从主干分支中：
- 剥离测试代码
- 根据不同场景组合抽取特定的源码目录
- 生成只读的、无历史记录的发布分支（Orphan 分支）

### 4. 业务侧干净

业务项目通过 Git Submodule 引用发布分支，拉取下来的物理目录中只有：
- 必需的 `.csproj` 文件
- 源码文件
- 解决方案文件（`.sln`）

没有任何多余内容（测试项目、无关场景代码、Git 历史记录等）。

---

## 分支类型详解

### 1. 开发主干分支：`dev/main`

**用途**：日常开发的集成分支，包含完整的代码结构。

**特点**：
- 包含所有工具库项目
- 包含所有测试项目（`ChaoticKit.LibTest.*`）
- 包含完整的解决方案文件
- 包含开发所需的配置文件

**操作规则**：
- ✅ 可以直接提交小改动
- ✅ 接受来自 `feature/*` 和 `exp/*` 的合并
- ❌ 不允许强制推送（force push）
- ❌ 不允许直接删除或重置

### 2. 专项开发分支：`feature/*`

**命名格式**：`feature/<功能名称>`

**示例**：
- `feature/add-json-serializer`
- `feature/optimize-tree-structure`
- `feature/new-data-structures`

**用途**：开发工具库的特定功能或改进。

**工作流程**：
```
1. 从 dev/main 创建分支
   git checkout dev/main
   git checkout -b feature/add-json-serializer

2. 开发功能（包含单元测试）
   编写代码 -> 编写测试 -> 验证通过

3. 合并回 dev/main
   git checkout dev/main
   git merge feature/add-json-serializer

4. 删除功能分支（可选）
   git branch -d feature/add-json-serializer
```

**特点**：
- 包含测试项目
- 可以多次提交
- 合并前应确保测试通过

### 3. 业务驱动开发分支：`exp/*`

**命名格式**：`exp/<业务项目名>-<描述>`

**示例**：
- `exp/project-alpha-fix-bug`
- `exp/project-beta-add-helper`
- `exp/demo-refactor-api`

**用途**：在业务项目开发过程中，发现需要修改或扩展工具库时使用。

**工作流程**：
```
1. 在业务项目开发中，发现需要修改工具库
   切换到工具库仓库

2. 从 dev/main 创建 exp 分支
   git checkout dev/main
   git checkout -b exp/project-alpha-fix-bug

3. 一边写业务代码，一边改工具库
   在工具库分支中修改 -> 在业务项目中测试

4. 业务功能完成后，合并回 dev/main
   git checkout dev/main
   git merge exp/project-alpha-fix-bug

5. 发布新版本
   执行发布脚本 -> 生成新的 Release 分支

6. 业务项目更新 Submodule
   git submodule update --remote
```

**特点**：
- 快速响应业务需求
- 可能包含临时性的修改
- 合并前应重构为通用方案

### 4. 发布分支：`release/<版本号>/<场景名>`

**命名格式**：`release/<yyyy.MMdd.HHmm>/<场景名>`

**示例**：
- `release/2026.0604.1430/core`
- `release/2026.0604.1430/winform`
- `release/2026.0605.1000/core`（第二天的新版本）

**用途**：为业务项目提供干净的工具库代码，通过 Git Submodule 引用。

**特点**：
- **自动生成**：由自动化脚本创建，人工绝不修改
- **Orphan 分支**：无 Git 历史记录，独立存在
- **只读分支**：不应直接修改
- **场景化**：仅包含特定场景所需的项目
- **剔除测试**：不包含任何测试项目

**场景配置**：

| 场景名 | 包含的项目 | 适用场景 |
|--------|-----------|---------|
| `core` | ChaoticKit, ChaoticKit.Data | 基础库，无 UI 依赖 |
| `winform` | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Winform | Winform 应用开发 |
| `wpf` | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Wpf | WPF 应用开发 |
| `excel-npoi` | ChaoticKit, ChaoticKit.Data, ChaoticKit.Excel.NPOI | Excel 处理（无 UI） |
| `excel-npoi-gdi` | ChaoticKit, ChaoticKit.Data, ChaoticKit.GDI, ChaoticKit.Excel.NPOI, ChaoticKit.Excel.NPOI.GDI | Excel 处理（GDI 图表） |
| `all` | 所有库项目 | 完整工具库 |

**创建方式**：
```powershell
# 自动化脚本创建
.\.local\git\New-ReleaseBranch.ps1
```

**生命周期**：
- 创建后永久存在（不删除旧版本）
- 业务项目可随时切换到任意版本
- 新版本不会覆盖旧版本

---

## 完整工作流程示例

### 场景 1：日常维护小改动

```
1. 在 dev/main 分支直接修改
   git checkout dev/main
   # 修改代码
   git commit -m "fix: 修复某个 bug"

2. 发布新版本
   .\.local\git\New-ReleaseBranch.ps1

3. 推送到远程
   git push origin dev/main
   git push origin 'release/2026.0604.1430/*'
```

### 场景 2：开发新功能

```
1. 创建 feature 分支
   git checkout dev/main
   git checkout -b feature/add-xml-parser

2. 开发功能
   # 编写代码
   # 编写测试
   # 验证功能

3. 合并到主干
   git checkout dev/main
   git merge feature/add-xml-parser

4. 发布新版本
   .\.local\git\New-ReleaseBranch.ps1

5. 清理分支
   git branch -d feature/add-xml-parser
```

### 场景 3：业务驱动开发

```
1. 业务项目开发中发现需要修改工具库
   # 在业务项目中
   cd libs/ChaoticKit
   git checkout dev/main
   git checkout -b exp/myapp-add-helper

2. 修改工具库代码
   # 在工具库分支中修改
   # 在业务项目中测试

3. 合并回主干
   git checkout dev/main
   git merge exp/myapp-add-helper

4. 发布新版本
   .\.local\git\New-ReleaseBranch.ps1

5. 更新业务项目的 Submodule
   cd /path/to/business-project
   git submodule update --remote libs/ChaoticKit
   git add libs/ChaoticKit
   git commit -m "chore: 更新 ChaoticKit"
```

### 场景 4：业务项目引用工具库

```
1. 添加 Submodule
   git submodule add -b release/2026.0604.1430/core https://github.com/user/ChaoticKit.git libs/ChaoticKit

2. 初始化 Submodule
   git submodule init
   git submodule update

3. 后续更新
   git submodule update --remote libs/ChaoticKit
```

---

## 分支保护规则

### 必须保护的分支

- `dev/main`：开发主干，禁止强制推送，禁止删除

### 不应修改的分支

- `release/*`：发布分支，只读，不应直接修改

### 建议的分支保护配置（GitHub/GitLab）

```yaml
# dev/main 分支保护
branches:
  - name: dev/main
    protection:
      required_pull_request_reviews: false
      required_status_checks: false
      enforce_admins: false
      required_linear_history: false
      allow_force_pushes: false
      allow_deletions: false
```

---

## 版本号策略详解

### 格式说明

`yyyy.MMdd.HHmm`

- `yyyy`：四位年份（如 2026）
- `MM`：两位月份（01-12）
- `dd`：两位日期（01-31）
- `HH`：两位小时（00-23）
- `mm`：两位分钟（00-59）

### 示例

- `2026.0604.0930`：2026年6月4日 09:30 发布
- `2026.0604.1430`：2026年6月4日 14:30 发布（同一天第二次发布）
- `2026.0605.1000`：2026年6月5日 10:00 发布（第二天发布）

### 优势

1. **唯一性**：时间戳确保每个版本号唯一
2. **可读性**：一眼看出发布时间
3. **有序性**：时间顺序即版本顺序
4. **自动化**：无需人工决策版本号

### 注意事项

- 如果在同一分钟内多次发布，版本号会相同，脚本会检测到分支已存在并跳过
- 建议至少间隔 1 分钟再执行发布脚本

---

## 常见问题

### Q: 为什么不用语义化版本号（SemVer）？

A: 对于内部工具库，语义化版本号的主观性较强（什么是 major change？什么是 minor change？）。时间戳版本号更客观，且能直接反映发布时间。

### Q: 为什么发布分支用 Orphan 分支？

A: 
1. 无历史记录，业务项目克隆体积最小
2. 业务项目不会看到工具库的开发历史
3. 每个版本独立存在，互不影响

### Q: 如何回退到旧版本？

A: 业务项目中切换 Submodule 到旧版本的分支：
```bash
cd libs/ChaoticKit
git checkout release/2026.0603.1000/core
cd ../..
git add libs/ChaoticKit
git commit -m "chore: 回退 ChaoticKit 到旧版本"
```

### Q: 发布分支可以删除吗？

A: 技术上可以删除，但不建议。保留所有历史版本可以让业务项目随时回退。如果确实需要清理，可以删除很久以前的版本。

### Q: 如何查看所有发布版本？

```bash
# 查看本地所有 release 分支
git branch --list 'release/*'

# 查看远程所有 release 分支
git branch -r --list 'origin/release/*'

# 查看特定场景的所有版本
git branch --list 'release/*/core'
```

### Q: exp 分支和 feature 分支有什么区别？

A: 
- `feature/*`：主动开发工具库功能，有明确的工具库改进目标
- `exp/*`：被动响应业务需求，在业务开发过程中发现需要修改工具库

---

## 总结

本分支方案的核心思想：

1. **开发在主干**：所有开发活动最终都汇聚到 `dev/main`
2. **发布即分支**：每次发布生成独立的 Orphan 分支
3. **场景化发布**：根据使用场景提取不同组合的代码
4. **业务侧干净**：业务项目只拉取必需的代码，无多余内容

这种方案既保证了开发效率，又满足了业务项目的干净引用需求。
