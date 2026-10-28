# 開発と公開

[English: Build](BUILDING.md) · [English: Publish](PUBLISHING.md) · [简体中文](PUBLISHING.zh-CN.md)

## ビルドとテスト

Windows で公式 Modding Toolchain と .NET SDK を用意し、リポジトリ直下から実行します。

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Validate.ps1
./scripts/Package.ps1
```

公式 Mod.props/Mod.targets、ソースジェネレーター、Mod Post Processor を維持します。通常の処理は artifacts 内に出力し、ゲームにインストールしたりアップロードしたりしません。必要に応じて ManagedPath/ToolPath を引数で指定し、個人の絶対パスやゲーム DLL をソースに含めません。

実行用 ZIP、ソース ZIP、GitHub 用ソース、Paradox 用資料と内容、チェックサムが生成されます。既存の出力先は上書きせず、再梱包には新しい OutputDirectory を使います。bin、obj、Library、Temp、PID／キャッシュはソースから除外します。

## 検証状況

1.2.0 の記録には公式ビルド、73 項目のプロトコル／DLL 検査、4 合成ケースの16ボーン、設定解析が含まれます。合成入力はゲーム内動作やスキニングの検証ではありません。ゲーム内乗降と購読版の動作検証は未記録です。推奨 1.6.* は全パッチのテスト済みという意味ではありません。

2026-10-25 のローカルの公開記録は、Mod ID 164831 の 1.2.0 が当時 Unlisted として更新されたことを示します。現在の遠隔可視性を再確認した記録ではありません。

## Paradox

既存 ID 164831 を保持し、パッケージ更新は NewVersion、資料のみの更新は Update を使います。Publish は新規条目用です。通常の publish ターゲットは EnableRemotePublish=true を明示しない限り停止します。具体的なコマンドは[英語の公開手順](PUBLISHING.md)にあります。認証情報はソースやコマンドラインに保存しません。

## GitHub

ソースリポジトリは **Private** とします。最初の push 前に Markdown と全テキスト／ソースをレビューし、SHA-256 マニフェストで対象を固定します。生成物、私的な履歴、ゲームバイナリ、認証情報を含めません。英語 README を主とし、日本語、中国語の順にリンクします。MIT ライセンスを保持します。

ソースコメントも英語を先、日本語を次に記載します。通常のビルドはリポジトリ作成・push・Release 公開を行いません。
