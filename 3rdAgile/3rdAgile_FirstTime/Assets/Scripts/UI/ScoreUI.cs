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

    [Header("ReturnButtonUI 参照用に使用するオブジェクト")]
    [SerializeField] private GameObject returnButtonUI = null;

    [Header("すべての称号の画像")]
    [SerializeField] private Texture2D[] titleTexture = null;

    /// <summary>
    /// スコアを取得してUIへ表示する
    /// </summary>
    private void OnEnable()
    {
        // シーン内のMoneyManagerを取得
        MoneyManager moneyManager = FindAnyObjectByType<MoneyManager>();

        // MoneyManagerが取得できていない場合は処理しない
        if (moneyManager == null) return;

        // ScoreUIに変更
        uiDocument.rootVisualElement.Clear();
        scoreUI.CloneTree(uiDocument.rootVisualElement);

        // UIDocumentのルート要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // UXML内からLabel "Score" を取得
        Label scoreLabel = root.Q<Label>("Score");

        // ScoreLabelが取得できていない場合は処理しない
        if (scoreLabel == null) return;
        // MoneyManagerにある合計スコアを表示する
        scoreLabel.text = moneyManager.totalMoney.ToString();

        // UXML内から各Buttonを取得
        Button hostReturnTitle = root.Q<Button>("HostReturnButton");
        Button clientReturnTitle = root.Q<Button>("ClientReturnButton");
        VisualElement visualElement = root.Q<VisualElement>("MessageLog");
        VisualElement titleSignboard = root.Q<VisualElement>("TitleSignboard");

        // ReturnButtonUIのコンポーネントを取得
        ReturnButtonUI button = returnButtonUI.GetComponent<ReturnButtonUI>();

        // 称号をスコアによって変更する処理
        ChangeTitleSignboard(moneyManager.totalMoney, titleSignboard);

        // ReturnButtonUIの各ボタンとMessageLogを設定
        button.hostButton = hostReturnTitle;
        button.clientButton = clientReturnTitle;
        button.messageLog = visualElement;

        // ReturnButtonUIを有効にする
        returnButtonUI.SetActive(true);
    }

    /// <summary>
    /// 称号をスコアによって変更する処理
    /// </summary>
    private void ChangeTitleSignboard(int totalMoney, VisualElement titleSignboard)
    {
        titleSignboard.style.backgroundImage = titleTexture[totalMoney switch
        {
            < 50000 => 0,
            < 100000 => 1,
            < 180000 => 2,
            < 250000 => 3,
            < 350000 => 4,
            _ => 5
        }];
    }
}
