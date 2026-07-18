namespace Mahjong.Core.Domain;

/// <summary>1人分の手牌（門前）。ツモ・打牌・捨て牌履歴を管理する。</summary>
public sealed class Hand
{
	private readonly List<Tile> _concealedTiles;
	private readonly List<Tile> _discards = [];
	private readonly List<Meld> _melds = [];
	private bool _hasPendingTile;

	/// <summary>配牌13枚から手牌を生成する。</summary>
	/// <exception cref="ArgumentException">startingTilesが13枚でない場合。</exception>
	public Hand(IEnumerable<Tile> startingTiles)
	{
		_concealedTiles = [.. startingTiles];
		if (_concealedTiles.Count != 13)
		{
			throw new ArgumentException($"配牌は13枚である必要があります(実際: {_concealedTiles.Count}枚)。", nameof(startingTiles));
		}
	}

	/// <summary>手牌（門前）。配牌時13枚、ツモ後14枚、打牌後13枚に戻る。</summary>
	public IReadOnlyList<Tile> ConcealedTiles => _concealedTiles;

	/// <summary>これまでの捨て牌（河）。打牌した順に並ぶ。</summary>
	public IReadOnlyList<Tile> Discards => _discards;

	/// <summary>鳴きによって公開された面子。</summary>
	public IReadOnlyList<Meld> Melds => _melds;

	/// <summary>ツモ牌を手牌に加える（13→14枚）。</summary>
	/// <exception cref="InvalidOperationException">既に14枚保持している場合。</exception>
	public void Draw(Tile tile)
	{
		if (_hasPendingTile)
		{
			throw new InvalidOperationException("既にツモ済みです。打牌してから次のツモを行ってください。");
		}

		_concealedTiles.Add(tile);
		_hasPendingTile = true;
	}

	/// <summary>指定した牌を手牌から取り除く（14→13枚）。</summary>
	/// <exception cref="InvalidOperationException">ツモ前（手牌13枚）の場合。</exception>
	/// <exception cref="ArgumentException">手牌に存在しない牌を指定した場合。</exception>
	public void Discard(Tile tile)
	{
		if (!_hasPendingTile)
		{
			throw new InvalidOperationException("ツモしてから打牌してください。");
		}

		if (!_concealedTiles.Remove(tile))
		{
			throw new ArgumentException($"手牌に存在しない牌です: {tile}", nameof(tile));
		}

		_discards.Add(tile);
		_hasPendingTile = false;
	}

	/// <summary>他家の捨て牌 <paramref name="claimedTile"/> と手牌2枚でポン（刻子）を成立させる。</summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	/// <exception cref="ArgumentException">
	/// handTile1/handTile2 の種類（Suit/Rank）が claimedTile と一致しない場合、
	/// または手牌に存在しない牌を指定した場合。
	/// </exception>
	public void Pon(Tile claimedTile, Tile handTile1, Tile handTile2)
	{
		if (_hasPendingTile)
		{
			throw new InvalidOperationException("打牌前はポンできません。");
		}

		if (!IsSameKind(handTile1, claimedTile))
		{
			throw new ArgumentException("ポンする牌はclaimedTileと種類が一致している必要があります。", nameof(handTile1));
		}

		if (!IsSameKind(handTile2, claimedTile))
		{
			throw new ArgumentException("ポンする牌はclaimedTileと種類が一致している必要があります。", nameof(handTile2));
		}

		var remaining = new List<Tile>(_concealedTiles);
		if (!remaining.Remove(handTile1))
		{
			throw new ArgumentException($"手牌に存在しない牌です: {handTile1}", nameof(handTile1));
		}

		if (!remaining.Remove(handTile2))
		{
			throw new ArgumentException($"手牌に存在しない牌です: {handTile2}", nameof(handTile2));
		}

		_concealedTiles.Clear();
		_concealedTiles.AddRange(remaining);
		_melds.Add(new Meld(MeldType.Pon, [handTile1, handTile2, claimedTile]));
		_hasPendingTile = true;
	}

	private static bool IsSameKind(Tile a, Tile b) => a.Suit == b.Suit && a.Rank == b.Rank;
}
