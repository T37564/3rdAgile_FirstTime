using Fusion;
using Network.Player;
using UnityEngine;

public class ArrowUIManager : MonoBehaviour
{
    [Header("シーン上に配置している矢印UI一覧")]
    [SerializeField] private DirectionArrowUI[] arrowUIs = null;

    // 自分が操作しているプレイヤーを保持する
    private PlayerController localPlayer = null;

    /// <summary>
    /// ローカルプレイヤーがまだ見つかっていない場合は探す
    /// </summary>
    private void Update()
    {
        // すでにローカルプレイヤーを取得済みなら何もしない
        if (localPlayer != null) return;

        FindLocalPlayer();
    }

    /// <summary>
    /// シーン上に存在するプレイヤーの中から、
    /// 自分が操作しているプレイヤーを探して矢印UIに渡す
    /// </summary>
    private void FindLocalPlayer()
    {
        // 現在シーン上に存在しているPlayerControllerをすべて取得
        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            // 念のためnullチェック
            if (player == null) continue;

            // 自分が操作しているプレイヤー以外はスキップ
            if (!player.HasInputAuthority) continue;

            // ローカルプレイヤーとして保持
            localPlayer = player;

            // 取得したプレイヤー情報をすべての矢印UIに渡す
            SetPlayerToArrows(localPlayer.transform);

            return;
        }
    }

    /// <summary>
    /// すべての矢印UIに、基準となるプレイヤーを設定する
    /// </summary>
    private void SetPlayerToArrows(Transform playerTransform)
    {
        // 矢印UI配列が未設定なら何もしない
        if (arrowUIs == null) return;

        foreach (DirectionArrowUI arrow in arrowUIs)
        {
            // 要素が空ならスキップ
            if (arrow == null) continue;

            // 各矢印UIにローカルプレイヤーを設定
            arrow.SetPlayer(playerTransform);
        }
    }
}