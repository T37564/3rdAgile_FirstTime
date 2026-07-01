// -----------------------------------------------------------------------------------
// ゲーム内のUIを管理するクラス
// InGameUIController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [Header("スコアUI")]
    [SerializeField] private ScoreUI scoreUI = null;

    /// <summary>
    /// スコアUIを表示する
    /// </summary>
    public void ShowScoreUI()
    {
        scoreUI.enabled = true;
    }
}
