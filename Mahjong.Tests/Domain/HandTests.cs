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
}
