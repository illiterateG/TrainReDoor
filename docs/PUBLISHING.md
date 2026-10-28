# Publishing

[English](PUBLISHING.md) · [日本語](DEVELOPMENT.ja.md) · [简体中文](PUBLISHING.zh-CN.md)

## Existing Paradox listing

Use Mod ID **164831** for updates. The last local publication receipt records version **1.2.0** on **2026-10-25**, recommended game pattern **1.6.*** and **Unlisted** visibility. The source configuration retains Unlisted for future uploads. Do not create a duplicate listing for an update.

The three-language game description is written for players. Creator and development details belong in the repository guides. The supplied preview is a cover illustration; it is not evidence of an in-game test.

Ordinary Build/Test/Validate/Package commands only create local outputs. The project rejects remote publication unless EnableRemotePublish=true is explicitly supplied. When a maintainer intends to upload a new package, run the official build/tests first, then use the official PublishNewVersion profile or:

```powershell
Push-Location ./src/TrainReDoor
try {
    dotnet publish ./TrainReDoor.csproj -c Release -p:ModPublisherCommand=NewVersion -p:EnableRemotePublish=true
} finally { Pop-Location }
```

The command above uploads to Paradox; it is not a local packaging command. NewVersion uploads a new package for the existing ID. Update changes metadata only. Publish creates a new listing and should not be used for this existing mod. Keep account credentials outside source files, profiles and command lines.

For the prepared Paradox directory, Properties contains publication metadata and the thumbnail; content/TrainReDoor is the flat payload folder passed to the official publisher. After upload, verify the subscribed package separately before claiming gameplay compatibility.

## Private GitHub source repository

Create the repository with **Private** visibility. Upload only the reviewed source tree, not the workspace root or its private history. Keep the English README as the default; link Japanese next and Chinese third. Preserve the MIT license.

The repository includes source, build scripts, configuration examples, synthetic fixtures, documentation and preview images. Exclude bin, obj, artifacts, Library, Temp, PID/cache files, game/Unity/compiler binaries, credentials, local logs and model archives. Git ignore patterns and the source package cleanup cover these generated paths.

The maintainer reviews all Markdown and other text/source files before the first push. A local SHA-256 manifest identifies the exact reviewed files. Repository creation, push, releases and changes to visibility are separate remote actions; they are not performed by the normal build scripts.

If a GitHub release is later requested, tag the corresponding source commit, attach the runtime ZIP and checksums, and describe the actual validation scope. Source archives are for development, not player installation. A private repository and its releases should retain their intended visibility.
