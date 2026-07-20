using Mahjong.Core.Domain;

namespace Mahjong.Tests.Domain;

public class FuCalculatorTests
{
	/// <summary>
	/// パス条件: 暗刻3つ+雀頭（非役牌）の門前ロン和了で、基本符20+門前ロン符10+暗刻符(4×3)+雀頭符0=42が
	/// 10の位に切り上げられて50になること。
	/// </summary>
	[Fact]
	public void CalculateFu_MenzenRonWithThreeAnkou_ReturnsRoundedTotal()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		var ronTile = new Tile(TileSuit.Pin, 9);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(50, fu);
	}

	/// <summary>
	/// パス条件: 暗刻3つ+雀頭（非役牌）の門前ツモ和了で、基本符20+ツモ符2+暗刻符(4×3)+雀頭符0=34が
	/// 10の位に切り上げられて40になること。
	/// </summary>
	[Fact]
	public void CalculateFu_MenzenTsumoWithThreeAnkou_ReturnsRoundedTotal()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 3),
			new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 4), new Tile(TileSuit.Sou, 4),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(40, fu);
	}

	/// <summary>パス条件: 七対子の場合、他の要素を無視して常に25符が返ること。</summary>
	[Fact]
	public void CalculateFu_Chiitoitsu_ReturnsTwentyFive()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 5), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Pin, 7), new Tile(TileSuit.Pin, 7),
			new Tile(TileSuit.Sou, 9), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
			new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 2),
		];

		var fu = FuCalculator.CalculateFu(tiles, [Yaku.Chiitoitsu], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(25, fu);
	}

	/// <summary>パス条件: 役満（国士無双）を渡すと ArgumentException になること。</summary>
	[Fact]
	public void CalculateFu_Kokushi_Throws()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 1), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Sou, 1), new Tile(TileSuit.Sou, 9),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 2), new Tile(TileSuit.Honor, 3),
			new Tile(TileSuit.Honor, 4), new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 6),
			new Tile(TileSuit.Honor, 7), new Tile(TileSuit.Honor, 7),
		];

		Assert.Throws<ArgumentException>(
			() => FuCalculator.CalculateFu(tiles, [Yaku.Kokushi], Seat.East, Seat.East, ronTile: null));
	}

	/// <summary>
	/// パス条件: ポンで作った刻子（明刻）は基本符20+ツモ符2+明刻符2=24が切り上げられて30になること
	/// （暗刻16符の暗槓を持つテスト（40符）より低いことの確認）。
	/// </summary>
	[Fact]
	public void CalculateFu_WithPonTriplet_ReturnsLowerThanClosedKan()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		Meld[] melds = [new Meld(MeldType.Pon, [new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2)])];

		var fu = FuCalculator.CalculateFu(concealedTiles, melds, [], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(30, fu);
	}

	/// <summary>パス条件: 暗槓は基本符20+ツモ符2+暗槓符16=38が切り上げられて40になること。</summary>
	[Fact]
	public void CalculateFu_WithClosedKan_ReturnsHigherThanPonTriplet()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.ClosedKan,
				[new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2)]),
		];

		var fu = FuCalculator.CalculateFu(concealedTiles, melds, [], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(40, fu);
	}

	/// <summary>
	/// パス条件: 大明槓（明槓）は基本符20+ツモ符2+明槓符8=30が切り上げられて30になり、暗槓より低いこと
	/// （和了牌Man8はMan6,7待ちの両面完成のため待ちの符0符。和了牌が単騎待ちにならない手牌で確認）。
	/// </summary>
	[Fact]
	public void CalculateFu_WithOpenKan_ReturnsLowerThanClosedKan()
	{
		Tile[] concealedTiles =
		[
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
		];
		Meld[] melds =
		[
			new Meld(
				MeldType.OpenKan,
				[new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2)]),
		];

		var fu = FuCalculator.CalculateFu(concealedTiles, melds, [], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(30, fu);
	}

	/// <summary>
	/// パス条件: ロン牌が3つ目の刻子を完成させる場合、その刻子は明刻扱いになり、
	/// 全て暗刻として数えた場合（テスト1の50符）より低い符になること。
	/// </summary>
	[Fact]
	public void CalculateFu_RonTileCompletesTriplet_TreatsItAsOpen()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 2),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 9), new Tile(TileSuit.Pin, 9),
		];
		var ronTile = new Tile(TileSuit.Man, 2);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(40, fu);
	}

	/// <summary>パス条件: 三元牌の雀頭は基本符20+門前ロン符10+三元牌雀頭符2=32が切り上げられて40になること。</summary>
	[Fact]
	public void CalculateFu_DragonPair_AddsTwoFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 5), new Tile(TileSuit.Honor, 5),
		];
		var ronTile = new Tile(TileSuit.Honor, 5);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(40, fu);
	}

	/// <summary>
	/// パス条件: 自風・場風が同じ字牌の雀頭（連風牌、ダブ東等）は基本符20+門前ロン符10+暗刻符8+
	/// 連風牌雀頭符4=42が切り上げられて50になること（自風・場風のいずれか一方のみなら40になるはずの
	/// ケースと区別できる値になっていることの確認）。
	/// </summary>
	[Fact]
	public void CalculateFu_DoubleWindPair_AddsFourFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 1),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Honor, 1), new Tile(TileSuit.Honor, 1),
		];
		var ronTile = new Tile(TileSuit.Honor, 1);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(50, fu);
	}

	/// <summary>
	/// パス条件: 嵌張待ち（Sou5,7待ちのSou6）で完成した場合、基本符20+門前ロン符10+嵌張符2=32が
	/// 切り上げられて40になること（待ちの符が無ければ30のままのはずの手牌で確認）。
	/// </summary>
	[Fact]
	public void CalculateFu_KanchanWait_AddsTwoFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Sou, 6);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(40, fu);
	}

	/// <summary>パス条件: 辺張待ち（Man1,2待ちのMan3）で完成した場合も同様に40になること。</summary>
	[Fact]
	public void CalculateFu_PenchanWait_AddsTwoFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 1), new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 3);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(40, fu);
	}

	/// <summary>パス条件: 両面待ちで完成した場合、待ちの符は0符のままで30になること（嵌張・辺張との対比）。</summary>
	[Fact]
	public void CalculateFu_RyanmenWait_AddsNoFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(30, fu);
	}

	/// <summary>
	/// パス条件: 単騎待ち（非役牌雀頭）で完成した場合、基本符20+門前ロン符10+単騎符2=32が
	/// 切り上げられて40になること。
	/// </summary>
	[Fact]
	public void CalculateFu_TankiWait_AddsTwoFu()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8), new Tile(TileSuit.Man, 9),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Pin, 2);

		var fu = FuCalculator.CalculateFu(tiles, [], Seat.East, Seat.East, ronTile);

		Assert.Equal(40, fu);
	}

	/// <summary>パス条件: 平和・ツモ和了の場合、他の要素を無視して常に20符になること。</summary>
	[Fact]
	public void CalculateFu_PinfuTsumo_ReturnsTwenty()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
		];

		var fu = FuCalculator.CalculateFu(tiles, [Yaku.Pinfu], Seat.East, Seat.East, ronTile: null);

		Assert.Equal(20, fu);
	}

	/// <summary>
	/// パス条件: 平和・ロン和了の場合は20符固定の対象外で、通常通り計算されること
	/// （基本符20+門前ロン符10=30。ツモの場合の20符固定との対比）。
	/// </summary>
	[Fact]
	public void CalculateFu_PinfuRon_ReturnsNormalCalculation()
	{
		Tile[] tiles =
		[
			new Tile(TileSuit.Man, 2), new Tile(TileSuit.Man, 3), new Tile(TileSuit.Man, 4),
			new Tile(TileSuit.Pin, 3), new Tile(TileSuit.Pin, 4), new Tile(TileSuit.Pin, 5),
			new Tile(TileSuit.Sou, 5), new Tile(TileSuit.Sou, 6), new Tile(TileSuit.Sou, 7),
			new Tile(TileSuit.Man, 6), new Tile(TileSuit.Man, 7), new Tile(TileSuit.Man, 8),
			new Tile(TileSuit.Pin, 2), new Tile(TileSuit.Pin, 2),
		];
		var ronTile = new Tile(TileSuit.Man, 6);

		var fu = FuCalculator.CalculateFu(tiles, [Yaku.Pinfu], Seat.East, Seat.East, ronTile);

		Assert.Equal(30, fu);
	}
}
