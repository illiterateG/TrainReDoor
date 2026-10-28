# Train ReDoor

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md)

A shared sliding-door controller for compatible trains in **Cities: Skylines II**. During boarding, doors follow the game's left/right platform signals. Train models are installed separately.

- Mod version: **1.2.0**; assembly: **1.2.0.0**.
- Recommended game versions: **1.6.***; recorded compilation build: **1.6.2f1**.
- [Paradox Mods — ID 164831](https://mods.paradoxplaza.com/mods/164831/Windows).

## Players

Add this mod and a train asset that supports Train ReDoor to the same playset, enable both, and restart the game after changing the playset. Run the train on a line to see boarding-side doors open and close.

For manual installation, use the runtime ZIP and place its TrainReDoor folder under the actual game user-data Mods directory. The source ZIP is for development. Use one controller copy and avoid overlapping door controllers. See [installation](docs/INSTALL.md).

Installing this controller does not add animated doors to every existing train. A compatible vehicle asset is required.

## Creators

Use the public ReDoor/TrainReDoor bone naming convention, or exact JSON mappings for existing bone names. Skin all moving door parts, including separate glass. Physical platform side is determined from the imported native X position.

[English creator guide](docs/CREATOR_GUIDE.en.md) · [日本語ガイド](docs/CREATOR_GUIDE.ja.md) · [中文接入教程](docs/CREATOR_GUIDE.zh-CN.md)

## Build and test

Install the official in-game Modding Toolchain and .NET SDK. From the repository root in Windows PowerShell:

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Validate.ps1
./scripts/Package.ps1
```

Official source generation and post-processing remain enabled. Ordinary builds stage inside the repository; they do not replace an installed mod or upload anything. See [build instructions](docs/BUILDING.md) and [publishing instructions](docs/PUBLISHING.md).

## Scope and validation

The controller supports translation-only sliding doors on eligible Train entities. It does not implement rotating doors, staged plug-door motion, sounds, collisions, departure interlocks or passenger routing. Static previews do not provide boarding signals. Per-asset JSON discovery is not implemented; one controller-side configuration is read at load time.

The local record confirms publication of 1.2.0 on 2026-10-25. Code/configuration tests and synthetic cases are recorded separately from gameplay. In-game boarding and subscribed-package verification have not been recorded. The recommended version pattern does not claim that every 1.6 patch was tested. See [validation status](docs/VALIDATION.md).

## Language and license

English is the primary documentation and source-comment language, followed by Japanese and Simplified Chinese. Meaningful source comments use English first and Japanese next. The project remains under the [MIT License](LICENSE). See [third-party notices](THIRD_PARTY_NOTICES.md).
