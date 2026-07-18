# mahjong

人間 vs CPU AI の日本式リーチ麻雀ゲーム。対局開始時に **二人麻雀** / **四人麻雀** を選択できる構成を目指しています。

## 技術スタック

- C# / .NET 10
- UI: WPF（MVVM パターン、予定）
- AI: C# で直接実装（Python/Rust 等の外部プロセスは使用しない）

## 現在の状態

開発初期段階です。現時点で実装済みなのは以下のみです。

- `Mahjong.Core`: 麻雀牌（`Tile` / `TileSuit`）のドメインモデル
- `Mahjong.Tests`: `Mahjong.Core` に対する単体テスト（xUnit）

牌山生成・配牌・手牌管理・和了判定・役/得点計算・AI・WPF UI 等はこれから実装します。詳細な設計方針は [CLAUDE.md](CLAUDE.md) を参照してください。

## ビルド・テスト

```bash
dotnet build Mahjong.slnx --configuration Release
dotnet test Mahjong.Tests/Mahjong.Tests.csproj --configuration Release
```

## CI

`main` への push / PR 時に GitHub Actions（`.github/workflows/ci.yml`）が以下を実行します。

- Linux / Windows でのビルド・テスト
- `dotnet format` によるフォーマットチェック
- Gitleaks によるシークレット検出
- 週次の NuGet 脆弱パッケージ監査

## ライセンス

[MIT License](LICENSE)
