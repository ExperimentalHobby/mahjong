namespace Mahjong.Core.Domain;

/// <summary>
/// 手牌の符（ふ）を計算する。点数表参照は対象外。面子由来の符・待ちの形による符が最大になる
/// 面子分解を採用する。
/// </summary>
public static class FuCalculator
{
	private const int KindCount = 34;

	/// <summary>門前手の符を計算する。</summary>
	/// <exception cref="ArgumentException">
	/// <paramref name="yaku"/>に役満が含まれる場合、または<paramref name="tiles"/>が和了形でない場合。
	/// </exception>
	public static int CalculateFu(
		IReadOnlyList<Tile> tiles, IReadOnlyList<Yaku> yaku, Seat seatWind, Seat roundWind, Tile? ronTile = null) =>
		CalculateFu(tiles, melds: [], yaku, seatWind, roundWind, ronTile);

	/// <summary>鳴き（副露）を含む手牌の符を計算する。</summary>
	/// <exception cref="ArgumentException">
	/// <paramref name="yaku"/>に役満が含まれる場合、または<paramref name="concealedTiles"/>と
	/// <paramref name="melds"/>を合わせた手牌が和了形でない場合。
	/// </exception>
	internal static int CalculateFu(
		IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, IReadOnlyList<Yaku> yaku,
		Seat seatWind, Seat roundWind, Tile? ronTile = null)
	{
		if (HanCalculator.IsYakuman(yaku))
		{
			throw new ArgumentException("役満は符の対象外です。", nameof(yaku));
		}

		var isComplete = melds.Count == 0
			? WinningHandChecker.IsComplete(concealedTiles)
			: WinningHandChecker.IsStandardFormComplete(concealedTiles, 4 - melds.Count);
		if (!isComplete)
		{
			throw new ArgumentException("判定対象は和了形である必要があります。", nameof(concealedTiles));
		}

		if (yaku.Contains(Yaku.Chiitoitsu))
		{
			return 25;
		}

		var isMenzen = melds.Count == 0;
		var isTsumo = ronTile is null;

		if (isTsumo && yaku.Contains(Yaku.Pinfu))
		{
			return 20;
		}

		var fu = 20;
		if (isMenzen && !isTsumo)
		{
			fu += 10;
		}

		if (isTsumo)
		{
			fu += 2;
		}

		fu += CalculateBestSetAndPairFu(concealedTiles, melds, seatWind, roundWind, ronTile);

		return RoundUpToTen(fu);
	}

	/// <summary>
	/// 雀頭候補ごとに残りの面子分解を列挙し、鳴きの面子・分解された面子・雀頭・待ちの形の符の合計が
	/// 最大になる値を返す。
	/// </summary>
	private static int CalculateBestSetAndPairFu(
		IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, Seat seatWind, Seat roundWind, Tile? ronTile)
	{
		var counts = new int[KindCount];
		foreach (var tile in concealedTiles)
		{
			counts[ToIndex(tile)]++;
		}

		var winningTile = ronTile ?? concealedTiles[^1];
		var meldFu = melds.Select(ToDecomposedSet).Sum(set => GetSetFu(set, ronTile: null));
		var requiredSets = 4 - melds.Count;
		var seatWindRank = ToWindRank(seatWind);
		var roundWindRank = ToWindRank(roundWind);

		var best = 0;
		var foundAny = false;
		for (var pairKind = 0; pairKind < KindCount; pairKind++)
		{
			if (counts[pairKind] < 2)
			{
				continue;
			}

			counts[pairKind] -= 2;
			foreach (var searched in EnumerateSetDecompositions(counts, requiredSets))
			{
				foundAny = true;
				var searchedFu = searched.Sum(set => GetSetFu(set, ronTile));
				var pairFu = GetPairFu(pairKind, seatWindRank, roundWindRank);
				var waitFu = GetWaitFu(pairKind, searched, winningTile);
				best = Math.Max(best, meldFu + searchedFu + pairFu + waitFu);
			}

			counts[pairKind] += 2;
		}

		return foundAny ? best : 0;
	}

	/// <summary>
	/// 待ちの形による符を返す。和了牌が雀頭候補<paramref name="pairKind"/>自身なら単騎待ちとして2符。
	/// それ以外は和了牌と同じスートの順子（刻子は対象外）を探し、嵌張・辺張（1-2-3の3待ち・
	/// 7-8-9の7待ち）なら2符、両面なら0符。該当する順子が複数あり得る場合は最大値を採用する
	/// （面子分解の探索と同じ「符が最大になる分解を採用する」方針に合わせるため）。刻子の一部として
	/// 消費されている（シャンポン等）場合は0符。
	/// </summary>
	private static int GetWaitFu(int pairKind, DecomposedSet[] decomposition, Tile winningTile)
	{
		if (pairKind == ToIndex(winningTile))
		{
			return 2;
		}

		var best = 0;
		foreach (var set in decomposition)
		{
			if (set.IsTriplet || set.Suit != winningTile.Suit)
			{
				continue;
			}

			var offset = winningTile.Rank - set.Rank;
			if (offset < 0 || offset > 2)
			{
				continue;
			}

			var isKanchanOrPenchan = offset == 1 || (offset == 0 && set.Rank == 7) || (offset == 2 && set.Rank == 1);
			best = Math.Max(best, isKanchanOrPenchan ? 2 : 0);
		}

		return best;
	}

	/// <summary>1つの面子の符を返す（順子は常に0）。</summary>
	private static int GetSetFu(DecomposedSet set, Tile? ronTile)
	{
		if (!set.IsTriplet)
		{
			return 0;
		}

		var isTerminalOrHonor = set.Suit == TileSuit.Honor || set.Rank == 1 || set.Rank == 9;
		var completedByRon = ronTile is { } tile && set.Suit == tile.Suit && set.Rank == tile.Rank;
		var isConcealed = set.IsConcealed && !completedByRon;

		return (isConcealed, set.IsKan, isTerminalOrHonor) switch
		{
			(false, false, false) => 2,
			(false, false, true) => 4,
			(true, false, false) => 4,
			(true, false, true) => 8,
			(false, true, false) => 8,
			(false, true, true) => 16,
			(true, true, false) => 16,
			(true, true, true) => 32,
		};
	}

	/// <summary>雀頭の符を返す（三元牌+2、自風+2、場風+2。連風牌は合計+4になる）。</summary>
	private static int GetPairFu(int pairKind, int seatWindRank, int roundWindRank)
	{
		var (suit, rank) = FromIndex(pairKind);
		if (suit != TileSuit.Honor)
		{
			return 0;
		}

		var fu = 0;
		if (rank is >= 5 and <= 7)
		{
			fu += 2;
		}

		if (rank == seatWindRank)
		{
			fu += 2;
		}

		if (rank == roundWindRank)
		{
			fu += 2;
		}

		return fu;
	}

	/// <summary>座席(自風・場風)を字牌のランクに変換する(東=1・南=2・西=3・北=4)。</summary>
	private static int ToWindRank(Seat seat) => seat switch
	{
		Seat.East => 1,
		Seat.South => 2,
		Seat.West => 3,
		Seat.North => 4,
		_ => throw new InvalidOperationException($"未知のSeatです: {seat}"),
	};

	/// <summary>
	/// countsを<paramref name="setsNeeded"/>個の面子（刻子・順子）に分解できるあらゆる組み合わせを列挙する。
	/// <see cref="YakuChecker"/>と同型のバックトラッキングだが、符計算専用に<see cref="DecomposedSet"/>を
	/// 独自に持つ（槓子/刻子の区別が符計算にのみ必要なため）。
	/// </summary>
	private static IEnumerable<DecomposedSet[]> EnumerateSetDecompositions(int[] counts, int setsNeeded)
	{
		if (setsNeeded == 0)
		{
			if (Array.TrueForAll(counts, count => count == 0))
			{
				yield return [];
			}

			yield break;
		}

		var kind = Array.FindIndex(counts, count => count > 0);
		if (kind < 0)
		{
			yield break;
		}

		if (counts[kind] >= 3)
		{
			counts[kind] -= 3;
			var (suit, rank) = FromIndex(kind);
			var set = new DecomposedSet(true, suit, rank, IsConcealed: true, IsKan: false);
			foreach (var rest in EnumerateSetDecompositions(counts, setsNeeded - 1))
			{
				yield return [set, .. rest];
			}

			counts[kind] += 3;
		}

		var isNumberTile = kind < 27;
		var rankInSuit = kind % 9;
		if (isNumberTile && rankInSuit <= 6 && counts[kind + 1] > 0 && counts[kind + 2] > 0)
		{
			counts[kind]--;
			counts[kind + 1]--;
			counts[kind + 2]--;
			var (suit, rank) = FromIndex(kind);
			var set = new DecomposedSet(false, suit, rank, IsConcealed: true, IsKan: false);
			foreach (var rest in EnumerateSetDecompositions(counts, setsNeeded - 1))
			{
				yield return [set, .. rest];
			}

			counts[kind]++;
			counts[kind + 1]++;
			counts[kind + 2]++;
		}
	}

	/// <summary>分解済みの1面子（刻子・槓子または順子）。順子はRankを開始ランクとする。</summary>
	private readonly record struct DecomposedSet(bool IsTriplet, TileSuit Suit, int Rank, bool IsConcealed = true, bool IsKan = false);

	/// <summary>鳴きの面子(<see cref="Meld"/>)を分解済みの1面子(<see cref="DecomposedSet"/>)に変換する。</summary>
	private static DecomposedSet ToDecomposedSet(Meld meld)
	{
		var first = meld.Tiles[0];
		var isConcealed = meld.Type == MeldType.ClosedKan;
		var isKan = meld.Type is MeldType.OpenKan or MeldType.ClosedKan or MeldType.AddedKan;
		return new DecomposedSet(meld.Type != MeldType.Chi, first.Suit, first.Rank, isConcealed, isKan);
	}

	/// <summary>符を10の位に切り上げる。</summary>
	private static int RoundUpToTen(int fu) => (fu + 9) / 10 * 10;

	/// <summary>牌の種類（Suit/Rank、赤ドラは区別しない）を0-33のインデックスに変換する。</summary>
	private static int ToIndex(Tile tile) => tile.Suit switch
	{
		TileSuit.Man => tile.Rank - 1,
		TileSuit.Pin => 9 + tile.Rank - 1,
		TileSuit.Sou => 18 + tile.Rank - 1,
		TileSuit.Honor => 27 + tile.Rank - 1,
		_ => throw new InvalidOperationException($"未知のSuitです: {tile.Suit}"),
	};

	/// <summary>0-33のインデックスを牌の種類（Suit/Rank）に変換する（<see cref="ToIndex"/>の逆変換）。</summary>
	private static (TileSuit Suit, int Rank) FromIndex(int kind) => kind switch
	{
		< 9 => (TileSuit.Man, kind + 1),
		< 18 => (TileSuit.Pin, kind - 9 + 1),
		< 27 => (TileSuit.Sou, kind - 18 + 1),
		_ => (TileSuit.Honor, kind - 27 + 1),
	};
}
