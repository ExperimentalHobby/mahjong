# mahjong

人間 vs CPU AI の日本式リーチ麻雀ゲーム。対局開始時に **二人麻雀** / **四人麻雀** を選択できる構成を目指しています。

## 技術スタック

- C# / .NET 10
- UI: WPF（MVVM パターン、予定）
- AI: C# で直接実装（Python/Rust 等の外部プロセスは使用しない）

## 現在の状態

開発初期段階です。四人麻雀のドメインモデルを中心に実装を進めています。

- `Mahjong.Core`: ドメインモデル
  - `Tile` / `TileSuit`: 麻雀牌（赤ドラ対応）
  - `Seat`: 四人麻雀の座席（東南西北）
  - `Wall`: 牌山（136枚生成・シャッフル・王牌分離・配牌）
  - `Hand`: 手牌（ツモ・打牌・捨て牌履歴、鳴き: ポン・チー・明槓・暗槓）
  - `Meld` / `MeldType`: 鳴きによって公開された面子
- `Mahjong.Tests`: 上記に対する単体テスト（xUnit、86件）

二人麻雀（ルール未確定のため対象外）・座席をまたぐ手番進行・加槓・和了判定・役/得点計算・AI・WPF UI 等はこれから実装します。詳細な設計方針は [CLAUDE.md](CLAUDE.md) を参照してください。

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
