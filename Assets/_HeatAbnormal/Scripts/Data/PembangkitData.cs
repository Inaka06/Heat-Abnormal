using UnityEngine;

[CreateAssetMenu(fileName = "PembangkitData", menuName = "Heat Abnormal/Pembangkit Data")]
public class PembangkitData : ScriptableObject
{
    public enum PembangkitId
    {
        PLTA,
        PLTB,
        PLTS,
        PLTN
    }

    public PembangkitId id;
    public string displayName;
    public int biayaDipilih;
    public int biayaSisa;
    public int baseLamaPeriode;
    public Sprite icon;
}
