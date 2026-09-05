using UnityEngine;

[CreateAssetMenu(fileName = "KontraktorData", menuName = "Heat Abnormal/Kontraktor Data")]
public class KontraktorData : ScriptableObject
{
    public enum KontraktorId
    {
        PokhrovUmaPyoiConstruction,
        DashiGroup,
        GunungSaljuPembangunan,
        NeoVantaIX
    }

    public KontraktorId id;
    public string displayName;
    [Range(0, 100)]
    public int kekokohan;
    [Range(0, 100)]
    public int pembengkakanDana;
    [Range(0, 100)]
    public int keselamatanKerja;
    [TextArea]
    public string deskripsi;
}
