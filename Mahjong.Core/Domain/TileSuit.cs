namespace Mahjong.Core.Domain;

/// <summary>牌の種類（萬子・筒子・索子・字牌）を表す。</summary>
public enum TileSuit
{
    /// <summary>萬子。</summary>
    Man,

    /// <summary>筒子。</summary>
    Pin,

    /// <summary>索子。</summary>
    Sou,

    /// <summary>字牌（風牌・三元牌）。</summary>
    Honor,
}
