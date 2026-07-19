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

	private static List<Tile> CreateThirteenTilesWithDuplicatePinOne()
	{
		var tiles = CreateThirteenTiles();
		tiles[0] = new Tile(TileSuit.Pin, 1);
		return tiles;
	}

	/// <summary>パス条件: Pon()成立時、Meldsに Type=Pon, Tiles=[handTile1, handTile2, claimedTile] のMeldが追加されること。</summary>
	[Fact]
	public void Pon_AddsMeldToMelds()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var handTile1 = new Tile(TileSuit.Pin, 1);
		var handTile2 = new Tile(TileSuit.Pin, 1);
		var claimedTile = new Tile(TileSuit.Pin, 1);

		hand.Pon(claimedTile, handTile1, handTile2);

		var meld = Assert.Single(hand.Melds);
		Assert.Equal(MeldType.Pon, meld.Type);
		Assert.Equal([handTile1, handTile2, claimedTile], meld.Tiles);
	}

	/// <summary>パス条件: Pon()成立時、使用した2枚がConcealedTilesから取り除かれること(13→11枚)。</summary>
	[Fact]
	public void Pon_RemovesTwoTilesFromConcealedTiles()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);

		hand.Pon(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Equal(11, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: claimedTileとhandTile1のSuit/Rankが一致しない場合 ArgumentException になること。</summary>
	[Fact]
	public void Pon_WithHandTile1MismatchedKind_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		var mismatchedHandTile1 = new Tile(TileSuit.Pin, 2);

		Assert.Throws<ArgumentException>(() => hand.Pon(claimedTile, mismatchedHandTile1, new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>パス条件: claimedTileとhandTile2のSuit/Rankが一致しない場合 ArgumentException になること。</summary>
	[Fact]
	public void Pon_WithHandTile2MismatchedKind_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		var mismatchedHandTile2 = new Tile(TileSuit.Pin, 2);

		Assert.Throws<ArgumentException>(() => hand.Pon(claimedTile, new Tile(TileSuit.Pin, 1), mismatchedHandTile2));
	}

	/// <summary>パス条件: handTile1が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。</summary>
	[Fact]
	public void Pon_WithHandTile1NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTilesWithDuplicatePinOne();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Sou, 9);
		var missingHandTile1 = new Tile(TileSuit.Sou, 9);

		Assert.Throws<ArgumentException>(() => hand.Pon(claimedTile, missingHandTile1, new Tile(TileSuit.Sou, 9)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: handTile2が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。
	/// (手牌に1枚しかないPin2をhandTile1として消費した後、handTile2に同じPin2を指定して検証する)
	/// </summary>
	[Fact]
	public void Pon_WithHandTile2NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTilesWithDuplicatePinOne();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Pin, 2);

		Assert.Throws<ArgumentException>(() => hand.Pon(claimedTile, new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 打牌待ち状態(ツモ直後)でPon()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Pon_WhenPendingDiscard_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		hand.Draw(new Tile(TileSuit.Sou, 9));
		var claimedTile = new Tile(TileSuit.Pin, 1);

		Assert.Throws<InvalidOperationException>(
			() => hand.Pon(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>パス条件: Pon()成立後は打牌待ち状態になり、Draw()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Pon_ThenDraw_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		hand.Pon(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Throws<InvalidOperationException>(() => hand.Draw(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>パス条件: Pon()成立後、Discard()が正しく機能すること(11→10枚、Discardsに追加される)。</summary>
	[Fact]
	public void Pon_ThenDiscard_Succeeds()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		hand.Pon(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		var discardedTile = new Tile(TileSuit.Man, 2);

		hand.Discard(discardedTile);

		Assert.Equal(10, hand.ConcealedTiles.Count);
		Assert.Equal([discardedTile], hand.Discards);
	}

	/// <summary>パス条件: Chi()成立時、MeldsにType=Chi, Tiles=[Rank昇順の3枚]のMeldが追加されること。</summary>
	[Fact]
	public void Chi_AddsMeldToMeldsInAscendingRankOrder()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);
		var handTile1 = new Tile(TileSuit.Man, 3);
		var handTile2 = new Tile(TileSuit.Man, 1);

		hand.Chi(claimedTile, handTile1, handTile2);

		var meld = Assert.Single(hand.Melds);
		Assert.Equal(MeldType.Chi, meld.Type);
		Assert.Equal(
			[new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3)],
			meld.Tiles);
	}

	/// <summary>パス条件: Chi()成立時、使用した2枚がConcealedTilesから取り除かれること(13→11枚)。</summary>
	[Fact]
	public void Chi_RemovesTwoTilesFromConcealedTiles()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);

		hand.Chi(claimedTile, new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 1));

		Assert.Equal(11, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: claimedTileが字牌の場合 ArgumentException になること。</summary>
	[Fact]
	public void Chi_WithHonorClaimedTile_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Honor, 1);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 3)));
	}

	/// <summary>パス条件: handTile1がclaimedTileと異なるSuitの場合 ArgumentException になること。</summary>
	[Fact]
	public void Chi_WithHandTile1DifferentSuit_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);
		var mismatchedHandTile1 = new Tile(TileSuit.Pin, 1);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, mismatchedHandTile1, new Tile(TileSuit.Man, 3)));
	}

	/// <summary>
	/// パス条件: handTile2がclaimedTileと異なるSuitの場合 ArgumentException になること。
	/// (Rankだけ見ると連続に見えてしまうケースでSuit不一致が正しく検出されることを確認する)
	/// </summary>
	[Fact]
	public void Chi_WithHandTile2DifferentSuit_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);
		var mismatchedHandTile2 = new Tile(TileSuit.Pin, 3);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Man, 1), mismatchedHandTile2));
	}

	/// <summary>パス条件: Rankが3つの連続した整数になっていない場合(例: 1,2,4) ArgumentException になること。</summary>
	[Fact]
	public void Chi_WithNonConsecutiveRanks_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 1);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 4)));
	}

	/// <summary>パス条件: handTile1が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。</summary>
	[Fact]
	public void Chi_WithHandTile1NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Sou, 5);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 6)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: handTile2が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。
	/// (手牌にはPin1〜4しかないため、Pin5は存在しない。handTile1=Pin3は実在するが、
	/// アトミックに検証されhandTile1側も取り除かれていないことを確認する)
	/// </summary>
	[Fact]
	public void Chi_WithHandTile2NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Pin, 4);

		Assert.Throws<ArgumentException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 5)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 打牌待ち状態(ツモ直後)でChi()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Chi_WhenPendingDiscard_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));
		var claimedTile = new Tile(TileSuit.Man, 2);

		Assert.Throws<InvalidOperationException>(
			() => hand.Chi(claimedTile, new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 3)));
	}

	/// <summary>パス条件: Chi()成立後は打牌待ち状態になり、Draw()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Chi_ThenDraw_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);
		hand.Chi(claimedTile, new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 3));

		Assert.Throws<InvalidOperationException>(() => hand.Draw(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>パス条件: Chi()成立後、Discard()が正しく機能すること(11→10枚、Discardsに追加される)。</summary>
	[Fact]
	public void Chi_ThenDiscard_Succeeds()
	{
		var hand = new Hand(CreateThirteenTiles());
		var claimedTile = new Tile(TileSuit.Man, 2);
		hand.Chi(claimedTile, new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 3));
		var discardedTile = new Tile(TileSuit.Man, 9);

		hand.Discard(discardedTile);

		Assert.Equal(10, hand.ConcealedTiles.Count);
		Assert.Equal([discardedTile], hand.Discards);
	}

	private static List<Tile> CreateThirteenTilesWithTriplicatePinOne()
	{
		var tiles = CreateThirteenTiles();
		tiles[0] = new Tile(TileSuit.Pin, 1);
		tiles[1] = new Tile(TileSuit.Pin, 1);
		return tiles;
	}

	/// <summary>パス条件: OpenKan()成立時、MeldsにType=OpenKan, Tiles=[handTile1,2,3,claimedTile]のMeldが追加されること。</summary>
	[Fact]
	public void OpenKan_AddsMeldToMelds()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		var handTile1 = new Tile(TileSuit.Pin, 1);
		var handTile2 = new Tile(TileSuit.Pin, 1);
		var handTile3 = new Tile(TileSuit.Pin, 1);
		var claimedTile = new Tile(TileSuit.Pin, 1);

		hand.OpenKan(claimedTile, handTile1, handTile2, handTile3);

		var meld = Assert.Single(hand.Melds);
		Assert.Equal(MeldType.OpenKan, meld.Type);
		Assert.Equal([handTile1, handTile2, handTile3, claimedTile], meld.Tiles);
	}

	/// <summary>パス条件: OpenKan()成立時、使用した3枚がConcealedTilesから取り除かれること(13→10枚)。</summary>
	[Fact]
	public void OpenKan_RemovesThreeTilesFromConcealedTiles()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);

		hand.OpenKan(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Equal(10, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: handTile1/2/3のいずれかがclaimedTileと種類不一致の場合 ArgumentException になること。</summary>
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void OpenKan_WithMismatchedHandTile_Throws(int mismatchPosition)
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		var mismatched = new Tile(TileSuit.Pin, 2);
		var handTile1 = mismatchPosition == 1 ? mismatched : new Tile(TileSuit.Pin, 1);
		var handTile2 = mismatchPosition == 2 ? mismatched : new Tile(TileSuit.Pin, 1);
		var handTile3 = mismatchPosition == 3 ? mismatched : new Tile(TileSuit.Pin, 1);

		Assert.Throws<ArgumentException>(() => hand.OpenKan(claimedTile, handTile1, handTile2, handTile3));
	}

	/// <summary>パス条件: handTile1が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。</summary>
	[Fact]
	public void OpenKan_WithHandTile1NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Sou, 5);

		Assert.Throws<ArgumentException>(() => hand.OpenKan(
			claimedTile, new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: handTile3(最後に検証する牌)が手牌に存在しない場合 ArgumentException になり、
	/// ConcealedTilesは変化しないこと(先に2枚の照合が成功していてもロールバックされることを確認する)。
	/// </summary>
	[Fact]
	public void OpenKan_WithHandTile3NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTilesWithDuplicatePinOne();
		var hand = new Hand(startingTiles);
		var claimedTile = new Tile(TileSuit.Pin, 1);

		Assert.Throws<ArgumentException>(() => hand.OpenKan(
			claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1)));
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 打牌待ち状態(ツモ直後)でOpenKan()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void OpenKan_WhenPendingDiscard_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Sou, 9));
		var claimedTile = new Tile(TileSuit.Pin, 1);

		Assert.Throws<InvalidOperationException>(() => hand.OpenKan(
			claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>
	/// パス条件: OpenKan()成立後も打牌待ちにならない(まだツモしていない)ため、
	/// Draw()を呼んでも例外にならないこと。
	/// </summary>
	[Fact]
	public void OpenKan_ThenDraw_Succeeds()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		hand.OpenKan(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Equal(11, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: OpenKan()成立直後(ツモ前)にDiscard()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void OpenKan_ThenDiscardImmediately_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		var claimedTile = new Tile(TileSuit.Pin, 1);
		hand.OpenKan(claimedTile, new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Throws<InvalidOperationException>(() => hand.Discard(new Tile(TileSuit.Man, 3)));
	}

	/// <summary>パス条件: ClosedKan()成立時、MeldsにType=ClosedKan, Tiles=[tile1,2,3,4]のMeldが追加されること。</summary>
	[Fact]
	public void ClosedKan_AddsMeldToMelds()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Pin, 1));
		var tile1 = new Tile(TileSuit.Pin, 1);
		var tile2 = new Tile(TileSuit.Pin, 1);
		var tile3 = new Tile(TileSuit.Pin, 1);
		var tile4 = new Tile(TileSuit.Pin, 1);

		hand.ClosedKan(tile1, tile2, tile3, tile4);

		var meld = Assert.Single(hand.Melds);
		Assert.Equal(MeldType.ClosedKan, meld.Type);
		Assert.Equal([tile1, tile2, tile3, tile4], meld.Tiles);
	}

	/// <summary>パス条件: ClosedKan()成立時、使用した4枚がConcealedTilesから取り除かれること(14→10枚)。</summary>
	[Fact]
	public void ClosedKan_RemovesFourTilesFromConcealedTiles()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Pin, 1));

		hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Equal(10, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: tile2/3/4のいずれかがtile1と種類不一致の場合 ArgumentException になること。</summary>
	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	public void ClosedKan_WithMismatchedTile_Throws(int mismatchPosition)
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Pin, 1));
		var mismatched = new Tile(TileSuit.Pin, 2);
		var tile1 = new Tile(TileSuit.Pin, 1);
		var tile2 = mismatchPosition == 2 ? mismatched : new Tile(TileSuit.Pin, 1);
		var tile3 = mismatchPosition == 3 ? mismatched : new Tile(TileSuit.Pin, 1);
		var tile4 = mismatchPosition == 4 ? mismatched : new Tile(TileSuit.Pin, 1);

		Assert.Throws<ArgumentException>(() => hand.ClosedKan(tile1, tile2, tile3, tile4));
	}

	/// <summary>
	/// パス条件: tile2が手牌に存在しない場合 ArgumentException になり、ConcealedTilesは変化しないこと。
	/// (手牌にはMan5が1枚しかないため、tile1で消費された後のtile2は存在しない)
	/// </summary>
	[Fact]
	public void ClosedKan_WithTile2NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));
		var expectedTiles = hand.ConcealedTiles.ToList();

		Assert.Throws<ArgumentException>(() => hand.ClosedKan(
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5)));
		Assert.Equal(expectedTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: tile4(最後に検証する牌)が手牌に存在しない場合 ArgumentException になり、
	/// ConcealedTilesは変化しないこと(先に3枚の照合が成功していてもロールバックされることを確認する)。
	/// </summary>
	[Fact]
	public void ClosedKan_WithTile4NotInHand_ThrowsAndLeavesHandUnchanged()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Sou, 9));
		var expectedTiles = hand.ConcealedTiles.ToList();

		Assert.Throws<ArgumentException>(() => hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1)));
		Assert.Equal(expectedTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: 打牌待ちでない状態(ツモ前)でClosedKan()を呼ぶと InvalidOperationException になること。
	/// (明槓・ポン・チーとは逆方向の判定であることを確認する)
	/// </summary>
	[Fact]
	public void ClosedKan_WhenNotPending_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());

		Assert.Throws<InvalidOperationException>(() => hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>
	/// パス条件: ClosedKan()成立後は打牌待ちにならない(嶺上牌のツモがまだのため)ため、
	/// Draw()を呼んでも例外にならないこと。
	/// </summary>
	[Fact]
	public void ClosedKan_ThenDraw_Succeeds()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Pin, 1));
		hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Equal(11, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: ClosedKan()成立直後(嶺上牌ツモ前)にDiscard()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void ClosedKan_ThenDiscardImmediately_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithTriplicatePinOne());
		hand.Draw(new Tile(TileSuit.Pin, 1));
		hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Throws<InvalidOperationException>(() => hand.Discard(new Tile(TileSuit.Man, 3)));
	}

	/// <summary>
	/// ポン(Pin1x3)成立済み・不要牌を打牌済み・4枚目のPin1をツモした状態の手牌を作る。
	/// AddedKan()のテスト用共通セットアップ。
	/// </summary>
	private static Hand CreateHandWithPonAndDrawnFourthTile()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));
		hand.Draw(new Tile(TileSuit.Pin, 1));
		return hand;
	}

	/// <summary>パス条件: AddedKan()成立時、既存のPonがMeldsから削除され、Type=AddedKan・Tiles=[元のPonの3枚..., tile]のMeldが追加されること。</summary>
	[Fact]
	public void AddedKan_ReplacesExistingPonWithAddedKanMeld()
	{
		var hand = CreateHandWithPonAndDrawnFourthTile();
		var originalPonTiles = hand.Melds.Single().Tiles;
		var tile = new Tile(TileSuit.Pin, 1);

		hand.AddedKan(tile);

		var meld = Assert.Single(hand.Melds);
		Assert.Equal(MeldType.AddedKan, meld.Type);
		Assert.Equal([.. originalPonTiles, tile], meld.Tiles);
	}

	/// <summary>パス条件: AddedKan()成立時、使用したtileがConcealedTilesから取り除かれること(1枚減る)。</summary>
	[Fact]
	public void AddedKan_RemovesOneTileFromConcealedTiles()
	{
		var hand = CreateHandWithPonAndDrawnFourthTile();
		var countBefore = hand.ConcealedTiles.Count;

		hand.AddedKan(new Tile(TileSuit.Pin, 1));

		Assert.Equal(countBefore - 1, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: 対応する種類のPonがMeldsに存在しない場合 ArgumentException になること。</summary>
	[Fact]
	public void AddedKan_WithoutMatchingPon_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Pin, 1));

		Assert.Throws<ArgumentException>(() => hand.AddedKan(new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>
	/// パス条件: tileが手牌に存在しない場合 ArgumentException になること。
	/// (Pin1のポンは成立済みだが、今回ツモったのはPin1ではないため手牌にPin1が残っていない)
	/// </summary>
	[Fact]
	public void AddedKan_WithTileNotInHand_Throws()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<ArgumentException>(() => hand.AddedKan(new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>パス条件: 打牌待ちでない状態(ツモ前)でAddedKan()を呼ぶと InvalidOperationException になること(暗槓と同じ方向の判定)。</summary>
	[Fact]
	public void AddedKan_WhenNotPending_Throws()
	{
		var hand = CreateHandWithPonAndDrawnFourthTile();
		hand.Discard(new Tile(TileSuit.Man, 2));

		Assert.Throws<InvalidOperationException>(() => hand.AddedKan(new Tile(TileSuit.Pin, 1)));
	}

	/// <summary>
	/// パス条件: AddedKan()成立後は打牌待ちにならない(嶺上牌のツモがまだのため)ため、
	/// Draw()を呼んでも例外にならないこと。
	/// </summary>
	[Fact]
	public void AddedKan_ThenDraw_Succeeds()
	{
		var hand = CreateHandWithPonAndDrawnFourthTile();
		var countBeforeAddedKan = hand.ConcealedTiles.Count;
		hand.AddedKan(new Tile(TileSuit.Pin, 1));

		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Equal(countBeforeAddedKan, hand.ConcealedTiles.Count);
	}

	/// <summary>パス条件: AddedKan()成立直後(嶺上牌ツモ前)にDiscard()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void AddedKan_ThenDiscardImmediately_Throws()
	{
		var hand = CreateHandWithPonAndDrawnFourthTile();
		hand.AddedKan(new Tile(TileSuit.Pin, 1));

		Assert.Throws<InvalidOperationException>(() => hand.Discard(new Tile(TileSuit.Man, 2)));
	}

	/// <summary>パス条件: 副露なし・標準形で完成している14枚の場合、IsComplete() が true になること。</summary>
	[Fact]
	public void IsComplete_NoMelds_StandardForm_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Man, 9));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: 副露なし・七対子で完成している14枚の場合、IsComplete() が true になること。</summary>
	[Fact]
	public void IsComplete_NoMelds_SevenPairs_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
			new Tile(TileSuit.Honor, 1),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Honor, 1));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: 副露なし・国士無双で完成している14枚の場合、IsComplete() が true になること。</summary>
	[Fact]
	public void IsComplete_NoMelds_ThirteenOrphans_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Honor, 7));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: 副露なし・完成していない14枚の場合、IsComplete() が false になること。</summary>
	[Fact]
	public void IsComplete_NoMelds_Incomplete_ReturnsFalse()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.False(hand.IsComplete());
	}

	/// <summary>パス条件: ポン1つ+残り11枚が3面子+雀頭に分解できる場合、IsComplete() が true になること。</summary>
	[Fact]
	public void IsComplete_OnePon_RemainingTilesComplete_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: ポン1つ+残り11枚が分解できない場合、IsComplete() が false になること。</summary>
	[Fact]
	public void IsComplete_OnePon_RemainingTilesIncomplete_ReturnsFalse()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.False(hand.IsComplete());
	}

	/// <summary>
	/// パス条件: 暗槓（4枚の副露）1つ+残り11枚が3面子+雀頭に分解できる場合、IsComplete() が true になること
	/// （カンの牌数が4枚でも面子1つ分としてしか数えないことの確認）。
	/// </summary>
	[Fact]
	public void IsComplete_OneClosedKan_RemainingTilesComplete_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Pin, 1));
		hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: ポン+チーの2副露+残り8枚が2面子+雀頭に分解できる場合、IsComplete() が true になること。</summary>
	[Fact]
	public void IsComplete_PonAndChi_RemainingTilesComplete_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9), new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Pin, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Pin, 9));
		hand.Chi(new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3));
		hand.Discard(new Tile(TileSuit.Sou, 9));
		hand.Draw(new Tile(TileSuit.Man, 9));

		Assert.True(hand.IsComplete());
	}

	/// <summary>パス条件: 打牌待ちでない状態（ツモ前）で IsComplete() を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void IsComplete_WhenNotPending_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());

		Assert.Throws<InvalidOperationException>(() => hand.IsComplete());
	}

	/// <summary>パス条件: 副露なし・打牌待ちでない状態で CalculateShanten() を呼ぶと、ShantenCalculatorへの委譲結果(1)が返ること。</summary>
	[Fact]
	public void CalculateShanten_NoMelds_DelegatesToShantenCalculator()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);

		Assert.Equal(1, hand.CalculateShanten());
	}

	/// <summary>パス条件: ポン1つ+残り10枚がテンパイ形の場合、CalculateShanten() が 0 になること。</summary>
	[Fact]
	public void CalculateShanten_OnePon_Tenpai_ReturnsZero()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));

		Assert.Equal(0, hand.CalculateShanten());
	}

	/// <summary>パス条件: ポン1つ+残り10枚がテンパイから離れた形の場合、CalculateShanten() が 4 になること。</summary>
	[Fact]
	public void CalculateShanten_OnePon_FourShanten_ReturnsFour()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));

		Assert.Equal(4, hand.CalculateShanten());
	}

	/// <summary>
	/// パス条件: 暗槓（4枚の副露）1つ+残り10枚がテンパイ形の場合、CalculateShanten() が 0 になること
	/// （カンの牌数が4枚でも面子1つ分としてしか数えないことの確認）。
	/// </summary>
	[Fact]
	public void CalculateShanten_OneClosedKan_Tenpai_ReturnsZero()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Pin, 1));
		hand.ClosedKan(
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));

		Assert.Equal(0, hand.CalculateShanten());
	}

	/// <summary>パス条件: ポン+チーの2副露+残り7枚がテンパイ形の場合、CalculateShanten() が 0 になること。</summary>
	[Fact]
	public void CalculateShanten_PonAndChi_Tenpai_ReturnsZero()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Pin, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Pin, 9));
		hand.Chi(new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3));
		hand.Discard(new Tile(TileSuit.Sou, 9));

		Assert.Equal(0, hand.CalculateShanten());
	}

	/// <summary>パス条件: 打牌待ち状態（ツモ直後）で CalculateShanten() を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void CalculateShanten_WhenPending_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<InvalidOperationException>(() => hand.CalculateShanten());
	}

	/// <summary>パス条件: Clone() で複製した Hand の ConcealedTiles/Discards/Melds が元と同じ内容であること。</summary>
	[Fact]
	public void Clone_CopiesConcealedTilesDiscardsAndMelds()
	{
		var hand = new Hand(CreateThirteenTilesWithDuplicatePinOne());
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));

		var clone = hand.Clone();

		Assert.Equal(hand.ConcealedTiles, clone.ConcealedTiles);
		Assert.Equal(hand.Discards, clone.Discards);
		Assert.Equal(hand.Melds, clone.Melds);
	}

	/// <summary>パス条件: Clone() 後、元の Hand に対する操作（Draw()等）が複製に影響しないこと。</summary>
	[Fact]
	public void Clone_IsIndependentFromOriginal()
	{
		var hand = new Hand(CreateThirteenTiles());
		var clone = hand.Clone();

		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Equal(14, hand.ConcealedTiles.Count);
		Assert.Equal(13, clone.ConcealedTiles.Count);
	}

	/// <summary>パス条件: 副露なし・ロン牌を加えると標準形で完成する場合、CanWinOn() が true になること。</summary>
	[Fact]
	public void CanWinOn_NoMelds_StandardForm_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);

		Assert.True(hand.CanWinOn(new Tile(TileSuit.Man, 9)));
	}

	/// <summary>パス条件: 副露なし・ロン牌を加えると七対子で完成する場合、CanWinOn() が true になること。</summary>
	[Fact]
	public void CanWinOn_NoMelds_SevenPairs_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 5),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
			new Tile(TileSuit.Honor, 1),
		];
		var hand = new Hand(startingTiles);

		Assert.True(hand.CanWinOn(new Tile(TileSuit.Honor, 1)));
	}

	/// <summary>パス条件: ロン牌を加えても完成しない場合、CanWinOn() が false になること。</summary>
	[Fact]
	public void CanWinOn_TileDoesNotCompleteHand_ReturnsFalse()
	{
		var hand = new Hand(CreateThirteenTiles());

		Assert.False(hand.CanWinOn(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>
	/// パス条件: 副露あり（ポン1つ）・ロン牌を加えると残りが3面子+雀頭に分解できる場合、
	/// CanWinOn() が true になること。
	/// </summary>
	[Fact]
	public void CanWinOn_OnePon_RemainingTilesCompleteWithRonTile_ReturnsTrue()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));

		Assert.True(hand.CanWinOn(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>パス条件: 打牌待ち（ツモ直後）の状態で CanWinOn() を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void CanWinOn_WhenPending_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<InvalidOperationException>(() => hand.CanWinOn(new Tile(TileSuit.Sou, 8)));
	}

	/// <summary>パス条件: CanWinOn() 呼び出し後も ConcealedTiles の内容が変化しないこと（非破壊であることの確認）。</summary>
	[Fact]
	public void CanWinOn_DoesNotMutateConcealedTiles()
	{
		var startingTiles = CreateThirteenTiles();
		var hand = new Hand(startingTiles);

		hand.CanWinOn(new Tile(TileSuit.Sou, 9));

		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: ツモ和了後の手牌（副露なし・断幺九の標準形）から DetermineYaku() で Yaku.Tanyao が判定できること。</summary>
	[Fact]
	public void DetermineYaku_NoMelds_StandardForm_ContainsTanyao()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8),
			new Tile(TileSuit.Pin, 2),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Pin, 2));

		var yaku = hand.DetermineYaku();

		Assert.Contains(Yaku.Tanyao, yaku);
	}

	/// <summary>
	/// パス条件: ロン牌tileを仮に加えると断幺九の標準形で完成する場合、DetermineYakuOn(tile) で
	/// Yaku.Tanyao が判定でき、呼び出し後も ConcealedTiles が変化しないこと（非破壊であることの確認）。
	/// </summary>
	[Fact]
	public void DetermineYakuOn_NoMelds_StandardForm_ContainsTanyaoAndDoesNotMutateConcealedTiles()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 5),
			new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 6),
			new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7), new Tile(TileSuit.Sou, 8),
			new Tile(TileSuit.Pin, 2),
		];
		var hand = new Hand(startingTiles);

		var yaku = hand.DetermineYakuOn(new Tile(TileSuit.Pin, 2));

		Assert.Contains(Yaku.Tanyao, yaku);
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>
	/// パス条件: 副露なし・指定した牌を打牌すると聴牌になる場合、Riichi(tile) で IsRiichi が true になり、
	/// 打牌（Discardsへの追加・打牌待ち解除）も成立すること。
	/// </summary>
	[Fact]
	public void Riichi_NoMelds_DiscardLeavesTenpai_SetsIsRiichiAndDiscards()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));

		hand.Riichi(new Tile(TileSuit.Sou, 9));

		Assert.True(hand.IsRiichi);
		Assert.Contains(new Tile(TileSuit.Sou, 9), hand.Discards);
		Assert.Equal(startingTiles, hand.ConcealedTiles);
	}

	/// <summary>パス条件: 副露がある場合、Riichi(tile) が ArgumentException になること（門前限定）。</summary>
	[Fact]
	public void Riichi_WithMeld_Throws()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Pon(new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1));
		hand.Discard(new Tile(TileSuit.Man, 9));
		hand.Draw(new Tile(TileSuit.Sou, 9));

		Assert.Throws<ArgumentException>(() => hand.Riichi(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>パス条件: 指定した牌を打牌しても聴牌にならない場合、Riichi(tile) が ArgumentException になること。</summary>
	[Fact]
	public void Riichi_DiscardDoesNotLeaveTenpai_Throws()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 7),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 4),
			new Tile(TileSuit.Honor, 5),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Honor, 6));

		Assert.Throws<ArgumentException>(() => hand.Riichi(new Tile(TileSuit.Honor, 6)));
	}

	/// <summary>パス条件: 既にリーチ宣言済みの状態で再度 Riichi() を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void Riichi_WhenAlreadyRiichi_Throws()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));
		hand.Riichi(new Tile(TileSuit.Sou, 9));
		hand.Draw(new Tile(TileSuit.Pin, 9));

		Assert.Throws<InvalidOperationException>(() => hand.Riichi(new Tile(TileSuit.Pin, 9)));
	}

	/// <summary>
	/// パス条件: 打牌待ちでない状態（ツモ前）で Riichi() を呼ぶと InvalidOperationException になること
	/// （EnsurePending からの伝播確認）。
	/// </summary>
	[Fact]
	public void Riichi_WhenNotPending_Throws()
	{
		var hand = new Hand(CreateThirteenTiles());

		Assert.Throws<InvalidOperationException>(() => hand.Riichi(new Tile(TileSuit.Man, 1)));
	}

	/// <summary>パス条件: リーチ後、直前にツモった牌以外を Discard() しようとすると ArgumentException になること。</summary>
	[Fact]
	public void Discard_AfterRiichi_WithTileOtherThanDrawnTile_Throws()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));
		hand.Riichi(new Tile(TileSuit.Sou, 9));
		hand.Draw(new Tile(TileSuit.Pin, 9));

		Assert.Throws<ArgumentException>(() => hand.Discard(new Tile(TileSuit.Man, 1)));
	}

	/// <summary>パス条件: リーチ後、直前にツモった牌は Discard() で問題なく打牌できること。</summary>
	[Fact]
	public void Discard_AfterRiichi_WithDrawnTile_Succeeds()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));
		hand.Riichi(new Tile(TileSuit.Sou, 9));
		hand.Draw(new Tile(TileSuit.Pin, 9));

		hand.Discard(new Tile(TileSuit.Pin, 9));

		Assert.Contains(new Tile(TileSuit.Pin, 9), hand.Discards);
	}

	/// <summary>パス条件: リーチ宣言後に Clone() すると、複製した手牌にも IsRiichi が保持されること。</summary>
	[Fact]
	public void Clone_PreservesIsRiichi()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var hand = new Hand(startingTiles);
		hand.Draw(new Tile(TileSuit.Sou, 9));
		hand.Riichi(new Tile(TileSuit.Sou, 9));

		var clone = hand.Clone();

		Assert.True(clone.IsRiichi);
	}
}
