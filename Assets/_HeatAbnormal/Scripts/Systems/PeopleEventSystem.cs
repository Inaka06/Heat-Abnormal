using System;
using UnityEngine;

public class PeopleEventSystem : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;
    [SerializeField] private GameDataRegistry registry;

    public event Action<EventRakyatData> OnPeopleEventTriggered;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInParent<GameClock>();
        if (registry == null) Debug.LogError("PeopleEventSystem requires a GameDataRegistry reference.");
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
        var politicians = GameSession.State.SelectedPolitikus;
        if (contractor == null || politicians == null || politicians.Count != 3) return;
        var first = politicians[0] as PolitikusData; var second = politicians[1] as PolitikusData; var third = politicians[2] as PolitikusData;
        if (first == null || second == null || third == null) return;
        var chance = CalculateEventRate(contractor.keselamatanKerja, first.kepercayaanPublik, second.kepercayaanPublik, third.kepercayaanPublik);
        if (!GameServices.Random.Roll(chance) || registry == null || registry.eventRakyatList.Count == 0) return;
        var index = GameServices.Random.NextInt(0, registry.eventRakyatList.Count);
        var eventData = registry.eventRakyatList[index];
        if (eventData != null) OnPeopleEventTriggered?.Invoke(eventData);
    }

    public static float CalculateEventRate(float safety, float publicTrust1, float publicTrust2, float publicTrust3)
    {
        var averageTrust = GameFormulas.AvgKepercayaanPublik(publicTrust1, publicTrust2, publicTrust3);
        return GameFormulas.TingkatEventRakyat(safety, averageTrust);
    }
}
