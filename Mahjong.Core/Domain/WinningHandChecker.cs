namespace Mahjong.Core.Domain;

/// <summary>標準形（4面子+雀頭）・七対子・国士無双の和了判定を行う。副露（鳴き）の考慮は対象外。</summary>
public static class WinningHandChecker
{
	private const int KindCount = 34;

	/// <summary>門前14枚が標準形・七対子・国士無双のいずれかとして完成しているかを判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が14枚でない場合。</exception>
	public static bool IsComplete(IReadOnlyList<Tile> tiles) =>
		IsStandardFormComplete(tiles) || IsSevenPairsComplete(tiles) || IsThirteenOrphansComplete(tiles);

	/// <summary>門前14枚が標準形（4面子+雀頭）として完成しているかを判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が14枚でない場合。</exception>
	public static bool IsStandardFormComplete(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 14)
		{
			throw new ArgumentException($"判定対象は14枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		for (var kind = 0; kind < KindCount; kind++)
		{
			if (counts[kind] < 2)
			{
				continue;
			}

			counts[kind] -= 2;
			var isComplete = CanFormSets(counts, 4);
			counts[kind] += 2;

			if (isComplete)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>門前14枚が七対子（異なる7種類の対子）として完成しているかを判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が14枚でない場合。</exception>
	public static bool IsSevenPairsComplete(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 14)
		{
			throw new ArgumentException($"判定対象は14枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		var pairKindCount = 0;
		foreach (var count in counts)
		{
			if (count == 0)
			{
				continue;
			}

			if (count != 2)
			{
				return false;
			}

			pairKindCount++;
		}

		return pairKindCount == 7;
	}

	/// <summary>么九牌13種（萬子1/9・筒子1/9・索子1/9・字牌7種）に対応するインデックス集合。</summary>
	private static readonly int[] YaochuuIndices = [0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33];

	/// <summary>門前14枚が国士無双（么九牌13種+その1種の対子）として完成しているかを判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が14枚でない場合。</exception>
	public static bool IsThirteenOrphansComplete(IReadOnlyList<Tile> tiles)
	{
		if (tiles.Count != 14)
		{
			throw new ArgumentException($"判定対象は14枚である必要があります(実際: {tiles.Count}枚)。", nameof(tiles));
		}

		var counts = new int[KindCount];
		foreach (var tile in tiles)
		{
			counts[ToIndex(tile)]++;
		}

		foreach (var kind in YaochuuIndices)
		{
			if (counts[kind] == 0)
			{
				return false;
			}
		}

		return YaochuuIndices.Sum(kind => counts[kind]) == 14;
	}

	/// <summary>残りの牌が<paramref name="setsNeeded"/>個の面子（刻子・順子）に分解できるかを判定する。</summary>
	private static bool CanFormSets(int[] counts, int setsNeeded)
	{
		if (setsNeeded == 0)
		{
			return Array.TrueForAll(counts, count => count == 0);
		}

		var kind = Array.FindIndex(counts, count => count > 0);
		if (kind < 0)
		{
			return false;
		}

		if (counts[kind] >= 3)
		{
			counts[kind] -= 3;
			var formed = CanFormSets(counts, setsNeeded - 1);
			counts[kind] += 3;
			if (formed)
			{
				return true;
			}
		}

		var isNumberTile = kind < 27;
		var rankInSuit = kind % 9;
		if (isNumberTile && rankInSuit <= 6 && counts[kind + 1] > 0 && counts[kind + 2] > 0)
		{
			counts[kind]--;
			counts[kind + 1]--;
			counts[kind + 2]--;
			var formed = CanFormSets(counts, setsNeeded - 1);
			counts[kind]++;
			counts[kind + 1]++;
			counts[kind + 2]++;
			if (formed)
			{
				return true;
			}
		}

		return false;
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
