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

	/// <summary>パス条件: ツモ和了成立後に Clone() すると、複製したエンジンにも WinningYaku が保持されること。</summary>
	[Fact]
	public void Clone_PreservesWinningYaku()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var eastHand = new Hand(startingTiles);
		eastHand.Draw(new Tile(TileSuit.Man, 9));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);
		engine.CallTsumo();

		var clone = engine.Clone();

		Assert.Contains(Yaku.Toitoitsu, clone.WinningYaku[Seat.East]);
	}

	/// <summary>パス条件: ダブロン成立後に Clone() すると、複製したエンジンにも Winners・WinningYaku が保持されること。</summary>
	[Fact]
	public void Clone_PreservesWinnersAndWinningYaku_AfterDoubleRon()
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.West, (discardedTile, Seat.North));
		engine.CallRon([Seat.East, Seat.South]);

		var clone = engine.Clone();

		Assert.Equal(2, clone.Winners.Count);
		Assert.Contains(Seat.East, clone.Winners);
		Assert.Contains(Seat.South, clone.Winners);
		Assert.Contains(Yaku.Toitoitsu, clone.WinningYaku[Seat.East]);
		Assert.Contains(Yaku.Toitoitsu, clone.WinningYaku[Seat.South]);
	}

	/// <summary>パス条件: トリプルロン（三家和）成立後に Clone() すると、複製したエンジンにも IsTripleRonDraw が保持されること。</summary>
	[Fact]
	public void Clone_PreservesIsTripleRonDraw()
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(
			[
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
				new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
				new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
				new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.North, (discardedTile, Seat.North));
		engine.CallRon([Seat.East, Seat.South, Seat.West]);

		var clone = engine.Clone();

		Assert.True(clone.IsTripleRonDraw);
		Assert.Empty(clone.Winners);
	}

	/// <summary>パス条件: Riichi() 後、手番が次の座席に進み、LastDiscard に打牌した牌と打牌者が記録されること。</summary>
	[Fact]
	public void Riichi_MovesTurnToNextSeatAndRecordsLastDiscard()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var eastHand = new Hand(startingTiles);
		eastHand.Draw(new Tile(TileSuit.Sou, 9));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);
		var riichiTile = new Tile(TileSuit.Sou, 9);

		engine.Riichi(riichiTile);

		Assert.Equal(Seat.South, engine.CurrentTurn);
		Assert.Equal((riichiTile, Seat.East), engine.LastDiscard);
		Assert.True(engine.Hands[Seat.East].IsRiichi);
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

	/// <summary>パス条件: 2人が同じ捨て牌に対して同時にロンを宣言すると、頭跳ねせず両者ともWinnersに含まれること（ダブロン）。</summary>
	[Fact]
	public void CallRon_TwoCallers_BothIncludedInWinners()
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.West, (discardedTile, Seat.North));

		engine.CallRon([Seat.East, Seat.South]);

		Assert.Equal(2, engine.Winners.Count);
		Assert.Contains(Seat.East, engine.Winners);
		Assert.Contains(Seat.South, engine.Winners);
	}

	/// <summary>パス条件: ダブロン成立時、和了者ごとにWinningYakuが各自の手牌に基づいて設定されること。</summary>
	[Fact]
	public void CallRon_TwoCallers_SetsWinningYakuPerCaller()
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.West, (discardedTile, Seat.North));

		engine.CallRon([Seat.East, Seat.South]);

		Assert.Contains(Yaku.Toitoitsu, engine.WinningYaku[Seat.East]);
		Assert.Contains(Yaku.Toitoitsu, engine.WinningYaku[Seat.South]);
	}

	/// <summary>
	/// パス条件: 3人が同じ捨て牌に対して同時にロンを宣言すると、IsTripleRonDraw が true になり、
	/// Winners は空のままであること（三家和）。
	/// </summary>
	[Fact]
	public void CallRon_ThreeCallers_SetsIsTripleRonDrawAndWinnersRemainsEmpty()
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(
			[
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
				new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
				new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
				new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.North, (discardedTile, Seat.North));

		engine.CallRon([Seat.East, Seat.South, Seat.West]);

		Assert.True(engine.IsTripleRonDraw);
		Assert.Empty(engine.Winners);
	}

	/// <summary>パス条件: CallRon(callers) に空のリストを渡すと ArgumentException になること。</summary>
	[Fact]
	public void CallRon_EmptyCallers_Throws()
	{
		var discardedTile = new Tile(TileSuit.Man, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon([]));
	}

	/// <summary>パス条件: CallRon(callers) に同じ座席を複数回指定すると ArgumentException になること。</summary>
	[Fact]
	public void CallRon_DuplicateCallers_Throws()
	{
		var discardedTile = new Tile(TileSuit.Man, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon([Seat.South, Seat.South]));
	}

	/// <summary>
	/// パス条件: CallRon(callers) の複数座席のうち、いずれかが打牌者自身の場合 ArgumentException になること。
	/// </summary>
	[Fact]
	public void CallRon_CallersIncludeDiscarder_Throws()
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
			Wall.CreateShuffled(new Random(1)), hands, Seat.West, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon([Seat.South, Seat.East]));
	}

	/// <summary>
	/// パス条件: CallRon(callers) の複数座席のうち、いずれかの手牌が捨て牌を加えても和了形にならない場合、
	/// ArgumentException になり、Winners が変化しないこと（All-or-nothingの検証確認）。
	/// </summary>
	[Fact]
	public void CallRon_OneOfMultipleCallersHandDoesNotComplete_ThrowsAndWinnersRemainsEmpty()
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
			Wall.CreateShuffled(new Random(1)), hands, Seat.North, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(() => engine.CallRon([Seat.South, Seat.West]));
		Assert.Empty(engine.Winners);
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

		Assert.Equal([Seat.South], engine.Winners);
	}

	/// <summary>パス条件: 他家の捨て牌が自分の和了牌のときロンを宣言すると、WinningYaku に成立した役が設定されること。</summary>
	[Fact]
	public void CallRon_WhenHandCompletesWithDiscardedTile_SetsWinningYaku()
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

		Assert.Contains(Yaku.Toitoitsu, engine.WinningYaku[Seat.South]);
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

	/// <summary>パス条件: 現在の手番の手牌がツモ和了形の場合、CallTsumo() で Winner が現在の手番になること。</summary>
	[Fact]
	public void CallTsumo_WhenHandIsComplete_SetsWinner()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var eastHand = new Hand(startingTiles);
		eastHand.Draw(new Tile(TileSuit.Man, 9));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);

		engine.CallTsumo();

		Assert.Equal([Seat.East], engine.Winners);
	}

	/// <summary>パス条件: 現在の手番の手牌がツモ和了形の場合、CallTsumo() で WinningYaku に成立した役が設定されること。</summary>
	[Fact]
	public void CallTsumo_WhenHandIsComplete_SetsWinningYaku()
	{
		List<Tile> startingTiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3), new Tile(TileSuit.Sou, 3),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Man, 9),
		];
		var eastHand = new Hand(startingTiles);
		eastHand.Draw(new Tile(TileSuit.Man, 9));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);

		engine.CallTsumo();

		Assert.Contains(Yaku.Toitoitsu, engine.WinningYaku[Seat.East]);
	}

	/// <summary>パス条件: 現在の手番の手牌が和了形になっていない場合、CallTsumo() が ArgumentException になること。</summary>
	[Fact]
	public void CallTsumo_WhenHandIsIncomplete_Throws()
	{
		var eastHand = new Hand(CreateThirteenFillerTiles());
		eastHand.Draw(new Tile(TileSuit.Sou, 9));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);

		Assert.Throws<ArgumentException>(() => engine.CallTsumo());
	}

	/// <summary>
	/// パス条件: 打牌待ちでない状態（ツモ前）で CallTsumo() を呼ぶと InvalidOperationException になること
	/// （Hand.IsComplete() からの伝播確認）。
	/// </summary>
	[Fact]
	public void CallTsumo_WhenNotPending_Throws()
	{
		var engine = MahjongEngine.Start(new Random(1));

		Assert.Throws<InvalidOperationException>(() => engine.CallTsumo());
	}

	/// <summary>パス条件: 生牌山が0枚かつ誰も和了していない場合、IsExhaustiveDraw が true になること。</summary>
	[Fact]
	public void IsExhaustiveDraw_NoLiveTilesAndNoWinner_ReturnsTrue()
	{
		var wall = new Wall([], Wall.CreateStandardTileSet().Take(14).ToList());
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(wall, hands, Seat.East, lastDiscard: null);

		Assert.True(engine.IsExhaustiveDraw);
	}

	/// <summary>パス条件: 生牌山が0枚でも誰かが和了している場合、IsExhaustiveDraw が false になること（Winnerが優先される）。</summary>
	[Fact]
	public void IsExhaustiveDraw_NoLiveTilesButHasWinner_ReturnsFalse()
	{
		var wall = new Wall([], Wall.CreateStandardTileSet().Take(14).ToList());
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(wall, hands, Seat.East, lastDiscard: null, winners: [Seat.East]);

		Assert.False(engine.IsExhaustiveDraw);
	}

	/// <summary>パス条件: 生牌山が残っている場合、IsExhaustiveDraw が false になること。</summary>
	[Fact]
	public void IsExhaustiveDraw_LiveTilesRemain_ReturnsFalse()
	{
		var engine = MahjongEngine.Start(new Random(1));

		Assert.False(engine.IsExhaustiveDraw);
	}

	/// <summary>
	/// パス条件: トリプルロン（三家和）成立時は、生牌山が0枚でも IsExhaustiveDraw が false になること
	/// （三家和と荒牌流局を排他にすることの確認）。
	/// </summary>
	[Fact]
	public void IsExhaustiveDraw_NoLiveTilesButIsTripleRonDraw_ReturnsFalse()
	{
		var discardedTile = new Tile(TileSuit.Man, 9);
		var wall = new Wall([], Wall.CreateStandardTileSet().Take(14).ToList());
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
			[Seat.South] = new Hand(
			[
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 4),
				new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
				new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 6),
				new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.West] = new Hand(
			[
				new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 1),
				new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 4),
				new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 1),
				new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3), new Tile(TileSuit.Honor, 3),
				new Tile(TileSuit.Man, 9),
			]),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(wall, hands, Seat.North, (discardedTile, Seat.North));

		engine.CallRon([Seat.East, Seat.South, Seat.West]);

		Assert.False(engine.IsExhaustiveDraw);
	}

	/// <summary>
	/// パス条件: 他家の捨て牌に対して大明槓を宣言すると、Hand.Meldsに追加され、
	/// CurrentTurnがカンした家に移り、嶺上牌をツモった状態（打牌待ち）になること。
	/// </summary>
	[Fact]
	public void CallOpenKan_ByOtherSeat_AddsMeldMovesTurnAndDrawsReplacement()
	{
		var discardedTile = new Tile(TileSuit.Pin, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(CreateThirteenFillerTiles()),
			[Seat.South] = new Hand(
			[
				discardedTile, discardedTile, discardedTile,
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1),
			]),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));
		var liveWallCountBefore = engine.LiveWallCount;

		engine.CallOpenKan(Seat.South, discardedTile, discardedTile, discardedTile);

		var meld = Assert.Single(engine.Hands[Seat.South].Melds);
		Assert.Equal(MeldType.OpenKan, meld.Type);
		Assert.Equal(Seat.South, engine.CurrentTurn);
		Assert.Null(engine.LastDiscard);
		Assert.Equal(11, engine.Hands[Seat.South].ConcealedTiles.Count);
		Assert.Equal(liveWallCountBefore - 1, engine.LiveWallCount);
		Assert.Throws<InvalidOperationException>(() => engine.Hands[Seat.South].Draw(new Tile(TileSuit.Sou, 9)));
	}

	/// <summary>パス条件: 自分の捨て牌に対して自分でカンしようとすると ArgumentException になること。</summary>
	[Fact]
	public void CallOpenKan_BySameSeatAsDiscarder_Throws()
	{
		var discardedTile = new Tile(TileSuit.Pin, 9);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = new Hand(
			[
				discardedTile, discardedTile, discardedTile,
				new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
				new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
				new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
				new Tile(TileSuit.Pin, 1),
			]),
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(
			Wall.CreateShuffled(new Random(1)), hands, Seat.South, (discardedTile, Seat.East));

		Assert.Throws<ArgumentException>(
			() => engine.CallOpenKan(Seat.East, discardedTile, discardedTile, discardedTile));
	}

	/// <summary>パス条件: LastDiscardが無い状態でCallOpenKan()を呼ぶと InvalidOperationException になること。</summary>
	[Fact]
	public void CallOpenKan_WithoutLastDiscard_Throws()
	{
		var engine = MahjongEngine.Start(new Random(1));
		engine.DrawForCurrentPlayer();
		var pinNine = new Tile(TileSuit.Pin, 9);

		Assert.Throws<InvalidOperationException>(() => engine.CallOpenKan(Seat.South, pinNine, pinNine, pinNine));
	}

	/// <summary>
	/// パス条件: 現在の手番の手牌4枚で暗槓が成立すると、Hand.Meldsに追加され、
	/// 嶺上牌をツモった状態（打牌待ち）になること。
	/// </summary>
	[Fact]
	public void CallClosedKan_ByCurrentTurn_AddsMeldAndDrawsReplacement()
	{
		var pinOne = new Tile(TileSuit.Pin, 1);
		var eastHand = new Hand(
		[
			pinOne, pinOne, pinOne,
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Sou, 9),
		]);
		eastHand.Draw(pinOne);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);
		var liveWallCountBefore = engine.LiveWallCount;

		engine.CallClosedKan(pinOne, pinOne, pinOne, pinOne);

		var meld = Assert.Single(engine.Hands[Seat.East].Melds);
		Assert.Equal(MeldType.ClosedKan, meld.Type);
		Assert.Equal(11, engine.Hands[Seat.East].ConcealedTiles.Count);
		Assert.Equal(liveWallCountBefore - 1, engine.LiveWallCount);
		Assert.Throws<InvalidOperationException>(() => engine.Hands[Seat.East].Draw(new Tile(TileSuit.Sou, 8)));
	}

	/// <summary>
	/// パス条件: 打牌待ちでない状態（ツモ前）で CallClosedKan() を呼ぶと InvalidOperationException になること
	/// （Hand.ClosedKan() からの伝播確認）。
	/// </summary>
	[Fact]
	public void CallClosedKan_WhenNotPending_Throws()
	{
		var engine = MahjongEngine.Start(new Random(1));
		var pinOne = new Tile(TileSuit.Pin, 1);

		Assert.Throws<InvalidOperationException>(() => engine.CallClosedKan(pinOne, pinOne, pinOne, pinOne));
	}

	/// <summary>
	/// パス条件: 現在の手番の既存ポンに対して加槓が成立すると、Hand.MeldsのPonがAddedKanに置き換わり、
	/// 嶺上牌をツモった状態（打牌待ち）になること。
	/// </summary>
	[Fact]
	public void CallAddedKan_ByCurrentTurn_ReplacesPonMeldAndDrawsReplacement()
	{
		var pinOne = new Tile(TileSuit.Pin, 1);
		var eastHand = new Hand(
		[
			pinOne, pinOne,
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Man, 4), new Tile(TileSuit.Man, 5), new Tile(TileSuit.Man, 6),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Sou, 9),
		]);
		eastHand.Pon(pinOne, pinOne, pinOne);
		eastHand.Discard(new Tile(TileSuit.Pin, 9));
		eastHand.Draw(pinOne);
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);
		var liveWallCountBefore = engine.LiveWallCount;

		engine.CallAddedKan(pinOne);

		var meld = Assert.Single(engine.Hands[Seat.East].Melds);
		Assert.Equal(MeldType.AddedKan, meld.Type);
		Assert.Equal(11, engine.Hands[Seat.East].ConcealedTiles.Count);
		Assert.Equal(liveWallCountBefore - 1, engine.LiveWallCount);
		Assert.Throws<InvalidOperationException>(() => engine.Hands[Seat.East].Draw(new Tile(TileSuit.Sou, 8)));
	}

	/// <summary>
	/// パス条件: 対応するポンが存在しない場合、CallAddedKan() が ArgumentException になること
	/// （Hand.AddedKan() からの伝播確認）。
	/// </summary>
	[Fact]
	public void CallAddedKan_WithoutMatchingPon_Throws()
	{
		var eastHand = new Hand(CreateThirteenFillerTiles());
		eastHand.Draw(new Tile(TileSuit.Pin, 1));
		var hands = new Dictionary<Seat, Hand>
		{
			[Seat.East] = eastHand,
			[Seat.South] = new Hand(CreateThirteenFillerTiles()),
			[Seat.West] = new Hand(CreateThirteenFillerTiles()),
			[Seat.North] = new Hand(CreateThirteenFillerTiles()),
		};
		var engine = new MahjongEngine(Wall.CreateShuffled(new Random(1)), hands, Seat.East, lastDiscard: null);

		Assert.Throws<ArgumentException>(() => engine.CallAddedKan(new Tile(TileSuit.Pin, 1)));
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
