using UnityEngine;
using System.Collections.Generic;
using Network.Player;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class PlayerDeathObservation : MonoBehaviour
{
    // PlayerController参照用
    private List<PlayerController> players = new List<PlayerController>();

    [SerializeField] private GameTimer gameTimer = null;

    [Header("ゲームオーバーになったことを知らせるテキスト")]
    [SerializeField] private TextMeshProUGUI gameOverMessage = null;
    [Header("ゲームオーバー時の背景")]
    [SerializeField] private Image gameOverBackImage = null;

    private int playerDeathCount = 0;

    // ゲームオーバー中かの判定
    private bool isGameOver = false;

    private void Update()
    {
        if (players.Count <= 0)
        {
            GetReference();
        }

        if (players.Count <= 0) return;

        foreach (PlayerController playerController in players)
        {
            if (!playerController.IsAlive && !playerController.isAliveCount)
            {
                playerController.isAliveCount = true;
                playerDeathCount++;
            }
        }

        // 全員死亡時ゲームを終わらせる
        if (playerDeathCount == players.Count && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(TotalWipeoutGameEnd());
        }
    }

    /// <summary>
    /// 全滅時のUI表示処理
    /// </summary>
    private IEnumerator TotalWipeoutGameEnd()
    {
        // 全滅したことを知らせるUI表示
        gameOverMessage.DOFade(1.0f, 1.0f);
        gameOverBackImage.DOFade(1.0f, 1.0f);

        yield return new WaitForSeconds(2.0f);

        // ゲームを終わらせる
        gameTimer.CurrentPhase = GamePhase.Finished;

        gameOverMessage.enabled = false;
        gameOverBackImage.enabled = false;
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
