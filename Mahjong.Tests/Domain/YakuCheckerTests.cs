using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class YakuCheckerTests
{
	/// <summary>パス条件: 2〜8のみで構成された標準形の和了形を渡すと Yaku.Tanyao が含まれること。</summary>
	[Fact]
	public void DetermineYaku_StandardFormWithoutTerminalsOrHonors_ContainsTanyao()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Tanyao, yaku);
	}

	/// <summary>パス条件: 老頭牌（1・9）を含む標準形の和了形を渡すと Yaku.Tanyao が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_StandardFormWithTerminal_DoesNotContainTanyao()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.Tanyao, yaku);
	}

	/// <summary>パス条件: 1色+字牌で構成された標準形の和了形を渡すと Yaku.Honitsu が含まれること。</summary>
	[Fact]
	public void DetermineYaku_StandardFormSingleSuitWithHonors_ContainsHonitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Honitsu, yaku);
	}

	/// <summary>
	/// パス条件: 1色のみ（字牌なし）で構成された標準形の和了形を渡すと Yaku.Chinitsu が含まれ、
	/// Yaku.Honitsu は含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_StandardFormSingleSuitWithoutHonors_ContainsChinitsuOnly()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Chinitsu, yaku);
		Assert.DoesNotContain(Yaku.Honitsu, yaku);
	}

	/// <summary>パス条件: 字牌のみで構成された和了形を渡すと Yaku.Tsuiisou が含まれること。</summary>
	[Fact]
	public void DetermineYaku_AllHonorTiles_ContainsTsuiisou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
			new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 4),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Tsuiisou, yaku);
	}

	/// <summary>パス条件: 七対子の和了形を渡すと Yaku.Chiitoitsu が含まれること。</summary>
	[Fact]
	public void DetermineYaku_SevenPairs_ContainsChiitoitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Chiitoitsu, yaku);
	}

	/// <summary>パス条件: 国士無双の和了形を渡すと Yaku.Kokushi のみが返ること（他の役と複合しないことの確認）。</summary>
	[Fact]
	public void DetermineYaku_ThirteenOrphans_ReturnsKokushiOnly()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Equal([Yaku.Kokushi], yaku);
	}

	/// <summary>
	/// パス条件: 2〜8のみで構成された七対子の和了形を渡すと Yaku.Tanyao と Yaku.Chiitoitsu の
	/// 両方が含まれること（複合役の確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_SevenPairsWithoutTerminalsOrHonors_ContainsTanyaoAndChiitoitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Tanyao, yaku);
		Assert.Contains(Yaku.Chiitoitsu, yaku);
	}

	/// <summary>パス条件: 断幺九・混一色・清一色・字一色・七対子・国士無双のいずれにも該当しない標準形の和了形を渡すと空のリストが返ること。</summary>
	[Fact]
	public void DetermineYaku_StandardFormWithoutAnyContentOnlyYaku_ReturnsEmptyList()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Empty(yaku);
	}

	/// <summary>パス条件: 完成していない14枚（または14枚以外）を渡すと ArgumentException になること。</summary>
	[Fact]
	public void DetermineYaku_IncompleteHand_ThrowsArgumentException()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Pin, 2),
		];

		Assert.Throws<ArgumentException>(() => YakuChecker.DetermineYaku(tiles));
	}

	/// <summary>パス条件: 4刻子+雀頭の標準形の和了形を渡すと Yaku.Toitoitsu が含まれること。</summary>
	[Fact]
	public void DetermineYaku_FourTripletsStandardForm_ContainsToitoitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Toitoitsu, yaku);
	}

	/// <summary>パス条件: 刻子1つ+順子3つ+雀頭の標準形の和了形を渡すと Yaku.Toitoitsu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_OneTripletThreeSequencesStandardForm_DoesNotContainToitoitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.Toitoitsu, yaku);
	}

	/// <summary>
	/// パス条件: 同じ順子が2組（1組のみ）ある標準形の和了形を渡すと Yaku.Iipeikou が含まれ、
	/// Yaku.Ryanpeikou は含まれないこと（重複順子グループが1個であることの確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_TwoIdenticalSequencesStandardForm_ContainsIipeikou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Iipeikou, yaku);
		Assert.DoesNotContain(Yaku.Ryanpeikou, yaku);
	}

	/// <summary>パス条件: 重複する順子が無い標準形の和了形を渡すと Yaku.Iipeikou が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_NoDuplicateSequenceStandardForm_DoesNotContainIipeikou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.Iipeikou, yaku);
	}

	/// <summary>
	/// パス条件: 同じ数字の順子が萬子・筒子・索子の3色全てに含まれる和了形を渡すと
	/// Yaku.SanshokuDoujun が含まれること。
	/// </summary>
	[Fact]
	public void DetermineYaku_SameSequenceAcrossThreeSuits_ContainsSanshokuDoujun()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.SanshokuDoujun, yaku);
	}

	/// <summary>パス条件: 3色に跨る同一順子が無い和了形を渡すと Yaku.SanshokuDoujun が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_NoSameSequenceAcrossThreeSuits_DoesNotContainSanshokuDoujun()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.SanshokuDoujun, yaku);
	}

	/// <summary>
	/// パス条件: 同一色で123・456・789の3順子を含む和了形を渡すと Yaku.Ittsuu が含まれること。
	/// </summary>
	[Fact]
	public void DetermineYaku_PureStraightInOneSuit_ContainsIttsuu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Ittsuu, yaku);
	}

	/// <summary>パス条件: 同一色で123・456・789の3順子が揃わない和了形を渡すと Yaku.Ittsuu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_NoPureStraightInOneSuit_DoesNotContainIttsuu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.Ittsuu, yaku);
	}

	/// <summary>
	/// パス条件: 4刻子が全て2〜8の標準形の和了形を渡すと Yaku.Toitoitsu と Yaku.Tanyao の
	/// 両方が含まれること（複合役の確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_FourTripletsWithoutTerminalsOrHonors_ContainsToitoitsuAndTanyao()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 7),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Toitoitsu, yaku);
		Assert.Contains(Yaku.Tanyao, yaku);
	}

	/// <summary>
	/// パス条件: 「4刻子」とも「3つの同一順子+1刻子」とも読める和了形を渡すと Yaku.Toitoitsu と
	/// Yaku.Iipeikou の両方が含まれること（分解探索が最初に見つかった1つの分解だけで
	/// 判定を打ち切っていないことの確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_AmbiguousDecomposition_ContainsBothToitoitsuAndIipeikou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Toitoitsu, yaku);
		Assert.Contains(Yaku.Iipeikou, yaku);
	}

	/// <summary>パス条件: 老頭牌のみで構成された標準形（字牌を含まない）の和了形を渡すと Yaku.Junchan が含まれること。</summary>
	[Fact]
	public void DetermineYaku_AllTerminalRelatedSetsWithoutHonors_ContainsJunchan()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Junchan, yaku);
	}

	/// <summary>
	/// パス条件: 字牌を含み全ての面子が么九牌絡みの標準形の和了形を渡すと Yaku.Chanta が含まれ、
	/// Yaku.Junchan は含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_AllTerminalOrHonorRelatedSetsWithHonors_ContainsChantaNotJunchan()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Chanta, yaku);
		Assert.DoesNotContain(Yaku.Junchan, yaku);
	}

	/// <summary>
	/// パス条件: 2〜6開始の順子を含む（么九牌絡みでない面子がある）標準形の和了形を渡すと
	/// Yaku.Junchan・Yaku.Chanta のいずれも含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_StandardFormWithMiddleSequence_ContainsNeitherJunchanNorChanta()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.DoesNotContain(Yaku.Junchan, yaku);
		Assert.DoesNotContain(Yaku.Chanta, yaku);
	}

	/// <summary>
	/// パス条件: 2組の異なる同一順子ペア（123+123+456+456）で構成される標準形の和了形を渡すと
	/// Yaku.Ryanpeikou が含まれ、Yaku.Iipeikou は含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_TwoDistinctDuplicateSequencePairs_ContainsRyanpeikouNotIipeikou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Ryanpeikou, yaku);
		Assert.DoesNotContain(Yaku.Iipeikou, yaku);
	}

	/// <summary>
	/// パス条件: 老頭牌・字牌絡みの刻子のみで構成された標準形の和了形を渡すと Yaku.Chanta と
	/// Yaku.Toitoitsu の両方が含まれること（複合役の確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_AllTerminalOrHonorTriplets_ContainsChantaAndToitoitsu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles);

		Assert.Contains(Yaku.Chanta, yaku);
		Assert.Contains(Yaku.Toitoitsu, yaku);
	}

	/// <summary>パス条件: 鳴き（ポン1つ）を含む手に対して2〜8のみで構成されていれば Yaku.Tanyao が含まれること。</summary>
	[Fact]
	public void DetermineYaku_WithPonMeldWithoutTerminalsOrHonors_ContainsTanyao()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Pin, 8), new Tile(TileSuit.Pin, 8),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.Contains(Yaku.Tanyao, yaku);
	}

	/// <summary>パス条件: 鳴きがある場合、Yaku.Chiitoitsu・Yaku.Kokushi は判定されないこと（門前限定のため）。</summary>
	[Fact]
	public void DetermineYaku_WithMeld_DoesNotContainChiitoitsuOrKokushi()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Pin, 8), new Tile(TileSuit.Pin, 8),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.DoesNotContain(Yaku.Chiitoitsu, yaku);
		Assert.DoesNotContain(Yaku.Kokushi, yaku);
	}

	/// <summary>
	/// パス条件: 鳴き（チー1つ）を含む手に対して、鳴きの面子を含めた分解で三色同順が成立する場合、
	/// Yaku.SanshokuDoujun が含まれること。
	/// </summary>
	[Fact]
	public void DetermineYaku_WithChiMeldFormingSanshokuDoujun_ContainsSanshokuDoujun()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];
		Meld[] melds = [new Meld(MeldType.Chi, [new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.Contains(Yaku.SanshokuDoujun, yaku);
	}

	/// <summary>
	/// パス条件: 鳴き（チー1つ）を含む手は、門前であれば一盃口になるはずの同一順子の重複があっても
	/// Yaku.Iipeikou・Yaku.Ryanpeikou は含まれないこと（門前限定のため）。
	/// </summary>
	[Fact]
	public void DetermineYaku_WithMeldAndDuplicateSequence_DoesNotContainIipeikouOrRyanpeikou()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		Meld[] melds = [new Meld(MeldType.Chi, [new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.DoesNotContain(Yaku.Iipeikou, yaku);
		Assert.DoesNotContain(Yaku.Ryanpeikou, yaku);
	}

	/// <summary>パス条件: 自風と一致する字牌の刻子を含む和了形を渡すと Yaku.Jikaze が含まれること。</summary>
	[Fact]
	public void DetermineYaku_SeatWindTriplet_ContainsJikaze()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.East, roundWind: Seat.West);

		Assert.Contains(Yaku.Jikaze, yaku);
	}

	/// <summary>パス条件: 場風と一致する字牌の刻子を含む和了形を渡すと Yaku.Bakaze が含まれること。</summary>
	[Fact]
	public void DetermineYaku_RoundWindTriplet_ContainsBakaze()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.West, roundWind: Seat.East);

		Assert.Contains(Yaku.Bakaze, yaku);
	}

	/// <summary>
	/// パス条件: 自風・場風が同じ字牌の刻子を含む和了形（ダブ東等）を渡すと、
	/// Yaku.Jikaze と Yaku.Bakaze の両方が含まれること。
	/// </summary>
	[Fact]
	public void DetermineYaku_SeatWindEqualsRoundWindTriplet_ContainsBothJikazeAndBakaze()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.East, roundWind: Seat.East);

		Assert.Contains(Yaku.Jikaze, yaku);
		Assert.Contains(Yaku.Bakaze, yaku);
	}

	/// <summary>パス条件: 白・發・中いずれかの刻子を含む和了形を渡すと Yaku.Sangenpai が含まれること。</summary>
	[Fact]
	public void DetermineYaku_DragonTriplet_ContainsSangenpai()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South);

		Assert.Contains(Yaku.Sangenpai, yaku);
	}

	/// <summary>
	/// パス条件: 三元牌の刻子を2種類含む和了形を渡すと、Yaku.Sangenpai がリストに2回含まれること
	/// （翻数計算はまだ対象外だが、複数成立をリストの重複で表現する）。
	/// </summary>
	[Fact]
	public void DetermineYaku_TwoDragonTriplets_ContainsSangenpaiTwice()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South);

		Assert.Equal(2, yaku.Count(y => y == Yaku.Sangenpai));
	}

	/// <summary>パス条件: 自風・場風・三元牌のいずれの刻子も無い和了形を渡すと、いずれも含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_NoMatchingHonorTriplet_ContainsNoneOfYakuhai()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.East, roundWind: Seat.South);

		Assert.DoesNotContain(Yaku.Jikaze, yaku);
		Assert.DoesNotContain(Yaku.Bakaze, yaku);
		Assert.DoesNotContain(Yaku.Sangenpai, yaku);
	}

	/// <summary>パス条件: 国士無双の和了形を渡すと Yaku.Kokushi のみが返ること（役牌が追加されないことの確認）。</summary>
	[Fact]
	public void DetermineYaku_ThirteenOrphans_WithWindContext_ReturnsKokushiOnly()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.East, roundWind: Seat.East);

		Assert.Equal([Yaku.Kokushi], yaku);
	}

	/// <summary>パス条件: 鳴き（ポン）で構成された役牌の刻子も判定されること。</summary>
	[Fact]
	public void DetermineYaku_WithPonMeldOfSeatWind_ContainsJikaze()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Pin, 8), new Tile(TileSuit.Pin, 8),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.East, roundWind: Seat.South);

		Assert.Contains(Yaku.Jikaze, yaku);
	}

	/// <summary>パス条件: 3つの暗刻を含むツモ和了形を渡すと Yaku.Sanankou が含まれること。</summary>
	[Fact]
	public void DetermineYaku_ThreeConcealedTripletsByTsumo_ContainsSanankou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.Contains(Yaku.Sanankou, yaku);
	}

	/// <summary>
	/// パス条件: ロン牌が3つ目の刻子を完成させる場合、その刻子は暗刻に数えられず
	/// Yaku.Sanankou が含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_RonTileCompletesThirdTriplet_DoesNotContainSanankou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];
		var ronTile = new Tile(TileSuit.Man, 1);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Sanankou, yaku);
	}

	/// <summary>
	/// パス条件: ロン牌が刻子と無関係な待ち（順子の待ち等）を完成させる場合、
	/// 既存の3つの暗刻はそのまま数えられ Yaku.Sanankou が含まれること。
	/// </summary>
	[Fact]
	public void DetermineYaku_RonTileUnrelatedToTriplets_ContainsSanankou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];
		var ronTile = new Tile(TileSuit.Man, 7);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.Contains(Yaku.Sanankou, yaku);
	}

	/// <summary>パス条件: 暗刻が2つ以下の和了形を渡すと Yaku.Sanankou が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_TwoConcealedTriplets_DoesNotContainSanankou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.Sanankou, yaku);
	}

	/// <summary>
	/// パス条件: 鳴き（ポン）で作った刻子は暗刻に数えないため、副露込みで刻子が3つあっても
	/// 暗刻が2つ以下なら Yaku.Sanankou が含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_WithPonTriplet_DoesNotContainSanankou()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1)])];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.Sanankou, yaku);
	}

	/// <summary>パス条件: 暗槓を含む3つの暗刻がある場合 Yaku.Sanankou が含まれること。</summary>
	[Fact]
	public void DetermineYaku_WithClosedKanTriplet_ContainsSanankou()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.ClosedKan,
				[new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.Contains(Yaku.Sanankou, yaku);
	}

	/// <summary>
	/// パス条件: 加槓（元はポン）を含む場合、暗刻が2つ以下なら Yaku.Sanankou が含まれないこと。
	/// </summary>
	[Fact]
	public void DetermineYaku_WithAddedKanTriplet_DoesNotContainSanankou()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.AddedKan,
				[new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.Sanankou, yaku);
	}

	/// <summary>パス条件: 副露なし・ツモ（ronTile: null）の場合、Yaku.MenzenTsumo が含まれること。</summary>
	[Fact]
	public void DetermineYaku_MenzenTsumo_ContainsMenzenTsumo()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.Contains(Yaku.MenzenTsumo, yaku);
	}

	/// <summary>パス条件: 副露なし・ロン（ronTile指定）の場合、Yaku.MenzenTsumo が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_MenzenRon_DoesNotContainMenzenTsumo()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Pin, 2);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.MenzenTsumo, yaku);
	}

	/// <summary>パス条件: 副露ありでツモ和了した場合、Yaku.MenzenTsumo が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_WithMeldTsumo_DoesNotContainMenzenTsumo()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		Meld[] melds =
		[
			new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.MenzenTsumo, yaku);
	}

	/// <summary>
	/// パス条件: 両面待ち・非役牌雀頭・全順子・門前のロン和了で Yaku.Pinfu が含まれること
	/// （Sou6でMan6,7,8を完成。7,8が2または5待ちならぬ6待ちだが開始ランク6は辺張の特例に該当しない両面）。
	/// </summary>
	[Fact]
	public void DetermineYaku_RyanmenWait_ContainsPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.Contains(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 嵌張待ち（Sou5,7待ちのSou6）で完成した場合、Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_KanchanWait_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Sou, 6);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 辺張待ち（Man1,2待ちのMan3）で完成した場合、Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_PenchanWait_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 3);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 単騎待ち（雀頭Pin2を和了牌で完成）の場合、Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_TankiWait_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Pin, 2);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 雀頭が三元牌の場合、両面待ちで完成しても Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_DragonPair_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 雀頭が自風牌の場合、両面待ちで完成しても Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_SeatWindPair_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.West, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 刻子を含む場合、両面待ちで完成しても Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_WithTriplet_DoesNotContainPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 副露がある場合、両面待ちで完成しても Yaku.Pinfu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_WithMeld_DoesNotContainPinfu()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2)])];
		var ronTile = new Tile(TileSuit.Man, 6);

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds, seatWind: Seat.South, roundWind: Seat.South, ronTile);

		Assert.DoesNotContain(Yaku.Pinfu, yaku);
	}

	/// <summary>
	/// パス条件: ツモ和了（ronTile: null、和了牌が手牌の末尾）でも両面待ちなら Yaku.Pinfu が含まれること
	/// （手牌配列の最後の要素を和了牌として扱う規約の確認）。
	/// </summary>
	[Fact]
	public void DetermineYaku_TsumoRyanmenWait_ContainsPinfu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Man, 6),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.Contains(Yaku.Pinfu, yaku);
	}

	/// <summary>パス条件: 同じ数字の刻子が萬子・筒子・索子に揃う場合、Yaku.SanshokuDoukou が含まれること。</summary>
	[Fact]
	public void DetermineYaku_SameRankTripletsAcrossThreeSuits_ContainsSanshokuDoukou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: []);

		Assert.Contains(Yaku.SanshokuDoukou, yaku);
	}

	/// <summary>パス条件: 刻子のランクが3色で揃わない場合、Yaku.SanshokuDoukou が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_DifferentRankTriplets_DoesNotContainSanshokuDoukou()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: []);

		Assert.DoesNotContain(Yaku.SanshokuDoukou, yaku);
	}

	/// <summary>パス条件: 3色のうち1色がカン（副露）の場合でも Yaku.SanshokuDoukou が含まれること。</summary>
	[Fact]
	public void DetermineYaku_SanshokuDoukouWithOpenKan_ContainsSanshokuDoukou()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.OpenKan,
				[new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.Contains(Yaku.SanshokuDoukou, yaku);
	}

	/// <summary>パス条件: 三元牌2種が刻子・残り1種が雀頭の場合、Yaku.Shousangen が含まれること。</summary>
	[Fact]
	public void DetermineYaku_TwoDragonTripletsAndDragonPair_ContainsShousangen()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.Contains(Yaku.Shousangen, yaku);
	}

	/// <summary>
	/// パス条件: 三元牌3種すべてが刻子（大三元相当）の場合、Yaku.Shousangen が含まれないこと
	/// （雀頭が三元牌でないため）。
	/// </summary>
	[Fact]
	public void DetermineYaku_AllThreeDragonTriplets_DoesNotContainShousangen()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.Shousangen, yaku);
	}

	/// <summary>パス条件: 三元牌の刻子が1種のみの場合、Yaku.Shousangen が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_OneDragonTriplet_DoesNotContainShousangen()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];

		var yaku = YakuChecker.DetermineYaku(tiles, melds: [], seatWind: Seat.South, roundWind: Seat.South, ronTile: null);

		Assert.DoesNotContain(Yaku.Shousangen, yaku);
	}

	/// <summary>パス条件: カン（暗槓・明槓・加槓の組み合わせ）が3つ成立している場合、Yaku.Sankantsu が含まれること。</summary>
	[Fact]
	public void DetermineYaku_ThreeKans_ContainsSankantsu()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.ClosedKan,
				[new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3)]),
			new Meld(
				MeldType.OpenKan,
				[new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5)]),
			new Meld(
				MeldType.AddedKan,
				[new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 7)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.Contains(Yaku.Sankantsu, yaku);
	}

	/// <summary>パス条件: カンが2つ以下の場合、Yaku.Sankantsu が含まれないこと。</summary>
	[Fact]
	public void DetermineYaku_TwoKans_DoesNotContainSankantsu()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.ClosedKan,
				[new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3)]),
			new Meld(
				MeldType.OpenKan,
				[new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5)]),
		];

		var yaku = YakuChecker.DetermineYaku(concealedTiles, melds);

		Assert.DoesNotContain(Yaku.Sankantsu, yaku);
	}
}
