using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class ScoreCalculatorTests
{
	/// <summary>パス条件: 3翻30符から基本点 30*2^(2+3)=960 が計算されること。</summary>
	[Fact]
	public void CalculateBasePoints_ThreeHanThirtyFu_Returns960()
	{
		var basePoints = ScoreCalculator.CalculateBasePoints(han: 3, fu: 30);

		Assert.Equal(960, basePoints);
	}

	/// <summary>
	/// パス条件: 4翻40符は計算上 40*2^(2+4)=2560 だが、2000点を超えるため満貫(2000点)に
	/// 切り上げられること。
	/// </summary>
	[Fact]
	public void CalculateBasePoints_FourHanFortyFu_ClampsToMangan()
	{
		var basePoints = ScoreCalculator.CalculateBasePoints(han: 4, fu: 40);

		Assert.Equal(2000, basePoints);
	}

	/// <summary>パス条件: 5翻は符に関わらず満貫(2000点)固定になること。</summary>
	[Fact]
	public void CalculateBasePoints_FiveHan_ReturnsManganRegardlessOfFu()
	{
		var basePoints = ScoreCalculator.CalculateBasePoints(han: 5, fu: 30);

		Assert.Equal(2000, basePoints);
	}

	/// <summary>パス条件: 6〜7翻は跳満(3000点)固定になること。</summary>
	[Fact]
	public void CalculateBasePoints_SixOrSevenHan_ReturnsHaneman()
	{
		Assert.Equal(3000, ScoreCalculator.CalculateBasePoints(han: 6, fu: 30));
		Assert.Equal(3000, ScoreCalculator.CalculateBasePoints(han: 7, fu: 30));
	}

	/// <summary>パス条件: 8〜10翻は倍満(4000点)固定になること。</summary>
	[Fact]
	public void CalculateBasePoints_EightToTenHan_ReturnsBaiman()
	{
		Assert.Equal(4000, ScoreCalculator.CalculateBasePoints(han: 8, fu: 30));
		Assert.Equal(4000, ScoreCalculator.CalculateBasePoints(han: 10, fu: 30));
	}

	/// <summary>パス条件: 11〜12翻は三倍満(6000点)固定になること。</summary>
	[Fact]
	public void CalculateBasePoints_ElevenOrTwelveHan_ReturnsSanbaiman()
	{
		Assert.Equal(6000, ScoreCalculator.CalculateBasePoints(han: 11, fu: 30));
		Assert.Equal(6000, ScoreCalculator.CalculateBasePoints(han: 12, fu: 30));
	}

	/// <summary>パス条件: 13翻以上は数え役満(8000点)固定になること。</summary>
	[Fact]
	public void CalculateBasePoints_ThirteenHanOrMore_ReturnsKazoeYakuman()
	{
		var basePoints = ScoreCalculator.CalculateBasePoints(han: 13, fu: 30);

		Assert.Equal(8000, basePoints);
	}

	/// <summary>パス条件: 3翻30符・子のロン和了で 960*4=3840 が100点未満切り上げで3900点になること。</summary>
	[Fact]
	public void CalculateRonPoints_NonDealer_Returns3900()
	{
		var points = ScoreCalculator.CalculateRonPoints(han: 3, fu: 30, isDealer: false);

		Assert.Equal(3900, points);
	}

	/// <summary>パス条件: 3翻30符・親のロン和了で 960*6=5760 が100点未満切り上げで5800点になること。</summary>
	[Fact]
	public void CalculateRonPoints_Dealer_Returns5800()
	{
		var points = ScoreCalculator.CalculateRonPoints(han: 3, fu: 30, isDealer: true);

		Assert.Equal(5800, points);
	}

	/// <summary>
	/// パス条件: 4翻30符・子のツモ和了で、親からは 1920*2=3840→3900点、
	/// 子からは 1920*1=1920→2000点になること。
	/// </summary>
	[Fact]
	public void CalculateNonDealerTsumoPoints_FourHanThirtyFu_ReturnsDealerAndNonDealerShares()
	{
		var (fromDealer, fromNonDealer) = ScoreCalculator.CalculateNonDealerTsumoPoints(han: 4, fu: 30);

		Assert.Equal(3900, fromDealer);
		Assert.Equal(2000, fromNonDealer);
	}

	/// <summary>パス条件: 3翻30符・親のツモ和了で、子1人あたり 960*2=1920→2000点になること。</summary>
	[Fact]
	public void CalculateDealerTsumoPointsFromEach_ThreeHanThirtyFu_Returns2000()
	{
		var points = ScoreCalculator.CalculateDealerTsumoPointsFromEach(han: 3, fu: 30);

		Assert.Equal(2000, points);
	}

	/// <summary>パス条件: 積み棒1本ありのロンで、本場分の300点が加算されて 3900+300=4200点になること。</summary>
	[Fact]
	public void CalculateRonPoints_OneHonba_AddsThreeHundredPoints()
	{
		var points = ScoreCalculator.CalculateRonPoints(han: 3, fu: 30, isDealer: false, honbaCount: 1);

		Assert.Equal(4200, points);
	}

	/// <summary>
	/// パス条件: 積み棒1本ありの子のツモで、親・子とも本場分の100点が加算されて
	/// 親2100点・子1100点になること。
	/// </summary>
	[Fact]
	public void CalculateNonDealerTsumoPoints_OneHonba_AddsHundredPointsToEach()
	{
		var (fromDealer, fromNonDealer) = ScoreCalculator.CalculateNonDealerTsumoPoints(han: 4, fu: 30, honbaCount: 1);

		Assert.Equal(4000, fromDealer);
		Assert.Equal(2100, fromNonDealer);
	}

	/// <summary>パス条件: 積み棒1本ありの親のツモで、各家の支払いに本場分の100点が加算されて2100点になること。</summary>
	[Fact]
	public void CalculateDealerTsumoPointsFromEach_OneHonba_AddsHundredPoints()
	{
		var points = ScoreCalculator.CalculateDealerTsumoPointsFromEach(han: 3, fu: 30, honbaCount: 1);

		Assert.Equal(2100, points);
	}

	/// <summary>パス条件: 聴牌1人・ノーテン3人の場合、聴牌者が3000点、ノーテン者が1000点ずつ減ること。</summary>
	[Fact]
	public void CalculateExhaustiveDrawPayments_OneTenpai_ReturnsThreeThousandAndOneThousand()
	{
		var (perTenpaiGain, perNotenLoss) = ScoreCalculator.CalculateExhaustiveDrawPayments(tenpaiCount: 1);

		Assert.Equal(3000, perTenpaiGain);
		Assert.Equal(1000, perNotenLoss);
	}

	/// <summary>パス条件: 聴牌2人・ノーテン2人の場合、それぞれ1500点ずつ授受されること。</summary>
	[Fact]
	public void CalculateExhaustiveDrawPayments_TwoTenpai_ReturnsFifteenHundredEach()
	{
		var (perTenpaiGain, perNotenLoss) = ScoreCalculator.CalculateExhaustiveDrawPayments(tenpaiCount: 2);

		Assert.Equal(1500, perTenpaiGain);
		Assert.Equal(1500, perNotenLoss);
	}

	/// <summary>パス条件: 聴牌3人・ノーテン1人の場合、聴牌者が1000点ずつ、ノーテン者が3000点減ること。</summary>
	[Fact]
	public void CalculateExhaustiveDrawPayments_ThreeTenpai_ReturnsOneThousandAndThreeThousand()
	{
		var (perTenpaiGain, perNotenLoss) = ScoreCalculator.CalculateExhaustiveDrawPayments(tenpaiCount: 3);

		Assert.Equal(1000, perTenpaiGain);
		Assert.Equal(3000, perNotenLoss);
	}

	/// <summary>パス条件: 聴牌0人・4人の場合、点数移動が無い(0,0)になること。</summary>
	[Theory]
	[InlineData(0)]
	[InlineData(4)]
	public void CalculateExhaustiveDrawPayments_ZeroOrFourTenpai_ReturnsZero(int tenpaiCount)
	{
		var (perTenpaiGain, perNotenLoss) = ScoreCalculator.CalculateExhaustiveDrawPayments(tenpaiCount);

		Assert.Equal(0, perTenpaiGain);
		Assert.Equal(0, perNotenLoss);
	}

	/// <summary>パス条件: 聴牌人数に0〜4の範囲外を渡すと ArgumentOutOfRangeException になること。</summary>
	[Theory]
	[InlineData(-1)]
	[InlineData(5)]
	public void CalculateExhaustiveDrawPayments_OutOfRangeTenpaiCount_Throws(int tenpaiCount)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => ScoreCalculator.CalculateExhaustiveDrawPayments(tenpaiCount));
	}
}
