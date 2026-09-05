using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDataRegistry", menuName = "Heat Abnormal/Game Data Registry")]
public class GameDataRegistry : ScriptableObject
{
    public List<PembangkitData> pembangkitList = new List<PembangkitData>();
    public List<KontraktorData> kontraktorList = new List<KontraktorData>();
    public List<PolitikusData> politikusList = new List<PolitikusData>();
    public List<EventRakyatData> eventRakyatList = new List<EventRakyatData>();
    public List<EventAlamData> eventAlamList = new List<EventAlamData>();

    private static GameDataRegistry instance;

    public static GameDataRegistry Instance => instance;

    public void InitializeAsInstance()
    {
        instance = this;
    }
}
