# VirtualFileSystem 虚拟文件系统 — 现状总结

## 一、概述

在 ChaoticKit 系列库中新增的"虚拟文件系统"（VirtualFileSystem），统一封装文件存储底层差异（本机文件系统、Windows 共享目录、FTP 等），为 Missy 项目后续的图片存储统一 Service 提供底层存储抽象。

当前状态：**编码完成，AI 验证测试通过（本地文件系统实现），未经过实际业务场景验证；人工清点确认接口定义的大方向没有问题。**

## 二、项目与文件结构

| 项目 | 说明 |
|---|---|
| `Common\ChaoticKit.VirtualFileSystem\` | 核心项目：全部接口 + 默认实现（本机文件系统、工厂、操作器、兜底传输） |
| `Common\ChaoticKit.VirtualFileSystem.FluentFTP\` | FTP 扩展：基于 FluentFTP 实现 |
| `Common\ChaoticKit.VirtualFileSystem.WindowsShare\` | Windows 共享目录扩展：P/Invoke 凭据挂载 |

前置重构（已并入 ChaoticKit 核心项目）：
- `IOperationResult` 系列 13 个文件从 `ChaoticKit.Data` 迁入 `ChaoticKit\Data\Struct\` 等目录（命名空间不变，迁移无损）
- 新增 `ChaoticKit\Data\README.md`：说明该目录存放轻量级类型；重量级内容新建独立 `ChaoticKit.*` 项目

## 三、核心接口（命名空间 `ChaoticKit.VirtualFileSystem`）

- **`IVirtualFileSystemDescriptor`**：描述契约 — `FileSystemType`（类型：Local/FTP/WindowsShare）+ `Source`（来源，用于调试）
- **`IVirtualFile`**：`Name` + `Directory`（所属目录条目）+ `FullPath`（仅显示/调试）+ 描述
- **`IVirtualDirectory`**：`Name` + `Paths`（通用路径段数组，与实现无关）+ `FullPath` + 描述
- **`VirtualFilePath`**：路径关系 — 可选 `RelativeSource`（`IVirtualDirectory`）+ `PathSegments`（查找路径段）
- **`IVirtualFileSystemProvider`**：单实现操作接口（读写删查、目录操作、枚举；`GetFileAsync`/`GetDirectoryAsync` 接收 `VirtualFilePath`，其余方法全部接收条目）
- **`IVirtualFileSystemTransfer`**：多目录操作接口（`MoveFileAsync`/`MoveDirectoryAsync`/`CopyFileAsync`/`CopyDirectoryAsync`）
- **`IVirtualFileSystemOperator`**：操作器（继承 Provider + Transfer；统一事件 `OperationInvoking`/`OperationInvoked`，用 `VirtualFileSystemOperation` 枚举区分操作，含 `CloseStream`）
- **`IVirtualFileSystem`**：工厂（注册/注销 Provider 与 Transfer、`GetTransfer` 未注册返回兜底、`CreateOperator`）
- **连接信息接口**（由条目承载，需要时类型转换获取）：
  - `IFtpConnectionInfo`（Host/Port/UserName/Password/RootPath）
  - `IHttpConnectionInfo`（BaseUri，预留 HTTP 文件系统）
  - `IWindowsShareConnectionInfo`（DeviceHost/ShareRootPath/Credential，定义于 WindowsShare 项目）
- **`VirtualPathHelper`**：路径段校验/规范化（段内拒绝 `/`；`.` 忽略；`..` 向上回溯）
- **`FileSystemTypeConstants`**：类型常量（Local/FTP/WindowsShare）

## 四、默认实现（命名空间 `ChaoticKit.VirtualFileSystem.Default`）

| 类型 | 说明 |
|---|---|
| `VirtualFileSystemProviderBase` | 抽象基类：描述属性、`RunAsync` 异常包装（返回失败结果不抛异常）、路径段合并 |
| `LocalFileSystem` | 本机文件系统实现，无参无状态；根 = 程序所在磁盘根目录；路径段首段为盘符（如 "C:"）时从该盘根解析（支持 Windows 跨盘） |
| `LocalFile` / `LocalDirectory` | 本地条目；提供 `FromPath(path, source)` 工厂（相对路径按当前程序路径 `AppContext.BaseDirectory` 解析为绝对路径） |
| `VirtualFileSystem` | 工厂默认实现：Provider 字典 + Transfer 注册表 + 兜底 |
| `VirtualFileSystemOperator` | 操作器默认实现：按条目类型解析 Provider 委托、统一事件、包装流（`EventStream`）Dispose 触发 `CloseStream` |
| `FallbackTransfer` | 兜底传输：源 `OpenReadAsync` → 目标 `OpenWriteAsync` 流式串联；Move = Copy + 删源 |
| `EventStream` | 事件包装流 |

## 五、FTP 扩展（`ChaoticKit.VirtualFileSystem.FluentFTP`）

- **`FtpFileSystem`**：无参构造（可注入 `ITempFileManager`，未提供时随实例创建 `TempFileManagerOfHelper` 并随实例释放）；连接信息全部从条目的 `IFtpConnectionInfo` 获取；每次操作创建并释放连接
- **`OpenReadAsync`**：下载到**临时文件**（`ITempFile`）而非内存（避免大文件问题），下载完成后立即断开 FTP 连接，返回 `TempFileReadStream`（释放时删除临时文件）
- **`OpenWriteAsync`**：临时文件承载；远程文件存在时先下载到临时文件；提交（`Dispose`/`DisposeAsync`）时读取临时文件上传 FTP 并删除临时文件（`TempFileWriteStream`）
- **`FtpFile` / `FtpDirectory`**：条目（实现 `IFtpConnectionInfo`，`FtpFile` 通过所属 `FtpDirectory` 转发）
- **`TempFileReadStream` / `TempFileWriteStream`**：临时文件包装流（均实现 `IAsyncDisposable`）

## 六、Windows 共享目录扩展（`ChaoticKit.VirtualFileSystem.WindowsShare`）

- **`WindowsShareFileSystem`**：无参构造；连接信息从条目的 `IWindowsShareConnectionInfo` 获取；实现内部区分运行时系统 — Windows 上有凭据时 P/Invoke `WNetUseConnection` 建立临时连接（操作后 `WNetCancelConnection2` 断开），无凭据直接以系统凭据访问 UNC；非 Windows 返回失败结果
- **`WindowsShareFile` / `WindowsShareDirectory`**：条目（实现 `IWindowsShareConnectionInfo`）
- **`WindowsShareCredential`**：凭据 record（UserName/Password/Domain）
- **`DisconnectAction`**：断开释放器

## 七、测试与验证状态（`ChaoticKit.LibTest.Console\VirtualFileSystem\`）

| 测试 | 内容 | 状态 |
|---|---|---|
| `LocalFileSystem001` | 基本读写删查 | ✅ 通过 |
| `LocalFileSystem002` | 目录操作与枚举（RelativeSource 风格：手动创建目录条目作为相对源） | ✅ 通过 |
| `LocalFileSystem003` | 失败场景（返回失败结果而非抛异常）、`.` 忽略与 `..` 回溯 | ✅ 通过 |
| `Transfer001` | 工厂注册/注销、传输方案注册与解析（未注册返回兜底）、兜底拷贝/移动串联 | ✅ 通过 |
| `Operator001` | 操作器事件机制（OpenWrite/CloseStream/FileExists/DeleteFile 事件序列） | ✅ 通过 |
| `TestPathHelper` | 辅助：绝对路径 → 盘符 + 路径段数组 | — |

编译验证：`ChaoticKit`、`ChaoticKit.Data`、核心项目、FTP、WindowsShare、测试项目 **全部编译通过**。

## 八、验证边界与未验证事项

1. **AI 验证范围**：仅覆盖**本地文件系统实现**（读写删查、目录、失败场景、工厂/传输、事件机制）
2. **未经过实际业务场景验证**：尚未接入 Missy 图片存储统一 Service，未经过真实业务链路验证
3. **FTP 扩展**：代码完成、编译通过，但**未运行验证**（需要真实 FTP 服务器环境；临时文件读写、提交上传、`DisposeAsync` 行为待实测）
4. **WindowsShare 扩展**：代码完成、编译通过，但**未运行验证**（需要 Windows + 共享目录环境；`WNetUseConnection` P/Invoke 凭据挂载待实测）
5. **接口定义大方向**：已人工清点确认无问题（条目承载连接信息、Provider 无状态、工厂/操作器/传输分层、兜底机制、事件机制、路径段语义等）

## 九、后续计划

1. 在有环境时补充 FTP / WindowsShare 的实际运行验证与测试
2. 视需要补充 HTTP 文件系统扩展（预留 `IHttpConnectionInfo`）
