namespace Mahjong.Core.Domain;

/// <summary>標準形（4面子+雀頭）の和了判定を行う。七対子・国士無双等の特殊形、副露（鳴き）の考慮は対象外。</summary>
public static class WinningHandChecker
{
	private const int KindCount = 34;

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
