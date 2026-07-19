namespace Mahjong.Core.Domain;

/// <summary>標準形（4面子+雀頭）のシャンテン数計算を行う。門前13枚のみ対象（副露・七対子・国士無双は対象外）。</summary>
public static class ShantenCalculator
{
	private const int KindCount = 34;
	private const int RequiredMelds = 4;

	/// <summary>門前13枚のシャンテン数を計算する（標準形・七対子・国士無双のうち最小の値）。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が13枚でない場合。</exception>
	public static int CalculateShanten(IReadOnlyList<Tile> tiles) => Math.Min(
		CalculateStandardFormShanten(tiles),
		Math.Min(CalculateSevenPairsShanten(tiles), CalculateThirteenOrphansShanten(tiles)));

	/// <summary>門前13枚の標準形シャンテン数を計算する（0=聴牌、最大8）。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が13枚でない場合。</exception>
	public static int CalculateStandardFormShanten(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 13)
		{
			throw new ArgumentException($"シャンテン数計算の対象は13枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		var best = SearchBestDecomposition(counts, kind: 0, melds: 0, taatsu: 0, hasPair: false);

		for (var kind = 0; kind < KindCount; kind++)
		{
			if (counts[kind] < 2)
			{
				continue;
			}

			counts[kind] -= 2;
			var candidate = SearchBestDecomposition(counts, kind: 0, melds: 0, taatsu: 0, hasPair: true);
			counts[kind] += 2;

			best = Math.Min(best, candidate);
		}

		return best;
	}

	/// <summary>門前13枚の七対子シャンテン数を計算する（0=聴牌、最大6）。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が13枚でない場合。</exception>
	public static int CalculateSevenPairsShanten(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 13)
		{
			throw new ArgumentException($"シャンテン数計算の対象は13枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		var pairKindCount = 0;
		var distinctKindCount = 0;
		foreach (var count in counts)
		{
			if (count == 0)
			{
				continue;
			}

			distinctKindCount++;
			if (count >= 2)
			{
				pairKindCount++;
			}
		}

		return 6 - pairKindCount + Math.Max(0, 7 - distinctKindCount);
	}

	/// <summary>么九牌13種（萬子1/9・筒子1/9・索子1/9・字牌7種）に対応するインデックス集合。</summary>
	private static readonly int[] YaochuuIndices = [0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33];

	/// <summary>門前13枚の国士無双シャンテン数を計算する（0=聴牌、最大13）。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が13枚でない場合。</exception>
	public static int CalculateThirteenOrphansShanten(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 13)
		{
			throw new ArgumentException($"シャンテン数計算の対象は13枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		var distinctYaochuuCount = 0;
		var hasYaochuuPair = false;
		foreach (var kind in YaochuuIndices)
		{
			if (counts[kind] == 0)
			{
				continue;
			}

			distinctYaochuuCount++;
			if (counts[kind] >= 2)
			{
				hasYaochuuPair = true;
			}
		}

		return 13 - distinctYaochuuCount - (hasYaochuuPair ? 1 : 0);
	}

	/// <summary>
	/// countsの残り牌から面子・搭子・浮き牌への割り当てを再帰的に試し、
	/// このノード以降で到達できる最小シャンテン数を返す。
	/// </summary>
	private static int SearchBestDecomposition(int[] counts, int kind, int melds, int taatsu, bool hasPair)
	{
		var best = ((RequiredMelds - melds) * 2) - taatsu - (hasPair ? 1 : 0);

		if (melds + taatsu >= RequiredMelds)
		{
			return best;
		}

		while (kind < KindCount && counts[kind] == 0)
		{
			kind++;
		}

		if (kind >= KindCount)
		{
			return best;
		}

		var isNumberTile = kind < 27;
		var rankInSuit = kind % 9;

		if (counts[kind] >= 3)
		{
			counts[kind] -= 3;
			best = Math.Min(best, SearchBestDecomposition(counts, kind, melds + 1, taatsu, hasPair));
			counts[kind] += 3;
		}

		if (isNumberTile && rankInSuit <= 6 && counts[kind + 1] > 0 && counts[kind + 2] > 0)
		{
			counts[kind]--;
			counts[kind + 1]--;
			counts[kind + 2]--;
			best = Math.Min(best, SearchBestDecomposition(counts, kind, melds + 1, taatsu, hasPair));
			counts[kind]++;
			counts[kind + 1]++;
			counts[kind + 2]++;
		}

		if (counts[kind] >= 2)
		{
			counts[kind] -= 2;
			best = Math.Min(best, SearchBestDecomposition(counts, kind, melds, taatsu + 1, hasPair));
			counts[kind] += 2;
		}

		if (isNumberTile && rankInSuit <= 7 && counts[kind + 1] > 0)
		{
			counts[kind]--;
			counts[kind + 1]--;
			best = Math.Min(best, SearchBestDecomposition(counts, kind, melds, taatsu + 1, hasPair));
			counts[kind]++;
			counts[kind + 1]++;
		}

		if (isNumberTile && rankInSuit <= 6 && counts[kind + 2] > 0)
		{
			counts[kind]--;
			counts[kind + 2]--;
			best = Math.Min(best, SearchBestDecomposition(counts, kind, melds, taatsu + 1, hasPair));
			counts[kind]++;
			counts[kind + 2]++;
		}

		counts[kind]--;
		best = Math.Min(best, SearchBestDecomposition(counts, kind, melds, taatsu, hasPair));
		counts[kind]++;

		return best;
	}

	/// <summary>牌の種類（Suit/Rank、赤ドラは区別しない）を0-33のインデックスに変換する。</summary>
	private static int ToIndex(Tile tile) => tile.Suit switch
	{
		TileSuit.Man => tile.Rank - 1,
		TileSuit.Pin => 9 + tile.Rank - 1,
		TileSuit.Sou => 18 + tile.Rank - 1,
		TileSuit.Honor => 27 + tile.Rank - 1,
		_ => throw new InvalidOperationException($"未知のSuitです: {tile.Suit}"),
	};
}
