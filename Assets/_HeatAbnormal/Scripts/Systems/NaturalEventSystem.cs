using System;
using UnityEngine;

public enum NaturalEventType
{
    Badai,
    GempaBumi,
    Kiamat
}

public class NaturalEventSystem : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;
    [SerializeField] private GameDataRegistry registry;
    [SerializeField, Range(0f, 100f)] private float eventChancePercent = 30f;

    public event Action<EventAlamData> OnNaturalEventTriggered;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInParent<GameClock>();
        if (registry == null) Debug.LogError("NaturalEventSystem requires a GameDataRegistry reference.");
    }

    public EventAlamData TryTriggerEvent()
    {
        if (!GameServices.Random.Roll(eventChancePercent)) return null;
        var type = DetermineEventType(GameServices.Random.NextFloat(0f, 100f));
        var eventData = FindEvent(type);
        return eventData;
    }

    public static NaturalEventType DetermineEventType(float roll)
    {
        if (roll < 85f) return NaturalEventType.Badai;
        if (roll < 99.9f) return NaturalEventType.GempaBumi;
        return NaturalEventType.Kiamat;
    }

    private EventAlamData FindEvent(NaturalEventType type)
    {
        if (registry == null) return null;
        var id = type == NaturalEventType.Badai ? "Ev. Alam 2" : type == NaturalEventType.GempaBumi ? "Ev. Alam 1" : "Ev. Alam 3";
        foreach (var item in registry.eventAlamList)
        {
            if (item != null && item.id == id) return item;
        }
        Debug.LogError("Natural event data not found: " + id);
        return null;
    }
}
