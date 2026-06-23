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

    [SerializeField] private GameObject returnButtonUI = null;

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

        // HostReturnButtonという名前のButtonを取得
        Button hostReturnTitle = root.Q<Button>("HostReturnButton");
        // ClientReturnButtonという名前のButtonを取得
        Button clientReturnTitle = root.Q<Button>("ClientReturnButton");
        // MessageLogという名前のVisualElementを取得
        VisualElement visualElement = root.Q<VisualElement>("MessageLog");

        ReturnButtonUI button = returnButtonUI.GetComponent<ReturnButtonUI>();
        button.hostButton = hostReturnTitle;
        button.clientButton = clientReturnTitle;
        button.messageLog = visualElement;

        returnButtonUI.SetActive(true);
    }
}
