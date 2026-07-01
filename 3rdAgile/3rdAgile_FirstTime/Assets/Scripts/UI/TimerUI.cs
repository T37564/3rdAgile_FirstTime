// -----------------------------------------------------------------------------------
// ゲームの残り時間をUIに表示するクラス
// TimerUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;

public class TimerUI : MonoBehaviour
{
    [Header("タイマー表示用のUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("GameTimer参照")]
    [SerializeField] private GameTimer gameTimer = null;

    // 残り時間を表示するLabel
    private Label timerLabel = null;

    /// <summary>
    /// UIDocument生成後、タイマー表示用のLabelを取得する
    /// </summary>
    private void Start()
    {
        // UIDocumentのルート要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // TimerLabelという名前のLabelを取得
        timerLabel = root.Q<Label>("TimerLabel");
    }

    /// <summary>
    /// 毎フレーム残り時間を更新してUIに表示する
    /// </summary>
    private void Update()
    {
        // GameTimer参照が未設定の場合は処理しない
        if (gameTimer == null)
            return;

        // timerLabelが取得できていない場合は処理しない
        if (timerLabel == null)
            return;

        // Spawn前はNetworkedプロパティにアクセスできないため処理しない
        if (gameTimer.Object == null || !gameTimer.Object.IsValid)
            return;

        // ゲームの残り時間を取得
        float time = gameTimer.RemainingTime;

        // 秒数を「分」と「秒」に変換
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // 00:00形式でUIに表示
        timerLabel.text = $"{minutes:00}:{seconds:00}";
    }
}