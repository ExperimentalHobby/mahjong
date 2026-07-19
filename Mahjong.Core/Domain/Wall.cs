namespace Mahjong.Core.Domain;

/// <summary>四人麻雀の牌山（山札）。</summary>
public sealed class Wall
{
	private const int DeadWallSize = 14;

	private static readonly Seat[] DealingOrder = [Seat.East, Seat.South, Seat.West, Seat.North];

	private readonly List<Tile> _liveWall;
	private readonly List<Tile> _deadWall;

	/// <summary>テスト用に牌山を直接組み立てるための内部コンストラクタ。</summary>
	internal Wall(List<Tile> liveWall, List<Tile> deadWall)
	{
		_liveWall = liveWall;
		_deadWall = deadWall;
	}

	/// <summary>生牌山の残り枚数。</summary>
	public int LiveWallCount => _liveWall.Count;

	/// <summary>
	/// 初期ドラ表示牌（王牌から1枚）。
	/// カンによるドラ表示牌の追加や裏ドラは今回のスコープ外。
	/// </summary>
	public Tile DoraIndicator => _deadWall[0];

	/// <summary>
	/// 136枚をシャッフルし、王牌14枚を分離した牌山を生成する。
	/// 生牌山は136-14=122枚となる。
	/// </summary>
	public static Wall CreateShuffled(Random random)
	{
		var tiles = new List<Tile>(CreateStandardTileSet());

		for (var i = tiles.Count - 1; i > 0; i--)
		{
			var j = random.Next(i + 1);
			(tiles[i], tiles[j]) = (tiles[j], tiles[i]);
		}

		var deadWall = tiles.GetRange(tiles.Count - DeadWallSize, DeadWallSize);
		var liveWall = tiles.GetRange(0, tiles.Count - DeadWallSize);

		return new Wall(liveWall, deadWall);
	}

	/// <summary>この牌山の独立した複製を返す（複製後は互いのDraw()/DrawReplacement()が影響し合わない）。</summary>
	public Wall Clone() => new(new List<Tile>(_liveWall), new List<Tile>(_deadWall));

	/// <summary>
	/// カン成立時に王牌から嶺上牌を1枚引く。王牌を14枚に保つため、生牌山の末尾から1枚を王牌に補充する
	/// （ドラ表示牌の位置＝deadWall[0]は補充対象にしない）。
	/// </summary>
	/// <exception cref="InvalidOperationException">生牌山が0枚のときに呼び出した場合。</exception>
	public Tile DrawReplacement()
	{
		if (_liveWall.Count == 0)
		{
			throw new InvalidOperationException("生牌山が尽きているため嶺上牌を引けません。");
		}

		var rinshanTile = _deadWall[^1];
		_deadWall.RemoveAt(_deadWall.Count - 1);

		_deadWall.Add(_liveWall[^1]);
		_liveWall.RemoveAt(_liveWall.Count - 1);

		return rinshanTile;
	}

	/// <summary>生牌山から1枚引く。</summary>
	/// <exception cref="InvalidOperationException">生牌山が0枚のときに呼び出した場合。</exception>
	public Tile Draw()
	{
		if (_liveWall.Count == 0)
		{
			throw new InvalidOperationException("生牌山が尽きています。");
		}

		var tile = _liveWall[^1];
		_liveWall.RemoveAt(_liveWall.Count - 1);
		return tile;
	}

	/// <summary>
	/// 配牌として East→South→West→North の順に各13枚を生牌山から引く。
	/// 東家の14枚目（最初のツモ）はここには含めない。
	/// </summary>
	public IReadOnlyDictionary<Seat, IReadOnlyList<Tile>> DealInitialHands()
	{
		var hands = new Dictionary<Seat, IReadOnlyList<Tile>>();

		foreach (var seat in DealingOrder)
		{
			var hand = new List<Tile>(13);
			for (var i = 0; i < 13; i++)
			{
				hand.Add(Draw());
			}

			hands[seat] = hand;
		}

		return hands;
	}

	/// <summary>
	/// 四人麻雀で使う136枚の牌一式を、シャッフルしていない決定的な順序で生成する。
	/// 数牌の各スートのRank=5は1枚を赤ドラとする。
	/// </summary>
	internal static IReadOnlyList<Tile> CreateStandardTileSet()
	{
		var tiles = new List<Tile>(136);

		foreach (var suit in new[] { TileSuit.Man, TileSuit.Pin, TileSuit.Sou })
		{
			for (var rank = 1; rank <= 9; rank++)
			{
				for (var copy = 0; copy < 4; copy++)
				{
					var isRedFive = rank == 5 && copy == 0;
					tiles.Add(new Tile(suit, rank, isRedFive));
				}
			}
		}

		for (var rank = 1; rank <= 7; rank++)
		{
			for (var copy = 0; copy < 4; copy++)
			{
				tiles.Add(new Tile(TileSuit.Honor, rank));
			}
		}

		return tiles;
	}
}
