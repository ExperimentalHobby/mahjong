namespace Mahjong.Core.Domain;

/// <summary>
/// 四人麻雀の卓状態を管理し、ツモ・打牌・鳴き（ポン・チー・カン全種）・リーチ・ロン・ツモ和了・流局判定による
/// 手番進行を統括する。ロン/ツモ和了の成立時に<see cref="WinningYaku"/>で役牌・三暗刻・リーチ・嶺上開花・
/// 海底摸月・河底撈魚・槍槓を含む役を確定させ、<see cref="WinningHan"/>・<see cref="WinningFu"/>・
/// <see cref="WinningPoints"/>で翻数・符・点数も計算し、<see cref="Scores"/>で座席ごとの持ち点も
/// 実際に増減させる（役満を除く）。平和など待ちの形を条件とする役、一発・裏ドラ、供託・積み棒、
/// 局の推移（連荘・場風の遷移）は対象外（今後のマイルストーンで対応）。
/// </summary>
public sealed class MahjongEngine
{
	private const int StartingScore = 25000;

	private readonly Wall _wall;
	private readonly Dictionary<Seat, Hand> _hands;
	private readonly Dictionary<Seat, int> _scores;
	private bool _isRinshanDraw;
	private bool _isChankanTile;

	/// <summary>テスト用に卓状態を直接組み立てるための内部コンストラクタ。</summary>
	internal MahjongEngine(
		Wall wall, Dictionary<Seat, Hand> hands, Seat currentTurn, (Tile Tile, Seat Discarder)? lastDiscard,
		IReadOnlyList<Seat>? winners = null, Seat roundWind = Seat.East, bool isRinshanDraw = false,
		bool isChankanTile = false, Dictionary<Seat, int>? scores = null)
	{
		_wall = wall;
		_hands = hands;
		CurrentTurn = currentTurn;
		LastDiscard = lastDiscard;
		Winners = winners ?? [];
		RoundWind = roundWind;
		_isRinshanDraw = isRinshanDraw;
		_isChankanTile = isChankanTile;
		_scores = scores ?? hands.Keys.ToDictionary(seat => seat, _ => StartingScore);
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

	/// <summary>この局の場風。役牌（場風牌）の判定に使う。局の推移（連荘・場風の遷移）自体は対象外で、単一の値として保持する。</summary>
	public Seat RoundWind { get; }

	/// <summary>
	/// この局の親（ディーラー）。局の推移（連荘・親の交代）は対象外のため、常に東家（Seat.East）を
	/// 親として扱う簡略化（<see cref="RoundWind"/>が常に東で開始されるのと同じ考え方）。
	/// </summary>
	public Seat DealerSeat => Seat.East;

	/// <summary>
	/// 和了した座席。まだ誰も和了していない場合は空。通常和了・ダブロン（頭跳ねなし）では1〜2人分の座席を含む
	/// （<see cref="CallRon"/>・<see cref="CallTsumo"/>で設定される。トリプルロン成立時は<see cref="IsTripleRonDraw"/>
	/// が<c>true</c>になり、空のままになる）。
	/// </summary>
	public IReadOnlyList<Seat> Winners { get; private set; } = [];

	/// <summary>
	/// 和了時に成立していた役。座席をキーにする（ダブロンでは和了者ごとに手牌・役が異なるため）。
	/// まだ誰も和了していない場合は空（<see cref="CallRon"/>・<see cref="CallTsumo"/>で設定される）。
	/// </summary>
	public IReadOnlyDictionary<Seat, IReadOnlyList<Yaku>> WinningYaku { get; private set; } =
		new Dictionary<Seat, IReadOnlyList<Yaku>>();

	/// <summary>
	/// 和了時の翻数。座席をキーにする。役満（国士無双・字一色）の場合は翻数で表現できないため
	/// エントリを持たない（<see cref="Winners"/>・<see cref="WinningYaku"/>には引き続き含まれる）。
	/// まだ誰も和了していない場合は空。
	/// </summary>
	public IReadOnlyDictionary<Seat, int> WinningHan { get; private set; } = new Dictionary<Seat, int>();

	/// <summary>
	/// 和了時の符。座席をキーにする。役満（国士無双・字一色）の場合は符の対象外のため
	/// エントリを持たない（<see cref="Winners"/>・<see cref="WinningYaku"/>・<see cref="WinningHan"/>には
	/// 引き続き含まれる）。まだ誰も和了していない場合は空。
	/// </summary>
	public IReadOnlyDictionary<Seat, int> WinningFu { get; private set; } = new Dictionary<Seat, int>();

	/// <summary>
	/// 和了時の点数。座席をキーにする。ツモ和了の場合は他家からの支払いの合計（内訳は含まない）。
	/// 役満（国士無双・字一色）の場合は点数計算の対象外のためエントリを持たない
	/// （<see cref="Winners"/>・<see cref="WinningYaku"/>・<see cref="WinningHan"/>・<see cref="WinningFu"/>には
	/// 引き続き含まれる）。まだ誰も和了していない場合は空。
	/// </summary>
	public IReadOnlyDictionary<Seat, int> WinningPoints { get; private set; } = new Dictionary<Seat, int>();

	/// <summary>
	/// 座席ごとの持ち点。対局開始時は全員25000点。<see cref="CallRon"/>・<see cref="CallTsumo"/>の成立時
	/// （役満を除く）に<see cref="WinningPoints"/>と同じ支払額に基づいて実際に増減する。
	/// 供託（リーチ棒）・積み棒・流局時の聴牌払いは対象外。
	/// </summary>
	public IReadOnlyDictionary<Seat, int> Scores => _scores;

	/// <summary>
	/// 3人が同じ捨て牌に対して同時にロンを宣言した場合（三家和）に<c>true</c>になる。
	/// この場合、誰も和了せずその局は流局になる（<see cref="Winners"/>は空のまま）。
	/// </summary>
	public bool IsTripleRonDraw { get; private set; }

	/// <summary>生牌山が尽きて、かつ誰も和了しておらず三家和でもない場合に<c>true</c>になる（荒牌流局）。</summary>
	public bool IsExhaustiveDraw => Winners.Count == 0 && !IsTripleRonDraw && LiveWallCount == 0;

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
		_isRinshanDraw = false;
		_isChankanTile = false;
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
		_isChankanTile = false;
		CurrentTurn = NextSeat(discarder);
	}

	/// <summary>
	/// 現在の手番のプレイヤーがリーチを宣言し、<paramref name="tile"/>を打牌して手番を次の座席に進める。
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// 打牌待ち（ツモ直後）でない場合、または既にリーチ宣言済みの場合。
	/// </exception>
	/// <exception cref="ArgumentException">
	/// 副露がある場合、手牌に存在しない牌を指定した場合、または指定した牌を打牌しても聴牌にならない場合。
	/// </exception>
	public void Riichi(Tile tile)
	{
		var discarder = CurrentTurn;
		_hands[discarder].Riichi(tile);
		LastDiscard = (tile, discarder);
		_isChankanTile = false;
		CurrentTurn = NextSeat(discarder);
	}

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="caller"/>がポンを宣言する。
	/// 成立後、手番は<paramref name="caller"/>に移る（打牌待ちになるため、続けて<see cref="Discard"/>を呼ぶ想定）。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合、または槍槓成立待ちの場合。</exception>
	/// <exception cref="ArgumentException">
	/// 自分自身の捨て牌に対してポンしようとした場合、または<see cref="Hand.Pon"/>の既存バリデーションに違反する場合。
	/// </exception>
	public void CallPon(Seat caller, Tile handTile1, Tile handTile2)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("ポンできる捨て牌がありません。");
		}

		if (_isChankanTile)
		{
			throw new InvalidOperationException("槍槓成立待ちの牌に対してポンはできません。");
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
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合、または槍槓成立待ちの場合。</exception>
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

		if (_isChankanTile)
		{
			throw new InvalidOperationException("槍槓成立待ちの牌に対してチーはできません。");
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
	/// 成立後、<see cref="Winners"/>が<paramref name="caller"/>のみを含むリストになる。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合。</exception>
	/// <exception cref="ArgumentException">
	/// 自分自身の捨て牌に対してロンしようとした場合、または捨て牌を加えても和了形にならない場合。
	/// </exception>
	public void CallRon(Seat caller) => CallRon([caller]);

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="callers"/>が同時にロンを宣言する（複数家同時ロンに対応）。
	/// 頭跳ねはせず、1〜2人の場合は全員が和了となり<see cref="Winners"/>に設定される。
	/// 3人（他家全員）の場合は三家和として流局になり（<see cref="IsTripleRonDraw"/>が<c>true</c>になる）、
	/// 誰も和了しない。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合。</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="callers"/>が空の場合、同じ座席を複数回指定した場合、
	/// いずれかが自分自身の捨て牌に対してロンしようとした場合、
	/// またはいずれかの手牌が捨て牌を加えても和了形にならない場合。
	/// </exception>
	public void CallRon(IReadOnlyList<Seat> callers)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("ロンできる捨て牌がありません。");
		}

		if (callers.Count == 0)
		{
			throw new ArgumentException("ロンする座席を1つ以上指定してください。", nameof(callers));
		}

		if (callers.Distinct().Count() != callers.Count)
		{
			throw new ArgumentException("同じ座席を複数回指定することはできません。", nameof(callers));
		}

		foreach (var caller in callers)
		{
			if (caller == lastDiscard.Discarder)
			{
				throw new ArgumentException("自分の捨て牌はロンできません。", nameof(callers));
			}

			if (!_hands[caller].CanWinOn(lastDiscard.Tile))
			{
				throw new ArgumentException($"捨て牌を加えても和了形になりません: {caller}", nameof(callers));
			}
		}

		if (callers.Count == 3)
		{
			IsTripleRonDraw = true;
			return;
		}

		Winners = callers;
		var winningYaku = new Dictionary<Seat, IReadOnlyList<Yaku>>();
		var winningHan = new Dictionary<Seat, int>();
		var winningFu = new Dictionary<Seat, int>();
		var winningPoints = new Dictionary<Seat, int>();
		foreach (var caller in callers)
		{
			var yaku = new List<Yaku>(_hands[caller].DetermineYakuOn(lastDiscard.Tile, caller, RoundWind));
			if (_isChankanTile)
			{
				yaku.Add(Yaku.Chankan);
			}

			if (LiveWallCount == 0)
			{
				yaku.Add(Yaku.Houtei);
			}

			winningYaku[caller] = yaku;
			if (!HanCalculator.IsYakuman(yaku))
			{
				var han = HanCalculator.CalculateHan(yaku, _hands[caller].Melds.Count == 0);

				var hypotheticalTiles = new List<Tile>(_hands[caller].ConcealedTiles) { lastDiscard.Tile };
				var fu = FuCalculator.CalculateFu(
					hypotheticalTiles, _hands[caller].Melds, yaku, caller, RoundWind, lastDiscard.Tile);
				winningHan[caller] = han;
				winningFu[caller] = fu;
				var points = ScoreCalculator.CalculateRonPoints(han, fu, isDealer: caller == DealerSeat);
				winningPoints[caller] = points;
				_scores[caller] += points;
				_scores[lastDiscard.Discarder] -= points;
			}
		}

		WinningYaku = winningYaku;
		WinningHan = winningHan;
		WinningFu = winningFu;
		WinningPoints = winningPoints;
	}

	/// <summary>
	/// 直前の捨て牌に対して<paramref name="caller"/>が大明槓を宣言する。
	/// 成立後、手番は<paramref name="caller"/>に移り、嶺上牌を1枚ツモった状態（打牌待ち）になる。
	/// </summary>
	/// <exception cref="InvalidOperationException">直前の捨て牌が無い場合、または槍槓成立待ちの場合。</exception>
	/// <exception cref="ArgumentException">
	/// 自分自身の捨て牌に対してカンしようとした場合、または<see cref="Hand.OpenKan"/>の既存バリデーションに違反する場合。
	/// </exception>
	public void CallOpenKan(Seat caller, Tile handTile1, Tile handTile2, Tile handTile3)
	{
		if (LastDiscard is not { } lastDiscard)
		{
			throw new InvalidOperationException("カンできる捨て牌がありません。");
		}

		if (_isChankanTile)
		{
			throw new InvalidOperationException("槍槓成立待ちの牌に対して大明槓はできません。");
		}

		if (caller == lastDiscard.Discarder)
		{
			throw new ArgumentException("自分の捨て牌はカンできません。", nameof(caller));
		}

		_hands[caller].OpenKan(lastDiscard.Tile, handTile1, handTile2, handTile3);
		CurrentTurn = caller;
		LastDiscard = null;

		var rinshanTile = _wall.DrawReplacement();
		_hands[caller].Draw(rinshanTile);
		_isRinshanDraw = true;
	}

	/// <summary>
	/// 現在の手番のプレイヤーが自分の手牌4枚（直前のツモを含む）で暗槓を宣言する。
	/// 成立後、嶺上牌を1枚ツモった状態（打牌待ち）になる。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException"><see cref="Hand.ClosedKan"/>の既存バリデーションに違反する場合。</exception>
	public void CallClosedKan(Tile tile1, Tile tile2, Tile tile3, Tile tile4)
	{
		_hands[CurrentTurn].ClosedKan(tile1, tile2, tile3, tile4);

		var rinshanTile = _wall.DrawReplacement();
		_hands[CurrentTurn].Draw(rinshanTile);
		_isRinshanDraw = true;
	}

	/// <summary>
	/// 現在の手番のプレイヤーが既存のポンにツモ牌を追加して加槓を宣言する。成立後、嶺上牌はまだツモらず、
	/// 加槓した牌を<see cref="LastDiscard"/>として他家に公開する（槍槓の割り込み窓）。
	/// 誰もロン（槍槓）しなければ、続けて<see cref="ResolveAddedKan"/>を呼んで嶺上牌のツモを確定させる想定。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException"><see cref="Hand.AddedKan"/>の既存バリデーションに違反する場合。</exception>
	public void CallAddedKan(Tile tile)
	{
		_hands[CurrentTurn].AddedKan(tile);

		LastDiscard = (tile, CurrentTurn);
		_isChankanTile = true;
	}

	/// <summary>
	/// <see cref="CallAddedKan"/>成立後、誰も槍槓（ロン）しなかった場合に呼び出し、嶺上牌を1枚ツモった状態
	/// （打牌待ち）に確定させる。
	/// </summary>
	/// <exception cref="InvalidOperationException"><see cref="CallAddedKan"/>による槍槓成立待ちの状態でない場合。</exception>
	public void ResolveAddedKan()
	{
		if (!_isChankanTile)
		{
			throw new InvalidOperationException("加槓が成立していないため、嶺上牌をツモれません。");
		}

		LastDiscard = null;
		_isChankanTile = false;

		var rinshanTile = _wall.DrawReplacement();
		_hands[CurrentTurn].Draw(rinshanTile);
		_isRinshanDraw = true;
	}

	/// <summary>
	/// 現在の手番のプレイヤーがツモ和了を宣言する。成立後、<see cref="Winners"/>が現在の手番のみを含むリストになる。
	/// </summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">手牌が和了形になっていない場合。</exception>
	public void CallTsumo()
	{
		if (!_hands[CurrentTurn].IsComplete())
		{
			throw new ArgumentException("現在の手牌はツモ和了できる形になっていません。");
		}

		Winners = [CurrentTurn];
		var yaku = new List<Yaku>(_hands[CurrentTurn].DetermineYaku(CurrentTurn, RoundWind));
		if (_isRinshanDraw)
		{
			yaku.Add(Yaku.RinshanKaihou);
		}
		else if (LiveWallCount == 0)
		{
			yaku.Add(Yaku.Haitei);
		}

		WinningYaku = new Dictionary<Seat, IReadOnlyList<Yaku>> { [CurrentTurn] = yaku };

		var winningHan = new Dictionary<Seat, int>();
		var winningFu = new Dictionary<Seat, int>();
		var winningPoints = new Dictionary<Seat, int>();
		if (!HanCalculator.IsYakuman(yaku))
		{
			var han = HanCalculator.CalculateHan(yaku, _hands[CurrentTurn].Melds.Count == 0);
			var fu = FuCalculator.CalculateFu(
				_hands[CurrentTurn].ConcealedTiles, _hands[CurrentTurn].Melds, yaku, CurrentTurn, RoundWind, ronTile: null);
			winningHan[CurrentTurn] = han;
			winningFu[CurrentTurn] = fu;
			if (CurrentTurn == DealerSeat)
			{
				var each = ScoreCalculator.CalculateDealerTsumoPointsFromEach(han, fu);
				winningPoints[CurrentTurn] = each * 3;
				foreach (var seat in _hands.Keys.Where(seat => seat != CurrentTurn))
				{
					_scores[seat] -= each;
				}

				_scores[CurrentTurn] += each * 3;
			}
			else
			{
				var (fromDealer, fromNonDealer) = ScoreCalculator.CalculateNonDealerTsumoPoints(han, fu);
				winningPoints[CurrentTurn] = fromDealer + (fromNonDealer * 2);
				_scores[DealerSeat] -= fromDealer;
				foreach (var seat in _hands.Keys.Where(seat => seat != CurrentTurn && seat != DealerSeat))
				{
					_scores[seat] -= fromNonDealer;
				}

				_scores[CurrentTurn] += fromDealer + (fromNonDealer * 2);
			}
		}

		WinningHan = winningHan;
		WinningFu = winningFu;
		WinningPoints = winningPoints;
	}

	/// <summary>この卓状態の独立した複製を返す（複製後は互いの操作が影響し合わない）。</summary>
	public MahjongEngine Clone()
	{
		var clonedHands = new Dictionary<Seat, Hand>();
		foreach (var (seat, hand) in _hands)
		{
			clonedHands[seat] = hand.Clone();
		}

		return new MahjongEngine(
			_wall.Clone(), clonedHands, CurrentTurn, LastDiscard, Winners, RoundWind, _isRinshanDraw, _isChankanTile,
			new Dictionary<Seat, int>(_scores))
		{
			WinningYaku = WinningYaku,
			WinningHan = WinningHan,
			WinningFu = WinningFu,
			WinningPoints = WinningPoints,
			IsTripleRonDraw = IsTripleRonDraw,
		};
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
