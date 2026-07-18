namespace Mahjong.Core.Domain;

/// <summary>面子（鳴き）の種類。</summary>
public enum MeldType
{
	/// <summary>ポン（刻子）。</summary>
	Pon,

	/// <summary>チー（順子）。</summary>
	Chi,

	/// <summary>明槓（大明槓）。</summary>
	OpenKan,

	/// <summary>暗槓。</summary>
	ClosedKan,

	/// <summary>加槓。</summary>
	AddedKan,
}
