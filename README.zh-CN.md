# Train ReDoor

[English（主文档）](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md)

《都市：天际线 2》兼容列车共用的滑动门控制器，按游戏左右上下客信号开关车门。列车模型需要另外安装。

- 模组版本：**1.2.0**；程序集：**1.2.0.0**。
- 推荐游戏版本：**1.6.***；记录的编译环境：**1.6.2f1**。
- [Paradox Mods — ID 164831](https://mods.paradoxplaza.com/mods/164831/Windows)。

## 玩家

将本模组与支持 Train ReDoor 的列车加入同一播放集并启用，调整播放集后重启游戏。列车在线路上运营时，会在上下客期间开关站台侧的门。

手动安装使用运行时 ZIP，将 TrainReDoor 文件夹放到实际游戏用户数据的 Mods 目录；源码 ZIP 用于开发。只启用一份控制器，避免多个模组同时接管同一车门。[安装说明](docs/INSTALL.zh-CN.md)。仅安装本控制器不会让所有既有列车自动获得可动门。

## 创作者

使用公共 ReDoor/TrainReDoor 骨名规范，或通过 JSON 精确映射既有骨名。门板和独立玻璃等可动部件需要正确蒙皮；实际侧别按导入后的游戏原生 X 坐标判断。

[English](docs/CREATOR_GUIDE.en.md) · [日本語](docs/CREATOR_GUIDE.ja.md) · [中文接入教程](docs/CREATOR_GUIDE.zh-CN.md)

## 开发与验证

安装官方 Modding Toolchain 和 .NET SDK，依次运行 scripts/Build.ps1、Test.ps1、Validate.ps1、Package.ps1。普通构建只写工程内输出，不替换游戏安装，也不上传。[构建](docs/BUILDING.zh-CN.md) · [发布](docs/PUBLISHING.zh-CN.md)。

支持平移滑动门；不实现旋转、多阶段塞拉门、声音、碰撞、发车联锁或乘客寻路。静态预览没有上下客信号。当前只读取控制器旁的一份 JSON，不自动发现车包配置。

本地记录确认 1.2.0 已于 2026-10-25 上传成功。程序集与合成测试和游戏验证分别记录；尚无游戏内上下客及平台订阅版验收记录。推荐 1.6.* 不代表已测试所有 1.6 补丁。[验证状况](docs/VALIDATION.zh-CN.md)。

主文档和源码注释以英文为先，日文其次，中文第三。保留 [MIT 许可证](LICENSE)。
