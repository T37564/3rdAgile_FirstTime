using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ゲームの残り時間をUIに表示するクラス。
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("タイマー表示用のUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("ゲーム時間を管理するクラス")]
    [SerializeField] private GameTimer gameTimer = null;

    // 残り時間を表示するLabel
    private Label timerLabel = null;

    /// <summary>
    /// UI生成時にLabelを取得する。
    /// </summary>
    private void Start()
    {
        // UIDocumentのルート要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // TimerLabelという名前のLabelを取得
        timerLabel = root.Q<Label>("TimerLabel");
    }

    /// <summary>
    /// 毎フレーム残り時間を更新してUIに表示する。
    /// </summary>
    private void Update()
    {
        // GameTimerが設定されていない場合は処理しない
        if (gameTimer == null)
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