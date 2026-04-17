using Fusion;
using UnityEngine;
using System;

public class GameTimer : NetworkBehaviour
{
    public event Action<GamePhase> OnPhaseChanged;
    public event Action OnTimeUp;

    [SerializeField] private float totalTime = 360f; // 6 minutes
    [SerializeField] private float phaseLength = 120f; // 2 minutes per phase

    [Networked] private TickTimer GameTimerTick { get; set; }
    [Networked] public GamePhase CurrentPhase { get; private set; }

    private GamePhase previousPhase;

    public float RemainingTime
    {
        get
        {
            if (GameTimerTick.ExpiredOrNotRunning(Runner))
                return 0f;

            return Mathf.Max(0.0f, GameTimerTick.RemainingTime(Runner) ?? 0.0f);
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
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
        int phaseIndex = Mathf.FloorToInt(elapsedTime / phaseLength);

        GamePhase newPhase = phaseIndex switch
        {
            0 => GamePhase.Phase1,
            1 => GamePhase.Phase2,
            2 => GamePhase.Phase3,
            _ => GamePhase.Finished
        };

        CurrentPhase = newPhase;
    }

    public override void Render()
    {
        if (CurrentPhase != previousPhase)
        {
            OnPhaseChanged?.Invoke(CurrentPhase);

            if (CurrentPhase == GamePhase.Finished)
            {
                OnTimeUp?.Invoke();
            }

            previousPhase = CurrentPhase;
        }
    }
}
