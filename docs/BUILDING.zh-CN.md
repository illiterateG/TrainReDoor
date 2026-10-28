# 构建、测试与本地包

## 环境

Windows、.NET SDK、游戏内安装并初始化的官方 Modding Toolchain。首次还原可能从 NuGet 获取 .NET Framework 引用程序集；不要求作者安装游戏 DLL 到源码仓库。

工程从 `CSII_TOOLPATH`（进程优先，其次用户环境）导入官方 Mod.props 与 Mod.targets，由其提供目标框架、源码生成器及 Mod Post Processor。环境不同请通过游戏设置里的 Modding 修复工具链；不要照抄本机 G/D 盘路径。当前本机编译环境游戏为 1.6.2f1、Unity 2022.3.62f2、Entities 1.3.10。这些是此次检查记录，不是永久版本要求。

在仓库根目录用 PowerShell 运行：

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Validate.ps1
./scripts/Package.ps1
```

路径不同可指定：

```powershell
./scripts/Build.ps1 -ManagedPath 'X:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2_Data\Managed' -ToolPath 'X:\ActualToolchain'
./scripts/Test.ps1 -ManagedPath 'X:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2_Data\Managed'
```

Build 保留官方后处理，不绕过源码生成器或替换后处理为复制。默认把结果暂存到 `artifacts/staging/TrainReDoor`，不改已安装插件。不运行发布目标就不会上传。

## 输出与复现

- `artifacts/staging/TrainReDoor/`：已后处理 DLL、默认 JSON、教程和示例。
- `artifacts/checks/`：实际构建、程序集测试、数据测试与验证日志。
- `artifacts/packages/TrainReDoor_1.2.0.zip`：玩家手动安装包，顶层为 TrainReDoor。
- `artifacts/packages/Paradox/content/TrainReDoor/`：官方发布器 `-c` 所指的平面内容目录，内层直接有 DLL。
- `artifacts/packages/GitHub/TrainReDoor/`：清理了 bin/obj 和私有历史的仓库文件。
- `artifacts/packages/TrainReDoor_1.2.0_Source.zip`：源码归档。
- `SHA256SUMS.txt`：两个 ZIP 的哈希。

Package 拒绝覆盖已存在输出目录。重新打包应使用新的相对输出目录，例如 `./scripts/Package.ps1 -OutputDirectory artifacts/packages-02`。正式交付版本保存在独立版本目录，保留原版本及其哈希。


官方后处理可能改变最终 DLL 字节，因此不要要求新 DLL 与旧直接编译 DLL 哈希相同。源码、发布版本和测试预期应保持一致。游戏/Unity/编译器 DLL 不随项目分发。

通用测试使用四份独立编写的合成案例，覆盖不同车宽、门组、行程和主体/玻璃配对；没有导入模型或执行游戏运营。本版主文档为英文，并提供日文接入与开发说明。

源码归档同时排除工具链生成的 Library／Temp、PID 和缓存文件；它们不是源码输入。
