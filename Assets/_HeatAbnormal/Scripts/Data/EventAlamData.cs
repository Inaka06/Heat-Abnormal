using UnityEngine;

[CreateAssetMenu(fileName = "EventAlamData", menuName = "Heat Abnormal/Event Alam Data")]
public class EventAlamData : ScriptableObject
{
    public string id;
    public string namaEvent;
    [TextArea]
    public string narasi;
    public float baseDamageMin;
    public float baseDamageMax;
    public bool isInstantGameOver;
}
