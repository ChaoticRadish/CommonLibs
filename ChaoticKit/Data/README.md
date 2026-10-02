# ChaoticKit\Data 目录说明

> 本目录是 ChaoticKit 核心项目（`ChaoticKit.csproj`）内的 `Data` 目录，与独立的 `ChaoticKit.Data` 项目（`Common\ChaoticKit.Data\`）是两回事。

## 目录定位

本目录（`ChaoticKit\Data\`）用于存放**比较轻量级**的类型与实现，例如：

- 操作结果系列：`Struct\IOperationResult.cs`、`OperationResult.cs`、`OperationResultEx.cs`、`OperationResultHelper.cs` 等
- 小结构体：`Struct\IndexMask.cs`、`Unit.cs`、`MaybeNull.cs`、`NeedInitObject.cs`
- 相关的扩展方法与异常：`Extensions\`、`Exceptions\`

## 取舍原则

- **轻量级**（单文件、小结构体、通用小工具）→ 放入本目录，命名空间 `ChaoticKit.Data.*`
- **较重量级**的需求与实现内容（功能复杂、涉及多文件/多类型的完整子系统，如 VirtualFileSystem）→ **新建独立的 `ChaoticKit.*` 项目**承载（如 `Common\ChaoticKit.VirtualFileSystem\`），不要放入本目录，避免 ChaoticKit 核心项目膨胀

## 注意事项

- 本目录内的类型属于 `ChaoticKit` 程序集，命名空间 `ChaoticKit.Data.*` 与 `ChaoticKit.Data` 项目的同名命名空间并存；两者引用方向为 `ChaoticKit.Data` → `ChaoticKit`，**不得**在 ChaoticKit 核心项目反向引用 `ChaoticKit.Data`（会造成循环依赖）
