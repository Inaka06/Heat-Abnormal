using System;
using UnityEngine;

public sealed class GameClock : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float periodDurationSeconds = 30f;

    private float elapsedSeconds;
    private bool isPaused;
    private float speedMultiplier = 1f;
    private bool stopAdvanceRequested;

    public event Action OnPeriodElapsed;

    public float PeriodDurationSeconds
    {
        get => periodDurationSeconds;
        set => periodDurationSeconds = Mathf.Max(0.01f, value);
    }

    public int CurrentPeriodCount { get; private set; }
    public bool IsPaused => isPaused;
    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = Mathf.Clamp(value, 1f, 9f); }
    public float SecondsUntilNextPeriod => Mathf.Max(0f, periodDurationSeconds - elapsedSeconds);

    private void Update()
    {
        if (isPaused)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime * speedMultiplier;
        // Resolve at most one period per frame. Any excess accumulated time is
        // intentionally discarded so event resolution can finish before the
        // next period is emitted.
        if (elapsedSeconds >= periodDurationSeconds)
        {
            elapsedSeconds = 0f;
            CurrentPeriodCount++;
            OnPeriodElapsed?.Invoke();
        }
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void ResetClock()
    {
        elapsedSeconds = 0f;
        CurrentPeriodCount = 0;
        isPaused = false;
    }

    public void AdvancePeriodNow()
    {
        AdvancePeriodsNow(1);
    }

    public void AdvancePeriodsNow(int amount)
    {
        if (isPaused || amount <= 0) return;
        stopAdvanceRequested = false;
        for (var i = 0; i < amount; i++)
        {
            elapsedSeconds = 0f;
            CurrentPeriodCount++;
            OnPeriodElapsed?.Invoke();
            if (stopAdvanceRequested) break;
        }
    }

    public void StopCurrentAdvance()
    {
        stopAdvanceRequested = true;
    }
}
