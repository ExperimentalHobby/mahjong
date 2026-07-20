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
}
