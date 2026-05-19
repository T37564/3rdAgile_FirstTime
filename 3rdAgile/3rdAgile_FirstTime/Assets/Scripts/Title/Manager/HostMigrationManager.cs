using UnityEngine;

public class HostMigrationManager : SingletonMonobehaviour<HostMigrationManager>
{
    /// <summary>
    /// ホストが抜けたとき
    /// ホストの移行処理
    /// </summary>
    public void HandleHostMigration()
    {

    }

    /// <summary>
    /// 前フェイズに復元する処理
    /// </summary>
    private void RestorePhase()
    {
    }


    /// <summary>
    /// プレイヤーの位置などを復元する処理
    /// </summary>
    private void RestorePlayers()
    {
    }


    /// <summary>
    /// アイテムを再生成する処理
    /// </summary>
    private void RegenerateItems()
    {
    }
}
