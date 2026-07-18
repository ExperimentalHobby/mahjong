namespace Mahjong.Core.Domain;

/// <summary>麻雀牌 1 枚を表す値。</summary>
public readonly record struct Tile
{
	/// <summary>牌の種類。</summary>
	public TileSuit Suit { get; }

	/// <summary>
	/// 牌の番号。数牌（萬子・筒子・索子）は 1〜9。
	/// 字牌は 1=東, 2=南, 3=西, 4=北, 5=白, 6=發, 7=中。
	/// </summary>
	public int Rank { get; }

	/// <summary>赤ドラかどうか。数牌の Rank=5 のときのみ true にできる。</summary>
	public bool IsRedFive { get; }

	/// <summary>牌を生成する。</summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 数牌で Rank が 1〜9 の範囲外、または字牌で Rank が 1〜7 の範囲外の場合。
	/// </exception>
	/// <exception cref="ArgumentException">数牌の Rank≠5、または字牌に対して isRedFive=true を指定した場合。</exception>
	public Tile(TileSuit suit, int rank, bool isRedFive = false)
	{
		var maxRank = suit == TileSuit.Honor ? 7 : 9;
		if (rank < 1 || rank > maxRank)
		{
			throw new ArgumentOutOfRangeException(nameof(rank), rank, $"{suit} の Rank は 1〜{maxRank} の範囲で指定してください。");
		}

		if (isRedFive && (suit == TileSuit.Honor || rank != 5))
		{
			throw new ArgumentException("赤ドラを指定できるのは Rank=5 の数牌のみです。", nameof(isRedFive));
		}

		Suit = suit;
		Rank = rank;
		IsRedFive = isRedFive;
	}

	private static readonly string[] HonorNames =
		["East", "South", "West", "North", "White", "Green", "Red"];

	/// <summary>人間が読める表記を返す（数牌: "1m"/"5p+"（赤五）、字牌: "East" 等）。</summary>
	public override string ToString()
	{
		if (Suit == TileSuit.Honor)
		{
			return HonorNames[Rank - 1];
		}

		var suitChar = Suit switch
		{
			TileSuit.Man => 'm',
			TileSuit.Pin => 'p',
			TileSuit.Sou => 's',
			_ => throw new InvalidOperationException($"未知の Suit です: {Suit}"),
		};

		return IsRedFive ? $"{Rank}{suitChar}+" : $"{Rank}{suitChar}";
	}
}
