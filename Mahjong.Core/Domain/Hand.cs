namespace Mahjong.Core.Domain;

/// <summary>1人分の手牌（門前）。ツモ・打牌・捨て牌履歴を管理する。</summary>
public sealed class Hand
{
	private readonly List<Tile> _concealedTiles;
	private readonly List<Tile> _discards = [];
	private readonly List<Meld> _melds = [];
	private bool _hasPendingTile;
	private bool _isRiichi;

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

	/// <summary>クローン用に、バリデーションを行わずに内部状態から直接手牌を生成する。</summary>
	private Hand(List<Tile> concealedTiles, List<Tile> discards, List<Meld> melds, bool hasPendingTile, bool isRiichi)
	{
		_concealedTiles = concealedTiles;
		_discards = discards;
		_melds = melds;
		_hasPendingTile = hasPendingTile;
		_isRiichi = isRiichi;
	}

	/// <summary>この手牌の独立した複製を返す（複製後は互いの操作が影響し合わない）。</summary>
	public Hand Clone() => new(
		new List<Tile>(_concealedTiles),
		new List<Tile>(_discards),
		new List<Meld>(_melds),
		_hasPendingTile,
		_isRiichi);

	/// <summary>手牌（門前）。配牌時13枚、ツモ後14枚、打牌後13枚に戻る。</summary>
	public IReadOnlyList<Tile> ConcealedTiles => _concealedTiles;

	/// <summary>これまでの捨て牌（河）。打牌した順に並ぶ。</summary>
	public IReadOnlyList<Tile> Discards => _discards;

	/// <summary>鳴きによって公開された面子。</summary>
	public IReadOnlyList<Meld> Melds => _melds;

	/// <summary>リーチを宣言しているかどうか。</summary>
	public bool IsRiichi => _isRiichi;

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

	/// <summary>
	/// 指定した牌を手牌から取り除く（14→13枚）。リーチ宣言後は直前にツモった牌以外を指定できない。
	/// </summary>
	/// <exception cref="InvalidOperationException">ツモ前（手牌13枚）の場合。</exception>
	/// <exception cref="ArgumentException">
	/// 手牌に存在しない牌を指定した場合、またはリーチ宣言後に直前にツモった牌以外を指定した場合。
	/// </exception>
	public void Discard(Tile tile)
	{
		EnsurePending("打牌");

		if (_isRiichi && !tile.Equals(_concealedTiles[^1]))
		{
			throw new ArgumentException("リーチ後は直前にツモった牌以外を打牌できません。", nameof(tile));
		}

		if (!_concealedTiles.Remove(tile))
		{
			throw new ArgumentException($"手牌に存在しない牌です: {tile}", nameof(tile));
		}

		_discards.Add(tile);
		_hasPendingTile = false;
	}

	/// <summary>
	/// リーチを宣言し、指定した牌<paramref name="tile"/>を打牌する（打牌待ち→打牌済みの遷移も同時に行う）。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合、または既にリーチ宣言済みの場合。</exception>
	/// <exception cref="ArgumentException">
	/// 副露がある場合、手牌に存在しない牌を指定した場合、または指定した牌を打牌しても聴牌にならない場合。
	/// </exception>
	public void Riichi(Tile tile)
	{
		EnsurePending("リーチ");

		if (_isRiichi)
		{
			throw new InvalidOperationException("既にリーチ宣言済みです。");
		}

		if (_melds.Count > 0)
		{
			throw new ArgumentException("リーチは門前（副露なし）でのみ宣言できます。");
		}

		if (!_concealedTiles.Contains(tile))
		{
			throw new ArgumentException($"手牌に存在しない牌です: {tile}", nameof(tile));
		}

		var remaining = new List<Tile>(_concealedTiles);
		remaining.Remove(tile);
		if (ShantenCalculator.CalculateShanten(remaining) != 0)
		{
			throw new ArgumentException("指定した牌を打牌しても聴牌になりません。", nameof(tile));
		}

		_concealedTiles.Remove(tile);
		_discards.Add(tile);
		_hasPendingTile = false;
		_isRiichi = true;
	}

	/// <summary>他家の捨て牌 <paramref name="claimedTile"/> と手牌2枚でポン（刻子）を成立させる。</summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	/// <exception cref="ArgumentException">
	/// handTile1/handTile2 の種類（Suit/Rank）が claimedTile と一致しない場合、
	/// または手牌に存在しない牌を指定した場合。
	/// </exception>
	public void Pon(Tile claimedTile, Tile handTile1, Tile handTile2)
	{
		EnsureNotPending("ポン");
		EnsureSameKind(claimedTile, "ポンする牌", handTile1, handTile2);
		RemoveFromConcealedAtomically(handTile1, handTile2);
		_melds.Add(new Meld(MeldType.Pon, [handTile1, handTile2, claimedTile]));
		_hasPendingTile = true;
	}

	/// <summary>上家の捨て牌 <paramref name="claimedTile"/> と手牌2枚でチー（順子）を成立させる。</summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	/// <exception cref="ArgumentException">
	/// claimedTileが字牌の場合、handTile1/handTile2のSuitが一致しない場合、
	/// Rankが3つの連続した整数にならない場合、または手牌に存在しない牌を指定した場合。
	/// </exception>
	public void Chi(Tile claimedTile, Tile handTile1, Tile handTile2)
	{
		EnsureNotPending("チー");

		if (claimedTile.Suit == TileSuit.Honor)
		{
			throw new ArgumentException("字牌でチーはできません。", nameof(claimedTile));
		}

		if (handTile1.Suit != claimedTile.Suit)
		{
			throw new ArgumentException("チーする牌はclaimedTileと同じSuitである必要があります。", nameof(handTile1));
		}

		if (handTile2.Suit != claimedTile.Suit)
		{
			throw new ArgumentException("チーする牌はclaimedTileと同じSuitである必要があります。", nameof(handTile2));
		}

		var sorted = new[] { claimedTile, handTile1, handTile2 }.OrderBy(t => t.Rank).ToArray();
		if (sorted[1].Rank != sorted[0].Rank + 1 || sorted[2].Rank != sorted[1].Rank + 1)
		{
			throw new ArgumentException("claimedTile・handTile1・handTile2のRankは3つの連続した整数である必要があります。");
		}

		RemoveFromConcealedAtomically(handTile1, handTile2);
		_melds.Add(new Meld(MeldType.Chi, sorted));
		_hasPendingTile = true;
	}

	/// <summary>
	/// 他家の捨て牌 <paramref name="claimedTile"/> と手牌3枚で明槓（大明槓）を成立させる。
	/// カン成立後は打牌待ちにはならない（<c>_hasPendingTile</c> は変化しない）。
	/// 嶺上牌のツモは呼び出し側が別途 <see cref="Draw"/> を呼ぶ想定。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	/// <exception cref="ArgumentException">
	/// handTile1/handTile2/handTile3 の種類（Suit/Rank）が claimedTile と一致しない場合、
	/// または手牌に存在しない牌を指定した場合。
	/// </exception>
	public void OpenKan(Tile claimedTile, Tile handTile1, Tile handTile2, Tile handTile3)
	{
		EnsureNotPending("カン");
		EnsureSameKind(claimedTile, "カンする牌", handTile1, handTile2, handTile3);
		RemoveFromConcealedAtomically(handTile1, handTile2, handTile3);
		_melds.Add(new Meld(MeldType.OpenKan, [handTile1, handTile2, handTile3, claimedTile]));
	}

	/// <summary>
	/// 自分の手牌4枚（直前のツモを含む）で暗槓を成立させる。
	/// カン成立後は打牌待ちにはならない（<c>_hasPendingTile</c> は false になる）。
	/// 嶺上牌のツモは呼び出し側が別途 <see cref="Draw"/> を呼ぶ想定。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">
	/// tile2/tile3/tile4 の種類（Suit/Rank）が tile1 と一致しない場合、
	/// または手牌に存在しない牌を指定した場合。
	/// </exception>
	public void ClosedKan(Tile tile1, Tile tile2, Tile tile3, Tile tile4)
	{
		EnsurePending("暗槓");
		EnsureSameKind(tile1, "暗槓する牌", tile2, tile3, tile4);
		RemoveFromConcealedAtomically(tile1, tile2, tile3, tile4);
		_melds.Add(new Meld(MeldType.ClosedKan, [tile1, tile2, tile3, tile4]));
		_hasPendingTile = false;
	}

	/// <summary>
	/// 既存のポンに自摸4枚目の <paramref name="tile"/> を追加して加槓を成立させる。
	/// カン成立後は打牌待ちにはならない（<c>_hasPendingTile</c> は false になる）。
	/// 嶺上牌のツモは呼び出し側が別途 <see cref="Draw"/> を呼ぶ想定。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">
	/// tile と同じ種類（Suit/Rank）の Pon が <see cref="Melds"/> に存在しない場合、
	/// または tile が手牌に存在しない場合。
	/// </exception>
	public void AddedKan(Tile tile)
	{
		EnsurePending("加槓");

		var existingPon = _melds.FirstOrDefault(m => m.Type == MeldType.Pon && IsSameKind(m.Tiles[0], tile));
		if (existingPon is null)
		{
			throw new ArgumentException($"対応するポンが見つかりません: {tile}", nameof(tile));
		}

		RemoveFromConcealedAtomically(tile);
		_melds.Remove(existingPon);
		_melds.Add(new Meld(MeldType.AddedKan, [.. existingPon.Tiles, tile]));
		_hasPendingTile = false;
	}

	/// <summary>
	/// 現在の手牌（門前牌+副露）が和了形として完成しているかを判定する（ツモのみが対象。ロンは対象外）。
	/// 副露が無ければ標準形・七対子・国士無双のいずれかを、副露があれば標準形のみを判定する
	/// （副露済みの面子は牌数に関わらず1面子として数え、七対子・国士無双は門前限定のため判定しない）。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	public bool IsComplete()
	{
		EnsurePending("和了判定");

		if (_melds.Count == 0)
		{
			return WinningHandChecker.IsComplete(_concealedTiles);
		}

		return WinningHandChecker.IsStandardFormComplete(_concealedTiles, 4 - _melds.Count);
	}

	/// <summary>
	/// 現在の手牌（門前牌+副露）のシャンテン数を計算する（打牌待ちでない状態＝門前13枚相当が対象）。
	/// 副露が無ければ標準形・七対子・国士無双のうち最小の値を、副露があれば標準形のみを判定する
	/// （副露済みの面子は牌数に関わらず1面子として数え、七対子・国士無双は門前限定のため判定しない）。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	public int CalculateShanten()
	{
		EnsureNotPending("シャンテン数計算");

		if (_melds.Count == 0)
		{
			return ShantenCalculator.CalculateShanten(_concealedTiles);
		}

		return ShantenCalculator.CalculateStandardFormShanten(_concealedTiles, _melds.Count);
	}

	/// <summary>
	/// 他家の捨て牌<paramref name="tile"/>を仮に加えたら和了形として完成するかを判定する（手牌は変更しない）。
	/// 打牌待ちでない状態（自分の手番でない間）が対象。副露が無ければ標準形・七対子・国士無双のいずれかを、
	/// 副露があれば標準形のみを判定する。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	public bool CanWinOn(Tile tile)
	{
		EnsureNotPending("ロン判定");

		var hypothetical = new List<Tile>(_concealedTiles) { tile };
		if (_melds.Count == 0)
		{
			return WinningHandChecker.IsComplete(hypothetical);
		}

		return WinningHandChecker.IsStandardFormComplete(hypothetical, 4 - _melds.Count);
	}

	/// <summary>
	/// ツモ和了後の手牌（門前牌+副露）に成立している役を判定する。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">手牌が和了形になっていない場合。</exception>
	public IReadOnlyList<Yaku> DetermineYaku()
	{
		EnsurePending("役判定");

		return YakuChecker.DetermineYaku(_concealedTiles, _melds);
	}

	/// <summary>
	/// 他家の捨て牌<paramref name="tile"/>を仮に加えたロン和了に成立する役を判定する（手牌は変更しない）。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）の場合。</exception>
	/// <exception cref="ArgumentException">捨て牌を加えても和了形にならない場合。</exception>
	public IReadOnlyList<Yaku> DetermineYakuOn(Tile tile)
	{
		EnsureNotPending("役判定");

		var hypothetical = new List<Tile>(_concealedTiles) { tile };
		return YakuChecker.DetermineYaku(hypothetical, _melds);
	}

	/// <summary>打牌待ち（ツモ直後）でないことを確認する。</summary>
	/// <exception cref="InvalidOperationException">打牌待ちの場合。</exception>
	private void EnsureNotPending(string meldActionName)
	{
		if (_hasPendingTile)
		{
			throw new InvalidOperationException($"打牌前は{meldActionName}できません。");
		}
	}

	/// <summary>打牌待ち（ツモ直後）であることを確認する。</summary>
	/// <exception cref="InvalidOperationException">打牌待ちでない場合。</exception>
	private void EnsurePending(string actionName)
	{
		if (!_hasPendingTile)
		{
			throw new InvalidOperationException($"ツモしてから{actionName}してください。");
		}
	}

	/// <summary>
	/// 手牌のコピー上で <paramref name="handTiles"/> が全て存在することを検証してから、まとめて本体の手牌に反映する。
	/// 一部だけ削除された不整合な状態が残らないようにするため。
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="handTiles"/> のいずれかが手牌に存在しない場合。</exception>
	private void RemoveFromConcealedAtomically(params Tile[] handTiles)
	{
		var remaining = new List<Tile>(_concealedTiles);
		foreach (var handTile in handTiles)
		{
			if (!remaining.Remove(handTile))
			{
				throw new ArgumentException($"手牌に存在しない牌です: {handTile}");
			}
		}

		_concealedTiles.Clear();
		_concealedTiles.AddRange(remaining);
	}

	/// <summary><paramref name="handTiles"/> が全て <paramref name="claimedTile"/> と同じ種類（Suit/Rank）であることを確認する。</summary>
	/// <exception cref="ArgumentException">種類が一致しない牌が含まれる場合。</exception>
	private static void EnsureSameKind(Tile claimedTile, string description, params Tile[] handTiles)
	{
		foreach (var handTile in handTiles)
		{
			if (!IsSameKind(handTile, claimedTile))
			{
				throw new ArgumentException($"{description}はclaimedTileと種類が一致している必要があります。");
			}
		}
	}

	private static bool IsSameKind(Tile a, Tile b) => a.Suit == b.Suit && a.Rank == b.Rank;
}
