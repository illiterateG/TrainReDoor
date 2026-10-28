# Install

1. Exit the game and download the runtime ZIP attached to a release. The automatic GitHub source archive is not an installer.
2. Find the actual game user-data directory. The usual Windows default is `%USERPROFILE%/AppData/LocalLow/Colossal Order/Cities Skylines II/`; launch settings can override it.
3. Extract the top-level TrainReDoor folder under Mods, so the DLL is at `Mods/TrainReDoor/TrainReDoor.dll` and the JSON is beside it. Do not add an extra nesting level or modify the game's Managed directory.
4. Enable one controller copy and a compatible train in the active playset. Avoid overlapping door controllers. Test on a real line.

The Loaded log prints the configuration path actually used. Configuration is read on load; restart after edits. Back up manual JSON edits before an update, as adjacent configuration may be replaced. Automatic bone naming is preferable for published assets.

To uninstall, exit and remove the local copy from all scanned mod locations, or disable the subscribed copy. The mod changes visual bone poses and does not require editing game files. A static preview has no boarding signals.

日本語：ゲーム終了後、実行用 ZIP の TrainReDoor フォルダーを実際のユーザーデータ内の Mods に配置し、対応車両と一つのコントローラーを有効にします。設定変更後は再起動してください。設定パスは Loaded ログで確認できます。
