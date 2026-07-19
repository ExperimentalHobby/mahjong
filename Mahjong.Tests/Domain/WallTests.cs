using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class WallTests
{
	/// <summary>パス条件: CreateStandardTileSet が生成する牌の合計枚数が136枚であること。</summary>
	[Fact]
	public void CreateStandardTileSet_TotalTileCount_Is136()
	{
		var tiles = Wall.CreateStandardTileSet();

		Assert.Equal(136, tiles.Count);
	}

	/// <summary>パス条件: (Suit,Rank)の組み合わせ34種それぞれが、ちょうど4枚ずつ存在すること。</summary>
	[Fact]
	public void CreateStandardTileSet_EachKindHasFourCopies()
	{
		var tiles = Wall.CreateStandardTileSet();

		var groups = tiles.GroupBy(t => (t.Suit, t.Rank)).ToList();

		Assert.Equal(34, groups.Count);
		Assert.All(groups, g => Assert.Equal(4, g.Count()));
	}

	/// <summary>パス条件: 数牌各スートのRank=5が、赤ドラ1枚・通常3枚の内訳になっていること。</summary>
	[Theory]
	[InlineData(TileSuit.Man)]
	[InlineData(TileSuit.Pin)]
	[InlineData(TileSuit.Sou)]
	public void CreateStandardTileSet_RankFive_HasOneRedAndThreeNormal(TileSuit suit)
	{
		var tiles = Wall.CreateStandardTileSet();

		var fives = tiles.Where(t => t.Suit == suit && t.Rank == 5).ToList();

		Assert.Equal(1, fives.Count(t => t.IsRedFive));
		Assert.Equal(3, fives.Count(t => !t.IsRedFive));
	}

	/// <summary>パス条件: CreateShuffled 直後の LiveWallCount が122(136-14)であること。</summary>
	[Fact]
	public void CreateShuffled_InitialLiveWallCount_Is122()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		Assert.Equal(122, wall.LiveWallCount);
	}

	/// <summary>パス条件: DoraIndicator を複数回参照しても同じ牌を返すこと。</summary>
	[Fact]
	public void DoraIndicator_ReturnsSameTileOnEachAccess()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		Assert.Equal(wall.DoraIndicator, wall.DoraIndicator);
	}

	/// <summary>パス条件: Draw() を呼ぶたびに LiveWallCount が1ずつ減ること。</summary>
	[Fact]
	public void Draw_DecrementsLiveWallCountByOne()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		wall.Draw();

		Assert.Equal(121, wall.LiveWallCount);
	}

	/// <summary>パス条件: 生牌山122枚を引き切った後、123回目の Draw() が InvalidOperationException になること。</summary>
	[Fact]
	public void Draw_WhenLiveWallExhausted_Throws()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		for (var i = 0; i < 122; i++)
		{
			wall.Draw();
		}

		Assert.Throws<InvalidOperationException>(() => wall.Draw());
	}

	/// <summary>パス条件: 同じseedのRandomから生成した2つのWallは、同じ順序で牌を引けること。</summary>
	[Fact]
	public void CreateShuffled_SameSeed_ProducesSameDrawOrder()
	{
		var wallA = Wall.CreateShuffled(new Random(42));
		var wallB = Wall.CreateShuffled(new Random(42));

		for (var i = 0; i < 122; i++)
		{
			Assert.Equal(wallA.Draw(), wallB.Draw());
		}
	}

	/// <summary>パス条件: DealInitialHands() が4座席それぞれに13枚ずつ配ること。</summary>
	[Fact]
	public void DealInitialHands_DealsThirteenTilesToEachSeat()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		var hands = wall.DealInitialHands();

		Assert.Equal(4, hands.Count);
		foreach (var seat in new[] { Seat.East, Seat.South, Seat.West, Seat.North })
		{
			Assert.Equal(13, hands[seat].Count);
		}
	}

	/// <summary>パス条件: DealInitialHands() 実行後、LiveWallCount が 122-52=70 になっていること。</summary>
	[Fact]
	public void DealInitialHands_ReducesLiveWallCountBySeatCountTimesThirteen()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		wall.DealInitialHands();

		Assert.Equal(70, wall.LiveWallCount);
	}

	/// <summary>パス条件: Clone() で複製した Wall の LiveWallCount/DoraIndicator が元と同じであること。</summary>
	[Fact]
	public void Clone_CopiesLiveWallCountAndDoraIndicator()
	{
		var wall = Wall.CreateShuffled(new Random(1));

		var clone = wall.Clone();

		Assert.Equal(wall.LiveWallCount, clone.LiveWallCount);
		Assert.Equal(wall.DoraIndicator, clone.DoraIndicator);
	}

	/// <summary>パス条件: Clone() 後、元の Wall に対する Draw() が複製の LiveWallCount に影響しないこと。</summary>
	[Fact]
	public void Clone_IsIndependentFromOriginal()
	{
		var wall = Wall.CreateShuffled(new Random(1));
		var clone = wall.Clone();

		wall.Draw();

		Assert.Equal(121, wall.LiveWallCount);
		Assert.Equal(122, clone.LiveWallCount);
	}
}
