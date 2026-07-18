using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class HandTests
{
	private static List<Tile> CreateThirteenTiles()
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

	/// <summary>パス条件: 13枚のTileを渡すと ConcealedTiles が13枚になり、内容が一致すること。</summary>
	[Fact]
	public void Constructor_WithThirteenTiles_SetsConcealedTiles()
	{
		var startingTiles = CreateThirteenTiles();

		var hand = new Hand(startingTiles);

		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 13枚以外(12枚・14枚)を渡すと ArgumentException になること。</summary>
	[Theory]
	[InlineData(12)]
	[InlineData(14)]
	public void Constructor_WithWrongTileCount_Throws(int count)
	{
		var startingTiles = CreateThirteenTiles().Take(count).ToList();
		while (startingTiles.Count < count)
		{
			startingTiles.Add(new Tile(TileSuit.Sou, 9));
		}

		Assert.Throws<ArgumentException>(() => new Hand(startingTiles));
	}

	/// <summary>パス条件: Draw(tile) を呼ぶと ConcealedTiles が14枚になり、渡した牌が含まれること。</summary>
	[Fact]
	public void Draw_AddsTileAndBecomesFourteen()
	{
		var hand = new Hand(CreateThirteenTiles());
		var drawnTile = new Tile(TileSuit.Sou, 9);

		hand.Draw(drawnTile);

		Assert.Equal(14, hand.ConcealedTiles.Count);
		Assert.Contains(drawnTile, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 既に14枚(Draw済み)の状態で Draw(tile) を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Draw_WhenAlreadyFourteen_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<InvalidOperationException>(() => hand.Draw(new Tile(TileSuit.Sou, 8)));
	}

	/// <summary>パス条件: Discard(tile) を呼ぶと ConcealedTiles が13枚に戻り、指定した牌が取り除かれること。</summary>
	[Fact]
	public void Discard_RemovesTileAndBecomesThirteen()
	{
		var hand = new Hand(CreateThirteenTiles());
		var drawnTile = new Tile(TileSuit.Sou, 9);
		hand.Draw(drawnTile);

		hand.Discard(drawnTile);

		Assert.Equal(13, hand.ConcealedTiles.Count);
		Assert.DoesNotContain(drawnTile, hand.ConcealedTiles);
	}

	/// <summary>パス条件: Discard(tile) を呼ぶと Discards の末尾に指定した牌が追加されること。</summary>
	[Fact]
	public void Discard_AppendsTileToDiscards()
	{
		var hand = new Hand(CreateThirteenTiles());
		var drawnTile = new Tile(TileSuit.Sou, 9);
		hand.Draw(drawnTile);

		hand.Discard(drawnTile);

		Assert.Equal([drawnTile], hand.Discards);
	}

	/// <summary>パス条件: 手牌が13枚(ツモ前)の状態で Discard(tile) を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Discard_WhenNotDrawnYet_Throws()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);

		Assert.Throws<InvalidOperationException>(() => hand.Discard(startingTiles[0]));
	}

	/// <summary>パス条件: 手牌に存在しない牌を Discard(tile) に渡すと ArgumentException になること。</summary>
	[Fact]
	public void Discard_WithTileNotInHand_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<ArgumentException>(() => hand.Discard(new Tile(TileSuit.Honor, 1)));
	}

	/// <summary>パス条件: 赤五と通常五を両方持つ手牌から通常五をDiscardすると、赤五だけがConcealedTilesに残ること。</summary>
	[Fact]
	public void Discard_NormalFive_KeepsRedFiveWhenBothPresent()
	{
		var startingTiles = CreateThirteenTiles();
		startingTiles[0] = new Tile(TileSuit.Pin, 5, isRedFive: true);
		var hand = new Hand(startingTiles);
		var normalFive = new Tile(TileSuit.Pin, 5);
		hand.Draw(normalFive);

		hand.Discard(normalFive);

		Assert.Contains(new Tile(TileSuit.Pin, 5, isRedFive: true), hand.ConcealedTiles);
		Assert.DoesNotContain(normalFive, hand.ConcealedTiles);
	}

	/// <summary>パス条件: ツモ→打牌を2回繰り返すと、Discardsが捨てた順序で2枚並ぶこと。</summary>
	[Fact]
	public void Discard_TwiceInSequence_KeepsDiscardOrder()
	{
		var hand = new Hand(CreateThirteenTiles());
		var firstDraw = new Tile(TileSuit.Sou, 9);
		var secondDraw = new Tile(TileSuit.Sou, 8);

		hand.Draw(firstDraw);
		hand.Discard(firstDraw);
		hand.Draw(secondDraw);
		hand.Discard(secondDraw);

		Assert.Equal([firstDraw, secondDraw], hand.Discards);
	}
}
