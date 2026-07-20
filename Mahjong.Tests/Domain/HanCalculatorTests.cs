using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class HanCalculatorTests
{
	/// <summary>パス条件: 門前の単一役(Tanyao)を渡すと1が返ること。</summary>
	[Fact]
	public void CalculateHan_SingleYakuMenzen_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Tanyao], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 門前/副露で翻数が変わる役(Honitsu)を門前で渡すと3が返ること。</summary>
	[Fact]
	public void CalculateHan_HonitsuMenzen_ReturnsThree()
	{
		var han = HanCalculator.CalculateHan([Yaku.Honitsu], isMenzen: true);

		Assert.Equal(3, han);
	}

	/// <summary>パス条件: 門前/副露で翻数が変わる役(Honitsu)を副露で渡すと2が返ること。</summary>
	[Fact]
	public void CalculateHan_HonitsuNotMenzen_ReturnsTwo()
	{
		var han = HanCalculator.CalculateHan([Yaku.Honitsu], isMenzen: false);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 複数役を渡すと合算されること(Tanyao=1 + Toitoitsu=2 = 3)。</summary>
	[Fact]
	public void CalculateHan_MultipleYaku_ReturnsSum()
	{
		var han = HanCalculator.CalculateHan([Yaku.Tanyao, Yaku.Toitoitsu], isMenzen: true);

		Assert.Equal(3, han);
	}

	/// <summary>パス条件: リストに同じ役が複数回含まれる場合、その回数分加算されること(Sangenpaiが2回で2)。</summary>
	[Fact]
	public void CalculateHan_YakuAppearingTwice_ReturnsSumOfBoth()
	{
		var han = HanCalculator.CalculateHan([Yaku.Sangenpai, Yaku.Sangenpai], isMenzen: true);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 空リストを渡すと0が返ること。</summary>
	[Fact]
	public void CalculateHan_EmptyList_ReturnsZero()
	{
		var han = HanCalculator.CalculateHan([], isMenzen: true);

		Assert.Equal(0, han);
	}

	/// <summary>パス条件: 役満(Kokushi)を含むリストを渡すと IsYakuman が true になること。</summary>
	[Fact]
	public void IsYakuman_ContainsKokushi_ReturnsTrue()
	{
		Assert.True(HanCalculator.IsYakuman([Yaku.Kokushi]));
	}

	/// <summary>パス条件: 通常役のみのリストを渡すと IsYakuman が false になること。</summary>
	[Fact]
	public void IsYakuman_ContainsOnlyRegularYaku_ReturnsFalse()
	{
		Assert.False(HanCalculator.IsYakuman([Yaku.Tanyao]));
	}

	/// <summary>パス条件: 役満(Kokushi)を含むリストを CalculateHan に渡すと ArgumentException になること。</summary>
	[Fact]
	public void CalculateHan_ContainsKokushi_Throws()
	{
		Assert.Throws<ArgumentException>(() => HanCalculator.CalculateHan([Yaku.Kokushi], isMenzen: true));
	}

	/// <summary>パス条件: 役満(Tsuiisou)を含むリストを CalculateHan に渡すと ArgumentException になること。</summary>
	[Fact]
	public void CalculateHan_ContainsTsuiisou_Throws()
	{
		Assert.Throws<ArgumentException>(() => HanCalculator.CalculateHan([Yaku.Tsuiisou], isMenzen: true));
	}

	/// <summary>パス条件: リーチ(Yaku.Riichi)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Riichi_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Riichi], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 門前清自摸和(Yaku.MenzenTsumo)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_MenzenTsumo_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.MenzenTsumo], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 嶺上開花(Yaku.RinshanKaihou)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_RinshanKaihou_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.RinshanKaihou], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 海底摸月(Yaku.Haitei)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Haitei_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Haitei], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 河底撈魚(Yaku.Houtei)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Houtei_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Houtei], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 槍槓(Yaku.Chankan)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Chankan_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Chankan], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 平和(Yaku.Pinfu)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Pinfu_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Pinfu], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: 三色同刻(Yaku.SanshokuDoukou)単独で2が返ること。</summary>
	[Fact]
	public void CalculateHan_SanshokuDoukou_ReturnsTwo()
	{
		var han = HanCalculator.CalculateHan([Yaku.SanshokuDoukou], isMenzen: true);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 小三元(Yaku.Shousangen)単独で2が返ること。</summary>
	[Fact]
	public void CalculateHan_Shousangen_ReturnsTwo()
	{
		var han = HanCalculator.CalculateHan([Yaku.Shousangen], isMenzen: true);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 三槓子(Yaku.Sankantsu)単独で2が返ること。</summary>
	[Fact]
	public void CalculateHan_Sankantsu_ReturnsTwo()
	{
		var han = HanCalculator.CalculateHan([Yaku.Sankantsu], isMenzen: true);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 役満8種それぞれを含むリストを渡すと IsYakuman が true になること。</summary>
	[Theory]
	[InlineData(Yaku.Suuankou)]
	[InlineData(Yaku.Daisangen)]
	[InlineData(Yaku.Ryuuiisou)]
	[InlineData(Yaku.Chinroutou)]
	[InlineData(Yaku.Shousuushi)]
	[InlineData(Yaku.Daisuushi)]
	[InlineData(Yaku.Suukantsu)]
	[InlineData(Yaku.Chuurenpoutou)]
	public void IsYakuman_ContainsNewYakuman_ReturnsTrue(Yaku yaku)
	{
		Assert.True(HanCalculator.IsYakuman([yaku]));
	}

	/// <summary>パス条件: 一発(Yaku.Ippatsu)単独で1が返ること。</summary>
	[Fact]
	public void CalculateHan_Ippatsu_ReturnsOne()
	{
		var han = HanCalculator.CalculateHan([Yaku.Ippatsu], isMenzen: true);

		Assert.Equal(1, han);
	}

	/// <summary>パス条件: ダブル立直(Yaku.DaburuRiichi)単独で2が返ること。</summary>
	[Fact]
	public void CalculateHan_DaburuRiichi_ReturnsTwo()
	{
		var han = HanCalculator.CalculateHan([Yaku.DaburuRiichi], isMenzen: true);

		Assert.Equal(2, han);
	}

	/// <summary>パス条件: 役満(Yaku.Tenhou・Yaku.Chiihou)それぞれを含むリストを渡すと IsYakuman が true になること。</summary>
	[Theory]
	[InlineData(Yaku.Tenhou)]
	[InlineData(Yaku.Chiihou)]
	public void IsYakuman_ContainsTenhouOrChiihou_ReturnsTrue(Yaku yaku)
	{
		Assert.True(HanCalculator.IsYakuman([yaku]));
	}
}
