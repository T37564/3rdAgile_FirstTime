//=================================================================================================
// スコアに関するメソッド
// 製作者：スズキ
//=================================================================================================

using Fusion;

/// <summary>
/// スコアに関する処理を持つクラス
/// </summary>
public class ScoreManager : NetworkBehaviour
{
    /// <summary>
    /// 総スコア
    /// </summary>
    [Networked] public int totalPoint { get; private set; }

    /// <summary>
    /// 加算メソッド
    /// </summary>
    public void AddScore(int point)
    {
        if(!Object.HasStateAuthority) return;
        totalPoint += point;
    }
}
