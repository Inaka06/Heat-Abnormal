using UnityEngine;

[CreateAssetMenu(fileName = "PolitikusData", menuName = "Heat Abnormal/Politikus Data")]
public class PolitikusData : ScriptableObject
{
    public enum PolitikusId
    {
        YohanSpica,
        AgutamaMachan,
        CahayaLightO,
        KrisPutriJayahadikusuma,
        DaiwaSoebaruningrat,
        IksanMamo
    }

    public PolitikusId id;
    public string displayName;
    [Range(0, 100)]
    public int lobby;
    [Range(0, 100)]
    public int pembengkakanDana;
    [Range(0, 100)]
    public int kepercayaanPublik;
    [Range(0, 100)]
    public int penyambungan;
    [TextArea]
    public string deskripsi;
}
