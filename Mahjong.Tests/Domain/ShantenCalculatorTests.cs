using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class ShantenCalculatorTests
{
	/// <summary>パス条件: 4面子+単騎待ち（1枚）の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_TankiTenpai_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];

		Assert.Equal(0, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 3面子+雀頭+両面搭子の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_RyanmenTenpai_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 5),
		];

		Assert.Equal(0, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 3面子+対子2組（シャンポン待ち）の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_ShanponTenpai_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
		];

		Assert.Equal(0, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 3面子+嵌張搭子のみで雀頭が存在しない13枚を渡すと 1 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_OneShantenWithoutPair_ReturnsOne()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Man, 9),
		];

		Assert.Equal(1, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>
	/// パス条件: 3面子+対子1組のみの13枚を渡すと 1 になること。
	/// 対子を搭子と雀頭の両方に二重カウントすると誤って0になってしまうケースの回帰テスト。
	/// </summary>
	[Fact]
	public void CalculateStandardFormShanten_SinglePairDoubleCountingTrap_ReturnsOne()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 6),
		];

		Assert.Equal(1, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 2面子+嵌張搭子+雀頭の13枚を渡すと 2 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_TwoShanten_ReturnsTwo()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
		];

		Assert.Equal(2, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 面子・搭子・雀頭が一切成立しない13枚を渡すと最大値の 8 になること。</summary>
	[Fact]
	public void CalculateStandardFormShanten_NoBlocksAtAll_ReturnsEight()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 4),
		];

		Assert.Equal(8, ShantenCalculator.CalculateStandardFormShanten(tiles));
	}

	/// <summary>パス条件: 13枚以外(12枚・14枚)を渡すと ArgumentException になること。</summary>
	[Theory]
	[InlineData(12)]
	[InlineData(14)]
	public void CalculateStandardFormShanten_WithWrongTileCount_Throws(int count)
	{
		var tiles = Enumerable.Range(0, count)
			.Select(i => new Tile(TileSuit.Man, (i % 9) + 1))
			.ToList();

		Assert.Throws<ArgumentException>(() => ShantenCalculator.CalculateStandardFormShanten(tiles));
	}
}
