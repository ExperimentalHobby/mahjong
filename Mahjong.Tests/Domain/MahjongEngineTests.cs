using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class MahjongEngineTests
{
	/// <summary>パス条件: Start() 直後は CurrentTurn が East で、4人全員の Hands がそれぞれ13枚配られていること。</summary>
	[Fact]
	public void Start_DealsThirteenTilesToEachSeatAndStartsWithEast()
	{
		var engine = MahjongEngine.Start(new Random(1));

		Assert.Equal(Seat.East, engine.CurrentTurn);
		foreach (var seat in new[] { Seat.East, Seat.South, Seat.West, Seat.North })
		{
			Assert.Equal(13, engine.Hands[seat].ConcealedTiles.Count);
		}
	}

	/// <summary>パス条件: DrawForCurrentPlayer() で現在の手番のHandに1枚加わり、LiveWallCountが1減ること。</summary>
	[Fact]
	public void DrawForCurrentPlayer_AddsTileToCurrentHandAndReducesLiveWallCount()
	{
		var engine = MahjongEngine.Start(new Random(1));
		var liveWallCountBefore = engine.LiveWallCount;

		engine.DrawForCurrentPlayer();

		Assert.Equal(14, engine.Hands[engine.CurrentTurn].ConcealedTiles.Count);
		Assert.Equal(liveWallCountBefore - 1, engine.LiveWallCount);
	}

	/// <summary>パス条件: Discard() で現在の手番のHandから指定牌が取り除かれ、CurrentTurnが次の席（東→南）に進むこと。</summary>
	[Fact]
	public void Discard_RemovesTileFromCurrentHandAndAdvancesTurn()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();
		var drawnTile = engine.Hands[Seat.East].ConcealedTiles[^1];

		engine.Discard(drawnTile);

		Assert.Equal(13, engine.Hands[Seat.East].ConcealedTiles.Count);
		Assert.Equal(Seat.South, engine.CurrentTurn);
	}

	/// <summary>パス条件: 4回分のDrawForCurrentPlayer()→Discard()で手番が東→南→西→北→東と一巡すること。</summary>
	[Fact]
	public void FourTurns_CycleThroughAllSeatsBackToEast()
	{
		var engine = MahjongEngine.Start(new Random(1));
		Seat[] expectedOrder = [Seat.South, Seat.West, Seat.North, Seat.East];

		foreach (var expectedNext in expectedOrder)
		{
			engine.DrawForCurrentPlayer();
			var drawnTile = engine.Hands[engine.CurrentTurn].ConcealedTiles[^1];

			engine.Discard(drawnTile);

			Assert.Equal(expectedNext, engine.CurrentTurn);
		}
	}

	/// <summary>パス条件: Clone() で複製したエンジンに対する操作が元のエンジンに影響しないこと。</summary>
	[Fact]
	public void Clone_IsIndependentFromOriginal()
	{
		var engine = MahjongEngine.Start(new Random(1));
		var clone = engine.Clone();

		engine.DrawForCurrentPlayer();
		var drawnTile = engine.Hands[Seat.East].ConcealedTiles[^1];
		engine.Discard(drawnTile);

		Assert.Equal(Seat.South, engine.CurrentTurn);
		Assert.Equal(Seat.East, clone.CurrentTurn);
		Assert.Equal(13, clone.Hands[Seat.East].ConcealedTiles.Count);
		Assert.Equal(engine.LiveWallCount + 1, clone.LiveWallCount);
	}

	/// <summary>パス条件: Discard() 後、LastDiscard に打牌した牌と打牌者（座席）が記録されること。</summary>
	[Fact]
	public void Discard_RecordsLastDiscard()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();
		var drawnTile = engine.Hands[Seat.East].ConcealedTiles[^1];

		engine.Discard(drawnTile);

		Assert.Equal((drawnTile, Seat.East), engine.LastDiscard);
	}

	/// <summary>パス条件: DrawForCurrentPlayer() を呼ぶと LastDiscard が null にクリアされること（誰も鳴かなかった扱い）。</summary>
	[Fact]
	public void DrawForCurrentPlayer_ClearsLastDiscard()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();
		var drawnTile = engine.Hands[Seat.East].ConcealedTiles[^1];
		engine.Discard(drawnTile);

		engine.DrawForCurrentPlayer();

		Assert.Null(engine.LastDiscard);
	}

	/// <summary>
	/// パス条件: 他家が直前の捨て牌に対してポンを宣言すると、
	/// その家のHand.Meldsに追加され、CurrentTurnがポンした家に移ること。
	/// </summary>
	[Fact]
	public void CallPon_ByOtherSeat_AddsMeldAndMovesTurnToCaller()
	{
		var discardedTile = new Tile(TileSuit.Pin, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(
			[
				discardedTile, discardedTile,
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		engine.CallPon(Seat.South, discardedTile, discardedTile);

		var meld = Assert.Single(engine.Hands[Seat.South].Melds);
		Assert.Equal(MeldType.Pon, meld.Type);
		Assert.Equal(Seat.South, engine.CurrentTurn);
		Assert.Null(engine.LastDiscard);
	}

	/// <summary>パス条件: 自分の捨て牌に対して自分でポンしようとすると ArgumentException になること。</summary>
	[Fact]
	public void CallPon_BySameSeatAsDiscarder_Throws()
	{
		var discardedTile = new Tile(TileSuit.Pin, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(
			[
				discardedTile, discardedTile,
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2),
			]),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallPon(Seat.East, discardedTile, discardedTile));
	}

	/// <summary>パス条件: LastDiscardが無い状態でCallPon()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void CallPon_WithoutLastDiscard_Throws()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();
		var pinOne = new Tile(TileSuit.Pin, 1);

		Assert.Throws<InvalidOperationException>(() => engine.CallPon(Seat.South, pinOne, pinOne));
	}

	/// <summary>
	/// パス条件: 上家（打牌者の下家＝NextSeat(打牌者)）がチーを宣言すると、
	/// Hand.Meldsに追加され、CurrentTurnがチーした家に移ること。
	/// </summary>
	[Fact]
	public void CallChi_ByNextSeat_AddsMeldAndMovesTurnToCaller()
	{
		var discardedTile = new Tile(TileSuit.Sou, 1);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		engine.CallChi(Seat.South, new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3));

		var meld = Assert.Single(engine.Hands[Seat.South].Melds);
		Assert.Equal(MeldType.Chi, meld.Type);
		Assert.Equal(Seat.South, engine.CurrentTurn);
		Assert.Null(engine.LastDiscard);
	}

	/// <summary>パス条件: 上家以外（対面・下家側）がチーしようとすると ArgumentException になること。</summary>
	[Fact]
	public void CallChi_ByNonNextSeat_Throws()
	{
		var discardedTile = new Tile(TileSuit.Sou, 1);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(
			[
				new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3),
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2),
			]),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(
			() => engine.CallChi(Seat.West, new Tile(TileSuit.Sou, 2), new Tile(TileSuit.Sou, 3)));
	}

	/// <summary>
	/// パス条件: 鳴き成立後、鳴いた家がDiscard()すると、そこからさらに次の席へ手番が進むこと
	/// （本来の手番順をスキップして鳴いた家から再開することの確認）。
	/// </summary>
	[Fact]
	public void AfterCall_Discard_AdvancesFromCallerSkippingOriginalOrder()
	{
		var discardedTile = new Tile(TileSuit.Pin, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(
			[
				discardedTile, discardedTile,
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 2),
			]),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		engine.CallPon(Seat.West, discardedTile, discardedTile);
		engine.Discard(new Tile(TileSuit.Man, 1));

		Assert.Equal(Seat.North, engine.CurrentTurn);
	}

	/// <summary>パス条件: 他家の捨て牌が自分の和了牌のときロンを宣言すると、Winnerがその家になること。</summary>
	[Fact]
	public void CallRon_WhenHandCompletesWithDiscardedTile_SetsWinner()
	{
		var discardedTile = new Tile(TileSuit.Man, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
				new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
				new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
				new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		engine.CallRon(Seat.South);

		Assert.Equal(Seat.South, engine.Winner);
	}

	/// <summary>パス条件: 自分の捨て牌に対して自分でロンしようとすると ArgumentException になること。</summary>
	[Fact]
	public void CallRon_BySameSeatAsDiscarder_Throws()
	{
		var discardedTile = new Tile(TileSuit.Man, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(
			[
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
				new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
				new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
				new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon(Seat.East));
	}

	/// <summary>パス条件: 手牌がロン牌を加えても完成しない場合、CallRon() が ArgumentException になること。</summary>
	[Fact]
	public void CallRon_HandDoesNotComplete_Throws()
	{
		var discardedTile = new Tile(TileSuit.Sou, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon(Seat.South));
	}

	/// <summary>パス条件: LastDiscardが無い状態でCallRon()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void CallRon_WithoutLastDiscard_Throws()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();

		Assert.Throws<InvalidOperationException>(() => engine.CallRon(Seat.South));
	}

	private static List<Tile> CreateThirteenFillerTiles()
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
