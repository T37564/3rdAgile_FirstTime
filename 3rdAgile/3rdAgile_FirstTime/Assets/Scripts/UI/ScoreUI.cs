// -----------------------------------------------------------------------------------
// リザルト画面にスコアを表示するクラス
// ScoreUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreUI : MonoBehaviour
{
    [Header("スコア表示用のUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("ScoreUI の Source Asset")]
    [SerializeField] private VisualTreeAsset scoreUI = null;

    /// <summary>
    /// スコアを取得してUIへ表示する
    /// </summary>
    private void OnEnable()
    {
        // MoneyManagerを取得
        MoneyManager moneyManager = FindAnyObjectByType<MoneyManager>();

        if (moneyManager == null) return;

        // UIの変更
        uiDocument.rootVisualElement.Clear();

        scoreUI.CloneTree(uiDocument.rootVisualElement);

        // UIDocumentのルート要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // Scoreという名前のLabelを取得
        Label scoreLabel = root.Q<Label>("Score");

        if (scoreLabel == null) return;

        // ScoreManagerにある合計ポイントをUIに表示する
        scoreLabel.text = moneyManager.totalMoney.ToString();

        StartCoroutine(DisplayScore());
    }   

    /// <summary>
    /// スコア画面を５秒間表示後接続を終了させる処理
    /// </summary>
    private IEnumerator DisplayScore()
    {
        yield return new WaitForSeconds(5.0f);

        NetworkGameStarter.Instance.ShutdownRunner();
    }
}
