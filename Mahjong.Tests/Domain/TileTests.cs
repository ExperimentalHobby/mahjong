using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class TileTests
{
	/// <summary>パス条件: Suit=Man, Rank=1 で牌を生成すると、両プロパティがコンストラクタ引数と一致すること。</summary>
	[Fact]
	public void Constructor_NumberTile_SetsSuitAndRank()
	{
		var tile = new Tile(TileSuit.Man, 1);

		Assert.Equal(TileSuit.Man, tile.Suit);
		Assert.Equal(1, tile.Rank);
	}

	/// <summary>パス条件: Suit=Honor, Rank=1（東）で牌を生成すると、両プロパティがコンストラクタ引数と一致すること。</summary>
	[Fact]
	public void Constructor_HonorTile_SetsSuitAndRank()
	{
		var tile = new Tile(TileSuit.Honor, 1);

		Assert.Equal(TileSuit.Honor, tile.Suit);
		Assert.Equal(1, tile.Rank);
	}

	/// <summary>パス条件: Suit=Pin, Rank=5, isRedFive=true で牌を生成すると、IsRedFive が true になること。</summary>
	[Fact]
	public void Constructor_RedFive_SetsIsRedFiveTrue()
	{
		var tile = new Tile(TileSuit.Pin, 5, isRedFive: true);

		Assert.True(tile.IsRedFive);
	}

	/// <summary>パス条件: 数牌(Man)で Rank に 0 または 10 を指定すると ArgumentOutOfRangeException になること。</summary>
	[Theory]
	[InlineData(0)]
	[InlineData(10)]
	public void Constructor_NumberTile_RankOutOfRange_Throws(int rank)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new Tile(TileSuit.Man, rank));
	}

	/// <summary>パス条件: 字牌(Honor)で Rank に 0 または 8 を指定すると ArgumentOutOfRangeException になること。</summary>
	[Theory]
	[InlineData(0)]
	[InlineData(8)]
	public void Constructor_HonorTile_RankOutOfRange_Throws(int rank)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new Tile(TileSuit.Honor, rank));
	}

	/// <summary>
	/// パス条件: 字牌に isRedFive=true を指定すると ArgumentException になること。
	/// Rank=5（白）は数牌の赤五と Rank が一致するため回帰的に確認する。
	/// </summary>
	[Theory]
	[InlineData(1)]
	[InlineData(5)]
	public void Constructor_HonorTile_WithRedFive_Throws(int rank)
	{
		Assert.Throws<ArgumentException>(() => new Tile(TileSuit.Honor, rank, isRedFive: true));
	}

	/// <summary>パス条件: 数牌で Rank≠5 に isRedFive=true を指定すると ArgumentException になること。</summary>
	[Fact]
	public void Constructor_NumberTile_RankNotFiveWithRedFive_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Tile(TileSuit.Man, 3, isRedFive: true));
	}

	/// <summary>パス条件: 同じ Suit/Rank/IsRedFive を持つ牌同士は == で true になること。</summary>
	[Fact]
	public void Equals_SameSuitRankAndRedFive_ReturnsTrue()
	{
		var a = new Tile(TileSuit.Sou, 7);
		var b = new Tile(TileSuit.Sou, 7);

		Assert.Equal(a, b);
		Assert.True(a == b);
	}

	/// <summary>パス条件: 赤五と通常の五は IsRedFive が異なるため == で false になること。</summary>
	[Fact]
	public void Equals_RedFiveAndNormalFive_ReturnsFalse()
	{
		var redFive = new Tile(TileSuit.Sou, 5, isRedFive: true);
		var normalFive = new Tile(TileSuit.Sou, 5);

		Assert.NotEqual(redFive, normalFive);
		Assert.False(redFive == normalFive);
	}

	/// <summary>パス条件: 数牌の ToString() が "{Rank}{suit文字}" 形式（例: "1m", "9s"）になること。</summary>
	[Theory]
	[InlineData(TileSuit.Man, 1, "1m")]
	[InlineData(TileSuit.Pin, 9, "9p")]
	[InlineData(TileSuit.Sou, 5, "5s")]
	public void ToString_NumberTile_ReturnsShorthandFormat(TileSuit suit, int rank, string expected)
	{
		var tile = new Tile(suit, rank);

		Assert.Equal(expected, tile.ToString());
	}

	/// <summary>パス条件: 赤五の ToString() が通常表記に "+" を付けた形式（例: "5p+"）になること。</summary>
	[Fact]
	public void ToString_RedFive_AppendsPlusMark()
	{
		var tile = new Tile(TileSuit.Pin, 5, isRedFive: true);

		Assert.Equal("5p+", tile.ToString());
	}

	/// <summary>パス条件: 字牌の ToString() が英名（East/South/West/North/White/Green/Red）になること。</summary>
	[Theory]
	[InlineData(1, "East")]
	[InlineData(2, "South")]
	[InlineData(3, "West")]
	[InlineData(4, "North")]
	[InlineData(5, "White")]
	[InlineData(6, "Green")]
	[InlineData(7, "Red")]
	public void ToString_HonorTile_ReturnsEnglishName(int rank, string expected)
	{
		var tile = new Tile(TileSuit.Honor, rank);

		Assert.Equal(expected, tile.ToString());
	}
}
