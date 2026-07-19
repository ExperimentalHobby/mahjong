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
}
