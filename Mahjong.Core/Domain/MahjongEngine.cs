namespace Mahjong.Core.Domain;

/// <summary>
/// 四人麻雀の卓状態を管理し、ツモ・打牌による手番進行を統括する。
/// 鳴き・ロン・リーチ・流局判定は対象外（今後のマイルストーンで対応）。
/// </summary>
public sealed class MahjongEngine
{
	private readonly Wall _wall;
	private readonly Dictionary<Seat, Hand> _hands;

	private MahjongEngine(Wall wall, Dictionary<Seat, Hand> hands, Seat currentTurn)
	{
		_wall = wall;
		_hands = hands;
		CurrentTurn = currentTurn;
	}

	/// <summary>現在の手番の座席。</summary>
	public Seat CurrentTurn { get; private set; }

	/// <summary>生牌山の残り枚数。</summary>
	public int LiveWallCount => _wall.LiveWallCount;

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

		return new MahjongEngine(wall, hands, Seat.East);
	}

	/// <summary>現在の手番のプレイヤーが牌山から1枚ツモる。</summary>
	/// <exception cref="InvalidOperationException">生牌山が0枚のときに呼び出した場合。</exception>
	public void DrawForCurrentPlayer()
	{
		var tile = _wall.Draw();
		_hands[CurrentTurn].Draw(tile);
	}

	/// <summary>現在の手番のプレイヤーが指定した牌を打牌し、手番を次の座席に進める。</summary>
	/// <exception cref="InvalidOperationException">打牌待ち（ツモ直後）でない場合。</exception>
	/// <exception cref="ArgumentException">手牌に存在しない牌を指定した場合。</exception>
	public void Discard(Tile tile)
	{
		_hands[CurrentTurn].Discard(tile);
		CurrentTurn = NextSeat(CurrentTurn);
	}

	/// <summary>この卓状態の独立した複製を返す（複製後は互いの操作が影響し合わない）。</summary>
	public MahjongEngine Clone()
	{
		var clonedHands = new Dictionary<Seat, Hand>();
		foreach (var (seat, hand) in _hands)
		{
			clonedHands[seat] = hand.Clone();
		}

		return new MahjongEngine(_wall.Clone(), clonedHands, CurrentTurn);
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
