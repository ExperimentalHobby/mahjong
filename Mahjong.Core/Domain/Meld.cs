namespace Mahjong.Core.Domain;

/// <summary>鳴きによって手牌から公開された面子。</summary>
public sealed record Meld(MeldType Type, IReadOnlyList<Tile> Tiles);
