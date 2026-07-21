namespace Mahjong.Core.Domain;

/// <summary>ドラ表示牌から実際のドラ牌を求め、手牌（副露・赤ドラ含む）に含まれる枚数を数える。</summary>
public static class DoraCalculator
{
	/// <summary>
	/// ドラ表示牌<paramref name="indicator"/>から実際のドラ牌を求める。数牌は次のランク（9の次は1）、
	/// 風牌は東→南→西→北→東、三元牌は白→發→中→白の順に巡回する。
	/// </summary>
	public static Tile GetDoraTile(Tile indicator)
	{
		if (indicator.Suit != TileSuit.Honor)
		{
			var nextRank = indicator.Rank == 9 ? 1 : indicator.Rank + 1;
			return new Tile(indicator.Suit, nextRank);
		}

		if (indicator.Rank <= 4)
		{
			var nextWindRank = indicator.Rank == 4 ? 1 : indicator.Rank + 1;
			return new Tile(TileSuit.Honor, nextWindRank);
		}

		var nextDragonRank = indicator.Rank == 7 ? 5 : indicator.Rank + 1;
		return new Tile(TileSuit.Honor, nextDragonRank);
	}

	/// <summary>
	/// <paramref name="concealedTiles"/>と<paramref name="melds"/>の中に、<paramref name="doraIndicators"/>から
	/// 求まるドラ牌が何枚あるかを数える（表ドラ・裏ドラ共通で利用する）。
	/// </summary>
	public static int CountDora(
		IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, IReadOnlyList<Tile> doraIndicators)
	{
		var doraTiles = doraIndicators.Select(GetDoraTile).ToArray();
		var allTiles = concealedTiles.Concat(melds.SelectMany(meld => meld.Tiles));
		return allTiles.Count(tile => doraTiles.Any(dora => dora.Suit == tile.Suit && dora.Rank == tile.Rank));
	}

	/// <summary>
	/// <paramref name="concealedTiles"/>と<paramref name="melds"/>の中の赤ドラ（<see cref="Tile.IsRedFive"/>）の枚数を数える。
	/// </summary>
	public static int CountAkaDora(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds) =>
		concealedTiles.Concat(melds.SelectMany(meld => meld.Tiles)).Count(tile => tile.IsRedFive);
}
