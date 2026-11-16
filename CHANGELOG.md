# Changelog

## Documentation update — 2026-11-17

- Completed in-game acceptance and stability validation, as confirmed by the maintainer. Actual boarding and full platform-subscribed-package verification are complete.
- Align English, Japanese and Simplified Chinese documentation and release notes with the completed validation; remove outdated wording that described gameplay checks as pending.
- Clarify checks for new train assets and future regressions. Runtime version 1.2.0 / assembly 1.2.0.0 and door-control behaviour are unchanged.

日本語：ゲーム内の動作確認および安定性検証を完了し、メンテナーが確認済みです。実際の乗降動作と購読パッケージも検証済みです。三言語の文書と更新履歴から未検証・確認待ちの古い表現を修正しました。新規車両と今後の回帰確認の手順を整理し、実行コードとバージョンは変更していません。

中文：已经完成稳定性验证及游戏内验收，并经维护者确认，实际乘降和平台订阅包的完整验证均已完成。同步修改英、日、中三语文档和更新说明，清理“尚未验收／未执行”等过时措辞，明确新车包及后续版本的回归检查要求。运行时代码、版本号及车门控制逻辑不变。

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
