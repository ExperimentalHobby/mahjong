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
}
