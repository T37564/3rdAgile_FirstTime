//======================================================================
// 製作者：スズキ
//======================================================================

using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[Serializable]
public class GamePhaseTime
{
    [Header("フェーズの種類")]
    public GamePhase phase;
    [Header("このフェーズの時間（秒）")]
    public float timeInSeconds;
}

/// <summary>
/// ゲーム時間を管理するクラス。
/// </summary>
public class GameTimer : NetworkBehaviour
{

    // ゲームのフェーズが変わるときに呼び出されるイベント
    public event Action<GamePhase> OnPhaseChanged;
    // タイムアップになったときに呼び出されるイベント
    public event Action OnTimeUp;

    [Header("-- Timer Settings --")]
    [Header("ゲーム全体の時間（秒）")]
    [SerializeField] private float totalTime = 360f; // 6 minutes

    [Header("各フェーズの時間（秒）")]
    [SerializeField] private float phaseLength = 120f; // 2 minutes per phase

    // ゲームの残り時間を管理するためのTickTimerと、現在のゲームフェーズを管理するためのNetworkedプロパティ
    [Networked] private TickTimer GameTimerTick { get; set; }
    [Networked] public GamePhase CurrentPhase { get;  set; }

    // 比較用の変数を用意して、フェーズが変わったときにイベントを呼び出すために使用
    private GamePhase previousPhase;

    [Header("フェーズごとの時間設定")]
    [SerializeField] private GamePhaseTime[] gamePhaseTimes;

    // タイマーが開始されているかの判定
    [Networked]
    private NetworkBool isStartedTimer { get; set; }

    // スコアUI表示するためのフラグ
    private bool isShowScore = false;

    private bool isSpawned = false;

    /// <summary>
    /// 残り時間を秒で返すプロパティ。
    /// タイマーが終了しているか、まだ開始されていない場合は0を返す。
    /// </summary>
    public float RemainingTime
    {
        get
        {
            // タイマーが終了しているか、まだ開始されていない場合は0を返す
            if (GameTimerTick.ExpiredOrNotRunning(Runner))
                return 0f;

            // マイナスにならないように、残り時間を返す
            return Mathf.Max(0.0f, GameTimerTick.RemainingTime(Runner) ?? 0.0f);
        }
    }

    public override void Spawned()
    {
        isSpawned = true;

        if (Object.HasStateAuthority)
        {
            isStartedTimer = false;
            CurrentPhase = GamePhase.Phase1;
        }

        previousPhase = CurrentPhase;
    }


    /// <summary>
    /// スコアUIを表示するための処理。ゲームが終了している場合はスコアUIを表示する。
    /// </summary>
    private void Update()
    {
        if(!isSpawned) return;

        if (isShowScore) return;

        if (CurrentPhase == GamePhase.Finished)
        {
            isShowScore = true;

            InGameUIController uiController = FindAnyObjectByType<InGameUIController>();

            if (uiController != null)
            {
                uiController.ShowScoreUI();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // StateAuthorityを持っているクライアントのみ処理
        if (!Object.HasStateAuthority) return;

        // カウントダウンが終了するまで待機
        if (!CountDownUI.isCountDownFinished) return;

        // ゲームタイマーが未開始なら開始する
        if (!isStartedTimer)
        {
            GameTimerTick = TickTimer.CreateFromSeconds(Runner, totalTime);
            isStartedTimer = true;
        }

        // 制限時間が終了したらゲームを終了する
        if (GameTimerTick.Expired(Runner))
        {
            CurrentPhase = GamePhase.Finished;
            return;
        }

        // 経過時間から現在のフェーズを更新
        float elapsedTime = totalTime - RemainingTime;
        GamePhase newPhase = GetPhaseByElapsedTime(elapsedTime);

        CurrentPhase = newPhase;
    }

    private GamePhase GetPhaseByElapsedTime(float elapsedTime)
    {
        float currentTime = 0.0f;

        foreach (var phaseTime in gamePhaseTimes)
        {
            currentTime += phaseTime.timeInSeconds;

            if (elapsedTime < currentTime)
            {
                return phaseTime.phase;
            }
        }

        return GamePhase.Finished; // 全てのフェーズを過ぎた場合はFinishedを返す
    }

    public override void Render()
    {
        // フェーズが変わったかを比較
        if (CurrentPhase != previousPhase)
        {
            // フェーズが変わったときにOnPhaseChangedイベントを呼び出す
            OnPhaseChanged?.Invoke(CurrentPhase);

            // フェーズがFinishedになったときにOnTimeUpイベントを呼び出す
            if (CurrentPhase == GamePhase.Finished)
            {
                OnTimeUp?.Invoke();
            }

            // フェーズの比較用の変数を更新
            previousPhase = CurrentPhase;
        }
    }
}
