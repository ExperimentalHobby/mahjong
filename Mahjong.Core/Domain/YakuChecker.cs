namespace Mahjong.Core.Domain;

/// <summary>
/// 面子の分解探索だけで判定できる役（役牌・三暗刻・平和など自風/場風・ロン/ツモの別・待ちの形といった
/// 追加コンテキストを必要とする役を除く）を判定する。タイミング限定の役、翻数・得点計算は対象外。
/// </summary>
public static class YakuChecker
{
	private const int KindCount = 34;

	/// <summary>門前14枚の和了形に成立している役を判定する。</summary>
	/// <exception cref="ArgumentException"><paramref name="tiles"/>が和了形でない場合（14枚でない場合を含む）。</exception>
	public static IReadOnlyList<Yaku> DetermineYaku(IReadOnlyList<Tile> tiles) => DetermineYaku(tiles, melds: []);

	/// <summary>鳴き（副露）を含む手牌の和了形に成立している役を判定する。</summary>
	/// <exception cref="ArgumentException">
	/// <paramref name="concealedTiles"/>と<paramref name="melds"/>を合わせた手牌が和了形でない場合。
	/// </exception>
	internal static IReadOnlyList<Yaku> DetermineYaku(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds)
	{
		var isComplete = melds.Count == 0
			? WinningHandChecker.IsComplete(concealedTiles)
			: WinningHandChecker.IsStandardFormComplete(concealedTiles, 4 - melds.Count);
		if (!isComplete)
		{
			throw new ArgumentException("判定対象は和了形である必要があります。", nameof(concealedTiles));
		}

		if (melds.Count == 0 && WinningHandChecker.IsThirteenOrphansComplete(concealedTiles))
		{
			return [Yaku.Kokushi];
		}

		var allTiles = concealedTiles.Concat(melds.SelectMany(meld => meld.Tiles)).ToList();
		var yaku = new List<Yaku>();

		if (melds.Count == 0 && WinningHandChecker.IsSevenPairsComplete(concealedTiles))
		{
			yaku.Add(Yaku.Chiitoitsu);
		}

		if (IsTanyao(allTiles))
		{
			yaku.Add(Yaku.Tanyao);
		}

		if (IsTsuiisou(allTiles))
		{
			yaku.Add(Yaku.Tsuiisou);
		}
		else if (IsChinitsu(allTiles))
		{
			yaku.Add(Yaku.Chinitsu);
		}
		else if (IsHonitsu(allTiles))
		{
			yaku.Add(Yaku.Honitsu);
		}

		AddDecompositionDependentYaku(concealedTiles, melds, yaku);

		return yaku;
	}

	/// <summary>
	/// 標準形（雀頭+4面子）として分解できるあらゆる組み合わせを横断し、いずれかの分解で成立する
	/// 役（対々和・一盃口・二盃口・三色同順・一気通貫・純全帯幺九・混全帯幺九）を判定する。
	/// 鳴きの面子は固定の面子として分解に組み込む（探索対象は<paramref name="concealedTiles"/>のみ）。
	/// 一盃口・二盃口は門前限定のため鳴きがある場合は判定しない。
	/// 「1つの分解を選ぶ」得点計算とは異なり、存在確認ベース（いずれかの分解で成立すれば役ありとする）の判定にとどめる。
	/// </summary>
	private static void AddDecompositionDependentYaku(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, List<Yaku> yaku)
	{
		var counts = new int[KindCount];
		foreach (var tile in concealedTiles)
		{
			counts[ToIndex(tile)]++;
		}

		var hasHonorTile = concealedTiles.Any(tile => tile.Suit == TileSuit.Honor) ||
			melds.Any(meld => meld.Tiles[0].Suit == TileSuit.Honor);
		var meldSets = melds.Select(ToDecomposedSet).ToArray();
		var requiredSets = 4 - melds.Count;

		var hasToitoitsu = false;
		var hasIipeikou = false;
		var hasRyanpeikou = false;
		var hasSanshokuDoujun = false;
		var hasIttsuu = false;
		var hasJunchan = false;
		var hasChanta = false;

		for (var pairKind = 0; pairKind < KindCount; pairKind++)
		{
			if (counts[pairKind] < 2)
			{
				continue;
			}

			counts[pairKind] -= 2;
			foreach (var searched in EnumerateSetDecompositions(counts, requiredSets))
			{
				DecomposedSet[] decomposition = [.. meldSets, .. searched];

				hasToitoitsu |= Array.TrueForAll(decomposition, set => set.IsTriplet);

				if (melds.Count == 0)
				{
					var duplicateSequenceGroups = CountDuplicateSequenceGroups(decomposition);
					hasIipeikou |= duplicateSequenceGroups == 1;
					hasRyanpeikou |= duplicateSequenceGroups == 2;
				}

				hasSanshokuDoujun |= HasSanshokuDoujun(decomposition);
				hasIttsuu |= HasIttsuu(decomposition);

				if (AllSetsContainTerminalOrHonor(decomposition, pairKind))
				{
					if (hasHonorTile)
					{
						hasChanta = true;
					}
					else
					{
						hasJunchan = true;
					}
				}
			}

			counts[pairKind] += 2;
		}

		if (hasToitoitsu)
		{
			yaku.Add(Yaku.Toitoitsu);
		}

		if (hasIipeikou)
		{
			yaku.Add(Yaku.Iipeikou);
		}

		if (hasRyanpeikou)
		{
			yaku.Add(Yaku.Ryanpeikou);
		}

		if (hasSanshokuDoujun)
		{
			yaku.Add(Yaku.SanshokuDoujun);
		}

		if (hasIttsuu)
		{
			yaku.Add(Yaku.Ittsuu);
		}

		if (hasJunchan)
		{
			yaku.Add(Yaku.Junchan);
		}

		if (hasChanta)
		{
			yaku.Add(Yaku.Chanta);
		}
	}

	/// <summary>分解内で同じスート・同じ開始ランクの順子が何グループ存在するかを数える（1個なら一盃口、2個なら二盃口の候補）。</summary>
	private static int CountDuplicateSequenceGroups(DecomposedSet[] decomposition) =>
		decomposition
			.Where(set => !set.IsTriplet)
			.GroupBy(set => (set.Suit, set.Rank))
			.Count(group => group.Count() >= 2);

	/// <summary>
	/// 雀頭と全ての面子が老頭牌（老頭牌の刻子）・字牌（字牌の刻子）・
	/// 1-2-3または7-8-9の順子のいずれかで構成されているかを判定する（純全帯幺九・混全帯幺九の共通条件）。
	/// </summary>
	private static bool AllSetsContainTerminalOrHonor(DecomposedSet[] decomposition, int pairKind)
	{
		var (pairSuit, pairRank) = FromIndex(pairKind);
		if (pairSuit != TileSuit.Honor && pairRank != 1 && pairRank != 9)
		{
			return false;
		}

		foreach (var set in decomposition)
		{
			if (set.Suit == TileSuit.Honor)
			{
				continue;
			}

			if (set.IsTriplet)
			{
				if (set.Rank != 1 && set.Rank != 9)
				{
					return false;
				}
			}
			else if (set.Rank != 1 && set.Rank != 7)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>分解内に同じ開始ランクの順子が萬子・筒子・索子の3スート全てに存在するかを判定する（三色同順）。</summary>
	private static bool HasSanshokuDoujun(DecomposedSet[] decomposition) =>
		decomposition
			.Where(set => !set.IsTriplet)
			.GroupBy(set => set.Rank)
			.Any(group => group.Select(set => set.Suit).Distinct().Count() == 3);

	/// <summary>分解内に同一スートで開始ランク1・4・7の順子が全て存在するかを判定する（一気通貫）。</summary>
	private static bool HasIttsuu(DecomposedSet[] decomposition)
	{
		foreach (var suit in new[] { TileSuit.Man, TileSuit.Pin, TileSuit.Sou })
		{
			var hasStart1 = decomposition.Any(set => !set.IsTriplet && set.Suit == suit && set.Rank == 1);
			var hasStart4 = decomposition.Any(set => !set.IsTriplet && set.Suit == suit && set.Rank == 4);
			var hasStart7 = decomposition.Any(set => !set.IsTriplet && set.Suit == suit && set.Rank == 7);
			if (hasStart1 && hasStart4 && hasStart7)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// countsを<paramref name="setsNeeded"/>個の面子（刻子・順子）に分解できるあらゆる組み合わせを列挙する。
	/// <see cref="WinningHandChecker"/>の完成判定と同じ「最小のkindを消費」というバックトラッキングを行うが、
	/// 存在確認(bool)ではなく実際の分解内容を全て列挙する点が異なる。
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
			var set = new DecomposedSet(true, suit, rank);
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
			var set = new DecomposedSet(false, suit, rank);
			foreach (var rest in EnumerateSetDecompositions(counts, setsNeeded - 1))
			{
				yield return [set, .. rest];
			}

			counts[kind]++;
			counts[kind + 1]++;
			counts[kind + 2]++;
		}
	}

	/// <summary>分解済みの1面子（刻子または順子）。順子はRankを開始ランクとする。</summary>
	private readonly record struct DecomposedSet(bool IsTriplet, TileSuit Suit, int Rank);

	/// <summary>鳴きの面子(<see cref="Meld"/>)を分解済みの1面子(<see cref="DecomposedSet"/>)に変換する。</summary>
	private static DecomposedSet ToDecomposedSet(Meld meld)
	{
		var first = meld.Tiles[0];
		return new DecomposedSet(meld.Type != MeldType.Chi, first.Suit, first.Rank);
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
