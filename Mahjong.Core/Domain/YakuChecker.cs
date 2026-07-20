namespace Mahjong.Core.Domain;

/// <summary>
/// 牌の内容・面子の分解・自風/場風・ロン/ツモの別・待ちの形から判定できる役を判定する。
/// タイミング限定の役、翻数・得点計算も対象外。
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
	/// 自風・場風を考慮し、役牌（自風牌・場風牌・三元牌）・三暗刻・門前清自摸和・平和も含めて役を判定する。
	/// <paramref name="ronTile"/>はロン和了牌（ツモ和了の場合は<c>null</c>）。ロンで完成した刻子は
	/// 明刻扱いとなり三暗刻の暗刻数に数えない。和了牌は<paramref name="ronTile"/>（ロン）または
	/// <paramref name="concealedTiles"/>の末尾（ツモ。<see cref="Hand.Draw"/>が末尾に追加する規約に基づく）
	/// から求め、平和の両面待ち判定に用いる。
	/// </summary>
	/// <exception cref="ArgumentException">
	/// <paramref name="concealedTiles"/>と<paramref name="melds"/>を合わせた手牌が和了形でない場合。
	/// </exception>
	internal static IReadOnlyList<Yaku> DetermineYaku(
		IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, Seat seatWind, Seat roundWind,
		Tile? ronTile = null)
	{
		var yaku = new List<Yaku>(DetermineYaku(concealedTiles, melds));
		if (yaku.Contains(Yaku.Kokushi))
		{
			return yaku;
		}

		if (ronTile is null && melds.Count == 0)
		{
			yaku.Add(Yaku.MenzenTsumo);
		}

		var winningTile = ronTile ?? concealedTiles[^1];
		if (HasPinfu(concealedTiles, melds, winningTile, ToWindRank(seatWind), ToWindRank(roundWind)))
		{
			yaku.Add(Yaku.Pinfu);
		}

		if (HasHonorTriplet(concealedTiles, melds, ToWindRank(seatWind)))
		{
			yaku.Add(Yaku.Jikaze);
		}

		if (HasHonorTriplet(concealedTiles, melds, ToWindRank(roundWind)))
		{
			yaku.Add(Yaku.Bakaze);
		}

		foreach (var dragonRank in DragonRanks)
		{
			if (HasHonorTriplet(concealedTiles, melds, dragonRank))
			{
				yaku.Add(Yaku.Sangenpai);
			}
		}

		if (HasSanankou(concealedTiles, melds, ronTile))
		{
			yaku.Add(Yaku.Sanankou);
		}

		return yaku;
	}

	/// <summary>
	/// 標準形（雀頭+4面子）として分解できるあらゆる組み合わせを横断し、いずれかの分解で
	/// 暗刻（門前で完成した刻子。ロンで完成した刻子・鳴きの刻子・加槓は除く）が3つ以上あるかを判定する。
	/// </summary>
	private static bool HasSanankou(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, Tile? ronTile)
	{
		var counts = new int[KindCount];
		foreach (var tile in concealedTiles)
		{
			counts[ToIndex(tile)]++;
		}

		var meldSets = melds.Select(ToDecomposedSet).ToArray();
		var requiredSets = 4 - melds.Count;

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
				var concealedTripletCount = decomposition.Count(set =>
					set.IsTriplet && set.IsConcealed &&
					!(ronTile is { } tile && set.Suit == tile.Suit && set.Rank == tile.Rank));
				if (concealedTripletCount >= 3)
				{
					counts[pairKind] += 2;
					return true;
				}
			}

			counts[pairKind] += 2;
		}

		return false;
	}

	/// <summary>
	/// 標準形（雀頭+4面子）として分解できるあらゆる組み合わせを横断し、いずれかの分解で
	/// (a)雀頭が役牌（三元牌・自風牌・場風牌）でない、(b)雀頭が和了牌自身でない（単騎待ちを除外）、
	/// (c)全ての面子が順子（刻子を含まない）、(d)いずれかの順子が和了牌によって両面完成している、
	/// の4条件を満たすかを判定する（平和）。副露がある場合は門前条件を満たさないため常に<c>false</c>。
	/// </summary>
	private static bool HasPinfu(
		IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, Tile winningTile,
		int seatWindRank, int roundWindRank)
	{
		if (melds.Count > 0)
		{
			return false;
		}

		var counts = new int[KindCount];
		foreach (var tile in concealedTiles)
		{
			counts[ToIndex(tile)]++;
		}

		var winningKind = ToIndex(winningTile);

		for (var pairKind = 0; pairKind < KindCount; pairKind++)
		{
			if (counts[pairKind] < 2)
			{
				continue;
			}

			if (pairKind == winningKind || IsYakuhaiPairKind(pairKind, seatWindRank, roundWindRank))
			{
				continue;
			}

			counts[pairKind] -= 2;
			foreach (var searched in EnumerateSetDecompositions(counts, 4))
			{
				if (Array.TrueForAll(searched, set => !set.IsTriplet) &&
					Array.Exists(searched, set => IsRyanmenCompletion(set, winningTile)))
				{
					counts[pairKind] += 2;
					return true;
				}
			}

			counts[pairKind] += 2;
		}

		return false;
	}

	/// <summary>
	/// 順子<paramref name="set"/>が和了牌<paramref name="winningTile"/>によって両面待ちで完成しているかを判定する。
	/// 和了牌が順子の中央（嵌張）、または1-2-3の3待ち・7-8-9の7待ち（辺張）の場合は<c>false</c>。
	/// </summary>
	private static bool IsRyanmenCompletion(DecomposedSet set, Tile winningTile)
	{
		if (set.IsTriplet || set.Suit != winningTile.Suit)
		{
			return false;
		}

		var offset = winningTile.Rank - set.Rank;
		if (offset < 0 || offset > 2 || offset == 1)
		{
			return false;
		}

		if (offset == 0 && set.Rank == 7)
		{
			return false;
		}

		if (offset == 2 && set.Rank == 1)
		{
			return false;
		}

		return true;
	}

	/// <summary>雀頭候補<paramref name="pairKind"/>が役牌（三元牌・自風牌・場風牌）かどうかを判定する。</summary>
	private static bool IsYakuhaiPairKind(int pairKind, int seatWindRank, int roundWindRank)
	{
		var (suit, rank) = FromIndex(pairKind);
		if (suit != TileSuit.Honor)
		{
			return false;
		}

		return DragonRanks.Contains(rank) || rank == seatWindRank || rank == roundWindRank;
	}

	/// <summary>白(5)・發(6)・中(7)の字牌ランク。</summary>
	private static readonly int[] DragonRanks = [5, 6, 7];

	/// <summary>指定したランクの字牌が門前に3枚以上あるか、または該当ランクの刻子・カンの副露があるかを判定する。</summary>
	private static bool HasHonorTriplet(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<Meld> melds, int rank)
	{
		var concealedCount = concealedTiles.Count(tile => tile.Suit == TileSuit.Honor && tile.Rank == rank);
		if (concealedCount >= 3)
		{
			return true;
		}

		return melds.Any(meld =>
			meld.Type != MeldType.Chi && meld.Tiles[0].Suit == TileSuit.Honor && meld.Tiles[0].Rank == rank);
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

	/// <summary>
	/// 分解済みの1面子（刻子または順子）。順子はRankを開始ランクとする。
	/// <paramref name="IsConcealed"/>は門前（暗刻・暗槓）かどうかで、三暗刻の判定に使う
	/// （<see cref="EnumerateSetDecompositions"/>由来のものは常に門前なので既定値<c>true</c>のまま）。
	/// </summary>
	private readonly record struct DecomposedSet(bool IsTriplet, TileSuit Suit, int Rank, bool IsConcealed = true);

	/// <summary>鳴きの面子(<see cref="Meld"/>)を分解済みの1面子(<see cref="DecomposedSet"/>)に変換する。</summary>
	private static DecomposedSet ToDecomposedSet(Meld meld)
	{
		var first = meld.Tiles[0];
		var isConcealed = meld.Type == MeldType.ClosedKan;
		return new DecomposedSet(meld.Type != MeldType.Chi, first.Suit, first.Rank, isConcealed);
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
