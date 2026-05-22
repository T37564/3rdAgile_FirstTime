using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private ScoreUI scoreUI = null;

    /// <summary>
    /// スコアUIを表示する
    /// </summary>
    public void ShowScoreUI()
    {
        scoreUI.enabled = true;
    }
}
