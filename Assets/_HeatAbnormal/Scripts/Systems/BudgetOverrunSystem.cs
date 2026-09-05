using System;
using UnityEngine;

public class BudgetOverrunSystem : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;

    public event Action<float> OnBudgetOverrun;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInParent<GameClock>();
    }

    private void OnEnable()
    {
        if (gameClock != null) gameClock.OnPeriodElapsed += HandlePeriodElapsed;
    }

    private void OnDisable()
    {
        if (gameClock != null) gameClock.OnPeriodElapsed -= HandlePeriodElapsed;
    }

    private void HandlePeriodElapsed()
    {
        var contractor = GameSession.State.SelectedKontraktor as KontraktorData;
        var plant = GameSession.State.SelectedPembangkit as PembangkitData;
        if (contractor == null || plant == null || GameSession.State.SelectedPolitikus.Count != 3)
        {
            Debug.LogError("BudgetOverrunSystem requires selected plant, contractor, and exactly 3 politicians.");
            return;
        }

        var p1 = GameSession.State.SelectedPolitikus[0] as PolitikusData;
        var p2 = GameSession.State.SelectedPolitikus[1] as PolitikusData;
        var p3 = GameSession.State.SelectedPolitikus[2] as PolitikusData;
        if (p1 == null || p2 == null || p3 == null)
        {
            Debug.LogError("BudgetOverrunSystem found invalid politician selection.");
            return;
        }

        var rp = GameFormulas.AvgPembengkakanDanaPolitikus(p1.pembengkakanDana, p2.pembengkakanDana, p3.pembengkakanDana);
        var rd = GameFormulas.RisikoPembengkakanDanaTotal(contractor.pembengkakanDana, rp);
        var cd = GameFormulas.PeluangPembengkakanDana(rd);
        if (!GameServices.Random.Roll(cd)) return;

        var rate = GameServices.Random.NextFloat(0.03f, 0.07f);
        var overrun = GameFormulas.BesarPembengkakanDana(plant.biayaDipilih, rate);
        var deduction = Mathf.Max(0, Mathf.RoundToInt(overrun));
        GameSession.State.Dana -= deduction;
        GameStateEvents.RaiseDanaChanged(GameSession.State.Dana);
        OnBudgetOverrun?.Invoke(deduction);
    }
}
