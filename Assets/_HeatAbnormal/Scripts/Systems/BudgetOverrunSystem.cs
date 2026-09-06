using System;
using UnityEngine;

public class BudgetOverrunSystem : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInParent<GameClock>();
    }

    public float? TryTriggerOverrun()
    {
        var contractor = GameSession.State.SelectedKontraktor;
        var plant = GameSession.State.SelectedPembangkit;
        if (contractor == null || plant == null || GameSession.State.SelectedPolitikus.Count != 3)
        {
            Debug.LogError("BudgetOverrunSystem requires selected plant, contractor, and exactly 3 politicians.");
            return null;
        }

        var p1 = GameSession.State.SelectedPolitikus[0];
        var p2 = GameSession.State.SelectedPolitikus[1];
        var p3 = GameSession.State.SelectedPolitikus[2];
        if (p1 == null || p2 == null || p3 == null)
        {
            Debug.LogError("BudgetOverrunSystem found invalid politician selection.");
            return null;
        }

        var rp = GameFormulas.AvgPembengkakanDanaPolitikus(p1.pembengkakanDana, p2.pembengkakanDana, p3.pembengkakanDana);
        var rd = GameFormulas.RisikoPembengkakanDanaTotal(contractor.pembengkakanDana, rp);
        var cd = GameFormulas.PeluangPembengkakanDana(rd);
        if (!GameServices.Random.Roll(cd)) return null;

        var rate = GameServices.Random.NextFloat(0.03f, 0.07f);
        var overrun = GameFormulas.BesarPembengkakanDana(plant.biayaDipilih, rate);
        var deduction = Mathf.Max(0, Mathf.RoundToInt(overrun));
        GameSession.State.Dana -= deduction;
        GameStateEvents.RaiseDanaChanged(GameSession.State.Dana);
        return deduction;
    }
}
