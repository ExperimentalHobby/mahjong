namespace Mahjong.Core.Domain;

/// <summary>役のリストから翻数（飜数）を計算する。符計算・点数表参照・ドラの翻数加算は対象外。</summary>
public static class HanCalculator
{
	/// <summary>役満（国士無双・字一色）が含まれているかどうかを判定する。</summary>
	public static bool IsYakuman(IReadOnlyList<Yaku> yaku) =>
		yaku.Contains(Yaku.Kokushi) || yaku.Contains(Yaku.Tsuiisou);

	/// <summary>
	/// 役のリストから合計翻数を計算する。同じ役がリストに複数回含まれる場合（三元牌の刻子が
	/// 2つある等）はその回数分加算される。
	/// </summary>
	/// <exception cref="ArgumentException">役満（<see cref="IsYakuman"/>が<c>true</c>）が含まれる場合。</exception>
	public static int CalculateHan(IReadOnlyList<Yaku> yaku, bool isMenzen)
	{
		if (IsYakuman(yaku))
		{
			throw new ArgumentException("役満は通常の翻数計算の対象外です。", nameof(yaku));
		}

		return yaku.Sum(y => GetHan(y, isMenzen));
	}

	/// <summary>1つの役の翻数を返す（門前/副露で翻数が変わる役は<paramref name="isMenzen"/>で分岐する）。</summary>
	private static int GetHan(Yaku yaku, bool isMenzen) => yaku switch
	{
		Yaku.Tanyao => 1,
		Yaku.Honitsu => isMenzen ? 3 : 2,
		Yaku.Chinitsu => isMenzen ? 6 : 5,
		Yaku.Chiitoitsu => 2,
		Yaku.Toitoitsu => 2,
		Yaku.Iipeikou => 1,
		Yaku.SanshokuDoujun => isMenzen ? 2 : 1,
		Yaku.Ittsuu => isMenzen ? 2 : 1,
		Yaku.Junchan => isMenzen ? 3 : 2,
		Yaku.Chanta => isMenzen ? 2 : 1,
		Yaku.Ryanpeikou => 3,
		Yaku.Jikaze => 1,
		Yaku.Bakaze => 1,
		Yaku.Sangenpai => 1,
		Yaku.Sanankou => 2,
		Yaku.Riichi => 1,
		Yaku.MenzenTsumo => 1,
		Yaku.RinshanKaihou => 1,
		Yaku.Haitei => 1,
		Yaku.Houtei => 1,
		Yaku.Chankan => 1,
		Yaku.Pinfu => 1,
		Yaku.SanshokuDoukou => 2,
		Yaku.Shousangen => 2,
		Yaku.Sankantsu => 2,
		_ => throw new ArgumentOutOfRangeException(nameof(yaku), yaku, $"未対応のYakuです: {yaku}"),
	};
}
