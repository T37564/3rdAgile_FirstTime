using Fusion;
using UnityEngine;
using System;

public class GameTimer : NetworkBehaviour
{
    public event Action<GamePhase> OnPhaseChanged;
    public event Action OnTimeUp;

    [SerializeField] private float[] phaseTimes = { 0.0f };

    [SerializeField] private float totalTime = 360f; // 6 minutes
    [SerializeField] private float phaseLength = 120f; // 2 minutes per phase

    [Networked] private TickTimer GameTimerTick { get; set; }
    [Networked] public GamePhase CurrentPhase { get; private set; }

    private GamePhase previousPhase;

    /// <summary>
    /// タイマーの残り時間を秒単位で取得し、
    /// タイマーが終了した場合は０を返すようにしているプロパティ
    /// </summary>
    public float RemainingTime
    {
        get
        {
            // タイマーが終了したとき0を返す
            if (GameTimerTick.ExpiredOrNotRunning(Runner))
                return 0f;

            // タイマーがまだ動いているときは、残り時間を返す
            return Mathf.Max(0.0f, GameTimerTick.RemainingTime(Runner) ?? 0.0f);
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            totalTime = GetTotalPhaseTime();

            GameTimerTick = TickTimer.CreateFromSeconds(Runner, totalTime);
            CurrentPhase = GamePhase.Phase1;
        }

        previousPhase = CurrentPhase;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (GameTimerTick.Expired(Runner))
        {
            CurrentPhase = GamePhase.Finished;
            return;
        }

        float elapsedTime = totalTime - RemainingTime;

        CurrentPhase = GetPhaseByElapsedTime(elapsedTime);

        //int phaseIndex = Mathf.FloorToInt(elapsedTime / phaseLength);
        //Debug.Log(elapsedTime);
        //GamePhase newPhase = phaseIndex switch
        //{
        //    0 => GamePhase.Phase1,
        //    1 => GamePhase.Phase2,
        //    2 => GamePhase.Phase3,
        //    _ => GamePhase.Finished
        //};

        //CurrentPhase = newPhase;
    }

    private float GetTotalPhaseTime()
    {
        float sum = 0.0f;

        foreach(float time in phaseTimes)
        {
            sum += time;
        }

        return sum;
    }

    private GamePhase GetPhaseByElapsedTime(float elapsed)
    {
        float cumulative = 0.0f;

        for(int i = 0; i < phaseTimes.Length; i++)
        {
            cumulative += phaseTimes[i];

            if (elapsed < cumulative)
            {
                return (GamePhase)(i + 1);
            }
        }

        return GamePhase.Finished;
    }

    public override void Render()
    {
        if (CurrentPhase != previousPhase)
        {
            Debug.Log($"Phase changed to {CurrentPhase}");
            OnPhaseChanged?.Invoke(CurrentPhase);

            if (CurrentPhase == GamePhase.Finished)
            {
                OnTimeUp?.Invoke();
            }

            previousPhase = CurrentPhase;
        }
    }
}
