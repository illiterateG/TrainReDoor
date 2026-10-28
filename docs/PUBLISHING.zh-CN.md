# 发布说明

[English](PUBLISHING.md) · [日本語](DEVELOPMENT.ja.md) · [简体中文](PUBLISHING.zh-CN.md)

## 已有 PDX 条目

更新沿用 Mod ID **164831**。本地最后一次发布记录为 **2026-10-25** 的 **1.2.0**，推荐游戏版本 **1.6.***，当时可见性 **Unlisted**。源码配置保留 Unlisted；更新不要新建重复条目。

游戏页面的三语说明仅面向玩家，作者和开发细节放在仓库教程中。指定封面是示意图，不是游戏验证证据。

普通 Build/Test/Validate/Package 只产生本地输出。官方发布目标默认禁用远端上传，只有显式 EnableRemotePublish=true 才能执行。维护者准备上传时，先构建和测试，再使用官方 PublishNewVersion profile 或：

```powershell
Push-Location ./src/TrainReDoor
try {
    dotnet publish ./TrainReDoor.csproj -c Release -p:ModPublisherCommand=NewVersion -p:EnableRemotePublish=true
} finally { Pop-Location }
```

这条命令会实际上传到 PDX，不是本地打包。NewVersion 上传已有条目的新包；Update 只改资料；Publish 用于创建条目，本模组更新不使用它。账号凭证不得放入源码、profile 或命令行。

准备好的 Paradox 目录中，Properties 存放资料和封面；content/TrainReDoor 是发布器使用的平面内容目录。上传后还需单独验收平台订阅包。

## GitHub 私有源码仓库

仓库可见性设为 **Private**。仅上传经过审阅的源码树，不上传工作区根目录及其历史。默认 README 使用英文，随后链接日文和中文；保留 MIT 许可证。

包含源码、构建脚本、配置示例、合成案例、文档及预览图。排除 bin、obj、artifacts、Library、Temp、PID／缓存文件、游戏／Unity／编译器二进制、凭证、本地日志和模型档案。忽略规则及源码打包清理涵盖这些生成路径。

第一次推送前由维护者审阅所有 Markdown 和其他文本／源码文件，SHA-256 清单标识确切文件。创建仓库、推送、创建 Release 或改变可见性都是独立的远端操作，普通构建脚本不执行这些动作。

以后需要 GitHub Release 时，再为对应源码提交打标签，附上运行时 ZIP 和校验清单，并如实说明验证范围。源码归档用于开发，不是玩家安装包。私有仓库及其 Release 保持预定可见性。
