namespace Mahjong.Core.Domain;

/// <summary>
/// 四人麻雀の卓状態を管理し、ツモ・打牌・鳴き（ポン・チー）・ロンによる手番進行を統括する。
/// 大明槓（嶺上牌を引く処理を含む）・リーチ・流局判定・役判定/得点計算は対象外（今後のマイルストーンで対応）。
/// </summary>
public sealed class MahjongEngine
{
	private readonly Wall _wall;
	private readonly Dictionary<Seat, Hand> _hands;

	/// <summary>テスト用に卓状態を直接組み立てるための内部コンストラクタ。</summary>
	internal MahjongEngine(
		Wall wall, Dictionary<Seat, Hand> hands, Seat currentTurn, (Tile Tile, Seat Discarder)? lastDiscard, Seat? winner = null)
	{
		_wall = wall;
		_hands = hands;
		CurrentTurn = currentTurn;
		LastDiscard = lastDiscard;
		Winner = winner;
	}

	/// <summary>現在の手番の座席。</summary>
	public Seat CurrentTurn { get; private set; }

	/// <summary>
	/// 直前の捨て牌とその打牌者。まだ誰も鳴いていない場合のみ値を持つ
	/// （<see cref="DrawForCurrentPlayer"/> や鳴きの成立でnullに戻る）。
	/// </summary>
	public (Tile Tile, Seat Discarder)? LastDiscard { get; private set; }

	/// <summary>生牌山の残り枚数。</summary>
	public int LiveWallCount => _wall.LiveWallCount;

	/// <summary>和了した座席。まだ誰も和了していない場合は<c>null</c>（<see cref="CallRon"/>で設定される）。</summary>
	public Seat? Winner { get; private set; }

	/// <summary>生牌山が尽きて、かつ誰も和了していない場合に<c>true</c>になる（流局）。</summary>
	public bool IsExhaustiveDraw => Winner is null && LiveWallCount == 0;

	/// <summary>座席ごとの手牌。</summary>
	public IReadOnlyDictionary<Seat, Hand> Hands => _hands;

	/// <summary>牌山をシャッフルし、東家から順に配牌して東家の手番から対局を開始する。</summary>
	public static MahjongEngine Start(Random random)
	{
		var wall = Wall.CreateShuffled(random);
		var dealt = wall.DealInitialHands();

		var hands = new Dictionary<Seat, Hand>();
		foreach (var (seat, tiles) in dealt)
		{
			hands[seat] = new Hand(tiles);
		}

		return new MahjongEngine(wall, hands, Seat.East, lastDiscard: null);
	}

	/// <summary>現在の手番のプレイヤーが牌山から1枚ツモる。</summary>
	/// <exception cref="InvalidOperationException">生牌山が0枚のときに呼び出した場合。</exception>
	public void DrawForCurrentPlayer()
	{
		LastDiscard = null;
		var tile = _wall.Draw();
		_hands[CurrentTurn].Draw(tile);
	}

	/// <summary>現在の手番のプレイヤーが指定した牌を打牌し、手番を次の座席に進める。</summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">手牌に存在しない牌を指定した場合。</exception>
	public void Discard(Tile tile)
	{
		var discarder = CurrentTurn;
		_hands[discarder].Discard(tile);
		LastDiscard = (tile, discarder);
		CurrentTurn = NextSeat(discarder);
	}

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="caller"/>がポンを宣言する。
	/// 成立後、手番は<paramref name="caller"/>に移る（打牌待ちになるため、続けて<see cref="Discard"/>を呼ぶ想定）。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合。</exception>
	/// <exception cref="ArgumentException">
	/// 自分自身の捨て牌に対してポンしようとした場合、または<see cref="Hand.Pon"/>の既存バリデーションに違反する場合。
	/// </exception>
	public void CallPon(Seat caller, Tile handTile1, Tile handTile2)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("ポンできる捨て牌がありません。");
		}

		if (caller == lastDiscard.Discarder)
		{
			throw new ArgumentException("自分の捨て牌はポンできません。", nameof(caller));
		}

		_hands[caller].Pon(lastDiscard.Tile, handTile1, handTile2);
		CurrentTurn = caller;
		LastDiscard = null;
	}

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="caller"/>がチーを宣言する。
	/// 成立後、手番は<paramref name="caller"/>に移る（打牌待ちになるため、続けて<see cref="Discard"/>を呼ぶ想定）。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合。</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="caller"/>が打牌者の下家（上家からの捨て牌）でない場合、
	/// または<see cref="Hand.Chi"/>の既存バリデーションに違反する場合。
	/// </exception>
	public void CallChi(Seat caller, Tile handTile1, Tile handTile2)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("チーできる捨て牌がありません。");
		}

		if (caller != NextSeat(lastDiscard.Discarder))
		{
			throw new ArgumentException("チーは上家の捨て牌に対してのみ可能です。", nameof(caller));
		}

		_hands[caller].Chi(lastDiscard.Tile, handTile1, handTile2);
		CurrentTurn = caller;
		LastDiscard = null;
	}

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="caller"/>がロンを宣言する。
	/// 成立後、<see cref="Winner"/>が<paramref name="caller"/>になる（対局終了の意思表示。
	/// 流局判定・対局終了処理は今後のマイルストーンで対応する）。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合。</exception>
	/// <exception cref="ArgumentException">
	/// 自分自身の捨て牌に対してロンしようとした場合、または捨て牌を加えても和了形にならない場合。
	/// </exception>
	public void CallRon(Seat caller)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("ロンできる捨て牌がありません。");
		}

		if (caller == lastDiscard.Discarder)
		{
			throw new ArgumentException("自分の捨て牌はロンできません。", nameof(caller));
		}

		if (!_hands[caller].CanWinOn(lastDiscard.Tile))
		{
			throw new ArgumentException("捨て牌を加えても和了形になりません。", nameof(caller));
		}

		Winner = caller;
	}

	/// <summary>
	/// 現在の手番のプレイヤーがツモ和了を宣言する。成立後、<see cref="Winner"/>が現在の手番になる。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">手牌が和了形になっていない場合。</exception>
	public void CallTsumo()
	{
		if (!_hands[CurrentTurn].IsComplete())
		{
			throw new ArgumentException("現在の手牌はツモ和了できる形になっていません。");
		}

		Winner = CurrentTurn;
	}

	/// <summary>この卓状態の独立した複製を返す（複製後は互いの操作が影響し合わない）。</summary>
	public MahjongEngine Clone()
	{
		var clonedHands = new Dictionary<Seat, Hand>();
		foreach (var (seat, hand) in _hands)
		{
			clonedHands[seat] = hand.Clone();
		}

		return new MahjongEngine(_wall.Clone(), clonedHands, CurrentTurn, LastDiscard, Winner);
	}

	private static Seat NextSeat(Seat seat) => seat switch
	{
		Seat.East => Seat.South,
		Seat.South => Seat.West,
		Seat.West => Seat.North,
		Seat.North => Seat.East,
		_ => throw new InvalidOperationException($"未知のSeatです: {seat}"),
	};
}
