# Changelog

## 1.2.0 — 2026-10-25

- Recommend Cities: Skylines II **1.6.*** in platform metadata; compilation still uses installed build 1.6.2f1.
- Provide player-facing Paradox descriptions in English, Japanese and Simplified Chinese.
- Use the supplied TrainReDoor.png as the platform thumbnail and include the identical PNG in the runtime package.
- Update release/assembly identifiers to 1.2.0 / 1.2.0.0. Door control behaviour is unchanged.
- Retain platform Mod ID 164831 and Unlisted visibility. Gameplay verification is tracked separately.

日本語：推奨ゲーム版、三言語のプレイヤー向け説明、プレビュー画像を更新しました。ドア制御の動作は変更していません。
中文：更新推荐游戏版本、三语玩家说明及预览图，车门控制逻辑保持不变。


## Source review preparation — 2026-10-28

- Retain runtime 1.2.0 / assembly 1.2.0.0 without runtime source changes.
- Align the English, Japanese and Chinese main documents and correct publication-status wording.
- Prepare a private GitHub source tree; exclude toolchain Library/Temp/PID/cache outputs.
- Preserve the previously delivered packages and PDX publication separately.

日本語：実行コードを変更せず、三言語の主文書と公開記録の表記を整理し、生成物を除いた非公開ソースのレビュー用コピーを準備しました。
中文：运行时代码不变，整理三语主文档和发布状态表述，准备排除生成物的私有源码审阅副本。
