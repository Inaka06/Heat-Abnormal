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

    public EventRakyatData TryTriggerEvent()
    {
        var contractor = GameSession.State.SelectedKontraktor;
        var politicians = GameSession.State.SelectedPolitikus;
        if (contractor == null || politicians == null || politicians.Count != 3) return null;
        var first = politicians[0]; var second = politicians[1]; var third = politicians[2];
        if (first == null || second == null || third == null) return null;
        var chance = CalculateEventRate(contractor.keselamatanKerja, first.kepercayaanPublik, second.kepercayaanPublik, third.kepercayaanPublik);
        if (!GameServices.Random.Roll(chance) || registry == null || registry.eventRakyatList.Count == 0) return null;
        var index = GameServices.Random.NextInt(0, registry.eventRakyatList.Count);
        var eventData = registry.eventRakyatList[index];
        return eventData;
    }

    public static float CalculateEventRate(float safety, float publicTrust1, float publicTrust2, float publicTrust3)
    {
        var averageTrust = GameFormulas.AvgKepercayaanPublik(publicTrust1, publicTrust2, publicTrust3);
        return GameFormulas.TingkatEventRakyat(safety, averageTrust);
    }
}
