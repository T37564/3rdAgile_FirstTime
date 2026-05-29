using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [Header("スコアを表示するクラス")]
    [SerializeField] private ScoreUI scoreUI = null;

    /// <summary>
    /// スコアUIを表示する
    /// </summary>
    public void ShowScoreUI()
    {
        scoreUI.enabled = true;
    }
}
