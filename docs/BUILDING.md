# Build and test

[日本語](DEVELOPMENT.ja.md) · [简体中文](BUILDING.zh-CN.md)

Install the official in-game Modding Toolchain and a .NET SDK on Windows. Repair/update the toolchain from the game's Modding settings when its environment paths are missing. The project imports official Mod.props/Mod.targets through CSII_TOOLPATH and keeps source generation and post-processing enabled.

From the repository root in PowerShell:

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Validate.ps1
./scripts/Package.ps1
```

Optional machine-specific paths can be provided with `Build.ps1 -ManagedPath <Cities2_Data/Managed> -ToolPath <toolchain directory>` and `Test.ps1 -ManagedPath <Cities2_Data/Managed>`. Do not put local machine paths or game DLLs in the public repository. Initial restore may download .NET Framework reference assemblies.

The build stages to `artifacts/staging/TrainReDoor` and does not replace an installed mod. Tests load that actual DLL, validate the protocol/configuration and exercise independently authored synthetic train cases. These fixtures are not recorded model data and do not establish import, skinning or gameplay correctness.

Package requires tests matching the staged DLL and completed local validation. It refuses an existing destination; use a fresh `-OutputDirectory artifacts/packages-02` for another local package. Outputs include a runtime ZIP, source ZIP, clean GitHub tree, a flat Paradox content directory with separate Properties metadata, and SHA256SUMS.txt.

The current compile environment uses game 1.6.2f1, Unity 2022.3.62f2 and Entities 1.3.10. These are build records, not permanent version requirements or compatibility promises. Test and document gameplay separately. Read [publishing](PUBLISHING.md) before any remote action.

Source packages exclude toolchain-generated Library/Temp directories and PID/cache files in addition to bin/obj/artifacts. These files are not source inputs.
