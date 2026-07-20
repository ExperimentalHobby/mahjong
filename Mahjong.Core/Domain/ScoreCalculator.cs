namespace Mahjong.Core.Domain;

/// <summary>
/// 翻数・符から点数（支払額）を計算する。役満（国士無双・字一色）の点数、供託（リーチ棒）・積み棒（本場）
/// による加算、流局時の聴牌払いは対象外。
/// </summary>
public static class ScoreCalculator
{
	/// <summary>
	/// 翻数・符から基本点を計算する。5翻以上は符に関わらず固定表（満貫2000／跳満3000／倍満4000／
	/// 三倍満6000／数え役満8000）を用い、1〜4翻は<c>fu * 2^(2+han)</c>で計算した上で
	/// 2000点を超える場合は満貫（2000点）に切り上げる。
	/// </summary>
	public static int CalculateBasePoints(int han, int fu)
	{
		if (han >= 13)
		{
			return 8000;
		}

		if (han >= 11)
		{
			return 6000;
		}

		if (han >= 8)
		{
			return 4000;
		}

		if (han >= 6)
		{
			return 3000;
		}

		if (han == 5)
		{
			return 2000;
		}

		var basePoints = fu * (1 << (2 + han));
		return Math.Min(basePoints, 2000);
	}

	/// <summary>ロン和了の支払額を計算する（<paramref name="isDealer"/>が<c>true</c>なら親、基本点×6。子は基本点×4。100点未満は切り上げ）。</summary>
	public static int CalculateRonPoints(int han, int fu, bool isDealer)
	{
		var basePoints = CalculateBasePoints(han, fu);
		var multiplier = isDealer ? 6 : 4;
		return RoundUpToHundred(basePoints * multiplier);
	}

	/// <summary>
	/// 子のツモ和了時の支払額を計算する。親からは基本点×2、他の子からは基本点×1
	/// （それぞれ100点未満は切り上げ）。
	/// </summary>
	public static (int FromDealer, int FromNonDealer) CalculateNonDealerTsumoPoints(int han, int fu)
	{
		var basePoints = CalculateBasePoints(han, fu);
		return (RoundUpToHundred(basePoints * 2), RoundUpToHundred(basePoints * 1));
	}

	/// <summary>親のツモ和了時、子1人あたりの支払額を計算する（基本点×2、100点未満は切り上げ）。</summary>
	public static int CalculateDealerTsumoPointsFromEach(int han, int fu)
	{
		var basePoints = CalculateBasePoints(han, fu);
		return RoundUpToHundred(basePoints * 2);
	}

	private static int RoundUpToHundred(int points) => (points + 99) / 100 * 100;
}
