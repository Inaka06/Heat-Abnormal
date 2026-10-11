using UnityEngine;

[CreateAssetMenu(fileName = "EventAlamData", menuName = "Heat Abnormal/Event Alam Data")]
public class EventAlamData : ScriptableObject
{
    public string id;
    public string namaEvent;
    [TextArea]
    public string narasi;
    [Tooltip("Relative progress damage in percent, before contractor resistance.")]
    public float progressDamageMin;
    public float progressDamageMax;
    [Tooltip("Additional construction periods caused by this event.")]
    public int additionalPeriodsMin;
    public int additionalPeriodsMax;
    public bool isInstantGameOver;
}
