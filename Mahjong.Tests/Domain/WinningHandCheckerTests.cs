using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class WinningHandCheckerTests
{
	/// <summary>パス条件: 刻子4つ+数牌の雀頭で構成される14枚を渡すと true になること。</summary>
	[Fact]
	public void IsStandardFormComplete_FourTripletsWithPair_ReturnsTrue()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];

		Assert.True(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 順子4つ+雀頭で構成される14枚を渡すと true になること。</summary>
	[Fact]
	public void IsStandardFormComplete_FourRunsWithPair_ReturnsTrue()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
		];

		Assert.True(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 刻子と順子が混在する14枚を渡すと true になること。</summary>
	[Fact]
	public void IsStandardFormComplete_TripletsAndRunsMixed_ReturnsTrue()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Man, 9),
		];

		Assert.True(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>
	/// パス条件: 雀頭候補が複数あり、East×2以外を雀頭にすると必ず分解不可になる14枚を渡すと、
	/// バックトラックの末にEast×2を雀頭とする分解が見つかり true になること。
	/// </summary>
	[Fact]
	public void IsStandardFormComplete_RequiresBacktrackingToFindCorrectPair_ReturnsTrue()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 8), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
		];

		Assert.True(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 4面子に分解できない（連続しない浮き牌が残る）14枚を渡すと false になること。</summary>
	[Fact]
	public void IsStandardFormComplete_CannotFormFourSets_ReturnsFalse()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5),
		];

		Assert.False(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 字牌を連続するRankで1枚ずつ並べても順子として成立せず false になること。</summary>
	[Fact]
	public void IsStandardFormComplete_HonorTilesCannotFormRun_ReturnsFalse()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
		];

		Assert.False(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 赤ドラを含む牌でも通常の5として判定され true になること。</summary>
	[Fact]
	public void IsStandardFormComplete_WithRedFive_ReturnsTrue()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 2),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5, isRedFive: true), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
		];

		Assert.True(WinningHandChecker.IsStandardFormComplete(tiles));
	}

	/// <summary>パス条件: 14枚以外(13枚・15枚)を渡すと ArgumentException になること。</summary>
	[Theory]
	[InlineData(13)]
	[InlineData(15)]
	public void IsStandardFormComplete_WithWrongTileCount_Throws(int count)
	{
		var tiles = Enumerable.Range(0, count)
			.Select(i => new Tile(TileSuit.Man, i % 9 + 1))
			.ToList();

		Assert.Throws<ArgumentException>(() => WinningHandChecker.IsStandardFormComplete(tiles));
	}
}
