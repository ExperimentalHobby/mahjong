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

	/// <summary>パス条件: 6対子+7種類の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateSevenPairsShanten_Tenpai_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
			new Tile(TileSuit.Honor, 1),
		];

		Assert.Equal(0, ShantenCalculator.CalculateSevenPairsShanten(tiles));
	}

	/// <summary>
	/// パス条件: 対子は4種類分あるが手牌の種類数自体が5種類しかない13枚を渡すと、
	/// 種類数不足の追加ペナルティにより 4 になること（6-4+max(0,7-5)=4）。
	/// </summary>
	[Fact]
	public void CalculateSevenPairsShanten_TooFewDistinctKinds_ReturnsFour()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5),
		];

		Assert.Equal(4, ShantenCalculator.CalculateSevenPairsShanten(tiles));
	}

	/// <summary>パス条件: 13種類バラバラ（対子0組）の13枚を渡すと最大値の 6 になること。</summary>
	[Fact]
	public void CalculateSevenPairsShanten_NoPairsAtAll_ReturnsSix()
	{
		var tiles = CreateThirteenDistinctTiles();

		Assert.Equal(6, ShantenCalculator.CalculateSevenPairsShanten(tiles));
	}

	/// <summary>パス条件: 13枚以外(12枚・14枚)を渡すと ArgumentException になること。</summary>
	[Theory]
	[InlineData(12)]
	[InlineData(14)]
	public void CalculateSevenPairsShanten_WithWrongTileCount_Throws(int count)
	{
		var tiles = Enumerable.Range(0, count)
			.Select(i => new Tile(TileSuit.Man, (i % 9) + 1))
			.ToList();

		Assert.Throws<ArgumentException>(() => ShantenCalculator.CalculateSevenPairsShanten(tiles));
	}

	/// <summary>パス条件: 么九牌12種類+その1種の対子の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateThirteenOrphansShanten_TenpaiWithPair_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6),
		];

		Assert.Equal(0, ShantenCalculator.CalculateThirteenOrphansShanten(tiles));
	}

	/// <summary>パス条件: 么九牌13種類が1枚ずつ（13面待ち）の聴牌形13枚を渡すと 0 になること。</summary>
	[Fact]
	public void CalculateThirteenOrphansShanten_ThirteenWaitTenpai_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 7),
		];

		Assert.Equal(0, ShantenCalculator.CalculateThirteenOrphansShanten(tiles));
	}

	/// <summary>パス条件: 么九牌6種類（対子なし）+中張牌7枚の13枚を渡すと 7 になること（13-6-0=7）。</summary>
	[Fact]
	public void CalculateThirteenOrphansShanten_FewYaochuuKinds_ReturnsSeven()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
		];

		Assert.Equal(7, ShantenCalculator.CalculateThirteenOrphansShanten(tiles));
	}

	/// <summary>パス条件: 么九牌を1枚も含まない13枚を渡すと最大値の 13 になること。</summary>
	[Fact]
	public void CalculateThirteenOrphansShanten_NoYaochuuAtAll_ReturnsThirteen()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 2),
		];

		Assert.Equal(13, ShantenCalculator.CalculateThirteenOrphansShanten(tiles));
	}

	/// <summary>パス条件: 13枚以外(12枚・14枚)を渡すと ArgumentException になること。</summary>
	[Theory]
	[InlineData(12)]
	[InlineData(14)]
	public void CalculateThirteenOrphansShanten_WithWrongTileCount_Throws(int count)
	{
		var tiles = Enumerable.Range(0, count)
			.Select(i => new Tile(TileSuit.Man, (i % 9) + 1))
			.ToList();

		Assert.Throws<ArgumentException>(() => ShantenCalculator.CalculateThirteenOrphansShanten(tiles));
	}

	/// <summary>パス条件: 標準形が最小になる13枚を渡すと CalculateShanten が 0 になること。</summary>
	[Fact]
	public void CalculateShanten_StandardFormIsMinimum_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];

		Assert.Equal(0, ShantenCalculator.CalculateShanten(tiles));
	}

	/// <summary>
	/// パス条件: 標準形では聴牌にならないが七対子では聴牌になる13枚を渡すと、
	/// CalculateShanten が 0 になること。
	/// </summary>
	[Fact]
	public void CalculateShanten_SevenPairsIsMinimum_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Honor, 2),
		];

		Assert.Equal(0, ShantenCalculator.CalculateShanten(tiles));
	}

	/// <summary>
	/// パス条件: 標準形・七対子では聴牌にならないが国士無双では聴牌になる13枚を渡すと、
	/// CalculateShanten が 0 になること。
	/// </summary>
	[Fact]
	public void CalculateShanten_ThirteenOrphansIsMinimum_ReturnsZero()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5),
			new Tile(TileSuit.Honor, 6), new Tile(TileSuit.Honor, 6),
		];

		Assert.Equal(0, ShantenCalculator.CalculateShanten(tiles));
	}

	private static List<Tile> CreateThirteenDistinctTiles()
	{
		var tiles = new List<Tile>();
		for (var rank = 1; rank <= 9; rank++)
		{
			tiles.Add(new Tile(TileSuit.Man, rank));
		}

		for (var rank = 1; rank <= 4; rank++)
		{
			tiles.Add(new Tile(TileSuit.Pin, rank));
		}

		return tiles;
	}
}
