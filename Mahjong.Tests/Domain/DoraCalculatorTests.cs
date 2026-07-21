using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class DoraCalculatorTests
{
	/// <summary>パス条件: 数牌のドラ表示牌（Man3）の次の牌（Man4）を1枚持つ場合、ドラ1枚として数えられること。</summary>
	[Fact]
	public void CountDora_HandContainsNextRank_ReturnsOne()
	{
		Tile[] concealedTiles = [new Tile(TileSuit.Man, 4), new Tile(TileSuit.Pin, 1)];
		Tile[] doraIndicators = [new Tile(TileSuit.Man, 3)];

		var count = DoraCalculator.CountDora(concealedTiles, melds: [], doraIndicators);

		Assert.Equal(1, count);
	}

	/// <summary>パス条件: 数牌9のドラ表示牌は1に巡回すること。</summary>
	[Fact]
	public void GetDoraTile_NumberNine_WrapsToOne()
	{
		var doraTile = DoraCalculator.GetDoraTile(new Tile(TileSuit.Sou, 9));

		Assert.Equal(new Tile(TileSuit.Sou, 1), doraTile);
	}

	/// <summary>パス条件: 風牌のドラ表示（東→南→西→北→東）が巡回すること。</summary>
	[Theory]
	[InlineData(1, 2)]
	[InlineData(2, 3)]
	[InlineData(3, 4)]
	[InlineData(4, 1)]
	public void GetDoraTile_WindTile_CyclesThroughFourWinds(int indicatorRank, int expectedRank)
	{
		var doraTile = DoraCalculator.GetDoraTile(new Tile(TileSuit.Honor, indicatorRank));

		Assert.Equal(new Tile(TileSuit.Honor, expectedRank), doraTile);
	}

	/// <summary>パス条件: 三元牌のドラ表示（白→發→中→白）が巡回すること。</summary>
	[Theory]
	[InlineData(5, 6)]
	[InlineData(6, 7)]
	[InlineData(7, 5)]
	public void GetDoraTile_DragonTile_CyclesThroughThreeDragons(int indicatorRank, int expectedRank)
	{
		var doraTile = DoraCalculator.GetDoraTile(new Tile(TileSuit.Honor, indicatorRank));

		Assert.Equal(new Tile(TileSuit.Honor, expectedRank), doraTile);
	}

	/// <summary>パス条件: 副露（ポン）の中の牌もドラとして数えられること。</summary>
	[Fact]
	public void CountDora_MeldContainsDoraTile_ReturnsOne()
	{
		Tile[] concealedTiles = [new Tile(TileSuit.Pin, 1)];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4)])];
		Tile[] doraIndicators = [new Tile(TileSuit.Man, 3)];

		var count = DoraCalculator.CountDora(concealedTiles, melds, doraIndicators);

		Assert.Equal(3, count);
	}

	/// <summary>パス条件: 赤ドラ（IsRedFive）が1枚につき1翻として数えられること。</summary>
	[Fact]
	public void CountAkaDora_ConcealedContainsRedFive_ReturnsOne()
	{
		Tile[] concealedTiles = [new Tile(TileSuit.Man, 5, isRedFive: true), new Tile(TileSuit.Pin, 1)];

		var count = DoraCalculator.CountAkaDora(concealedTiles, melds: []);

		Assert.Equal(1, count);
	}
}
