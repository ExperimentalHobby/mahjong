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
}
