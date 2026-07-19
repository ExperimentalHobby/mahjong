namespace Mahjong.Core.Domain;

/// <summary>
/// 牌の内容のみで判定できる役（分解方法に依存しない役）を判定する。
/// 平和・一盃口・三色同順など面子の分解方法に依存する役、タイミング限定の役、翻数・得点計算は対象外。
/// </summary>
public static class YakuChecker
{
	/// <summary>門前14枚の和了形に成立している役を判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が和了形でない場合（14枚でない場合を含む）。</exception>
	public static IReadOnlyList<Yaku> DetermineYaku(IReadOnlyList<Tile> tiles)
	{
		if (!WinningHandChecker.IsComplete(tiles))
		{
			throw new ArgumentException("判定対象は和了形である必要があります。", nameof(tiles));
		}

		if (WinningHandChecker.IsThirteenOrphansComplete(tiles))
		{
			return [Yaku.Kokushi];
		}

		var yaku = new List<Yaku>();

		if (WinningHandChecker.IsSevenPairsComplete(tiles))
		{
			yaku.Add(Yaku.Chiitoitsu);
		}

		if (IsTanyao(tiles))
		{
			yaku.Add(Yaku.Tanyao);
		}

		if (IsTsuiisou(tiles))
		{
			yaku.Add(Yaku.Tsuiisou);
		}
		else if (IsChinitsu(tiles))
		{
			yaku.Add(Yaku.Chinitsu);
		}
		else if (IsHonitsu(tiles))
		{
			yaku.Add(Yaku.Honitsu);
		}

		return yaku;
	}

	private static bool IsTanyao(IReadOnlyList<Tile> tiles) =>
		tiles.All(tile => tile.Suit != TileSuit.Honor && tile.Rank != 1 && tile.Rank != 9);

	private static bool IsTsuiisou(IReadOnlyList<Tile> tiles) =>
		tiles.All(tile => tile.Suit == TileSuit.Honor);

	private static bool IsChinitsu(IReadOnlyList<Tile> tiles) =>
		!tiles.Any(tile => tile.Suit == TileSuit.Honor) &&
		tiles.Select(tile => tile.Suit).Distinct().Count() == 1;

	private static bool IsHonitsu(IReadOnlyList<Tile> tiles)
	{
		var numberSuitCount = tiles.Where(tile => tile.Suit != TileSuit.Honor).Select(tile => tile.Suit).Distinct().Count();
		var hasHonor = tiles.Any(tile => tile.Suit == TileSuit.Honor);
		return numberSuitCount == 1 && hasHonor;
	}
}
