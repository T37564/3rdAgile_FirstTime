using UnityEngine;
using System.Collections.Generic;
using Network.Player;

public class PlayerDeathObservation : MonoBehaviour
{
    // PlayerController参照用
    List<PlayerController> players = new List<PlayerController>();

    [SerializeField] private GameTimer gameTimer = null;

    private int playerDeathCount = 0;

    private void Update()
    {
        if (players.Count <= 0)
        {
            GetReference();
        }

        if (players.Count <= 0) return;

        foreach (PlayerController playerController in players)
        {
            if (!playerController.IsAlive)
            {
                playerDeathCount++;
            }
        }

        // 全員死亡時ゲームを終わらせる
        if (playerDeathCount == players.Count)
        {
            gameTimer.CurrentPhase = GamePhase.Finished;
        }
    }

    /// <summary>
    /// 参照用PlayerController取得
    /// </summary>
    private void GetReference()
    {
        // 現在シーン上に存在するPlayerControllerをすべて取得
        PlayerController[] playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController playerController in playerControllers)
        {
            players.Add(playerController);
        }
    }
}
