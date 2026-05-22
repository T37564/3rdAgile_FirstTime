// -----------------------------------------------------------------------------------
// リザルト画面にスコアを表示するクラス
// ScoreUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreUI : MonoBehaviour
{
    [Header("スコア表示用のUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    /// <summary>
    /// スコアを取得してUIへ表示する
    /// </summary>
    private void OnEnable()
    {
        // ScoreManagerを取得
        ScoreManager scoreManager = FindAnyObjectByType<ScoreManager>();

        if (scoreManager == null) return;

        // UIDocumentのルート要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // Scoreという名前のLabelを取得
        Label scoreLabel = root.Q<Label>("Score");

        if (scoreLabel == null) return;

        // ScoreManagerにある合計ポイントをUIに表示する
        scoreLabel.text = scoreManager.totalPoint.ToString();
    }
}
