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

    [Header("ScoreUI の Source Asset")]
    [SerializeField] private VisualTreeAsset scoreUI = null;

    public Button hostReturnTitle = null;
    public Button clientReturnTitle = null;

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

        // HostReturnButtonという名前のButtonを取得
        hostReturnTitle = root.Q<Button>("HostReturnButton");
        // ClientReturnButtonという名前のButtonを取得
        clientReturnTitle = root.Q<Button>("ClientReturnButton");

        if (scoreLabel == null) return;

        // ScoreManagerにある合計ポイントをUIに表示する
        scoreLabel.text = moneyManager.totalMoney.ToString();

        //ここでネットワーク処理できるボタン処理を追加
    }   
}
